using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using DG.Tweening;
using Pathfinding;
using Unity.VisualScripting;
using UnityEngine.Rendering;


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

public class UnitController : MonoBehaviour, IStatusAble, IDamageAble //유닛의 전반적인 컨트롤
{
    public event Action<GameObject> OnDestroyed;

    private Dictionary<StatType, float> baseStats = new();
    private List<StatModifier> statModifierList = new();
    private Dictionary<StatType, float> finalStats = new();

    private List<OnHitItem> onHitItemList = new();

    public delegate void UnitAttackCountEvent();

    public static event UnitAttackCountEvent OnUnitAttackCount;

    private StatusEffectManager statusEffectManager;

    [HideInInspector] public Unit unit;

    private Vector2 _lastPosition; //애니메이션 좌우 반전을 위한 변수

    [SerializeField] private List<Vector2> fullPath = new(); //유닛의 경로를 저장하기 위한 변수

    //private bool inCombat = false; //유닛이 전투중인지 확인하기 위한 변수

    private bool _isFacingRight = true;

    private SciptableObjects.UnitData _currentData; //유닛의 데이터 변화 감지를 위한 변수
    protected bool isUnitDie;

    protected bool disableFlip = false; //애니메이션 좌우 반전을 막기 위한 변수

    private Transform _lastAttacker; //넉백을 위해 마지막 공격자를 알아내는 변수
    public IActiveSkill unitSkill;
    public IPasseiveSkillAttack unitPassiveSkill;
    public bool canMana = true;


    private string currentAnimationName;
    private bool isPaused = false;
    private int pausedStateHash;
    private float pausedTime;

    [HideInInspector] public bool isStunned = false;
    public float maxHP;
    [HideInInspector] public float currentHP { get; private set; }
    public float maxMP;
    [HideInInspector] public float currentMP;
    public float unitDamage;
    public float unitSpeed;
    [HideInInspector] public float unitAttackDistance;

    private float unitAttackSpeed = 1.0f;
    private float unitSenseDistance = 1.0f;

    protected IUnitState currentState;
    private IUnitState manaSkillState;

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

    public void DestroyEvent(GameObject gameObject)
    {
        OnDestroyed?.Invoke(gameObject);
    }
    #endregion
    
    protected virtual void Start()
    {
        isUnitDie = false;
        unitPassiveSkill = GetComponent<IPasseiveSkillAttack>();
        unit = GetComponent<Unit>();
        statusEffectManager = GetComponent<StatusEffectManager>();
        _lastPosition = transform.position;
        SetUnit();

        currentState = GetInitialState();
        manaSkillState = GetManaSkillState();
        currentState.Enter(this);
    }

    private void Update()
    {
        unit.animator.SetFloat("speed", unit.aiPath.velocity.magnitude);
        currentState?.Update();
        //if(!IsDestroyed() && unit.aiPath.canMove == true && unit.settler.target == null)
        //{
        //    unit.aiPath.canMove = false;
        //    ChangeState(new UnitIdleState());
        //    return;
        //}
    }

    private void FixedUpdate()
    {
        //Debug.Log($"{name} - FixedUpdate velocity: {_unit.rb.velocity}");

        FlipAnimation();
    }
    #region 큐
    

    #endregion

    #region FSM
    private readonly IUnitState idleState = new UnitIdleState();
    private readonly IUnitState attackState = new UnitAttackState();
    private readonly IUnitState followState = new UnitFollowState();
    private readonly IUnitState dieState = new UnitDieState();
    private readonly IUnitState callState = new UnitCallState();

    public virtual void GoIdle() => ChangeState(idleState);
    public virtual void GoAttack() => ChangeState(attackState);
    public virtual void GoFollow() => ChangeState(followState);
    public virtual void GoDie() => ChangeState(dieState);
    public void GoCall() => ChangeState(callState);

    public void ChangeState(IUnitState newState)
    {
        if (currentState?.GetType() == newState.GetType()) 
        {
            //Debug.Log($"[FSM] Skip: Already in {newState.GetType().Name}");
            return;
        }
        //Debug.Log($"[FSM] Change State: {currentState?.GetType().Name} -> {newState.GetType().Name}");
        currentState?.Exit();
        currentState = newState;
        currentState.Enter(this);
    }

    public IUnitState GetCurrentState()
    {
        return currentState;
    }

