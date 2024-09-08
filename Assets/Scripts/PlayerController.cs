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

    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        _moveInput.x = Input.GetAxis("Horizontal") * playerSpeed;
        _moveInput.y = Input.GetAxis("Vertical") * playerSpeed;
        _rb.velocity = new Vector2 (_moveInput.x, _moveInput.y);

        playerPos = _rb.position;
        
    }
}
