using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ProjectileController))]
public class ProjectileVisual : MonoBehaviour
{
    [SerializeField] private Transform projectileVisual;
    [SerializeField] private Transform projectileShadow;
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
        UpdateShadowPosition();

        float trajectoryProgressMagnitude = (transform.position - trajectoryStartPoint).magnitude;
        float trajectoryMagnitude = (target.position - trajectoryStartPoint).magnitude;

        float trajectoryProgressNormalized = trajectoryProgressMagnitude / trajectoryMagnitude;

        if (trajectoryProgressNormalized > shadowPositionDivider)
        {
            UpdateShadowRotation();
        }
    }

    private void UpdateShadowPosition()
    {
        Vector3 trajectoryRange = target.position - trajectoryStartPoint;
        Vector3 newPostion = transform.position;

        if (MathF.Abs(trajectoryRange.normalized.x) < MathF.Abs(trajectoryRange.normalized.y))
        {
            newPostion.x = trajectoryStartPoint.x + projectile.GetNextXTrajectoryPosition() / shadowPositionDivider + projectile.GetNextPositionXCorrectionAbsolute();
        }
        else
        {
            newPostion.y = trajectoryStartPoint.y + projectile.GetNextYTrajectoryPosition()/ shadowPositionDivider + projectile.GetNextPositionYCorrectionAbsolute();
        }

        projectileShadow.position = newPostion;
    }

    private void UpdateProjectileRotation()
    {
        Vector3 moveDirection = projectile.GetMoveDirection();

        projectileVisual.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg);
    }

    private void UpdateShadowRotation()
    {
        Vector3 moveDirection = projectile.GetMoveDirection();

        projectileShadow.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg);
    }

    public void SetTarget(Transform target)
    {
        this.target = target;
    }
}
