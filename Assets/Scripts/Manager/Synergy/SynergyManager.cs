using AYellowpaper.SerializedCollections;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class SynergyUIData
{
    public string name;
    public string description;
    public int count;
    public Sprite icon;
    public int tier;
}
public class SynergyManager : SingleTonBehaviour<SynergyManager>
{
    public Action OnSynergyUpdated;

    private Dictionary<string, List<UnitController>> allaySynergyDict = new Dictionary<string, List<UnitController>>();
    private Dictionary<string, List<UnitController>> enemySynergyDict = new Dictionary<string, List<UnitController>>();

    private Dictionary<string, int> allaySynergyCountDict = new Dictionary<string, int>();
    private Dictionary<string, int> enemySynergyCountDict = new Dictionary<string, int>();

    [SerializeField] private List<Sprite> tierIcons;

    private string dominantFactionTag = null;

    public Sprite GetTierIcon(int tier)
    {
        return tierIcons[tier];
    }

    public List<SynergyUIData> GetSynergyData(bool isAllay = true)
    {
        var result = new List<SynergyUIData>();
        var dict = isAllay ? allaySynergyDict : enemySynergyDict;
        var countDict = isAllay ? allaySynergyCountDict : enemySynergyCountDict;

        foreach (var kvp in countDict)
        {
            if (SynergyInstaller.synergyTagTypeMap.TryGetValue(kvp.Key, out var synergyType) && synergyType == SynergyType.Faction)
            {
                continue;
            }

            var unitList = dict.GetValueOrDefault(kvp.Key, new List<UnitController>());
            var firstUnit = unitList.FirstOrDefault();

            if (firstUnit != null)
            {
                var matchedSynergy = firstUnit.GetComponents<ISynergy>().FirstOrDefault(s => s.Tag == kvp.Key);
                if (matchedSynergy != null)
                {
                    result.Add(new SynergyUIData
                    {
                        name = matchedSynergy.Name,
                        description = matchedSynergy.synergyDescription,
                        count = kvp.Value,
                        icon = matchedSynergy.synergyIcon,
                        tier = matchedSynergy.currentTier
                    });
                }
            }
        }

        return result;
    }

    private void UpdateFactionSynergies(Dictionary<string, List<UnitController>> dict, bool isAllay)
    {
        var factionTags = SynergyInstaller.synergyTagTypeMap
            .Where(kvp => kvp.Value == SynergyType.Faction)
            .Select(kvp => kvp.Key)
            .ToList();

        // 가장 많은 유닛 수를 가진 Faction 시너지 찾기
        dominantFactionTag = factionTags
            .Where(tag => tag != "Neutral" && dict.ContainsKey(tag))
            .OrderByDescending(tag => dict[tag].Count)
            .FirstOrDefault();

        if (dominantFactionTag == null) return;

        int unitCount = dict[dominantFactionTag].Count;

        foreach (var tag in new[] { dominantFactionTag, "Neutral" })
        {
            if (tag == null) continue;
            if (!dict.TryGetValue(tag, out var unitList)) continue;

            foreach (var unit in unitList)
            {
                foreach (var synergy in unit.GetComponents<ISynergy>())
                {
                    if (synergy.Tag == tag)
                    {
                        synergy.OnCountUpdate(unitCount);
                    }
                }
            }
        }

        var dictExample = isAllay ? allaySynergyCountDict : enemySynergyCountDict;

        if (dictExample.ContainsKey(tag))
        {
            dictExample[tag] = unitCount;
        }
        else
        {
            dictExample.Add(tag, unitCount);
        }
    }


    public void RegisterUnit(UnitController unit, bool isAllay)
    {
        var dict = isAllay ? allaySynergyDict : enemySynergyDict;

        foreach (string tag in unit.unit.data.unitSynergyTags)
        {
            if (!dict.ContainsKey(tag))
            {
                dict[tag] = new List<UnitController>();
            }

            dict[tag].Add(unit);
            NotifyCountChanged(tag, dict, isAllay);
        }

        if (unit.unit.data.unitSynergyTags.Any(tag => SynergyInstaller.synergyTagTypeMap.ContainsKey(tag) && SynergyInstaller.synergyTagTypeMap[tag] == SynergyType.Faction))
        {
            UpdateFactionSynergies(dict, isAllay);
        }

        if (isAllay)
        {
            UnitManager.instance.AddAllayList(unit);
        }
        else
        {
            UnitManager.instance.AddEnemyList(unit);
        }
    }

    public void UnregisterUnit(UnitController unit, bool isAllay)
    {
        var dict = isAllay ? allaySynergyDict : enemySynergyDict;

        foreach (var tag in unit.unit.data.unitSynergyTags)
        {
            if (dict.TryGetValue(tag, out var list))
            {
                list.Remove(unit);
                NotifyCountChanged(tag, dict, isAllay);

                if (list.Count == 0)
                {
                    dict.Remove(tag);
                }
            }
        }

        if (unit.unit.data.unitSynergyTags.Any(tag => SynergyInstaller.synergyTagTypeMap.ContainsKey(tag) && SynergyInstaller.synergyTagTypeMap[tag] == SynergyType.Faction))
        {
            UpdateFactionSynergies(dict, isAllay);
        }
    }

    private void NotifyCountChanged(string tag, Dictionary<string, List<UnitController>> dict, bool isAllay)
    {
        if (IsFactionSynergy(tag)) return;
        if (!dict.TryGetValue(tag, out var unitList)) return;

        int totalCount = unitList.Count;
        int countToUse = totalCount;

        // Global Synergy는 첫 번째 유닛에만 적용
        var firstUnit = unitList.FirstOrDefault();

        if (firstUnit != null)
        {
            var synergy = firstUnit.GetComponents<ISynergy>().FirstOrDefault(s => s.Tag == tag);
            if (synergy != null)
            {
                countToUse = synergy.allowDuplicate
                        ? totalCount
                        : dict[tag].Select(u => u.unit.data.name).Distinct().Count();

                if (synergy is ISynergyGlobal globalSynergy)
                {
                    globalSynergy.ApplyToGlobal(countToUse);
                }
            }
        }

        foreach (var unit in unitList)
        {
            // 유닛에 여러 시너지 컴포넌트가 붙어 있을 수 있으므로,
            // 모든 ISynergy를 가져와서 해당 태그에 일치하는 것만 호출
            foreach (var synergy in unit.GetComponents<ISynergy>())
            {
                if (synergy.Tag == tag)
                {
                    synergy.OnCountUpdate(countToUse);
                }
            }
        }

        var dictExample = isAllay ? allaySynergyCountDict : enemySynergyCountDict;

        if (dictExample.ContainsKey(tag))
        {
            dictExample[tag] = countToUse;
        }
        else
        {
            dictExample.Add(tag, countToUse);
        }
        OnSynergyUpdated?.Invoke();
    }

    private bool IsFactionSynergy(string tag)
    {
        return SynergyInstaller.synergyTagTypeMap.TryGetValue(tag, out var type) && type == SynergyType.Faction;
    }

}
