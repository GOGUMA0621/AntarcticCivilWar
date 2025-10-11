using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LobbyPreset", menuName = "Scriptable Object/LobbyPreset", order = 1)]
public class LobbyPreset : ScriptableObject
{
    public string presetName;
    public List<UnitGroup> startingUnits = new List<UnitGroup>();
    public List<ItemDB> startingItems = new List<ItemDB>();
    public int startingGold = 0;
    [Multiline]
    public string description;
}
