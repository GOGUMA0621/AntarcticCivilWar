using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[Serializable]
public enum RewardType
{
    Unit,     // 유닛만
    Item,     // 아이템만
    Both,     // 둘 다 (랜덤으로 선택)
    UnitAndItem  // 유닛과 아이템 모두 지급
}

[Serializable]
public class TierProbability
{
    [Tooltip("티어 (1~5)")]
    [Range(1, 5)] public int tier = 1;
    [Tooltip("이 티어의 선택 확률 (%)")]
    [Range(0f, 100f)] public float probability = 100f;
    [Tooltip("보상 타입: Unit(유닛만), Item(아이템만), Both(둘 중 랜덤), UnitAndItem(둘 다)")]
    public RewardType rewardType = RewardType.Both;
}

[Serializable]
public class RewardResult
{
    public int id;        // 보상 ID (유닛 ID 또는 아이템 ID)
    public bool isUnit;   // true: 유닛, false: 아이템
    public int tier;      // 선택된 티어
}

[Serializable]
public class EventOutcome
{
    [Header("결과 정보")]
    [TextArea] public string resultText; // 결과 텍스트
    [Range(0f, 100f)] public float probability = 100f; // 발생 확률 (%)
    
    [Header("고정 보상")]
    public int goldReward = 0;           // 골드 보상
    public int healthChange = 0;         // 체력 변화 (음수면 데미지)
    public List<GameObject> unitRewards = new List<GameObject>(); // 유닛 보상 프리팹들
    public List<GameObject> itemRewards = new List<GameObject>(); // 아이템 보상 프리팹들
    
    [Header("랜덤 티어 보상")]
    [Tooltip("랜덤 유닛 개수")]
    public int randomUnitCount = 0;      // 랜덤 유닛 보상 개수
    [Tooltip("랜덤 아이템 개수")]
    public int randomItemCount = 0;      // 랜덤 아이템 보상 개수
    
    [Header("티어 선택 방식")]
    [Tooltip("true: 가중치 기반 랜덤, false: 고정 티어 또는 특정 티어들 중 선택")]
    public bool useWeightedRandom = true;
    
    [Header("가중치 기반 랜덤 (useWeightedRandom = true)")]
    [Tooltip("티어별 가중치 [1티어, 2티어, 3티어, 4티어, 5티어] - 높을수록 선택 확률 증가")]
    public float[] tierWeights = new float[5] { 50f, 30f, 15f, 4f, 1f }; // 티어별 가중치
    
    [Header("고정/특정 티어 선택 (useWeightedRandom = false)")]
    [Tooltip("고정 티어 사용 여부 (true: 하나의 고정 티어, false: 여러 티어 중 선택)")]
    public bool useFixedTier = false;
    [Tooltip("고정 티어 값 (1~5)")]
    [Range(1, 5)] public int fixedTier = 1;
    [Tooltip("선택 가능한 티어들과 각각의 확률 설정")]
    public TierProbability[] specificTiers = new TierProbability[0];
    
    [Header("고급 보상 타입 제어")]
    [Tooltip("특정 티어에서 보상 타입까지 제어하는 고급 모드 사용 여부")]
    public bool useAdvancedRewardControl = false;
    
    [Header("추가 효과")]
    public List<string> unlockEventIds = new List<string>(); // 이 결과로 해금되는 이벤트 ID들
    public UnityEvent onOutcomeTriggered;  // 이 결과 발생 시 실행될 이벤트
}

[Serializable]
public class EventChoice
{
    [Header("선택지 정보")]
    public string choiceText;            // 선택지 텍스트 (예: "유물을 가져간다")
    
    [Header("확률 기반 결과")]
    [Tooltip("확률 기반 결과가 없으면 기본 결과만 사용됩니다.")]
    public List<EventOutcome> possibleOutcomes = new List<EventOutcome>(); // 확률 기반 결과들
    
    [Header("기본 고정 결과 (확률 결과가 없을 때 사용)")]
    [TextArea] public string defaultResultText; // 기본 결과 텍스트
    public int defaultGoldReward = 0;
    public int defaultHealthChange = 0;
    public List<GameObject> defaultUnitRewards = new List<GameObject>();
    public List<GameObject> defaultItemRewards = new List<GameObject>();
    public List<string> defaultUnlockEventIds = new List<string>();
    
    [Header("기본 랜덤 티어 결과")]
    [Tooltip("기본 랜덤 유닛 개수")]
    public int defaultRandomUnitCount = 0;      // 기본 랜덤 유닛 보상 개수
    [Tooltip("기본 랜덤 아이템 개수")]
    public int defaultRandomItemCount = 0;      // 기본 랜덤 아이템 보상 개수
    
    [Header("기본 티어 선택 방식")]
    [Tooltip("true: 가중치 기반 랜덤, false: 고정 티어 또는 특정 티어들 중 선택")]
    public bool defaultUseWeightedRandom = true;
    
    [Header("기본 가중치 기반 랜덤 (defaultUseWeightedRandom = true)")]
    [Tooltip("기본 티어별 가중치 [1티어, 2티어, 3티어, 4티어, 5티어] - 높을수록 선택 확률 증가")]
    public float[] defaultTierWeights = new float[5] { 50f, 30f, 15f, 4f, 1f }; // 기본 티어별 가중치
    
    [Header("기본 고정/특정 티어 선택 (defaultUseWeightedRandom = false)")]
    [Tooltip("기본 고정 티어 사용 여부 (true: 하나의 고정 티어, false: 여러 티어 중 선택)")]
    public bool defaultUseFixedTier = false;
    [Tooltip("기본 고정 티어 값 (1~5)")]
    [Range(1, 5)] public int defaultFixedTier = 1;
    [Tooltip("기본 선택 가능한 티어들과 각각의 확률 설정")]
    public TierProbability[] defaultSpecificTiers = new TierProbability[0];
    
    [Header("기본 고급 보상 타입 제어")]
    [Tooltip("기본 특정 티어에서 보상 타입까지 제어하는 고급 모드 사용 여부")]
    public bool defaultUseAdvancedRewardControl = false;
    
    public UnityEvent onChoiceSelected;  // 커스텀 효과를 위한 UnityEvent
}

public class StageEventCandidate : MonoBehaviour
{
    [Header("식별")]
    public string id;                    // 고유 ID (ex: "event_find_relic")
    public string displayName;

    [Header("이벤트 표시")]
    public GameObject eventUIPrefab;     // 이벤트 전용 UI 프리팹
    public Sprite eventImage;            // 이벤트 대표 이미지
    [TextArea(3, 5)] public string eventDescription; // 이벤트 설명 텍스트

