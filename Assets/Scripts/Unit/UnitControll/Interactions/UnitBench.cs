using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UnitBench : MonoBehaviour
{
    public List<UnitDugout> bench = new List<UnitDugout>();
    public PlacementGridManager placementGrid;

    public Dictionary<string, List<UnitController>> sortedUnitDict => GetSortedUnitDictionary();

    /// <summary>
    /// 이름과 레벨로 유닛 리스트를 가져옵니다.
    /// </summary>
    public List<UnitController> GetUnitsByNameAndLevel(string unitName, int unitLevel)
    {
        Debug.Log($"GetUnitsByNameAndLevel 호출: unitName={unitName}, unitLevel={unitLevel}");
        if (sortedUnitDict.TryGetValue(unitName, out var list))
        {
            foreach (var u in list)
                Debug.Log($"- dict 유닛: {u.unit.data.UnitName}, Lv{u.unitLevel}");
            return list.FindAll(u => u.unitLevel == unitLevel);
        }
        Debug.Log("딕셔너리에 해당 이름 없음");
        return new List<UnitController>();
    }

    /// <summary>
    /// 같은 이름과 레벨을 가진 유닛이 3개 이상이 될 수 있는지(새 유닛 포함) 판단
    /// </summary>
    public bool CanLevelUpWithNewUnit(string unitName, int unitLevel)
    {
        var candidates = GetUnitsByNameAndLevel(unitName, unitLevel);
        // 새로 들어올 유닛까지 포함해서 3개 이상이면 true
        return candidates.Count + 1 >= 3;
    }

    /// <summary>
    /// 벤치의 가장 앞 빈 슬롯에 유닛을 추가하고, 빈 슬롯이 없으면 레벨업 가능 여부를 판단해 처리
    /// </summary>
    public void AddUnitToBench(UnitDB unit)
    {
        GameObject unitPrefab = UnitPrefabsLoader.GetPrefab(unit.name);
        if (unitPrefab == null)
        {
            Debug.LogWarning($"'{unit.name}' 유닛 프리팹을 찾을 수 없습니다.");
            return;
        }

        var unitController = Instantiate(unitPrefab).GetComponent<UnitController>();
        unitController.unitLevel = 1;

        string unitName = unitController.unit.data.UnitName;
        int unitLevel = unitController.unitLevel;

        // 1. 가장 앞 빈 dugout 찾기
        var emptyDugout = bench.Find(d => d.unitInDugout == null);
        if (emptyDugout != null)
        {
            emptyDugout.SetUnitInDugout(unitController);

            // 딕셔너리 상태 출력
            DebugUnitDictionary();

            TryLevelUpUnit(unitName, unitLevel);
            return;
        }

        // 2. 빈 슬롯이 없을 때: 레벨업 가능 여부 판단
        if (CanLevelUpWithNewUnit(unitName, unitLevel))
        {
            // TryLevelUpUnit이 extraUnit 파라미터를 지원하므로, 새 유닛을 인자로 넘김
            TryLevelUpUnit(unitName, unitLevel, unitController);
            return;
        }

        // 3. 레벨업도 불가능하면 경고
        Debug.LogWarning("벤치에 빈 슬롯이 없고, 레벨업도 불가능합니다!");
    }

    /// <summary>
    /// 같은 이름과 레벨을 가진 유닛이 3개 이상이면 레벨업 처리
    /// </summary>
    public void TryLevelUpUnit(string unitName, int unitLevel, UnitController extraUnit = null)
    {
        var candidates = GetUnitsByNameAndLevel(unitName, unitLevel);
        Debug.Log($"레벨업 시도: {unitName} Lv{unitLevel}, candidates.Count={candidates.Count}");
        if (extraUnit != null && !candidates.Contains(extraUnit))
            candidates.Add(extraUnit);

        if (candidates.Count < 3)
            return;

        // candidates를 bench 우선 정렬(필요시)
        candidates.Sort((a, b) => {
            int aBench = bench.Any(d => d.unitInDugout == a) ? 0 : 1;
            int bBench = bench.Any(d => d.unitInDugout == b) ? 0 : 1;
            return aBench.CompareTo(bBench);
        });

        // 가장 앞 유닛을 레벨업
        var mainUnit = candidates[0];
        mainUnit.unitLevel += 1;
        mainUnit.SetUnit(); // 스탯 갱신 등

        // 나머지 2개는 bench/필드에서 제거 및 Destroy
        for (int i = 1; i < 3; i++)
        {
            // bench에서 제거
            foreach (var dugout in bench)
            {
                if (dugout.unitInDugout == candidates[i])
                {
                    dugout.RemoveUnitFromDugout();
                }
            }
            // placementGrid 등 필드에서도 제거 필요시 처리
        }

        // 재귀적으로 레벨업(연속 레벨업 지원)
        TryLevelUpUnit(unitName, unitLevel + 1);
    }
    /// <summary>
    /// 벤치와 필드의 유닛들을 이름별로 정렬하여 딕셔너리로 반환
    /// </summary>
    /// <returns>이름별로 정렬된 유닛 딕셔너리</returns>
    public Dictionary<string, List<UnitController>> GetSortedUnitDictionary()
    {
        var dict = new Dictionary<string, List<UnitController>>();

        // 1. 필드 유닛 먼저 추가 (우선순위: x가 클수록, x가 같으면 y가 클수록)
        List<(UnitController unit, int x, int y)> fieldUnits = new List<(UnitController, int, int)>();
        for (int x = 0; x < placementGrid.width; x++)
        {
            for (int y = 0; y < placementGrid.height; y++)
            {
                var unit = placementGrid.GetUnitByPos(new Vector2Int(x, y));
                if (unit != null && unit.controller != null)
                {
                    fieldUnits.Add((unit.controller, x, y));
                }
            }
        }
        // 정렬: x 내림차순, x가 같으면 y 내림차순, 레벨 오름차순
        fieldUnits.Sort((a, b) =>
        {
            int cmp = b.x.CompareTo(a.x);
            if (cmp == 0) cmp = b.y.CompareTo(a.y);
            if (cmp == 0) cmp = a.unit.unitLevel.CompareTo(b.unit.unitLevel); // 레벨 낮은 게 앞
            return cmp;
        });

        foreach (var (unit, _, _) in fieldUnits)
        {
            string name = unit.unit.data.UnitName;
            if (!dict.ContainsKey(name))
                dict[name] = new List<UnitController>();
            dict[name].Add(unit);
        }

        // 2. 벤치 유닛 추가 (우선순위: 인덱스가 작을수록 우선, 레벨 낮은 게 앞)
        List<(UnitController unit, int index)> benchUnits = new List<(UnitController, int)>();
        for (int i = 0; i < bench.Count; i++)
        {
            var dugout = bench[i];
            if (dugout.unitInDugout != null)
            {
                benchUnits.Add((dugout.unitInDugout, i));
            }
        }
        benchUnits.Sort((a, b) =>
        {
            int cmp = a.index.CompareTo(b.index);
            if (cmp == 0) cmp = a.unit.unitLevel.CompareTo(b.unit.unitLevel); // 레벨 낮은 게 앞
            return cmp;
        });

        foreach (var (unit, _) in benchUnits)
        {
            string name = unit.unit.data.UnitName;
            if (!dict.ContainsKey(name))
                dict[name] = new List<UnitController>();
            dict[name].Add(unit);
        }

        return dict;
    }

    /// <summary>
    /// 현재 딕셔너리 상태를 Debug.Log로 출력
    /// </summary>
    private void DebugUnitDictionary()
    {
        var dict = GetSortedUnitDictionary();
        Debug.Log("=== 유닛 딕셔너리 상태 ===");
        foreach (var kvp in sortedUnitDict)
        {
            string units = string.Join(", ", kvp.Value.ConvertAll(u => $"[{u.unit.data.UnitName} Lv{u.unitLevel}]"));
            Debug.Log($"{kvp.Key}: {units}");
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
