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
    [SerializeField] private GameObject itemBuyUIPrefab; // ItemBuyUI 프리팹
    //[SerializeField] private TextMeshProUGUI currencyText;

    private List<ItemDB> allItems = new();
    private List<ItemDB> previousItems = new();
    private List<ItemDB> currentItems = new();

    private void Start()
    {
        CloseShop();
        InitializeMarket();
        Debug.Log($"???? ???? {allItems.Count}");
    }

    public void InitializeMarket()
    {
        rerollButton.onClick.AddListener(RerollShop);
        closeButton.onClick.AddListener(CloseShop);
        allItems = FirebaseManager.items.Values.ToList();
        
        // 각 슬롯에 매니저 참조 전달
        foreach (var slot in shopSlots)
        {
            slot.SetBlackMarketManager(this);
        }
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
    /// ?????? ??????? ?????? ?±?
    /// </summary>
    private void GenerateRandomItems()
    {
        currentItems.Clear();

        // ?????? ???? ???????? ??????? ????
        var candidates = allItems.Except(previousItems).OrderBy(x => Random.value).Take(3).ToList();

        // ??? ???? ?????? ??? ?????? ????
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
    
    /// <summary>
    /// ItemBuyUI 프리팹을 생성하여 아이템 구매 UI 열기
    /// </summary>
    /// <param name="item">구매할 아이템</param>
    public void OpenItemBuyUI(ItemDB item)
    {
        if (item == null)
        {
            Debug.LogError("구매할 아이템이 null입니다.");
            return;
        }

        if (itemBuyUIPrefab == null)
        {
            Debug.LogError("ItemBuyUI 프리팹이 할당되지 않았습니다. Inspector에서 할당하세요.");
            return;
        }

        // 메인 캔버스를 찾아서 그 하위에 생성
        Canvas mainCanvas = FindObjectOfType<Canvas>();
        if (mainCanvas == null)
        {
            Debug.LogError("메인 캔버스를 찾을 수 없습니다.");
            return;
        }

        // ItemBuyUI 프리팹 인스턴스화
        GameObject itemBuyUIInstance = Instantiate(itemBuyUIPrefab, mainCanvas.transform);
        ItemBuyUI itemBuyUI = itemBuyUIInstance.GetComponent<ItemBuyUI>();

        if (itemBuyUI != null)
        {
            // BlackMarketUI의 CanvasGroup 찾아서 설정
            CanvasGroup blackMarketCanvasGroup = blackMarketUI.GetComponent<CanvasGroup>();
            if (blackMarketCanvasGroup != null)
            {
                itemBuyUI.SetBlackMarketUI(blackMarketCanvasGroup);
            }
            else
            {
                Debug.LogWarning("BlackMarketUI에 CanvasGroup 컴포넌트가 없습니다.");
            }
            
            // 아이템 정보로 UI 열기
            itemBuyUI.Open(item);
            Debug.Log($"ItemBuyUI 생성 완료: {item.name_kr}");
        }
        else
        {
            Debug.LogError("생성된 프리팹에 ItemBuyUI 컴포넌트가 없습니다.");
            Destroy(itemBuyUIInstance);
        }
    }
}
