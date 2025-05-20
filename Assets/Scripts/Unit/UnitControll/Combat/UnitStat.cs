
public enum StatType
{
    MaxHealth,
    HealthRegen,
    MaxMana,
    ManaRegen,
    AttackDamage,
    AttackSpeed,
    AttackRange,
    MoveSpeed,
    AOEDamage,
    CritChance,
    Endurance,
    DamageAmp
}

public enum ModifierMethod
{
    Additive,
    Multiplicative
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