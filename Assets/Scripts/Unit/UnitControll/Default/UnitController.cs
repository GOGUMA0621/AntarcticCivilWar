using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


[Serializable]
public class DamageData
{
    public float damage;
    public StatusEffectType effectType;
    public float buildAmount;

    public DamageData(float damage, StatusEffectType effectType, float buildAmount)
    {
        this.damage = damage;
        this.effectType = effectType;
        this.buildAmount = buildAmount;
    }
}

public class UnitController : Unit,IStatusAble,IDamageAble //유닛의 전반적인 컨트롤
{
    public event Action<GameObject> OnDestroyed;

    private Dictionary<StatType, float> baseStats = new();
    private List<StatModifier> statModifierList = new();
    private Dictionary<StatType, float> finalStats = new();

    public delegate void UnitAttackCountEvent();

    public static event UnitAttackCountEvent OnUnitAttackCount;

    private StatusEffectManager statusEffectManager;

    private Unit _unit;
    private Vector2 _lastPosition; //애니메이션 좌우 반전을 위한 변수
    private bool _isFacingRight = true; 
    private SciptableObjects.UnitData _currentData; //유닛의 데이터 변화 감지를 위한 변수
    private bool isUnitDie;
    private Transform _lastAttacker; //넉백을 위해 마지막 공격자를 알아내는 변수
    public IActiveSkill unitSkill;
    public IPasseiveSkillAttack unitPassiveSkill;
    public bool canMana = true;

    [HideInInspector] public bool isStunned = false;
     public float maxHP;
    [HideInInspector] public float currentHP { get; private set; }
     public float maxMP;
    [HideInInspector] public float currentMP { get; private set; }
     public float unitDamage;
     public float unitSpeed;
    [HideInInspector] public float unitAttackDistance;

    private float unitAttackSpeed = 1.0f;
    private float unitSenseDistance = 1.0f;

    #region 이벤트 관리
    private void OnEnable()
    {
        UnitAttackController.OnUnitAttack += HandleAttackEvent;
    }

    private void OnDisable()
    {
        UnitAttackController.OnUnitAttack -= HandleAttackEvent;
    }

    private void HandleAttackEvent(Transform tr)
    {
        //Debug.Log(tr);
        _lastAttacker = tr;
    }
    #endregion

    protected override void Start()
    {
        isUnitDie = false;
        base.Start();
        unitPassiveSkill = GetComponent<IPasseiveSkillAttack>();
        _unit = GetComponent<Unit>();
        data = _unit.data;
        statusEffectManager = GetComponent<StatusEffectManager>();
        _lastPosition = transform.position;
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        SetUnit();
    }

    void Update()
    {
        animator.SetFloat("speed", agent.velocity.magnitude);
        
    }

    private void FixedUpdate()
    {
        if (_currentData != data)
        {
            SetUnit();
        }
        FlipAnimation();
    }
    #region 기본셋업
    private void SetUnit()
    {
        if (data != null)
        {

            baseStats.Add(StatType.MaxHealth, data.UnitHP);
            baseStats.Add(StatType.HealthRegen, 0);
            baseStats.Add(StatType.MaxMana, data.UnitMP);
            baseStats.Add(StatType.ManaRegen, 5);
            baseStats.Add(StatType.AttackDamage, data.UnitDamage);
            baseStats.Add(StatType.AttackSpeed, data.UnitSpeed);
            baseStats.Add(StatType.AttackRange, data.UnitAttackDistance);
            baseStats.Add(StatType.MoveSpeed, data.UnitSpeed);
            baseStats.Add(StatType.CritChance, 0);

            RecalculateStats();

            currentHP = maxHP;
            currentMP = 0;
            unitSenseDistance = data.UnitSenseRadius;
            

            if (PlayerUnitManager.Instance.allayList != null && tag == "Unit")
            {
                PlayerUnitManager.Instance.AddAllayList(this.gameObject);
            }

            if (rb != null)
            {
                rb.drag = 1.0f;
            }

            _currentData = data;
        }
    }
    #endregion

    #region 이동관련
    public void MoveTo(Vector2 targetPos)
    {
        agent.SetDestination(targetPos);
    }
    #endregion

