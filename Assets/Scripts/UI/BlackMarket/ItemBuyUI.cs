using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemBuyUI : MonoBehaviour
{
    [SerializeField] private CanvasGroup blackMarketUI;
    [SerializeField] private Image itemImg;
    [SerializeField] private TextMeshProUGUI itemName;
    [SerializeField] private TextMeshProUGUI itemDes;
    [SerializeField] private TextMeshProUGUI itemPrice;
    [SerializeField] private TextMeshProUGUI itemRarity;

    [SerializeField] private Button buyButton;
    [SerializeField] private Button closeButton;

    private ItemDB currentItem;

    private void Awake()
    {
        // 프리팹으로 생성될 때는 활성화 상태로 시작
        gameObject.SetActive(true);
    }

    /// <summary>
    /// BlackMarketUI 참조 설정 (프리팹 생성 시 호출)
    /// </summary>
    /// <param name="blackMarketUIRef">BlackMarket UI의 CanvasGroup</param>
    public void SetBlackMarketUI(CanvasGroup blackMarketUIRef)
    {
        blackMarketUI = blackMarketUIRef;
    }

    /// <summary>
    /// ������ ����â ����
    /// </summary>
    /// <param name="item"></param>
    public void Open(ItemDB item)
    {
        closeButton.onClick.AddListener(Close);
        buyButton.onClick.AddListener(BuyItem);

        if (blackMarketUI != null)
        {
            blackMarketUI.blocksRaycasts = false;
        }

        currentItem = item;

        var sprite = Resources.Load<Sprite>($"Icons/{item.name}");

        if (sprite != null)
            itemImg.sprite = sprite;
        else
            itemImg.sprite = Resources.Load<Sprite>("Icons/OldDagger");

        itemName.text = item.name_kr;
        itemDes.text = item.des;
        itemRarity.text = item.rarity.ToString();
        itemPrice.text = item.price.ToString();

        gameObject.SetActive(true);
    }

    /// <summary>
    /// ������ ����
    /// </summary>
    private void BuyItem()
    {
        // ItemDB를 GameObject로 변환하여 InventoryManager에 추가
        if (InventoryManager.instance != null)
        {
            // ItemDB의 name을 사용하여 Resources에서 프리팹 로드
            GameObject itemPrefab = Resources.Load<GameObject>($"Items/{currentItem.name}");
            
            if (itemPrefab != null)
            {
                InventoryManager.instance.AddItem(itemPrefab);
                Debug.Log($"{currentItem.name_kr} 아이템 구매 완료");
            }
            else
            {
                Debug.LogError($"아이템 프리팹을 찾을 수 없습니다: Items/{currentItem.name}");
                Debug.LogError("Resources/Items/ 폴더에 해당 프리팹이 있는지 확인하세요.");
            }
        }
        else
        {
            Debug.LogError("InventoryManager 인스턴스가 없습니다!");
        }

        Close();
    }

    public void Close()
    {
        // 블랙마켓 UI의 raycast 복원
        if (blackMarketUI != null)
        {
            blackMarketUI.blocksRaycasts = true;
        }

        // 이벤트 리스너 정리
        if (buyButton != null)
            buyButton.onClick.RemoveAllListeners();
        if (closeButton != null)
            closeButton.onClick.RemoveAllListeners();

        // GameObject 삭제 (프리팹 인스턴스이므로)
        Destroy(gameObject);
    }
}
