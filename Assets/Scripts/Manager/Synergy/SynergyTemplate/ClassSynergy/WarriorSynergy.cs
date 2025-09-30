using System.Collections.Generic;
using UnityEngine;

[SynergyTag("Warrior", "전사", SynergyType.ClassType)]
public class WarriorSynergy : MonoBehaviour, ISynergy, ISynergyGlobal
{
    public string Tag => "Warrior";
    public string Name => "전사";
    public bool allowDuplicate => true;
    public string synergyDescription => "";
    public Sprite synergyIcon => Resources.Load<Sprite>($"Synergy/{Name}");
    public int currentTier => lastTier;
    public int[] tierThresholds => new int[] { 2, 5, 8, 10 };

    private UnitController unit;
    private int lastTier = -1;

    // 티어별 효과 정의
    private static readonly SynergyTierEffect[] WarriorTierEffects = new SynergyTierEffect[]
    {
        new SynergyTierEffect { RequiredCount = 2, Description = "받는피해 10% 감소, 전사 캐릭터들의 체력 100증가", StatModifiers = new() { { StatType.Endurance, 0.10f }, { StatType.MaxHealth, 100f } } },
        new SynergyTierEffect { RequiredCount = 5, Description = "받는피해 10% 감소, 전사 캐릭터들의 체력 200증가", StatModifiers = new() { { StatType.Endurance, 0.10f }, { StatType.MaxHealth, 200f } } },
        new SynergyTierEffect { RequiredCount = 8, Description = "받는피해 10% 감소, 전사 캐릭터들의 체력 500증가", StatModifiers = new() { { StatType.Endurance, 0.10f }, { StatType.MaxHealth, 500f } } },
        new SynergyTierEffect { RequiredCount = 10, Description = "받는피해 15% 감소, 전사 캐릭터들의 체력 800증가", StatModifiers = new() { { StatType.Endurance, 0.15f }, { StatType.MaxHealth, 800f } } },
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

        // 전사 유닛(자기 자신)에게 체력 증가만 적용
        var statMods = WarriorTierEffects[tier].StatModifiers;
        if (statMods.TryGetValue(StatType.MaxHealth, out float maxHealth))
        {
            unit.AddModifierStat(new StatModifier(Tag, StatType.MaxHealth, maxHealth, ModifierMethod.Additive));
        }
    }

    public void ApplyToGlobal(int count)
    {
        int tier = GetTier(count);

        foreach (var u in GetAllUnits())
        {
            u.RemoveModifierStats(Tag + "_Global");

            if (tier >= 0 && WarriorTierEffects[tier].StatModifiers.TryGetValue(StatType.Endurance, out float endurance))
            {
                u.AddModifierStat(new StatModifier(Tag + "_Global", StatType.Endurance, endurance, ModifierMethod.Additive));
            }
        }
    }

    private IEnumerable<UnitController> GetAllUnits()
    {
        return FindObjectsOfType<UnitController>();
    }
}
