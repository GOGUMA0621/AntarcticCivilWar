using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
/// <summary>
/// 공격 형식을 저장하는 클래스입니다.
/// 공격 시 데미지와 상태이상을 저장합니다.
/// </summary>
[Serializable]
public class DamageData
{
    /// <summary>
    /// 공격 데미지
    /// </summary>
    public float damage;
    /// <summary>
    /// 상태이상 타입
    /// </summary>
    public StatusEffectType effectType;
    /// <summary>
    /// 상태이상 축적량
    /// </summary>
    public float buildAmount;
    /// <summary>
    /// DamageData 생성자입니다.
    /// 공격시 가하는 피해량과 상태이상 타입, 상태이상 축적량을 저장합니다.
    /// </summary>
    /// <param name="damage">가하는 피해량 입니다.</param>
    /// <param name="effectType">적용 할 상태이상 타입입니다.</param>
    /// <param name="buildAmount">상태이상 축적량 입니다.</param>
    public DamageData(float damage, StatusEffectType effectType, float buildAmount)
    {
        this.damage = damage;
        this.effectType = effectType;
        this.buildAmount = buildAmount;
    }
}
/// <summary>
/// 유닛의 전반적인 컨트롤을 담당하는 클래스입니다.
/// 유닛의 이동, 공격, 상태이상 적용, 스탯 계산 등을 담당합니다.
/// </summary>
public class UnitController : MonoBehaviour, IStatusAble, IDamageAble //유닛의 전반적인 컨트롤
{
    /// <summary>
    /// 유닛이 파괴될 때 호출되는 이벤트입니다.
    /// </summary>
    public event Action<IDamageAble> OnDestroyed;

    /// <summary>
    /// 유닛의 스탯을 저장하는 딕셔너리입니다.
    /// 스탯의 종류에 따라 스탯을 저장합니다.
    /// 초기 저장할 스탯을 저장합니다.
    /// </summary>
    private Dictionary<StatType, float> baseStats = new();

    /// <summary>
    /// 유닛의 스탯에 영향을 주는 모든 스탯 모디파이어를 저장하는 리스트입니다.
    /// 스탯 모디파이어는 스탯을 변경하는 모든 요소를 저장합니다.
    /// </summary>
    private List<StatModifier> statModifierList = new();
    /// <summary>
    /// 유닛의 최종 스탯을 저장하는 딕셔너리입니다.
    /// 스탯의 종류에 따라 최종 스탯을 저장합니다.
    /// 최종 스탯은 기본 스탯과 모든 스탯 모디파이어를 적용한 스탯입니다.
    /// </summary>
    private Dictionary<StatType, float> finalStats = new();

    /// <summary>
    /// 유닛이 공격시 적용할 아이템을 저장하는 리스트입니다.
    /// 아이템은 유닛이 공격할 때 적용되는 효과를 저장합니다.
    /// </summary>
    private List<OnHitItem> onHitItemList = new();

    /// <summary>
    /// 유닛이 공격할 때 이 이벤트가 호출됩니다.
    /// </summary>
    public event Action OnHit;

    /// <summary>
    /// 유닛이 공격당할 때 이 이벤트가 호출됩니다.
    /// </summary>
    public event Action WhenHit;

    private StatusEffectManager statusEffectManager;

    public Unit unit;
    /// <summary>
    /// 애니메이션 좌우 반전을 위한 변수입니다.
    /// </summary>
    private Vector2 _lastPosition;
 
    private Coroutine followCoroutine; //유닛의 이동을 위한 코루틴
    /// <summary>
    /// 유닛의 이동 목표까지 남은 거리
    /// </summary>
    public int RemainedDistance => unit.mover.GetRemainingTileDistanceToTarget();


    //private bool inCombat = false; //유닛이 전투중인지 확인하기 위한 변수

    private bool _isFacingRight = true; //유닛이 바라보는 방향을 저장하기 위한 변수

    protected bool isUnitDie;

    protected bool disableFlip = false; //애니메이션 좌우 반전을 막기 위한 변수

    private Transform lastAttacker; //넉백을 위해 마지막 공격자를 알아내는 변수
    public IActiveSkill unitSkill;
    public IPasseiveSkillAttack unitPassiveSkill;
    public bool canMana = true;


    private string currentAnimationName;
    private bool isPaused = false;
    private int pausedStateHash;
    private float pausedTime;

