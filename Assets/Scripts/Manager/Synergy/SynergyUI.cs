using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SynergyUI : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI countText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Image tierIcon;

    public void SetSynergyData(SynergyUIData data)
    {
        if (icon != null)
        {
            icon.sprite = data.icon;
        }

        if (nameText != null)
        {
            nameText.text = data.name;
        }

        if (countText != null)
        {
            countText.text = data.count.ToString();
        }

        if (descriptionText != null)
        {
            descriptionText.text = data.description;
        }
        
        if (tierIcon != null)
        {
            tierIcon.sprite = SynergyManager.instance.GetTierIcon(data.tier);
        }
    }
}
