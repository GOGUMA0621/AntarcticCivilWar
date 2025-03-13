using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class Unit : MonoBehaviour //유닛에 대한 부모 파일
{
    protected UnitController controller;
    public UnitController unitController { get { return controller; } }
    protected UnitDistinction distinction;
    public UnitDistinction unitDistinction { get { return distinction; } }
    protected UnitDetectTarget detectTarget;
    public UnitDetectTarget unitDetectTarget { get { return detectTarget; } }
    protected UnitAttackController attackController;
    public UnitAttackController unitAttackController { get { return attackController; } }
    public SciptableObjects.UnitData data;

    protected NavMeshAgent agent;
    public NavMeshAgent unitAgent {  get { return agent; } }
    protected Rigidbody2D rb;
    protected CircleCollider2D circleCollider;
    protected SpriteRenderer spriteRenderer;
    [HideInInspector] public PlayerController playerController;
    protected PlayerUnitManager playerUnitManager;
    protected Animator animator;
    public Animator unitAnimator {  get { return animator; } }

    protected virtual void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        controller = GetComponent<UnitController>();
        rb = GetComponent<Rigidbody2D>();
        circleCollider = GetComponent<CircleCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        playerController = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
        distinction = GetComponentInChildren<UnitDistinction>();
        detectTarget = GetComponentInChildren<UnitDetectTarget>();
        attackController = GetComponent<UnitAttackController>();
        playerUnitManager = playerController.GetComponent<PlayerUnitManager>();
    }
    public Unit GetUnit() { return this; }
    public SciptableObjects.UnitData GetData() { return data; }
}
