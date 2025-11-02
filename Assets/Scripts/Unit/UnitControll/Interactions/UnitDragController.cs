using System;
using UnityEngine;

public class UnitDragController : MonoBehaviour, IBeginWorldDragHandler, IWorldDragHandler, IEndWorldDragHandler, IWorldDraggable
{
    public bool isDropAllowed => isDropAllowedToBench;
    public bool isDropAllowedToBench = true; // bench에 드롭 허용 여부
    private Vector3 startPosition;
    private PlacementGridManager gridManager;
    private UnitController unit;
    public bool canDrag = true;

    public void Awake()
    {
        gridManager = GridManager.instance.allayGrid;
        unit = GetComponent<UnitController>();
        
    }

    public void OnBeginDrag(DragEventData data)
    {
        Debug.Log($"OnBeginDrag 호출됨 - {gameObject.name}");
        Debug.Log($"현재 상태: {unit.GetCurrentState()?.GetType().Name}");
        Debug.Log($"canDrag: {canDrag}, Tag: {unit.tag}");
        
        if (unit.GetCurrentState() is not UnitPlaceState)
        {
            Debug.LogWarning($"유닛이 배치 상태가 아닙니다. 현재 상태: {unit.GetCurrentState()?.GetType().Name}");
            return;
        }

        if (unit.tag == "Enemy" || !canDrag)
        {
            Debug.LogWarning($"드래그 불가 - Enemy: {unit.tag == "Enemy"}, canDrag: {canDrag}");
            return;
        }
        Debug.Log("드래그 시작");
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
                if (!isDropAllowed)
                {
                    Debug.Log("이 오브젝트에는 드롭할 수 없습니다.");
                    break; // 드롭이 허용되지 않으면 다음 콜라이더 검사
                }
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
