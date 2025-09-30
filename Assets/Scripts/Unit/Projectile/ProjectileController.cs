using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    [SerializeField] private ProjectileVisual visual;
    private UnitAttackController attackController;

    private Action OnHitCallback;

    private Unit unit;
    private Vector3 currentVelocity;
    private Vector3 previousPos;

    public DamageData projectileDamageData;

    public Transform target { get; private set; }
    [SerializeField] private bool isAOE = false;
    [SerializeField] private float AOERange = 0f;
    private float moveSpeed;
    private float maxMoveSpeed;
    private float distanceToTargetDestroyProjectile = 0.5f;
    private float trajectoryMaxRelativeHeight;

    private AnimationCurve trajectoryAniamaionCurve;
    private AnimationCurve axisCorrectionAnimationCurve;
    private AnimationCurve speedAnimationCurve;

    private Vector3 trajectoryStartPoint;
    private Vector3 projectileMoveDirection;
    private Vector3 trajectoryRange;

    private float nextYTrajectoryPosition;
    private float nextXTrajectoryPosition;
    private float nextPositionYCorrectionAbsolute;
    private float nextPositionXCorrectionAbsolute;

    private bool hasMoved = false;

    private void Start()
    {

        previousPos = transform.localPosition;
    }

    private void Update()
    {
        if (target == null) { Destroy(gameObject); return; }

        UpdateProjectilePosition();

        Vector3 currentPos = transform.position;
        currentVelocity = (currentPos - previousPos) / Time.deltaTime;

        // 최소 1프레임이라도 움직인 뒤 검사하도록
        if (!hasMoved && currentVelocity.magnitude > 0.01f)
            hasMoved = true;

        previousPos = currentPos;

        // 실제 거리 기반 파괴 조건
        if (hasMoved && Vector3.Distance(currentPos, target.position) < distanceToTargetDestroyProjectile)
        {
            TryHitAndDestroy("Destroy Distance");
            return;
        }

        // 움직임이 멈춘 경우
        if (hasMoved && currentVelocity.magnitude < 0.01f)
        {
            TryHitAndDestroy("Destroy Velocity 0");
            return;
        }
    }

    private void OnDestroy()
    {
        if (attackController != null)
            attackController.RemoveProjectile(this);
    }

    void TryHitAndDestroy(string reason = "")
    {
        if (this.TryGetComponent<Animator>(out Animator animator))
        {
            if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
            {
                if (target.TryGetComponent<IDamageAble>(out IDamageAble i))
                {
                    i.ReceiveDamage(projectileDamageData);
                    OnHitAction();
                }
                Debug.Log(reason + " + Animator 완료 후 파괴");
                Destroy(this.gameObject);
            }
        }
        else
        {
            if (target.TryGetComponent<IDamageAble>(out IDamageAble i))
            {
                i.ReceiveDamage(projectileDamageData);
                OnHitAction();
            }
            //Debug.Log(reason + " + 바로 파괴");
            Destroy(this.gameObject);
        }
    }

    private void DoAreaOnEffect()
    {
        if (isAOE)
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, AOERange);
            foreach (Collider2D collider in colliders)
            {
                if (collider.TryGetComponent<IDamageAble>(out IDamageAble target) && collider != this.gameObject && collider.tag != unit.tag)
                {
                    target.ReceiveDamage(projectileDamageData);
                    OnHitAction();
                }
            }
        }
    }

    private void UpdateProjectilePosition()
    {
        trajectoryRange = target.position - trajectoryStartPoint;

        if (Mathf.Abs(trajectoryRange.normalized.x) < Mathf.Abs(trajectoryRange.normalized.y))
        {
            if (trajectoryRange.y < 0)
            {
                moveSpeed = -moveSpeed;
            }

            UpdatePositionWithXCurve();
        }
        else
        {
            if (trajectoryRange.x < 0)
            {
                moveSpeed = -moveSpeed;
            }

            UpdatePositionWithYCurve();
        }


    }

    private void UpdatePositionWithXCurve()
    {
        float nextPositionY = transform.position.y + moveSpeed * Time.deltaTime;
        float nextPositionYNormalized = (nextPositionY - trajectoryStartPoint.y) / trajectoryRange.y;

        float nextPositionXNormailized = trajectoryAniamaionCurve.Evaluate(nextPositionYNormalized);
        nextXTrajectoryPosition = nextPositionXNormailized * trajectoryMaxRelativeHeight;

        float nextPositionXCorrectionNormalized = axisCorrectionAnimationCurve.Evaluate(nextPositionYNormalized);
        nextPositionXCorrectionAbsolute = nextPositionXCorrectionNormalized * trajectoryRange.x;

        float nextPositionX = trajectoryStartPoint.x + nextXTrajectoryPosition + nextPositionXCorrectionAbsolute;

        if (trajectoryRange.x > 0 && trajectoryRange.y > 0)
        {
            nextXTrajectoryPosition = -nextXTrajectoryPosition;
        }

        if (trajectoryRange.x < 0 && trajectoryRange.y < 0)
        {
            nextXTrajectoryPosition = -nextXTrajectoryPosition;
        }

        Vector3 newPosition = new Vector3(nextPositionX, nextPositionY, 0);


        CalculateNextSpeed(nextPositionYNormalized);
        projectileMoveDirection = newPosition - transform.position;

        transform.position = newPosition;
    }

    private void UpdatePositionWithYCurve()
    {
        float nextPositionX = transform.position.x + moveSpeed * Time.deltaTime;
        float nextPositionXNormalized = (nextPositionX - trajectoryStartPoint.x) / trajectoryRange.x;

        float nextPositionYNormailized = trajectoryAniamaionCurve.Evaluate(nextPositionXNormalized);
        nextYTrajectoryPosition = nextPositionYNormailized * trajectoryMaxRelativeHeight;

        float nextPositionYCorrectionNormalized = axisCorrectionAnimationCurve.Evaluate(nextPositionXNormalized);
        nextPositionYCorrectionAbsolute = nextPositionYCorrectionNormalized * trajectoryRange.y;

        float nextPositionY = trajectoryStartPoint.y + nextYTrajectoryPosition + nextPositionYCorrectionAbsolute;

        Vector3 newPosition = new Vector3(nextPositionX, nextPositionY, 0);

        CalculateNextSpeed(nextPositionXNormalized);

        projectileMoveDirection = newPosition - transform.position;

        transform.position = newPosition;
    }

    private void CalculateNextSpeed(float nextPostionXNormalized)
    {
        float nextMoveSpeedNormailized = speedAnimationCurve.Evaluate(nextPostionXNormalized);

        moveSpeed = nextMoveSpeedNormailized * maxMoveSpeed;
    }

    public void InitializeProjectile(Transform target, float maxMoveSpeed, float trajectoryMaxHeight, Unit unit, UnitAttackController attackController = null)
    {
        this.target = target;
        this.maxMoveSpeed = maxMoveSpeed;
        this.unit = unit;
        this.attackController = attackController;

        float xDistanceToTarget = target.position.x - trajectoryStartPoint.x;
        this.trajectoryMaxRelativeHeight = Mathf.Abs(xDistanceToTarget) * trajectoryMaxHeight;
        trajectoryStartPoint = transform.position;

        visual.SetTarget(target);
    }

    public void InitializeDamageData(DamageData damageData)
    {
        this.projectileDamageData = damageData;
    }

    public void InitializeAnimaionCurve(AnimationCurve trajectoyAnimationCure, AnimationCurve axisCorrectionAnimationCurve, AnimationCurve speedAnimationCurve)
    {
        this.trajectoryAniamaionCurve = trajectoyAnimationCure;
        this.axisCorrectionAnimationCurve = axisCorrectionAnimationCurve;
        this.speedAnimationCurve = speedAnimationCurve;
    }

    public void SetOnHitCallback(Action callback)
    {
        if (callback == null) return;
        OnHitCallback = callback;
    }

    private void OnHitAction()
    {
        OnHitCallback?.Invoke();
    }

    public Vector3 GetMoveDirection()
    {
        return projectileMoveDirection;
    }

    public float GetNextYTrajectoryPosition()
    {
        return nextYTrajectoryPosition;
    }

    public float GetNextPositionYCorrectionAbsolute()
    {
        return nextPositionYCorrectionAbsolute;
    }

    public float GetNextXTrajectoryPosition()
    {
        return nextXTrajectoryPosition;
    }

    public float GetNextPositionXCorrectionAbsolute()
    {
        return nextPositionXCorrectionAbsolute;
    }
    
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        visual.SetTarget(newTarget);
        // 필요하다면 trajectoryStartPoint 등도 재설정
    }
}
