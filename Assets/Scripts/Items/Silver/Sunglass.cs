using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sunglass : StatItem
{
    public override string itemId => "Sunglasses";

    public float attackSpeedIncrease =15f;

    public override void ApplyEffect(UnitController unit)
    {
        float sumIncrease = attackSpeedIncrease * currentStack;
        float totalIncrease = 1 + (sumIncrease / 100);
        unit.AddModifierStat(new StatModifier(itemId, StatType.AttackSpeed, totalIncrease, ModifierMethod.Multiplicative));
    }

    public override void UpdateEffect(UnitController unit)
    {
        throw new System.NotImplementedException();
    }
}
