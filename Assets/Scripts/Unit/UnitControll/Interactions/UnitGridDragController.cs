using System;
using UnityEngine;

public class UnitDragController : MonoBehaviour, IBeginWorldDragHandler, IWorldDragHandler, IEndWorldDragHandler
{
    private Vector3 startPosition;
    private PlacementGridManager gridManager;
    private UnitController unit;
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
        if (unit.GetCurrentState() is not UnitPlaceState)
        {
            Debug.LogWarning("유닛이 배치 상태가 아닙니다. 드래그할 수 없습니다.");
            return;
        }

        if (unit.tag == "Enemy" || !canDrag)
        {
            Debug.LogWarning("적 유닛은 드래그할 수 없습니다.");
            return;
        }
        Debug.Log("드래그 시작");
        unit.unit.rb.simulated = false;
        startPosition = this.transform.position;
        if (unit == null) return;

         // === 같은 좌표에 있는 드롭핸들러 접근 및 메소드 실행 ===
        int droppableLayer = LayerMask.NameToLayer("Droppable");
        int mask = 1 << droppableLayer;
        Ray ray = Camera.main.ScreenPointToRay(InputManager.instance.GetPointerScreenPosition());
        RaycastHit2D hit = Physics2D.GetRayIntersection(ray, 10f, mask);
        if (hit.collider != null)
        {
            var dropHandler = hit.collider.GetComponent<IWorldDropHandler>();
            if (dropHandler != null)
            {
                Debug.Log($"드래그 시작 위치에 {hit.collider.name} 발견");
                // 원하는 메소드 호출 (예시)
                dropHandler.OnDragSourceRemoved(new DragEventData
                (
                    this.transform.position,
                    InputManager.instance.GetPointerScreenPosition(),
                    Vector2.zero,
                    true,
                    Time.time,
                    unit
                ));
            }
        }

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

        int droppableLayer = LayerMask.NameToLayer("Droppable");
        int mask = 1 << droppableLayer;

        // OverlapPoint는 해당 위치에 있는 모든 콜라이더를 반환
        Collider2D[] hits = Physics2D.OverlapPointAll(worldPos, mask);
        foreach (var col in hits)
        {
            var dropHandler = col.GetComponent<IWorldDropHandler>();
            if (dropHandler != null)
            {
                Debug.Log("드롭 핸들러 발견: " + col.name);
                return; // 드롭이 성공했으므로 더 이상 진행하지 않음
            }
        }

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
                transform.position = GridUtility.GridToWorld(startPosition, origin, cellSize);
            }
            SynergyManager.instance.RegisterUnit(unit, true);
        }
    }
}
