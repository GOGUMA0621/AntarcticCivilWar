using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUnitManager : MonoBehaviour
{
    public List<GameObject> allayList = new List<GameObject>();
    public List<GameObject> enemyList = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
    }

    #region  酒焙 包府
    public void AddAllayList(GameObject allay)
    {
        if (!allayList.Contains(allay))
        {
            allayList.Add(allay);
            
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

    #region 利焙 包府
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
}
