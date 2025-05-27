using System;
using UnityEngine;

public class UnitGridDragController : MonoBehaviour, IBeginWorldDragHandler, IWorldDragHandler, IEndWorldDragHandler
{
    private Vector3 startPosition;
    private PlacementGridManager gridManager;
    private UnitController unit;
    [NonSerialized]
    public bool canDrag = true;

    public void Awake()
    {
        var grid = FindObjectsByType<PlacementGridManager>(FindObjectsSortMode.None);
        foreach (var g in grid)
        {
            if (g != null && g.tag == "Allay")
            {
                gridManager = g;
                break;
            }
        }
        unit = GetComponent<UnitController>();
    }

    public void OnBeginDrag(DragEventData data)
    {
        if(unit.tag == "Enemy" || !canDrag)
        {
            Debug.LogWarning("적 유닛은 드래그할 수 없습니다.");
            return;
        }
        Debug.Log("드래그 시작");
        unit.unit.rb.simulated = false;
        startPosition = this.transform.position;
        if (unit == null) return;
        var pos = GridUtility.WorldToGrid(startPosition, gridManager.origin, gridManager.cellSize);
        gridManager.RemoveUnit(pos);
        SynergyManager.instance.UnregisterUnit(unit, true);
    }

    public void OnDrag(DragEventData data)
    {
        if(unit.tag == "Enemy" || !canDrag) return;

        Vector3 worldPos = data.worldPostion;
        worldPos.z = 0;

        this.transform.position = worldPos;
    }

    public void OnEndDrag(DragEventData data)
    {
        if(unit.tag == "Enemy" || !canDrag) return;

        Vector3 worldPos = data.worldPostion;
        unit.unit.rb.simulated = true;
        worldPos.z = 0;

        if (gridManager != null)
        {
            Vector3 origin = gridManager.origin;
            float cellSize = gridManager.cellSize;
            Vector2Int gridPos = GridUtility.WorldToGrid(worldPos, origin, cellSize);
            if (gridManager.CanPlace(gridPos))
            {
                transform.position = GridUtility.GridToWorld(gridPos, origin, cellSize);
            }
            else
            {
                Vector2Int startPosition = GridUtility.WorldToGrid(this.startPosition, origin, cellSize);
            }
        }
        SynergyManager.instance.RegisterUnit(unit, true);
    }
}
