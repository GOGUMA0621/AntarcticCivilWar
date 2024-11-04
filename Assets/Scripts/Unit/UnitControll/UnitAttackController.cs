using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class UnitAttackController : Unit
{
    private Unit _unit;
    private bool _isAttacking = false;
    private GameObject _pfProjectile;
    private Transform _pfProjectilePos;
    
    void Start()
    {
        _unit = transform.parent.GetComponent<Unit>();
        if(_unit.data.unitAttackType == UnitAttackType.Range)
        {
            _pfProjectile = _unit.data.UnitProjectile;
            _pfProjectilePos = _pfProjectile.transform;
        }
    }

    private void FixedUpdate()
    {
        if (_unit.detectTarget.targetToAttack != null)
        {
            Vector2 targetDirection = _unit.detectTarget.targetToAttack.position - this.transform.position;

            float rotZ = Mathf.Atan2(targetDirection.y, targetDirection.x)*Mathf.Rad2Deg;

            transform.rotation = Quaternion.Euler(0, 0, rotZ);
        }
    }

    internal void Attack()
    {
        Debug.Log("АјАн"+ name);
        var attackType = _unit.data.unitAttackType;
        switch (attackType)
        {
            case (UnitAttackType.Melee):
                MeleeAttack();
                break;

            case (UnitAttackType.Range):
                RangeAttack();
                break;

        }

    }

    void RangeAttack()
    {
        if(_pfProjectile != null)
        {
            if (detectTarget.targetToAttack != null)
            {
                Vector3 targetDirection = detectTarget.targetToAttack.position - this.transform.position;
                Vector3 targetRotation = this.transform.position - detectTarget.targetToAttack.position;
                GameObject projectile = Instantiate(_pfProjectile,this.transform.position,Quaternion.identity);
                if (projectile != null)
                {
                    projectile.GetComponent<ProjectileController>().SetDirection(targetDirection,targetRotation,_unit);
                }
            }
        }
    }

    void MeleeAttack()
    {
        Collider2D[] collider = Physics2D.OverlapCircleAll(transform.position, _unit.data.UnitSenseRadius);
        foreach (Collider2D targetCollider in collider)
        {
            if (targetCollider.transform == _unit.detectTarget.targetToAttack)
            {
                if(targetCollider.TryGetComponent<Unit>(out Unit targetUnit))
                {
                    targetUnit.controller.ReceiveDamage(_unit.controller.unitDamage, _unit);
                }
            }
        }
    }
}
