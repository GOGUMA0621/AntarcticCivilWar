using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UnitSlotUI : MonoBehaviour
{
    public Image unitImage;
    public Image tierFrame;
    public TextMeshProUGUI[] synergyTexts;
    public TextMeshProUGUI priceText;

    public void Set(UnitDB unit, Sprite sprite)
    {
        unitImage.sprite = sprite;
        tierFrame.sprite = UnitPrefabsLoader.GetTierSprite(unit.tier);
        unitImage.enabled = true;

        for (int i = 0; i < synergyTexts.Length; i++)
        {
            if (i < unit.synergy.Count)
                synergyTexts[i].text = unit.synergy[i];
            else
                synergyTexts[i].text = "";
        }

        priceText.text = "10000"; // 나중에 unit.price로 교체 가능
    }

    public void Clear()
    {
        unitImage.sprite = null;
        foreach (var txt in synergyTexts)
            txt.text = "";
        priceText.text = "";
    }
}
