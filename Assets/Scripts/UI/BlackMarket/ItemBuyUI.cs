using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemBuyUI : MonoBehaviour
{
    public static ItemBuyUI Instance;

    [SerializeField] private Player player;
    [SerializeField] private CanvasGroup blackMarketUI;
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
    }

    /// <summary>
    /// 아이템 구매창 열림
    /// </summary>
    /// <param name="item"></param>
    public void Open(ItemDB item)
    {
        closeButton.onClick.AddListener(Close);

        buyButton.onClick.AddListener(BuyItem);

        blackMarketUI.blocksRaycasts = false;

        currentItem = item;

        var sprite = Resources.Load<Sprite>($"Icons/{item.name}");

        if (sprite != null)
            itemImg.sprite = sprite;
        else
            itemImg.sprite = Resources.Load<Sprite>("Icons/OldDagger");

        itemName.text = item.name_kr;
        itemDes.text = item.des;
        itemRarity.text = item.rarity.ToString();
        itemPrice.text = item.price.ToString();

        gameObject.SetActive(true);
    }

    /// <summary>
    /// 아이템 구매
    /// </summary>
    private void BuyItem()
    {
        if (player.coinAmount >= currentItem.price)
        {
            player.coinAmount -= currentItem.price;

            if (InventoryUI.Instance != null)
            {
                InventoryUI.Instance.AddItem(currentItem);
                Debug.Log($"{currentItem.name} 아이템 구매 완료");
            }
            else
            {
                Debug.LogWarning("InventoryUI 인스턴스가 없습니다!");
            }
          

            Close();
        }
        else
        {
            Debug.Log("재화가 부족합니다");
        }
    }

    public void Close()
    {
        gameObject.SetActive(false);
        blackMarketUI.blocksRaycasts = true;

        //ㅈ고수의 훈수
        buyButton.onClick.RemoveAllListeners();
        closeButton.onClick.RemoveAllListeners();
    }
}
