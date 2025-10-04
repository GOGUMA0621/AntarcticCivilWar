using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManaBigBomb : MonoBehaviour, IActiveSkill
{
    private UnitController unit;
    private Transform targetTransform;
    [SerializeField] private GameObject _pfBigBomb;

    public bool IsDurationSkill => false;

    public bool IsStandingSkill => true;

    public float Duration => 0f;

    [SerializeField] private DamageData[] damageDatas;

    public void ThrowBigBomb()
    {
        DamageData data = damageDatas[unit.unitLevel - 1];

        ProjectileController projectile = Instantiate(_pfBigBomb, transform.position, Quaternion.identity).GetComponent<ProjectileController>();
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
