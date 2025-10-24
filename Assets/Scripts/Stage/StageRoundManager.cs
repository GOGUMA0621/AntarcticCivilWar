using System;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using UnityEngine;
using UnityEngine.UI;

// 라운드 종류를 정의하는 열거형
[Serializable]
public enum RoundType
{
    Battle, // 전투 라운드
    Shop,   // 상점 라운드
    Event,  // 이벤트 라운드
    Rest    // 휴식 라운드
}

// 전투/이벤트의 세부 타입을 위한 열거형 예시
public enum BattleRoundType { Normal, Elite }
public enum EventRoundType { All, Special }

// 라운드 후보 객체
// - 라운드 타입과 서브타입 문자열로 구성
// - Dictionary 키로 쓰일 수 있도록 값 기반 비교 구현(IEquatable)
[Serializable]
public class RoundCandidate : IEquatable<RoundCandidate>
{
    public RoundType roundType; // 라운드 대분류
    public string subType;      // 추가 식별용 서브타입(예: "Normal", "Elite" 등)

    // 생성자: 간단한 초기화
    public RoundCandidate(RoundType type, string subType = "")
    {
        roundType = type;
        this.subType = subType ?? "";
    }

    // 디버그/로깅용 문자열 표현
    public override string ToString()
    {
        return string.IsNullOrEmpty(subType) ? roundType.ToString() : $"{roundType} ({subType})";
    }

    // 값 기반 비교: 같은 roundType과 subType이면 동일 후보로 본다
    public bool Equals(RoundCandidate other)
    {
        if (ReferenceEquals(this, other)) return true;
        if (other is null) return false;
        return this.roundType == other.roundType && string.Equals(this.subType, other.subType);
    }

    public override bool Equals(object obj) => Equals(obj as RoundCandidate);

    // Dictionary/HashSet에서 사용될 해시코드 구현
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

// 에디터에서 지정할 수 있는 적 풀 구조체
[Serializable]
public class EnemyPool
{
    public string poolName;                // 풀 이름(디버그/구분용)
    public List<TextAsset> enemyJsons = new List<TextAsset>(); // 적 JSON 목록
}

// 스테이지 및 라운드 후보를 관리하는 매니저
public partial class StageRoundManager : MonoBehaviour
{
    [Header("라운드")]
    public int currentRound = 0;    // 현재 선택/표시 중인 라운드 인덱스
    public int maxRounds = 8;      // 총 라운드 수 (마지막은 휴식 등으로 사용 가능)
    public int candidateCount = 2; // 각 라운드에서 보여줄 후보 수(옵션)
    public int restAvailableAfterRound = 3; // 이 라운드 이후부터 휴식 후보 허용

    [Header("스테이지 지도")]
    [SerializeField] GameObject map;                    // 맵 UI 오브젝트 (토글용)
    [SerializeField] Transform roundContainer;          // 라운드 후보 UI들이 생성될 부모
    [SerializeField] GameObject roundCandidatePrefab;   // 후보 UI 프리팹 (Image+Button으로 가정)
    public SerializedDictionary<RoundCandidate, Sprite> roundIcons; // 후보->아이콘 매핑 (SerializedDictionary 사용)

    [Header("전투")]
    [SerializeField] private PlacementGridManager defaultGrid; // 전투용 그리드(씬에 할당 가능)
    [Tooltip("Resources 경로 포맷. 예: Enemies/Round_{0}_{1} -> Resources/Enemies/Round_1_1.json")]
    public string enemyJsonResourcePathFormat = "Enemies/Round_{0}_{1}";

    [Header("구역 풀 (적 소환을 랜덤화))")]
    [Tooltip("한 구역에 몇 라운드(예: 5)인지")]
    [SerializeField] private int zoneSize = 5;

    [Tooltip("구역별로 사용할 Enemy JSON 풀을 할당하세요. 총 라운드가 15면 zoneSize=5 로 zonePools 3개를 만드세요.")]
    [SerializeField] private List<EnemyPool> zonePools = new List<EnemyPool>();

