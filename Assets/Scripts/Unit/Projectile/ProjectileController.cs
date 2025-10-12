using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// 발사체(Projectile) 동작 제어기
/// - 목표를 향해 곡선(애니메이션 커브 기반)으로 이동
/// - 목표에 도달하거나 속도가 거의 0이 되면 타격 판정 및 파괴
/// - 범위 공격(AOE) 지원
/// </summary>
public class ProjectileController : MonoBehaviour
{
    [Header("Visual / Controller")]
    [SerializeField] private ProjectileVisual visual; // 비주얼 컴포넌트 (타겟 트래킹용)
    private UnitAttackController attackController; // 발사한 유닛의 공격 컨트롤러 (등록/해제용)

    private Action OnHitCallback; // 히트 시 콜백

    private Unit unit; // 발사한 유닛 참조
    private Vector3 currentVelocity; // 현재 프레임 속도 추정치
    private Vector3 previousPos; // 이전 프레임 위치

    public DamageData projectileDamageData; // 이 발사체가 줄 데미지

    public Transform target { get; private set; } // 타겟 Transform (외부에서 세팅)
    [SerializeField] private bool isAOE = false; // AOE 여부
    [SerializeField] private float AOERange = 0f; // AOE 범위
    private float moveSpeed; // 현재 프레임 이동 속도(곡선 기반)
    private float maxMoveSpeed; // 최대 속도
    private float distanceToTargetDestroyProjectile = 0.5f; // 목표 근접 파괴 임계값
    private float trajectoryMaxRelativeHeight; // 곡선 기반 최대 높이(상대값)

    // 애니메이션 커브들(외부에서 설정)
    private AnimationCurve trajectoryAniamaionCurve;
    private AnimationCurve axisCorrectionAnimationCurve; //
    private AnimationCurve speedAnimationCurve;

    // 곡선 이동에 필요한 값들
    private Vector3 trajectoryStartPoint; // 발사 시작 위치
    private Vector3 projectileMoveDirection; // 마지막 프레임의 이동 방향(벡터)
    private Vector3 trajectoryRange; // 목표와 시작점 사이의 벡터

    private float nextYTrajectoryPosition;
    private float nextXTrajectoryPosition;
    private float nextPositionYCorrectionAbsolute;
    private float nextPositionXCorrectionAbsolute;

    private bool hasMoved = false; // 최소 한 프레임이라도 이동했는지 플래그

    private void Start()
    {
        // 초기 이전 위치 세팅 (로컬 위치 사용, 이후 Update에서 world position으로 계산)
        previousPos = transform.localPosition;
    }

    private void Update()
    {
        // 타겟이 사라지면 발사체 제거
        if (target == null) { Destroy(gameObject); return; }

        // 발사체 위치 업데이트 (곡선 이동)
        UpdateProjectilePosition();

        // 현재 속도 계산 (현재 위치 - 이전 위치) / delta
        Vector3 currentPos = transform.position;
        currentVelocity = (currentPos - previousPos) / Time.deltaTime;

        // 적어도 1프레임 이상 움직였는지 판별
        if (!hasMoved && currentVelocity.magnitude > 0.01f)
            hasMoved = true;

        previousPos = currentPos;

        // 목표에 충분히 가까우면 타격 시도 후 파괴
        if (hasMoved && Vector3.Distance(currentPos, target.position) < distanceToTargetDestroyProjectile)
        {
            TryHitAndDestroy("Destroy Distance");
            return;
        }

        // 움직임이 거의 멈춘 경우(정지)도 타격/파괴 처리
        if (hasMoved && currentVelocity.magnitude < 0.01f)
        {
            TryHitAndDestroy("Destroy Velocity 0");
            return;
        }
    }

    private void OnDestroy()
    {
        // 발사체가 파괴될 때, 발사자 컨트롤러에서 목록에서 제거
        if (attackController != null)
            attackController.RemoveProjectile(this);
    }