    protected virtual IUnitState GetInitialState()
    {
        return new UnitIdleState();
    }

    protected virtual IUnitState GetManaSkillState()
    {
        return new UnitManaSkillState();
    }

    #endregion

    #region 기본셋업
    private void SetUnit()
    {
        if (unit.data != null)
        {
            baseStats.Clear();
            baseStats.Add(StatType.MaxHealth, unit.data.UnitHP);
            baseStats.Add(StatType.HealthRegen, 0);
            baseStats.Add(StatType.MaxMana, unit.data.UnitMP);
            baseStats.Add(StatType.ManaRegen, 5);
            baseStats.Add(StatType.AttackDamage, unit.data.UnitDamage);
            baseStats.Add(StatType.AttackSpeed, unit.data.UnitSpeed);
            baseStats.Add(StatType.AttackRange, unit.data.UnitAttackDistance);
            baseStats.Add(StatType.MoveSpeed, unit.data.UnitSpeed);
            baseStats.Add(StatType.CritChance, 0);

            RecalculateStats();

            unit.rb.drag = 0.5f;

            currentHP = maxHP;
            currentMP = 0;
            unitSenseDistance = unit.data.UnitSenseRadius;

            SetMoveSpeed(unitSpeed);

            //if (PlayerUnitManager.instance.allayPrefabList != null && tag == "Unit")
            //{
            //    PlayerUnitManager.instance.AddAllayList(this.gameObject);
            //}
            //Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Unit"), LayerMask.NameToLayer("Unit"), true);
            _currentData = unit.data;
        }
    }


    #endregion

    #region 이동

    public void SetMoveSpeed(float speed)
    {
        unitSpeed = speed;
        unit.aiPath.maxSpeed = unitSpeed;
    }

    public void SetMoveWork(bool canMove)
    {
        unit.aiPath.canMove = canMove;
    }

    public void ToggleAITrue()
    {
        unit.aiPath.canMove = true;
    }

    public void SetTargetToMove(Transform target)
    {
        if (target == null)
        {
            //Debug.LogWarning($"[SetTargetToMove] target is null");
            return;
        }

        if (unit.settler == null)
        {
            //Debug.LogError($"[SetTargetToMove] settler is NULL on {unit.name}");
            return;
        }
        //Debug.Log($"[SetTargetToMove] {name} {target.name}");
        unit.settler.target = target;
    }

    public void MoveToTarget(Vector3 targetWorldPos)
    {
        if(fullPath != null && fullPath.Count > 0)
        {
            return;
        }
        List<Vector2Int> path = PathFinding.instance.FindPath(GridManager.instance.WorldToGrid(transform.position), GridManager.instance.WorldToGrid(targetWorldPos));
        if(path != null)
        {
            fullPath = path.Select(v=> (Vector2)v).ToList();
            Debug.Log($"[이동 경로] {name} {fullPath.Count}");
        }
    }

    public void MoveAlongPath()
    {
        if(fullPath == null || fullPath.Count == 0)
        {
            return;
        }
        var speed = unitSpeed * Time.deltaTime;

        transform.position = Vector2.MoveTowards(transform.position, fullPath[0], speed);
        transform.position = new Vector2(transform.position.x, transform.position.y);

        if (Vector2.Distance(transform.position, fullPath[0]) < 0.1f)
        {
            fullPath.RemoveAt(0);
            if (fullPath.Count == 0)
            {
                StopMovement();
                return;
            }
        }
    }

    public void StopMovement()
    {
        //Debug.Log($"{name} 이동 중지");
        StopAllCoroutines();
        unit.aiPath.canMove = false;
        unit.settler.target = null;
        unit.rb.velocity = Vector2.zero;
    }
    #endregion