    [Header("이벤트 라운드")]
    [Tooltip("에디터에서 이벤트 ScriptableObject들을 할당하세요 (StageEventCandidate)")]
    [SerializeField] private List<StageEventCandidate> eventCandidates = new List<StageEventCandidate>();

    // 전체 라운드 후보 리스트: 각 라운드(인덱스)마다 후보 리스트를 보관
    public List<List<RoundCandidate>> roundCandidatesList = new List<List<RoundCandidate>>();

    [Header("선택 라운드")]
    [Tooltip("플레이어가 선택한 라운드들이 여기에 추가됩니다.")]
    public List<RoundCandidate> selectedRounds = new List<RoundCandidate>();

    // 활성화된(발생한) 이벤트 ID 집합 (런타임)
    private HashSet<string> activatedEventIds = new HashSet<string>();

    // PlayerPrefs 키 (간단 저장)
    private const string PREFS_EVENT_KEY = "ActivatedEvents_v1";

    void Start()
    {
        // 시작 시 모든 라운드 후보 생성 후 첫 화면 세팅
        GenerateAllRoundCandidates();
        map.SetActive(false); // 기본적으로 맵 UI 숨김
        ShowRoundCandidates();
    }

    private void Awake()
    {
        LoadActivatedEvents();
        // 초기 activeByDefault 적용
        foreach (var ev in eventCandidates)
        {
            if (ev != null && ev.activeByDefault)
                activatedEventIds.Add(ev.id);
        }
    }

    // 전체 라운드 후보 생성 로직
    public void GenerateAllRoundCandidates()
    {
        roundCandidatesList.Clear();
        bool restIncluded = false; // 휴식 루틴이 이미 포함되었는지 추적

        for (int i = 0; i < maxRounds; i++)
        {
            List<RoundCandidate> candidates = new List<RoundCandidate>();

            // 마지막 라운드는 강제로 휴식으로 설정
            if (i == maxRounds - 1)
            {
                candidates.Add(new RoundCandidate(RoundType.Rest));
                roundCandidatesList.Add(candidates);
                continue;
            }

            // 전체 후보 풀을 구성 (예시 항목들)
            List<RoundCandidate> allPool = new List<RoundCandidate>
            {
                new RoundCandidate(RoundType.Battle, BattleRoundType.Normal.ToString()),
                new RoundCandidate(RoundType.Battle, BattleRoundType.Elite.ToString()),
                new RoundCandidate(RoundType.Shop),
                new RoundCandidate(RoundType.Event, EventRoundType.All.ToString()),
                new RoundCandidate(RoundType.Event, EventRoundType.Special.ToString())
            };

            // 풀에서 무작위로 2~3개 선택
            int poolCount = UnityEngine.Random.Range(2, 4); // 2 또는 3
            Debug.Log($"라운드 {i + 1} 후보 생성, 풀에서 선택할 개수: {poolCount}");

            List<RoundCandidate> pool = new List<RoundCandidate>();
            List<RoundCandidate> tempPool = new List<RoundCandidate>(allPool);
            for (int j = 0; j < poolCount && tempPool.Count > 0; j++)
            {
                int idx = UnityEngine.Random.Range(0, tempPool.Count);
                pool.Add(tempPool[idx]);
                tempPool.RemoveAt(idx);
            }

            // 휴식 후보는 restAvailableAfterRound 이후에만 등장 가능
            bool canAddRest = i >= restAvailableAfterRound;

            // 특정 조건에서 휴식 후보를 강제로 또는 확률적으로 추가
            if (canAddRest && !restIncluded && i == maxRounds - 2)
            {
                candidates.Add(new RoundCandidate(RoundType.Rest));
                restIncluded = true;
            }
            else if (canAddRest && !restIncluded && UnityEngine.Random.value < 0.2f) // 20% 확률
            {
                candidates.Add(new RoundCandidate(RoundType.Rest));
                restIncluded = true;
            }

            // 선택된 풀 항목들을 후보 리스트에 추가
            foreach (var round in pool)
            {
                candidates.Add(round);
            }

            roundCandidatesList.Add(candidates);
        }
    }

