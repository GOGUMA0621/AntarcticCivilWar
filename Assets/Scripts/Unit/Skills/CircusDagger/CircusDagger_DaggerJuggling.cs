using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircusDagger_DaggerJuggling : MonoBehaviour, IActiveSkill
{
    [SerializeField] private float skillDuration = 5f;
    [SerializeField] private GameObject daggerPrefab;
    private UnitController unit;

    private DamageData[] damageDatas;

    public bool IsDurationSkill => true;

    public bool IsStandingSkill => true;

    public float Duration => skillDuration;

    public void DaggerJuggling()
    {
        ProjectileController dagger = Instantiate(daggerPrefab, transform.position, Quaternion.identity).GetComponent<ProjectileController>();
        dagger.SetTarget(unit.unit.detectTarget.targetToAttack.GetTransform());
        dagger.InitializeProjectile(unit.unit.detectTarget.targetToAttack.GetTransform(), unit.unit.data.UnitMaxProjectileSpeed, unit.unit.data.UnitMaxProjectileHeight, unit.unit);
        dagger.InitializeAnimaionCurve(unit.unit.data.ProjectileTrajectoryAnimationCurve, unit.unit.data.ProjectileCorrectionAnimationCurve, unit.unit.data.ProjectileSpeedAnimationCurve); 
        dagger.InitializeDamageData(damageDatas[unit.unitLevel - 1]);
    }

    public void ActivateSkill(UnitController unit)
    {
        this.unit = unit;
        StartCoroutine(DaggerJugglingRoutine());
    }

    public void DeactivateSkill(UnitController unit)
    {
        unit.isSkillActive = false;
        unit.canMana = true;
    }

    private IEnumerator DaggerJugglingRoutine()
    {
        yield return new WaitForSeconds(skillDuration);
        DeactivateSkill(unit);
    }
}
