using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class BlackMarketManager : MonoBehaviour
{
    private bool isvisit = false;

    [SerializeField] private BlackMarketSlot[] shopSlots;
    [SerializeField] private Button rerollButton;
    [SerializeField] private Button closeButton;
    //[SerializeField] private TextMeshProUGUI currencyText;

    private List<ItemDB> allItems = new();
    private List<ItemDB> previousItems = new();
    private List<ItemDB> currentItems = new();


    private void Start()
    {
        rerollButton.onClick.AddListener(RerollShop);
        closeButton.onClick.AddListener(CloseShop);
        allItems = FirebaseManager.items.Values.ToList();
    }

    public void OpenShop()
    {
        if (!isvisit)
        {
            GenerateRandomItems();
            isvisit = true;
        }

        UpdateUI();
        gameObject.SetActive(true);
    }

    private void RerollShop()
    {
        GenerateRandomItems();
        UpdateUI();
    }

    private void GenerateRandomItems()
    {
        currentItems.Clear();

        // 이전에 나온 아이템은 이번에는 제외
        var candidates = allItems.Except(previousItems).OrderBy(x => Random.value).Take(3).ToList();

        // 앞에 나온 아이템 중복 확인하고 돌림
        if (candidates.Count < 3)
        {
            var fallback = allItems.OrderBy(x => Random.value).Take(3 - candidates.Count);
            candidates.AddRange(fallback);
        }

        currentItems = candidates;
        previousItems = new List<ItemDB>(currentItems);
    }

    private void UpdateUI()
    {
        for (int i = 0; i < shopSlots.Length; i++)
        {
            if (i < currentItems.Count)
                shopSlots[i].SetItem(currentItems[i]);
            else
                shopSlots[i].ClearSlot();
        }

        //UpdateCurrencyUI();
    }

    //private void UpdateCurrencyUI()
    //{
    //    currencyText.text = PlayerStats.Currency.ToString();
    //}

    private void CloseShop()
    {
        gameObject.SetActive(false);
    }
}
