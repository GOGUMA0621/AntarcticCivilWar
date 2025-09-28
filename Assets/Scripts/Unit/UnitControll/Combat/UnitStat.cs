
using System;
using System.Collections.Generic;

public enum StatType
{
    MaxHealth,
    HealthRegen,
    MaxMana,
    ManaRegen,
    AttackDamage,
    AdditionalDamage,
    AttackSpeed,
    AttackRange,
    MoveSpeed,
    AOEDamage,
    CritChance,
    CritDamage,
    Endurance,
    DamageAmp,
    Pengforce
}

public enum ModifierMethod
{
    Additive,
    Multiplicative,
    AdditivePercent,
    MultiplicativePercent,
}

public class StatModifier
{
    public string sourceId { get; }
    public StatType statType { get; }
    public float value { get; }
    public ModifierMethod modifierMethod { get; }

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
    public float maxHP;
    public float maxMP;
    public float pengforce;
    public float attackDamage;
    public float attackSpeed;
    public float attackRange;
    public float moveSpeed;
    public float healthRegen;
    public float manaRegen;
    public float critChance;
    public float critDamage;
    public float endurance;
    public float damageAmp;

    public UnitStats(Dictionary<StatType, float> stats)
    {
        maxHP        = stats.TryGetValue(StatType.MaxHealth, out var hp) ? hp : 100f;
        maxMP        = stats.TryGetValue(StatType.MaxMana, out var mp) ? mp : 100f;
        pengforce    = stats.TryGetValue(StatType.Pengforce, out var pf) ? pf : 0f;
        attackDamage = stats.TryGetValue(StatType.AttackDamage, out var ad) ? ad : 10f;
        attackSpeed  = stats.TryGetValue(StatType.AttackSpeed, out var a) ? a : 1f;
        attackRange  = stats.TryGetValue(StatType.AttackRange, out var ar) ? ar : 1f;
        moveSpeed    = stats.TryGetValue(StatType.MoveSpeed, out var ms) ? ms : 1f;
        healthRegen  = stats.TryGetValue(StatType.HealthRegen, out var hr) ? hr : 0f;
        manaRegen    = stats.TryGetValue(StatType.ManaRegen, out var mr) ? mr : 0f;
        critChance   = stats.TryGetValue(StatType.CritChance, out var cc) ? cc : 0.2f;
        critDamage   = stats.TryGetValue(StatType.CritDamage, out var cd) ? cd : 1.3f;
        endurance    = stats.TryGetValue(StatType.Endurance, out var en) ? en : 0f;
        damageAmp    = stats.TryGetValue(StatType.DamageAmp, out var da) ? da : 0f;
    }
}