    public bool isStunned = false;

    [HideInInspector] public UnitStats UnitStats => new UnitStats(finalStats);
    [SerializeField] private UnitStats baseUnitStats;
    [SerializeField] public float currentHP;

    public float currentMP { get; private set; }

    public bool isAllay = true;

    public int unitLevel = 1;

    public float unitAttackSpeed = 1.0f;
    protected IUnitState currentState;

    #region 이벤트 관리

    public void DestroyEvent(IDamageAble damageAble)
    {
        OnDestroyed?.Invoke(damageAble);
    }
    #endregion

    private void OnEnable()
    {
        foreach (var synergy in GetComponents<ISynergy>())
        {
            synergy?.Initialize(this);
        }
        SetUnit();
    }
    protected virtual void Start()
    {
        isUnitDie = false;
        unitPassiveSkill = GetComponent<IPasseiveSkillAttack>();
        unit.rb.isKinematic = true;
        unit.rb.gravityScale = 0f;
        unit.rb.velocity = Vector2.zero;
        statusEffectManager = GetComponent<StatusEffectManager>();
        _lastPosition = transform.position;
        unitSkill = GetComponent<IActiveSkill>();
        GoPlace();
    }

    private void Update()
    {
        unit.animator.SetFloat("speed", unit.rb.velocity.magnitude);
        currentState?.Update();

    }

    private void FixedUpdate()
    {
        //Debug.Log($"{name} - FixedUpdate velocity: {_unit.rb.velocity}");
        if (currentState != placeState)
        {
            FlipAnimation();
        }
    }

    #region FSM
    private readonly IUnitState placeState = new UnitPlaceState(); 
    private readonly IUnitState idleState = new UnitIdleState();
    private readonly IUnitState attackState = new UnitAttackState();
    private readonly IUnitState followState = new UnitFollowState();
    private readonly IUnitState dieState = new UnitDieState();
    private readonly IUnitState manaSkillState = new UnitManaSkillState();

    public virtual void GoPlace() => ChangeState(placeState);
    public virtual void GoIdle() => ChangeState(idleState);
    public virtual void GoAttack() => ChangeState(attackState);
    public virtual void GoFollow() => ChangeState(followState);
    public virtual void GoDie() => ChangeState(dieState);
    public virtual void GoSkill(bool isStanding = false, float duration = 0f) => ChangeState(manaSkillState, isStanding, duration);

    public void ChangeState(IUnitState newState, bool force = false, float duration = 0f)
    {
        if (currentState?.GetType() == newState.GetType())
        {
            return;
        }

        currentState?.Exit();
        currentState = newState;
        currentState.Enter(this);
    }

    public IUnitState GetCurrentState()
    {
        return currentState;
    }

    protected virtual IUnitState GetManaSkillState()
    {
        return new UnitManaSkillState();
    }

    #endregion

    #region 기본셋업
    /// <summary>
    /// 유닛의 기본 스탯을 설정하는 메서드
    /// </summary>
    public void SetUnit()
    {
        if (unit.data != null)
        {
            int idx = Mathf.Clamp(unitLevel - 1, 0, unit.data.UnitHP.Length - 1);
            baseStats.Clear();
            baseStats.Add(StatType.MaxHealth, unit.data.UnitHP[idx]);
            baseStats.Add(StatType.HealthRegen, 0);
            baseStats.Add(StatType.MaxMana, unit.data.UnitMP);
            baseStats.Add(StatType.ManaRegen, 5);
            baseStats.Add(StatType.AttackDamage, unit.data.UnitDamage[idx]);
            baseStats.Add(StatType.AttackSpeed, unit.data.UnitAttackSpeed);
            baseStats.Add(StatType.AttackRange, unit.data.UnitAttackDistance);
            baseStats.Add(StatType.MoveSpeed, unit.data.UnitSpeed);
            baseStats.Add(StatType.CritChance, 0);
            baseStats.Add(StatType.CritDamage, 1.3f);
            baseStats.Add(StatType.Endurance, 0);
            baseStats.Add(StatType.DamageAmp, 0);

            baseUnitStats = new UnitStats(baseStats);

            RecalculateStats();

            currentHP = UnitStats.maxHP;
            currentMP = 0;
        }
    }

    #endregion

