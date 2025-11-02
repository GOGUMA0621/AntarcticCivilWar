using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 이벤트 관련 유틸리티 기능만 제공하는 간소화된 EventManager
/// 실제 이벤트 실행은 StageRoundManager에서 담당
/// </summary>
public class EventManager : SingleTonBehaviour<EventManager>
{
    [Header("이벤트 관리")]
    private StageRoundManager stageRoundManager;

    protected override void Awake()
    {
        base.Awake();
        
        // StageRoundManager 찾기
        stageRoundManager = FindObjectOfType<StageRoundManager>();
    }

    private void Start()
    {
        if (stageRoundManager == null)
        {
            Debug.LogError("StageRoundManager를 찾을 수 없습니다!");
            return;
        }
    }

    /// <summary>
    /// 이벤트 해금 (StageRoundManager에 위임)
    /// </summary>
    public void UnlockEvent(string eventId)
    {
        Debug.Log($"이벤트 해금: {eventId}");
        
        if (stageRoundManager != null)
        {
            // StageRoundManager에 이벤트 활성화 요청
            Debug.Log($"StageRoundManager에 이벤트 {eventId} 활성화 요청");
        }
    }

    /// <summary>
    /// 발생 가능한 이벤트들 반환 (StageRoundManager에서 가져옴)
    /// </summary>
    public List<StageEventCandidate> GetAvailableEvents()
    {
        if (stageRoundManager == null)
            return new List<StageEventCandidate>();
            
        return stageRoundManager.GetAvailableEventCandidates();
    }

    /// <summary>
    /// 현재 라운드 반환 (StageRoundManager에서 가져옴)
    /// </summary>
    public int GetCurrentRound()
    {
        return stageRoundManager != null ? stageRoundManager.currentRound + 1 : 1;
    }
}