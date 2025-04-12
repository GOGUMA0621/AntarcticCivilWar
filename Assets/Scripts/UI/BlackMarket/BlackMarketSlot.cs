using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BlackMarketSlot : MonoBehaviour
{
    [SerializeField] private Image itemImage;
    [SerializeField] private Button buyButton;

    private ItemDB currentItem;

    public void SetItem(ItemDB item)
    {
        currentItem = item;
        itemImage.sprite = Resources.Load<Sprite>($"Icons/{item.name}");
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
