using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Snipe : MonoBehaviour, IActiveSkill
{
    private UnitController unit;
    private Transform targetTransform;
    [SerializeField] private GameObject _pfbolt;

    public bool IsDurationSkill => false;
    public bool IsStandingSkill => true;
    public float Duration => 0f;

    [SerializeField] private DamageData[] damageDatas;

    // 레벨별 계수 (230%, 300%, 420%)
    private static readonly float[] snipeDamageMultiplier = { 2.3f, 3.0f, 4.2f };

    public void FireSnipeProjectile()
    {
        var target = unit.unit.detectTarget.targetToAttack as IDamageAble;
        if (target == null || target.IsDestroyed())
        {
            DeactivateSkill(unit);
            return;
        }

        int levelIdx = Mathf.Clamp(unit.unitLevel - 1, 0, snipeDamageMultiplier.Length - 1);
        float baseDamage = unit.UnitStats.attackDamage;
        float snipeDamage = baseDamage * snipeDamageMultiplier[levelIdx];

        DamageData data = new DamageData(snipeDamage, StatusEffectType.Physical, 0);
        ProjectileController projectile = Instantiate(_pfbolt, transform.position, Quaternion.identity).GetComponent<ProjectileController>();
        projectile.InitializeDamageData(data);
        projectile.SetTarget(targetTransform);
        projectile.InitializeProjectile(targetTransform, unit.unit.data.UnitMaxProjectileSpeed, unit.unit.data.UnitMaxProjectileHeight, unit.unit);
        projectile.InitializeAnimaionCurve(unit.unit.data.ProjectileTrajectoryAnimationCurve, unit.unit.data.ProjectileCorrectionAnimationCurve, unit.unit.data.ProjectileSpeedAnimationCurve);

        DeactivateSkill(unit);
    }

    public void ActivateSkill(UnitController unit)
    {
        this.unit = unit;
        targetTransform = unit.unit.detectTarget.targetToAttack.GetTransform();
    }


    public void DeactivateSkill(UnitController unit)
    {
        unit.isSkillActive = false;
    }
}
