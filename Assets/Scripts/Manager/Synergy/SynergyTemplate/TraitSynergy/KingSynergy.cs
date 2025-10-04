using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SynergyTag("King","KING", SynergyType.Trait)]
public class KingSynergy : MonoBehaviour, ISynergy
{
    public string Tag => "King";

    public string Name => "KING";

    public bool allowDuplicate => false;

    public string synergyDescription => "";

    public Sprite synergyIcon => Resources.Load<Sprite>($"Synergy/{Name}");

    public int currentTier => lastTier;

    public int[] tierThresholds => new int[] { 1, 4 };

    private UnitController unit;

    private int lastTier = 0;

    public void Initialize(UnitController unit)
    {
        this.unit = unit;
        Debug.Log($"[Synergy] {this.unit} : {Tag} initialized");
    }

    public void OnCountUpdate(int count)
    {
        int tier = 0;
        for(int i = 0; i < tierThresholds.Length; i++)
        {
            if (count == tierThresholds[i])
            {
                tier = i + 1;
            }
        }

        if (lastTier == tier) return;

        switch (tier)
        {
            case 1:
                unit.RemoveModifierStats(Tag);
                unit.AddModifierStats(new List<StatModifier>
                {
                    new StatModifier(Tag, StatType.DamageAmp, 0.2f, ModifierMethod.Additive),
                    new StatModifier(Tag, StatType.Endurance, 0.2f, ModifierMethod.Additive),
                });
                break;
            case 2:
                unit.RemoveModifierStats(Tag);
                unit.AddModifierStats(new List<StatModifier>
                {
                    new StatModifier(Tag, StatType.DamageAmp, 0.2f, ModifierMethod.Additive),
                    new StatModifier(Tag, StatType.Endurance, 0.2f, ModifierMethod.Additive),
                    new StatModifier(Tag, StatType.ManaGain, 4f, ModifierMethod.Additive),
                });
                break;
        }

        lastTier = tier;
    }
}
