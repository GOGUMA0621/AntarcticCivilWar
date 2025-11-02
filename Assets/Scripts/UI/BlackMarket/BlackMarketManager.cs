using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class BlackMarketManager : MonoBehaviour
{
    private bool isvisit = false;

    [SerializeField] private GameObject blackMarketUI;
    [SerializeField] private BlackMarketSlot[] shopSlots;
    [SerializeField] private Button rerollButton;
    [SerializeField] private Button closeButton;
    //[SerializeField] private TextMeshProUGUI currencyText;

    private List<ItemDB> allItems = new();
    private List<ItemDB> previousItems = new();
    private List<ItemDB> currentItems = new();

    private void Start()
    {
        CloseShop();
        rerollButton.onClick.AddListener(RerollShop);
        closeButton.onClick.AddListener(CloseShop);
        allItems = FirebaseManager.items.Values.ToList();
        Debug.Log($"���� ���� {allItems.Count}");
    }

    public void OpenShop()
    {
        if (!isvisit)
        {
            GenerateRandomItems();
            isvisit = true;
        }
        UpdateUI();
        blackMarketUI.SetActive(true);
        
        // 블랙 마켓이 열릴 때 기존 유닛 상점 버튼 숨기기
        HideUnitMarketButton();
    }

    private void RerollShop()
    {
        GenerateRandomItems();
        UpdateUI();
    }

    /// <summary>
    /// ������ �����ϰ� ����ֱ� �±�
    /// </summary>
    private void GenerateRandomItems()
    {
        currentItems.Clear();

        // ������ ���� �������� �̹����� ����
        var candidates = allItems.Except(previousItems).OrderBy(x => Random.value).Take(3).ToList();

        // �տ� ���� ������ �ߺ� Ȯ���ϰ� ����
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

    }

    private void CloseShop()
    {
        blackMarketUI.SetActive(false);
        
        // 블랙 마켓이 닫힐 때 기존 유닛 상점 버튼 다시 표시
        ShowUnitMarketButton();
    }
    
    /// <summary>
    /// 기존 유닛 상점 버튼 숨기기
    /// </summary>
    private void HideUnitMarketButton()
    {
        UnitMarketManager unitMarketManager = FindObjectOfType<UnitMarketManager>();
        if (unitMarketManager != null)
        {
            unitMarketManager.SetToggleButtonVisibility(false);
            Debug.Log("유닛 상점 버튼 숨김");
        }
        else
        {
            Debug.LogWarning("UnitMarketManager를 찾을 수 없습니다.");
        }
    }
    
    /// <summary>
    /// 기존 유닛 상점 버튼 다시 표시
    /// </summary>
    private void ShowUnitMarketButton()
    {
        UnitMarketManager unitMarketManager = FindObjectOfType<UnitMarketManager>();
        if (unitMarketManager != null)
        {
            unitMarketManager.SetToggleButtonVisibility(true);
            Debug.Log("유닛 상점 버튼 다시 표시");
        }
        else
        {
            Debug.LogWarning("UnitMarketManager를 찾을 수 없습니다.");
        }
    }
}
