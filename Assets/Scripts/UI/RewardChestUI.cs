using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RewardChestUI : MonoBehaviour
{
    public Image rewardIcon;
    public TextMeshProUGUI rewardName;
    public TextMeshProUGUI rewardDescription;

    public Image[] images;

    public UnitGroupSO rewardUnitGroup;
    public Item rewardItem;

    public void AddUnitToList()
    {
        Transform playerTransform = FindAnyObjectByType<PlayerController>().GetComponent<Transform>();

        try
        {
            UnitManager.instance.AddUnitSOPrefabList(rewardUnitGroup);
            UnitManager.instance.AddUnitSOAllayList(rewardUnitGroup, playerTransform.position);
        }
        catch(System.Exception e)
        {
            Debug.LogError("Error adding unit to list: " + e.Message);
        }
        Time.timeScale = 1;
    }
}
