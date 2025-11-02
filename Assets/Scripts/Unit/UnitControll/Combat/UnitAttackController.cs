using System;
using System.Collections.Generic;
using UnityEngine;

public class UnitAttackController : MonoBehaviour
{
    public Action<Transform> OnAttackTransform;
    private List<ProjectileController> activeProjectiles = new List<ProjectileController>();

    // 기존 단일 프리팹 유지(호환용)
    public GameObject pfProjectile;

    // 추가: 여러 발사체를 지원하는 풀과 발사 모드
    public List<GameObject> projectilePrefabs = new List<GameObject>();

    public enum ProjectileFireMode { Random, Sequential }
    public ProjectileFireMode projectileFireMode = ProjectileFireMode.Random;

    private int nextProjectileIndex = 0;

    private Unit unit;
    private void Start()
    {
        unit = GetComponent<Unit>();
        if (unit.data.unitAttackType == UnitAttackType.Range)
        {
            // 기존 단일 프리팹을 기본으로 사용하되, 풀에 없으면 추가
            pfProjectile = unit.data.UnitProjectile;
            if (pfProjectile != null && (projectilePrefabs == null || projectilePrefabs.Count == 0))
            {
                projectilePrefabs = new List<GameObject> { pfProjectile };
            }
        }
    }
    /// <summary>
    /// 공격 실행
    /// </summary>
    internal void Attack()
    {
        // 데미지 데이터 생성
        DamageData damageData = new DamageData(unit.controller.UnitStats.attackDamage, StatusEffectType.Physical, 0);
        if (IsCritical(unit.controller.UnitStats.critChance))
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
                case UnitAttackType.Melee: // 근접 공격
                    MeleeAttack(damageData);
                    break;

                case UnitAttackType.Range: // 원거리 공격
                    RangeAttack(damageData);
                    break;

            }
        }
    }

    /// <summary>
    /// 다음 발사체 프리팹을 선택합니다.
    /// </summary>
    /// <returns>선택된 발사체 프리팹</returns>
    private GameObject GetNextProjectilePrefab()
    {
        if (projectilePrefabs != null && projectilePrefabs.Count > 0)
        {
            if (projectileFireMode == ProjectileFireMode.Random) // 무작위
            {
                int idx = UnityEngine.Random.Range(0, projectilePrefabs.Count);
                return projectilePrefabs[idx];
            }
            else // 순차적
            {
                var prefab = projectilePrefabs[nextProjectileIndex % projectilePrefabs.Count];
                nextProjectileIndex = (nextProjectileIndex + 1) % projectilePrefabs.Count;
                return prefab;
            }
        }
        // 폴백: 기존 단일 프리팹 사용
        return pfProjectile;
    }
    /// <summary>
    /// 원거리 공격 처리
    /// </summary>
    /// <param name="damageData">공격에 대한 데미지 데이터</param>
    void RangeAttack(DamageData damageData = null)
    {
        var chosenPrefab = GetNextProjectilePrefab();
        if (chosenPrefab != null)
        {
            if (unit.detectTarget.targetToAttack != null)
            {
                IDamageAble target = unit.detectTarget.targetToAttack;
                Transform targetTransform = target.GetTransform();
                GameObject projectileObject = Instantiate(chosenPrefab, transform.position, Quaternion.identity);
                projectileObject.SetActive(true);
                ProjectileController projectile = projectileObject.GetComponent<ProjectileController>();
                projectile.InitializeProjectile(targetTransform, unit.data.UnitMaxProjectileSpeed, unit.data.UnitMaxProjectileHeight, unit, this);
                projectile.InitializeDamageData(damageData);
                projectile.InitializeAnimaionCurve(unit.data.ProjectileTrajectoryAnimationCurve,
                                                    unit.data.ProjectileCorrectionAnimationCurve, unit.data.ProjectileSpeedAnimationCurve);
                AddProjectile(projectile);
                projectile.SetOnHitCallback(() => { unit.controller.TriggerOnHit(target); });
            }
        }
    }
    /// <summary>
    /// 근접 공격 처리
    /// </summary>
    /// <param name="damageData">공격에 대한 데미지 데이터</param>
    void MeleeAttack(DamageData damageData = null)
    {
        IDamageAble target = unit.detectTarget.targetToAttack;
        if (target != null && !target.IsDestroyed())
        {
            // 근접 공격 전 거리 재확인 (안전장치)
            float distance = Vector2.Distance(transform.position, target.GetTransform().position);
            float attackRange = unit.controller.UnitStats.attackRange;
            
            if (distance <= attackRange + 0.5f) // 약간의 여유 거리 추가
            {
                target.ReceiveDamage(damageData);
                unit.controller.TriggerOnHit(target);
                Debug.Log($"{unit.controller.name}: 근접 공격 성공! 거리: {distance:F2}, 사거리: {attackRange:F2}");
            }
            else
            {
                Debug.LogWarning($"{unit.controller.name}: 근접 공격 실패 - 거리가 너무 멀음! 거리: {distance:F2}, 사거리: {attackRange:F2}");
            }
        }
        else
        {
            Debug.LogWarning($"{unit.controller.name}: 근접 공격 실패 - 타겟이 없거나 파괴됨!");
        }
    }

    public void ResetProjectile()
    {
        pfProjectile = unit.data.UnitProjectile;
        nextProjectileIndex = 0;
    }

    // 기존 SetProjectile 유지 + 풀을 설정하는 새 오버로드
    public void SetProjectile(GameObject projectile)
    {
        pfProjectile = projectile;
        projectilePrefabs = new List<GameObject> { projectile };
        nextProjectileIndex = 0;
    }

    public void SetProjectiles(List<GameObject> projectiles, ProjectileFireMode mode = ProjectileFireMode.Random)
    {
        projectilePrefabs = projectiles ?? new List<GameObject>();
        projectileFireMode = mode;
        nextProjectileIndex = 0;
    }

    public bool IsCritical(float criticalChance)
    {
        // criticalChance: 0~1 사이 값 (예: 0.25f = 25% 확률)
        return UnityEngine.Random.value < criticalChance;
    }

    public void RemoveProjectile(ProjectileController projectile)
    {
        if (activeProjectiles.Contains(projectile))
        {
            activeProjectiles.Remove(projectile);
        }
    }
    
    public void AddProjectile(ProjectileController projectile)
    {
        if (!activeProjectiles.Contains(projectile))
        {
            activeProjectiles.Add(projectile);
        }
    }
}