    #region 이동
    /// <summary>
    /// 유닛의 이동 목표를 설정하는 메서드
    /// </summary>
    /// <param name="target"></param>
    public void SetTargetToMove(Transform target, Action onComplete = null) => unit.mover.FollowTarget(target, onComplete);

    /// <summary>
    /// 유닛의 이동을 멈추는 메서드
    /// </summary>
    public void StopMovement()
    {
        unit.mover.SetCanMove(false);
    }

    public void OnPathCompleteToAttack()
    {
        if(currentState is UnitFollowState)
        {
          GoAttack();
        }
    }

    /// <summary>
    /// 유닛의 이동을 가능하게 하는 메서드
    /// </summary>
    public void StartMovement()
    {
        unit.mover.SetCanMove(true);
    }
    #endregion

    #region 애니메이션
    /// <summary>
    /// 유닛의 애니메이션을 판단하여 좌우 반전시키는 메서드
    /// </summary>
    public void FlipAnimation()
    {
        if (isUnitDie || disableFlip) return;

        AnimatorStateInfo stateInfo = unit.animator.GetCurrentAnimatorStateInfo(0);
        Vector2 currentPosition = transform.position;
        Transform targetTransform = null;
        if(unit.detectTarget.targetToAttack != null)
        {
            IDamageAble target = unit.detectTarget.targetToAttack;
            if (target is Component comp)
                targetTransform = comp.transform;
        }
        if (stateInfo.IsTag("Battle"))
        {
            if (unit.detectTarget.targetToAttack != null)
            {
                float targetDirection = targetTransform.position.x - currentPosition.x;

                if (targetDirection > 0)
                {
                    unit.spriteRenderer.flipX = false;
                }
                else if (targetDirection < 0)
                {
                    unit.spriteRenderer.flipX = true;
                }
            }
        }
        else
        {
            float moveDirection = currentPosition.x - _lastPosition.x;

            if (moveDirection > 0)
            {
                unit.spriteRenderer.flipX = false;
            }
            else if (moveDirection < 0)
            {
                unit.spriteRenderer.flipX = true;
            }

            _lastPosition = currentPosition;
        }
    }
    /// <summary>
    /// 유닛의 애니메이션을 좌우 반전시크는 메서드
    /// </summary>
    protected void Flip()
    {
        Vector2 curentScale = gameObject.transform.localScale;
        curentScale *= new Vector2(-1, 1);
        gameObject.transform.localScale = curentScale;

        _isFacingRight = !_isFacingRight;
    }

    /// <summary>
    /// 유닛의 애니메이션을 설정하는 메서드
    /// </summary>
    /// <param name="animationName"></param>
    public void SetAnimation(string animationName)
    {
        if(currentAnimationName == animationName)
        {
            return;
        }

        unit.animator.Play(animationName);
        currentAnimationName = animationName;
    }
    /// <summary>
    /// 유닛의 애니메이션을 일시정지하는 메서드
    /// </summary>
    public void PauseAnimation()
    {
        if (!isPaused)
        {
            AnimatorStateInfo stateInfo = unit.animator.GetCurrentAnimatorStateInfo(0);
            pausedStateHash = stateInfo.shortNameHash;
            pausedTime = stateInfo.normalizedTime + Time.deltaTime;
            unit.animator.speed = 0;
            isPaused = true;
            Debug.Log($"[애니메이션 일시정지]");
        }
    }
    /// <summary>
    /// 유닛의 애니메이션을 재개하는 메서드
    /// </summary>
    public void ResumeAnimation()
    {
        StartCoroutine(ResumeCoroutine());
    }
    /// <summary>
    /// 유닛의 애니메이션을 재개하는 코루틴
    /// </summary>
    /// <returns></returns>
    private IEnumerator ResumeCoroutine()
    {
        if(isPaused)
        {
            
            unit.animator.speed = 1;
            unit.animator.Play(pausedStateHash, 0, pausedTime);
            yield return new WaitUntil(() => 
            {
                var stateInfo = unit.animator.GetCurrentAnimatorStateInfo(0);
                return unit.animator.speed > 0f && stateInfo.normalizedTime != pausedTime;
            });
            isPaused = false;
            Debug.Log($"[애니메이션 재개] {name} {pausedStateHash} {pausedTime}");
        }
    }

    #endregion

