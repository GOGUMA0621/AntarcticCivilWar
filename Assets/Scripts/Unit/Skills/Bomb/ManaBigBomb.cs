using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManaBigBomb : MonoBehaviour, IActiveSkill
{
    private Unit unit;
    private Transform targetTransform;
    [SerializeField] private GameObject _pfBigBomb;

    void Start()
    {
      unit = GetComponent<Unit>();
      if (unit.detectTarget.targetToAttack is Component comp)
          targetTransform = comp.transform;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ThrowBigBomb()
    {
        ProjectileController projectile = Instantiate(_pfBigBomb, transform.position, Quaternion.identity).GetComponent<ProjectileController>();
        projectile.InitialzeProjectile(targetTransform, unit.data.UnitMaxProjectileSpeed, unit.data.UnitMaxProjectileHeight, unit);
        projectile.InitializeAnimaionCurve(unit.data.ProjectileTrajectoryAnimationCurve, unit.data.ProjectileCorrectionAnimationCurve, unit.data.ProjectileSpeedAnimationCurve);
    }

    public void DoActiveSkill()
    {
        unit.animator.Play("ManaSkill");
    }
}
