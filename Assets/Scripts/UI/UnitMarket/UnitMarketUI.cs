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
    public bool isFirstLoad = true;

    private void Start()
    {
        isFirstLoad = true;
        StartCoroutine(RefreshUnitMarketAfterDelay());
    }

    public void GenerateShopUnits()
    {
        Debug.Log("GenerateShopUnits 시작");
        selected.Clear();

        var availableUnits = new List<UnitDB>(FirebaseManager.units.Values);
        MemberGrade grade = unitMarketManager.GetMemberGrade();

        int slotCount = Mathf.Min(5, slotGroup.childCount);
        for (int slotIdx = 0; slotIdx < slotCount; slotIdx++)
        {
            //등급에 따라 티어 확률로 티어 결정
            int tier = unitMarketManager.GetRandomTier(grade);

            //해당 티어의 유닛만 필터링
            var unitsOfTier = availableUnits.FindAll(u => u.tier == tier);

            //만약 해당 티어 유닛이 없으면 전체에서 랜덤
            if (unitsOfTier.Count == 0)
                unitsOfTier = availableUnits;
            if (unitsOfTier.Count == 0) break;

            int i = Random.Range(0, unitsOfTier.Count);
            UnitDB unit = unitsOfTier[i];
            selected.Add(unit);
            availableUnits.Remove(unit);

            Transform slotTransform = slotGroup.GetChild(slotIdx);
            UnitSlotUI slotUI = slotTransform.GetComponent<UnitSlotUI>();
            slotTransform.gameObject.SetActive(true);
            if (slotUI == null)
            {
                Debug.LogWarning($"슬롯 {slotIdx}에 UnitSlotUI가 없습니다.");
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
    private IEnumerator RefreshUnitMarketAfterDelay()
    {
        GenerateShopUnits();
        yield return new WaitForFixedUpdate();
        gameObject.SetActive(false);
    }

    public UnitDB GetUnitFromSlot(int index)
    {
        if (index >= 0 && index < selected.Count)
            return selected[index];
        return null;
    }

    public GameObject GetSlotObject(int index)
    {
        if (index >= 0 && index < slotGroup.childCount)
            return slotGroup.GetChild(index).gameObject;
        return null;
    }
}
