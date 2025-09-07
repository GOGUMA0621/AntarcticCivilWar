using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UnitDugout : MonoBehaviour, IWorldDropHandler
{
    [HideInInspector] public UnitController unitInDugout;

    public void OnDrop(DragEventData data)
    {
        Debug.Log("OnDrop 호출됨 - 유닛이 참호에 드롭되었습니다.");
        var unit = data.source.GetComponent<UnitController>();

        if (unitInDugout != null)
        {
            Debug.LogWarning("참호에 이미 유닛이 있습니다. 기존 유닛을 교체합니다.");
            unitInDugout.unit.rb.simulated = true;
            unitInDugout.transform.position = unit.transform.position; // 기존 유닛을 드롭한 위치로 이동

            // 기존 유닛을 드래그 상태로 전환
            var dragController = unitInDugout.GetComponent<UnitGridDragController>();
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
        unit.transform.SetParent(this.transform); // 부모를 참호로 지정
        unit.transform.position = this.transform.position;
        unit.unit.rb.simulated = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
