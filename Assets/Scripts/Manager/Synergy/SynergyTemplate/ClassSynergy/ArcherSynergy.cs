using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[SynergyTag("Archer","궁수", SynergyType.ClassType)]
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
    private int lastTier = -1;

    private static readonly SynergyTierEffect[] ArcherTierEffects = new SynergyTierEffect[]
    {
        new SynergyTierEffect { RequiredCount = 3, Description = "공격속도 10% 증가", StatModifiers = new() { { StatType.AttackSpeed, 0.10f } } },
        new SynergyTierEffect { RequiredCount = 6, Description = "공격속도 15% 증가", StatModifiers = new() { { StatType.AttackSpeed, 0.15f } } },
        new SynergyTierEffect { RequiredCount = 8, Description = "공격속도 20% 증가", StatModifiers = new() { { StatType.AttackSpeed, 0.20f } } },
        new SynergyTierEffect { RequiredCount = 10, Description = "공격속도 25% 증가", StatModifiers = new() { { StatType.AttackSpeed, 0.25f } } },
    };

    private static readonly float[] globalAttackSpeed = { 0.10f, 0.10f, 0.10f, 0.20f }; // 모든 유닛
    private static readonly float[] archerBonusAttackSpeed = { 0.10f, 0.15f, 0.20f, 0.25f }; // 궁수 추가

    public void Initialize(UnitController unit)
    {
        this.unit = unit;
    }

    private int GetTier(int count)
    {
        int tier = -1;
        for (int i = 0; i < tierThresholds.Length; i++)
        {
            if (count >= tierThresholds[i])
                tier = i;
        }
        return tier;
    }

    public void OnCountUpdate(int count)
    {
        int tier = GetTier(count);

        unit.RemoveModifierStats(Tag);

        if (tier < 0)
        {
            lastTier = -1;
            return;
        }

        lastTier = tier;

        float bonus = archerBonusAttackSpeed[tier];
        unit.AddModifierStat(new StatModifier(Tag, StatType.AttackSpeed, bonus, ModifierMethod.Additive));
    }

    public void ApplyToGlobal(int count)
    {
        int tier = GetTier(count);

        foreach (var u in GetAllUnits())
        {
            u.RemoveModifierStats(Tag);

            if (tier >= 0)
            {
                u.AddModifierStat(new StatModifier(Tag + "_Global", StatType.AttackSpeed, globalAttackSpeed[tier], ModifierMethod.Additive));
            }
        }
    }

    private IEnumerable<UnitController> GetAllUnits()
    {
        return FindObjectsOfType<UnitController>();
        //return UnitManager.instance.GetAllUnits();
    }
}
