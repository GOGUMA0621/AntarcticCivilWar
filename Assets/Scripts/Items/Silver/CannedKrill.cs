using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannedKrill : StatItem, IPassiveItem
{
    public override string itemId => "CannedKrillShrimp";

    public float healthIncrease = 15f;

    public override void ApplyEffect(UnitController unit)
    {
        float totlaIncrease = healthIncrease * currentStack;
        unit.AddModifierStat(new StatModifier(itemId, StatType.MaxHealth, totlaIncrease, ModifierMethod.Additive));
    }

    public override void UpdateEffect(UnitController unit)
    {
        throw new System.NotImplementedException();
    }

}