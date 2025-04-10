using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RewardChest : MonoBehaviour
{
    [SerializeField] private GameObject rewardUIPrefab;
    private RectTransform rewardChestViewport;

    private UnitGroupSO[] spawnUnitsSO;
    private Item[] item;

    public RewardChest(UnitGroupSO[] spawnUnitsSO, Item[] item, RectTransform rewardChestViewport)
    {
        this.spawnUnitsSO = spawnUnitsSO;
        this.item = item;
        this.rewardChestViewport = rewardChestViewport;
    }

    public void ConnectReward(UnitGroupSO[] spawnUnitsSO, Item[] item, RectTransform rewardChestViewport)
    {
        this.spawnUnitsSO = spawnUnitsSO;
        this.item = item;
        this.rewardChestViewport = rewardChestViewport;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && InputManager.instance.GetInterActionPressed())
        {
            Debug.Log("º¸»ó ¿ÀÇÂ");
            OpenChest();
            Destroy(gameObject);
        }
    }

    public void OpenChest()
    {
        Time.timeScale = 0;
        RewardUI rewardUIP = FindAnyObjectByType<RewardUI>();
        rewardUIP.OpenViewport();
        for (int i = 0; i < spawnUnitsSO.Length; i++)
        {
            GameObject rewardUI = Instantiate(rewardUIPrefab, rewardChestViewport);
            if(spawnUnitsSO.Any())
            {
                rewardUI.GetComponent<RewardChestUI>().rewardUnitGroup = spawnUnitsSO[i];
            }
            else if(item.Any())
            {
                rewardUI.GetComponent<RewardChestUI>().rewardItem = item[i];
            }
        }
    }
}
