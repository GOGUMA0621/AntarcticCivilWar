using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BlackMarketSlot : MonoBehaviour
{
    [SerializeField] private Image itemImg;
    [SerializeField] private Image itemFrame;
    [SerializeField] private Image slotFrame;
    [SerializeField] private TextMeshProUGUI itemPrice;
    [SerializeField] private Button buyButton;

    private ItemDB currentItem;
    private BlackMarketManager blackMarketManager;

    /// <summary>
    /// BlackMarketManager 참조 설정
    /// </summary>
    /// <param name="manager">BlackMarketManager 인스턴스</param>
    public void SetBlackMarketManager(BlackMarketManager manager)
    {
        blackMarketManager = manager;
    }

    public void SetItem(ItemDB item)
    {
        if (item == null)
        {
            Debug.LogError("SetItem에 null 아이템이 전달되었습니다.");
            return;
        }

        Debug.Log(item.name);
        currentItem = item;

        var ItemSprite = Resources.Load<Sprite>($"Icons/{item.name}");
        var raritySprite = Resources.Load<Sprite>($"Frame/{item.rarity}");
        //var itemFrameSprite = PassiveItemManager.GetItemFrame(item.rarity);

        if (ItemSprite != null && raritySprite != null)
        {
            itemImg.sprite = ItemSprite;
            slotFrame.sprite = raritySprite;
            //itemFrame.sprite = itemFrameSprite;
        }
        else
        {
            Debug.LogWarning($"아이템 스프라이트를 로드할 수 없습니다: {item.name}, 레어도: {item.rarity}");
        }

        itemPrice.text = item.price.ToString();
        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(() => {
            if (blackMarketManager != null)
            {
                blackMarketManager.OpenItemBuyUI(currentItem);
            }
            else
            {
                Debug.LogError("BlackMarketManager가 설정되지 않았습니다.");
            }
        });
    }

    public void ClearSlot()
    {
        itemImg.sprite = null;
        currentItem = null;
        buyButton.onClick.RemoveAllListeners();
    }
}
