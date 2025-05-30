using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

[RequireComponent(typeof(IGridScanner))]
/// <summary>
/// A* 알고리즘을 사용하여 경로를 찾는 클래스입니다.
/// </summary>
public class AstarPathFinding : SingleTonBehaviour<AstarPathFinding>
{
    private IGridScanner gridScanner;

    private const int MaxPathCacheCount = 500; // 원하는 최대 캐시 개수
    private readonly Queue<Action> mainThreadActions = new Queue<Action>();
    private Dictionary<(Vector2Int, Vector2Int), List<Vector2Int>> pathCache = new();
    private Dictionary<Vector2Int, float> lastRequestTime = new();
    private Queue<PathRequest> requestQueue = new();
    private bool isProcessingRequest = false;
    private float pathCooldown = 0.2f;
    private LinkedList<(Vector2Int, Vector2Int)> cacheOrder = new(); // 캐시 순서 관리

    protected override void Awake()
    {
        base.Awake();
       gridScanner = GetComponent<IGridScanner>();
        if (gridScanner == null)
        {
            Debug.LogError("AstarPathFinding requires a GridScanner to function. Please assign one in the inspector or ensure it is present in the scene.");
        }
    }

    private void Update()
    {
        // 메인 스레드에서 실행할 액션이 있다면 실행
        lock (mainThreadActions)
        {
            while (mainThreadActions.Count > 0)
            {
                mainThreadActions.Dequeue()?.Invoke();
            }
        }
    }

    /// <summary>
    /// A* 경로 요청 큐에 추가합니다.
    /// 요청이 들어오면 자동으로 경로 계산을 시작합니다.
    /// 요청이 들어온 순서대로 처리됩니다.
    /// 요청이 들어온 후, 경로 계산이 완료되면 callback이 호출됩니다.
    /// 경로 계산이 완료되면, 요청 큐에서 제거됩니다.
    /// 요청 큐에 있는 경로는 캐싱됩니다.
    /// 캐싱된 경로는 요청 큐에서 제거되지 않습니다.
    /// 캐싱된 경로는 요청 큐에 있는 경로와 동일한 시작점과 끝점을 가진 경로입니다.
    /// </summary>
    /// <param name="start">시작 점 입니다.</param>
    /// <param name="end">끝 점 입니다.</param>
    /// <param name="callback">경로 계산 이후 호출될 callback 입니다.</param>
    public void RequestPath(Vector2Int start, Vector2Int end, GameObject self, Action<List<Vector2Int>> callback)
    {
        float now = Time.time;
        if (lastRequestTime.TryGetValue(start, out float last) && now - last < pathCooldown)
        {
            callback?.Invoke(null);
            return;
        }
        lastRequestTime[start] = now;

        requestQueue.Enqueue(new PathRequest(start, end, callback));
        if (!isProcessingRequest)
        {
            ProcessPathRequests(self);
        }
    }
    /// <summary>
    /// 요청 큐에 있는 경로를 처리합니다.
    /// 요청 큐에 있는 경로는 캐싱됩니다.
    /// 캐싱된 경로는 요청 큐에서 제거되지 않습니다.
    /// 캐싱된 경로는 요청 큐에 있는 경로와 동일한 시작점과 끝점을 가진 경로입니다.
    /// </summary>
    private async void ProcessPathRequests(GameObject self)
    {
        isProcessingRequest = true;
        // Debug.Log("Processing path requests...");
        while (requestQueue.Count > 0)
        {
            var request = requestQueue.Dequeue();
            var result = await Task.Run(() => FindPath(request.start, request.end, self));

            lock (mainThreadActions)
            {
                mainThreadActions.Enqueue(() => request.callback?.Invoke(result));
            }
        }
        isProcessingRequest = false;
    }

    /// <summary>
    /// A* 경로를 찾습니다.
    /// </summary>
    /// <param name="startPos">시작 점 입니다.</param>
    /// <param name="endPos">끝 점 입니다.</param>
    /// <returns>경로를 반환합니다. 경로가 없으면 null을 반환합니다.</returns>
    /// <remarks>
    /// 경로를 찾는 데 시간이 걸릴 수 있습니다. 이 메서드는 코루틴으로 실행됩니다.
    /// 경로를 찾는 데 시간이 걸리는 경우, null을 반환합니다.
    /// 경로를 찾는 데 시간이 걸리는 경우, 코루틴을 사용하여 경로를 찾는 것이 좋습니다.
    /// </remarks>
    public List<Vector2Int> FindPath(Vector2Int startPos, Vector2Int endPos, GameObject self) 
    {
        var key = (startPos, endPos);
        if (pathCache.TryGetValue(key, out var cached))
        {
            cacheOrder.Remove(key);
            cacheOrder.AddLast(key);
            return cached;
        }

        if (!gridScanner.HasNode(startPos) ||
            !gridScanner.HasNode(endPos) ||
            !gridScanner.GetNode(endPos).isWalkable)
            return null;

        Node startNode = gridScanner.GetNode(startPos);
        Node endNode = gridScanner.GetNode(endPos);

        MinHeap<Node> openHeap = new();
        HashSet<Node> closedList = new();

        startNode.gCost = 0;
        startNode.hCost = GetHeuristic(startPos, endPos);
        startNode.parentNode = null;
        openHeap.Add(startNode);

        while (openHeap.Count > 0)
        {
            Node current = openHeap.RemoveMin();
            if (current == endNode)
            {
                var path = RetracePath(startNode, endNode);
                pathCache[key] = path;
                cacheOrder.AddLast(key);

                if (cacheOrder.Count > MaxPathCacheCount)
                {
                    var oldest = cacheOrder.First.Value;
                    cacheOrder.RemoveFirst();
                    pathCache.Remove(oldest);
                }
                return path;
            }

            closedList.Add(current);

            foreach (Node neighbor in gridScanner.GetNeighbors(current))
            {
                if (!neighbor.isWalkable || closedList.Contains(neighbor))
                    continue;

                int extraCost = 0;
                // 유닛이 있는 칸이면 soft obstacle 비용 추가 (목표 위치는 예외)
                if (IsUnitSoftObstacle(neighbor.gridPosition, self, endNode.gridPosition))
                    extraCost = 10; // 값은 상황에 맞게 조정

                int newG = current.gCost + GetDistance(current.gridPosition, neighbor.gridPosition) + extraCost;

                if (newG < neighbor.gCost || !openHeap.Contains(neighbor))
                {
                    neighbor.gCost = newG;
                    neighbor.hCost = GetHeuristic(neighbor.gridPosition, endPos);
                    neighbor.parentNode = current;

                    if (!openHeap.Contains(neighbor)) openHeap.Add(neighbor);
                }
            }
        }

        return null;
    }

