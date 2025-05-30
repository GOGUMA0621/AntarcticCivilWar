using UnityEngine;

public class WorldDragController
{
    private MonoBehaviour currentTarget;
    private bool isDragging = false;

    public void BeginDrag()
    {
        Vector3 worldPos = InputManager.instance.GetPointerWorldPosition();
        currentTarget = FindDraggableUnderPointer();

        if (currentTarget is IBeginWorldDragHandler beginHandler)
        {
            beginHandler.OnBeginDrag(CreateDragEventData(worldPos));
            isDragging = true;
        }
    }

    public void Drag()
    {
        if (!isDragging || currentTarget == null) return;
        Vector3 worldPos = InputManager.instance.GetPointerWorldPosition();

        if (currentTarget is IWorldDragHandler dragHandler)
        {
            dragHandler.OnDrag(CreateDragEventData(worldPos));
        }
    }

    public void EndDrag()
    {
        if (!isDragging || currentTarget == null) return;
        Vector3 worldPos = InputManager.instance.GetPointerWorldPosition();

        if (currentTarget is IEndWorldDragHandler endHandler)
        {
            endHandler.OnEndDrag(CreateDragEventData(worldPos));
        }

        currentTarget = null;
        isDragging = false;
    }

    private MonoBehaviour FindDraggableUnderPointer()
    {
        int mask = ~(1 << LayerMask.NameToLayer("Detector"));
        Ray ray = Camera.main.ScreenPointToRay(InputManager.instance.GetPointerScreenPosition());

        RaycastHit2D hit2D = Physics2D.GetRayIntersection(ray, 10f, mask);
        if (hit2D.collider != null && hit2D.collider.TryGetComponent<IWorldDraggable>(out var _))
        {
            
            return hit2D.collider.GetComponent<IWorldDraggable>() as MonoBehaviour;
        }
            

        if (Physics.Raycast(ray, out RaycastHit hit3D, 100f, mask))
        {
            if (hit3D.collider.TryGetComponent<IWorldDraggable>(out var _))
            {
                Debug.Log($"{hit3D.collider.name}");
                return hit3D.collider.GetComponent<IWorldDraggable>() as MonoBehaviour;
            }
        }

        return null;
    }

    public DragEventData CreateDragEventData(Vector3 worldPosition)
    {
        return new DragEventData(worldPosition, InputManager.instance.GetPointerScreenPosition(), Vector2.zero, true, Time.time);
    }
}

public interface IWorldDraggable
{

}

public interface IBeginWorldDragHandler : IWorldDraggable
{
    public void OnBeginDrag(DragEventData data);
}

public interface IWorldDragHandler : IWorldDraggable
{
    public void OnDrag(DragEventData data);
}

public interface IEndWorldDragHandler : IWorldDraggable
{
    public void OnEndDrag(DragEventData data);
}

/// <summary>
/// 드래그 이벤트시 필요한 data 클래스입니다.
/// 월드 포지션, 스크린 포지션, delta, isTouch, 시간 등 여러 요소가 포함되어 있습니다.
/// </summary>
public class DragEventData
{
    public Vector3 worldPostion;
    public Vector2 screenPosition;
    public Vector2 delta;
    public bool isTouch;
    public float time;

    public DragEventData(Vector3 worldPosition, Vector2 screenPosition, Vector2 delta, bool isTouch, float time)
    {
        this.worldPostion = worldPosition;
        this.screenPosition = screenPosition;
        this.delta = delta;
        this.isTouch = isTouch;
        this.time = time;
    }
}
