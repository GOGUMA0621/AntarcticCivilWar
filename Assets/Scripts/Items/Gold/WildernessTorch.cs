using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WildernessTorch : StatItem
{
    private bool isTriggerd = false;
    
    public override void ApplyEffect(UnitController unit)
    {
        throw new System.NotImplementedException();
    }

    public override void UpdateEffect(UnitController unit)
    {
        if (!isTriggerd && unit.GetNormalizedHealth() <= 0.4)
        {
            isTriggerd = true;
            unit.AddModifierStat(new StatModifier(itemName, StatType.AttackSpeed, 1.3f, ModifierMethod.Multiplicative));
        }
        else if(isTriggerd && unit.GetNormalizedHealth() >0.4)
        {
            isTriggerd = false;
            unit.RecalculateModifier(itemName);
        }
    }
}
