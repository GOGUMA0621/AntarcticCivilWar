using System;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : SingleTonBehaviour<InventoryManager>
{
    public Dictionary<GameObject, int> inventoryItems = new Dictionary<GameObject, int>();
    public RectTransform content;

    public void AddItem(GameObject item)
    {
        if (inventoryItems.ContainsKey(item))
        {
            inventoryItems[item]++;
            PassiveItem PassiveItem = item.GetComponent<PassiveItem>();
            PassiveItem.IncreaseStack();
            PlayerUnitManager.Instance.ApplyItemToUnits(PassiveItem);
        }
        else
        {
            GameObject newItemPrefab = Instantiate(item, content);
            PassiveItem newItem = newItemPrefab.GetComponent<PassiveItem>();
            PlayerUnitManager.Instance.ApplyItemToUnits(newItem);
            inventoryItems.Add(item, 1);
        }
    }
}


[Serializable]
public class InventoryItem
{
    public PassiveItem item;
    public int stack;
}
