using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BlackMarketSlot : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private Button iconButton;

    private ItemDB currentItem;

    public void SetItem(ItemDB item)
    {
        currentItem = item;
        iconImage.sprite = Resources.Load<Sprite>($"Icons/{item.name}");
        iconButton.onClick.AddListener(OnClickIcon);
    }

    private void OnClickIcon()
    {
        ItemInfoUI.Instance.Open(currentItem);
    }

    public void ClearSlot()
    {
        currentItem = null;
        iconImage.sprite = null;
        iconButton.onClick.RemoveAllListeners();
    }
}
