using UnityEngine;

public class WorldDragController
{
    private MonoBehaviour currentTarget;
    private bool isDragging = false;
    private bool isPressing = false;
    private float pressStartTime = 0f;
    public float holdThreshold = 0.3f;

    public void OnPointerDown()
    {
        isPressing = true;
        pressStartTime = Time.time;
    }

    public void OnPointerUp()
    {
        if (isDragging)
        {
            Drop();
            EndDrag();
        }
        isPressing = false;
        isDragging = false;
    }

    public void OnUpdate()
    {
        if (isPressing && !isDragging)
        {
            if (Time.time - pressStartTime >= 0.3f) // 0.3초 이상 누르고 있으면 드래그 시작
            {
                isDragging = true;
                BeginDrag();
            }
        }

        if (isDragging)
        {
            Drag();
        }
    }
    public void BeginDrag(DragEventData data = null)
    {
        Vector3 worldPos = InputManager.instance.GetPointerWorldPosition();

        if (data != null && data.source != null)
        {
            currentTarget = data.source;
        }
        else
        {
            currentTarget = FindDraggableUnderPointer();
        }

        if (currentTarget is IBeginWorldDragHandler beginHandler)
        {
            isDragging = true;
            isPressing = false;
            beginHandler.OnBeginDrag(CreateDragEventData(worldPos));
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

    public void Drop()
    {
        if (!isDragging) return;

        // 드래그 종료 위치에서 레이캐스트
        int droppableLayer = LayerMask.NameToLayer("Droppable");
        int mask = 1 << droppableLayer;
        Vector3 worldPos = InputManager.instance.GetPointerWorldPosition();
        Ray ray = Camera.main.ScreenPointToRay(InputManager.instance.GetPointerScreenPosition());
        RaycastHit2D hit = Physics2D.GetRayIntersection(ray, 10f, mask);

        if (hit.collider != null)
        {
            var dropHandler = hit.collider.GetComponent<IWorldDropHandler>();
            Debug.Log($"드롭 위치에 {dropHandler} 발견");
            if (dropHandler != null)
            {
                dropHandler.OnDrop(CreateDragEventData(worldPos));
            }
        }
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
        return new DragEventData(worldPosition, InputManager.instance.GetPointerScreenPosition(), Vector2.zero, true, Time.time, currentTarget);
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

public interface IWorldDropHandler : IWorldDraggable
{
    public void OnDrop(DragEventData data);
    public void OnDragSourceRemoved(DragEventData data);
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
    public MonoBehaviour source;

    public DragEventData(Vector3 worldPosition, Vector2 screenPosition, Vector2 delta, bool isTouch, float time, MonoBehaviour source)
    {
        this.worldPostion = worldPosition;
        this.screenPosition = screenPosition;
        this.delta = delta;
        this.isTouch = isTouch;
        this.time = time;
        this.source = source;
    }
}