    [Header("UI 컴포넌트 직접 지정 (TextMeshPro만 사용)")]
    [SerializeField] private TMPro.TextMeshProUGUI titleText;            // 제목 텍스트 (TextMeshPro)
    [SerializeField] private TMPro.TextMeshProUGUI descriptionText;      // 설명 텍스트 (TextMeshPro)
    [SerializeField] private TMPro.TextMeshProUGUI resultText;           // 결과 텍스트 (선택 후 표시)
    [SerializeField] private UnityEngine.UI.Image eventImageUI;          // 이벤트 이미지 UI
    [SerializeField] private Transform choiceContainer;                  // 선택지 버튼들이 생성될 부모 컨테이너

    /// <summary>
    /// SerializedField로 할당된 선택지 컨테이너에 대한 접근자
    /// </summary>
    public Transform ChoiceContainer => choiceContainer;

    [Header("선택지들")]
    public List<EventChoice> choices = new List<EventChoice>();

    [Header("발생 조건")]
    public int minRound = 0;             // 최소 라운드 번호
    public List<string> requiredEventIds = new List<string>(); // 선행 이벤트 ID들 (모두 만족 시 가능)
    public bool isRepeatable = false;    // 반복 가능 여부

    [Header("디버그")]
    [Tooltip("초기 활성화(디버그용). 런타임 시작 시 적용됨.")]
    public bool activeByDefault = false;

    /// <summary>
    /// 이벤트 시작 시 호출 - 직접 UI 생성 (기존 호환성)
    /// </summary>
    /// <param name="eventCanvas">UI가 생성될 캔버스</param>
    /// <param name="choiceButtonPrefab">선택지 버튼 프리팹</param>
    /// <returns>생성된 이벤트 UI GameObject</returns>
    public virtual GameObject StartEvent(Canvas eventCanvas, GameObject choiceButtonPrefab)
    {
        return StartEventWithParent(eventCanvas.transform, choiceButtonPrefab);
    }

    /// <summary>
    /// 특정 부모 Transform 하위에 이벤트 UI 생성
    /// </summary>
    /// <param name="parentTransform">UI가 생성될 부모 Transform</param>
    /// <param name="choiceButtonPrefab">선택지 버튼 프리팹</param>
    /// <returns>생성된 이벤트 UI GameObject</returns>
    public virtual GameObject StartEventWithParent(Transform parentTransform, GameObject choiceButtonPrefab)
    {
        Debug.Log($"=== 이벤트 시작 (부모 지정) ===");
        Debug.Log($"이벤트명: {displayName} (ID: {id})");
        Debug.Log($"선택지 개수: {choices.Count}");
        Debug.Log($"부모 Transform: {parentTransform.name}");
        
        // 선택지 내용 로그
        for (int i = 0; i < choices.Count; i++)
        {
            Debug.Log($"선택지 {i + 1}: '{choices[i].choiceText}'");
        }
        
        // 이벤트 UI 프리팹 검증
        if (eventUIPrefab == null)
        {
            Debug.LogError($"이벤트 '{displayName}'의 eventUIPrefab이 설정되지 않았습니다.");
            return null;
        }

        if (parentTransform == null)
        {
            Debug.LogError("Parent Transform이 null입니다.");
            return null;
        }

        Debug.Log($"Parent: {parentTransform.name}");
        Debug.Log($"ChoiceButtonPrefab: {choiceButtonPrefab.name}");
        Debug.Log($"EventUIPrefab: {eventUIPrefab.name}");

        // 이벤트 UI를 지정된 부모 하위에 생성
        GameObject eventUI = Instantiate(eventUIPrefab, parentTransform);
        eventUI.name = $"EventUI_{displayName}";
        Debug.Log($"이벤트 UI 인스턴스 생성됨: {eventUI.name} (부모: {parentTransform.name})");
        
        // 선택지 UI 생성
        CreateEventChoiceUI(eventUI, choiceButtonPrefab);
        
        Debug.Log($"=== 이벤트 시작 완료 ===");
        return eventUI;
    }

    /// <summary>
    /// 이벤트 UI에 선택지 생성 및 텍스트 설정
    /// </summary>
    /// <param name="eventUI">생성된 이벤트 UI</param>
    /// <param name="choiceButtonPrefab">선택지 버튼 프리팹</param>
    protected virtual void CreateEventChoiceUI(GameObject eventUI, GameObject choiceButtonPrefab)
    {
        if (eventUI == null || choiceButtonPrefab == null)
        {
            Debug.LogError("EventUI 또는 ChoiceButtonPrefab이 null입니다.");
            return;
        }

        Debug.Log($"이벤트 '{displayName}' UI 생성 시작");
        Debug.Log($"EventUI: {eventUI.name}, ChoiceButtonPrefab: {choiceButtonPrefab.name}");

        // 이벤트 UI의 제목과 설명 설정
        SetupEventUITexts(eventUI);

        // SerializedField로 설정된 선택지 컨테이너 사용
        if (choiceContainer == null)
        {
            Debug.LogError($"선택지 컨테이너가 할당되지 않았습니다. Inspector에서 choiceContainer를 할당하세요.");
            Debug.LogError($"현재 이벤트: {displayName} (ID: {id})");
            return;
        }

        Debug.Log($"선택지 컨테이너 확인됨: {choiceContainer.name}");
        Debug.Log($"선택지 개수: {choices.Count}");

        // 선택지 버튼 생성
        CreateChoiceButtons(choiceButtonPrefab, choiceContainer, OnChoiceSelected);
    }

    /// <summary>
    /// 이벤트 UI의 제목과 설명 텍스트 설정 (SerializedField 기반)
    /// </summary>
    /// <param name="eventUI">이벤트 UI GameObject (사용되지 않음, SerializedField 사용)</param>
    protected virtual void SetupEventUITexts(GameObject eventUI)
    {
        // 제목 설정
        SetEventTitle(displayName);
        
        // 설명 설정
        SetEventDescription(eventDescription);
        
        // 이미지 설정 (있는 경우)
        SetEventImage(eventImage);
    }

    /// <summary>
    /// 이벤트 제목 설정 (TextMeshPro만 사용)
    /// </summary>
    /// <param name="title">설정할 제목</param>
    protected virtual void SetEventTitle(string title)
    {
        if (string.IsNullOrEmpty(title)) return;

        if (titleText != null)
        {
            titleText.text = title;
            Debug.Log($"이벤트 제목 설정 완료: {title}");
        }
        else
        {
            Debug.LogWarning($"제목 텍스트 컴포넌트가 할당되지 않았습니다. Inspector에서 titleText를 할당하세요.");
        }
    }

