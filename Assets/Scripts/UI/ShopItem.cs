using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopItem : MonoBehaviour
{

    private Button shopItemButton;
    public Image itemImage;
    public TextMeshProUGUI itemName;
    public TextMeshProUGUI itemPrice;

    public SciptableObjects.ItemData itemData;

    
    // Start is called before the first frame update
    void Awake()
    {
        shopItemButton = GetComponent<Button>();
        itemName.text = itemData.Name;
        itemImage.sprite = itemData.Icon;
        itemPrice.text = itemData.ItemPrice.ToString();
    }
}
