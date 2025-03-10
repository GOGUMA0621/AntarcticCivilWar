using System.Collections;
using System.Collections.Generic;
using TMPro.EditorUtilities;
using UnityEditor;
using UnityEngine;

public class UnitAttackController : Unit //À¯´Ö °ø°Ý °ü·Ã
{
    public delegate void UnitAttackEvent(Transform t);

    public static event UnitAttackEvent OnUnitAttack;

    public GameObject pfProjectile; //À¯´Ö Åõ»çÃ¼ ÇÁ¸®ÆÕ
    
    private Unit unit;
    protected override void Start()
    {
        base.Start();
        unit = GetComponent<Unit>();
        data = unit.data;
        if(unit.data.unitAttackType == UnitAttackType.Range)
        {
            pfProjectile = data.UnitProjectile;
        }
    }

    internal void Attack()
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

    void RangeAttack()
    {
        if(pfProjectile != null)
        {
            if (detectTarget.targetToAttack != null)
            {
                ProjectileController projectile = Instantiate(pfProjectile, transform.position, Quaternion.identity).GetComponent<ProjectileController>();
                projectile.InitialzeProjectile(detectTarget.targetToAttack, data.UnitMaxProjectileSpeed, data.UnitMaxProjectileHeight,unit);
                projectile.InitializeAnimaionCurve(data.ProjectileTrajectoryAnimationCurve, data.ProjectileCorrectionAnimationCurve, data.ProjectileSpeedAnimationCurve);
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
                target.ReceiveDamage(new DamageData(unit.data.UnitDamage,StatusEffectType.None,0));
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
