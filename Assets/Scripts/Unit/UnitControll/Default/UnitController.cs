using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

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

public class UnitController : Unit,IDamageAble //유닛의 전반적인 컨트롤
{
    public delegate void UnitDeathEvent(GameObject unit);
    public delegate void UnitAttackCountEvent();

    public static event UnitDeathEvent OnUnitDeath;
    public static event UnitAttackCountEvent OnUnitAttackCount;

    private StatusEffectManager statusEffectManager;
    private Unit _unit;
    private Vector2 _lastPosition; //애니메이션 좌우 번전을 위한 변수
    private bool _isFacingRight = true; 
    private SciptableObjects.UnitData _currentData; //유닛의 데이터 변화 감지를 위한 변수
    internal bool isUnitDie = false;
    private Transform _lastAttacker; //넉백을 위해 마지막 공격자를 알아내는 변수
    public IActiveSkill unitSkill;
    public IPasseiveSkillAttack unitPassiveSkill;
    public bool canMana = true;

    [HideInInspector] public bool isStunned = false;
    [HideInInspector] public float maxHP;
    [HideInInspector] public float currentHP { get; private set; }
    [HideInInspector] public float maxMP;
    [HideInInspector] public float currentMP { get; private set; }
    [HideInInspector] public float unitDamage;
    [HideInInspector] public float unitSpeed;
    [HideInInspector] public float unitAttackDistance;

    private float _unitAttackSpeed = 1.0f;
    private float _unitSenseDistance = 1.0f;

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
            maxHP = data.UnitHP;
            maxMP = data.UnitMP;
            currentHP = maxHP;
            currentMP = 0;
            unitDamage = data.UnitDamage;
            unitSpeed = data.UnitSpeed;
            unitAttackDistance = data.UnitAttackDistance;
            _unitAttackSpeed = data.UnitAttackSpeed;
            _unitSenseDistance = data.UnitSenseRadius;
            agent.speed = data.UnitSpeed;

            if (playerUnitManager != null && tag == "Unit")
            {
                playerUnitManager.AddAllayList(GetUnit().gameObject);
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
        OnUnitDeath?.Invoke(gameObject);
        isUnitDie = true;
        animator.SetBool("isDie", true);
        StartCoroutine(KnockBack(2.0f));
        agent.enabled = false;
        detectTarget.ClearTarget();

        if (this.transform.tag == "Unit")
        {
            playerUnitManager.RemoveAllayList(base.GetUnit().gameObject);
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
        playerUnitManager.AddAllayList(GetUnit().gameObject);
    }

    private void ApplyEffect(DamageData damage)
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
            currentMP += 5;
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

    private void OnDrawGizmos() //디버그용 기즈모
    {
        if (GetUnit() != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(this.transform.position, _unitSenseDistance);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(this.transform.position, unitAttackDistance);
        }
    }


}
