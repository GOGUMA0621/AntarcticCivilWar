using UnityEngine;

public enum GridDistanceMetric
{
    Chebyshev, // max(|dx|,|dy|)
    Manhattan,
    Euclidean
}

/// <summary>
/// 씬 의존성 없이 사용 가능한 static 그리드 유틸.
/// AstarPathfinder 또는 PlacementGridManager를 호출 쪽에서 전달하여 사용.
/// </summary>
public static class GridRangeUtility
{
    // AstarPathfinder 오브젝트를 넘겨 계산
    public static Vector2Int WorldToGrid(AstarPathfinder astar, Vector3 world)
    {
        if (astar == null) throw new System.ArgumentNullException(nameof(astar));
        return astar.WorldToGrid(world);
    }

    // PlacementGridManager 오브젝트를 넘겨 계산
    public static Vector2Int WorldToGrid(PlacementGridManager placement, Vector3 world)
    {
        if (placement == null) throw new System.ArgumentNullException(nameof(placement));
        Vector3 local = placement.transform.InverseTransformPoint(world) - placement.origin;
        int x = Mathf.FloorToInt(local.x / placement.cellSize);
        int y = Mathf.FloorToInt(local.y / placement.cellSize);
        return new Vector2Int(x, y);
    }

    // Grid -> World (셀 중심)
    public static Vector3 GridToWorld(AstarPathfinder astar, Vector2Int gridPos)
    {
        if (astar == null) throw new System.ArgumentNullException(nameof(astar));
        return astar.GridToWorld(gridPos);
    }

    public static Vector3 GridToWorld(PlacementGridManager placement, Vector2Int gridPos)
    {
        if (placement == null) throw new System.ArgumentNullException(nameof(placement));
        Vector3 local = placement.origin + new Vector3(gridPos.x * placement.cellSize, gridPos.y * placement.cellSize, 0f);
        return placement.transform.TransformPoint(local);
    }

    // 그리드 좌표끼리 거리 계산 (셀 단위)
    public static int GridDistance(Vector2Int a, Vector2Int b, GridDistanceMetric metric = GridDistanceMetric.Chebyshev)
    {
        int dx = Mathf.Abs(a.x - b.x);
        int dy = Mathf.Abs(a.y - b.y);
        switch (metric)
        {
            case GridDistanceMetric.Manhattan: return dx + dy;
            case GridDistanceMetric.Euclidean: return Mathf.CeilToInt(Mathf.Sqrt(dx * dx + dy * dy));
            default: return Mathf.Max(dx, dy);
        }
    }

    // 편의 오버로드: world 위치를 전달하고 어떤 그리드를 사용하는지 명시
    public static int GridDistance(AstarPathfinder astar, Vector3 worldA, Vector3 worldB, GridDistanceMetric metric = GridDistanceMetric.Chebyshev)
    {
        var A = WorldToGrid(astar, worldA);
        var B = WorldToGrid(astar, worldB);
        return GridDistance(A, B, metric);
    }

    public static int GridDistance(PlacementGridManager placement, Vector3 worldA, Vector3 worldB, GridDistanceMetric metric = GridDistanceMetric.Chebyshev)
    {
        var A = WorldToGrid(placement, worldA);
        var B = WorldToGrid(placement, worldB);
        return GridDistance(A, B, metric);
    }
}