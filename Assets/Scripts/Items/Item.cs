using SciptableObjects;
using System;
using UnityEngine;

public enum ItemRarity
{
    Silver,
    Gold,
    Platinum,
    Diamond,
    Special
}

public abstract class Item : MonoBehaviour
{
    public ItemRarity itemRarity { get; private set; }

    public string itemId;
    public string itemName;
    public string itemDescription;
    public string itemAbilityDescription;
    public float itemCooldown;
    public int itemPrice;
    public Sprite icon;

}

public abstract class ActiveItem : Item
{
    public abstract void Use(UnitController unit);
}

public abstract class PassiveItem : Item, IPassiveItem
{
    protected int currentStack = 1;
    protected float currentCooldown = 0;

    [SerializeField] protected float effectBaseValue;
    [SerializeField] protected float effectStackValue;
    [SerializeField] protected float effectDuration;

    public abstract void ApplyEffect(UnitController unit);

    public abstract void UpdateEffect(UnitController unit);

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