    // 현재 라운드의 후보들을 UI로 보여주는 메서드
    public void ShowRoundCandidates()
    {
        if (currentRound < 0 || currentRound >= roundCandidatesList.Count) return;
        map.SetActive(true);

        // 기존 UI 정리: 컨테이너의 자식 오브젝트 삭제
        if (roundContainer != null)
        {
            foreach (Transform t in roundContainer) Destroy(t.gameObject);
        }

        var candidates = roundCandidatesList[currentRound];
        for (int i = 0; i < candidates.Count; i++)
        {
            var c = candidates[i];
            var candidateUI = Instantiate(roundCandidatePrefab, roundContainer);

            // 후보 프리팹에서 Image 컴포넌트 찾기 (직접 또는 하위)
            var img = candidateUI.GetComponent<Image>() ?? candidateUI.GetComponentInChildren<Image>();
            // roundIcons 딕셔너리에 아이콘이 있으면 설정
            if (img != null && roundIcons != null && roundIcons.ContainsKey(c))
                img.sprite = roundIcons[c];

            // 디버그 로그: 아이콘 존재 여부 출력
            Debug.Log($"라운드 {currentRound + 1} 후보 {i + 1}: {c}" + (roundIcons.ContainsKey(c) ? " (아이콘 있음)" : " (아이콘 없음)"));

            // 버튼 콜백 연결: 후보 선택 시 지도 닫고 OnRoundCandidateSelected 호출
            var btn = candidateUI.GetComponent<Button>() ?? candidateUI.GetComponentInChildren<Button>();
            int candidateIndex = i; // closure 문제 방지용 로컬 복사
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => OnRoundCandidateSelected(currentRound, candidateIndex));
                btn.onClick.AddListener(() => CloseMap());
            }
        }
    }

    // UI에서 후보를 클릭했을 때 호출되는 진입점
    public void OnRoundCandidateSelected(int roundIndex, int candidateIndex)
    {
        SelectRound(roundIndex, candidateIndex);

        // 선택된 라운드를 가져와 타입에 따라 처리 (전투이면 맵 열기)
        var chosen = selectedRounds.Count > 0 ? selectedRounds[selectedRounds.Count - 1] : null;
        if (chosen != null && chosen.roundType == RoundType.Battle)
        {
            OpenBattleMap(chosen, roundIndex, candidateIndex);
        }
        else
        {
            // 그 외 타입은 추후 처리
            Debug.Log("Selected non-battle round: " + chosen);
        }
    }

    // 실제 라운드를 선택하고 selectedRounds에 추가
    public void SelectRound(int roundIndex, int candidateIndex)
    {
        if (roundIndex < 0 || roundIndex >= roundCandidatesList.Count) return;
        var candidates = roundCandidatesList[roundIndex];
        if (candidateIndex < 0 || candidateIndex >= candidates.Count) return;
        RoundCandidate selected = candidates[candidateIndex];
        selectedRounds.Add(selected);
        Debug.Log($"선택된 라운드: {selected}");
    }

    // 맵 UI를 닫는 간단한 헬퍼
    private void CloseMap()
    {
        map.SetActive(false);
    }
    #region  전투 라운드
    // 전투 맵을 열고 적을 로드하는 로직
    private void OpenBattleMap(RoundCandidate rc, int roundIndex, int candidateIndex)
    {
        // 전투용 그리드 취득: 인스펙터에 할당된 defaultGrid 우선, 없으면 씬에서 검색
        PlacementGridManager targetGrid = defaultGrid != null ? defaultGrid : FindObjectOfType<PlacementGridManager>();
        if (targetGrid == null)
        {
            Debug.LogError("OpenBattleMap: PlacementGridManager가 씬에 없습니다. defaultGrid를 할당하세요.");
            return;
        }

        // EnemyUnitLoader 얻기 (없으면 추가)
        EnemyUnitLoader loader = targetGrid.GetComponentInChildren<EnemyUnitLoader>();
        if (loader == null)
            loader = targetGrid.gameObject.AddComponent<EnemyUnitLoader>();

        // zone 계산: zoneSize를 이용해 어떤 풀을 쓸지 결정
        int zoneIndex = zoneSize > 0 ? Mathf.Clamp(roundIndex / zoneSize, 0, Mathf.Max(0, zonePools.Count - 1)) : 0;
        TextAsset chosenTA = null;

        // zonePools에 등록된 풀 중 하나를 랜덤으로 선택 (존재하면)
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

        // zone 풀에서 선택되지 않았다면 리소스 포맷에 따라 JSON을 로드 시도
        if (chosenTA == null)
        {
            string resPath = string.Format(enemyJsonResourcePathFormat, roundIndex + 1, candidateIndex + 1);
            chosenTA = Resources.Load<TextAsset>(resPath);
            if (chosenTA == null && !string.IsNullOrEmpty(rc.subType))
            {
                // subType 기반 fallback 로드 시도
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

        // 최종적으로 로더에 JSON과 그리드를 전달해 적을 생성
        loader.LoadFromTextAsset(chosenTA, targetGrid);
        Debug.Log($"OpenBattleMap: existing grid에서 적 로드 완료 (zone {zoneIndex})");
    }
    #endregion

    // 이벤트 사용 가능 여부 검사
    public bool IsEventAvailable(StageEventCandidate ev)
    {
        if (ev == null) return false;
        // 이미 활성화되어있고 반복 불가면 불가
        if (activatedEventIds.Contains(ev.id) && !ev.isRepeatable) return false;
        // 라운드 조건
        if (currentRound + 1 < ev.minRound) return false; // currentRound가 0-based 라면 +1 사용 or 조정
        // 선행 이벤트 조건
        foreach (var req in ev.requiredEventIds)
        {
            if (!activatedEventIds.Contains(req)) return false;
        }
        return true;
    }

    // 이벤트 활성화 — 활성화 저장, unlock 처리(옵션)
    public void ActivateEvent(StageEventCandidate ev)
    {
        if (ev == null) return;
        activatedEventIds.Add(ev.id);
        // unlock 처리: unlockEventIds에 있는 ID들을 활성화 표시하거나 다른 로직을 수행
        foreach (var id in ev.unlockEventIds)
        {
            // 단순히 활성화 표시(원하면 별도 논리로 변경)
            activatedEventIds.Add(id);
        }
        SaveActivatedEvents();
    }

    // 현재 할당된 eventCandidates 중 사용 가능한 것 반환
    public List<StageEventCandidate> GetAvailableEventCandidates()
    {
        var list = new List<StageEventCandidate>();
        foreach (var ev in eventCandidates)
        {
            if (IsEventAvailable(ev)) list.Add(ev);
        }
        return list;
    }

    // 간단한 직렬화/저장 (PlayerPrefs에 JSON 배열 저장)
    private void SaveActivatedEvents()
    {
        try
        {
            var arr = new List<string>(activatedEventIds);
            string json = JsonUtility.ToJson(new StringListWrapper { items = arr });
            PlayerPrefs.SetString(PREFS_EVENT_KEY, json);
            PlayerPrefs.Save();
        }
        catch (Exception ex)
        {
            Debug.LogWarning("SaveActivatedEvents 실패: " + ex.Message);
        }
    }

    private void LoadActivatedEvents()
    {
        activatedEventIds.Clear();
        if (!PlayerPrefs.HasKey(PREFS_EVENT_KEY)) return;
        try
        {
            string json = PlayerPrefs.GetString(PREFS_EVENT_KEY);
            var wrapper = JsonUtility.FromJson<StringListWrapper>(json);
            if (wrapper != null && wrapper.items != null)
            {
                foreach (var s in wrapper.items) activatedEventIds.Add(s);
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning("LoadActivatedEvents 실패: " + ex.Message);
        }
    }

    [Serializable]
    private class StringListWrapper
    {
        public List<string> items = new List<string>();
    }
}