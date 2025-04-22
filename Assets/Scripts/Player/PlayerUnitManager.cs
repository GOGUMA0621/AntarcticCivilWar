using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public class UnitPrefabEntry
{
    public GameObject prefab;
    public int count;
}

public class PlayerUnitManager : SingleTonBehaviour<PlayerUnitManager>
{
    [SerializeField] private List<GameObject> allayList = new List<GameObject>();

    private List<PassiveItem> itemUpdateEffects = new();

    public List<UnitPrefabEntry> allayPrefabList = new List<UnitPrefabEntry>();
    public List<GameObject> enemyList = new List<GameObject>();

    public int allaySquadId = 10015;
    public UnitSquad allaySquad;
    public int playerGroupPower = 0;

    // Start is called before the first frame update
    protected override void Awake()
    {
        base.Awake();
        //CreateSquad();
    }

    private void Update()
    {
        foreach (var allay in allayList) 
        {
            if(allay.TryGetComponent<UnitController>(out UnitController unit))
            {
                foreach(var effect in itemUpdateEffects)
                {
                    effect.UpdateEffect(unit);
                }
            }
        }
    }

    //private void CreateSquad()
    //{
    //    allaySquad = new UnitSquad();
    //    allaySquad.id = allaySquadId;
    //    allaySquad.units.Clear();
    //    UnitSquadManager.instance.DeleteSquadById(allaySquad.id);
    //    foreach (var unit in allayList)
    //    {
    //        if (unit.TryGetComponent<UnitController>(out UnitController unitController))
    //        {
    //            allaySquad.units.Add(unitController);
    //        }
    //    }
    //    UnitSquadManager.instance.allSquads.Add(allaySquad);
    //}

    #region 무리력
    private void CalculatePlayerGroupPower()
    {
        int playerGroupPower = 0;
        foreach (GameObject allay in allayList)
        {
            if (allay.TryGetComponent<UnitController>(out UnitController unitController))
            {
                if (unitController.unit.data != null)
                {
                    playerGroupPower += unitController.unit.data.UnitPower;
                }
            }
        }
        this.playerGroupPower = playerGroupPower;
    }
    #endregion

    #region  아군 관리
    public void AddAllayList(GameObject allay)
    {
        if (!allayList.Contains(allay))
        {
            allayList.Add(allay);
            UnitSquadManager.instance.AddUnitToSquadById(allaySquad.id, allay.GetComponent<UnitController>());
            ApplyItemToUnit(allay.GetComponent<UnitController>());
            AddUnitPrefabList(allay.GetComponent<Unit>().originPrefab);
            CalculatePlayerGroupPower();
        }
    }
    public void RemoveAllayList(GameObject allay)
    {
        if (allayList.Contains(allay))
        {
            allay.GetComponent<UnitController>().ResetUnit();
            allayList.Remove(allay);
            UnitSquadManager.instance.RemoveUnitToSquadById(allaySquad.id, allay.GetComponent<UnitController>());
            RemoveUnitPrefabList(allay.GetComponent<Unit>().originPrefab);
            CalculatePlayerGroupPower();
        }
    }

    public void ChangeStateAllayList(string unitState)
    {
        foreach (var unit in allayList)
        {
            if(unit.TryGetComponent<UnitController>(out UnitController unitController))
            {
                switch(unitState)
                {
                    case "IdleState":
                        unitController.GoIdle();
                        break;
                    case "FollowState":
                        unitController.GoFollow();
                        break;
                    case "AttackState":
                        unitController.GoAttack();
                        break;
                    case "DieState":
                        unitController.GoDie();
                        break;
                    case "CallState":
                        unitController.GoCall();
                        break;
                    default:
                        Debug.LogError($"Unknown state: {unitState}");
                        break;
                }
            }
        }
    }
    #endregion

    #region 적군 관리
    public void AddEnemyList(GameObject enemy)
    {
        if (!enemyList.Contains(enemy))
        {
            enemyList.Add(enemy);
        }
    }

