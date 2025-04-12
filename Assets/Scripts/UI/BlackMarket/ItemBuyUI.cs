using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemBuyUI : MonoBehaviour
{
    public static ItemBuyUI Instance;

    [SerializeField] private Image itemImg;
    [SerializeField] private TextMeshProUGUI itemName;
    [SerializeField] private TextMeshProUGUI itemDes;
    [SerializeField] private TextMeshProUGUI itemPrice;

    private void Awake()
    {
        Instance = this;
        gameObject.SetActive(false);
    }

    public void Open(ItemDB item)
    {
        itemImg.sprite = Resources.Load<Sprite>($"Icons/{item.name}");
        itemName.text = item.name_kr;
        itemDes.text = item.des;
        itemPrice.text = item.price.ToString();

        gameObject.SetActive(true);
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }
}
