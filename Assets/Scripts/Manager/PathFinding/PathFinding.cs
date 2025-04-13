using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFinding : SingleTonBehaviour<PathFinding>
{

    private Dictionary<(Vector2Int, Vector2Int), List<Vector2Int>> pathCache = new();
    private Dictionary<Vector2Int, float> lastRequestTime = new();
    private Queue<PathRequest> requestQueue = new();
    private bool isProcessingRequest = false;
    private float pathCooldown = 0.2f;

    // 경로 요청: 큐에 추가 + 콜백 등록
    public void RequestPath(Vector2Int start, Vector2Int end, Action<List<Vector2Int>> callback)
    {
        requestQueue.Enqueue(new PathRequest(start, end, callback));
        if (!isProcessingRequest)
            StartCoroutine(ProcessPathRequests());
    }

    private IEnumerator ProcessPathRequests()
    {
        isProcessingRequest = true;

        while (requestQueue.Count > 0)
        {
            var request = requestQueue.Dequeue();
            List<Vector2Int> result = FindPath(request.start, request.end);
            request.callback?.Invoke(result);
            yield return null;
        }

        isProcessingRequest = false;
    }

    // 실제 A* 경로 계산
    public List<Vector2Int> FindPath(Vector2Int startPos, Vector2Int endPos)
    {
        float now = Time.time;
        if (lastRequestTime.TryGetValue(startPos, out float last) && now - last < pathCooldown)
            return null;
        lastRequestTime[startPos] = now;

        var key = (startPos, endPos);
        if (pathCache.TryGetValue(key, out var cached)) return cached;

        if (!GridManager.instance.HasNode(startPos) ||
            !GridManager.instance.HasNode(endPos) ||
            !GridManager.instance.GetNode(endPos).isWalkable)
            return null;

        Node startNode = GridManager.instance.GetNode(startPos);
        Node endNode = GridManager.instance.GetNode(endPos);

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

            foreach (Node neighbor in GetNeighbors(current))
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

    private Node GetLowestFCostNode(List<Node> nodes)
    {
        Node best = nodes[0];
        foreach (var n in nodes)
            if (n.fCost < best.fCost || (n.fCost == best.fCost && n.fCost < best.fCost))
                best = n;
        return best;
    }

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
            if (GridManager.instance.HasNode(check))
                neighbors.Add(GridManager.instance.GetNode(check));
        }
        return neighbors;
    }

    private int GetHeuristic(Vector2Int a, Vector2Int b)
        => (Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y)) * 10;

    private int GetDistance(Vector2Int a, Vector2Int b)
    {
        int dx = Mathf.Abs(a.x - b.x);
        int dy = Mathf.Abs(a.y - b.y);
        return dx > dy ? 14 * dy + 10 * (dx - dy) : 14 * dx + 10 * (dy - dx);
    }

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

    private class PathRequest
    {
        public Vector2Int start, end;
        public Action<List<Vector2Int>> callback;
        public PathRequest(Vector2Int s, Vector2Int e, Action<List<Vector2Int>> cb)
        {
            start = s; end = e; callback = cb;
        }
    }
}
