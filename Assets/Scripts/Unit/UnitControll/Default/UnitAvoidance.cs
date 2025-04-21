using UnityEngine;
using Pathfinding;

[RequireComponent(typeof(AIPath), typeof(Collider2D), typeof(Rigidbody2D))]
public class UnitAvoidance : MonoBehaviour
{
    private Rigidbody2D rb;
    private AIPath aiPath;

    [SerializeField] private float sidestepSpeed = 1f;
    [SerializeField] private LayerMask unitLayer;
    [SerializeField] private float avoidanceStrength = 0.5f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        aiPath = GetComponent<AIPath>();
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (((1 << collision.gameObject.layer) & unitLayer) != 0)
        {
            Vector2 away = (transform.position - collision.transform.position).normalized;

            // 2D 기준 수직 방향으로 비켜가기 (-y, x)
            Vector2 sidestep = Random.value > 0.5f
                ? new Vector2(-away.y, away.x) // 왼쪽
                : new Vector2(away.y, -away.x); // 오른쪽

            // 회피 이동 적용
            rb.MovePosition(rb.position + sidestep * sidestepSpeed * avoidanceStrength * Time.fixedDeltaTime);
        }
    }
}
