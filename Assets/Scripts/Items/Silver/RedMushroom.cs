using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RedMushroom : StatItem
{

    public float attackRangeIncrease = .5f;
    public float healthIncrease = 10f;

    public override void ApplyEffect(UnitController unit)
    {
        float totalAttackRangeIncrease = attackRangeIncrease * currentStack;
        float totalHealthIncrease = healthIncrease * currentStack;

        if (unit.data.unitAttackType == UnitAttackType.Melee)
        {
            unit.AddModifierStat(new StatModifier(itemId, StatType.AttackRange, totalAttackRangeIncrease, ModifierMethod.Additive));
            unit.AddModifierStat(new StatModifier(itemId, StatType.MaxHealth, totalHealthIncrease, ModifierMethod.Additive));
        }
    }

    public override void UpdateEffect(UnitController unit)
    {
        throw new System.NotImplementedException();
    }
}
