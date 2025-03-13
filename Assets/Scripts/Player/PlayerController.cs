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

        moveInput = InputManager.Instance.GetMoveDirection() * playerSpeed;
        rb.velocity = new Vector2 (moveInput.x, moveInput.y);

        playerPos = rb.position;

        FlipAniamtion();
        animator.SetFloat("Speed", rb.velocity.magnitude);
    }

    void FlipAniamtion()
    {
        Vector3 currentPosition = transform.position;
        float movementDirection = currentPosition.x - lastPosition.x;

        if (movementDirection > 0)
        {
            spriteRenderer.flipX = false; // 오른쪽으로 이동하면 기본 상태
        }
        else if (movementDirection < 0)
        {
            spriteRenderer.flipX = true; // 왼쪽으로 이동하면 스프라이트 반전
        }

        lastPosition = currentPosition;
    }
}