    public void RemoveEnemyList(GameObject enemy)
    {
        if (enemyList.Contains(enemy))
        {
            foreach (GameObject unit in allayList)
            {

            }
            enemyList.Remove(enemy);
        }
    }
    #endregion

    #region 프리팹 관리
    public void SpawnPlayerUnits(Vector3 spawnPos)
    {
        List<UnitPrefabEntry> copy = new List<UnitPrefabEntry>(allayPrefabList);
        allayList.Clear();
        foreach (var unit in copy)
        {
            for(int i = 0; i < unit.count; i++)
            {
                GameObject allay = Instantiate(unit.prefab, spawnPos, Quaternion.identity);
                allay.GetComponent<Unit>().originPrefab = unit.prefab;
                allay.tag = "Unit";
                allayList.Add(allay);
            }
        }
    }

    public void AddUnitPrefabList(GameObject prefab)
    {
        UnitPrefabEntry entry = allayPrefabList.Find(x => x.prefab == prefab);

        if (entry != null)
        {
            entry.count++;
        }
        else
        {
            allayPrefabList.Add(new UnitPrefabEntry { prefab = prefab, count = 1 });
        }
    }

    public void RemoveUnitPrefabList(GameObject prefab)
    {
        UnitPrefabEntry entry = allayPrefabList.Find(x => x.prefab == prefab);

        if (entry != null)
        {
            entry.count--;
            if (entry.count <= 0)
            {
                allayPrefabList.Remove(entry);
            }
        }
    }
    
    public void AddUnitSOPrefabList(UnitGroupSO unitGroupSO)
    {
        foreach (var unit in unitGroupSO.groupUnits)
        {
            if (unit.pfUnit == null)
            {
                Debug.LogError("pfUnit이 null임");
                continue;
            }
            UnitPrefabEntry entry = allayPrefabList.Find(x => x.prefab == unit.pfUnit);

            if (entry != null)
            {
                entry.count += unit.count;
            }
            else
            {
                allayPrefabList.Add(new UnitPrefabEntry { prefab = unit.pfUnit, count = unit.count });
            }
        }
        CalculatePlayerGroupPower();
    }

    public void AddUnitSOAllayList(UnitGroupSO unitGroupSO, Vector3 position)
    {
        foreach (var unit in unitGroupSO.groupUnits)
        {
            if (unit.pfUnit == null)
            {
                Debug.LogError("pfUnit이 null임");
                continue;
            }
            for (int i = 0; i < unit.count; i++)
            {
                GameObject allay = Instantiate(unit.pfUnit, position, Quaternion.identity);
                allay.GetComponent<Unit>().originPrefab = unit.pfUnit;
                allay.tag = "Unit";
                allayList.Add(allay);
            }
        }
        CalculatePlayerGroupPower();
    }

    #endregion

    #region 아이템 효과
    public void ApplyItemsToAllUnit()
    {
        foreach (var item in InventoryManager.instance.inventoryItems)
        {
            PassiveItem passiveItem = item.Key.GetComponent<PassiveItem>();
            foreach (GameObject allay in allayList)
            {
                if (allay.TryGetComponent<UnitController>(out UnitController allayController))
                {
                    passiveItem.ApplyEffect(allayController);
                }
            }
        }
    }

    public void ApplyItemToUnit(UnitController unit)
    {
        if (InventoryManager.instance.inventoryItems.Any())
        {
            foreach (var item in InventoryManager.instance.inventoryItems)
            {
                var passiveItem = item.Key.GetComponent<PassiveItem>();
                passiveItem.ApplyEffect(unit);

                if(passiveItem is IPassiveItem updateEffect)
                {
                    itemUpdateEffects.Add(passiveItem);
                }
            }
        }
    }

    public void ApplyItemToUnits(PassiveItem item)
    {
        if (InventoryManager.instance.inventoryItems.Any())
        {
            foreach (GameObject allay in allayList)
            {
                item.ApplyEffect(allay.GetComponent<UnitController>());
            }
        }
    }
    #endregion
}
