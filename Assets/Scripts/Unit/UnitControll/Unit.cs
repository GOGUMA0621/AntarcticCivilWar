using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class Unit : MonoBehaviour //유닛에 대한 부모 파일
{
    [SerializeField]protected UnitController controller;
    public UnitController unitController { get { return controller; } }
    protected UnitDistinction distinction;
    public UnitDistinction unitDistinction { get { return distinction; } }
    [SerializeField]protected UnitDetectTarget detectTarget;
    public UnitDetectTarget unitDetectTarget { get { return detectTarget; } }
    protected UnitAttackController attackController;
    protected UnitAnimationOverride unitAnimationOverride;
    public UnitData data;

    protected NavMeshAgent agent;
    public NavMeshAgent unitAgent {  get { return agent; } }
    protected Rigidbody2D rb;
    protected CircleCollider2D circleCollider;
    protected SpriteRenderer spriteRenderer;
    public PlayerController playerController;
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
        detectTarget = GetComponent<UnitDetectTarget>();
        attackController = GetComponent<UnitAttackController>();
        playerUnitManager = playerController.GetComponent<PlayerUnitManager>();
        unitAnimationOverride = GetComponent<UnitAnimationOverride>();
    }
    public Unit GetUnit() { return this; }
    public UnitData GetData() { return data; }
}
