using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : SingleTonBehaviour<InventoryManager>
{
    public Dictionary<GameObject, int> inventoryItems = new Dictionary<GameObject, int>();
    //private List<GameObject> inventoryUIList = new List<GameObject>();
    public RectTransform content;

    public void AddItem(GameObject item)
    {
        if (inventoryItems.ContainsKey(item))
        {
            inventoryItems[item]++;
            SetItemStack(item);
            PassiveItem PassiveItem = item.GetComponent<PassiveItem>();
            PassiveItem.IncreaseStack();
            UnitManager.instance.ApplyItemToUnits(PassiveItem);
        }
        else
        {
            GameObject newItemPrefab = InstantiateItem(item);
            PassiveItem newItem = newItemPrefab.GetComponent<PassiveItem>();
            UnitManager.instance.ApplyItemToUnits(newItem);
            inventoryItems.Add(item, 1);
        }
    }

    public void RemoveItem(GameObject item)
    {
        if (inventoryItems.ContainsKey(item))
        {
            inventoryItems[item]--;
            SetItemStack(item);
            PassiveItem passiveItem = item.GetComponent<PassiveItem>();
            passiveItem.DecreaseStack();
            UnitManager.instance.ApplyItemToUnits(passiveItem);
        }
        else
        {
            inventoryItems.Remove(item);
            Destroy(item);
        }
    }

    public void ConnectUI(InventoryUI newUI)
    {
        content = newUI.content;

        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }

        foreach (var item in inventoryItems)
        {
            GameObject newItemPrefab = InstantiateItem(item.Key);
            newItemPrefab.GetComponentInChildren<TextMeshProUGUI>().text = item.Value.ToString();
        }
    }

    private GameObject InstantiateItem(GameObject item)
    {
        GameObject newItemPrefab = Instantiate(item, content);
        newItemPrefab.GetComponent<Image>().sprite = item.GetComponent<Item>().icon;
        return newItemPrefab;
    }

    private void SetItemStack(GameObject item)
    {
        if(content != null)
        {
            GameObject itemPrefab = content.Find(item.name+"(Clone)").gameObject;
            itemPrefab.GetComponentInChildren<TextMeshProUGUI>().text = inventoryItems[item].ToString();
        }
    }
}
[Serializable]
public class ItemPrefab
{
    public GameObject item;
    public int stack;
}
