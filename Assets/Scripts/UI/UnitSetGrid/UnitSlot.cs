using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitSlot : MonoBehaviour
{
    [SerializeField] public Image icon;
    [SerializeField] private GameObject unitPrefab;
    [NonSerialized]
    public UnitDragFromUI dragHandler;

    [Header("테스트용")]
    [SerializeField] private Sprite testSprite;
    [SerializeField] private GameObject testUnitPrefab;

    void Awake()
    {
        if (unitPrefab == null)
        {
            icon.enabled = false;
        }
        dragHandler = GetComponentInChildren<UnitDragFromUI>();
        if (dragHandler == null)
            Debug.LogWarning("UnitSlot의 자식에서 UnitDragFromUI를 찾을 수 없습니다.");
    }

    void Start()
    {
        // 테스트용 유닛 설정
        if( testSprite == null || testUnitPrefab == null)
        {
            Debug.LogWarning("테스트용 스프라이트나 유닛 프리팹이 설정되지 않았습니다.");
            return;
        }
        SetUnit(testSprite, testUnitPrefab);
    }

    /// <summary>
    /// 아이콘 이미지와 드래그할 유닛 프리팹을 슬롯에 설정.
    /// </summary>
    /// <param name="unitIcon">유닛 아이콘으로 사용할 스프라이트</param>
    /// <param name="unitPrefab">드래그 시 인게임에 배치될 유닛 프리팹</param>
    public void SetUnit(Sprite unitIcon, GameObject unitPrefab)
    {
        Debug.Log($"SetUnit 호출됨 - {unitPrefab.name}");
        icon.sprite = unitIcon;
        this.unitPrefab = unitPrefab;

        dragHandler.unitPrefab = unitPrefab;
        icon.enabled = true;
        dragHandler.enabled = true;
    }

    /// <summary>
    /// 유닛이 배치되었을 때 해당 슬롯을 초기화.
    /// 아이콘과 드래그 기능을 비활성화.
    /// </summary>
    public void Clear()
    {
        icon.sprite = null;
        icon.enabled = false;
        dragHandler.unitPrefab = null;
        dragHandler.enabled = false;
    }
}
