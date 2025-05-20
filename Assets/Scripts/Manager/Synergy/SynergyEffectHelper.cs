using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SynergyEffectHelper
{
    public static void ApplyToUnits(string sourceId, List<UnitController> dict, StatModifier statModifier)
    {
        foreach (var unit in dict)
        {
            if (unit == null) continue;
            unit.AddModifierStat(new StatModifier(sourceId, statModifier.statType, statModifier.value, statModifier.modifierMethod));
        }
    }

    public static void RemoveFromUnits(string sourceId,List<UnitController> dict)
    {
        foreach (var unit in dict)
        {
            if (unit == null) continue;
            unit.RemoveModifierStats(sourceId);
        }
    }
}
