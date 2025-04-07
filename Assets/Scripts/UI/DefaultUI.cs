using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DefaultUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI groupPower;
    private void Update()
    {
        groupPower.text = PlayerUnitManager.instance.playerGroupPower.ToString();
    }
}
