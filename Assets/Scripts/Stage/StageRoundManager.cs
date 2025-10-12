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
public class RoundCandidate : IEquatable<RoundCandidate>
{
    public RoundType roundType;
    public string subType; // 세부 타입 이름

    public RoundCandidate(RoundType type, string subType = "")
    {
        roundType = type;
        this.subType = subType ?? "";
    }

    public override string ToString()
    {
        return string.IsNullOrEmpty(subType) ? roundType.ToString() : $"{roundType} ({subType})";
    }

    // 값 기반 비교 구현
    public bool Equals(RoundCandidate other)
    {
        if (ReferenceEquals(this, other)) return true;
        if (other is null) return false;
        return this.roundType == other.roundType && string.Equals(this.subType, other.subType);
    }

    public override bool Equals(object obj) => Equals(obj as RoundCandidate);

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 31 + (int)roundType;
            hash = hash * 31 + (subType != null ? subType.GetHashCode() : 0);
            return hash;
        }
    }

    public static bool operator ==(RoundCandidate a, RoundCandidate b) => Equals(a, b);
    public static bool operator !=(RoundCandidate a, RoundCandidate b) => !Equals(a, b);
}

[Serializable]
public class EnemyPool
{
    public string poolName;
    public List<TextAsset> enemyJsons = new List<TextAsset>(); // inspector에서 JSON 파일들 할당
}

// 스테이지(전체 진행) 관리 클래스
public class StageRoundManager : SingleTonBehaviour<StageRoundManager>
{
    [Header("라운드")]
    public int currentRound = 0; // 현재 진행 중인 라운드
    public int maxRounds = 8; // 마지막 라운드는 보스전 전 휴식
    public int candidateCount = 2;
    public int restAvailableAfterRound = 3; // 3라운드 이후부터 휴식 등장 가능
    [Header("스테이지 지도")]
    [SerializeField] GameObject map; // 맵 UI 오브젝트
    [SerializeField] Transform roundContainer; // 라운드 후보 UI 컨테이너
    [SerializeField] GameObject roundCandidatePrefab; // 라운드 후보 UI 프리팹
    public SerializedDictionary<RoundCandidate, Sprite> roundIcons; // 라운드 아이콘 매핑

    // 새로 추가: 전투 맵 프리팹과 그리드 레퍼런스, JSON 경로 포맷
    [Header("전투")]
    [SerializeField] private PlacementGridManager defaultGrid; // 씬의 기존 그리드를 할당해두면 우선 사용
    [Tooltip("Resources 경로 포맷. 예: Enemies/Round_{0}_{1} -> Resources/Enemies/Round_1_1.json")]
    public string enemyJsonResourcePathFormat = "Enemies/Round_{0}_{1}";

    [Header("구역 풀 (적 소환을 랜덤화))")]
    [Tooltip("한 구역에 몇 라운드(예: 5)인지")]
    [SerializeField] private int zoneSize = 5;

    [Tooltip("구역별로 사용할 Enemy JSON 풀을 할당하세요. 총 라운드가 15면 zoneSize=5 로 zonePools 3개를 만드세요.")]
    [SerializeField] private List<EnemyPool> zonePools = new List<EnemyPool>();

    // 전체 라운드 후보 리스트 (각 라운드별 후보 리스트)
    public List<List<RoundCandidate>> roundCandidatesList = new List<List<RoundCandidate>>();

    [Header("선택 라운드")]
    // 실제 선택된 라운드 리스트
    [Tooltip("플레이어가 선택한 라운드들이 여기에 추가됩니다.")]
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
        map.SetActive(true);

        // 기존 UI 정리
        if (roundContainer != null)
        {
            foreach (Transform t in roundContainer) Destroy(t.gameObject);
        }

