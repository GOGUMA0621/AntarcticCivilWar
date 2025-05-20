using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SynergyTag("Neutral", SynergyType.Faction)]
public class NeutralSynergy : MonoBehaviour, ISynergy
{
    public string Tag => "Neutral";

    public string Name => "중립";

    public bool allowDuplicate => true;

    public string synergyDescription => "";

    private int lastTier = 0;
    private UnitController unit;
    
    public Sprite synergyIcon => Resources.Load<Sprite>($"Synergy/{Name}");

    public int currentTier => lastTier;
    public int[] tierThresholds => new int[0];

    public void Initialize(UnitController unit)
    {
        this.unit = unit;
    }

    public void OnCountUpdate(int count)
    {
        int tier = 0;

        if (count >= 5) tier = 1;
        else if (count >= 8) tier = 2;
        else if (count >= 10) tier = 3;
        else if (count >= 12) tier = 4;
        else if (count >= 14) tier = 5;

        if (tier == lastTier) return;

        switch (tier)
        {
            case 1:
                unit.AddModifierStat(new StatModifier(Tag, StatType.AttackDamage, 10, ModifierMethod.Additive));
                break;
            case 2:
                unit.AddModifierStat(new StatModifier(Tag, StatType.AttackDamage, 10, ModifierMethod.Additive));
                break;
            case 3:
                unit.AddModifierStat(new StatModifier(Tag, StatType.AttackDamage, 10, ModifierMethod.Additive));
                break;
            case 4:
                unit.AddModifierStat(new StatModifier(Tag, StatType.AttackDamage, 10, ModifierMethod.Additive));
                break;
            case 5:
                unit.AddModifierStats(new List<StatModifier>
                {
                    new StatModifier(Tag, StatType.AttackDamage, 35f, ModifierMethod.Additive),
                    new StatModifier(Tag, StatType.Endurance, 0.1f, ModifierMethod.Additive)
                });
                break;
        }

        lastTier = tier;
    
    }
}
