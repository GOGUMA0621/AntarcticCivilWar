using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UnitMarketManager : MonoBehaviour
{
   // [SerializeField] private Player player;
    [SerializeField] private UnitMarketUI unitMarketUI;
    [SerializeField] private List<UnitSlot> playerUnitSlots; 
    //[SerializeField] private TextMeshProUGUI goldText;

    [SerializeField] private Button toggleMarketButton;
    [SerializeField] private Image toggleMarketButtonImage;

    private Sprite openSprite;
    private Sprite closeSprite;

    private void Start()
    {
        Debug.Log("UnitMarketManager Start 실행됨");
        UpdateGoldUI();
        LoadMarket();

        openSprite = Resources.Load<Sprite>("Shop/OpenButton");
        closeSprite = Resources.Load<Sprite>("Shop/CloseButton");

        if (toggleMarketButton != null)
            toggleMarketButton.onClick.AddListener(ToggleMarket);

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
            Debug.LogError(" marketUI가 연결되지 않았습니다.");
            return;
        }

        Debug.Log("LoadMarket() 호출됨 → GenerateShopUnits() 실행");

        unitMarketUI.GenerateShopUnits();
    }

    public void OnRerollClicked()
    {
        //if (player.coinAmount < 2000)
        //{
        //    Debug.Log("골드 부족!");
        //    return;
        //}

        //player.coinAmount -= 2000;
        UpdateGoldUI();
        LoadMarket();
    }

    public void UnitMarketClose()
    {
        unitMarketUI.gameObject.SetActive(false);
    }

    public void UnitMarketOpen()
    {
        unitMarketUI.gameObject.SetActive(true);
    }

    public void BuyUnit(int slotIndex)
    {
        UnitDB unit = unitMarketUI.GetUnitFromSlot(slotIndex);
        if (unit == null)
        {
            Debug.LogWarning($"슬롯 {slotIndex}에 유닛 정보 없음");
            return;
        }

        if (playerUnitSlots[slotIndex].icon.enabled == false)
        {
            Debug.LogWarning($"슬롯 {slotIndex}은(는) 이미 구매되었습니다.");
            return;
        }

        //if (player.coinAmount < 10000)
        //{
        //    Debug.Log("골드 부족!");
        //    return;
        //}

        //player.coinAmount -= 10000;
        UpdateGoldUI();

        Debug.Log($"유닛 구매 완료: {unit.name_kr} (Tier {unit.tier})");

        GameObject prefab = UnitPrefabsLoader.GetPrefab(unit.name);
        if (prefab == null) return;

        Sprite icon = UnitPrefabsLoader.GetSprite(unit.name);
        if (icon == null) return;

        // 슬롯에 유닛 할당 및 아이콘 비활성화(구매 완료 표시)
        playerUnitSlots[slotIndex].SetUnit(prefab);
        playerUnitSlots[slotIndex].icon.enabled = false;
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
