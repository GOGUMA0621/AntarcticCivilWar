using UnityEngine;

public class UnitDragController : MonoBehaviour, IBeginWorldDragHandler, IWorldDragHandler, IEndWorldDragHandler
{
    private Vector3 startPosition;
    private PlacementGridManager gridManager;
    private UnitController unit;

    public void Awake()
    {
        gridManager = FindAnyObjectByType<PlacementGridManager>();
        unit = GetComponent<UnitController>();
    }

    public void OnBeginDrag(DragEventData data)
    {
        Debug.Log("드래그 시작");
        startPosition = this.transform.position;
        if (unit == null) return;
        SynergyManager.instance.UnregisterUnit(unit, true);
    }

    public void OnDrag(DragEventData data)
    {
        Vector3 worldPos = data.worldPostion;
        worldPos.z = 0;

        this.transform.position = worldPos;
    }

    public void OnEndDrag(DragEventData data)
    {
        Vector3 worldPos = data.worldPostion;
        worldPos.z = 0;

        if (gridManager != null)
        {
            Vector3 origin = gridManager.origin;
            float cellSize = gridManager.cellSize;
            Vector2Int gridPos = GridUtility.WorldToGrid(worldPos, origin, cellSize);
            if (gridManager.CanPlace(gridPos))
            {
                this.transform.position = GridUtility.GridToWorld(gridPos, origin, cellSize);
            }
            else
            {
                this.transform.position = startPosition;
            }
        }
        else
        {
            this.transform.position = startPosition;
        }
        SynergyManager.instance.RegisterUnit(unit, true);
    }
}
