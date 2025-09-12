using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    public static InventoryUI Instance;

    [SerializeField] private GameObject itemSlotPrefab;
    [SerializeField] public RectTransform content;
    [SerializeField] public RectTransform frame;
    [SerializeField] private Button expandButton;
    private List<ItemDB> ownedItems = new();
    Tween tween;

    private bool isExpanded = false;

    private Vector2 originalFrameSize;
    private Vector2 originalContentSize;
    private Vector2 expandedFrameSize = new Vector2(1040f, 800f);
    private Vector2 expandedContentSize = new Vector2(900f, 700f);

    private void Awake()
    {
        Instance = this;

        originalFrameSize = frame.sizeDelta;
        originalContentSize = content.sizeDelta;
        expandButton.onClick.AddListener(ExpandUI);
    }
    private void Start()
    {
        InventoryManager.instance.ConnectUI(this);
    }

    public void ExpandUI()
    {
        if (!isExpanded)
        {
            tween = frame.DOSizeDelta(expandedFrameSize, 0.5f).SetEase(Ease.OutQuad);
            content.DOSizeDelta(expandedContentSize, 0.5f).SetEase(Ease.OutQuad);
            isExpanded = true;
        }
        else
        {
            tween = frame.DOSizeDelta(originalFrameSize, 0.5f).SetEase(Ease.OutQuad);
            content.DOSizeDelta(originalContentSize, 0.5f).SetEase(Ease.OutQuad);
            isExpanded = false;
        }
    }
    public void AddItem(ItemDB item)
    {
        ownedItems.Add(item);
        GameObject itemslot = Instantiate(itemSlotPrefab, content);
        InvenSlot invenSlot = itemslot.GetComponent<InvenSlot>();
        invenSlot.SetItemImg(item);
    }
}
