using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] public RectTransform content;
    Tween tween;

    private void Start()
    {
        InventoryManager.instance.ConnectUI(this);
    }

    void ExpandUI()
    {
        Vector2 deltaSize = new Vector2(500f, 600f);
        //content.DOSizeDelta(deltaSize, 0.5f).
        tween = content.DOSizeDelta(deltaSize,0.5f).SetEase(Ease.OutQuad);
    }
}
