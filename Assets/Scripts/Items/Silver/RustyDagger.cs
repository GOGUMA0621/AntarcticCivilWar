
public class RustyDagger : StatItem
{
    
    public float damageIncrease = 5f;
    public float stackDamageAmount = 5f;

    public override void ApplyEffect(UnitController unit)
    {
        var item = FirebaseManager.GetItemByID(itemId);
        float totalIncrease = item.base_effect[0] * currentStack;
        unit.AddModifierStat(new StatModifier(itemId.ToString(), StatType.AttackDamage, totalIncrease, ModifierMethod.Additive));
    }

    public override void UpdateEffect(UnitController unit)
    {
        return;
    }

    public override void IncreaseStack()
    {
        base.IncreaseStack();
    }
}
