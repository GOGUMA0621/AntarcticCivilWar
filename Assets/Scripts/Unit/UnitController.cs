using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;
using UnityEngine.AI;

public class UnitController : MonoBehaviour
{
    private NavMeshAgent _agent;
    private PlayerController _controller;
    private Animator _animator;
    private Rigidbody2D _rb;
    public UnitData unitData;

    private float unitHP;
    private float unitDamage;
    private float unitSpeed;
    private float unitAttackDistance;

    private void Awake()
    {
        GameObject obj = GameObject.Find("Player");
        _controller = obj.GetComponent<PlayerController>();
        _animator = GetComponent<Animator>();
        _agent = GetComponent<NavMeshAgent>();
        _rb = GetComponent<Rigidbody2D>();
        
        unitHP = unitData.UnitHP;
        unitDamage = unitData.UnitDamage;
        unitSpeed = unitData.UnitSpeed;
        unitAttackDistance = unitData.UnitAttackDistance;

    }

    void Start()
    {
        #region
        _agent.speed = unitData.UnitSpeed;
        _agent.updateRotation = false;
        _agent.updateUpAxis = false;
        #endregion
    }

    void Update()
    {
        _animator.SetFloat("speed",_agent.velocity.magnitude);
        //Debug.Log(_agent.velocity.magnitude);
        if (CompareTag("Unit") && !_animator.GetBool("isFollow") && !_animator.GetBool("isAttack") && _controller != null )
        {
            MoveTo(_controller.playerPos);
        }

        
    }

    private void MoveTo(Vector2 targetPos)
    {
        _agent.SetDestination(targetPos);
    }
}
