using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class UnitAttackController : MonoBehaviour
{
    private Unit _unit;
    private PolygonCollider2D _attackCollider;
    private bool _isAttacking = false;
    private GameObject _pfProjectile;
    private Transform _pfProjectilePos;
    
    void Start()
    {
        _unit = this.transform.parent.GetComponent<Unit>();
        _attackCollider = this.GetComponent<PolygonCollider2D>();
        _attackCollider.enabled = false;
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
        Debug.Log("АјАн"+ _unit.name);
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
            if (_unit.detectTarget.targetToAttack != null)
            {
                Vector3 targetDirection = _unit.detectTarget.targetToAttack.position - this.transform.position;
                Vector3 targetRotation = this.transform.position - _unit.detectTarget.targetToAttack.position;
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
        if (!_isAttacking)
        {
            _attackCollider.enabled = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (this.transform.tag != other.transform.tag)
        {
            if (other.transform == _unit.detectTarget.targetToAttack)
            {
                other.GetComponent<UnitController>().ReceiveDamage(_unit.controller.unitDamage,_unit);
                _isAttacking = true;
            }
            _isAttacking = false;
            _attackCollider.enabled=false;
        }
    }

}