    #region 애니메이션 반전
    public void FlipAnimation()
    {
        if (!isUnitDie)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            Vector2 currentPosition = transform.position;
            if (stateInfo.IsTag("Battle"))
            {
                if (detectTarget.targetToAttack != null)
                {
                    float targetDirection = detectTarget.targetToAttack.transform.position.x - currentPosition.x;

                    if (targetDirection > 0 && !_isFacingRight)
                    {
                        Flip();
                    }
                    else if (targetDirection < 0 && _isFacingRight)
                    {
                        Flip();
                    }
                }
            }
            else
            {
                float moveDirection = currentPosition.x - _lastPosition.x;

                if (moveDirection > 0 && !_isFacingRight)
                {
                    Flip();
                }
                else if (moveDirection < 0 && _isFacingRight)
                {
                    Flip();
                }

                _lastPosition = currentPosition;
            }
        }
    }

    void Flip()
    {
        Vector2 curentScale = gameObject.transform.localScale;
        curentScale *= new Vector2(-1, 1);
        gameObject.transform.localScale = curentScale;

        _isFacingRight = !_isFacingRight;
    }
    #endregion

    #region 전투 관련
    public void UnitAttack()
    {
        CollectMana();
        DoSkill();
        OnUnitAttackCount?.Invoke();

        if (unitPassiveSkill != null)
        {
            if (unitPassiveSkill.PassiveCondition())
            {
                unitPassiveSkill.DoPassiveSkill();
            }
            else
            {
                attackController.Attack();
            }
        }
        else
        {
            attackController.Attack();
        }
    }

    public void ReceiveDamage(DamageData damage)
    {
        UnitAttackController.OnUnitAttack += HandleAttackEvent;
        currentHP -= damage.damage;
        ApplyEffect(damage);
        CollectMana();
        currentHP = Mathf.Max(currentHP, 0);
        DoSkill();
        UnitAttackController.OnUnitAttack -= HandleAttackEvent;
        if (currentHP <= 0 && !isUnitDie) Death();
    }

    internal void Death()
    {
        isUnitDie = true;
        OnDestroyed?.Invoke(this.gameObject);

        animator.SetBool("isDie", true);
        StartCoroutine(KnockBack(2.0f));
        agent.enabled = false;
        detectTarget.ClearTarget();

        if (this.transform.tag == "Unit")
        {
            PlayerUnitManager.Instance.RemoveAllayList(this.gameObject);
        }
    }

    internal void Revive()
    {
        this.tag = "Unit";
        rb.velocity = Vector2.zero;
        agent.enabled = true;
        detectTarget.ClearTarget();
        animator.SetBool("isDie", false);
        isUnitDie = false;
        currentHP = maxHP;
        animator.Play("IdleState");
        PlayerUnitManager.Instance.AddAllayList(GetUnit().gameObject);
    }

    public void ApplyEffect(DamageData damage)
    {
        if (damage.effectType == StatusEffectType.None)
        {
            return;
        }
        else if(!isUnitDie)
        {
            statusEffectManager.OnStatusTriggerBuildup(damage.effectType, damage.buildAmount);
        }
    }

    void CollectMana()
    {
        if (canMana)
        {
            currentMP += finalStats[StatType.ManaRegen];
        }
    }

    public void DoSkill()
    {
        if (currentMP >= maxMP && maxMP != 0) 
        {
            animator.Play("ManaSkill");
            Debug.Log("마나");
            currentMP = 0;
        }
    }

    public void ResetUnit()
    {
        if (data != null)
        {
            maxHP = data.UnitHP;
            maxMP = data.UnitMP;
            unitDamage = data.UnitDamage;
            unitSpeed = data.UnitSpeed;
            unitAttackDistance = data.UnitAttackDistance;
            unitAttackSpeed = data.UnitAttackSpeed;
            unitSenseDistance = data.UnitSenseRadius;
            agent.speed = data.UnitSpeed;
        }
    }

    IEnumerator KnockBack(float amount)
    {
        if (!data.UnitUnstoppable)
        {
            if (amount >= 0)
            {
                agent.enabled = false;
                Vector2 direction = (this.transform.position - _lastAttacker.position).normalized;
                rb.AddForce(direction * amount, ForceMode2D.Impulse);
            }
            yield return new WaitForSeconds(0.5f);
            rb.velocity = Vector2.zero;
            agent.enabled = true;
        }
    }
    #endregion

    #region 스탯
    public void AddModifierStat(StatModifier mod)
    {
        statModifierList.RemoveAll(m => m.sourceId == mod.sourceId && m.statType == mod.statType);
        statModifierList.Add(mod);
        RecalculateStats();
    }

    public void RecalculateModifier(string sourceId)
    {
        statModifierList.RemoveAll(m => m.sourceId == sourceId);
        RecalculateStats();
    }

    private void RecalculateStats()
    {
        finalStats.Clear();

        foreach (var stat in baseStats)
        {
            float add = 0f;
            float multiple = 1f;

            foreach(var mod in statModifierList.Where(m => m.statType == stat.Key))
            {
                if (mod.modifierMethod == ModifierMethod.Additive) 
                    add += mod.value;
                else if(mod.modifierMethod == ModifierMethod.Multiplicative)
                    multiple *= mod.value;
            }
            finalStats[stat.Key] = (stat.Value + add) * multiple;
        }

        maxHP = finalStats[StatType.MaxHealth];
        maxMP = finalStats[StatType.MaxMana];
        unitDamage = finalStats[StatType.AttackDamage];
        unitAttackDistance = finalStats[StatType.AttackRange];
        unitAttackSpeed = finalStats[StatType.AttackSpeed];
        agent.speed = finalStats[StatType.MoveSpeed];
    }

    #endregion

    private void OnDrawGizmos() //디버그용 기즈모
    {
        if (GetUnit() != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(this.transform.position, unitSenseDistance);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(this.transform.position, unitAttackDistance);
        }
    }

    public bool GetIsStunned()
    {
        return isStunned;
    }

    public bool IsDestroyed()
    {
        return isUnitDie;
    }
}
