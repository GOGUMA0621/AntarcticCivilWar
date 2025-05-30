using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class InputManager : SingleTonBehaviour<InputManager>
{
    private Vector2 moveDirection = Vector2.zero;
    private WorldDragController dragController = new WorldDragController();
    private Vector2 pointerPosition = Vector2.zero;
    public PlayerInput playerInput;

    [HideInInspector] public bool callTriggerd = false;

    private bool movePressed;
    private bool interActionPressed;
    private bool callPressed;
    private bool revivePressed;

    private bool isPressing = false;
    private bool isDragging = false;
    private float pressStartTime = 0f;
    [SerializeField] private float holdThreshold = 0.3f;

    protected override void Awake()
    {
        base.Awake();
        playerInput = GetComponent<PlayerInput>();
    }

    private void Update()
    {
        if (isPressing && !isDragging)
        {
            if (Time.time - pressStartTime >= holdThreshold)
            {
                isDragging = true;
            }
        }

        if (isDragging)
        {
            dragController.Drag();
        }
    }

    public void OnClick(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            dragController.BeginDrag();

            isPressing = true;
            pressStartTime = Time.time;
        }
        else if (context.canceled)
        {
            if (isDragging)
            {
                dragController.EndDrag();
            }

            isPressing = false;
            isDragging = false;
        }
    }

    public void OnPoint(InputAction.CallbackContext context)
    {
        Vector2 pos = context.ReadValue<Vector2>();
        pointerPosition = pos;
    }


    public void MovePressed(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            moveDirection = context.ReadValue<Vector2>();
            movePressed = true;
        }
        else if (context.canceled)
        {
            moveDirection = context.ReadValue<Vector2>();
            movePressed = false;
        }
    }

    public void InterActionPressed(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            interActionPressed = true;
        }
        else if (context.canceled)
        {
            interActionPressed= false;
        }
    }

    public void CallPressed(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            callPressed = true;
        }
        else if (context.canceled)
        {
            callPressed= false;
        }
    }

    public void RevivePressed(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            revivePressed = true;
        }
        else if (context.canceled)
        {
            revivePressed = false;
        }
    }

    public Vector2 GetMoveDirection()
    {
        return moveDirection;
    }

    public bool GetMovePressed()
    {
        bool result = movePressed;
        movePressed = false;

        return result;
    }

    public bool GetInterActionPressed()
    {
        bool result = interActionPressed;
        interActionPressed = false;

        return result;
    }

    public void TriggerInterAcion()
    {
        interActionPressed = true;
    }

    public bool GetCallPressed()
    {
        bool result = callPressed;
        callPressed = false;

        return result;
    }

    public bool GetRevivePressed()
    {
        bool result = revivePressed;
        revivePressed = false;

        return result;
    }

    public void TriggerCall()
    {
        callPressed = true;
    }

    public void TtriggerRevive()
    {
        revivePressed = true;
    }
    
    public Vector2 GetPointerScreenPosition()
    {
        return pointerPosition;
    }
    /// <summary>
    /// 카메라의 종류(orthographic or perspective)에 따라 포인터의 월드 좌표를 반환합니다.
    /// 만약 orthographic 카메라라면, z축은 0으로 설정됩니다.
    /// 만약 perspective 카메라라면, z축은 nearClipPlane으로 설정됩니다.
    /// 만약 customZ가 null이 아닐 경우, 해당 값을 z축으로 사용합니다.
    /// </summary>
    /// <param name="customZ">Perspective일 경우 수동으로 지정한 z 값 (null이면 nearClipPlane값 사용)</param>
    /// <returns></returns>
    public Vector3 GetPointerWorldPosition(float? customZ = null)
    {
        Camera cam = Camera.main;
        Vector2 screenPosition = GetPointerScreenPosition();

        float z;
        if (cam.orthographic)
        {
            z = 0f;
        }
        else
        {
            z = customZ ?? cam.nearClipPlane;
        }
        Vector3 screenPoint = new Vector3(screenPosition.x, screenPosition.y, z);
        Vector3 worldPosition = cam.ScreenToWorldPoint(screenPoint);

        return worldPosition;
    }

    
}
