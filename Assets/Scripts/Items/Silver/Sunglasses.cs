using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sunglasses : StatItem
{

    public override void ApplyEffect(UnitController unit)
    {
        var item = FirebaseManager.GetItemByID(itemId);
        if (item != null) 
        { 
            //Debug.Log(item.base_effect[0]);
            float sumIncrease = item.base_effect[0] * currentStack;
            float totalIncrease = 1 + (sumIncrease / 100);
            unit.AddModifierStat(new StatModifier(itemId.ToString(), StatType.AttackSpeed, totalIncrease, ModifierMethod.Multiplicative));
        }

    }

    public override void UpdateEffect(UnitController unit)
    {
        return;
    }
}
