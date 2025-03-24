using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUnitManager : SingleTonBehaviour<PlayerUnitManager>
{
    public List<GameObject> allayList = new List<GameObject>();
    public List<GameObject> enemyList = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
    }

    #region  아군 관리
    public void AddAllayList(GameObject allay)
    {
        if (!allayList.Contains(allay))
        {
            allayList.Add(allay);
            ApplyItemToUnit(allay.GetComponent<UnitController>());
        }
    }
    public void RemoveAllayList(GameObject allay)
    {
        if (allayList.Contains(allay))
        {
            allayList.Remove(allay);

        }
    }
    #endregion

    #region 적군 관리
    public void AddEnemyList(GameObject enemy)
    {
        if(!enemyList.Contains(enemy))
        {
            enemyList.Add(enemy);
        }
    }

    public void RemoveEnemyList(GameObject enemy)
    {
        if(enemyList.Contains(enemy))
        {
            foreach (GameObject unit in allayList)
            {

            }
            enemyList.Remove(enemy);
        }
    }
    #endregion

    #region 아이템 효과
    public void ApplyItemToAllUnit()
    {
        foreach (var item in InventoryManager.Instance.inventoryItems)
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
        foreach (var item in InventoryManager.Instance.inventoryItems)
        {
            PassiveItem passiveItem = item.Key.GetComponent<PassiveItem>();
            passiveItem.ApplyEffect(unit);
        }
    }
    #endregion
}
