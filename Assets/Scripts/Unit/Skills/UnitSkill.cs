using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitSkill : MonoBehaviour
{
    public void StopEffect()
    {
        gameObject.SetActive(false);
        Destroy(gameObject, 2f);
    }
}
