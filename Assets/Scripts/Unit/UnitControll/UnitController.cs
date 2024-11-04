using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UnitController : Unit
{
    internal Unit unit;

    private Vector2 _lastPosition;
    private UnitData _currentData;
    private bool _isFacingRight = true;
    internal bool isUnitDie = false;
    [SerializeField] private List<Unit> _attackers;

    [HideInInspector]
    public float unitHP;
    public float unitDamage;
    public float unitSpeed;
    public float unitAttackDistance;

    private float _unitAttackSpeed = 1.0f;
    private float _unitSenseDistance = 1.0f;

    void Start()
    {
        unit = GetComponent<Unit>();
        _attackers = new List<Unit>();
        _lastPosition = transform.position;

        SetUnit();
    }

    void Update()
    {
        unit.animator.SetFloat("speed", unit.agent.velocity.magnitude);
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
        if (unit != null && unit.data != null)
        {
            unitHP = unit.data.UnitHP;
            unitDamage = unit.data.UnitDamage;
            unitSpeed = unit.data.UnitSpeed;
            unitAttackDistance = unit.data.UnitAttackDistance;
            _unitAttackSpeed = unit.data.UnitAttackSpeed;
            _unitSenseDistance = unit.data.UnitSenseRadius;

            if (unit.agent != null)
            {
                unit.agent.speed = unit.data.UnitSpeed;
                unit.agent.updateRotation = false;
                unit.agent.updateUpAxis = false;
            }

            if (playerUnitManager != null && tag == "Unit")
            {
                playerUnitManager.AddAllayList(unit);
            }

            if (unitAnimationOverride != null)
            {
                unitAnimationOverride.SetAniamtion(unit.data.AnimatorOverrideController);
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
        unit.agent.SetDestination(targetPos);
    }
    #endregion

    #region 애니메이션 반전
    public void FlipAnimation()
    {

        AnimatorStateInfo stateInfo = unit.animator.GetCurrentAnimatorStateInfo(0);
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

    internal void ReceiveDamage(float damageInflict, Unit attacker)
    {
        unitHP -= damageInflict;
        unitHP = Mathf.Max(unitHP, 0);
        if (!_attackers.Contains(attacker))
        {
            _attackers.Add(attacker);
        }
        else if (_attackers[0] != attacker)
        {
            SwapElements<Unit>(_attackers, 0, _attackers.IndexOf(attacker));
        }
        Die();
    }
    void SwapElements<T>(List<T> list, int indexA, int indexB)
    {
        T temp = list[indexA];
        list[indexA] = list[indexB];
        list[indexB] = temp;
    }

    internal void Die()
    {
        if (unitHP <= 0 && !isUnitDie)
        {
            isUnitDie = true;
            animator.SetBool("isDie", true);
            StartCoroutine(KnockBack(2));
            unit.agent.enabled = false;
            unit.detectTarget.ClearTarget();
            foreach (Unit attacker in _attackers)
            {
                attacker.detectTarget.ClearTarget();
            }
            _attackers.Clear();

            if (this.transform.tag == "Unit")
            {
                playerUnitManager.RemoveAllayList(this.unit);
            }
            else
            {
                playerUnitManager.RemoveEnemyList(this.unit);
            }
        }
    }

    internal void Revive()
    {
        this.tag = "Unit";
        rb.velocity = Vector2.zero;
        agent.enabled = true;
        animator.SetBool("isDie", false);
        isUnitDie = false;
        unitHP = unit.data.UnitHP;
        animator.Play("IdleState");
        playerUnitManager.AddAllayList(this.unit);
    }

    IEnumerator KnockBack(float amount)
    {
        if (!unit.data.UnitUnstoppable)
        {
            if (amount >= 0)
            {
                agent.enabled = false;
                Vector2 direction = (this.transform.position - _attackers[0].transform.position).normalized;
                rb.AddForce(direction * amount, ForceMode2D.Impulse);
            }
            yield return new WaitForSeconds(0.5f);
            rb.velocity = Vector2.zero;
            agent.enabled = true;
        }
    }
    #endregion

    private void OnDrawGizmos()
    {
        if (unit != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(this.transform.position, _unitSenseDistance);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(this.transform.position, unitAttackDistance);
        }
    }
}
