using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FightState : MonoBehaviour
{
    Button fightButton; // 전투 시작 버튼
    [SerializeField] private UnitBench unitBench;

    void Start()
    {
        fightButton = GetComponent<Button>();
        fightButton.onClick.AddListener(OnFightState);
    }

    private void OnFightState()
    {
        UnitManager.instance.AssignTargetsToAllUnits();
        UnitManager.instance.ChangeStateAllayList("IdleState");
        UnitManager.instance.ChangeStateEnemyList("IdleState");

        fightButton.interactable = false; // 전투 시작 후 버튼 비활성화
    }
}