    #region 애니메이션
    public void FlipAnimation()
    {
        if (isUnitDie || disableFlip) return;

        AnimatorStateInfo stateInfo = unit.animator.GetCurrentAnimatorStateInfo(0);
        Vector2 currentPosition = transform.position;
        if (stateInfo.IsTag("Battle"))
        {
            if(unit.detectTarget.targetToAttack != null)
            {
                float targetDirection = unit.detectTarget.targetToAttack.transform.position.x - currentPosition.x;

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

    protected void Flip()
    {
        Vector2 curentScale = gameObject.transform.localScale;
        curentScale *= new Vector2(-1, 1);
        gameObject.transform.localScale = curentScale;

        _isFacingRight = !_isFacingRight;
    }

    public void SetAnimation(string animationName)
    {
        if(currentAnimationName == animationName)
        {
            return;
        }

        unit.animator.Play(animationName);
        currentAnimationName = animationName;
    }

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

    public void ResumeAnimation()
    {
        StartCoroutine(ResumeCoroutine());
    }

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

    public virtual void Heal(float amount)
    {
        currentHP = Math.Clamp(currentHP += amount, 0f, maxHP);
    }

    public virtual void ReceiveDamage(DamageData damage)
    {
        UnitAttackController.OnUnitAttack += HandleAttackEvent;
        currentHP -= damage.damage;
        ApplyEffect(damage);
        CollectMana();
        currentHP = Mathf.Max(currentHP, 0);
        DoSkill();
        UnitAttackController.OnUnitAttack -= HandleAttackEvent;
        if (currentHP <= 0 && !isUnitDie) Die();
    }

    internal void Die()
    {
        StopMovement();
        isUnitDie = true;
        OnDestroyed?.Invoke(this.gameObject);

        ChangeState(new UnitDieState());
        StartCoroutine(KnockBack(2.0f));
        unit.detectTarget.ClearTarget();
        unit.capsuleCollider.enabled = false;

        if (this.tag != "Unit")
        {
            PlayerUnitManager.instance.AddUnitToRevive(this);

            if (this.transform.tag == "Unit")
            {
                PlayerUnitManager.instance.RemoveAllayList(this.gameObject);
            }
        }
    }

    internal void Revive() //일어나라
    {
        this.tag = "Unit";
        unit.capsuleCollider.enabled = true;
        unit.rb.velocity = Vector2.zero;
        unit.detectTarget.ClearTarget();
        isUnitDie = false;
        currentHP = maxHP;
        ChangeState(new UnitIdleState());
        PlayerUnitManager.instance.AddAllayList(this.gameObject);
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
            if (currentMP >= maxMP && maxMP > 0)
            {
                currentMP = 0;
                ChangeState(manaSkillState);
            }
        }
    }

    public void DoSkill()
    {
        if (currentMP >= maxMP && maxMP != 0) 
        {
            unit.animator.Play("ManaSkill");
            Debug.Log("마나");
            currentMP = 0;
        }
    }

    public void ResetUnit()
    {
        if (unit.data != null)
        {
            maxHP = unit.data.UnitHP;
            maxMP = unit.data.UnitMP;
            unitDamage = unit.data.UnitDamage;
            unitSpeed = unit.data.UnitSpeed;
            unitAttackDistance = unit.data.UnitAttackDistance;
            unitAttackSpeed = unit.data.UnitAttackSpeed;
            unitSenseDistance = unit.data.UnitSenseRadius;
        }
    }

    IEnumerator KnockBack(float amount)
    {
        if (!unit.data.UnitUnstoppable)
        {
            if (amount >= 0)
            {
                Vector2 direction = (this.transform.position - _lastAttacker.position).normalized;
                unit.rb.AddForce(direction * amount, ForceMode2D.Impulse);
            }
            yield return new WaitForSeconds(0.5f);
            unit.rb.velocity = Vector2.zero;
        }
    }

    public void UnitAddForce(Vector2 amount, ForceMode2D forcemod)
    {
        StartCoroutine(UnitAddForceCorutine(amount, forcemod));
    }

    public IEnumerator UnitAddForceCorutine(Vector2 amount, ForceMode2D forcemod)
    {
        if (!unit.data.UnitUnstoppable)
        {
            unit.aiPath.canMove = false;
            unit.rb.AddForce(amount, forcemod);
        }
        yield return new WaitForSeconds(0.5f);
        unit.rb.velocity = Vector2.zero;
        unit.aiPath.canMove = true;
    }

    public float GetNormalizedHealth()
    {
        return currentHP / maxHP;
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
        foreach(var effect in onHitItemList)
        {
            effect.OnHit(this, target);
        }
    }

    #endregion

    #region 스탯
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
        unitSpeed = finalStats[StatType.MoveSpeed];
    }

    #endregion

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (unit != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(this.transform.position, unitSenseDistance);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(this.transform.position, unitAttackDistance);
        }
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
