using System;
using Unity.VisualScripting;
using UnityEngine;

public class UnitAttackController : MonoBehaviour 
{
    public Action<Transform> OnAttackTransform; 

    public GameObject pfProjectile;
    
    private Unit unit;
    private void Start()
    {
        unit = GetComponent<Unit>();
        if(unit.data.unitAttackType == UnitAttackType.Range)
        {
            pfProjectile = unit.data.UnitProjectile;
        }
    }

    internal void Attack()
    {
        if (!unit.detectTarget.IsDestroyed())
        {
            OnAttackTransform?.Invoke(GetComponent<Transform>());
            var attackType = unit.data.unitAttackType;
            switch (attackType)
            {
                case UnitAttackType.Melee:
                    MeleeAttack();
                    break;

                case UnitAttackType.Range:
                    RangeAttack();
                    break;

            }
        }
    }

    void RangeAttack()
    {
        if(pfProjectile != null)
        {
            if (unit.detectTarget.targetToAttack != null)
            {
                IDamageAble target = unit.detectTarget.targetToAttack.GetComponent<IDamageAble>();
                GameObject projectileObject = Instantiate(pfProjectile, transform.position, Quaternion.identity);
                //Debug.Log(projectileObject);
                projectileObject.SetActive(true);
                ProjectileController projectile = projectileObject.GetComponent<ProjectileController>();
                projectile.InitialzeProjectile(unit.detectTarget.targetToAttack, unit.data.UnitMaxProjectileSpeed, unit.data.UnitMaxProjectileHeight,unit);
                projectile.InitializeAnimaionCurve(unit.data.ProjectileTrajectoryAnimationCurve, unit.data.ProjectileCorrectionAnimationCurve, unit.data.ProjectileSpeedAnimationCurve);
                projectile.SetOnHitCallback(() => { unit.controller.TriggerOnHit(target); });
            }
        }
    }

    void MeleeAttack()
    {
        Collider2D[] collider = Physics2D.OverlapCircleAll(transform.position, unit.controller.unitAttackDistance);
        foreach (Collider2D targetCollider in collider)
        {
            if (targetCollider.transform == unit.detectTarget.targetToAttack)
            {
                IDamageAble target = targetCollider.GetComponent<IDamageAble>();
                target.ReceiveDamage(new DamageData(unit.controller.unitDamage,StatusEffectType.None,0));
                unit.controller.TriggerOnHit(target);
            }
        } 
    }

    public void ResetProjectile()
    {
        pfProjectile = unit.data.UnitProjectile;
    }

    public void SetProjectile(GameObject projectile)
    {
        pfProjectile = projectile;
    }
}
