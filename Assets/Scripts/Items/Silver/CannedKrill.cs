using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CannedKrill : StatItem, IPassiveItem
{
    public override void ApplyEffect(UnitController unit)
    {
        var item = FirebaseManager.GetItemByID(itemId);
        float totlaIncrease = item.base_effect[0] * currentStack;
        unit.AddModifierStat(new StatModifier(itemId.ToString(), StatType.MaxHealth, totlaIncrease, ModifierMethod.Additive));
    }

    public override void UpdateEffect(UnitController unit)
    {
        throw new System.NotImplementedException();
    }

}