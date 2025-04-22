using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class UnitAttackController : MonoBehaviour //À¯´Ö °ø°Ý °ü·Ã
{
    public delegate void UnitAttackEvent(Transform t);

    public static event UnitAttackEvent OnUnitAttack;


    public GameObject pfProjectile; //À¯´Ö Åõ»çÃ¼ ÇÁ¸®ÆÕ
    
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
            OnUnitAttack?.Invoke(GetComponent<Transform>());
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
                projectile.SetOnHitCallback(() => { unit.controller.TriggerOnHit(target); });
                projectile.InitialzeProjectile(unit.detectTarget.targetToAttack, unit.data.UnitMaxProjectileSpeed, unit.data.UnitMaxProjectileHeight,unit);
                projectile.InitializeAnimaionCurve(unit.data.ProjectileTrajectoryAnimationCurve, unit.data.ProjectileCorrectionAnimationCurve, unit.data.ProjectileSpeedAnimationCurve);
            }
        }
    }

    void MeleeAttack()
    {
        Collider2D[] collider = Physics2D.OverlapCircleAll(transform.position, unit.data.UnitSenseRadius);
        foreach (Collider2D targetCollider in collider)
        {
            if (targetCollider.transform == unit.detectTarget.targetToAttack)
            {
                IDamageAble target = targetCollider.GetComponent<IDamageAble>();
                target.ReceiveDamage(new DamageData(unit.data.UnitDamage,StatusEffectType.None,0));
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
