using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public int coinAmount;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(ApplyItems());
    }

    private IEnumerator ApplyItems()
    {
        UnitManager.instance.SpawnPlayerUnits(this.transform.position);
        yield return new WaitForFixedUpdate();
        UnitManager.instance.ApplyItemsToAllUnit();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
