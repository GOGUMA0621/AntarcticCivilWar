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

public class UnitManager : SingleTonBehaviour<UnitManager>
{
    [SerializeField] public List<UnitController> allayList = new List<UnitController>();
    [SerializeField] public List<UnitController> enemyList = new List<UnitController>();

    

    private List<PassiveItem> itemUpdateEffects = new();

    public List<UnitPrefabEntry> allayPrefabList = new List<UnitPrefabEntry>();

    public List<UnitController> unitToRevive = new List<UnitController>();

    public int playerGroupPower = 0;

    protected override void Awake()
    {
        base.Awake();
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

    #region 무리력
    public void CalculatePlayerGroupPower()
    {
        int playerGroupPower = 0;
        foreach (var allay in allayPrefabList)
        {
            var allaydata = allay.prefab.GetComponent<Unit>().data;
            playerGroupPower += allaydata.UnitTier * allay.count;
        }
        this.playerGroupPower = playerGroupPower;
    }

    public void AddUnitToRevive(UnitController unitController)
    {
        if (unitToRevive.Contains(unitController)) return;
        unitToRevive.Add(unitController);
    }

    public void ReviveAllUnit()
    {
        foreach(UnitController unitController in unitToRevive)
        {
            unitController.Revive();
        }
        unitToRevive.Clear();
    }

    #endregion

    #region  아군 관리
    public void AddAllayList(UnitController allay)
    {
        if (!allayList.Contains(allay))
        {
            allayList.Add(allay);
            ApplyItemToUnit(allay);
            AddUnitPrefabList(allay.unit.originPrefab);
            CalculatePlayerGroupPower();
        }
    }
    public void RemoveAllayList(UnitController allay)
    {
        if (allayList.Contains(allay))
        {
            allayList.Remove(allay);
            RemoveUnitPrefabList(allay.unit.originPrefab);
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

    public bool HasUnit(string unitName)
    {
        foreach (var unit in allayList)
        {
            if (unit.unit.data.UnitName == unitName)
            {
                return true;
            }
        }
        Debug.LogError($"유닛 {unitName}이(가) allayList에 없습니다.");
        return false;
    }
    #endregion

    #region 적군 관리
    public void AddEnemyList(UnitController enemy)
    {
        if (!enemyList.Contains(enemy))
        {
            enemyList.Add(enemy);
        }
    }

    public void RemoveEnemyList(UnitController enemy)
    {
        if (enemyList.Contains(enemy))
        {
            enemyList.Remove(enemy);
        }
    }
    
    public void ChangeStateEnemyList(string unitState)
    {
        foreach (var unit in enemyList)
        {
            if (unit.TryGetComponent<UnitController>(out UnitController unitController))
            {
                switch (unitState)
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

    #region 프리팹 관리
    public void SpawnPlayerUnits(Vector3 spawnPos)
    {
        List<UnitPrefabEntry> copy = new List<UnitPrefabEntry>(allayPrefabList);
        allayList.Clear();
        foreach (var unit in copy)
        {
            for (int i = 0; i < unit.count; i++)
            {
                GameObject allay = Instantiate(unit.prefab, spawnPos, Quaternion.identity);
                UnitController allayController = allay.GetComponent<UnitController>();
                allayController.unit.originPrefab = unit.prefab;
                allay.tag = "Unit";
                allayList.Add(allayController);
            }
        }
        CalculatePlayerGroupPower();
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
                UnitController allayController = allay.GetComponent<UnitController>();
                allayController.unit.originPrefab = unit.pfUnit;
                allay.tag = "Unit";

                allayList.Add(allayController);
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
            foreach (UnitController allay in allayList)
            {
                passiveItem.ApplyEffect(allay);
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
            foreach (UnitController allay in allayList)
            {
                item.ApplyEffect(allay);
            }
        }
    }
    #endregion

    public void AssignTargetsToAllUnits()
    {
        // 아군 유닛에게 적군 리스트를 타겟으로 할당
        foreach (var allay in allayList)
        {
            List<IDamageAble> enemyObjects = enemyList
                .Where(e => e != null && e.gameObject.TryGetComponent<IDamageAble>(out _))
                .Select(e => e.gameObject.GetComponent<IDamageAble>())
                .ToList();
            allay.unit.detectTarget.AddTargets(enemyObjects);
        }

        // 적군 유닛에게 아군 리스트를 타겟으로 할당
        foreach (var enemy in enemyList)
        {
            List<IDamageAble> allayObjects = allayList
                .Where(a => a != null && a.gameObject.TryGetComponent<IDamageAble>(out _))
                .Select(a => a.gameObject.GetComponent<IDamageAble>())
                .ToList();
            enemy.unit.detectTarget.AddTargets(allayObjects);
        }
    }
}
