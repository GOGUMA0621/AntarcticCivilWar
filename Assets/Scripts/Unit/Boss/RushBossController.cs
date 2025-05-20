using DG.Tweening;
using System.Collections;
using UnityEngine;

public class RushBossController : BossController
{
    private bool isSkillActive = false;
    private bool isTriggered = false;

    private Vector2 currentDirection = Vector2.zero;

    private bool isDashRunning = false;
    private Coroutine coroutine;

    private float hpThresholdStep = 0.2f;
    private int currentHpStep = 5;

    [SerializeField] private GameObject servantPrefab;
    private Tween tween;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        isSkillActive = false;
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        if (CanSkill() && !isSkillActive)
        {
            Debug.Log("스킬 사용 가능");
            isSkillActive = true;
            UseSkill();
        }
    }

    #region FSM
    private readonly IUnitState idleState = new RushBossIdleState();
    private readonly IUnitState dogFightState = new RushBossDogFightState();
    private readonly IUnitState battlefieldCrusherState = new RushBossBattlefieldCrusherState();
    private readonly IUnitState attackState = new RushBossAttackState();
    private readonly IUnitState followState = new RushBossFollowState();
    private readonly IUnitState dieState = new RushBossDieState();


    protected override IUnitState GetManaSkillState()
    {
        return new RushBossManaSkillState();
    }

    public override void GoIdle()=> ChangeState(idleState);
    public override void GoAttack() => ChangeState(attackState);
    public override void GoFollow() => ChangeState(followState);
    public override void GoDie() => ChangeState(dieState);
    public void GoDogFight() => ChangeState(dogFightState);
    public void GoBattlefieldCrusher() => ChangeState(battlefieldCrusherState);


    #endregion

    public override void ReceiveDamage(DamageData damage)
    {
        float damageBase = damage.damage;
        float damageAmount = damageBase * 0.9f;

        if(currentHP <= 0f)
        {
            Die();
        }

        if (currentHP <= maxHP * 0.5f)
        {
            float reducedDamage = damageBase * 0.8f;
            damage.damage = reducedDamage;
            base.ReceiveDamage(damage);
        }
        else
        {
            damage.damage = damageAmount;
            base.ReceiveDamage(damage);
        }
        float hpPercent = currentHP / maxHP;
        int newHpStep = Mathf.FloorToInt(hpPercent / hpThresholdStep);

        if (newHpStep < currentHpStep)
        {
            currentHpStep = newHpStep;
            GoDogFight(); 
        }
    }

    #region 스킬

    protected override bool CanSkill()
    {
        return currentHP <= maxHP * 0.5f;
    }

    protected override void UseSkill()
    {
        ChangeState(new RushBossBattlefieldCrusherState());
    }

    public void AdvanceForward()
    {
        //Debug.Log($"AdvanceForward called. isTriggered: {isTriggered}, coroutine: {coroutine != null}");
        if (isTriggered || coroutine != null) return; // 이미 대시 중이면 대시를 하지 않음
        isTriggered = true;
        coroutine = StartCoroutine(DashForward(3f));
    }

    private IEnumerator DashForward(float distance)
    {
        if (isDashRunning) yield break; // 이미 실행 중이면 종료
        isDashRunning = true;

        try
        {
            PauseAnimation();
            StopMovement();

            float dashDuration = 0.2f;

            Vector3 startPos = transform.position;
            Vector3 direction = transform.localScale.x > 0 ? Vector3.right : Vector3.left;
            Vector3 targetPos = startPos + direction * distance;

            transform.DOMove(targetPos, dashDuration)
                .SetEase(Ease.Linear)
                .OnComplete(() =>
                {
                    //Debug.Log("대시 완료");
                    ResumeAnimation();
                });
        }
        finally
        {
            isDashRunning = false;
            isTriggered = false;
            coroutine = null; // 코루틴 종료 후 null로 설정
        }
    }

    public void StartJump()
    {
        PauseAnimation();
        //Debug.Log($"StartJump called. isTriggered: {isTriggered}");
        if (isTriggered) return; // 이미 대시 중이면 대시를 하지 않음
        isTriggered = true;

        coroutine = StartCoroutine(JumpStart());
    }

    private IEnumerator JumpStart()
    {
        Debug.Log($"JumpStart called");
        PauseAnimation();

        StopMovement();

        float jumpHeight = 3f;
        float jumpDuration = 0.7f; // 상승 시간
        Vector3 target = transform.position + new Vector3(0, 0, -jumpHeight);

        Debug.Log($"Before DOTween: {transform.position.z}");
        transform.DOLocalMoveZ(target.z, jumpDuration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                //Debug.Log(transform.position.z);
                isTriggered = false; // 여기서만 초기화
                coroutine = null; // 코루틴 종료 후 null로 설정
                ResumeAnimation();
            });
        yield return null;
    }
    public void StartFall()
    {
        if (isTriggered || coroutine != null) return; // 이미 대시 중이면 대시를 하지 않음
        isTriggered = true;

        coroutine = StartCoroutine(FallStart());
    }

    private IEnumerator FallStart()
    {
        try
        {
            PauseAnimation();
            

            float fallDuration = 0.7f;

            Vector3 target = new Vector3(transform.position.x, transform.position.y, 0f);
            transform.DOLocalMoveZ(target.z, fallDuration) // 다시 원래 Z로
                .SetEase(Ease.InQuad).OnComplete(() =>
                { 
                    //Debug.Log("점프 완료");
                    ResumeAnimation();
                });

            yield return null;
        }
        finally
        {
            isTriggered = false;
            coroutine = null; // 코루틴 종료 후 null로 설정
        }
    }

    public void SmashImpact()
    {
        Vector2 center = transform.position;
        float radius = 4f;
        float knockbackForce = 2f;
        float stunDuration = 3f;
        float damage = 100f;

        Collider2D[] hits = Physics2D.OverlapCircleAll(center, radius);

        foreach (var hit in hits)
        {
            if (hit.TryGetComponent(out IDamageAble target) && hit.gameObject.tag != gameObject.tag)
            {
                // 넉백
                if (hit.TryGetComponent(out UnitController targetController))
                {
                    Vector2 knockDir = ((Vector2)hit.transform.position - center).normalized;
                    targetController.UnitAddForce(knockDir * knockbackForce, ForceMode2D.Impulse);
                }

                // 피해 + 스턴 효과
                target.ReceiveDamage(new DamageData(damage, StatusEffectType.Stun, stunDuration));
            }
        }
    }

    public void RushEnd()
    {
        StartMovement();
        unit.detectTarget.targetToAttack = null;
        unit.detectTarget.SortClosetTarget();
        if (unit.detectTarget.targetToAttack != null)
        {
            RushAttack(new DamageData(100f, StatusEffectType.Stun, 100));
        }
        StartCoroutine(Rush()); // 서번트 소환
    }

    private IEnumerator Rush()
    {
        yield return new WaitForSeconds(0.3f);
        InstantiateServant(4); // 서번트 소환
    }
    
    public void DogFightStart()
    {
        if (isTriggered || coroutine != null) return; // 이미 대시 중이면 대시를 하지 않음
        isTriggered = true;
        PauseAnimation();
        coroutine = StartCoroutine(DogFightStartCoroutine(4f));
    }

    private IEnumerator DogFightStartCoroutine(float distance)
    {
        
        StopMovement();
        isTriggered = true;

        float dashDuration = 0.4f;

        Vector3 startPos = transform.position;
        Vector3 direction = transform.localScale.x > 0 ? Vector3.right : Vector3.left;
        currentDirection = direction;
        Vector3 targetPos = startPos + direction * distance;

        transform.DOMove(targetPos, dashDuration)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                Debug.Log("대시 시작 완료");
                isTriggered = false;
                coroutine = null; // 코루틴 종료 후 null로 설정
                //Debug.Log(isTriggered);
                //SetUntarget();
                InstantiateServant(5);
            });
        yield return new WaitForSeconds(0.5f);
        ResumeAnimation();

        //yield return new WaitForSeconds(2f);
        //isUnitDie = false;
    }

    public void DogFightEnd()
    {
        if (isTriggered || coroutine != null) return; // 이미 대시 중이면 대시를 하지 않음
        Debug.Log("DogFightEnd");
        isTriggered = true;

        coroutine = StartCoroutine(DashBackword(4f));
    }

    private IEnumerator DashBackword(float distance)
    {
        PauseAnimation();
        StopMovement();
        isTriggered = true;
        disableFlip = true;

        float dashDuration = 0.4f;

        Vector3 startPos = transform.position;
        Vector3 direction = currentDirection.x > 0 ? Vector3.left : Vector3.right;
        Vector3 targetPos = startPos + direction * distance;
            
        Debug.Log($"DashBackword called. targetPos: {targetPos}");
        transform.DOMove(targetPos, dashDuration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                Debug.Log("대시 완료");
                isTriggered = false;
                coroutine = null; // 코루틴 종료 후 null로 설정
                disableFlip = false;
                ResumeAnimation();
                unit.DOFlip();
                currentDirection = Vector2.zero;
            });

        yield return null;
    }

    #endregion

    private void SetUntarget()
    {
        DestroyEvent(this.gameObject);
        isUnitDie = true;
    }

    private void InstantiateServant(int count)
    {
        StartCoroutine(SpawnServant(count));
    }

    private IEnumerator SpawnServant(int count)
    {
        for (int i = 0; i < count; i++)
        {
            Vector3 spawnPosition = transform.position + new Vector3(Random.Range(-2f, 2f), Random.Range(-2f, 2f), 0);
            GameObject servant = Instantiate(servantPrefab, spawnPosition, Quaternion.identity);
            servant.tag = "Royal";
            servant.GetComponent<Unit>().originPrefab = servantPrefab;
            yield return new WaitForFixedUpdate();
            servant.GetComponent<UnitController>().AddModifierStat(new StatModifier("펭스토", StatType.AttackDamage, 1.3f, ModifierMethod.Multiplicative));
            servant.GetComponent<UnitController>().AddModifierStat(new StatModifier("펭스토", StatType.MoveSpeed, 1.5f, ModifierMethod.Multiplicative));
        }
    }

    public void MeleeAttack(float damage)
    {
        DamageData damageData = new DamageData(damage, StatusEffectType.None, 0);

        Collider2D[] collider = Physics2D.OverlapCircleAll(transform.position, unit.data.UnitSenseRadius);
        foreach (Collider2D targetCollider in collider)
        {
            if (targetCollider.transform == unit.detectTarget.targetToAttack)
            {
                IDamageAble target = targetCollider.GetComponent<IDamageAble>();
                target.ReceiveDamage(damageData);
            }
        }
    }

    public void RushAttack(DamageData damageData)
    {
        Collider2D[] collider = Physics2D.OverlapCircleAll(transform.position, unit.data.UnitSenseRadius);
        foreach (Collider2D targetCollider in collider)
        {
            if (targetCollider.transform == unit.detectTarget.targetToAttack)
            {
                if(targetCollider.TryGetComponent<IDamageAble>(out IDamageAble i)&& i is MonoBehaviour target)
                {
                    if(target.TryGetComponent<UnitController>(out UnitController unitController))
                    {
                        Vector2 knockDir = ((Vector2)target.transform.position - (Vector2)transform.position).normalized;
                        unitController.UnitAddForce(knockDir * 3f, ForceMode2D.Impulse);
                    }
                    i.ReceiveDamage(damageData);
                }
            }
        }
    }
}
