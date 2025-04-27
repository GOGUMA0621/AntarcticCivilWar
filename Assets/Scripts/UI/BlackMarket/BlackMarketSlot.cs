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

    public void SetItem(ItemDB item)
    {
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

        itemPrice.text = item.price.ToString();
        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(() => ItemBuyUI.Instance.Open(currentItem));
    }

    public void ClearSlot()
    {
        itemImg.sprite = null;
        currentItem = null;
        buyButton.onClick.RemoveAllListeners();
    }
}
