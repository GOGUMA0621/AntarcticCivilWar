using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SynergyTag("Archer", SynergyType.ClassType)]
public class ArcherSynergy : MonoBehaviour, ISynergy, ISynergyGlobal
{
    public string Tag => "Archer";
    public string Name => "궁수";
    public bool allowDuplicate => true;
    public string synergyDescription => "";
    public Sprite synergyIcon => Resources.Load<Sprite>($"Synergy/{Name}");
    public int currentTier => lastTier;

    public int[] tierThresholds => new int[] { 3, 6, 8, 10 };

    private UnitController unit;
    
    private int lastTier = 0;

    public void Initialize(UnitController unit)
    {
        this.unit = unit;
    }

    public void OnCountUpdate(int count)
    {
        int tier = 0;
        for(int i = 0; i < tierThresholds.Length; i++)
        {
            if (count >= tierThresholds[i])
            {
                tier = i + 1;
            }
        }

        if (tier == lastTier) return;

        int attackSpeed = 0;

        switch (tier)
        {
            case 1:
                attackSpeed = 10;
                break;
            case 2:
                attackSpeed = 25;
                break;
            case 3:
                attackSpeed = 20;
                break;
            case 4:
                attackSpeed = 25;
                break;
        }

        lastTier = tier;

        float attackSpeedPercent = attackSpeed / 100f;
        unit.AddModifierStat(new StatModifier(Tag, StatType.AttackSpeed, attackSpeedPercent, ModifierMethod.Additive));

    }

    public void ApplyToGlobal(int count)
    {
        
    }
}
