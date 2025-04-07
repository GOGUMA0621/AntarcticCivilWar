using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] public RectTransform content;

    private void Start()
    {
        InventoryManager.instance.ConnectUI(this);
    }
}
