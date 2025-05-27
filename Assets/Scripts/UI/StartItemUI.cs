using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
class ItemSetUI
{
    public string itemSetName = "New Item Set";
    public List<ItemPrefab> items = new List<ItemPrefab>();

    public ItemSetUI(string name, List<ItemPrefab> itemList)
    {
        this.itemSetName = name;
        this.items = itemList;
    }
}

public class StartItemUI : MonoBehaviour
{
    [SerializeField] GameObject objectPrefab;
    [SerializeField] RectTransform content;

    [SerializeField] List<ItemSetUI> itemList = new List<ItemSetUI>();
    [SerializeField] TextMeshProUGUI itemSetNameText;

    private int currentListIndex = 0;

    private void Start()
    {
        PreviewItemSet(currentListIndex);
    }

    public void PreviousItemSet()
    {
        currentListIndex--;
        if (currentListIndex < 0)
            currentListIndex = itemList.Count - 1;
        ClearItemSet();
        PreviewItemSet(currentListIndex);
    }

    public void NextItemSet()
    {
        currentListIndex++;
        if (currentListIndex >= itemList.Count)
            currentListIndex = 0;
        ClearItemSet();
        PreviewItemSet(currentListIndex);
    }

    private void ClearItemSet()
    {
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }
    }

    void PreviewItemSet(int index)
    {

        itemSetNameText.text = itemList[index].itemSetName;
        foreach (var itemSet in itemList[index].items)
        {
            GameObject previewItem = Instantiate(objectPrefab, content);
            previewItem.GetComponent<Image>().sprite = itemSet.item.GetComponent<Item>().icon;
            previewItem.GetComponentInChildren<TextMeshProUGUI>().text = itemSet.stack.ToString();
        }
    }

    public void AddItemToInventory()
    {
        foreach (var itemSet in itemList[currentListIndex].items)
        {
            for (int i = 0; i < itemSet.stack; i++)
            {
                InventoryManager.instance.AddItem(itemSet.item);
            }
        }
    }
}
