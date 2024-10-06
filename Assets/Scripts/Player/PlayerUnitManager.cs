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
                unit.detectTarget.AddTarget(enemy);
            }
        }
    }
    internal void RemoveAllayList(Unit unit)
    {
        if (allayList.Contains(unit))
        {
            allayList.Remove(unit);
            //Debug.Log("酒焙 府胶飘 昏力" + unit.ToString());
            foreach (Unit enemy in enemyList)
            {
                enemy.detectTarget.RemoveTarget(unit);
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
                allay.detectTarget.AddTarget(unit);
            }
        }
    }

    internal void RemoveEnemyList(Unit unit)
    {
        if (enemyList.Contains(unit))
        {
            //Debug.Log("利 府胶飘 昏力"+unit.ToString());
            enemyList.Remove(unit);
            foreach(Unit allay in allayList)
            {
                allay.detectTarget.RemoveTarget(unit);
            }
        }
    }
    #endregion
}
