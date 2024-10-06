using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.AI;

public class UnitController : MonoBehaviour
{
    internal Unit unit;
    [SerializeField] UnitData _unitData;

    private Vector2 _lastPosition;
    private UnitData _currentData;
    private bool _isFacingRight = true;
    internal bool isUnitDie = false;
    [SerializeField] private List <Unit> _attackers;
    
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
        if (unit.isSetup)
        {
            SetUnit();
        }
    }

    void Update()
    {
        unit.detectTarget.detectRadiusCollider.radius = unit.data.UnitSenseRadius;
        unit.animator.SetFloat("speed", unit.agent.velocity.magnitude);
    }

    private void FixedUpdate()
    {
        if (_currentData != unit.data)
        {
            SetUnit();
        }
        FlipAnimation();
    }
    #region 기본셋업
    private void SetUnit()
    {
        this.transform.rotation = Quaternion.identity;
        unitHP = unit.data.UnitHP;
        unitDamage = unit.data.UnitDamage;
        unitSpeed = unit.data.UnitSpeed;
        unitAttackDistance = unit.data.UnitAttackDistance;
        _unitAttackSpeed = unit.data.UnitAttackSpeed;
        _unitSenseDistance = unit.data.UnitSenseRadius;
        unit.agent.enabled = true;
        unit.agent.speed = unit.data.UnitSpeed;
        unit.agent.updateRotation = false;
        unit.agent.updateUpAxis = false;
        if(unit.tag == "Unit") unit.playerUnitManager.AddAllayList(unit);
        unit.unitAnimationOverride.SetAniamtion(unit.data.AnimatorOverrideController);

        unit.rb.drag = 1.0f;
        _currentData = unit.data;
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
            else if(moveDirection < 0 && _isFacingRight)
            {
                Flip();
            }

            _lastPosition = currentPosition;
        }
    }

    void Flip()
    {
        Vector2 curentScale = gameObject.transform.localScale;
        curentScale *= new Vector2( -1, 1 );
        gameObject.transform.localScale = curentScale;

        _isFacingRight = !_isFacingRight;
    }
    #endregion

    #region 전투 관련
    public void UnitAttack()
    {
        unit.attackController.Attack();
    }

    internal void ReceiveDamage(float damageInflict,Unit attacker)
    {
        unitHP -= damageInflict;
        unitHP = Mathf.Max(unitHP, 0);
        if (!_attackers.Contains(attacker))
        {
            _attackers.Add(attacker);
        }
        else
        {
            _attackers.Insert(0, attacker);
        }
        Die();
    }

    internal void Die()
    {
        if( unitHP <= 0 && !isUnitDie)
        {
            isUnitDie = true;
            unit.animator.SetBool("isDie", true);
            StartCoroutine(KnockBack(2));
            unit.detectTarget.ClearTarget();
            _attackers.Clear();
            unit.detectTarget.StopCollider();
            
            if (this.transform.tag == "Unit")
            {
                unit.playerUnitManager.RemoveAllayList(this.unit);
            }
            else
            {
                unit.playerUnitManager.RemoveEnemyList(this.unit);
            }
        }
    }

    internal void Revive()
    {
        this.tag = "Unit";
        unit.detectTarget.StartCollider();
        unit.rb.velocity = Vector2.zero;
        unit.agent.enabled = true;
        unit.animator.SetBool("isDie", false);
        isUnitDie = false;
        unitHP = unit.data.UnitHP;
        unit.animator.Play("IdleState");
        unit.playerUnitManager.AddAllayList(this.unit);
    }

    IEnumerator KnockBack(float amount)
    {
        if (!unit.data.UnitUnstoppable)
        {
            if (amount >= 0)
            {
                unit.agent.enabled = false;
                Vector2 direction = (this.transform.position - _attackers[0].transform.position).normalized;
                unit.rb.AddForce(direction * amount, ForceMode2D.Impulse);
            }
            yield return new WaitForSeconds(0.5f);
            unit.rb.velocity = Vector2.zero;
            unit.agent.enabled = true;
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
