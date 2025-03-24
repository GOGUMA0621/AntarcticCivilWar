using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RustyDagger : StatItem
{
    public override string itemId => "RustyDagger";
    
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
