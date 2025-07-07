using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SynergyTag("Knight", SynergyType.ClassType)]
public class KnightSynergy : MonoBehaviour, ISynergy
{
    public string Tag => "Knight";
    public string Name => "기사";
    public bool allowDuplicate => true;
    public string synergyDescription => throw new System.NotImplementedException();
    private UnitController unit;
    public Sprite synergyIcon => throw new System.NotImplementedException();

    public int currentTier => lastTier;

    public int[] tierThresholds => new int[] { 3, 5, 6, 10 };

    private int lastTier = 0;

    public void Initialize(UnitController unit)
    {
        this.unit = unit;
    }

    public void OnCountUpdate(int count)
    {
        int tier = 0;

        for (int i = 0; i < tierThresholds.Length; i++)
        {
            if (count >= tierThresholds[i])
            {
                tier = i + 1;
            }
        }

        if (tier == lastTier) return;

        switch (tier)
        {
            case 1:
                unit.AddModifierStats(new List<StatModifier>
                {
                    new StatModifier(Tag, StatType.Endurance, 0.1f, ModifierMethod.Additive),
                    new StatModifier(Tag, StatType.MaxHealth, 40f, ModifierMethod.Additive)
                });
                break;
            case 2:
                unit.AddModifierStats(new List<StatModifier>
                {
                    new StatModifier(Tag, StatType.Endurance, 0.1f, ModifierMethod.Additive),
                    new StatModifier(Tag, StatType.MaxHealth, 100f, ModifierMethod.Additive)
                });
                break;
            case 3:
                unit.AddModifierStats(new List<StatModifier>
                {
                    new StatModifier(Tag, StatType.Endurance, 0.1f, ModifierMethod.Additive),
                    new StatModifier(Tag, StatType.MaxHealth, 150f, ModifierMethod.Additive)
                });
                break;
            case 4:
                unit.AddModifierStats(new List<StatModifier>
                {
                    new StatModifier(Tag, StatType.Endurance, 0.1f, ModifierMethod.Additive),
                    new StatModifier(Tag, StatType.MaxHealth, 250f, ModifierMethod.Additive)
                });
                break;
        }

        lastTier = tier;
    }
}