    /// <summary>
    /// 타격 판정 시도 및 발사체 파괴 처리
    /// - Animator 유무와 상관없이 대상에 데미지 전달
    /// - OnHitCallback 실행
    /// </summary>
    /// <param name="reason">디버그용 이유 문자열</param>
    void TryHitAndDestroy(string reason = "")
    {
        if (this.TryGetComponent<Animator>(out Animator animator))
        {
            // 애니메이터가 있어도 현재는 즉시 데미지 전달 후 파괴
            if (target.TryGetComponent<IDamageAble>(out IDamageAble i))
            {
                i.ReceiveDamage(projectileDamageData);
                Debug.Log(reason + " + 타격 판정");
                OnHitAction();
            }
            Destroy(this.gameObject);
        }
        else
        {
            // 애니메이터가 없는 경우 동일 동작
            if (target.TryGetComponent<IDamageAble>(out IDamageAble i))
            {
                i.ReceiveDamage(projectileDamageData);
                Debug.Log(reason + " + 타격 판정");
                OnHitAction();
            }
            Destroy(this.gameObject);
        }
    }

    /// <summary>
    /// AOE 데미지 처리 (발사체 폭발/타격 시 호출)
    /// </summary>
    private void DoAreaOnEffect()
    {
        if (isAOE)
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, AOERange);
            foreach (Collider2D collider in colliders)
            {
                // 자신의 발사체 및 아군 태그 제외 후 데미지 전달
                if (collider.TryGetComponent<IDamageAble>(out IDamageAble target) && collider != this.gameObject && collider.tag != unit.tag)
                {
                    target.ReceiveDamage(projectileDamageData);
                    OnHitAction();
                }
            }
        }
    }

    /// <summary>
    /// 투사체 위치를 업데이트하는 메인 로직
    /// - 타겟과 시작점으로부터 이동 방향을 계산하고,
    ///   X축 우선/ Y축 우선 판단에 따라 서로 다른 곡선 업데이트 사용
    /// </summary>
    private void UpdateProjectilePosition()
    {
        trajectoryRange = target.position - trajectoryStartPoint;

        // 이동 축 우선 결정: x축 변위보다 y축 변위가 크면 Y 우선 (세로 이동), 아니면 X 우선
        if (Mathf.Abs(trajectoryRange.normalized.x) < Mathf.Abs(trajectoryRange.normalized.y))
        {
            // Y 축이 주된 변화면 moveSpeed sign 조정
            if (trajectoryRange.y < 0)
            {
                moveSpeed = -moveSpeed;
            }

            UpdatePositionWithXCurve();
        }
        else
        {
            // X 축이 주된 변화면 moveSpeed sign 조정
            if (trajectoryRange.x < 0)
            {
                moveSpeed = -moveSpeed;
            }


            UpdatePositionWithYCurve();
        }
    }

    /// <summary>
    /// X축 곡선 기반 위치 업데이트 (주로 세로 이동이 주인 경우 호출됨)
    /// </summary>
    private void UpdatePositionWithXCurve()
    {
        // 다음 Y 위치 계산 (현재 Y에서 속도 * dt)
        float nextPositionY = transform.position.y + moveSpeed * Time.deltaTime;
        float nextPositionYNormalized = (nextPositionY - trajectoryStartPoint.y) / trajectoryRange.y;

        // 애니메이션 커브로 X축 상대 위치 계산
        float nextPositionXNormailized = trajectoryAniamaionCurve.Evaluate(nextPositionYNormalized);
        nextXTrajectoryPosition = nextPositionXNormailized * trajectoryMaxRelativeHeight;

        // 축 보정 커브 (X 보정)
        float nextPositionXCorrectionNormalized = axisCorrectionAnimationCurve.Evaluate(nextPositionYNormalized);
        nextPositionXCorrectionAbsolute = nextPositionXCorrectionNormalized * trajectoryRange.x;

        // 최종 X 좌표 (시작점 + 곡선값 + 보정)
        float nextPositionX = trajectoryStartPoint.x + nextXTrajectoryPosition + nextPositionXCorrectionAbsolute;

        // 방향 보정: 특정 사분면에 대해 값 반전
        if (trajectoryRange.x > 0 && trajectoryRange.y > 0)
        {
            nextXTrajectoryPosition = -nextXTrajectoryPosition;
        }
        if (trajectoryRange.x < 0 && trajectoryRange.y < 0)
        {
            nextXTrajectoryPosition = -nextXTrajectoryPosition;
        }

        Vector3 newPosition = new Vector3(nextPositionX, nextPositionY, 0);

        // 속도 커브에 따라 moveSpeed 업데이트
        CalculateNextSpeed(nextPositionYNormalized);
        projectileMoveDirection = newPosition - transform.position;

        transform.position = newPosition;
    }

    /// <summary>
    /// Y축 곡선 기반 위치 업데이트 (주로 가로 이동이 주인 경우 호출됨)
    /// </summary>
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

    /// <summary>
    /// 속도 커브를 읽어 다음 프레임의 moveSpeed를 계산
    /// </summary>
    private void CalculateNextSpeed(float nextPostionXNormalized)
    {
        float nextMoveSpeedNormailized = speedAnimationCurve.Evaluate(nextPostionXNormalized);

        moveSpeed = nextMoveSpeedNormailized * maxMoveSpeed;
    }

    /// <summary>
    /// 발사체 초기화 (호출자에서 필수로 세팅할 값들)
    /// </summary>
    /// <param name="target">목표 Transform</param>
    /// <param name="maxMoveSpeed">최대 이동 속도</param>
    /// <param name="trajectoryMaxHeight">곡선 높이 비율</param>
    /// <param name="unit">발사한 유닛 참조</param>
    /// <param name="attackController">발사자 UnitAttackController (선택)</param>
    public void InitializeProjectile(Transform target, float maxMoveSpeed, float trajectoryMaxHeight, Unit unit, UnitAttackController attackController = null)
    {
        this.target = target;
        this.maxMoveSpeed = maxMoveSpeed;
        this.unit = unit;
        this.attackController = attackController;

        // x 거리 기준으로 상대 최대 높이 계산
        float xDistanceToTarget = target.position.x - trajectoryStartPoint.x;
        this.trajectoryMaxRelativeHeight = Mathf.Abs(xDistanceToTarget) * trajectoryMaxHeight;
        trajectoryStartPoint = transform.position;

        visual.SetTarget(target);
    }

    /// <summary>
    /// 데미지 데이터 초기화
    /// </summary>
    public void InitializeDamageData(DamageData damageData)
    {
        this.projectileDamageData = damageData;
    }

    /// <summary>
    /// 애니메이션 커브들 초기화
    /// </summary>
    public void InitializeAnimaionCurve(AnimationCurve trajectoyAnimationCure, AnimationCurve axisCorrectionAnimationCurve, AnimationCurve speedAnimationCurve)
    {
        this.trajectoryAniamaionCurve = trajectoyAnimationCure;
        this.axisCorrectionAnimationCurve = axisCorrectionAnimationCurve;
        this.speedAnimationCurve = speedAnimationCurve;
    }

    /// <summary>
    /// 발사자(유닛)의 히트 콜백 등록
    /// </summary>
    public void SetOnHitCallback(Action callback)
    {
        if (callback == null) return;
        OnHitCallback = callback;
    }

    /// <summary>
    /// 내부 히트 콜백 실행
    /// </summary>
    private void OnHitAction()
    {
        OnHitCallback?.Invoke();
    }

    // 접근자 / 유틸리티 메서드들
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

    /// <summary>
    /// 타겟을 동적으로 변경할 때 사용
    /// </summary>
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        visual.SetTarget(newTarget);
        // 필요 시 trajectoryStartPoint 재설정하거나 보정 로직 추가 가능
    }
}
