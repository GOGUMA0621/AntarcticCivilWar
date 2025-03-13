using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering.LookDev;
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
