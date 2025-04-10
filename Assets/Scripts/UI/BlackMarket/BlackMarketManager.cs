using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class BlackMarketManager : MonoBehaviour
{
    [SerializeField] private BlackMarketSlot[] shopSlots;
    [SerializeField] private Button refreshButton;
    [SerializeField] private Button exitButton;
    [SerializeField] private TextMeshProUGUI currencyText;

    private List<ItemDB> shopItems = new();

    private void Start()
    {
        refreshButton.onClick.AddListener(RefreshShop);
        exitButton.onClick.AddListener(CloseShop);
    }

    public void OpenShop()
    {
        GenerateRandomItems();
        UpdateUI();
        gameObject.SetActive(true);
    }

    void GenerateRandomItems()
    {
        shopItems.Clear();
        var allItems = FirebaseManager.items.Values.ToList();
        shopItems = allItems.OrderBy(x => Random.value).Take(6).ToList();
    }

    void UpdateUI()
    {
        for (int i = 0; i < shopSlots.Length; i++)
        {
            if (i < shopItems.Count)
                shopSlots[i].SetItem(shopItems[i]);
            else
                shopSlots[i].ClearSlot();
        }

        UpdateCurrencyUI();
    }

    void RefreshShop()
    {
        GenerateRandomItems();
        UpdateUI();
    }

    void CloseShop()
    {
        gameObject.SetActive(false);
    }

    void UpdateCurrencyUI()
    {
        currencyText.text = PlayerStats.Currency.ToString();
    }
}
