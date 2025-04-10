using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassiveItemManager
{
    private PassiveItem[] silverItems = Resources.LoadAll<PassiveItem>("Items/Silver");
    private PassiveItem[] goldItems = Resources.LoadAll<PassiveItem>("Items/Gold");
    private PassiveItem[] platinumItems = Resources.LoadAll<PassiveItem>("Items/Platinum");
    private PassiveItem[] diamondItems = Resources.LoadAll<PassiveItem>("Items/Diamond");
    private PassiveItem[] specialItems = Resources.LoadAll<PassiveItem>("Items/Special");

    public PassiveItem[] GetRewardPassiveItems(int count, ItemRarity rarity)
    {
        PassiveItem[] items = null;
        switch (rarity)
        {
            case ItemRarity.silver:
                items = silverItems;
                break;
            case ItemRarity.gold:
                items = goldItems;
                break;
            case ItemRarity.platinum:
                items = platinumItems;
                break;
            case ItemRarity.diamond:
                items = diamondItems;
                break;
            case ItemRarity.special:
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
