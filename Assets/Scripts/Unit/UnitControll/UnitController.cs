using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class UnitController : Unit,IDamageAble //유닛의 전반적인 컨트롤
{
    public delegate void UnitDeathEvent(Unit unit);

    public static event UnitDeathEvent OnUnitDeath;

    private Unit _unit;
    private Vector2 _lastPosition; //애니메이션 좌우 번전을 위한 변수
    private bool _isFacingRight = true; 
    private UnitData _currentData; //유닛의 데이터 변화 감지를 위한 변수
    internal bool isUnitDie = false;
    private Unit _lastAttacker; //넉백을 위해 마지막 공격자를 알아내는 변수

    [HideInInspector] public float unitHP;
    [HideInInspector] public float unitMax_MP;
    [HideInInspector] public float unitMP;
    [HideInInspector] public float unitDamage;
    [HideInInspector] public float unitSpeed;
    [HideInInspector] public float unitAttackDistance;
    [HideInInspector] public IUnitSkill ManaSkill;
    [HideInInspector] public float m_SkillDelay;
    [HideInInspector] public IUnitSkill UniqeSkill;

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

    private void HandleAttackEvent(Unit attacker)
    {
        Debug.Log(attacker);
        _lastAttacker = attacker;
    }
    #endregion

    protected override void Start()
    {
        base.Start();
        _unit = GetComponent<Unit>();
        data = _unit.data;
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
            unitHP = data.UnitHP;
            unitMax_MP = data.UnitMax_MP;
            unitMP = data.UnitMP;
            unitDamage = data.UnitDamage;
            unitSpeed = data.UnitSpeed;
            ManaSkill = data.manaSkill;
            m_SkillDelay = data.M_SkillDelay;
            unitAttackDistance = data.UnitAttackDistance;
            _unitAttackSpeed = data.UnitAttackSpeed;
            _unitSenseDistance = data.UnitSenseRadius;
            agent.speed = data.UnitSpeed;


            if (playerUnitManager != null && tag == "Unit")
            {
                playerUnitManager.AddAllayList(GetUnit());
            }

            if (unitAnimationOverride != null)
            {
                unitAnimationOverride.SetAniamtion(data.AnimatorOverrideController);
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

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        Vector2 currentPosition = transform.position;
        if (stateInfo.IsName("AttackState"))
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
        attackController.Attack();
        
    }

    public void ReceiveDamage(float damageInflict)
    {
        UnitAttackController.OnUnitAttack += HandleAttackEvent;
        unitHP -= damageInflict;
        Debug.Log("피해 입음");
        unitHP = Mathf.Max(unitHP, 0);
        UnitAttackController.OnUnitAttack -= HandleAttackEvent;
        if (unitHP <= 0 && !isUnitDie) Death();
        
    }

    internal void Death()
    {
        OnUnitDeath?.Invoke(GetComponent<Unit>());
        isUnitDie = true;
        animator.SetBool("isDie", true);
        StartCoroutine(KnockBack(2.0f));
        agent.enabled = false;
        detectTarget.ClearTarget();

        if (this.transform.tag == "Unit")
        {
            playerUnitManager.RemoveAllayList(base.GetUnit());
        }

    }

    internal void Revive()
    {
        if (this.tag != "A.Fabric")
        {
            this.tag = "Unit";
            rb.velocity = Vector2.zero;
            agent.enabled = true;
            detectTarget.ClearTarget();
            animator.SetBool("isDie", false);
            isUnitDie = false;
            unitHP = data.UnitHP;
            unitMax_MP = data.UnitMax_MP;
            unitMP = data.UnitMP;
            animator.Play("IdleState");
            playerUnitManager.AddAllayList(GetUnit());
        }
    }

    IEnumerator KnockBack(float amount)
    {
        if (!data.UnitUnstoppable)
        {
            if (amount >= 0)
            {
                agent.enabled = false;
                Vector2 direction = (this.transform.position - _lastAttacker.transform.position).normalized;
                rb.AddForce(direction * amount, ForceMode2D.Impulse);
            }
            yield return new WaitForSeconds(0.5f);
            rb.velocity = Vector2.zero;
            agent.enabled = true;
        }
    }

    public void UseManaSkill()
    {

        ManaSkill.Execute(this);
        unitMP = 0;
        



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
