using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public int coinAmount;

    // Start is called before the first frame update
    void Start()
    {
        PlayerUnitManager.instance.SpawnPlayerUnits(this.transform.position);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
