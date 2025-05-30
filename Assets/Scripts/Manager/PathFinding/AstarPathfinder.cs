using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AstarPathfinder : MonoBehaviour, IGridScanner
{

    [Header("Grid Settings")]
    public int width = 0;
    public int height = 0;
    public float nodeSize = 1f;
    public Vector3 center = new Vector3(0, 1.0f, 0);

    [Header("Obstacle Settings")]
    public LayerMask obstacleLayer;
    public float collisionDiameter = 1.3f;
    public bool use2DPhysics = true;

    [Header("Connection Settings")]
    public bool useEightDirections = true;

    internal Node[,] grid;

    [HideInInspector]
    public Vector2 boundsCenter;
    [HideInInspector]
    public Vector2 boundsSize;

    private void OnEnable()
    {
        Scan();
    }

    public void Scan()
    {
        grid = new Node[width, height];
        Vector3 bottomLeft = center - new Vector3(width * nodeSize, height * nodeSize, 0) * 0.5f;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // XY 평면 기준으로 worldPosition 생성
                Vector3 worldPoint = bottomLeft + new Vector3((x + 0.5f) * nodeSize, (y + 0.5f) * nodeSize, 0);
                bool walkable = !CheckCollision(worldPoint);
                grid[x, y] = new Node(new Vector2Int(x, y), worldPoint, walkable);
                if (!walkable)
                    Debug.Log($"Node ({x}, {y}) - Walkable: {walkable}"); // 디버그용 로그
            }
        }
    }

    private bool CheckCollision(Vector3 worldPoint)
    {
        if (use2DPhysics)
            return Physics2D.OverlapCircle(worldPoint, collisionDiameter * 0.5f, obstacleLayer);
        else
            return Physics.CheckSphere(worldPoint, collisionDiameter * 0.5f, obstacleLayer);
    }

    public bool HasNode(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < width && pos.y >= 0 && pos.y < height;
    }

    public Node GetNode(Vector2Int pos)
    {
        if (grid == null) return null;
        if (!HasNode(pos)) return null;
        return grid[pos.x, pos.y];
    }

    public IEnumerable<Node> GetAllNodes()
    {
        foreach (var node in grid)
            yield return node;
    }

    public List<Node> GetNeighbors(Node node)
    {
        List<Node> neighbors = new();
        Vector2Int[] dirs = useEightDirections
            ? new Vector2Int[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right, new(1, 1), new(-1, 1), new(1, -1), new(-1, -1) }
            : new Vector2Int[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        foreach (var dir in dirs)
        {
            Vector2Int check = node.gridPosition + dir;
            if (!HasNode(check))
                continue;

            // 대각선 이동 체크
            if (Mathf.Abs(dir.x) == 1 && Mathf.Abs(dir.y) == 1)
            {
                Vector2Int check1 = node.gridPosition + new Vector2Int(dir.x, 0);
                Vector2Int check2 = node.gridPosition + new Vector2Int(0, dir.y);

                if (!HasNode(check1) || !HasNode(check2))
                    continue;
                if (!GetNode(check1).isWalkable || !GetNode(check2).isWalkable)
                    continue;
            }

            neighbors.Add(GetNode(check));
        }
        return neighbors;
    }

    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        Vector3 bottomLeft = center - new Vector3(width * nodeSize, height * nodeSize, 0) * 0.5f;
        Vector3 offset = worldPos - bottomLeft;
        int x = Mathf.Clamp(Mathf.FloorToInt(offset.x / nodeSize), 0, width - 1);
        int y = Mathf.Clamp(Mathf.FloorToInt(offset.y / nodeSize), 0, height - 1);
        return new Vector2Int(x, y);
    }

    public Vector3 GridToWorld(Vector2Int gridPos)
    {
        Vector3 bottomLeft = center - new Vector3(width * nodeSize, height * nodeSize, 0) * 0.5f;
        return bottomLeft + new Vector3((gridPos.x + 0.5f) * nodeSize, (gridPos.y + 0.5f) * nodeSize, 0);
    }
}
