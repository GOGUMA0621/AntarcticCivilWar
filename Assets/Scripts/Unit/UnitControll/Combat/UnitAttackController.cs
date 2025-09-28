using System;
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
        DamageData damageData = new DamageData(unit.controller.UnitStats.attackDamage, StatusEffectType.Physical, 0);
        if(IsCritical(unit.controller.UnitStats.critChance))
        {
            damageData.damage *= unit.controller.UnitStats.critDamage;
            // 크리티컬 효과 추가 처리
        }

        if (unit.detectTarget.targetToAttack != null)
        {
            OnAttackTransform?.Invoke(GetComponent<Transform>());
            var attackType = unit.data.unitAttackType;
            switch (attackType)
            {
                case UnitAttackType.Melee:
                    MeleeAttack(damageData);
                    break;

                case UnitAttackType.Range:
                    RangeAttack(damageData);
                    break;

            }
        }
    }

    void RangeAttack(DamageData damageData = null)
    {
        if(pfProjectile != null)
        {
            if (unit.detectTarget.targetToAttack != null)
            {
                IDamageAble target = unit.detectTarget.targetToAttack;
                Transform targetTransform = target.GetTransform();
                GameObject projectileObject = Instantiate(pfProjectile, transform.position, Quaternion.identity);
                projectileObject.SetActive(true);
                ProjectileController projectile = projectileObject.GetComponent<ProjectileController>();
                projectile.InitialzeProjectile(targetTransform, unit.data.UnitMaxProjectileSpeed, unit.data.UnitMaxProjectileHeight,unit);
                projectile.InitializeAnimaionCurve(unit.data.ProjectileTrajectoryAnimationCurve, unit.data.ProjectileCorrectionAnimationCurve, unit.data.ProjectileSpeedAnimationCurve);
                projectile.SetOnHitCallback(() => { unit.controller.TriggerOnHit(target); });
            }
        }
    }

    void MeleeAttack(DamageData damageData = null)
    {
        IDamageAble target = unit.detectTarget.targetToAttack;
        Transform targetTransform = target.GetTransform();

        Collider2D[] collider = Physics2D.OverlapCircleAll(transform.position, unit.controller.UnitStats.attackRange);
        foreach (Collider2D targetCollider in collider)
        {
            if (targetCollider.transform == targetTransform)
            {
                target.ReceiveDamage(damageData);
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

    public bool IsCritical(float criticalChance)
    {
        // criticalChance: 0~1 사이 값 (예: 0.25f = 25% 확률)
        return UnityEngine.Random.value < criticalChance;
    }
}
