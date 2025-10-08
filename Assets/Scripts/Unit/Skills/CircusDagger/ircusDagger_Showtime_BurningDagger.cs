using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CircusDagger_Showtime_BurningDagger : MonoBehaviour, IShowtime
{
    private UnitController unit;
    [SerializeField] private AnimationClip burning;
    [SerializeField] private float burningDuration = 5f;
    private AnimationClip originalClip;
    

    [SerializeField] private GameObject burningDaggerPrefab;

    [SerializeField] private DamageData[] damageDatas;

    private void Start()
    {
        unit = GetComponent<UnitController>();
    }

    public void BurningDaggerAttack()
    {
        GameObject daggerObject = Instantiate(burningDaggerPrefab, transform.position, Quaternion.identity);
        ProjectileController dagger = daggerObject.GetComponent<ProjectileController>();
        dagger.SetTarget(unit.unit.detectTarget.targetToAttack.GetTransform());
        dagger.InitializeProjectile(unit.unit.detectTarget.targetToAttack.GetTransform(), unit.unit.data.UnitMaxProjectileSpeed, unit.unit.data.UnitMaxProjectileHeight, unit.unit);
        dagger.InitializeAnimaionCurve(unit.unit.data.ProjectileTrajectoryAnimationCurve, unit.unit.data.ProjectileCorrectionAnimationCurve, unit.unit.data.ProjectileSpeedAnimationCurve);
        dagger.InitializeDamageData(damageDatas[unit.unitLevel - 1]);
    }

    public void StartShowtimeSkill()
    {
        var animator = unit.unit.animator;
        var ovverrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
        originalClip = (AnimationClip)ovverrideController["AttackState"];

        ovverrideController["AttackState"] = burning;
        animator.runtimeAnimatorController = ovverrideController;
        StartCoroutine(BurningDaggerRoutine());
    }
    public void EndShowtimeSkill()
    {
        var animator = unit.unit.animator;
        var ovverrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);

        ovverrideController["AttackState"] = originalClip;
        animator.runtimeAnimatorController = ovverrideController;
    }
    
    private IEnumerator BurningDaggerRoutine()
    {
        yield return new WaitForSeconds(burningDuration);
        EndShowtimeSkill();
        unit.isSkillActive = false;
        unit.canMana = true;
    }

}
