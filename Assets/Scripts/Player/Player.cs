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
        PlayerUnitManager.instance.SpawnPlayerUnits(this.transform.position);
        yield return new WaitForFixedUpdate();
        PlayerUnitManager.instance.ApplyItemsToAllUnit();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
