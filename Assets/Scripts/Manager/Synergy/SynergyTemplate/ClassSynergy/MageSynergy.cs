using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SynergyTag("Mage", "마법사", SynergyType.ClassType)]
public class MageSynergy : MonoBehaviour, ISynergy, ISynergyGlobal
{
    public string Tag => "Mage";
    public string Name => "마법사";
    public bool allowDuplicate => true;
    public string synergyDescription => "";
    public Sprite synergyIcon => Resources.Load<Sprite>($"Synergy/{Name}");
    public int currentTier => lastTier;
    public int[] tierThresholds => new int[] { 3, 5, 7, 9 };

    private UnitController unit;
    private int lastTier = -1;

    private static readonly SynergyTierEffect[] MageTierEffects = new SynergyTierEffect[]
    {
        new SynergyTierEffect{ RequiredCount = 3, Description = "펭포스 20증가, 마술사 캐릭터들의 초당 마나 회복 +1", StatModifiers = new() { { StatType.Pengforce, 20f }, { StatType.ManaRegen, 1f } } },
        new SynergyTierEffect{ RequiredCount = 5, Description = "펭포스 25증가, 마술사 캐릭터들의 초당 마나 회복 +2", StatModifiers = new() { { StatType.Pengforce, 25f }, { StatType.ManaRegen, 2f } } },
        new SynergyTierEffect{ RequiredCount = 7, Description = "펭포스 30증가, 마술사 캐릭터들의 초당 마나 회복 +3", StatModifiers = new() { { StatType.Pengforce, 30f }, { StatType.ManaRegen, 3f } } },
        new SynergyTierEffect{ RequiredCount = 9, Description = "펭포스 40증가, 마술사 캐릭터들의 초당 마나 회복 +6", StatModifiers = new() { { StatType.Pengforce, 40f }, { StatType.ManaRegen, 6f } } },
    };


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

        // 마법사 캐릭터들의 초당 마나 회복량 증가
        if (MageTierEffects[tier].StatModifiers.TryGetValue(StatType.ManaRegen, out float manaRegen))
        {
            unit.AddModifierStat(new StatModifier(Tag, StatType.ManaRegen, manaRegen, ModifierMethod.Additive));
        }
    }

    public void ApplyToGlobal(int count)
    {
        int tier = GetTier(count);

        foreach (var u in GetAllUnits())
        {
            u.RemoveModifierStats(Tag + "_Global");

            if (tier >= 0 && MageTierEffects[tier].StatModifiers.TryGetValue(StatType.Pengforce, out float pengforce))
            {
                u.AddModifierStat(new StatModifier(Tag + "_Global", StatType.Pengforce, pengforce, ModifierMethod.Additive));
            }
        }
    }

    private IEnumerable<UnitController> GetAllUnits()
    {
        return FindObjectsOfType<UnitController>();
        //return UnitManager.instance.GetAllUnits();
    }
}