    /// <summary>
    /// 이벤트 설명 설정 (TextMeshPro만 사용)
    /// </summary>
    /// <param name="description">설정할 설명</param>
    protected virtual void SetEventDescription(string description)
    {
        if (string.IsNullOrEmpty(description)) return;

        if (descriptionText != null)
        {
            descriptionText.text = description;
            Debug.Log($"이벤트 설명 설정 완료: {description.Substring(0, Mathf.Min(50, description.Length))}...");
        }
        else
        {
            Debug.LogWarning($"설명 텍스트 컴포넌트가 할당되지 않았습니다. Inspector에서 descriptionText를 할당하세요.");
        }
        
        // 결과 텍스트는 처음에 숨김 처리
        if (resultText != null)
        {
            resultText.gameObject.SetActive(false);
            Debug.Log("결과 텍스트 초기화: 숨김 처리");
        }
    }

    /// <summary>
    /// 이벤트 이미지 설정 (SerializedField 기반)
    /// </summary>
    /// <param name="sprite">설정할 이미지</param>
    protected virtual void SetEventImage(Sprite sprite)
    {
        if (sprite == null) return;

        if (eventImageUI != null)
        {
            eventImageUI.sprite = sprite;
            Debug.Log($"이벤트 이미지 설정 완료: {sprite.name}");
        }
        else
        {
            Debug.LogWarning($"이미지 UI 컴포넌트가 할당되지 않았습니다. Inspector에서 eventImageUI를 할당하세요.");
        }
    }





    /// <summary>
    /// 선택지 선택 콜백 (결과 표시 후 종료 버튼 생성)
    /// </summary>
    public void OnChoiceSelected(int choiceIndex)
    {
        // 선택지 처리 (보상 적용)
        SelectChoice(choiceIndex);
        
        // 결과 텍스트 표시
        ShowEventResult();
        
        // 기존 선택지 버튼들 제거
        ClearChoiceButtons(choiceContainer);
        
        // "종료" 버튼 생성
        CreateExitButton();
    }
    
    /// <summary>
    /// 이벤트 결과 텍스트 표시
    /// </summary>
    private void ShowEventResult()
    {
        if (resultText != null)
        {
            // 결과 텍스트로 이벤트 설명을 표시
            resultText.text = eventDescription;
            resultText.gameObject.SetActive(true);
        }
    }
    
    /// <summary>
    /// "종료" 버튼 생성
    /// </summary>
    private void CreateExitButton()
    {
        // StageRoundManager에서 choiceButtonPrefab을 사용하여 종료 버튼 생성
        StageRoundManager roundManager = FindObjectOfType<StageRoundManager>();
        if (roundManager != null && choiceContainer != null)
        {
            // 종료용 가짜 선택지 생성
            EventChoice exitChoice = new EventChoice
            {
                choiceText = "종료"
            };
            
            // 현재 choices 리스트를 임시로 백업하고 종료 선택지로 교체
            var originalChoices = choices;
            choices = new System.Collections.Generic.List<EventChoice> { exitChoice };
            
            // 종료 버튼 생성
            CreateChoiceButtons(
                roundManager.ChoiceButtonPrefab, 
                choiceContainer, 
                (exitIndex) => {
                    OnEventExit();
                }
            );
            
            // 원래 choices 복원
            choices = originalChoices;
        }
    }
    
    /// <summary>
    /// 이벤트 종료 처리
    /// </summary>
    private void OnEventExit()
    {
        // StageRoundManager에게 이벤트 완료 알림
        StageRoundManager roundManager = FindObjectOfType<StageRoundManager>();
        if (roundManager != null)
        {
            roundManager.OnEventCompleted(this);
        }
    }

    /// <summary>
    /// 선택지 선택 시 호출
    /// </summary>
    public virtual void SelectChoice(int choiceIndex)
    {
        if (choiceIndex < 0 || choiceIndex >= choices.Count)
        {
            Debug.LogError($"잘못된 선택지 인덱스: {choiceIndex}");
            return;
        }

        EventChoice selectedChoice = choices[choiceIndex];
        Debug.Log($"선택지 선택됨: {selectedChoice.choiceText}");

        // 선택지 결과 적용
        ApplyChoiceResult(selectedChoice);
    }

    /// <summary>
    /// 선택지 결과를 적용 (확률 기반 결과 처리)
    /// </summary>
    protected virtual void ApplyChoiceResult(EventChoice choice)
    {
        EventOutcome selectedOutcome = null;
        
        // 확률 기반 결과가 있는 경우 확률에 따라 결과 선택
        if (choice.possibleOutcomes != null && choice.possibleOutcomes.Count > 0)
        {
            selectedOutcome = SelectRandomOutcome(choice.possibleOutcomes);
        }
        
        // 선택된 결과가 있으면 적용, 없으면 기본 결과 적용
        if (selectedOutcome != null)
        {
            ApplyOutcome(selectedOutcome);
        }
        else
        {
            ApplyDefaultOutcome(choice);
        }

        // 커스텀 효과 실행
        choice.onChoiceSelected?.Invoke();
    }

    /// <summary>
    /// 확률에 따라 랜덤 결과 선택
    /// </summary>
    protected virtual EventOutcome SelectRandomOutcome(List<EventOutcome> outcomes)
    {
        // 확률 정규화 및 가중치 기반 선택
        float totalWeight = 0f;
        foreach (var outcome in outcomes)
        {
            totalWeight += outcome.probability;
        }

        if (totalWeight <= 0f)
        {
            Debug.LogWarning("모든 결과의 확률이 0입니다. 첫 번째 결과를 반환합니다.");
            return outcomes.Count > 0 ? outcomes[0] : null;
        }

        float randomValue = UnityEngine.Random.Range(0f, totalWeight);
        float currentWeight = 0f;

        foreach (var outcome in outcomes)
        {
            currentWeight += outcome.probability;
            if (randomValue <= currentWeight)
            {
                return outcome;
            }
        }

        // 안전장치: 마지막 결과 반환
        return outcomes[outcomes.Count - 1];
    }

