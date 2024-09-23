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
    private NavMeshAgent _agent;
    private PlayerController _playerController;
    public PlayerController PlayerController { get { return _playerController; } }
    private Animator _animator;
    private Rigidbody2D _rb;
    private CircleCollider2D _detectRadiusCollider;
    private SpriteRenderer _spriteRenderer;
    public UnitDetectTarget detectTarget;
    public UnitData unitData;

    private Vector2 _lastPosition;
    private bool isUnitDie = false;
    private float unitHP;
    [SerializeField] private List <UnitController> _attackers;
    [HideInInspector] public float UnitHP;
    public float unitDamage;
    public float unitSpeed;
    public float unitAttackDistance;
    public float unitAttackSpeed;

    private void Awake()
    {
        GameObject obj = GameObject.Find("Player");
        _playerController = obj.GetComponent<PlayerController>();
        _animator = GetComponent<Animator>();
        _agent = GetComponent<NavMeshAgent>();
        _rb = GetComponent<Rigidbody2D>();
        _detectRadiusCollider = GetComponentInChildren<CircleCollider2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        detectTarget = GetComponentInChildren<UnitDetectTarget>();
        
        _attackers = new List<UnitController>();
        unitHP = unitData.UnitHP;
        unitDamage = unitData.UnitDamage;
        unitSpeed = unitData.UnitSpeed;
        unitAttackDistance = unitData.UnitAttackDistance;
        unitAttackSpeed = unitData.UnitAttackSpeed;
    }

    void Start()
    {
        _agent.enabled = true;
        #region
        _agent.speed = unitData.UnitSpeed;
        _agent.updateRotation = false;
        _agent.updateUpAxis = false;
        #endregion
        _lastPosition = transform.position;
        _rb.drag = 1.0f;
    }

    void Update()
    {
        _detectRadiusCollider.radius = unitData.UnitSenseRadius;

        FlipAnimation();

        _animator.SetFloat("speed", _agent.velocity.magnitude);
    }

    public void MoveTo(Vector2 targetPos)
    {
        _agent.SetDestination(targetPos);
    }

    public void FlipAnimation()
    {
        AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
        Vector2 currentPosition = transform.position;
        //if (stateInfo.IsName("AttackState"))
        //{
        //    float targetDirection = currentPosition.x - detectTarget.targetToAttack.transform.position.x;
        //}
        float moveDirection = currentPosition.x - _lastPosition.x;

        if (moveDirection > 0)
        {
            _spriteRenderer.flipX = false;
        }
        else if(moveDirection < 0)
        {
            _spriteRenderer.flipX = true;
        }

        _lastPosition = currentPosition;
    }

    internal void ReceiveDamage(float damageInflict, UnitController attacker)
    {
        unitHP -= damageInflict;

        if (_attackers == null || !_attackers.Contains(attacker))
        {
            _attackers.Add(attacker);
            //Debug.Log("공격 당함" + attacker.name);
        }
        else if (_attackers.Contains(attacker) && _attackers[0] != attacker)
        {
            _attackers.Insert(0, attacker);
        }
        Die();
    }

    internal void Die()
    {
        if( unitHP <= 0 && !isUnitDie)
        {
            unitHP = 0;
            isUnitDie = true;
            _animator.SetBool("isDie", true);
            
            _agent.enabled = false;
            Vector2 direction = (this.transform.position - _attackers[0].transform.position).normalized;
            _rb.AddForce(direction * 2.0f, ForceMode2D.Impulse);


            foreach (UnitController attacker in _attackers)
            {
                attacker.detectTarget.RemoveTarget(this);
            }
            _attackers.Clear();
        }
    }

    //internal void 

    internal void Revive()
    {
        _rb.velocity = Vector2.zero;
        _agent.enabled = true;
        _animator.SetBool("isDie", false);
        detectTarget.ClearTarget();
        isUnitDie = false;
        this.tag = "Unit";
        unitHP = unitData.UnitHP;
        _animator.Play("IdleState");
        
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(this.transform.position , unitData.UnitSenseRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(this.transform.position, unitData.UnitAttackDistance);
    }

}
