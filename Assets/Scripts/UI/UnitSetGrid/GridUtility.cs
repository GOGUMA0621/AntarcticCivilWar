using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GridUtility
{
    /// <summary>
    /// 월드 좌표를 그리드 셀 좌표(Vector2Int)로 변환합니다.
    /// </summary>
    /// <param name="worldPos">변환할 월드 좌표</param>
    /// <returns>해당 위치의 그리드 좌표</returns>
    public static Vector2Int WorldToGrid(Vector3 worldPos, Vector3 origin, float cellSize)
    {
        int x = Mathf.FloorToInt((worldPos.x - origin.x) / cellSize);
        int y = Mathf.FloorToInt((worldPos.y - origin.y) / cellSize);
        return new Vector2Int(x, y);
    }

    /// <summary>
    /// 그리드 셀 좌표를 해당 셀의 중심 월드 좌표(Vector3)로 변환.
    /// </summary>
    /// <param name="gridPos">변환할 그리드 좌표</param>
    /// <returns>그리드 셀의 중심에 해당하는 월드 좌표</returns>
    public static Vector3 GridToWorld(Vector2Int gridPos, Vector3 origin, float cellSize)
    {
        return new Vector3(gridPos.x * cellSize + cellSize * 0.5f,
                           gridPos.y * cellSize + cellSize * 0.5f,
                           0f) + origin;
    }
}
