using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Pathfinding;
using Pathfinding.Util;

[RequireComponent(typeof(Seeker))]
[RequireComponent(typeof(Rigidbody2D))]
public class DotweenMoveAvoidance : MonoBehaviour
{
    
    public float moveSpeed = 3f;
    public float separationRadius = 1.5f;
    public float separationForce = 1f;
    public LayerMask unitLayer;

    private Unit unit;
    private Seeker seeker;
    private Rigidbody2D rb;
    private Tween pathTween;

    private void Awake()
    {
        seeker = GetComponent<Seeker>();
        rb = GetComponent<Rigidbody2D>();
        unit = GetComponent<Unit>();
    }

    private void Start()
    {
    }

    public void MoveTo(Vector3 targetPosition, float moveSpeed)
    {
        this.moveSpeed = moveSpeed;
        if (seeker.IsDone())
        {
            seeker.StartPath(transform.position, targetPosition, OnPathComplete);
        }
    }

    private void OnPathComplete(Path path)
    {
        if (path.error || path.vectorPath.Count < 2) return;

        if (pathTween != null && pathTween.IsActive())
            pathTween.Kill();

        float duration = path.vectorPath.Count * (1f / moveSpeed);
        pathTween = transform.DOPath(path.vectorPath.ToArray(), duration, PathType.CatmullRom, PathMode.TopDown2D)
            .SetEase(Ease.Linear)
            .OnUpdate(() => ApplySeparation())
            .OnComplete(() => Debug.Log($"{name} 이동 완료"));
    }

    private void ApplySeparation()
    {
        Collider2D[] neighbors = Physics2D.OverlapCircleAll(transform.position, separationRadius, unitLayer);

        Vector2 push = Vector2.zero;
        foreach (Collider2D neighbor in neighbors)
        {
            if (neighbor.gameObject == gameObject) continue;

            Vector2 diff = (Vector2)(transform.position - neighbor.transform.position);
            float dist = diff.magnitude;
            if (dist > 0f)
                push += diff.normalized / dist; // 더 가까운 유닛일수록 강하게 밀어냄
        }

        if (push != Vector2.zero)
        {
            transform.position += (Vector3)(push.normalized * separationForce * Time.deltaTime);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, separationRadius);
    }
}
