using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using DG.Tweening;
using Pathfinding;


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

public class UnitController : Unit, IStatusAble, IDamageAble //유닛의 전반적인 컨트롤
{
    public event Action<GameObject> OnDestroyed;

    private Dictionary<StatType, float> baseStats = new();
    private List<StatModifier> statModifierList = new();
    private Dictionary<StatType, float> finalStats = new();

    private Tween pathTween;
    private DotweenMoveAvoidance moveAvoidance;

    public delegate void UnitAttackCountEvent();

    public static event UnitAttackCountEvent OnUnitAttackCount;

    private StatusEffectManager statusEffectManager;

    private Unit _unit;

    private Vector2 _lastPosition; //애니메이션 좌우 반전을 위한 변수

    [SerializeField] private List<Vector2> fullPath = new(); //유닛의 경로를 저장하기 위한 변수

    private bool inCombat = false; //유닛이 전투중인지 확인하기 위한 변수

    private bool _isFacingRight = true;

    private SciptableObjects.UnitData _currentData; //유닛의 데이터 변화 감지를 위한 변수
    private bool isUnitDie;
    private AIDestinationSetter _destinationSetter;
    private AIPath _aiPath;

    private Transform _lastAttacker; //넉백을 위해 마지막 공격자를 알아내는 변수
    public IActiveSkill unitSkill;
    public IPasseiveSkillAttack unitPassiveSkill;
    public bool canMana = true;

    private string currentAnimationName;

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
    private Vector3 currentTargetWorldPos;

    private IUnitState currentState;

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
    
    void Awake()
    {
        _destinationSetter = GetComponent<AIDestinationSetter>();
        _aiPath = GetComponent<AIPath>();
    }

    protected override void Start()
    {
        isUnitDie = false;
        base.Start();
        unitPassiveSkill = GetComponent<IPasseiveSkillAttack>();
        _unit = GetComponent<Unit>();
        data = _unit.data;
        statusEffectManager = GetComponent<StatusEffectManager>();
        _lastPosition = transform.position;
        rb.drag = 0.5f; // 물리적 마찰력 조정
        SetUnit();

        ChangeState(new UnitIdleState());
    }

    void Update()
    {
        animator.SetFloat("speed", _aiPath.velocity.magnitude);
        currentState?.Update();
        if(!IsDestroyed() && _aiPath.canMove == true && _destinationSetter.target == null)
        {
            _aiPath.canMove = false;
            ChangeState(new UnitIdleState());
            return;
        }
        //rb.velocity = Vector2.right * 5f;
    }

    private void FixedUpdate()
    {
        //Debug.Log($"{name} - FixedUpdate velocity: {rb.velocity}");

        FlipAnimation();
    }
    #region FSM

    public void ChangeState(IUnitState newState)
    {
        currentState?.Exit();
        currentState = newState;
        currentState.Enter(this);
    }

    #endregion

    #region 기본셋업
    private void SetUnit()
    {
        if (data != null)
        {
            baseStats.Clear();
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

            SetMoveSpeed(unitSpeed);

            if (PlayerUnitManager.instance.allayPrefabList != null && tag == "Unit")
            {
                PlayerUnitManager.instance.AddAllayList(this.gameObject);
            }
            //Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Unit"), LayerMask.NameToLayer("Unit"), true);
            _currentData = data;
        }
    }


    #endregion

    #region 이동

    public void SetMoveSpeed(float speed)
    {
        unitSpeed = speed;
        _aiPath.maxSpeed = unitSpeed;
    }

    public void SetMoveWork(bool canMove)
    {
        _aiPath.canMove = canMove;
    }

    public void SetTargetToMove(Transform target)
    {
        _destinationSetter.target = target;
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
        rb.velocity = Vector2.zero;
    }
    #endregion

    #region 애니메이션
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

    public void SetAnimation(string animationName)
    {
        if(currentAnimationName == animationName)
        {
            return;
        }

        animator.Play(animationName);
        currentAnimationName = animationName;
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
        if (currentHP <= 0 && !isUnitDie) Die();
    }

    internal void Die()
    {
        StopMovement();
        isUnitDie = true;
        OnDestroyed?.Invoke(this.gameObject);

        ChangeState(new UnitDieState());
        StartCoroutine(KnockBack(2.0f));
        detectTarget.ClearTarget();

        if (this.transform.tag == "Unit")
        {
            PlayerUnitManager.instance.RemoveAllayList(this.gameObject);
        }
    }

    internal void Revive() //일어나라
    {
        this.tag = "Unit";
        rb.velocity = Vector2.zero;
        detectTarget.ClearTarget();
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
        }
    }

    IEnumerator KnockBack(float amount)
    {
        if (!data.UnitUnstoppable)
        {
            if (amount >= 0)
            {
                Vector2 direction = (this.transform.position - _lastAttacker.position).normalized;
                rb.AddForce(direction * amount, ForceMode2D.Impulse);
            }
            yield return new WaitForSeconds(0.5f);
            rb.velocity = Vector2.zero;
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
        unitSpeed = finalStats[StatType.MoveSpeed];
    }

    #endregion

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (GetUnit() != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(this.transform.position, unitSenseDistance);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(this.transform.position, unitAttackDistance);
        }

        if (fullPath == null || fullPath.Count == 0)
            return;

        Gizmos.color = Color.cyan;

        Vector3 prev = transform.position;
        foreach (var point in fullPath)
        {
            Gizmos.DrawLine(prev, point);
            Gizmos.DrawSphere(point, 0.1f);
            prev = point;
        }
    }

    public void DebugDrawPath(List<Vector2Int> path, Color color, float duration = 1f)
    {
        if (path == null || path.Count < 2) return;

        for (int i = 0; i < path.Count - 1; i++)
        {
            Vector3 start = GridManager.instance.GridToWorld(path[i]);
            Vector3 end = GridManager.instance.GridToWorld(path[i + 1]);

            Debug.DrawLine(start, end, color, duration);
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
