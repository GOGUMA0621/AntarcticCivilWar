using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SynergyTag("Warrior", SynergyType.ClassType)]
public class WarriorSynergy : MonoBehaviour, ISynergy
{
    private UnitController unit;
    
    public string Tag => "Warrior";
    public string Name => "전사";
    public string synergyDescription => "";
    public bool allowDuplicate => true;
    [SerializeField] private Sprite[] synergyIconsPreview;
    public Sprite synergyIcon => Resources.Load<Sprite>($"Synergy/{Name}");
    public int currentTier => lastTier;

    public int[] tierThresholds => new int[] { 3, 6, 10, 13, 15 };

    private string synergyTag;

    private int lastTier = 0;

    public void Initialize(UnitController unit)
    {
        this.unit = unit;
        synergyTag = Tag + "_Synergy";
    }

    public void OnCountUpdate(int count)
    {
        int tier = 0;
        
        for (int i = 0; i < tierThresholds.Length; i++)
        {
            if (count >= tierThresholds[i])
            {
                tier = i + 1;
                Debug.Log($"Warrior Synergy Tier: {tier}");
            }
        }

        if (tier == lastTier) return;


        switch (tier)
        {
            case 1:
                unit.AddModifierStats(new List<StatModifier> 
                {
                    new StatModifier(synergyTag, StatType.Endurance, 0.1f, ModifierMethod.Additive),
                    new StatModifier(synergyTag, StatType.MaxHealth, 40f, ModifierMethod.Additive)
                });
                break;
            case 2:
                unit.AddModifierStats(new List<StatModifier>
                {
                    new StatModifier(synergyTag, StatType.Endurance, 0.1f, ModifierMethod.Additive),
                    new StatModifier(synergyTag, StatType.MaxHealth, 100f, ModifierMethod.Additive)
                });
                break;
            case 3:
                unit.AddModifierStats(new List<StatModifier>
                {
                    new StatModifier(synergyTag, StatType.Endurance, 0.1f, ModifierMethod.Additive),
                    new StatModifier(synergyTag, StatType.MaxHealth, 150f, ModifierMethod.Additive)
                });
                break;
            case 4:
                unit.AddModifierStats(new List<StatModifier>
                {
                    new StatModifier(synergyTag, StatType.Endurance, 0.1f, ModifierMethod.Additive),
                    new StatModifier(synergyTag, StatType.MaxHealth, 200f, ModifierMethod.Additive)
                });
                break;
            case 5:
                unit.AddModifierStats(new List<StatModifier>
                {
                    new StatModifier(synergyTag, StatType.Endurance, 0.15f, ModifierMethod.Additive),
                    new StatModifier(synergyTag, StatType.MaxHealth, 300f, ModifierMethod.Additive)
                });
                break;
        }
        
        lastTier = tier;
    }
}