    /// <summary>
    /// 특정 결과 적용
    /// </summary>
    protected virtual void ApplyOutcome(EventOutcome outcome)
    {
        Debug.Log($"결과: {outcome.resultText}");

        // 골드 보상
        if (outcome.goldReward != 0)
        {
            // PlayerManager.instance.AddGold(outcome.goldReward);
            Debug.Log($"골드 {outcome.goldReward} 획득");
        }

        // 체력 변화
        if (outcome.healthChange != 0)
        {
            // PlayerManager.instance.ChangeHealth(outcome.healthChange);
            Debug.Log($"체력 {outcome.healthChange} 변화");
        }

        // 고정 유닛 보상
        foreach (var unitPrefab in outcome.unitRewards)
        {
            if (unitPrefab != null)
            {
                // UnitManager.instance.AddUnitToBench(unitPrefab);
                Debug.Log($"고정 유닛 {unitPrefab.name} 획득");
            }
        }

        // 고정 아이템 보상
        foreach (var itemPrefab in outcome.itemRewards)
        {
            if (itemPrefab != null)
            {
                // InventoryManager.instance.AddItem(itemPrefab);
                Debug.Log($"고정 아이템 {itemPrefab.name} 획득");
            }
        }

        // 랜덤 티어 보상 (고급 모드 vs 기본 모드)
        if (outcome.useAdvancedRewardControl && !outcome.useWeightedRandom && !outcome.useFixedTier)
        {
            // 고급 모드: 보상 타입까지 제어
            int totalRewardCount = outcome.randomUnitCount + outcome.randomItemCount;
            if (totalRewardCount > 0)
            {
                var advancedRewards = GetRandomTierRewardsWithType(totalRewardCount, outcome.specificTiers);
                foreach (var reward in advancedRewards)
                {
                    if (reward.isUnit)
                    {
                        Debug.Log($"고급 랜덤 유닛 ID {reward.id} 획득 (티어: {reward.tier})");
                        // TODO: UnitManager.instance.AddUnitToBench(reward.id);
                    }
                    else
                    {
                        Debug.Log($"고급 랜덤 아이템 ID {reward.id} 획득 (티어: {reward.tier})");
                        // TODO: InventoryManager.instance.AddItem(reward.id);
                    }
                }
            }
        }
        else
        {
            // 기본 모드: 유닛과 아이템 별도 처리
            if (outcome.randomUnitCount > 0)
            {
                var randomUnits = GetRandomTierRewards(outcome.randomUnitCount, outcome.useWeightedRandom, 
                    outcome.tierWeights, outcome.useFixedTier, outcome.fixedTier, outcome.specificTiers, true);
                foreach (var unitId in randomUnits)
                {
                    Debug.Log($"랜덤 유닛 ID {unitId} 획득 (티어: {GetUnitTierByID(unitId)})");
                    // TODO: UnitManager.instance.AddUnitToBench(unitId);
                }
            }

            if (outcome.randomItemCount > 0)
            {
                var randomItems = GetRandomTierRewards(outcome.randomItemCount, outcome.useWeightedRandom, 
                    outcome.tierWeights, outcome.useFixedTier, outcome.fixedTier, outcome.specificTiers, false);
                foreach (var itemId in randomItems)
                {
                    Debug.Log($"랜덤 아이템 ID {itemId} 획득 (티어: {GetItemTierByID(itemId)})");
                    // TODO: InventoryManager.instance.AddItem(itemId);
                }
            }
        }

        // 이벤트 ID 해금
        foreach (var eventId in outcome.unlockEventIds)
        {
            // EventManager.instance.UnlockEvent(eventId);
            Debug.Log($"이벤트 {eventId} 해금");
        }

        // 결과별 커스텀 효과 실행
        outcome.onOutcomeTriggered?.Invoke();
    }

    /// <summary>
    /// 기본 결과 적용 (확률 기반 결과가 없을 때)
    /// </summary>
    protected virtual void ApplyDefaultOutcome(EventChoice choice)
    {
        Debug.Log($"결과: {choice.defaultResultText}");

        // 골드 보상
        if (choice.defaultGoldReward != 0)
        {
            // PlayerManager.instance.AddGold(choice.defaultGoldReward);
            Debug.Log($"골드 {choice.defaultGoldReward} 획득");
        }

        // 체력 변화
        if (choice.defaultHealthChange != 0)
        {
            // PlayerManager.instance.ChangeHealth(choice.defaultHealthChange);
            Debug.Log($"체력 {choice.defaultHealthChange} 변화");
        }

        // 고정 유닛 보상
        foreach (var unitPrefab in choice.defaultUnitRewards)
        {
            if (unitPrefab != null)
            {
                // UnitManager.instance.AddUnitToBench(unitPrefab);
                Debug.Log($"고정 유닛 {unitPrefab.name} 획득");
            }
        }

        // 고정 아이템 보상
        foreach (var itemPrefab in choice.defaultItemRewards)
        {
            if (itemPrefab != null)
            {
                // InventoryManager.instance.AddItem(itemPrefab);
                Debug.Log($"고정 아이템 {itemPrefab.name} 획득");
            }
        }

        // 기본 랜덤 티어 보상 (고급 모드 vs 기본 모드)
        if (choice.defaultUseAdvancedRewardControl && !choice.defaultUseWeightedRandom && !choice.defaultUseFixedTier)
        {
            // 고급 모드: 보상 타입까지 제어
            int totalDefaultRewardCount = choice.defaultRandomUnitCount + choice.defaultRandomItemCount;
            if (totalDefaultRewardCount > 0)
            {
                var advancedRewards = GetRandomTierRewardsWithType(totalDefaultRewardCount, choice.defaultSpecificTiers);
                foreach (var reward in advancedRewards)
                {
                    if (reward.isUnit)
                    {
                        Debug.Log($"기본 고급 랜덤 유닛 ID {reward.id} 획득 (티어: {reward.tier})");
                        // TODO: UnitManager.instance.AddUnitToBench(reward.id);
                    }
                    else
                    {
                        Debug.Log($"기본 고급 랜덤 아이템 ID {reward.id} 획득 (티어: {reward.tier})");
                        // TODO: InventoryManager.instance.AddItem(reward.id);
                    }
                }
            }
        }
        else
        {
            // 기본 모드: 유닛과 아이템 별도 처리
            if (choice.defaultRandomUnitCount > 0)
            {
                var randomUnits = GetRandomTierRewards(choice.defaultRandomUnitCount, choice.defaultUseWeightedRandom, 
                    choice.defaultTierWeights, choice.defaultUseFixedTier, choice.defaultFixedTier, choice.defaultSpecificTiers, true);
                foreach (var unitId in randomUnits)
                {
                    Debug.Log($"기본 랜덤 유닛 ID {unitId} 획득 (티어: {GetUnitTierByID(unitId)})");
                    // TODO: UnitManager.instance.AddUnitToBench(unitId);
                }
            }

            if (choice.defaultRandomItemCount > 0)
            {
                var randomItems = GetRandomTierRewards(choice.defaultRandomItemCount, choice.defaultUseWeightedRandom, 
                    choice.defaultTierWeights, choice.defaultUseFixedTier, choice.defaultFixedTier, choice.defaultSpecificTiers, false);
                foreach (var itemId in randomItems)
                {
                    Debug.Log($"기본 랜덤 아이템 ID {itemId} 획득 (티어: {GetItemTierByID(itemId)})");
                    // TODO: InventoryManager.instance.AddItem(itemId);
                }
            }
        }

        // 이벤트 ID 해금
        foreach (var eventId in choice.defaultUnlockEventIds)
        {
            // EventManager.instance.UnlockEvent(eventId);
            Debug.Log($"이벤트 {eventId} 해금");
        }
    }

