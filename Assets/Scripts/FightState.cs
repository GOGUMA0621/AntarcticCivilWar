using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FightState : MonoBehaviour
{
    Button fightButton; // 전투 시작 버튼
    [SerializeField] private UnitWaitingContainer waitingContainer;

    void Start()
    {
        fightButton = GetComponent<Button>();
        fightButton.onClick.AddListener(OnFightState);
    }

    private void OnFightState()
    {
        waitingContainer.DisalbeUnitSlots(); // 유닛 슬롯 비활성화
        waitingContainer.DownContainer(); // 컨테이너 내려가기
        foreach (var unit in UnitManager.instance.allayList)
        {
            if (unit == null) continue; // 유닛이 파괴되었을 경우를 대비
            unit.GoIdle();
        }

        foreach (var enemy in UnitManager.instance.enemyList)
        {
            if (enemy == null) continue; // 적이 파괴되었을 경우를 대비
            foreach (var target in UnitManager.instance.allayList)
            {
                if (target == null) continue; // 타겟이 파괴되었을 경우를 대비
                enemy.unit.detectTarget.AddTarget(target.gameObject); // 적이 공격할 타겟 추가
            }
            enemy.GoIdle();
        }

        fightButton.interactable = false; // 전투 시작 후 버튼 비활성화
    }
}
