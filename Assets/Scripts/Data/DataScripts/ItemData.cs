using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemType
{
    UnitShiftor,
    Artifact
}

public enum ItemRarity
{
    Silver,
    Gold,
    Platinum
}
namespace SciptableObjects
{
    [CreateAssetMenu(fileName = "Item Data", menuName = "Scriptable Object/Item Data")]
    [Serializable]
    public class ItemData : ScriptableObject
    {
        [field : SerializeField] public ItemType Type {  get; private set; }
        [field : SerializeField] public ItemRarity Rarity { get; private set; }
        [field : SerializeField] public string Name {  get; private set; }

        [field : SerializeField, Multiline] public string Description { get; private set; }

        [field : Space]
        [field : SerializeField] public int ItemPrice { get; private set; }
        [field : SerializeField] public Sprite Icon { get; private set; }
        [field : SerializeField] public UnitData UnitData { get; private set; }
    }
}