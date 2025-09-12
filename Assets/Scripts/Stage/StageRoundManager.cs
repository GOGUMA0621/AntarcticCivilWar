using System;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using UnityEngine;
using UnityEngine.UI;

// 라운드 종류
[Serializable]
public enum RoundType
{
    Battle,
    Shop,
    Event,
    Rest
}

// 세부 라운드 타입
public enum BattleRoundType { Normal, Elite }
public enum EventRoundType { All, Special }

// 라운드 후보 클래스
[Serializable]
public class RoundCandidate
{
    public RoundType roundType;
    public string subType; // 세부 타입 이름

    public RoundCandidate(RoundType type, string subType = "")
    {
        roundType = type;
        this.subType = subType;
    }

    public override string ToString()
    {
        return string.IsNullOrEmpty(subType) ? roundType.ToString() : $"{roundType} ({subType})";
    }
}

// 스테이지(전체 진행) 관리 클래스
public class StageRoundManager : SingleTonBehaviour<StageRoundManager>
{

    public int currentRound = 0; // 현재 진행 중인 라운드
    public int maxRounds = 8; // 마지막 라운드는 보스전 전 휴식
    public int candidateCount = 2;
    public int restAvailableAfterRound = 3; // 3라운드 이후부터 휴식 등장 가능
    [SerializeField] GameObject map; // 맵 UI 오브젝트
    [SerializeField] Transform roundContainer; // 라운드 후보 UI 컨테이너
    [SerializeField] GameObject roundCandidatePrefab; // 라운드 후보 UI 프리팹
    public SerializedDictionary<RoundCandidate, Sprite> roundIcons; // 라운드 아이콘 매핑

    // 전체 라운드 후보 리스트 (각 라운드별 후보 리스트)
    public List<List<RoundCandidate>> roundCandidatesList = new List<List<RoundCandidate>>();

    // 실제 선택된 라운드 리스트
    public List<RoundCandidate> selectedRounds = new List<RoundCandidate>();

    void Start()
    {
        GenerateAllRoundCandidates();
        map.SetActive(false); // 초기에는 맵 UI 비활성화
        // 첫 라운드 후보 보여주기 등
    }

    // 전체 라운드 후보 생성
    public void GenerateAllRoundCandidates()
    {
        roundCandidatesList.Clear();
        bool restIncluded = false;

        for (int i = 0; i < maxRounds; i++)
        {
            List<RoundCandidate> candidates = new List<RoundCandidate>();

            // 마지막 라운드는 무조건 휴식
            if (i == maxRounds - 1)
            {
                candidates.Add(new RoundCandidate(RoundType.Rest));
                roundCandidatesList.Add(candidates);
                continue;
            }

            // 전체 후보 풀
            List<RoundCandidate> allPool = new List<RoundCandidate>
            {
                new RoundCandidate(RoundType.Battle, BattleRoundType.Normal.ToString()),
                new RoundCandidate(RoundType.Battle, BattleRoundType.Elite.ToString()),
                new RoundCandidate(RoundType.Shop),
                new RoundCandidate(RoundType.Event, EventRoundType.All.ToString()),
                new RoundCandidate(RoundType.Event, EventRoundType.Special.ToString())
            };

            // 풀에서 최소 2, 최대 3개 랜덤 선택
            int poolCount = UnityEngine.Random.Range(2, 4); // 2 이상 4 미만 → 2 또는 3
            Debug.Log($"라운드 {i + 1} 후보 생성, 풀에서 선택할 개수: {poolCount}");

            List<RoundCandidate> pool = new List<RoundCandidate>();
            List<RoundCandidate> tempPool = new List<RoundCandidate>(allPool);
            for (int j = 0; j < poolCount && tempPool.Count > 0; j++)
            {
                int idx = UnityEngine.Random.Range(0, tempPool.Count);
                pool.Add(tempPool[idx]);
                tempPool.RemoveAt(idx);
            }

            // 휴식 라운드는 restAvailableAfterRound 이후에만 등장 가능
            bool canAddRest = i >= restAvailableAfterRound;

            // 휴식 라운드는 최소 1개 포함
            if (canAddRest && !restIncluded && i == maxRounds - 2)
            {
                candidates.Add(new RoundCandidate(RoundType.Rest));
                restIncluded = true;
            }
            else if (canAddRest && !restIncluded && UnityEngine.Random.value < 0.2f) // 20% 확률로 휴식 추가
            {
                candidates.Add(new RoundCandidate(RoundType.Rest));
                restIncluded = true;
            }

            // 후보 리스트에 pool의 모든 요소 추가 (최소 2, 최대 3개)
            foreach (var round in pool)
            {
                candidates.Add(round);
            }

            roundCandidatesList.Add(candidates);
        }

        // for(int i = 0; i < roundCandidatesList.Count; i++)
        // {
        //     Debug.Log($"라운드 {i + 1} 후보: {string.Join(", ", roundCandidatesList[i])}");
        // }
    }

    // 특정 라운드 후보 보여주기 (예시: 콘솔 출력)
    public void ShowRoundCandidates()
    {
        if (currentRound < 0 || currentRound >= roundCandidatesList.Count) return;
        Debug.Log($"라운드 {currentRound + 1} 후보:");
        map.SetActive(true);
        foreach (var c in roundCandidatesList[currentRound])
        {
            var candidateUI = Instantiate(roundCandidatePrefab, roundContainer);
            candidateUI.GetComponent<Image>().sprite = roundIcons[c];
        }
    }

    // 라운드 선택
    public void SelectRound(int roundIndex, int candidateIndex)
    {
        if (roundIndex < 0 || roundIndex >= roundCandidatesList.Count) return;
        var candidates = roundCandidatesList[roundIndex];
        if (candidateIndex < 0 || candidateIndex >= candidates.Count) return;
        RoundCandidate selected = candidates[candidateIndex];
        selectedRounds.Add(selected);
        Debug.Log($"선택된 라운드: {selected}");
        // 다음 라운드 진행 등
    }
}