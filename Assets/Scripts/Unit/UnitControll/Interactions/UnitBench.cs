using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitBench : MonoBehaviour
{
    public List<UnitDugout> bench = new List<UnitDugout>();
    public PlacementGridManager placementGrid;

    public void AddUnitToBench(UnitController unit)
    {
        var newDugout = new UnitDugout();
        newDugout.SetUnitInDugout(unit);
        bench.Add(newDugout);
    }

    public int GetSameUnitCount(UnitController target)
    {
        int count = 0;
        foreach (var dugout in bench)
        {
            if (dugout.unitInDugout != null &&
                dugout.unitInDugout.unit.data.UnitName == target.unit.data.UnitName &&
                dugout.unitInDugout.unitLevel == target.unitLevel)
            {
                count++;
            }
        }
        return count;
    }

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

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
