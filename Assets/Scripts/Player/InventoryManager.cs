using System;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : SingleTonBehaviour<InventoryManager>
{
    public Dictionary<GameObject, int> inventoryItems = new Dictionary<GameObject, int>();


    public void AddItem(GameObject item)
    {
        if (inventoryItems.ContainsKey(item))
        {
            inventoryItems[item]++;
            PassiveItem PassiveItem = item.GetComponent<PassiveItem>();
            PassiveItem.IncreaseStack();
        }
        else
        {
            GameObject newItemPrefab = Instantiate(item, transform);
            PassiveItem newItem = newItemPrefab.GetComponent<PassiveItem>();
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
