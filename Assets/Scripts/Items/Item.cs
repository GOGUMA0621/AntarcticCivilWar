using SciptableObjects;
using UnityEngine;

public abstract class Item
{
    public ItemData itemData;

    [HideInInspector] public string id;
    [HideInInspector] public string name;
    [HideInInspector] public string abilityDescription;
    [HideInInspector] public string description;
    [HideInInspector] public Sprite icon;
    [HideInInspector] public ItemRarity itemRarity;
    [HideInInspector] public float itemCooldown;
    [HideInInspector] public int itemStack;

    public Item(ItemData data)
    {
        this.itemData = data;
        SetItemToData();
    }

    protected void SetItemToData()
    {
        if (itemData == null)
        {
            Debug.LogError("ItemData is null! Please assign a valid ItemData.");
            return;
        }

        id = itemData.Id;
        name = itemData.Name;
        abilityDescription = itemData.AbilityDescription;
        description = itemData.Description;
        icon = itemData.Icon;
        itemRarity = itemData.Rarity;
        itemCooldown = itemData.ItemCooldown;
    }

    // 모든 유닛에게 적용할 효과
    public abstract void ApplyToUnit(UnitController unit);

    // 아이템이 제거될 때 모든 유닛에서 해제할 효과
    public abstract void RemoveFromUnit(UnitController unit);
}