    /// <summary>
    /// 이벤트 발생 가능 여부 확인
    /// </summary>
    public virtual bool CanTrigger(int currentRound, List<string> completedEventIds)
    {
        // 최소 라운드 조건 확인
        if (currentRound < minRound)
            return false;

        // 선행 이벤트 조건 확인
        foreach (var requiredId in requiredEventIds)
        {
            if (!completedEventIds.Contains(requiredId))
                return false;
        }

        // 반복 불가능 이벤트가 이미 완료되었는지 확인
        if (!isRepeatable && completedEventIds.Contains(id))
            return false;

        return true;
    }

    #region 선택지 UI 생성 메서드들
    
    /*
     * 이벤트 UI 설정 가이드 (TextMeshPro 전용):
     * 
     * === Inspector에서 UI 컴포넌트 직접 할당 (필수) ===
     * 1. titleText: 제목 표시용 TextMeshProUGUI
     * 2. descriptionText: 설명 표시용 TextMeshProUGUI
     * 3. choiceContainer: 선택지 버튼들이 생성될 부모 Transform (필수!)
     * 4. eventImageUI: 이벤트 이미지 표시용 Image (선택사항)
     * 
     * === 선택지 프리팹 설정 ===
     * 1. 기본 방식: 
     *    - Button + TextMeshProUGUI 컴포넌트만 있는 프리팹
     *    - 자동으로 텍스트 설정 및 클릭 이벤트 연결
     * 
     * 2. EventChoiceUI 스크립트 방식 (권장):
     *    - 선택지 프리팹에 EventChoiceUI 스크립트 추가
     *    - 자동으로 TextMeshPro 컴포넌트 찾기 및 설정
     *    - 커스터마이징 가능
     * 
     * === 자동 텍스트 설정 (TextMeshPro만) ===
     * - displayName → titleText (TextMeshProUGUI)에 자동 설정
     * - eventDescription → descriptionText (TextMeshProUGUI)에 자동 설정
     * - eventImage → eventImageUI (Image)에 자동 설정
     * - choices → choiceContainer에 선택지 버튼들 자동 생성
     */

    /// <summary>
    /// 선택지 버튼들을 동적으로 생성
    /// </summary>
    /// <param name="buttonPrefab">버튼 프리팹 (EventChoiceUI 스크립트 포함 권장)</param>
    /// <param name="parentTransform">버튼들이 생성될 부모 Transform</param>
    /// <param name="onChoiceCallback">선택지 선택 시 호출될 콜백 (choiceIndex 파라미터)</param>
    public virtual void CreateChoiceButtons(GameObject buttonPrefab, Transform parentTransform, System.Action<int> onChoiceCallback)
    {
        if (buttonPrefab == null || parentTransform == null)
        {
            Debug.LogError("ButtonPrefab 또는 ParentTransform이 null입니다.");
            return;
        }

        Debug.Log($"선택지 버튼 생성 시작 - 총 {choices.Count}개 선택지");
        Debug.Log($"부모 Transform: {parentTransform.name}");

        // 기존 버튼들 제거
        ClearChoiceButtons(parentTransform);

        // 선택지가 없는 경우 경고
        if (choices.Count == 0)
        {
            Debug.LogWarning($"이벤트 '{displayName}'에 선택지가 없습니다!");
            return;
        }

        // 각 선택지마다 버튼 생성
        for (int i = 0; i < choices.Count; i++)
        {
            Debug.Log($"선택지 {i + 1} 버튼 생성 중: '{choices[i].choiceText}'");
            GameObject buttonObj = UnityEngine.Object.Instantiate(buttonPrefab, parentTransform);
            buttonObj.name = $"ChoiceButton_{i}_{choices[i].choiceText}";
            SetupChoiceButton(buttonObj, i, choices[i], onChoiceCallback);
            Debug.Log($"선택지 {i + 1} 버튼 생성 완료: {buttonObj.name}");
        }

        Debug.Log($"모든 선택지 버튼 생성 완료! 총 {choices.Count}개");
    }

    /// <summary>
    /// 개별 선택지 버튼 설정 (EventChoiceUI 스크립트 우선 사용)
    /// </summary>
    /// <param name="buttonObj">버튼 GameObject</param>
    /// <param name="choiceIndex">선택지 인덱스</param>
    /// <param name="choice">선택지 데이터</param>
    /// <param name="onChoiceCallback">콜백 함수</param>
    protected virtual void SetupChoiceButton(GameObject buttonObj, int choiceIndex, EventChoice choice, System.Action<int> onChoiceCallback)
    {
        // EventChoiceUI 스크립트가 있는지 확인
        EventChoiceUI choiceUI = buttonObj.GetComponent<EventChoiceUI>();
        
        if (choiceUI != null)
        {
            // EventChoiceUI 스크립트를 통한 자동 설정
            choiceUI.SetupChoice(choice, choiceIndex, onChoiceCallback);
            Debug.Log($"EventChoiceUI 스크립트로 선택지 설정 완료: {choice.choiceText}");
        }
        else
        {
            // 기존 방식: 수동 컴포넌트 설정
            SetupChoiceButtonManually(buttonObj, choiceIndex, choice, onChoiceCallback);
            Debug.Log($"수동 방식으로 선택지 설정 완료: {choice.choiceText}");
        }
    }

