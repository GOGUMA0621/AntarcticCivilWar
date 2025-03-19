using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        [field : SerializeField] public ItemRarity Rarity { get; private set; }
        [field : SerializeField] public string Id {  get; private set; }
        [field : SerializeField] public string Name {  get; private set; }

        [field : SerializeField, Multiline] public string Description { get; private set; }
        [field : SerializeField, Multiline] public string AbilityDescription {  get; private set; }
        [field : SerializeField] public float ItemCooldown { get; private set; }

        [field : Space]
        [field : SerializeField] public int ItemPrice { get; private set; }
        [field : SerializeField] public Sprite Icon { get; private set; }
    }
}