using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UnitDugout : MonoBehaviour, IWorldDropHandler
{
    [HideInInspector] public UnitController unitInDugout;

    public bool isDropAllowed => true;

    public void OnDrop(DragEventData data)
    {
        Debug.Log("OnDrop 호출됨 - 유닛이 참호에 드롭되었습니다." + this.transform.name);
        var unit = data.source.GetComponent<UnitController>();

        if (unitInDugout != null)
        {
            Debug.LogWarning("참호에 이미 유닛이 있습니다. 기존 유닛을 교체합니다.");
            unitInDugout.transform.position = unit.transform.position; // 기존 유닛을 드롭한 위치로 이동

            // 기존 유닛을 드래그 상태로 전환
            var dragController = unitInDugout.GetComponent<UnitDragController>();
            if (dragController != null)
            {
                DragEventData newDragData = new DragEventData
                (
                    unitInDugout.transform.position,
                    InputManager.instance.GetPointerScreenPosition(),
                    Vector2.zero,
                    true,
                    Time.time,
                    unitInDugout
                );
                InputManager.instance.dragController.BeginDrag(newDragData);
            }
        }
        SetUnitInDugout(unit);

    }

    public void SetUnitInDugout(UnitController unit)
    {
        unitInDugout = unit;
        unit.transform.position = this.transform.position; // 참호 중앙에 위치

        // Null 체크 추가
        if (unit.unit == null)
        {
            Debug.LogError("UnitController의 unit 필드가 할당되어 있지 않습니다! " + unit.name);
            return;
        }
        if (unit.unit.rb == null)
        {
            Debug.LogError("Unit의 rb 필드가 할당되어 있지 않습니다! " + unit.name);
            return;
        }
        unit.transform.tag = "Allay"; // 태그를 Allay로 변경
        Debug.Log("SetUnitInDugout 호출됨 - 유닛이 참호에 배치되었습니다." + unitInDugout.name);
    }

    public void RemoveUnitFromDugout()
    {
        if (unitInDugout != null)
        {
            Destroy(unitInDugout.gameObject);
            unitInDugout = null;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnDragSourceRemoved(DragEventData data)
    {
        if (unitInDugout != null && unitInDugout.unit != null && unitInDugout.unit.rb != null)
        {
            unitInDugout.transform.SetParent(null);
            unitInDugout = null;
        }
        else
        {
            unitInDugout = null;
        }
    }
}
