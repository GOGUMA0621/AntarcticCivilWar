using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Shop : MonoBehaviour
{
    public List<Button> shopItemButtons = new List<Button>();
    protected Player player;
    public Button BuyButton;

    [SerializeField] private Button _selectedButton;

    void Awake()
    {
        player = FindAnyObjectByType<Player>();
        foreach(Button button in shopItemButtons) 
        {
            button.onClick.AddListener(() => SelectItem(button));
        }
    }
    private void Start()
    {
        gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void SelectItem(Button button)
    {
        _selectedButton = button;
        Debug.Log(button);
    }

    public void BuyItem()
    {
        if (_selectedButton != null)
        {
            Debug.Log("¼±ÅÃ");
            if (_selectedButton.gameObject.TryGetComponent<ShopItem>(out ShopItem item))
            {
                Debug.Log("¹öÆ°");
                if(item.itemData.ItemPrice <= player.coinAmount)
                {
                    Debug.Log("µ·");
                    _selectedButton.interactable = false;
                    _selectedButton = null;
                    player.itemList.Add(item.itemData);
                }
            }
        }
    }
}
