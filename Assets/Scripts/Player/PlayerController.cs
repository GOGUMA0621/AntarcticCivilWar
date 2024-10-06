using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public float playerSpeed;
    private Vector2 _moveInput;
    public Vector2 playerPos;
    private Rigidbody2D _rb;
    private Animator _animator;
    private Vector2 _lastPosition;
    private SpriteRenderer _spriteRenderer;
    

    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _lastPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        
        _moveInput.x = Input.GetAxis("Horizontal") * playerSpeed;
        _moveInput.y = Input.GetAxis("Vertical") * playerSpeed;
        _rb.velocity = new Vector2 (_moveInput.x, _moveInput.y);

        playerPos = _rb.position;

        FlipAniamtion();
        _animator.SetFloat("Speed", _rb.velocity.magnitude);
    }

    void FlipAniamtion()
    {
        Vector3 currentPosition = transform.position;
        float movementDirection = currentPosition.x - _lastPosition.x;

        if (movementDirection > 0)
        {
            _spriteRenderer.flipX = false; // 오른쪽으로 이동하면 기본 상태
        }
        else if (movementDirection < 0)
        {
            _spriteRenderer.flipX = true; // 왼쪽으로 이동하면 스프라이트 반전
        }

        _lastPosition = currentPosition;
    }
}
