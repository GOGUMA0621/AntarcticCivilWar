using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DefaultUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI groupPower;
    [SerializeField] TextMeshProUGUI coin;
    [SerializeField] Player player;
    private void Update()
    {
        coin.text = player.coinAmount.ToString();
        groupPower.text = PlayerUnitManager.instance.playerGroupPower.ToString();
    }
}
