using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitSlot : MonoBehaviour, IBeginWorldDragHandler, IEndWorldDragHandler
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
        if( testUnitPrefab == null)
        {
            Debug.LogWarning("테스트용 스프라이트나 유닛 프리팹이 설정되지 않았습니다.");
            return;
        }
        SetUnit( testUnitPrefab);
    }

    /// <summary>
    /// 아이콘 이미지와 드래그할 유닛 프리팹을 슬롯에 설정.
    /// </summary>
    /// <param name="unitIcon">유닛 아이콘으로 사용할 스프라이트</param>
    /// <param name="unitPrefab">드래그 시 인게임에 배치될 유닛 프리팹</param>
    public void SetUnit(GameObject unitPrefab)
    {
        Debug.Log($"SetUnit 호출됨 - {unitPrefab.name}");
        this.unitPrefab = unitPrefab;
        var unit = unitPrefab.GetComponent<Unit>();
        icon.sprite = unit.data.unitIcon; // 유닛의 아이콘을 슬롯 아이콘으로 설정 


        dragHandler.unitPrefab = unitPrefab;
        icon.enabled = true;
        dragHandler.enabled = true;
    }

    public GameObject GetUnitPrefab()
    {
        return unitPrefab;
    }

    public UnitController GetUnitController()
    {
        if (unitPrefab == null)
            return null;

        var unit = unitPrefab.GetComponent<Unit>();
        if (unit == null)
        {
            Debug.LogWarning("유닛 프리팹에 Unit 컴포넌트가 없습니다.");
            return null;
        }

        var unitController = unit.GetComponent<UnitController>();
        if (unitController == null)
        {
            Debug.LogWarning("유닛 프리팹에 UnitController 컴포넌트가 없습니다.");
            return null;
        }

        return unitController;
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

    public void OnEndDrag(DragEventData data)
    {
        throw new NotImplementedException();
    }

    public void OnBeginDrag(DragEventData data)
    {
        throw new NotImplementedException();
    }
}
