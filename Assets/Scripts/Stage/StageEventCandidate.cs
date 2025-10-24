using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Stage/EventCandidate")]
public class StageEventCandidate : ScriptableObject
{
    [Header("식별")]
    public string id;                    // 고유 ID (ex: "event_find_relic")
    public string displayName;

    [Header("표시")]
    [TextArea] public string[] dialogLines; // 이벤트에 표시할 다이얼로그(여러 줄)
    public Sprite icon;                  // UI용 이미지/썸네일

    [Header("발생 조건")]
    public int minRound = 0;             // 최소 라운드 번호(옵션)
    public List<string> requiredEventIds = new List<string>(); // 선행 이벤트 ID들 (모두 만족 시 가능)
    public bool isRepeatable = false;    // 반복 가능 여부

    [Header("발생 시 효과 / 언락")]
    public List<string> unlockEventIds = new List<string>(); // 이 이벤트 발생 시 해제하거나 활성화할 이벤트 ID

    [Header("디버그")]
    [Tooltip("초기 활성화(디버그용). 런타임 시작 시 적용됨.")]
    public bool activeByDefault = false;
}
