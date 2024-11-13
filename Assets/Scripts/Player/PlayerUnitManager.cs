using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUnitManager : MonoBehaviour
{
    public List<Unit> allayList;
    public List<Unit> enemyList;

    // Start is called before the first frame update
    void Start()
    {
        allayList = new List<Unit>();
        enemyList = new List<Unit>();
    }

    #region  酒焙 包府
    internal void AddAllayList(Unit unit)
    {
        if(!allayList.Contains(unit))
        {
            allayList.Add(unit);
            foreach (Unit enemy in enemyList)
            {
                unit.unitDetectTarget.AddTarget(enemy);
            }
        }
    }
    internal void RemoveAllayList(Unit unit)
    {
        if (allayList.Contains(unit))
        {
            allayList.Remove(unit);
            foreach (Unit enemy in enemyList)
            {
                enemy.unitDetectTarget.RemoveTarget(unit);
            }
        }
    }
    #endregion

    #region 利焙 包府
    internal void AddEnemyList(Unit unit)
    {
        if (!enemyList.Contains(unit))
        {
            enemyList.Add(unit);
            foreach(Unit allay in allayList)
            {
                allay.unitDetectTarget.AddTarget(unit);
            }
        }
    }

    internal void RemoveEnemyList(Unit unit)
    {
        if (enemyList.Contains(unit))
        {
            enemyList.Remove(unit);
            foreach(Unit allay in allayList)
            {
                allay.unitDetectTarget.RemoveTarget(unit);
            }
        }
    }
    #endregion
}
