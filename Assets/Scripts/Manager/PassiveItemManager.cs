using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PassiveItemManager
{
    private static PassiveItem[] silverItems = Resources.LoadAll<PassiveItem>("Items/Silver");
    private static PassiveItem[] goldItems = Resources.LoadAll<PassiveItem>("Items/Gold");
    private static PassiveItem[] platinumItems = Resources.LoadAll<PassiveItem>("Items/Platinum");
    private static PassiveItem[] diamondItems = Resources.LoadAll<PassiveItem>("Items/Diamond");
    private static PassiveItem[] specialItems = Resources.LoadAll<PassiveItem>("Items/Special");

    private static Sprite silverFrame = Resources.Load<Sprite>("Item/ItemFrame/Silver");
    private static Sprite goldFrame = Resources.Load<Sprite>("Item/ItemFrame/Gold");
    private static Sprite platinumFrame = Resources.Load<Sprite>("Item/ItemFrame/Platinum");
    private static Sprite diamondFrame = Resources.Load<Sprite>("Item/ItemFrame/Diamond");
    private static Sprite specialFrame = Resources.Load<Sprite>("Item/ItemFrame/Special");

    public static PassiveItem[] GetRewardPassiveItems(int count, ItemRarity rarity)
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

    public static Sprite GetItemFrame(ItemRarity rarity)
    {
        switch (rarity)
        {
            case ItemRarity.silver:
                return silverFrame;
            case ItemRarity.gold:
                return goldFrame;
            case ItemRarity.platinum:
                return platinumFrame;
            case ItemRarity.diamond:
                return diamondFrame;
            case ItemRarity.special:
                return specialFrame;

            default:
                return null;
        }
    }

    private static PassiveItem[] GetRandomPassiveItems(PassiveItem[] items, int count)
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
