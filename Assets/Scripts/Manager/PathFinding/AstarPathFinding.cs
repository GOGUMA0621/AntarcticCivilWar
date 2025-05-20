using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// A* 알고리즘을 사용하여 경로를 찾는 클래스입니다.
/// </summary>
public class AstarPathFinding
{
    private IGridScanner gridScanner;

    public AstarPathFinding(IGridScanner gridScanner)
    {
        this.gridScanner = gridScanner;
    }

    private Dictionary<(Vector2Int, Vector2Int), List<Vector2Int>> pathCache = new();
    private Dictionary<Vector2Int, float> lastRequestTime = new();
    private Queue<PathRequest> requestQueue = new();
    private bool isProcessingRequest = false;
    private float pathCooldown = 0.2f;

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
    public void RequestPath(Vector2Int start, Vector2Int end, Action<List<Vector2Int>> callback)
    {
        requestQueue.Enqueue(new PathRequest(start, end, callback));
        if (!isProcessingRequest)
            _ = ProcessPathRequests();
    }
    /// <summary>
    /// 요청 큐에 있는 경로를 처리합니다.
    /// 요청 큐에 있는 경로는 캐싱됩니다.
    /// 캐싱된 경로는 요청 큐에서 제거되지 않습니다.
    /// 캐싱된 경로는 요청 큐에 있는 경로와 동일한 시작점과 끝점을 가진 경로입니다.
    /// </summary>
    private async Task ProcessPathRequests()
    {
        isProcessingRequest = true;

        while (requestQueue.Count > 0)
        {
            var request = requestQueue.Dequeue();
            List<Vector2Int> result = FindPath(request.start, request.end);
            request.callback?.Invoke(result);
            await Task.Yield(); 
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
    public List<Vector2Int> FindPath(Vector2Int startPos, Vector2Int endPos)
    {
        float now = Time.time;
        if (lastRequestTime.TryGetValue(startPos, out float last) && now - last < pathCooldown)
            return null;
        lastRequestTime[startPos] = now;

        var key = (startPos, endPos);
        if (pathCache.TryGetValue(key, out var cached)) return cached;

        if (!gridScanner.HasNode(startPos) ||
            !gridScanner.HasNode(endPos) ||
            !gridScanner.GetNode(endPos).isWalkable)
            return null;

        Node startNode = gridScanner.GetNode(startPos);
        Node endNode = gridScanner.GetNode(endPos);

        List<Node> openList = new() { startNode };
        HashSet<Node> closedList = new();

        startNode.gCost = 0;
        startNode.hCost = GetHeuristic(startPos, endPos);
        startNode.parentNode = null;

        while (openList.Count > 0)
        {
            Node current = GetLowestFCostNode(openList);
            if (current == endNode)
            {
                var path = RetracePath(startNode, endNode);
                pathCache[key] = path;
                return path;
            }

            openList.Remove(current);
            closedList.Add(current);

            foreach (Node neighbor in gridScanner.GetNeighbors(current))
            {
                if (!neighbor.isWalkable || closedList.Contains(neighbor)) continue;

                int newG = current.gCost + GetDistance(current.gridPosition, neighbor.gridPosition);
                if (newG < neighbor.gCost || !openList.Contains(neighbor))
                {
                    neighbor.gCost = newG;
                    neighbor.hCost = GetHeuristic(neighbor.gridPosition, endPos);
                    neighbor.parentNode = current;

                    if (!openList.Contains(neighbor)) openList.Add(neighbor);
                }
            }
        }

        return null;
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
    /// 주변 노드를 반환합니다.
    /// 대각선 이동을 지원합니다.
    /// 대각선 이동을 지원하기 위해, 대각선 방향으로 이동할 수 있는 노드가 있는지 체크합니다.
    /// </summary>
    /// <param name="node">주변 노드를 반환하기 위한 시작 노드 입니다.</param>
    /// <returns>들어온 노드 주변 노드들을 반환합니다.</returns>
    private List<Node> GetNeighbors(Node node)
    {
        List<Node> neighbors = new();
        Vector2Int[] dirs = {
            Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right,
            new(1,1), new(-1,1), new(1,-1), new(-1,-1)
        };

        foreach (var dir in dirs)
        {
            Vector2Int check = node.gridPosition + dir;
            if (!gridScanner.HasNode(check))
                continue;

            // 대각선 이동 체크
            if (Mathf.Abs(dir.x) == 1 && Mathf.Abs(dir.y) == 1)
            {
                Vector2Int check1 = node.gridPosition + new Vector2Int(dir.x, 0);
                Vector2Int check2 = node.gridPosition + new Vector2Int(0, dir.y);

                // 둘 중 하나라도 막혀 있으면 대각선 이동 불가
                if (!gridScanner.HasNode(check1) || !gridScanner.HasNode(check2))
                    continue;
                if (!gridScanner.GetNode(check1).isWalkable || !gridScanner.GetNode(check2).isWalkable)
                    continue;
            }

            neighbors.Add(gridScanner.GetNode(check));
        }
        return neighbors;
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
    /// 요청 큐에 추가된 경로는 캐싱됩니다.
    /// </summary>
    public void ClearPathCache()
    {
        pathCache.Clear();
    }
}
