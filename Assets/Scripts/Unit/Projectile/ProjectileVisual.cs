using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ProjectileController))]
public class ProjectileVisual : MonoBehaviour
{
    [SerializeField] private Transform projectileVisual;
    [SerializeField] private ProjectileController projectile;

    private Transform target;
    private Vector3 trajectoryStartPoint;

    [SerializeField] private float shadowPositionDivider = 6f;

    private void Start()
    {
        trajectoryStartPoint = transform.position;
    }

    private void Update()
    {
        UpdateProjectileRotation();

    }

    private void UpdateProjectileRotation()
    {
        Vector3 moveDirection = projectile.GetMoveDirection();

        projectileVisual.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg);
    }


    public void SetTarget(Transform target)
    {
        this.target = target;
    }
}
