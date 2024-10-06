using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Unit : MonoBehaviour
{
    internal UnitController controller;
    internal UnitDistinction distinction;
    internal UnitDetectTarget detectTarget;
    internal UnitAttackController attackController;
    internal UnitAnimationOverride unitAnimationOverride;
    public CircleCollider2D detectRadiusCollider;
    public UnitData data;

    internal NavMeshAgent agent;
    internal Rigidbody2D rb;
    internal CircleCollider2D circleCollider;
    internal SpriteRenderer spriteRenderer;
    internal PlayerController player;
    internal PlayerUnitManager playerUnitManager;
    internal Animator animator;
    private bool _isSetup = false;
    public bool isSetup { get { return _isSetup; } }

    private void Start()
    {
        controller = GetComponent<UnitController>();
        rb = GetComponent<Rigidbody2D>();
        agent = GetComponent<NavMeshAgent>();
        circleCollider = GetComponent<CircleCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        player = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
        distinction = GetComponentInChildren<UnitDistinction>();
        detectTarget = GetComponentInChildren<UnitDetectTarget>();
        attackController = GetComponentInChildren<UnitAttackController>();
        playerUnitManager = player.GetComponent<PlayerUnitManager>();
        unitAnimationOverride = GetComponentInChildren<UnitAnimationOverride>();
        _isSetup = true;
    }

}
