using System.Collections.Generic;
using System.Linq;
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
    public List<UnitPrefabEntry> allayPrefabList = new List<UnitPrefabEntry>();
    public List<GameObject> enemyList = new List<GameObject>();

    public int playerGroupPower = 0;

    // Start is called before the first frame update
    protected override void Awake()
    {
        base.Awake();
    }

    #region 公府仿
    private void CalculatePlayerGroupPower()
    {
        int playerGroupPower = 0;
        foreach (GameObject allay in allayList)
        {
            if (allay.TryGetComponent<UnitController>(out UnitController unitController))
            {
                if (unitController.data != null)
                {
                    playerGroupPower += unitController.data.UnitPower;
                }
            }
        }
        this.playerGroupPower = playerGroupPower;
    }
    #endregion

    #region  酒焙 包府
    public void AddAllayList(GameObject allay)
    {
        if (!allayList.Contains(allay))
        {
            allayList.Add(allay);
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
            RemoveUnitPrefabList(allay.GetComponent<Unit>().originPrefab);
            CalculatePlayerGroupPower();
        }
    }
    #endregion

    #region 橇府普 包府
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
    #endregion

    #region 利焙 包府
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

    #region 酒捞袍 瓤苞
    public void ApplyItemsToAllUnit()
    {
        foreach (var item in InventoryManager.instance.inventoryItems)
        {
            PassiveItem passiveItem = item.Key.GetComponent<PassiveItem>();
            foreach (GameObject allay in allayList)
            {
                if (TryGetComponent<UnitController>(out UnitController allayController))
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
                PassiveItem passiveItem = item.Key.GetComponent<PassiveItem>();
                passiveItem.ApplyEffect(unit);
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
