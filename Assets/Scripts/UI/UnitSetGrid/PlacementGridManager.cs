using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlacementGridManager : MonoBehaviour
{
    public enum GridType { Allay, Enemy }
    public GridType gridType;

    public int width = 10;      // 그리드 가로 크기
    public int height = 10;     // 그리드 세로 크기
    public float cellSize = 1f; // 한 칸 크기 
    public Vector3 origin = Vector3.zero;

    private Unit[,] grid;   // 유닛 배치 상태 저장
    private Node[,] nodes;  // 그리드 노드 상태 저장

    void Awake()
    {
        grid = new Unit[width, height];
        nodes = new Node[width, height];

        if (gridType == GridType.Allay)
            GridManager.instance.RegisterAllayGrid(this);
        else if (gridType == GridType.Enemy)
            GridManager.instance.RegisterEnemyGrid(this);
    }

    /// <summary>
    /// 지정한 그리드 좌표에 유닛을 배치할 수 있는지 확인합니다.
    /// </summary>
    /// <param name="pos">확인할 그리드 좌표</param>
    /// <returns>해당 위치가 그리드 범위 and 유닛 없으면 true반환</returns>
    public bool CanPlace(Vector2Int pos)
    {
        return IsInsideGrid(pos) && grid[pos.x, pos.y] == null;
    }

    /// <summary>
    /// 지정한 그리드 좌표에 유닛을 배치 and 유닛 오브젝트를 해당 위치로 이동.
    /// </summary>
    /// <param name="unit">배치할 유닛</param>
    /// <param name="pos">배치할 그리드 좌표</param>
    public void PlaceUnit(Unit unit, Vector2Int pos)
    {
        if (!CanPlace(pos)) return;

        grid[pos.x, pos.y] = unit;
        unit.transform.position = GridUtility.GridToWorld(pos, origin, cellSize);
        unit.tag = "Allay"; // 유닛 태그를 "Allay"로 설정
        SynergyManager.instance.RegisterUnit(unit.controller, true);
        foreach (var enemy in UnitManager.instance.enemyList)
        {
            if (enemy != null)
            {
                unit.detectTarget.AddTarget(enemy.GetComponent<IDamageAble>());
            }
        }
    }

    /// <summary>
    /// 지정한 그리드 좌표가 유효한 그리드 범위 내에 있는지 확인.
    /// </summary>
    /// <param name="pos">확인할 그리드 좌표</param>
    /// <returns>좌표가 0 이상이고, 너비 및 높이 범위 내에 있으면 true 반환</returns>
    public bool IsInsideGrid(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < width && pos.y >= 0 && pos.y < height;
    }

    /// <summary>
    /// 지정한 그리드 좌표에서 유닛을 제거.
    /// </summary>
    /// <param name="pos">제거할 유닛의 그리드 좌표</param>
    public void RemoveUnit(Vector2Int pos)
    {
        if (!IsInsideGrid(pos)) return;

        grid[pos.x, pos.y] = null;
    }

    /// <summary>
    /// 지정한 그리드 좌표에 있는 유닛을 반환.
    /// </summary>
    /// <param name="pos">지정한 그리드 좌표</param>
    /// <returns>해당 위치에 배치된 유닛 객체를 반환</returns>
    public Unit GetUnitByPos(Vector2Int pos)
    {
        if (!IsInsideGrid(pos)) return null;

        return grid[pos.x, pos.y];
    }

    /// <summary>
    /// 그리드의 중앙 좌표를 반환.
    /// </summary>
    /// <returns></returns>
    public Vector2Int GetCenterGridPos()
    {
        int centerX = width / 2;
        int centerY = height / 2;
        return new Vector2Int(centerX, centerY);
    }

    /// <summary>
    /// 그리드의 중앙 월드 좌표를 반환.
    /// </summary>
    /// <returns></returns>
    public Vector3 GetCenterWorldPos()
    {
        Vector2Int centerGrid = GetCenterGridPos();
        return GridUtility.GridToWorld(centerGrid, origin, cellSize);
    }

    /// <summary>
    /// 중심 좌표(center) 기준으로 가장 가까운 비어있는 그리드 좌표를 반환.
    /// (center가 비어있으면 center를 반환, 아니면 주변에서 탐색)
    /// </summary>
    public Vector2Int? GetNearestEmptyGrid(Vector2Int center)
    {
        if (CanPlace(center))
            return center;

        int maxRadius = Mathf.Max(width, height);
        for (int radius = 1; radius < maxRadius; radius++)
        {
            for (int dx = -radius; dx <= radius; dx++)
            {
                for (int dy = -radius; dy <= radius; dy++)
                {
                    // 테두리만 검사 (정사각형의 외곽)
                    if (Mathf.Abs(dx) != radius && Mathf.Abs(dy) != radius)
                        continue;

                    Vector2Int checkPos = new Vector2Int(center.x + dx, center.y + dy);
                    if (IsInsideGrid(checkPos) && CanPlace(checkPos))
                        return checkPos;
                }
            }
        }
        // 비어있는 그리드가 없으면 null 반환
        return null;
    }

    /// <summary>
    /// 지정한 그리드 좌표의 월드 위치를 반환.
    /// </summary>
    public Vector3 GetGridWorldPos(Vector2Int pos)
    {
        if (!IsInsideGrid(pos)) return Vector3.zero;

        return GridUtility.GridToWorld(pos, origin, cellSize);
    }
}
