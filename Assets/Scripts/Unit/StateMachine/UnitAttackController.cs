using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitAttackController : MonoBehaviour
{
    private UnitController _unitController;
    // Start is called before the first frame update
    void Start()
    {
        _unitController = GetComponentInParent<UnitController>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
