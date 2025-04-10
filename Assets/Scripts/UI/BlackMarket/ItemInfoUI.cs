using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemInfoUI : MonoBehaviour
{
    public static ItemInfoUI Instance;

    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI desText;
    [SerializeField] private Image iconImage;

    private void Awake()
    {
        Instance = this;
        gameObject.SetActive(false);
    }

    public void Open(ItemDB item)
    {
        nameText.text = item.name_kr;
        desText.text = item.des;
        iconImage.sprite = Resources.Load<Sprite>($"Icons/{item.name}");

        gameObject.SetActive(true);
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }
}
