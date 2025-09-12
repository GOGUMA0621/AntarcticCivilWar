using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnitFormation;
using System;

[Serializable]
public class UnitSquad
{
    public int id;
    public List<UnitController> units = new();
    public Vector3 currentTarget;

    public void SetMoveTarget(Vector3 centerTarget, FormationType formation)
    {
        currentTarget = centerTarget;
        var positions = UnitFormationUtility.GetFormationPositions(centerTarget, units.Count, formation);

        for (int i = 0; i < units.Count; i++)
        {
            Vector3 worldPos = positions[i];
            // Vector2Int grid = GridManager.instance.WorldToGrid(worldPos);
            // Vector2Int validGrid = UnitFormationUtility.FindClosestWalkable(grid);
            // Vector3 finalWorld = GridManager.instance.GridToWorld(validGrid);
            // units[i].MoveToTarget(finalWorld);
        }
    }

    public void AddUnit(UnitController unit)
    {
        if (!units.Contains(unit)) units.Add(unit);
    }
}

public class UnitSquadManager : SingleTonBehaviour<UnitSquadManager>
{
    public List<UnitSquad> allSquads = new List<UnitSquad>();

    public UnitSquad CreateSquad(List<UnitController> selectedUnits)
    {
        var squad = new UnitSquad();
        foreach (var unit in selectedUnits)
            squad.AddUnit(unit);

        allSquads.Add(squad);
        return squad;
    }

    public void CommandMoveById(int squadId, Vector3 worldTarget, FormationType formation)
    {
        UnitSquad squad = GetSquadById(squadId);
        if (squad != null)
        {
            squad.SetMoveTarget(worldTarget, formation);
            //Debug.Log($"������ ID {squadId}�� ��ǥ ��ġ�� �̵��մϴ�.");
        }
        else
        {
            Debug.LogWarning($"������ ID {squadId}�� ã�� �� �����ϴ�. �̵� ���� ����.");
        }
    }

    public void AddUnitToSquadById(int squadId, UnitController unit)
    {
        UnitSquad squad = GetSquadById(squadId);
        if (squad != null)
        {
            squad.AddUnit(unit);
            Debug.Log($"���� {unit.name}�� ������ ID {squadId}�� �߰��Ǿ����ϴ�.");
        }
        else
        {
            Debug.LogWarning($"������ ID {squadId}�� ã�� �� �����ϴ�. ���� �߰� ����.");
        }
    }

    public void RemoveUnitToSquadById(int squadId, UnitController unit)
    {
        UnitSquad squad = GetSquadById(squadId);
        if (squad != null)
        {
            if (squad.units.Contains(unit))
            {
                squad.units.Remove(unit);
                Debug.Log($"���� {unit.name}�� ������ ID {squadId}���� ���ŵǾ����ϴ�.");
            }
            else
            {
                Debug.LogWarning($"������ ID {squadId}�� ���� {unit.name}�� �����ϴ�.");
            }
        }
        else
        {
            Debug.LogWarning($"������ ID {squadId}�� ã�� �� �����ϴ�. ���� ���� ����.");
        }
    }


    public UnitSquad GetSquadById(int id)
    {
        return allSquads.FirstOrDefault(squad => squad.id == id);
    }

    public void DeleteSquadById(int id)
    {
        var squad = allSquads.FirstOrDefault(s => s.id == id);
        if (squad != null)
        {
            squad.units.Clear(); // ���� ����Ʈ �ʱ�ȭ
            allSquads.Remove(squad); // ������ ��ü ����
        }
    }
}


