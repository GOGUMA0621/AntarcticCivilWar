using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemManager : SingleTonBehaviour<ItemManager>
{
    [SerializeField] private PassiveItem[] silverItems;
    private PassiveItem[] goldItems;
    private PassiveItem[] platinumItems;
    private PassiveItem[] diamondItems;
    private PassiveItem[] specialItems;

    protected override void Awake()
    {
        base.Awake();
        LoadPassiveItems();
    }

    private void LoadPassiveItems()
    {
        silverItems = Resources.LoadAll<PassiveItem>("Items/Silver");
        goldItems = Resources.LoadAll<PassiveItem>("Items/Gold");
        platinumItems = Resources.LoadAll<PassiveItem>("Items/Platinum");
        diamondItems = Resources.LoadAll<PassiveItem>("Items/Diamond");
        specialItems = Resources.LoadAll<PassiveItem>("Items/Special");
    }

    public PassiveItem[] GetRewardPassiveItems(int count, ItemRarity rarity)
    {
        PassiveItem[] items = null;
        switch (rarity)
        {
            case ItemRarity.Silver:
                items = silverItems;
                break;
            case ItemRarity.Gold:
                items = goldItems;
                break;
            case ItemRarity.Platinum:
                items = platinumItems;
                break;
            case ItemRarity.Diamond:
                items = diamondItems;
                break;
            case ItemRarity.Special:
                items = specialItems;
                break;
        }
        if (items != null && items.Length > 0)
        {
            return GetRandomPassiveItems(items, count);
        }
        return null;
    }

    private PassiveItem[] GetRandomPassiveItems(PassiveItem[] items, int count)
    {
        int currentCount = 0;
        PassiveItem[] randomItems = new PassiveItem[count];
        int randomIndex = Random.Range(0, items.Length);
        while (currentCount + 1 < count)
        {
            if (items[randomIndex] != null)
            {
                randomItems[currentCount] = items[randomIndex];
                currentCount++;
            }
            randomIndex = Random.Range(0, items.Length);
        }
        return randomItems;
    }
}