    #region 전투
    /// <summary>
    /// 유닛의 공격을 수행하는 메서드
    /// </summary>
    public void UnitAttack()
    {
        CollectMana();
        DoSkill();
        OnHit?.Invoke();

        if (unitPassiveSkill != null)
        {
            if (unitPassiveSkill.PassiveCondition())
            {
                unitPassiveSkill.DoPassiveSkill();
            }
            else
            {
                //Debug.Log(name);
                unit.attackController.Attack();
            }
        }
        else
        {
            //Debug.Log(name);
            unit.attackController.Attack();
        }
    }
    /// <summary>
    /// 유닛 자신의 체력을 회복하는 메서드
    /// </summary>
    /// <param name="amount"></param>
    public virtual void Heal(float amount)
    {
        currentHP = Math.Clamp(currentHP += amount, 0f, UnitStats.maxHP);
    }

    /// <summary>
    /// 유닛 자신에게 피해를 입히는 메서드
    /// </summary>
    /// <param name="damage"></param>
    public virtual void ReceiveDamage(DamageData damage)
    {
        float endurance = finalStats[StatType.Endurance] / 100f;
        float damageRecution = Mathf.Clamp(endurance,0f,0.75f);
        float reducedDamage = damage.damage * (1 - damageRecution);
        
        currentHP -= reducedDamage;
        ApplyEffect(damage);
        CollectMana();
        DoSkill();
        WhenHit?.Invoke();
        if (currentHP <= 0 && !isUnitDie) Die();
    }

    /// <summary>
    /// 유닛이 죽었을 때 호출되는 메서드
    /// </summary>
    internal void Die()
    {
        StopMovement();
        isUnitDie = true;
        OnDestroyed?.Invoke(this);

        GoDie();
        // StartCoroutine(KnockBack(2.0f));
        unit.detectTarget.ClearTarget();
        unit.capsuleCollider.enabled = false;

        if (this.tag != "Unit")
        {
            UnitManager.instance.AddUnitToRevive(this);

        }
        if (this.transform.tag == "Unit")
        {
            UnitManager.instance.RemoveAllayList(this);
        }
    }

    /// <summary>
    /// 유닛이 부활할 때 호출되는 메서드
    /// </summary>
    internal void Revive() //일어나라
    {
        this.tag = "Unit";
        unit.capsuleCollider.enabled = true;
        unit.rb.velocity = Vector2.zero;
        unit.detectTarget.ClearTarget();
        isUnitDie = false;
        currentHP = UnitStats.maxHP;
        ChangeState(new UnitIdleState());
        UnitManager.instance.AddAllayList(this);
    }

    /// <summary>
    /// 유닛에게 상태이상을 적용하는 메서드
    /// </summary>
    /// <param name="damage">유닛에게 적용할 DamageData형식의 값</param>
    public void ApplyEffect(DamageData damage)
    {
        if (damage.effectType == StatusEffectType.Physical || damage.effectType == StatusEffectType.Magical)
        {
            return;
        }
        else if(!isUnitDie)
        {
            //statusEffectManager.OnStatusTriggerBuildup(damage.effectType, damage.buildAmount);
        }
    }

    /// <summary>
    /// 유닛의 마나를 회복하는 메서드
    /// </summary>
    void CollectMana()
    {
        if (canMana)
        {
            currentMP += finalStats[StatType.ManaRegen];
            if (currentMP >= UnitStats.maxMP && UnitStats.maxMP > 0)
            {
                currentMP = 0;
                GoSkill(true);
            }
        }
    }
    /// <summary>
    /// 유닛의 스킬을 사용하는 메서드
    /// </summary>
    public void DoSkill()
    {
        if(unitSkill != null && currentMP >= UnitStats.maxMP && UnitStats.maxMP != 0)
        {
            if (unitSkill.IsDurationSkill)
            {
                GoSkill(true, unitSkill.Duration);
            }
            else
            {
                unitSkill.ActivateSkill(this);
            }
            currentMP = 0;
        }
    }
    /// <summary>
    /// 유닛을 넉백시키는 메서드
    /// </summary>
    /// <param name="amount">유닛이 넉백당하는 양</param>
    /// <returns></returns>
    IEnumerator KnockBack(float amount)
    {
        if (!unit.data.UnitUnstoppable)
        {
            if (amount >= 0)
            {
                Vector2 direction = (this.transform.position - lastAttacker.position).normalized;
                unit.rb.AddForce(direction * amount, ForceMode2D.Impulse);
            }
            yield return new WaitForSeconds(0.5f);
            unit.rb.velocity = Vector2.zero;
        }
    }

