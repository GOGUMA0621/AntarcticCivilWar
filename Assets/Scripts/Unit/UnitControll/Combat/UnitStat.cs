
using System;
using System.Collections.Generic;

public enum StatType
{
    MaxHealth, HealthRegen,
    MaxMana, ManaGain, ManaRegen,
    AttackDamage, AttackSpeed, AttackRange,
    AdditionalDamage,
    MoveSpeed,
    AOEDamage,
    CritChance, CritDamage,
    Endurance, //피해 감소
    DamageAmp, //데미지 증가
    Pengforce //마법 공격력 개념
}

public enum ModifierMethod
{
    Additive, //덧셈
    Multiplicative, //곱셈
    AdditivePercent, //덧셈 퍼센트
    MultiplicativePercent, //곱셈 퍼센트
}

public class StatModifier
{
    public string sourceId { get; } //어떤 오브젝트에서 온 버프인지 구분하기 위한 아이디
    public StatType statType { get; } //어떤 스탯에 영향을 주는지
    public float value { get; } //얼마나 영향을 주는지
    public ModifierMethod modifierMethod { get; } //어떤 방식으로 영향을 주는지

    /// <summary>
    /// 스탯 버프/디버프를 생성합니다.
    /// </summary>
    /// <param name="sourceId">어떤 오브젝트에서 온 버프인지 구분하기 위한 아이디</param>
    /// <param name="statType">어떤 스탯에 영향을 주는지</param>
    /// <param name="value">얼마나 영향을 주는지</param>
    /// <param name="modifierMethod">어떤 방식으로 영향을 주는지</param>
    public StatModifier(string sourceId, StatType statType, float value, ModifierMethod modifierMethod)
    {
        this.sourceId = sourceId;
        this.statType = statType;
        this.value = value;
        this.modifierMethod = modifierMethod;
    }
}
[Serializable]
public class UnitStats
{
    public float maxHP; //최대 체력
    public float maxMP; //최대 마나
    public float pengforce; //펭포스
    public float attackDamage; //공격력
    public float attackSpeed; //공격 속도
    public float attackRange; //공격 범위
    public float moveSpeed; //이동 속도
    public float healthRegen; //체력 재생
    public float manaGain; //마나 획득
    public float manaRegen; //마나 재생
    public float critChance; //치명타 확률
    public float critDamage; //치명타 피해
    public float endurance; //피해 감소
    public float damageAmp; //데미지 증가

    /// <summary>
    /// 스탯을 초기화합니다.
    /// </summary>
    /// <param name="stats">스탯 값의 딕셔너리</param>
    public UnitStats(Dictionary<StatType, float> stats)
    {
        maxHP = stats.TryGetValue(StatType.MaxHealth, out var hp) ? hp : 100f;
        maxMP = stats.TryGetValue(StatType.MaxMana, out var mp) ? mp : 100f;
        pengforce = stats.TryGetValue(StatType.Pengforce, out var pf) ? pf : 0f;
        attackDamage = stats.TryGetValue(StatType.AttackDamage, out var ad) ? ad : 10f;
        attackSpeed = stats.TryGetValue(StatType.AttackSpeed, out var a) ? a : 1f;
        attackRange = stats.TryGetValue(StatType.AttackRange, out var ar) ? ar : 1f;
        moveSpeed = stats.TryGetValue(StatType.MoveSpeed, out var ms) ? ms : 1f;
        healthRegen = stats.TryGetValue(StatType.HealthRegen, out var hr) ? hr : 0f;
        manaGain = stats.TryGetValue(StatType.ManaGain, out var mg) ? mg : 0f;
        manaRegen = stats.TryGetValue(StatType.ManaRegen, out var mr) ? mr : 0f;
        critChance = stats.TryGetValue(StatType.CritChance, out var cc) ? cc : 0.2f;
        critDamage = stats.TryGetValue(StatType.CritDamage, out var cd) ? cd : 1.3f;
        endurance = stats.TryGetValue(StatType.Endurance, out var en) ? en : 0f;
        damageAmp = stats.TryGetValue(StatType.DamageAmp, out var da) ? da : 0f;
    }
}