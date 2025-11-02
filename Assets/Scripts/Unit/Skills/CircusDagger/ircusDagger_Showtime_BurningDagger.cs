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
        
        // DamageData 배열이 비어있으면 기본값 생성
        if (damageDatas == null || damageDatas.Length == 0)
        {
            Debug.LogWarning($"{gameObject.name}: DamageData 배열이 비어있어 기본값을 생성합니다.");
            CreateDefaultDamageData();
        }
    }
    
    /// <summary>
    /// 기본 DamageData 배열 생성 - 단계별 공격력 증가 시스템
    /// </summary>
    private void CreateDefaultDamageData()
    {
        // 3단계 레벨 시스템
        int maxLevel = 3;
        damageDatas = new DamageData[maxLevel];
        
        // 유닛의 기본 공격력 가져오기
        float baseDamage = 100f; // 기본값
        if (unit != null && unit.unit != null && unit.unit.data != null)
        {
            int tierIndex = Mathf.Clamp(unit.unit.data.UnitTier - 1, 0, 2);
            baseDamage = unit.unit.data.UnitDamage[tierIndex];
        }
        
        // 각 단계별 DamageData 생성 (모든 단계에서 출혈 효과 적용)
        // 1단계: 20% 공격력 + 출혈
        damageDatas[0] = new DamageData(baseDamage * 0.2f, StatusEffectType.Bleed, 3.0f);
        
        // 2단계: 30% 공격력 + 출혈
        damageDatas[1] = new DamageData(baseDamage * 0.3f, StatusEffectType.Bleed, 4.0f);
        
        // 3단계: 150% 공격력 + 출혈
        damageDatas[2] = new DamageData(baseDamage * 1.5f, StatusEffectType.Bleed, 5.0f);
        
        Debug.Log($"{gameObject.name}: 단계별 DamageData 시스템 생성 완료");
        Debug.Log($"1단계: {baseDamage * 0.2f} 데미지 + 출혈 3초");
        Debug.Log($"2단계: {baseDamage * 0.3f} 데미지 + 출혈 4초");
        Debug.Log($"3단계: {baseDamage * 1.5f} 데미지 + 출혈 5초");
    }

    public void BurningDaggerAttack()
    {
        // 배열 인덱스 안전성 검사
        int damageIndex = unit.unitLevel - 1;
        if (damageDatas == null || damageDatas.Length == 0)
        {
            Debug.LogWarning("DamageData 배열이 비어있어 스킬을 실행할 수 없습니다!");
            return;
        }
        
        // 안전한 인덱스 계산
        int safeIndex = Mathf.Clamp(damageIndex, 0, damageDatas.Length - 1);
        DamageData currentDamage = damageDatas[safeIndex];
        
        // 단계별 효과 로그
        string stageInfo = GetStageInfo(safeIndex + 1);
        Debug.Log($"번닝 대거 공격! {stageInfo}");
        
        // 단검 프로젝타일 생성
        GameObject daggerObject = Instantiate(burningDaggerPrefab, transform.position, Quaternion.identity);
        ProjectileController dagger = daggerObject.GetComponent<ProjectileController>();
        
        // 타겟 설정 및 초기화
        Transform targetTransform = unit.unit.detectTarget.targetToAttack.GetTransform();
        dagger.SetTarget(targetTransform);
        dagger.InitializeProjectile(targetTransform, unit.unit.data.UnitMaxProjectileSpeed, unit.unit.data.UnitMaxProjectileHeight, unit.unit);
        dagger.InitializeAnimaionCurve(unit.unit.data.ProjectileTrajectoryAnimationCurve, unit.unit.data.ProjectileCorrectionAnimationCurve, unit.unit.data.ProjectileSpeedAnimationCurve);
        
        // 단계별 데미지 데이터 적용 (모든 단계에서 출혈 효과 포함)
        dagger.InitializeDamageData(currentDamage);
    }
    
    /// <summary>
    /// 단계별 정보를 반환
    /// </summary>
    /// <param name="stage">단계 (1~3)</param>
    /// <returns>단계 정보 문자열</returns>
    private string GetStageInfo(int stage)
    {
        return stage switch
        {
            1 => "1단계: 20% 공격력 + 출혈 3초",
            2 => "2단계: 30% 공격력 + 출혈 4초", 
            3 => "3단계: 150% 공격력 + 출혈 5초",
            _ => "기본 단계"
        };
    }

    public void StartShowtimeSkill()
    {
        var animator = unit.unit.animator;
        var ovverrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
        originalClip = (AnimationClip)ovverrideController["AttackState"];

        ovverrideController["AttackState"] = burning;
        animator.runtimeAnimatorController = ovverrideController;
        
        // 단계별 스킬 지속시간 조정
        float skillDuration = GetSkillDuration();
        Debug.Log($"번닝 대거 스킬 시작! 지속시간: {skillDuration}초");
        
        StartCoroutine(BurningDaggerRoutine(skillDuration));
    }
    
    /// <summary>
    /// 유닛 레벨에 따른 스킬 지속시간 반환
    /// </summary>
    /// <returns>스킬 지속시간</returns>
    private float GetSkillDuration()
    {
        int level = unit.unitLevel;
        return level switch
        {
            1 => 3.0f, // 1단계: 3초
            2 => 4.0f, // 2단계: 4초  
            3 => 5.0f, // 3단계: 5초 (이미지에서 5초 동안)
            _ => burningDuration // 기본값
        };
    }
    public void EndShowtimeSkill()
    {
        var animator = unit.unit.animator;
        var ovverrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);

        ovverrideController["AttackState"] = originalClip;
        animator.runtimeAnimatorController = ovverrideController;
    }
    
    private IEnumerator BurningDaggerRoutine(float duration)
    {
        yield return new WaitForSeconds(duration);
        EndShowtimeSkill();
        unit.isSkillActive = false;
        unit.canMana = true;
        
        Debug.Log($"번닝 대거 스킬 종료! (지속시간: {duration}초)");
    }

}
