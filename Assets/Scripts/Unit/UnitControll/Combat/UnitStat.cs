
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