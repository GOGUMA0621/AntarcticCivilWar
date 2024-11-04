using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class Unit : MonoBehaviour
{
    [HideInInspector] public UnitController controller;
    protected UnitDistinction distinction;
    [HideInInspector] public UnitDetectTarget detectTarget;
    protected UnitAttackController attackController;
    protected UnitAnimationOverride unitAnimationOverride;
    public UnitData data;

    public NavMeshAgent agent;
    protected Rigidbody2D rb;
    protected CircleCollider2D circleCollider;
    protected SpriteRenderer spriteRenderer;
    public PlayerController playerController;
    protected PlayerUnitManager playerUnitManager;
    [HideInInspector] public Animator animator;

    void Awake()
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
        attackController = GetComponentInChildren<UnitAttackController>();
        playerUnitManager = playerController.GetComponent<PlayerUnitManager>();
        unitAnimationOverride = GetComponentInChildren<UnitAnimationOverride>();
    }
    //private void Setup()
    //{
    //    controller = GetComponent<UnitController>();
    //    rb = GetComponent<Rigidbody2D>();
    //    agent = GetComponent<NavMeshAgent>();
    //    circleCollider = GetComponent<CircleCollider2D>();
    //    spriteRenderer = GetComponent<SpriteRenderer>();
    //    animator = GetComponent<Animator>();
    //    player = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
    //    distinction = GetComponentInChildren<UnitDistinction>();
    //    detectTarget = GetComponentInChildren<UnitDetectTarget>();
    //    attackController = GetComponentInChildren<UnitAttackController>();
    //    playerUnitManager = player.GetComponent<PlayerUnitManager>();
    //    unitAnimationOverride = GetComponentInChildren<UnitAnimationOverride>();
    //}
}
