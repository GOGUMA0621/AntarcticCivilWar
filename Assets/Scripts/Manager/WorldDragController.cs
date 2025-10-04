using System.Runtime.InteropServices;
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

        // 항상 currentTarget을 새로 찾음
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
            Debug.Log($"BeginDrag 호출됨 - {currentTarget.name}");
            beginHandler.OnBeginDrag(CreateDragEventData(worldPos));
        }
        else
        {
            // currentTarget이 없으면 드래그 상태 초기화
            isDragging = false;
            isPressing = false;
            currentTarget = null;
        }
    }

    public void Drag()
    {
        
        if (!isDragging || currentTarget == null) return;
        Vector3 worldPos = InputManager.instance.GetPointerWorldPosition();

        if (currentTarget is IWorldDragHandler dragHandler)
        {
            Debug.Log($"Drag 호출됨 - {currentTarget.name}");
            dragHandler.OnDrag(CreateDragEventData(worldPos));
        }
    }

    public void EndDrag()
    {
       
        if (!isDragging || currentTarget == null || InputManager.instance == null) return;
        Vector3 worldPos = InputManager.instance.GetPointerWorldPosition();

        if (currentTarget is IEndWorldDragHandler endHandler)
        {
            Debug.Log($"EndDrag 호출됨 - {currentTarget.name}");
            endHandler.OnEndDrag(CreateDragEventData(worldPos));
            currentTarget = null;
        }
        currentTarget = null;

        isDragging = false;
    }

    public void Drop()
    {
        if (!isDragging || InputManager.instance == null) return;
        if (currentTarget == null) return;

        // Drop 금지 조건 추가 (예: canDropToBench가 false면 Drop 금지)
        if (currentTarget.TryGetComponent(out IWorldDraggable worldDragHandler))
        {
            if (!worldDragHandler.isDropAllowed)
            {
                return;
            }
        }

        int droppableLayer = LayerMask.NameToLayer("Droppable");
        int mask = 1 << droppableLayer;
        Vector3 worldPos = InputManager.instance.GetPointerWorldPosition();
        Ray ray = Camera.main.ScreenPointToRay(InputManager.instance.GetPointerScreenPosition());
        RaycastHit2D hit = Physics2D.GetRayIntersection(ray, 10f, mask);

        if (hit.collider != null)
        {
            var dropHandler = hit.collider.GetComponent<IWorldDropHandler>();
            if (dropHandler != null)
            {
                dropHandler.OnDrop(CreateDragEventData(worldPos));
            }
        }
    }

    private MonoBehaviour FindDraggableUnderPointer()
    {
        Debug.Log("FindDraggableUnderPointer 호출됨");
        int mask = ~(1 << LayerMask.NameToLayer("Detector")); // Detector 레이어 제외
        Vector3 worldPos = InputManager.instance.GetPointerWorldPosition();
        Collider2D col = Physics2D.OverlapPoint(worldPos, mask);
        if (col != null && col.TryGetComponent<IWorldDraggable>(out var draggable) && !col.TryGetComponent<IWorldDropHandler>(out var _))
        {
            Debug.Log($"드래그 감지: {col.name}");
            return draggable as MonoBehaviour;
        }

        Debug.Log("드래그 가능한 오브젝트를 찾지 못함");
        return null;
    }

    public DragEventData CreateDragEventData(Vector3 worldPosition)
    {
        return new DragEventData(worldPosition, InputManager.instance.GetPointerScreenPosition(), Vector2.zero, true, Time.time, currentTarget);
    }
}

public interface IWorldDraggable
{
    public bool isDropAllowed { get; }
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