    /// <summary>
    /// 수동 방식으로 선택지 버튼 설정 (EventChoiceUI가 없는 경우의 fallback)
    /// </summary>
    /// <param name="buttonObj">버튼 GameObject</param>
    /// <param name="choiceIndex">선택지 인덱스</param>
    /// <param name="choice">선택지 데이터</param>
    /// <param name="onChoiceCallback">콜백 함수</param>
    protected virtual void SetupChoiceButtonManually(GameObject buttonObj, int choiceIndex, EventChoice choice, System.Action<int> onChoiceCallback)
    {
        Debug.Log($"수동 버튼 설정 시작 - 선택지 {choiceIndex}: {choice.choiceText}");
        
        // Button 컴포넌트 가져오기
        var button = buttonObj.GetComponent<UnityEngine.UI.Button>();
        if (button == null)
        {
            Debug.Log($"GameObject에서 Button을 찾을 수 없음, 하위 오브젝트에서 검색 중...");
            button = buttonObj.GetComponentInChildren<UnityEngine.UI.Button>();
        }

        if (button != null)
        {
            Debug.Log($"Button 컴포넌트 발견: {button.name}");
            
            // 버튼 클릭 이벤트 설정
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => {
                Debug.Log($"버튼 클릭됨! 선택지 {choiceIndex}: {choice.choiceText}");
                onChoiceCallback?.Invoke(choiceIndex);
            });
            
            Debug.Log($"onClick 이벤트 설정 완료 - 선택지 {choiceIndex}");
        }
        else
        {
            Debug.LogError($"Button 컴포넌트를 찾을 수 없습니다: {buttonObj.name}");
            Debug.LogError($"GameObject 구조를 확인하세요. Button 컴포넌트가 있어야 합니다.");
        }

        // TextMeshPro 텍스트 설정
        var tmpText = buttonObj.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        if (tmpText != null)
        {
            tmpText.text = choice.choiceText;
        }
        else
        {
            Debug.LogWarning($"TextMeshProUGUI 컴포넌트를 찾을 수 없습니다: {buttonObj.name}");
        }

