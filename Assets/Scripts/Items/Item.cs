using SciptableObjects;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public enum ItemRarity
{
    silver,
    gold,
    platinum,
    diamond,
    special
}

public enum  ItemType
{
    Weapon,
    Armor,
    Accessory,
    Chemical,
    Food
}

public abstract class Item : MonoBehaviour
{
    public ItemRarity itemRarity;

    object lockObject = new object();

    public string itemId;
    public string itemName;
    public string itemDescription;
    public string itemAbilityDescription;
    public float itemCooldown;
    public int itemPrice;
    public Sprite icon;

    protected async virtual void Start()
    {
        await FirebaseManager.ItemLoadData();
        lock (lockObject)
        {
            var item = FirebaseManager.GetItemByID(int.Parse(itemId));
            itemName = item.name_kr;
            itemDescription = item.des;
            itemAbilityDescription = item.effect;
            itemCooldown = item.cooltime;
            itemPrice = item.price;
            itemRarity = (ItemRarity)Enum.Parse(typeof(ItemRarity), item.rarity);
        }
    }

}

public abstract class ActiveItem : Item
{
    public abstract void Use(UnitController unit);
}

public abstract class PassiveItem : Item, IPassiveItem
{
    protected int currentStack = 1;
    protected float currentCooldown = 0;

    [SerializeField] protected List<float> effectBaseValue;
    [SerializeField] protected List<float> effectStackValue;
    [SerializeField] protected float effectDuration;

    public abstract void ApplyEffect(UnitController unit);

    public abstract void UpdateEffect(UnitController unit);

    protected override void Start()
    {
        base.Start();
        var item = FirebaseManager.items[int.Parse(itemId)];
        effectBaseValue = item.base_effect;
        effectStackValue = item.stack_effect;
    }

    public virtual void IncreaseStack()
    {
        currentStack++;
    }

    public virtual void DecreaseStack()
    {
        if (currentStack > 1)
        {
            currentStack--;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}


public abstract class StatItem : PassiveItem
{

}

public abstract class AttackedItem : PassiveItem
{

}

public abstract class OnHitItem : PassiveItem
{

}



public interface IPassiveItem
{

}

