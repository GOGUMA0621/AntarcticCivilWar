using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UnitMarketUI : MonoBehaviour
{
    public Transform slotGroup;
    public UnitMarketManager unitMarketManager; // 매니저 연결

    private List<UnitDB> selected = new();

    public void GenerateShopUnits()
    {
        Debug.Log("GenerateShopUnits 시작");
        selected.Clear();

        var availableUnits = new List<UnitDB>(FirebaseManager.units.Values);
        while (selected.Count < 4 && availableUnits.Count > 0)
        {
            int i = Random.Range(0, availableUnits.Count);
            selected.Add(availableUnits[i]);
            availableUnits.RemoveAt(i);
        }

        for (int i = 0; i < selected.Count; i++)
        {
            UnitDB unit = selected[i];
            Transform slotTransform = slotGroup.GetChild(i);
            UnitSlotUI slotUI = slotTransform.GetComponent<UnitSlotUI>();

            if (slotUI == null)
            {
                Debug.LogWarning($"슬롯 {i}에 UnitSlotUI가 없습니다.");
                continue;
            }

            Sprite sprite = UnitPrefabsLoader.GetSprite(unit.name);
            if (sprite == null)
            {
                slotUI.Clear();
                continue;
            }

            slotUI.Set(unit, sprite);
        }
    }

    public UnitDB GetUnitFromSlot(int index)
    {
        if (index >= 0 && index < selected.Count)
            return selected[index];
        return null;
    }
}