        var candidates = roundCandidatesList[currentRound];
        for (int i = 0; i < candidates.Count; i++)
        {
            var c = candidates[i];
            var candidateUI = Instantiate(roundCandidatePrefab, roundContainer);
            var img = candidateUI.GetComponent<Image>() ?? candidateUI.GetComponentInChildren<Image>();
            if (img != null && roundIcons != null && roundIcons.ContainsKey(c))
                img.sprite = roundIcons[c];
            Debug.Log($"라운드 {currentRound + 1} 후보 {i + 1}: {c}" + (roundIcons.ContainsKey(c) ? " (아이콘 있음)" : " (아이콘 없음)"));
            var btn = candidateUI.GetComponent<Button>() ?? candidateUI.GetComponentInChildren<Button>();
            int candidateIndex = i;
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => OnRoundCandidateSelected(currentRound, candidateIndex));
                btn.onClick.AddListener(() => CloseMap());
            }
        }
    }

    // 버튼에서 호출되는 진입점
    public void OnRoundCandidateSelected(int roundIndex, int candidateIndex)
    {
        SelectRound(roundIndex, candidateIndex);

        // 선택된 라운드 불러오기
        var chosen = selectedRounds.Count > 0 ? selectedRounds[selectedRounds.Count - 1] : null;
        if (chosen != null && chosen.roundType == RoundType.Battle)
        {
            OpenBattleMap(chosen, roundIndex, candidateIndex);
        }
        else
        {
            // 기타 타입은 나중 처리
            Debug.Log("Selected non-battle round: " + chosen);
        }
    }

    // 라운드 선택 (기존)
    public void SelectRound(int roundIndex, int candidateIndex)
    {
        if (roundIndex < 0 || roundIndex >= roundCandidatesList.Count) return;
        var candidates = roundCandidatesList[roundIndex];
        if (candidateIndex < 0 || candidateIndex >= candidates.Count) return;
        RoundCandidate selected = candidates[candidateIndex];
        selectedRounds.Add(selected);
        Debug.Log($"선택된 라운드: {selected}");
    }

    private void CloseMap()
    {
        map.SetActive(false);
    }

    // 새로 추가: 전투 맵 생성 및 Enemy JSON 로드
    private void OpenBattleMap(RoundCandidate rc, int roundIndex, int candidateIndex)
    {
        // 기존 그리드/loader 준비
        PlacementGridManager targetGrid = defaultGrid != null ? defaultGrid : FindObjectOfType<PlacementGridManager>();
        if (targetGrid == null)
        {
            Debug.LogError("OpenBattleMap: PlacementGridManager가 씬에 없습니다. defaultGrid를 할당하세요.");
            return;
        }

        EnemyUnitLoader loader = targetGrid.GetComponentInChildren<EnemyUnitLoader>();
        if (loader == null)
            loader = targetGrid.gameObject.AddComponent<EnemyUnitLoader>();

        // zone 계산
        int zoneIndex = zoneSize > 0 ? Mathf.Clamp(roundIndex / zoneSize, 0, Mathf.Max(0, zonePools.Count - 1)) : 0;
        TextAsset chosenTA = null;

        // zonePools에서 랜덤 선택 시도
        if (zonePools != null && zonePools.Count > zoneIndex && zonePools[zoneIndex] != null)
        {
            var pool = zonePools[zoneIndex].enemyJsons;
            if (pool != null && pool.Count > 0)
            {
                int pick = UnityEngine.Random.Range(0, pool.Count);
                chosenTA = pool[pick];
                Debug.Log($"OpenBattleMap: zone {zoneIndex} pool selected: {zonePools[zoneIndex].poolName} index {pick}");
            }
        }

        // 풀에서 못 고르면 기존 포맷(resPath)으로 시도
        if (chosenTA == null)
        {
            string resPath = string.Format(enemyJsonResourcePathFormat, roundIndex + 1, candidateIndex + 1);
            chosenTA = Resources.Load<TextAsset>(resPath);
            if (chosenTA == null && !string.IsNullOrEmpty(rc.subType))
            {
                chosenTA = Resources.Load<TextAsset>($"Enemies/{rc.subType}");
            }

            if (chosenTA != null)
                Debug.Log($"OpenBattleMap: fallback JSON loaded: {resPath}");
        }

        if (chosenTA == null)
        {
            Debug.LogError($"OpenBattleMap: Enemy JSON을 찾을 수 없습니다. zoneIndex={zoneIndex}");
            return;
        }

        // 로더에 그리드 전달하여 적 소환
        loader.LoadFromTextAsset(chosenTA, targetGrid);
        Debug.Log($"OpenBattleMap: existing grid에서 적 로드 완료 (zone {zoneIndex})");
    }
}