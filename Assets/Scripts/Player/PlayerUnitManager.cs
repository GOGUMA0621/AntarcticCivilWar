using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUnitManager : MonoBehaviour
{
    public List<GameObject> allayList;
    public List<GameObject> enemyList;

    // Start is called before the first frame update
    void Start()
    {
        allayList = new List<GameObject>();
        enemyList = new List<GameObject>();
    }

    #region  酒焙 包府
    internal void AddAllayList(GameObject unit)
    {
        if(!allayList.Contains(unit))
        {
            allayList.Add(unit);
            foreach (GameObject enemy in enemyList)
            {
                if (enemy.TryGetComponent<Unit>(out Unit enemyUnit))
                {
                    enemyUnit.unitDetectTarget.AddTarget(enemy);
                }
            }
        }
    }
    internal void RemoveAllayList(GameObject unit)
    {
        if (allayList.Contains(unit))
        {
            allayList.Remove(unit);
            foreach (GameObject enemy in enemyList)
            {
                if (enemy.TryGetComponent<Unit>(out Unit enemyUnit)) 
                { 
                    enemyUnit.unitDetectTarget.RemoveTarget(unit);
                }
            }
        }
    }
    #endregion

    #region 利焙 包府
    internal void AddEnemyList(GameObject unit)
    {
        if (!enemyList.Contains(unit))
        {
            enemyList.Add(unit);
            foreach(GameObject allay in allayList)
            {
                if (allay.TryGetComponent<Unit>(out Unit allayUnit))
                {
                    allayUnit.unitDetectTarget.AddTarget(unit);
                }
            }
        }
    }

    internal void RemoveEnemyList(GameObject unit)
    {
        if (enemyList.Contains(unit))
        {
            enemyList.Remove(unit);
            foreach(GameObject allay in allayList)
            {
                if (allay.TryGetComponent<Unit>(out Unit allayUnit))
                {
                    allayUnit.unitDetectTarget.RemoveTarget(unit);
                }
            }
        }
    }
    #endregion
}
