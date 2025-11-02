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
    [Tooltip("에디터에서 이벤트 프리팹들을 할당하세요 (StageEventCandidate가 붙은 GameObject)")]
    [SerializeField] private List<GameObject> eventCandidatePrefabs = new List<GameObject>();

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
        // 시작 시 모든 라운드 후보 생성
        GenerateAllRoundCandidates();
        map.SetActive(false); // 기본적으로 맵 UI 숨김
        
        Debug.Log("라운드 시스템 초기화 완료. 맵 버튼을 눌러서 첫 라운드를 시작하세요.");
        // ShowRoundCandidates() 제거 - 사용자가 수동으로 맵을 열어야 함
    }

    private void Awake()
    {
        LoadActivatedEvents();
        // 초기 activeByDefault 적용
        foreach (var eventPrefab in eventCandidatePrefabs)
        {
            if (eventPrefab != null)
            {
                var eventCandidate = eventPrefab.GetComponent<StageEventCandidate>();
                if (eventCandidate != null && eventCandidate.activeByDefault)
                    activatedEventIds.Add(eventCandidate.id);
            }
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

        // 선택된 라운드를 가져와 타입에 따라 처리
        var chosen = selectedRounds.Count > 0 ? selectedRounds[selectedRounds.Count - 1] : null;
        if (chosen != null)
        {
            switch (chosen.roundType)
            {
                case RoundType.Battle:
                    OpenBattleMap(chosen, roundIndex, candidateIndex);
                    break;
                    
                case RoundType.Event:
                    OpenEventRound();
                    break;
                    
                case RoundType.Shop:
                    OpenShopRound();
                    break;
                    
                case RoundType.Rest:
                    OpenRestRound();
                    break;
                    
                default:
                    Debug.Log("Selected round: " + chosen);
                    break;
            }
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

    /// <summary>
    /// 맵을 수동으로 여는 public 메서드 (사용자가 버튼을 눌러서 호출)
    /// </summary>
    public void OpenMap()
    {
        if (currentRound >= 0 && currentRound < roundCandidatesList.Count)
        {
            Debug.Log($"맵 열기: {currentRound + 1}라운드 선택지 표시");
            ShowRoundCandidates();
        }
        else
        {
            Debug.LogWarning("표시할 라운드가 없습니다.");
        }
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

    #region 이벤트 라운드
    /*
     * 이벤트 시스템 사용법 (프리팹 기반):
     * 
     * === 이벤트 프리팹 설정 ===
     * 1. GameObject를 생성하고 StageEventCandidate 컴포넌트 추가
     * 2. 각종 이벤트 설정 (id, displayName, choices 등) 구성
     * 3. 프리팹으로 저장 후 eventCandidatePrefabs 리스트에 추가
     * 
     * === 실행 방식 ===
     * - OpenEventRound(): 랜덤 이벤트 자동 선택 및 실행
     * - TriggerEventById(string id): 특정 ID 이벤트 직접 실행
     * - TriggerEventFromPrefab(GameObject prefab): 프리팹으로 직접 실행
     * 
     * === 이벤트 계층 구조 ===
     * EventCanvas (캔버스)
     *  └── EventInstance (인스턴스화된 이벤트 프리팹 - 후보자 역할)
     *       └── EventUI (이벤트 UI 프리팹)
     *            └── ChoiceButtons (선택지 버튼들)
     * 
     * === 이벤트 생명주기 ===
     * 1. 프리팹 → Canvas 하위에 인스턴스화 (후보자 생성)
     * 2. 후보자 → UI 생성 (선택지들 포함)
     * 3. 플레이어 선택 → 결과 적용
     * 4. OnEventCompleted 호출 → 모든 인스턴스 정리 → 다음 라운드
     * 
     * === 디버깅 기능 ===
     * - GetCurrentEventStatus(): 현재 이벤트 상태 확인
     * - ForceEndCurrentEvent(): 강제 이벤트 종료
     */
    
    [Header("이벤트 UI")]
    [SerializeField] private Canvas eventCanvas; // 이벤트 UI가 생성될 캔버스
    [SerializeField] private GameObject choiceButtonPrefab; // 선택지 버튼 프리팹
    
    /// <summary>
    /// 선택지 버튼 프리팹에 대한 public 접근자
    /// </summary>
    public GameObject ChoiceButtonPrefab => choiceButtonPrefab;
    
    private GameObject currentEventUI; // 현재 생성된 이벤트 UI
    private GameObject currentEventInstance; // 현재 인스턴스화된 이벤트 후보자 (부모 역할)
    private StageEventCandidate currentEventCandidate; // 현재 활성 이벤트 후보자 컴포넌트
    
    /// <summary>
    /// 이벤트 라운드 시작 - 직접 랜덤 이벤트 선택 및 실행
    /// </summary>
    private void OpenEventRound()
    {
        Debug.Log("이벤트 라운드 시작");
        
        // 사용 가능한 이벤트 목록 가져오기
        var availableEvents = GetAvailableEventCandidates();
        
        if (availableEvents.Count == 0)
        {
            Debug.LogWarning("사용 가능한 이벤트가 없습니다. 다음 라운드로 진행합니다.");
            AdvanceToNextRound();
            return;
        }
        
        // 랜덤으로 이벤트 선택
        int randomIndex = UnityEngine.Random.Range(0, availableEvents.Count);
        StageEventCandidate selectedEvent = availableEvents[randomIndex];
        
        Debug.Log($"선택된 이벤트: {selectedEvent.displayName}");
        
        // 이벤트 프리팹을 인스턴스화하여 시작
        TriggerEventFromCandidate(selectedEvent);
    }
    
    /// <summary>
    /// 이벤트 후보자(프리팹에서 가져온 컴포넌트)로부터 이벤트 실행
    /// </summary>
    public void TriggerEventFromCandidate(StageEventCandidate eventCandidate)
    {
        if (eventCandidate == null)
        {
            Debug.LogError("이벤트 후보가 null입니다.");
            return;
        }

        // 해당 후보의 프리팹을 찾아서 인스턴스화
        GameObject eventPrefab = FindEventPrefabByCandidate(eventCandidate);
        if (eventPrefab == null)
        {
            Debug.LogError($"이벤트 후보 {eventCandidate.displayName}에 해당하는 프리팹을 찾을 수 없습니다.");
            return;
        }

        // 프리팹을 인스턴스화하여 이벤트 실행
        TriggerEventFromPrefab(eventPrefab);
    }

    /// <summary>
    /// 이벤트 프리팹으로부터 직접 이벤트 실행
    /// </summary>
    public void TriggerEventFromPrefab(GameObject eventPrefab)
    {
        if (eventPrefab == null)
        {
            Debug.LogError("이벤트 프리팹이 null입니다.");
            return;
        }

        if (eventCanvas == null)
        {
            Debug.LogError("EventCanvas가 설정되지 않았습니다.");
            return;
        }

        if (choiceButtonPrefab == null)
        {
            Debug.LogError("ChoiceButtonPrefab이 설정되지 않았습니다.");
            return;
        }

        // 기존 이벤트 관련 오브젝트들 정리
        CleanupCurrentEvent();

        // 이벤트 프리팹을 캔버스에 인스턴스화 (프리팹에 이미 UI가 포함되어 있음)
        currentEventInstance = Instantiate(eventPrefab, eventCanvas.transform);
        
        // 인스턴스화된 객체에서 StageEventCandidate 컴포넌트 가져오기
        currentEventCandidate = currentEventInstance.GetComponent<StageEventCandidate>();
        
        if (currentEventCandidate == null)
        {
            Debug.LogError($"이벤트 프리팹 {eventPrefab.name}에 StageEventCandidate 컴포넌트가 없습니다.");
            CleanupCurrentEvent();
            return;
        }

        Debug.Log($"이벤트 프리팹 인스턴스 생성 완료: {eventPrefab.name}");
        Debug.Log($"구조: {eventCanvas.name} > {currentEventInstance.name} (프리팹에 UI 포함)");
        
        // 프리팹에 UI가 포함되어 있으므로 SerializedField로 할당된 선택지 컨테이너 사용
        if (choiceButtonPrefab != null)
        {
            Debug.Log("선택지 버튼 생성 시작...");
            
            // SerializedField로 할당된 선택지 컨테이너 확인
            if (currentEventCandidate.ChoiceContainer != null)
            {
                Debug.Log($"SerializedField 선택지 컨테이너 사용: {currentEventCandidate.ChoiceContainer.name}");
                currentEventCandidate.GenerateChoiceUI(choiceButtonPrefab, currentEventCandidate.ChoiceContainer);
                Debug.Log("선택지 버튼 생성 완료");
            }
            else
            {
                Debug.LogError("선택지 컨테이너가 할당되지 않았습니다. StageEventCandidate의 choiceContainer를 Inspector에서 할당하세요.");
            }
        }
        else
        {
            Debug.LogWarning("ChoiceButtonPrefab이 null입니다. 선택지 버튼을 생성할 수 없습니다.");
        }
        
        // UI 참조는 인스턴스 자체로 설정
        currentEventUI = currentEventInstance;
    }

    /// <summary>
    /// 특정 이벤트 후보에 해당하는 프리팹 찾기
    /// </summary>
    private GameObject FindEventPrefabByCandidate(StageEventCandidate targetCandidate)
    {
        foreach (var eventPrefab in eventCandidatePrefabs)
        {
            if (eventPrefab != null)
            {
                var candidate = eventPrefab.GetComponent<StageEventCandidate>();
                if (candidate != null && candidate.id == targetCandidate.id)
                {
                    return eventPrefab;
                }
            }
        }
        return null;
    }

    /// <summary>
    /// 특정 이벤트 실행 (기존 호환성용 - 직접 컴포넌트 사용)
    /// </summary>
    public void TriggerEvent(StageEventCandidate eventCandidate)
    {
        TriggerEventFromCandidate(eventCandidate);
    }
    
    /// <summary>
    /// 이벤트 완료 시 호출 (StageEventCandidate에서 호출)
    /// </summary>
    public void OnEventCompleted(StageEventCandidate completedEvent)
    {
        Debug.Log($"이벤트 완료: {completedEvent.displayName}");
        
        // 이벤트 활성화 처리
        ActivateEvent(completedEvent);
        
        // 현재 이벤트 관련 모든 오브젝트들 정리
        CleanupCurrentEvent();
        
        // 다음 라운드로 진행
        AdvanceToNextRound();
    }

    /// <summary>
    /// 현재 활성 이벤트와 관련된 모든 오브젝트들을 정리
    /// </summary>
    private void CleanupCurrentEvent()
    {
        // 이벤트 인스턴스 정리 (프리팹에 포함된 UI도 함께 정리됨)
        if (currentEventInstance != null)
        {
            Debug.Log($"이벤트 인스턴스 정리 중: {currentEventInstance.name}");
            Destroy(currentEventInstance);
            currentEventInstance = null;
        }
        
        // 이벤트 후보자 컴포넌트 참조 정리
        currentEventCandidate = null;
        
        // UI 참조도 정리 (별도 생성하지 않으므로 null로 설정)
        currentEventUI = null;
        
        Debug.Log("현재 이벤트 정리 완료");
    }
    #endregion

    #region 상점 라운드
    /// <summary>
    /// 상점 라운드 시작 - 블랙 마켓 열기
    /// </summary>
    private void OpenShopRound()
    {
        Debug.Log("상점 라운드 시작 - 블랙 마켓 열기");
        
        // 블랙 마켓 매니저 찾기 및 상점 열기
        BlackMarketManager blackMarketManager = FindObjectOfType<BlackMarketManager>();
        if (blackMarketManager != null)
        {
            Debug.Log("블랙 마켓 매니저 발견, 상점 열기");
            blackMarketManager.InitializeMarket();
            blackMarketManager.OpenShop();
        }
        else
        {
            Debug.LogError("BlackMarketManager를 찾을 수 없습니다! 씬에 BlackMarketManager가 있는지 확인하세요.");
        }
    }
    #endregion

    #region 휴식 라운드
    /// <summary>
    /// 휴식 라운드 시작
    /// </summary>
    private void OpenRestRound()
    {
        Debug.Log("휴식 라운드 시작");
        
        // 휴식 효과 적용 (체력 회복 등)
        // PlayerStats.instance.RestoreHealth(50);
        // PlayerStats.instance.RestoreMana(30);
        
        Debug.Log("휴식을 취했습니다. 체력과 마나가 회복되었습니다.");
        Debug.Log("맵 버튼을 눌러서 다음 라운드로 진행하세요.");
        
        // 자동 진행 제거 - 사용자가 수동으로 맵을 열어야 함
        AdvanceToNextRound();
    }
    
    /// <summary>
    /// 다음 라운드로 진행
    /// </summary>
    private void AdvanceToNextRound()
    {
        currentRound++;
        if (currentRound < maxRounds)
        {
            Debug.Log($"다음 라운드로 진행: {currentRound + 1}라운드");
            Debug.Log("맵 버튼을 눌러서 다음 라운드 선택지를 확인하세요.");
            // ShowRoundCandidates() 호출 제거 - 사용자가 수동으로 맵을 열어야 함
        }
        else
        {
            Debug.Log("모든 라운드 완료!");
            // 스테이지 완료 처리
        }
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
        // foreach (var id in ev.unlockEventIds)
        // {
        //     // 단순히 활성화 표시(원하면 별도 논리로 변경)
        //     activatedEventIds.Add(id);
        // }
        SaveActivatedEvents();
    }

    // 현재 할당된 eventCandidatePrefabs 중 사용 가능한 것 반환
    public List<StageEventCandidate> GetAvailableEventCandidates()
    {
        var list = new List<StageEventCandidate>();
        foreach (var eventPrefab in eventCandidatePrefabs)
        {
            if (eventPrefab != null)
            {
                var eventCandidate = eventPrefab.GetComponent<StageEventCandidate>();
                if (eventCandidate != null && IsEventAvailable(eventCandidate))
                {
                    list.Add(eventCandidate);
                }
            }
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

    /// <summary>
    /// 특정 ID의 이벤트 프리팹으로 직접 이벤트 실행
    /// </summary>
    /// <param name="eventId">실행할 이벤트 ID</param>
    public void TriggerEventById(string eventId)
    {
        GameObject eventPrefab = FindEventPrefabById(eventId);
        if (eventPrefab != null)
        {
            TriggerEventFromPrefab(eventPrefab);
        }
        else
        {
            Debug.LogError($"ID '{eventId}'에 해당하는 이벤트 프리팹을 찾을 수 없습니다.");
        }
    }

    /// <summary>
    /// 특정 ID의 이벤트 프리팹 찾기
    /// </summary>
    /// <param name="eventId">찾을 이벤트 ID</param>
    /// <returns>해당하는 프리팹, 없으면 null</returns>
    private GameObject FindEventPrefabById(string eventId)
    {
        foreach (var eventPrefab in eventCandidatePrefabs)
        {
            if (eventPrefab != null)
            {
                var candidate = eventPrefab.GetComponent<StageEventCandidate>();
                if (candidate != null && candidate.id == eventId)
                {
                    return eventPrefab;
                }
            }
        }
        return null;
    }

    /// <summary>
    /// 이벤트 프리팹 리스트에서 사용 가능한 이벤트 개수 반환
    /// </summary>
    /// <returns>사용 가능한 이벤트 개수</returns>
    public int GetAvailableEventCount()
    {
        int count = 0;
        foreach (var eventPrefab in eventCandidatePrefabs)
        {
            if (eventPrefab != null)
            {
                var candidate = eventPrefab.GetComponent<StageEventCandidate>();
                if (candidate != null && IsEventAvailable(candidate))
                {
                    count++;
                }
            }
        }
        return count;
    }

    /// <summary>
    /// 현재 활성 이벤트 상태 반환 (디버깅용)
    /// </summary>
    /// <returns>현재 이벤트 상태 정보</returns>
    public string GetCurrentEventStatus()
    {
        if (currentEventCandidate == null && currentEventInstance == null)
        {
            return "현재 활성 이벤트 없음";
        }

        string status = "=== 현재 이벤트 상태 ===\n";
        
        if (currentEventCandidate != null)
            status += $"이벤트 후보자: {currentEventCandidate.displayName} (ID: {currentEventCandidate.id})\n";
        
        if (currentEventInstance != null)
            status += $"이벤트 인스턴스: {currentEventInstance.name} (프리팹에 UI 포함)\n";
            
        status += $"구조: EventCanvas > EventInstance (프리팹 UI 포함)\n";
        
        return status;
    }

    /// <summary>
    /// 강제로 현재 이벤트 종료 (디버깅/테스트용)
    /// </summary>
    public void ForceEndCurrentEvent()
    {
        if (currentEventCandidate != null)
        {
            Debug.Log($"강제 이벤트 종료: {currentEventCandidate.displayName}");
            OnEventCompleted(currentEventCandidate);
        }
        else
        {
            Debug.Log("종료할 활성 이벤트가 없습니다.");
            CleanupCurrentEvent();
        }
    }



    [Serializable]
    private class StringListWrapper
    {
        public List<string> items = new List<string>();
    }
}