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
    [SerializeField] private TextMeshProUGUI itemRarity;

    [SerializeField] private Button buyButton;
    [SerializeField] private Button closeButton;

    private ItemDB currentItem;

    private void Awake()
    {
        Instance = this;
        gameObject.SetActive(false);

        closeButton.onClick.AddListener(Close);
        buyButton.onClick.AddListener(BuyItem);
    }

    public void Open(ItemDB item)
    {
        currentItem = item;

        //itemImg.sprite = Resources.Load<Sprite>($"Icons/{item.name}");
        itemImg.sprite = Resources.Load<Sprite>("Icons/OldDagger");
        itemName.text = item.name_kr;
        itemDes.text = item.des;
        itemRarity.text = item.rarity;
        itemPrice.text = item.price.ToString();

        gameObject.SetActive(true);
    }

    private void BuyItem()
    {
        if (PlayerStats.Currency >= currentItem.price)
        {
            PlayerStats.Currency -= currentItem.price;
            //InventorySystem.AddItem(currentItem); // 인벤토리 시스템에서 처리
            Debug.Log($"{currentItem.name} 아이템 구매 완료!");

            Close();
            // 상점 재화 UI 갱신하려면 BlackMarketManager의 public 메서드 호출 가능
        }
        else
        {
            Debug.Log("재화가 부족합니다!");
        }
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }
}
