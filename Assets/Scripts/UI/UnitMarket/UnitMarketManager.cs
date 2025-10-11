using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UnitMarketManager : MonoBehaviour
{
   // [SerializeField] private Player player;
    [SerializeField] private UnitMarketUI unitMarketUI;
    //[SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private UnitBench unitBench;
    [SerializeField] private bool isInfiniteUnit = false;

    [SerializeField] private Button toggleMarketButton;
    [SerializeField] private Button rerollButton;
    [SerializeField] private Image toggleMarketButtonImage;

    private Sprite openSprite;
    private Sprite closeSprite;

    private void Start()
    {
        Debug.Log("UnitMarketManager 시작");
        UpdateGoldUI();
        LoadMarket();

        openSprite = Resources.Load<Sprite>("Shop/OpenButton");
        closeSprite = Resources.Load<Sprite>("Shop/CloseButton");

        if (toggleMarketButton != null)
            toggleMarketButton.onClick.AddListener(ToggleMarket);
        if (rerollButton != null)
            rerollButton.onClick.AddListener(OnRerollClicked);

        if (toggleMarketButtonImage != null && closeSprite != null)
            toggleMarketButtonImage.sprite = closeSprite;
    }

    public void ToggleMarket()
    {
        if (unitMarketUI.gameObject.activeSelf)
        {
            UnitMarketClose();
            if (toggleMarketButtonImage != null && openSprite != null)
                toggleMarketButtonImage.sprite = openSprite;
        }
        else
        {
            UnitMarketOpen();
            if (toggleMarketButtonImage != null && closeSprite != null)
                toggleMarketButtonImage.sprite = closeSprite;
        }
    }

    public void LoadMarket()
    {
        if (unitMarketUI == null)
        {
            Debug.LogError("unitMarketUI가 없습니다.");
            return;
        }

        Debug.Log("LoadMarket() 호출: GenerateShopUnits() 실행");

        unitMarketUI.GenerateShopUnits();
    }

    public void OnRerollClicked()
    {
        //if (player.coinAmount < 2000)
        //{
        //    Debug.Log("��� ����!");
        //    return;
        //}

        //player.coinAmount -= 2000;
        UpdateGoldUI();
        LoadMarket();
    }

    public void UnitMarketClose()
    {
        rerollButton.gameObject.SetActive(false);
        unitMarketUI.gameObject.SetActive(false);
    }

    public void UnitMarketOpen()
    {
        unitMarketUI.gameObject.SetActive(true);
        rerollButton.gameObject.SetActive(true);
        //첫 로드일 때만 유닛 생성
        if (unitMarketUI.isFirstLoad)
        {
            unitMarketUI.GenerateShopUnits();
            unitMarketUI.isFirstLoad = false;
        }
    }

    public void BuyUnit(int slotIndex)
    {
        UnitDB unitData = unitMarketUI.GetUnitFromSlot(slotIndex);
        if (unitData == null)
        {
            Debug.LogWarning($"슬롯 {slotIndex}에 유닛이 없습니다.");
            return;
        }

        UpdateGoldUI();

        unitBench.AddUnitToBench(unitData);
        if(!isInfiniteUnit)
            unitMarketUI.GetSlotObject(slotIndex).SetActive(false);
    }

    private void UpdateGoldUI()
    {
        //if (goldText != null)
        //    goldText.text = $"{player.coinAmount:N0} G";
    }

    private GameObject LoadUnitPrefab(string unitName)
    {
        GameObject prefab = Resources.Load<GameObject>($"Units/{unitName}");
        if (prefab == null)
            Debug.LogWarning($"유닛 프리팹을 찾을 수 없습니다: {unitName}");
        return prefab;
    }
}