    private bool IsUnitOnPosition(Vector3 worldPosition, GameObject self, Vector3 goal)
    {
        if ((worldPosition - goal).sqrMagnitude < 0.01f)
            return false; // 목표 위치는 예외 처리
        float radius = 0.2f;
        Collider2D[] colliders = Physics2D.OverlapCircleAll(worldPosition, radius);
        foreach (var collider in colliders)
        {
            if (collider.gameObject == self)
                continue; // 자기 자신은 제외

            if (collider.gameObject != self && collider.gameObject.layer == LayerMask.NameToLayer("Unit"))
            {
                return true; // 다른 유닛이 해당 위치에 있음
            }
        }   
        return false; // 해당 위치에 다른 유닛이 없음        
    }


    private bool IsUnitSoftObstacle(Vector2Int gridPos, GameObject self, Vector2Int goalGrid)
    {
        if (gridPos == goalGrid) return false; // 목표 위치는 예외

        // 모든 유닛의 위치를 순회 (자기 자신 제외)
        if(AstarMover.AllUnitGridPositions.Contains(gridPos))
        {
           return true; // 해당 그리드 위치에 유닛이 있음
        }
        return false;
    }

    /// <summary>
    /// 가장 낮은 fCost를 가진 노드를 반환합니다.
    /// fCost는 gCost + hCost입니다.
    /// </summary>
    /// <param name="nodes">계산될 노드 리스트 입니다.</param>
    /// <returns>가장 낮은 코스트를 가진 노드를 반환합니다.</returns>
    private Node GetLowestFCostNode(List<Node> nodes)
    {
        Node best = nodes[0];
        foreach (var n in nodes)
            if (n.fCost < best.fCost)
                best = n;
        return best;
    }
    /// <summary>
    /// 휴리스틱 함수를 사용하여 두 점 사이의 거리를 계산합니다.
    /// A* 알고리즘에서 사용됩니다.
    /// </summary>
    /// <param name="a">함수에 사용될 첫 번째 점입니다.</param>
    /// <param name="b">함수에 사용될 두 번째 점입니다.</param>
    /// <returns>두 점 사이의 거리를 계산합니다.</returns>
    private int GetHeuristic(Vector2Int a, Vector2Int b)
        => (Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y)) * 10;
    /// <summary>
    /// 두 점 사이의 거리를 계산합니다.
    /// A* 알고리즘에서 사용됩니다.
    /// </summary>
    /// <param name="a">함수에 사용될 첫 번째 점입니다.</param>
    /// <param name="b">함수에 사용될 두 번째 점입니다.</param>
    /// <returns>두 점 사이의 거리를 계산합니다.</returns>
    /// <remarks>
    private int GetDistance(Vector2Int a, Vector2Int b)
    {
        int dx = Mathf.Abs(a.x - b.x);
        int dy = Mathf.Abs(a.y - b.y);
        return dx > dy ? 14 * dy + 10 * (dx - dy) : 14 * dx + 10 * (dy - dx);
    }
    /// <summary>
    /// 주어진 시작 노드에서 끝 노드까지의 경로를 재추적합니다.
    /// A* 알고리즘에서 사용됩니다.
    /// </summary>
    /// <param name="start">시작 노드 입니다.</param>
    /// <param name="end">끝 노드 입니다.</param>
    /// <returns>시작 노드 와 끝 노드 까지의 경로를 반환합니다.</returns>
    private List<Vector2Int> RetracePath(Node start, Node end)
    {
        List<Vector2Int> path = new();
        Node current = end;

        while (current != start)
        {
            path.Add(current.gridPosition);
            current = current.parentNode;
        }

        path.Reverse();
        return path;
    }


    /// <summary>
    /// A* 알고리즘에서 사용되는 요청 클래스입니다.
    /// 요청 큐에 추가된 경로를 처리하기 위한 클래스입니다.
    /// 요청 큐에 추가된 경로는 캐싱됩니다.
    /// 캐싱된 경로는 요청 큐에서 제거되지 않습니다.
    /// 캐싱된 경로는 요청 큐에 있는 경로와 동일한 시작점과 끝점을 가진 경로입니다.
    /// </summary>
    private class PathRequest
    {
        public Vector2Int start, end;
        public Action<List<Vector2Int>> callback;
        public PathRequest(Vector2Int s, Vector2Int e, Action<List<Vector2Int>> cb)
        {
            start = s; end = e; callback = cb;
        }
    }
    /// <summary>
    /// A* 알고리즘에서 사용되는 경로 캐시를 초기화합니다.
    /// 
    /// 요청 큐에 추가된 경로는 캐싱됩니다.
    /// </summary>
    public void ClearPathCache()
    {
        pathCache.Clear();
    }
}
