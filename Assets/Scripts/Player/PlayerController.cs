using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public float playerSpeed;
    private Vector2 moveInput;
    public Vector2 playerPos;
    private Rigidbody2D rb;
    private Animator animator;
    private Vector2 lastPosition;
    private SpriteRenderer spriteRenderer;

    private Vector2Int lastGridPosition = Vector2Int.zero;


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        lastPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {

        moveInput = InputManager.instance.GetMoveDirection() * playerSpeed;
        rb.linearVelocity = new Vector2 (moveInput.x, moveInput.y);

        playerPos = rb.position;

        FlipAniamtion();
        animator.SetFloat("Speed", rb.linearVelocity.magnitude);

        if(InputManager.instance.GetCallPressed())
        {
            if(InputManager.instance.callTriggerd)
            {
                Debug.Log("���� ����");
                InputManager.instance.callTriggerd = false;
                UnitManager.instance.ChangeStateAllayList("IdleState");
            }
            else if(!InputManager.instance.callTriggerd)
            {
                Debug.Log("����");
                InputManager.instance.callTriggerd = true;
                UnitManager.instance.ChangeStateAllayList("CallState");
            }
        }
        if (InputManager.instance.GetRevivePressed())
        {
            UnitManager.instance.ReviveAllUnit();
        }
    }

    void FlipAniamtion()
    {
        Vector3 currentPosition = transform.position;
        float movementDirection = currentPosition.x - lastPosition.x;

        if (movementDirection > 0)
        {
            spriteRenderer.flipX = false; // ���������� �̵��ϸ� �⺻ ����
        }
        else if (movementDirection < 0)
        {
            spriteRenderer.flipX = true; // �������� �̵��ϸ� ��������Ʈ ����
        }

        lastPosition = currentPosition;
    }

    private bool HasChangedGridPosition(Vector2Int newGridPosition)
    {
        if (newGridPosition != lastGridPosition)
        {
            lastGridPosition = newGridPosition;
            return true;
        }
        return false;
    }
}
