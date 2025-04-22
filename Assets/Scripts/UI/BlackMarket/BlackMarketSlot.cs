using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BlackMarketSlot : MonoBehaviour
{
    [SerializeField] private Image itemImage;
    [SerializeField] private Image itemFrame;
    [SerializeField] private TextMeshProUGUI itemPrice;
    [SerializeField] private Button buyButton;

    private ItemDB currentItem;

    public void SetItem(ItemDB item)
    {
        Debug.Log(item.name);
        currentItem = item;
        //itemImage.sprite = Resources.Load<Sprite>($"Icons/{item.name}");
        itemImage.sprite = Resources.Load<Sprite>("Icons/OldDagger");
        var itemFrameSprite = PassiveItemManager.GetItemFrame(item.rarity);
        if(itemFrameSprite != null ) itemFrame.sprite = itemFrameSprite;
        itemPrice.text = item.price.ToString();
        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(() => ItemBuyUI.Instance.Open(currentItem));
    }

    public void ClearSlot()
    {
        itemImage.sprite = null;
        currentItem = null;
        buyButton.onClick.RemoveAllListeners();
    }
}
