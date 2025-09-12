using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class UnitWaitingContainer : MonoBehaviour
{
    private UnitSlot[] unitSlots; // 유닛 슬롯 배열

    [Header("UI 설정")]
    [SerializeField] private RectTransform containerRect; // 컨테이너의 RectTransform
    [SerializeField] private Button button; // 버튼 컴포넌트
    [SerializeField] private Image buttonImage; // 버튼 이미지
    [SerializeField] private Sprite upButtonSprite; // 기본 버튼 스프라이트
    [SerializeField] private Sprite downButtonSprite; // 선택된 버튼 스프라이트

    private bool isContainerReady = true; // 버튼이 눌렸는지 여부

    private void Awake()
    {
        unitSlots = GetComponentsInChildren<UnitSlot>();
        if (unitSlots == null || unitSlots.Length == 0)
        {
            Debug.LogError("유닛 슬롯이 설정되지 않았습니다. UnitSlot 컴포넌트를 자식 오브젝트에 추가해주세요.");
        }

        button.onClick.AddListener(ToggleButtonState);
    }

    /// <summary>
    /// 유닛 슬롯에 유닛을 설정합니다.
    /// </summary>
    /// <param name="unitIcon">유닛 아이콘 스프라이트</param>
    /// <param name="unitPrefab">유닛 프리팹</param>
    public void SetUnitToSlot(GameObject unitPrefab)
    {
        foreach (var slot in unitSlots)
        {
            if (slot != null && slot.icon.sprite == null) // 빈 슬롯 찾기
            {
                slot.SetUnit(unitPrefab);
                return;
            }
        }
        Debug.LogWarning("모든 슬롯이 이미 채워져 있습니다.");
    }

    /// <summary>
    /// 모든 유닛 슬롯을 초기화합니다.
    /// </summary>
    public void ClearAllSlots()
    {
        foreach (var slot in unitSlots)
        {
            if (slot != null)
            {
                slot.Clear();
            }
        }
    }

    private void ToggleButtonState()
    {
        isContainerReady = !isContainerReady;
        buttonImage.sprite = isContainerReady ? downButtonSprite : upButtonSprite;

        // 컨테이너의 높이를 토글
        containerRect.DOAnchorPosY(
            isContainerReady ? 0 : -containerRect.rect.height,
            0.3f
        ).OnStart(() =>
        {
            button.interactable = false; // 애니메이션 시작 시 버튼 비활성화
        }).OnComplete(() =>
        {
            button.interactable = true; // 애니메이션 완료 후 버튼 활성화
        }).SetEase(Ease.OutCubic);
    }

    public void DownContainer()
    {
        if (!isContainerReady) return; // 이미 내려간 상태면 아무것도 안함
        isContainerReady = false;
        buttonImage.sprite = upButtonSprite;

        // 컨테이너의 높이를 토글
        containerRect.DOAnchorPosY(-containerRect.rect.height, 0.3f)
            .SetEase(Ease.OutCubic)
            .OnStart(() => button.interactable = false)
            .OnComplete(() => button.interactable = true);
    }

    public void UpContainer()
    {
        if (isContainerReady) return; // 이미 올라간 상태면 아무것도 안함
        isContainerReady = true;
        buttonImage.sprite = downButtonSprite;

        // 컨테이너의 높이를 토글
        containerRect.DOAnchorPosY(0, 0.3f)
            .SetEase(Ease.OutCubic)
            .OnStart(() => button.interactable = false)
            .OnComplete(() => button.interactable = true);
    }
    
    public void DisalbeUnitSlots()
    {
        foreach (var slot in unitSlots)
        {
            if (slot != null)
            {
                slot.dragHandler.enabled = false; // 드래그 기능 비활성화
            }
        }
    }
}