        // 버튼 이름 설정 (디버깅용)
        buttonObj.name = $"ChoiceButton_{choiceIndex}_{choice.choiceText}";
    }

    /// <summary>
    /// 기존 선택지 버튼들 제거
    /// </summary>
    /// <param name="parentTransform">부모 Transform</param>
    public virtual void ClearChoiceButtons(Transform parentTransform)
    {
        if (parentTransform == null) return;

        int childCount = parentTransform.childCount;
        Debug.Log($"기존 선택지 버튼 정리 중... 현재 자식 개수: {childCount}");

        for (int i = parentTransform.childCount - 1; i >= 0; i--)
        {
            Transform child = parentTransform.GetChild(i);
            Debug.Log($"기존 버튼 제거: {child.name}");
            if (UnityEngine.Application.isPlaying)
            {
                UnityEngine.Object.Destroy(child.gameObject);
            }
            else
            {
                UnityEngine.Object.DestroyImmediate(child.gameObject);
            }
        }

        Debug.Log("기존 선택지 버튼 정리 완료");
    }

    /// <summary>
    /// 특정 Transform에 선택지 UI를 한 번에 생성
    /// </summary>
    /// <param name="buttonPrefab">버튼 프리팹</param>
    /// <param name="parentTransform">부모 Transform</param>
    public virtual void GenerateChoiceUI(GameObject buttonPrefab, Transform parentTransform)
    {
        CreateChoiceButtons(buttonPrefab, parentTransform, (choiceIndex) => {
            OnChoiceSelected(choiceIndex);  // SelectChoice + 이벤트 완료 처리 포함
        });
    }

    /// <summary>
    /// 버튼 스타일 커스터마이징 (선택사항)
    /// </summary>
    /// <param name="buttonObj">버튼 GameObject</param>
    /// <param name="choiceIndex">선택지 인덱스</param>
    protected virtual void CustomizeButtonAppearance(GameObject buttonObj, int choiceIndex)
    {
        // 상속받는 클래스에서 버튼 외형을 커스터마이징할 수 있도록 빈 메서드 제공
        // 예: 특정 선택지는 다른 색상, 특별한 아이콘 등
    }

    #endregion

    #region 티어 기반 랜덤 보상 시스템

    /*
     * 티어 기반 랜덤 보상 시스템 사용법:
     * 
     * === 4가지 티어 선택 방식 ===
     * 1. 가중치 기반 랜덤 (useWeightedRandom = true)
     *    - tierWeights 배열로 각 티어의 선택 확률 설정
     *    - 예: [50, 30, 15, 4, 1] = 1티어 50%, 2티어 30%, 3티어 15%, 4티어 4%, 5티어 1%
     * 
     * 2. 고정 티어 (useWeightedRandom = false, useFixedTier = true)
     *    - fixedTier 값으로 항상 같은 티어 선택
     *    - 예: fixedTier = 3 = 항상 3티어 유닛/아이템만 지급
     * 
     * 3. 특정 티어들 중 확률 선택 (useWeightedRandom = false, useFixedTier = false)
     *    - specificTiers 배열로 원하는 티어들과 각각의 확률 설정
     *    - 기본 모드: 유닛과 아이템 개수를 따로 설정
     * 
     * 4. 고급 보상 타입 제어 (useAdvancedRewardControl = true)
     *    - 특정 티어에서 유닛/아이템/둘다/둘중랜덤을 개별 설정 가능
     *    - RewardType으로 세밀한 제어
     * 
     * === RewardType 옵션 ===
     * - Unit: 해당 티어에서 유닛만 지급
     * - Item: 해당 티어에서 아이템만 지급  
     * - Both: 해당 티어에서 유닛 또는 아이템 중 랜덤 선택 (50:50)
     * - UnitAndItem: 해당 티어에서 유닛과 아이템 모두 지급
     * 
     * === Inspector 설정 예시 ===
     * 
     * 예시 1 - 가중치 기반 (일반적인 가챠):
     * - useWeightedRandom = true
     * - randomUnitCount = 2, randomItemCount = 1
     * - tierWeights = [50, 30, 15, 4, 1]
     * 
     * 예시 2 - 고정 3티어만:
     * - useWeightedRandom = false
     * - useFixedTier = true
     * - fixedTier = 3
     * - randomUnitCount = 1, randomItemCount = 1
     * 
     * 예시 3 - 1티어는 유닛만, 5티어는 둘 다:
     * - useWeightedRandom = false
     * - useFixedTier = false
     * - useAdvancedRewardControl = true
     * - randomUnitCount + randomItemCount = 총 보상 개수
     * - specificTiers = [
     *     {tier: 1, probability: 70, rewardType: Unit},
     *     {tier: 5, probability: 30, rewardType: UnitAndItem}
     *   ]
     * 
     * 예시 4 - 복잡한 설정 (2티어 아이템만, 3티어 둘다, 4티어 랜덤):
     * - useAdvancedRewardControl = true
     * - specificTiers = [
     *     {tier: 2, probability: 50, rewardType: Item},
     *     {tier: 3, probability: 30, rewardType: UnitAndItem}, 
     *     {tier: 4, probability: 20, rewardType: Both}
     *   ]
     */

    /// <summary>
    /// 티어 설정에 따라 랜덤한 보상들을 반환 (오버로드 - EventOutcome용)
    /// </summary>
    /// <param name="count">보상 개수</param>
    /// <param name="useWeightedRandom">가중치 기반 랜덤 사용 여부</param>
    /// <param name="tierWeights">티어별 가중치 배열 [1티어, 2티어, 3티어, 4티어, 5티어]</param>
    /// <param name="useFixedTier">고정 티어 사용 여부</param>
    /// <param name="fixedTier">고정 티어 값</param>
    /// <param name="specificTiers">특정 티어들과 확률 배열</param>
    /// <param name="isUnit">true: 유닛, false: 아이템</param>
    /// <returns>선택된 보상 ID 리스트</returns>
    protected virtual List<int> GetRandomTierRewards(int count, bool useWeightedRandom, float[] tierWeights, 
        bool useFixedTier, int fixedTier, TierProbability[] specificTiers, bool isUnit)
    {
        List<int> rewards = new List<int>();

        for (int i = 0; i < count; i++)
        {
            int selectedTier;

            if (useWeightedRandom)
            {
                // 기존 가중치 기반 선택
                selectedTier = SelectRandomTier(tierWeights);
            }
            else if (useFixedTier)
            {
                // 고정 티어 사용
                selectedTier = fixedTier;
            }
            else
            {
                // 특정 티어들 중에서 확률 기반 선택
                selectedTier = SelectRandomTierFromSpecific(specificTiers);
            }
            
            // 선택된 티어에 해당하는 보상 선택
            int rewardId = isUnit ? GetRandomUnitByTier(selectedTier) : GetRandomItemByTier(selectedTier);
            
            if (rewardId != -1)
            {
                rewards.Add(rewardId);
            }
            else
            {
                Debug.LogWarning($"티어 {selectedTier}에 해당하는 {(isUnit ? "유닛" : "아이템")}이 없습니다.");
            }
        }

        return rewards;
    }

    /// <summary>
    /// 특정 티어 보상에서 보상 타입까지 고려한 보상 생성
    /// </summary>
    /// <param name="count">보상 개수</param>
    /// <param name="specificTiers">티어별 확률과 보상 타입 설정</param>
    /// <returns>선택된 보상들 (유닛과 아이템이 섞여있을 수 있음)</returns>
    protected virtual List<RewardResult> GetRandomTierRewardsWithType(int count, TierProbability[] specificTiers)
    {
        List<RewardResult> rewards = new List<RewardResult>();

        for (int i = 0; i < count; i++)
        {
            if (specificTiers == null || specificTiers.Length == 0)
            {
                Debug.LogWarning("특정 티어 설정이 없습니다.");
                continue;
            }

            // 티어와 보상 타입 선택
            var selectedTierProb = SelectRandomTierProbability(specificTiers);
            int selectedTier = selectedTierProb.tier;
            RewardType rewardType = selectedTierProb.rewardType;

            // 보상 타입에 따라 처리
            switch (rewardType)
            {
                case RewardType.Unit:
                    {
                        int unitId = GetRandomUnitByTier(selectedTier);
                        if (unitId != -1)
                            rewards.Add(new RewardResult { id = unitId, isUnit = true, tier = selectedTier });
                        break;
                    }
                case RewardType.Item:
                    {
                        int itemId = GetRandomItemByTier(selectedTier);
                        if (itemId != -1)
                            rewards.Add(new RewardResult { id = itemId, isUnit = false, tier = selectedTier });
                        break;
                    }
                case RewardType.Both:
                    {
                        // 50:50 확률로 유닛 또는 아이템 선택
                        bool selectUnit = UnityEngine.Random.Range(0f, 1f) < 0.5f;
                        if (selectUnit)
                        {
                            int unitId = GetRandomUnitByTier(selectedTier);
                            if (unitId != -1)
                                rewards.Add(new RewardResult { id = unitId, isUnit = true, tier = selectedTier });
                        }
                        else
                        {
                            int itemId = GetRandomItemByTier(selectedTier);
                            if (itemId != -1)
                                rewards.Add(new RewardResult { id = itemId, isUnit = false, tier = selectedTier });
                        }
                        break;
                    }
                case RewardType.UnitAndItem:
                    {
                        // 유닛과 아이템 모두 지급
                        int unitId = GetRandomUnitByTier(selectedTier);
                        int itemId = GetRandomItemByTier(selectedTier);
                        if (unitId != -1)
                            rewards.Add(new RewardResult { id = unitId, isUnit = true, tier = selectedTier });
                        if (itemId != -1)
                            rewards.Add(new RewardResult { id = itemId, isUnit = false, tier = selectedTier });
                        break;
                    }
            }
        }

        return rewards;
    }

    /// <summary>
    /// 기존 호환성을 위한 메서드 (가중치 기반만)
    /// </summary>
    /// <param name="count">보상 개수</param>
    /// <param name="tierWeights">티어별 가중치 배열 [1티어, 2티어, 3티어, 4티어, 5티어]</param>
    /// <param name="isUnit">true: 유닛, false: 아이템</param>
    /// <returns>선택된 보상 ID 리스트</returns>
    protected virtual List<int> GetRandomTierRewards(int count, float[] tierWeights, bool isUnit)
    {
        return GetRandomTierRewards(count, true, tierWeights, false, 1, new TierProbability[0], isUnit);
    }

    /// <summary>
    /// 가중치에 따라 랜덤 티어 선택
    /// </summary>
    /// <param name="tierWeights">티어별 가중치 배열 [1티어, 2티어, 3티어, 4티어, 5티어]</param>
    /// <returns>선택된 티어 (1~5)</returns>
    protected virtual int SelectRandomTier(float[] tierWeights)
    {
        if (tierWeights == null || tierWeights.Length == 0)
        {
            Debug.LogWarning("티어 가중치가 설정되지 않았습니다. 기본값 1티어를 반환합니다.");
            return 1;
        }

        // 총 가중치 계산
        float totalWeight = 0f;
        for (int i = 0; i < tierWeights.Length; i++)
        {
            totalWeight += tierWeights[i];
        }

        if (totalWeight <= 0f)
        {
            Debug.LogWarning("모든 티어 가중치가 0입니다. 기본값 1티어를 반환합니다.");
            return 1;
        }

        // 랜덤 값으로 티어 선택
        float randomValue = UnityEngine.Random.Range(0f, totalWeight);
        float currentWeight = 0f;

        for (int i = 0; i < tierWeights.Length; i++)
        {
            currentWeight += tierWeights[i];
            if (randomValue <= currentWeight)
            {
                return i + 1; // 티어는 1부터 시작
            }
        }

        // 안전장치: 마지막 티어 반환
        return tierWeights.Length;
    }

    /// <summary>
    /// 특정 티어들 중에서 확률에 따라 랜덤 선택
    /// </summary>
    /// <param name="specificTiers">선택 가능한 티어들과 각각의 확률</param>
    /// <returns>선택된 티어 (1~5)</returns>
    protected virtual int SelectRandomTierFromSpecific(TierProbability[] specificTiers)
    {
        if (specificTiers == null || specificTiers.Length == 0)
        {
            Debug.LogWarning("특정 티어 설정이 없습니다. 기본값 1티어를 반환합니다.");
            return 1;
        }

        // 총 확률 계산
        float totalProbability = 0f;
        foreach (var tierProb in specificTiers)
        {
            totalProbability += tierProb.probability;
        }

        if (totalProbability <= 0f)
        {
            Debug.LogWarning("모든 티어 확률이 0입니다. 첫 번째 티어를 반환합니다.");
            return specificTiers[0].tier;
        }

        // 랜덤 값으로 티어 선택
        float randomValue = UnityEngine.Random.Range(0f, totalProbability);
        float currentProbability = 0f;

        foreach (var tierProb in specificTiers)
        {
            currentProbability += tierProb.probability;
            if (randomValue <= currentProbability)
            {
                return tierProb.tier;
            }
        }

        // 안전장치: 마지막 티어 반환
        return specificTiers[specificTiers.Length - 1].tier;
    }

    /// <summary>
    /// 특정 티어들 중에서 확률에 따라 TierProbability 객체를 랜덤 선택
    /// </summary>
    /// <param name="specificTiers">선택 가능한 티어들과 각각의 확률</param>
    /// <returns>선택된 TierProbability 객체</returns>
    protected virtual TierProbability SelectRandomTierProbability(TierProbability[] specificTiers)
    {
        if (specificTiers == null || specificTiers.Length == 0)
        {
            Debug.LogWarning("특정 티어 설정이 없습니다. 기본값을 반환합니다.");
            return new TierProbability { tier = 1, probability = 100f, rewardType = RewardType.Both };
        }

        // 총 확률 계산
        float totalProbability = 0f;
        foreach (var tierProb in specificTiers)
        {
            totalProbability += tierProb.probability;
        }

        if (totalProbability <= 0f)
        {
            Debug.LogWarning("모든 티어 확률이 0입니다. 첫 번째 설정을 반환합니다.");
            return specificTiers[0];
        }

        // 랜덤 값으로 티어 선택
        float randomValue = UnityEngine.Random.Range(0f, totalProbability);
        float currentProbability = 0f;

        foreach (var tierProb in specificTiers)
        {
            currentProbability += tierProb.probability;
            if (randomValue <= currentProbability)
            {
                return tierProb;
            }
        }

        // 안전장치: 마지막 설정 반환
        return specificTiers[specificTiers.Length - 1];
    }

    /// <summary>
    /// 특정 티어의 랜덤 유닛 ID 반환
    /// </summary>
    /// <param name="tier">원하는 티어 (1~5)</param>
    /// <returns>유닛 ID, 없으면 -1</returns>
    protected virtual int GetRandomUnitByTier(int tier)
    {
        if (!FirebaseManager.isLoaded)
        {
            Debug.LogWarning("Firebase 데이터가 로드되지 않았습니다.");
            return -1;
        }

        // 해당 티어의 유닛들 필터링
        List<int> tierUnits = new List<int>();
        foreach (var kvp in FirebaseManager.units)
        {
            if (kvp.Value.tier == tier)
            {
                tierUnits.Add(kvp.Key);
            }
        }

        if (tierUnits.Count == 0)
        {
            Debug.LogWarning($"티어 {tier}에 해당하는 유닛이 없습니다.");
            return -1;
        }

        // 랜덤 선택
        int randomIndex = UnityEngine.Random.Range(0, tierUnits.Count);
        return tierUnits[randomIndex];
    }

    /// <summary>
    /// 특정 티어의 랜덤 아이템 ID 반환 (아이템은 rarity 기반으로 구현)
    /// </summary>
    /// <param name="tier">원하는 티어 (1~5)</param>
    /// <returns>아이템 ID, 없으면 -1</returns>
    protected virtual int GetRandomItemByTier(int tier)
    {
        if (!FirebaseManager.isLoaded)
        {
            Debug.LogWarning("Firebase 데이터가 로드되지 않았습니다.");
            return -1;
        }

        // 티어를 ItemRarity로 매핑 (예시)
        ItemRarity targetRarity = tier switch
        {
            1 => ItemRarity.common,
            2 => ItemRarity.rare,
            3 => ItemRarity.epic,
            4 => ItemRarity.legend,
            5 => ItemRarity.special,
            _ => ItemRarity.common
        };

        // 해당 희귀도의 아이템들 필터링
        List<int> tierItems = new List<int>();
        foreach (var kvp in FirebaseManager.items)
        {
            if (kvp.Value.rarity == targetRarity)
            {
                tierItems.Add(kvp.Key);
            }
        }

        if (tierItems.Count == 0)
        {
            Debug.LogWarning($"티어 {tier} (희귀도: {targetRarity})에 해당하는 아이템이 없습니다.");
            return -1;
        }

        // 랜덤 선택
        int randomIndex = UnityEngine.Random.Range(0, tierItems.Count);
        return tierItems[randomIndex];
    }

    /// <summary>
    /// 유닛 ID로 티어 조회
    /// </summary>
    /// <param name="unitId">유닛 ID</param>
    /// <returns>티어, 없으면 -1</returns>
    protected virtual int GetUnitTierByID(int unitId)
    {
        var unit = FirebaseManager.GetUnitByID(unitId);
        return unit?.tier ?? -1;
    }

    /// <summary>
    /// 아이템 ID로 티어 조회 (rarity 기반)
    /// </summary>
    /// <param name="itemId">아이템 ID</param>
    /// <returns>티어, 없으면 -1</returns>
    protected virtual int GetItemTierByID(int itemId)
    {
        var item = FirebaseManager.GetItemByID(itemId);
        if (item == null) return -1;

        // ItemRarity를 티어로 매핑
        return item.rarity switch
        {
            ItemRarity.common => 1,
            ItemRarity.rare => 2,
            ItemRarity.epic => 3,
            ItemRarity.legend => 4,
            ItemRarity.special => 5,
            _ => 1
        };
    }

    #endregion
}
