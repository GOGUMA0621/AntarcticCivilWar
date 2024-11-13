using System.Collections;
using System.Collections.Generic;
using TMPro.EditorUtilities;
using UnityEditor;
using UnityEngine;

public class UnitAttackController : Unit //유닛 공격 관련
{
    public delegate void UnitAttackEvent(Unit unit);

    public static event UnitAttackEvent OnUnitAttack;

    private GameObject _pfProjectile; //유닛 투사체 프리팹
    private Transform _pfProjectilePos; //투사체 위치값
    private Unit unit;
    protected override void Start()
    {
        base.Start();
        unit = GetComponent<Unit>();
        data = unit.data;
        if(unit.data.unitAttackType == UnitAttackType.Range)
        {
            _pfProjectile = data.UnitProjectile;
            _pfProjectilePos = _pfProjectile.transform;
        }
    }

    internal void Attack()
    {
       OnUnitAttack?.Invoke(GetComponent<Unit>());
        var attackType = unit.data.unitAttackType;
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
                    projectile.GetComponent<ProjectileController>().SetDirection(targetDirection,targetRotation,GetUnit());
                }
            }
        }
    }

    void MeleeAttack()
    {
        Collider2D[] collider = Physics2D.OverlapCircleAll(transform.position, data.UnitSenseRadius);
        foreach (Collider2D targetCollider in collider)
        {
            if (targetCollider.transform == detectTarget.targetToAttack)
            {
                IDamageAble target = targetCollider.GetComponent<IDamageAble>();
                target.ReceiveDamage(unit.unitController.unitDamage);
            }
        }
    }
}
