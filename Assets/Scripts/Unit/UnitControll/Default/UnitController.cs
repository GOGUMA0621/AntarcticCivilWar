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
public partial class UnitController : MonoBehaviour, IStatusAble, IDamageAble //유닛의 전반적인 컨트롤
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

    private bool _isFacingRight = true; //유닛이 바라보는 방향을 저장하기 위한 변수

    protected bool isUnitDie; //유닛이 죽었는지 확인하기 위한 변수

    protected bool disableFlip = false; //애니메이션 좌우 반전을 막기 위한 변수

    private Transform lastAttacker; //넉백을 위해 마지막 공격자를 알아내는 변수
    public IActiveSkill unitSkill; //유닛의 스킬을 저장하는 변수
    public IPasseiveSkillAttack unitPassiveSkill; //유닛의 패시브 스킬을 저장하는 변수
    public bool canMana = true; //유닛이 마나를 사용할 수 있는지 여부
    public bool isSkillActive = false; //유닛의 스킬이 활성화되어 있는지 여부

    private string currentAnimationName; //현재 재생중인 애니메이션 이름
    private bool isPaused = false; //애니메이션이 일시정지 상태인지 여부
    private int pausedStateHash; //일시정지 상태의 해시값
    private float pausedTime; //일시정지된 시간

    public bool isStunned = false; //유닛이 기절 상태인지 여부

    [HideInInspector] public UnitStats UnitStats; //유닛의 스탯을 저장하는 변수
    [SerializeField] private UnitStats baseUnitStats; //유닛의 기본 스탯을 저장하는 변수
    [SerializeField] public float currentHP; //유닛의 현재 체력을 저장하는 변수

    public float currentMP { get; private set; }

    public bool isAllay = true; //유닛이 아군인지 여부

    public int unitLevel = 1; //유닛의 레벨을 저장하는 변수

    public float unitAttackSpeed = 1.0f; //유닛의 공격 속도를 저장하는 변수
    protected IUnitState currentState; //유닛의 현재 상태를 저장하는 변수

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
    }

    private void Awake()
    {
        cachedAstarGrid =  asterGrid != null ? asterGrid : FindObjectOfType<AstarPathfinder>();
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
        SetUnit();
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
    private readonly IUnitState placeState = new UnitPlaceState(); //유닛이 배치되는 상태
    private readonly IUnitState idleState = new UnitIdleState(); //유닛이 대기 상태인 경우
    private readonly IUnitState attackState = new UnitAttackState(); //유닛이 공격 상태인 경우
    private readonly IUnitState followState = new UnitFollowState(); //유닛이 추적 상태인 경우
    private readonly IUnitState dieState = new UnitDieState(); //유닛이 사망 상태인 경우
    private readonly IUnitState manaSkillState = new UnitManaSkillState(); //유닛이 마나 스킬 상태인 경우

    public virtual void GoPlace() => ChangeState(placeState); //유닛이 배치 상태로 전환
    public virtual void GoIdle() => ChangeState(idleState); //유닛이 대기 상태로 전환
    public virtual void GoAttack() => ChangeState(attackState); //유닛이 공격 상태로 전환
    public virtual void GoFollow() => ChangeState(followState); //유닛이 추적 상태로 전환
    public virtual void GoDie() => ChangeState(dieState); //유닛이 사망 상태로 전환
    public virtual void GoSkill(bool isStanding = false, float duration = 0f) 
        => ChangeState(manaSkillState); //유닛이 마나 스킬 상태로 전환
    /// <summary>
    /// 유닛의 상태를 변경하는 메서드입니다.
    /// </summary>
    /// <param name="newState">변경할 새로운 상태입니다.</param>
    public void ChangeState(IUnitState newState)
    {
        if (currentState?.GetType() == newState.GetType())
        {
            return;
        }

        currentState?.Exit();
        currentState = newState;
        currentState.Enter(this);
    }
    /// <summary>
    /// 유닛의 현재 상태를 반환하는 메서드입니다.
    /// </summary>
    /// <returns>유닛의 현재 상태입니다.</returns>
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
            baseStats.Add(StatType.ManaGain, 5);
            baseStats.Add(StatType.AttackDamage, unit.data.UnitDamage[idx]);
            baseStats.Add(StatType.AttackSpeed, unit.data.UnitAttackSpeed);
            baseStats.Add(StatType.AttackRange, unit.data.UnitAttackDistance);
            baseStats.Add(StatType.MoveSpeed, unit.data.UnitSpeed);
            baseStats.Add(StatType.CritChance, 0.2f);
            baseStats.Add(StatType.CritDamage, 1.3f);
            baseStats.Add(StatType.Endurance, 0);
            baseStats.Add(StatType.DamageAmp, 0);

            baseUnitStats = new UnitStats(baseStats);

            RecalculateStats();

            currentHP = UnitStats.maxHP;
            currentMP = 0;
        }
    }

    public int GetUnitLevel()
    {
        return unitLevel;
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
          if(isSkillActive)
            {
                GoSkill(true, unitSkill.Duration);
            }
            else
            {
                GoAttack();
            }
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
        CollectMana();// 마나 수집
        TryActivateSkill(); // 스킬 발동 시도
        OnHit?.Invoke();// 공격시 이벤트 호출

        if (unitPassiveSkill != null)
        {
            if (unitPassiveSkill.PassiveCondition())
            {
                unitPassiveSkill.DoPassiveSkill();// 패시브 스킬 발동
            }
            else
            {
                //Debug.Log(name);
                unit.attackController.Attack();// 일반 공격
            }
        }
        else
        {
            //Debug.Log(name);
            unit.attackController.Attack();// 일반 공격
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
        float endurance = UnitStats.endurance;
        float damageRecution = Mathf.Clamp(endurance,0f,0.75f);
        float reducedDamage = damage.damage * (1 - damageRecution);
        
        currentHP -= reducedDamage;
        ApplyEffect(damage);
        CollectMana();
        TryActivateSkill();
        WhenHit?.Invoke();
        if (currentHP <= 0 && !isUnitDie) Die();
    }

    /// <summary>
    /// 유닛이 죽었을 때 호출되는 메서드
    /// </summary>
    internal void Die()
    {
        StopMovement(); //유닛의 이동 멈춤
        isUnitDie = true;
        OnDestroyed?.Invoke(this); //유닛이 죽었음을 알림

        GoDie();// 유닛의 상태를 죽음 상태로 변경
        unit.detectTarget.ClearTarget(); //유닛의 타겟 초기화
        unit.capsuleCollider.enabled = false; //유닛의 콜라이더 비활성화
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
            currentMP += UnitStats.manaGain;
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
    public void TryActivateSkill()
    {
        if(unitSkill != null && currentMP >= UnitStats.maxMP && UnitStats.maxMP != 0)
        {
            unitSkill.ActivateSkill(this);
            isSkillActive = true;
            canMana = false;
            GoSkill(true);
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
    /// <summary>
    /// 유닛의 공격 시 발동할 아이템을 등록하는 메서드
    /// </summary>
    /// <param name="effect">등록할 아이템 효과</param>
    public void RegisterOnHitEffect(OnHitItem effect)
    {
        if (!onHitItemList.Contains(effect))
            onHitItemList.Add(effect);
    }
    /// <summary>
    /// 유닛의 공격 시 발동할 아이템을 해제하는 메서드
    /// </summary>
    /// <param name="effect">해제할 아이템 효과</param>
    public void UnregisterOnHitEffect(OnHitItem effect)
    {
        if (onHitItemList.Contains(effect))
            onHitItemList.Remove(effect);
    }
    /// <summary>
    /// 유닛이 공격할 때 아이템 효과를 발동하는 메서드
    /// </summary>
    /// <param name="target">대상 유닛</param>
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
    /// <summary>
    /// 유닛의 스탯에 모디파이어를 추가하는 메서드
    /// </summary>
    /// <param name="mod">추가할 모디파이어</param>
    public void AddModifierStat(StatModifier mod)
    {
        if(baseStats.Count == 0)
        {
            SetUnit();
        }
        // 기존 같은 소스ID와 스탯타입을 가진 모디파이어 제거 후 추가
        statModifierList.RemoveAll(m => m.sourceId == mod.sourceId && m.statType == mod.statType);
        statModifierList.Add(mod);
        RecalculateStats();
    }
    /// <summary>
    /// 유닛의 스탯에 모디파이어를 여러개 추가하는 메서드
    /// </summary>
    /// <param name="mods">추가할 모디파이어 리스트</param>
    public void AddModifierStats(List<StatModifier> mods)
    {
        if (baseStats.Count == 0)
        {
            SetUnit();
        }
        // 기존 같은 소스ID와 스탯타입을 가진 모디파이어 제거 후 추가
        foreach (var mod in mods)
        {
            statModifierList.RemoveAll(m => m.sourceId == mod.sourceId && m.statType == mod.statType);
            statModifierList.Add(mod);
        }
        RecalculateStats();
    }
    /// <summary>
    /// 유닛의 스탯에서 특정 소스ID를 가진 모디파이어를 제거하는 메서드
    /// </summary>
    /// <param name="sourceId">제거할 모디파이어의 소스ID</param>
    public void RemoveModifierStats(string sourceId)
    {
        statModifierList.RemoveAll(m => m != null && m.sourceId == sourceId);
        RecalculateStats();
    }
    /// <summary>
    /// 유닛의 스탯을 재계산하는 메서드
    /// </summary>
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
                if (mod.modifierMethod == ModifierMethod.Additive) //더하기
                {
                    add += mod.value;
                }
                else if (mod.modifierMethod == ModifierMethod.Multiplicative) //곱하기
                    multiple *= 1 + mod.value;
                else if (mod.modifierMethod == ModifierMethod.AdditivePercent) //백분율 더하기
                    percent += mod.value;
            }
            finalStats[stat.Key] = (stat.Value + add + (stat.Value * (1 + percent))) * multiple;
        }

        UnitStats = new UnitStats(finalStats); // 최종 스탯 업데이트
        ReBuildStats();
    }
    /// <summary>
    /// 유닛의 스탯이 비어있을 경우 기본 스탯을 다시 설정하는 메서드
    /// </summary>
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

    public float GetFinalStat(StatType statType)
    {
        if (finalStats != null && finalStats.TryGetValue(statType, out float value))
            return value;
        return 0f;
    }

    public void ApplyStun(float duration)
    {
        if (isStunned) return;
        StartCoroutine(StunCoroutine(duration));
    }

    private IEnumerator StunCoroutine(float duration)
    {
        isStunned = true;
        yield return new WaitForSeconds(duration);
        isStunned = false;
    }
}
