using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
class ItemSetUI
{
    public string name;
    public List<ItemPrefab> itemList;
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

        itemSetNameText.text = itemList[index].name;
        foreach (var itemSet in itemList[index].itemList)
        {
            GameObject previewItem = Instantiate(objectPrefab, content);
            previewItem.GetComponent<Image>().sprite = itemSet.item.GetComponent<PassiveItem>().itemData.Icon;
            previewItem.GetComponentInChildren<TextMeshProUGUI>().text = itemSet.stack.ToString();
        }
    }

    public void AddItemToInventory()
    {
        foreach (var itemSet in itemList[currentListIndex].itemList)
        {
            for (int i = 0; i < itemSet.stack; i++)
            {
                InventoryManager.instance.AddItem(itemSet.item);
            }
        }
    }
}
