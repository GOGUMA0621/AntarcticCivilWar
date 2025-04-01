using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class InputManager : SingleTonBehaviour<InputManager>
{
    private Vector2 moveDirection = Vector2.zero;

    private bool movePressed;
    private bool interActionPressed;
    private bool callPressed;

    protected override void Awake()
    {
        base.Awake();
    }

    public void OnClick(InputAction.CallbackContext context)
    {
        if (context.started) return;

        if (Camera.main == null || Mouse.current == null) return;

        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        var hit = Physics2D.GetRayIntersection(ray);
        if (!hit.collider) return;

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

    public void TriggerCall()
    {
        callPressed = true;
    }
}
