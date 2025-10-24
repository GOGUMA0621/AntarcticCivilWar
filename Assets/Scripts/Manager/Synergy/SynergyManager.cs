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
    /// <summary>
    /// 시너지 아이콘 가져오기
    /// </summary>
    /// <param name="tier">티어 인덱스</param>
    /// <returns>시너지 아이콘 스프라이트</returns>
    public Sprite GetTierIcon(int tier)
    {
        if (tier < 0 || tier >= tierIcons.Count)
            return null;
        return tierIcons[tier];
    }
    /// <summary>
    /// 시너지 데이터 가져오기
    /// </summary>
    /// <param name="isAllay">아군 시너지 여부</param>
    /// <returns>시너지 UI 데이터 리스트</returns>
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
    /// <summary>
    /// 진영 시너지 업데이트
    /// </summary>
    /// <param name="dict">시너지 딕셔너리</param>
    /// <param name="isAllay">아군 시너지 여부</param>
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

    /// <summary>
    /// 유닛 등록
    /// </summary>
    /// <param name="unit">등록할 유닛</param>
    /// <param name="isAllay">아군 유닛 여부</param>
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

        if (unit.unit.data.unitSynergyTags.Any(tag => SynergyInstaller.synergyTagTypeMap.ContainsKey(tag) &&
                SynergyInstaller.synergyTagTypeMap[tag] == SynergyType.Faction))
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
    /// <summary>
    /// 유닛 등록 해제
    /// </summary> <param name="unit">등록 해제할 유닛</param>
    /// <param name="isAllay">아군 유닛 여부</param>
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

        if (unit.unit.data.unitSynergyTags.Any(tag => SynergyInstaller.synergyTagTypeMap.ContainsKey(tag) 
            && SynergyInstaller.synergyTagTypeMap[tag] == SynergyType.Faction))
        {
            UpdateFactionSynergies(dict, isAllay);
        }
    }
    /// <summary>
    /// 시너지 카운트 변경 알림
    /// </summary> <param name="tag">시너지 태그</param>
    /// <param name="dict">시너지 딕셔너리</param>
    /// <param name="isAllay">아군 시너지 여부</param>
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
    /// <summary>
    /// 진영 시너지인지 확인
    /// </summary> <param name="tag">시너지 태그</param>
    /// <returns>진영 시너지 여부</returns>
    private bool IsFactionSynergy(string tag)
    {
        return SynergyInstaller.synergyTagTypeMap.TryGetValue(tag, out var type) && type == SynergyType.Faction;
    }

}