    /// <summary>
    /// 유닛에게 힘을 가하는 메서드
    /// </summary>
    /// <param name="amount"></param>
    /// <param name="forcemod"></param>
    public void UnitAddForce(Vector2 amount, ForceMode2D forcemod)
    {
        if (followCoroutine != null)
        {
            StopCoroutine(followCoroutine);
            followCoroutine = null;
        }

        StartCoroutine(UnitAddForceCorutine(amount, forcemod));
    }

    public IEnumerator UnitAddForceCorutine(Vector2 amount, ForceMode2D forcemod)
    {
        if (!unit.data.UnitUnstoppable)
        {
            unit.rb.AddForce(amount, forcemod);
            yield return new WaitForSeconds(0.5f);
            unit.rb.velocity = Vector2.zero;
        }
    }

    public float GetNormalizedHealth()
    {
        if (UnitStats.maxHP == 0) return 0;
        return currentHP / UnitStats.maxHP;
    }
    
    public float GetNormalizedMana()
    {
        if (UnitStats.maxMP == 0) return 0;
        return currentMP / UnitStats.maxMP;
    }
    #endregion

    #region 아이템

    public void RegisterOnHitEffect(OnHitItem effect)
    {
        if (!onHitItemList.Contains(effect))
            onHitItemList.Add(effect);
    }

    public void UnregisterOnHitEffect(OnHitItem effect)
    {
        if (onHitItemList.Contains(effect))
            onHitItemList.Remove(effect);
    }

    public void TriggerOnHit(IDamageAble target)
    {
        foreach (var effect in onHitItemList)
        {
            effect.OnHit(this, target);
        }
        OnHit?.Invoke();
    }

    #endregion

    #region 스탯
    public void SetCurrentMana(float amount)
    {
        currentMP = amount;
    }

    public void AddModifierStat(StatModifier mod)
    {
        if(baseStats.Count == 0)
        {
            SetUnit();
        }

        statModifierList.RemoveAll(m => m.sourceId == mod.sourceId && m.statType == mod.statType);
        statModifierList.Add(mod);
        RecalculateStats();
    }

    public void AddModifierStats(List<StatModifier> mods)
    {
        if (baseStats.Count == 0)
        {
            SetUnit();
        }
        foreach (var mod in mods)
        {
            statModifierList.RemoveAll(m => m.sourceId == mod.sourceId && m.statType == mod.statType);
            statModifierList.Add(mod);
        }
        RecalculateStats();
    }

    public void RemoveModifierStats(string sourceId)
    {
        statModifierList.RemoveAll(m => m != null && m.sourceId == sourceId);
        RecalculateStats();
    }

    private void RecalculateStats()
    {
        finalStats.Clear();

        foreach (var stat in baseStats)
        {
            float add = 0f;
            float multiple = 1f;
            float percent = 0f;

            foreach (var mod in statModifierList.Where(m => m.statType == stat.Key))
            {
                if (mod.modifierMethod == ModifierMethod.Additive)
                {
                    add += mod.value;
                }
                else if (mod.modifierMethod == ModifierMethod.Multiplicative)
                    multiple *= 1 + mod.value;
                else if (mod.modifierMethod == ModifierMethod.AdditivePercent)
                    percent += mod.value;
            }
            finalStats[stat.Key] = (stat.Value + add) * multiple * (1 + percent);
        }

        ReBuildStats();
    }

    private void ReBuildStats()
    {
        if (baseStats.Count == 0)
        {
            SetUnit();
        }
    }

    #endregion

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (unit != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(this.transform.position, UnitStats.attackRange);
        }
    }
    public void ReattachSynergy()
    {

        EditorApplication.delayCall += () =>
        {
            if (this == null) return; // 오브젝트가 파괴된 경우 방지
            foreach (var component in GetComponents<MonoBehaviour>())
            {
                if (component is ISynergy)
                {
                    Undo.DestroyObjectImmediate(component);
                }
            }
            SynergyInstaller.AttachSynergy(this);
            EditorUtility.SetDirty(this);
        };
    }
#endif

    public bool GetIsStunned()
    {
        return isStunned;
    }

    public bool IsDestroyed()
    {
        return isUnitDie;
    }

}
