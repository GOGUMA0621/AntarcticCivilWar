
public class RustyDagger : StatItem
{
    
    public float damageIncrease = 5f;
    public float stackDamageAmount = 5f;

    public override void ApplyEffect(UnitController unit)
    {
        float totalIncrease = damageIncrease + (stackDamageAmount * currentStack - 1);
        unit.AddModifierStat(new StatModifier(itemId, StatType.AttackDamage, totalIncrease, ModifierMethod.Additive));
    }

    public override void UpdateEffect(UnitController unit)
    {
        throw new System.NotImplementedException();
    }

    public override void IncreaseStack()
    {
        base.IncreaseStack();
    }
}
