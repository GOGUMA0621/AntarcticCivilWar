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
            Debug.Log("��ų ��� ����");
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

        if (currentHP <= UnitStats.maxHP * 0.5f)
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
        float hpPercent = currentHP / UnitStats.maxHP;
        int newHpStep = Mathf.FloorToInt(hpPercent / hpThresholdStep);

        if (newHpStep < currentHpStep)
        {
            currentHpStep = newHpStep;
            GoDogFight(); 
        }
    }

    #region ��ų

    protected override bool CanSkill()
    {
        return currentHP <= UnitStats.maxHP * 0.5f;
    }

    protected override void UseSkill()
    {
        ChangeState(new RushBossBattlefieldCrusherState());
    }

    public void AdvanceForward()
    {
        //Debug.Log($"AdvanceForward called. isTriggered: {isTriggered}, coroutine: {coroutine != null}");
        if (isTriggered || coroutine != null) return; // �̹� ��� ���̸� ��ø� ���� ����
        isTriggered = true;
        coroutine = StartCoroutine(DashForward(3f));
    }

    private IEnumerator DashForward(float distance)
    {
        if (isDashRunning) yield break; // �̹� ���� ���̸� ����
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
                    //Debug.Log("��� �Ϸ�");
                    ResumeAnimation();
                });
        }
        finally
        {
            isDashRunning = false;
            isTriggered = false;
            coroutine = null; // �ڷ�ƾ ���� �� null�� ����
        }
    }

    public void StartJump()
    {
        PauseAnimation();
        //Debug.Log($"StartJump called. isTriggered: {isTriggered}");
        if (isTriggered) return; // �̹� ��� ���̸� ��ø� ���� ����
        isTriggered = true;

        coroutine = StartCoroutine(JumpStart());
    }

    private IEnumerator JumpStart()
    {
        Debug.Log($"JumpStart called");
        PauseAnimation();

        StopMovement();

        float jumpHeight = 3f;
        float jumpDuration = 0.7f; // ��� �ð�
        Vector3 target = transform.position + new Vector3(0, 0, -jumpHeight);

        Debug.Log($"Before DOTween: {transform.position.z}");
        transform.DOLocalMoveZ(target.z, jumpDuration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                //Debug.Log(transform.position.z);
                isTriggered = false; // ���⼭�� �ʱ�ȭ
                coroutine = null; // �ڷ�ƾ ���� �� null�� ����
                ResumeAnimation();
            });
        yield return null;
    }
    public void StartFall()
    {
        if (isTriggered || coroutine != null) return; // �̹� ��� ���̸� ��ø� ���� ����
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
            transform.DOLocalMoveZ(target.z, fallDuration) // �ٽ� ���� Z��
                .SetEase(Ease.InQuad).OnComplete(() =>
                { 
                    //Debug.Log("���� �Ϸ�");
                    ResumeAnimation();
                });

            yield return null;
        }
        finally
        {
            isTriggered = false;
            coroutine = null; // �ڷ�ƾ ���� �� null�� ����
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
                // �˹�
                if (hit.TryGetComponent(out UnitController targetController))
                {
                    Vector2 knockDir = ((Vector2)hit.transform.position - center).normalized;
                    targetController.UnitAddForce(knockDir * knockbackForce, ForceMode2D.Impulse);
                }

                // ���� + ���� ȿ��
                target.ReceiveDamage(new DamageData(damage, StatusEffectType.Stun, stunDuration));
            }
        }
    }

    public void RushEnd()
    {
        StartMovement();
        unit.detectTarget.targetToAttack = null;
        unit.detectTarget.SortClosestTarget();
        if (unit.detectTarget.targetToAttack != null)
        {
            RushAttack(new DamageData(100f, StatusEffectType.Stun, 100));
        }
        StartCoroutine(Rush()); // ����Ʈ ��ȯ
    }

    private IEnumerator Rush()
    {
        yield return new WaitForSeconds(0.3f);
        InstantiateServant(4); // ����Ʈ ��ȯ
    }
    
    public void DogFightStart()
    {
        if (isTriggered || coroutine != null) return; // �̹� ��� ���̸� ��ø� ���� ����
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
                Debug.Log("��� ���� �Ϸ�");
                isTriggered = false;
                coroutine = null; // �ڷ�ƾ ���� �� null�� ����
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
        if (isTriggered || coroutine != null) return; // �̹� ��� ���̸� ��ø� ���� ����
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
                Debug.Log("��� �Ϸ�");
                isTriggered = false;
                coroutine = null; // �ڷ�ƾ ���� �� null�� ����
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
        DestroyEvent(this);
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
            servant.GetComponent<UnitController>().AddModifierStat(new StatModifier("�뽺��", StatType.AttackDamage, 1.3f, ModifierMethod.Multiplicative));
            servant.GetComponent<UnitController>().AddModifierStat(new StatModifier("�뽺��", StatType.MoveSpeed, 1.5f, ModifierMethod.Multiplicative));
        }
    }

    public void MeleeAttack(float damage)
    {
        DamageData damageData = new DamageData(damage, StatusEffectType.Physical, 0);
        Transform targetTransform = null;
        if (unit.detectTarget.targetToAttack is Component comp)
            targetTransform = comp.transform;
        Collider2D[] collider = Physics2D.OverlapCircleAll(transform.position, unit.data.UnitAttackDistance + 1f);
        foreach (Collider2D targetCollider in collider)
        {
            if (targetCollider.transform == targetTransform)
            {
                IDamageAble target = targetCollider.GetComponent<IDamageAble>();
                target.ReceiveDamage(damageData);
            }
        }
    }

    public void RushAttack(DamageData damageData)
    {
        Transform targetTransform = null;
        if (unit.detectTarget.targetToAttack is Component comp)
            targetTransform = comp.transform;
        Collider2D[] collider = Physics2D.OverlapCircleAll(transform.position, unit.data.UnitAttackDistance + 1f);
        foreach (Collider2D targetCollider in collider)
        {

            if (targetCollider.transform == targetTransform)
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
