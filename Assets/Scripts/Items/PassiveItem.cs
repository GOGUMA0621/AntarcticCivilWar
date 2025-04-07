using SciptableObjects;
using UnityEngine;

public abstract class PassiveItem : MonoBehaviour
{
    public ItemData itemData;

    protected int currentStack = 1;
    protected float currentCooldown = 0;

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

public abstract class StatItem : PassiveItem, IPassiveItem
{
    public abstract string itemId { get; }
}

public abstract class AttackedItem : PassiveItem, IPassiveItem
{

}

public abstract class OnHitItem : PassiveItem, IPassiveItem
{

}



public interface IPassiveItem
{

}
