using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WildernessTorch : StatItem
{
    private bool isTriggerd = false;
    private float attackSpeed = 0f;

    public override void ApplyEffect(UnitController unit)
    {
        var item = FirebaseManager.GetItemByID(itemId);
        attackSpeed = (1 + item.base_effect[1]) * currentStack;
    }

    public override void UpdateEffect(UnitController unit)
    {
        if (!isTriggerd && unit.GetNormalizedHealth() <= 0.4)
        {
            isTriggerd = true;
            unit.AddModifierStat(new StatModifier(itemName, StatType.AttackSpeed, attackSpeed, ModifierMethod.Multiplicative));
        }
        else if(isTriggerd && unit.GetNormalizedHealth() >0.4)
        {
            isTriggerd = false;
            unit.RemoveModifierStats(itemName);
        }
    }
}
