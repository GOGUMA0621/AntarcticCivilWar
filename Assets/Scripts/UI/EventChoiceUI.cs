using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

/// <summary>
/// 간단한 이벤트 선택지 버튼 UI 컴포넌트
/// 텍스트 표시와 버튼 연결만을 담당하는 경량화 버전
/// </summary>
public class EventChoiceUI : MonoBehaviour
{
    [Header("UI 컴포넌트")]
    [SerializeField] private Button choiceButton;
    [SerializeField] private TextMeshProUGUI choiceTextTMP;

    [Header("자동 검색 설정")]
    [Tooltip("컴포넌트를 자동으로 찾을지 여부 (Inspector에서 설정하지 않은 경우)")]
    [SerializeField] private bool autoFindComponents = true;

    // 선택지 데이터 및 콜백
    private EventChoice choiceData;
    private int choiceIndex;
    private Action<int> onChoiceCallback;

    private void Awake()
    {
        if (autoFindComponents)
        {
            FindUIComponents();
        }
    }

    /// <summary>
    /// UI 컴포넌트들을 자동으로 찾기
    /// </summary>
    private void FindUIComponents()
    {
        // GameObject가 유효한지 먼저 체크
        if (gameObject == null)
        {
            Debug.LogError("GameObject가 null입니다!");
            return;
        }
        
        Debug.Log($"FindUIComponents 시작 - GameObject: {gameObject.name}");
        
        // Button 컴포넌트 찾기
        if (choiceButton == null)
        {
            Debug.Log("Button이 할당되지 않음, 자동 검색 중...");
            choiceButton = GetComponent<Button>();
            if (choiceButton == null)
            {
                Debug.Log("현재 GameObject에서 Button을 찾을 수 없음, 하위 오브젝트 검색 중...");
                choiceButton = GetComponentInChildren<Button>();
            }
            
            if (choiceButton != null)
            {
                Debug.Log($"Button 컴포넌트 발견: {choiceButton.name}");
                Debug.Log($"Button 활성화 상태: {choiceButton.gameObject.activeInHierarchy}");
                Debug.Log($"Button interactable: {choiceButton.interactable}");
            }
            else
            {
                Debug.LogError($"Button 컴포넌트를 찾을 수 없습니다! GameObject: {gameObject.name}");
            }
        }
        else
        {
            Debug.Log($"Button이 이미 할당됨: {choiceButton.name}");
        }
        
        // TextMeshPro 컴포넌트 찾기
        if (choiceTextTMP == null)
        {
            choiceTextTMP = GetComponentInChildren<TextMeshProUGUI>();
            if (choiceTextTMP != null)
            {
                Debug.Log($"TextMeshPro 컴포넌트 발견: {choiceTextTMP.name}");
            }
            else
            {
                Debug.LogWarning($"TextMeshProUGUI 컴포넌트를 찾을 수 없습니다! GameObject: {gameObject.name}");
            }
        }
    }

    /// <summary>
    /// 선택지 설정 및 초기화
    /// </summary>
    /// <param name="choice">선택지 데이터</param>
    /// <param name="index">선택지 인덱스</param>
    /// <param name="callback">선택 시 호출될 콜백</param>
    public void SetupChoice(EventChoice choice, int index, Action<int> callback)
    {
        Debug.Log($"EventChoiceUI 설정 시작 - 선택지 {index}: {choice.choiceText}");
        
        choiceData = choice;
        choiceIndex = index;
        onChoiceCallback = callback;

        // 컴포넌트 다시 찾기 (혹시나 해서)
        FindUIComponents();

        // 텍스트 설정
        SetChoiceText(choice.choiceText);

        // 버튼 이벤트 설정
        SetupButton();

        // 객체 이름 설정 (디버깅용)
        gameObject.name = $"EventChoice_{index}_{choice.choiceText}";
        
        // Canvas 정보 확인
        Canvas parentCanvas = GetComponentInParent<Canvas>();
        if (parentCanvas != null)
        {
            Debug.Log($"부모 Canvas: {parentCanvas.name}, sortingOrder: {parentCanvas.sortingOrder}");
            Debug.Log($"Canvas renderMode: {parentCanvas.renderMode}");
        }
        
        // RectTransform 정보 확인
        RectTransform rectTransform = GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            Debug.Log($"RectTransform 크기: {rectTransform.rect.size}");
            Debug.Log($"RectTransform 위치: {rectTransform.anchoredPosition}");
        }
        
        Debug.Log($"EventChoiceUI 설정 완료 - 선택지 {index}");
    }

    /// <summary>
    /// 선택지 텍스트 설정 (Text와 TextMeshPro 모두 지원)
    /// </summary>
    /// <param name="text">표시할 텍스트</param>
    protected virtual void SetChoiceText(string text)
    {
        if (choiceTextTMP != null)
        {
            choiceTextTMP.text = text;
        }
    }

    /// <summary>
    /// 버튼 클릭 이벤트 설정
    /// </summary>
    protected virtual void SetupButton()
    {
        Debug.Log($"EventChoiceUI SetupButton 시작 - Button: {(choiceButton != null ? choiceButton.name : "null")}");
        
        if (choiceButton != null)
        {
            // 버튼 상태 체크
            Debug.Log($"Button 게임오브젝트 활성화: {choiceButton.gameObject.activeInHierarchy}");
            Debug.Log($"Button 컴포넌트 활성화: {choiceButton.enabled}");
            Debug.Log($"Button interactable: {choiceButton.interactable}");
            
            // Raycast 타겟 체크
            var image = choiceButton.GetComponent<UnityEngine.UI.Image>();
            if (image != null)
            {
                Debug.Log($"Button Image raycastTarget: {image.raycastTarget}");
            }
            else
            {
                Debug.LogWarning("Button에 Image 컴포넌트가 없습니다. Raycast 타겟이 없을 수 있습니다.");
            }
            
            // GraphicRaycaster 체크
            Canvas parentCanvas = choiceButton.GetComponentInParent<Canvas>();
            if (parentCanvas != null)
            {
                var raycaster = parentCanvas.GetComponent<UnityEngine.UI.GraphicRaycaster>();
                Debug.Log($"Canvas에 GraphicRaycaster 있음: {raycaster != null}");
            }
            
            // 안전한 이벤트 설정
            if (choiceButton.onClick != null)
            {
                choiceButton.onClick.RemoveAllListeners();
                choiceButton.onClick.AddListener(() => Debug.Log("직접 호출 테스트 성공!"));
                choiceButton.onClick.AddListener(OnChoiceClicked);
                Debug.Log($"Button onClick 이벤트 설정 완료: {choiceButton.name}");
            }
            else
            {
                Debug.LogError("Button의 onClick 이벤트가 null입니다.");
            }
            
            // 테스트용: 직접 호출해보기
            Debug.Log("테스트: 버튼 직접 클릭 시뮬레이션");
        }
        else
        {
            Debug.LogError($"EventChoiceUI: {gameObject.name}에서 Button 컴포넌트를 찾을 수 없습니다.");
            Debug.LogError("Button 컴포넌트가 할당되었는지 Inspector를 확인하세요.");
        }
    }

    /// <summary>
    /// 선택지 클릭 시 호출
    /// </summary>
    protected virtual void OnChoiceClicked()
    {
        Debug.Log($"=== 선택지 클릭됨! ===");
        Debug.Log($"GameObject: {gameObject.name}");
        Debug.Log($"선택지 텍스트: {choiceData?.choiceText}");
        Debug.Log($"선택지 인덱스: {choiceIndex}");
        Debug.Log($"콜백 함수: {(onChoiceCallback != null ? "있음" : "null")}");
        Debug.Log($"현재 시간: {Time.time}");
        
        // 버튼 상태 재확인
        if (choiceButton != null)
        {
            Debug.Log($"버튼 interactable: {choiceButton.interactable}");
            Debug.Log($"버튼 게임오브젝트 활성화: {choiceButton.gameObject.activeInHierarchy}");
        }
        
        // 콜백 호출
        if (onChoiceCallback != null)
        {
            Debug.Log($"콜백 호출 중... 인덱스: {choiceIndex}");
            try
            {
                onChoiceCallback.Invoke(choiceIndex);
                Debug.Log($"콜백 호출 완료");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"콜백 호출 중 오류 발생: {e.Message}");
            }
        }
        else
        {
            Debug.LogError("콜백 함수가 null입니다! SetupChoice에서 올바르게 설정되었는지 확인하세요.");
        }
    }



    /// <summary>
    /// 선택지 활성화/비활성화
    /// </summary>
    /// <param name="interactable">상호작용 가능 여부</param>
    public void SetInteractable(bool interactable)
    {
        if (choiceButton != null)
        {
            choiceButton.interactable = interactable;
        }
    }

    /// <summary>
    /// 컴포넌트가 비활성화될 때 안전하게 정리
    /// </summary>
    private void OnDisable()
    {
        // 버튼 이벤트 리스너 정리 (NullReference 방지)
        if (choiceButton != null && choiceButton.onClick != null)
        {
            choiceButton.onClick.RemoveAllListeners();
        }
    }

    /// <summary>
    /// 오브젝트가 파괴될 때 안전하게 정리
    /// </summary>
    private void OnDestroy()
    {
        // 콜백 참조 정리
        onChoiceCallback = null;
        choiceData = null;
        
        // 컴포넌트 참조 정리
        if (choiceButton != null && choiceButton.onClick != null)
        {
            choiceButton.onClick.RemoveAllListeners();
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// 에디터에서 컴포넌트 자동 할당 (Inspector 버튼용)
    /// </summary>
    [ContextMenu("Auto Find Components")]
    public void EditorAutoFindComponents()
    {
        try
        {
            if (this != null && gameObject != null)
            {
                FindUIComponents();
                UnityEditor.EditorUtility.SetDirty(this);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"EditorAutoFindComponents 오류: {e.Message}");
        }
    }

    /// <summary>
    /// Unity Editor의 Inspector 검증
    /// </summary>
    private void OnValidate()
    {
        try
        {
            // Editor에서만 실행, Runtime 중에는 실행하지 않음
            if (!Application.isPlaying && this != null && gameObject != null)
            {
                // 컴포넌트가 null인 경우에만 자동 찾기
                if (autoFindComponents)
                {
                    if (choiceButton == null)
                    {
                        choiceButton = GetComponent<Button>();
                        if (choiceButton == null)
                            choiceButton = GetComponentInChildren<Button>();
                    }
                    
                    if (choiceTextTMP == null)
                    {
                        choiceTextTMP = GetComponentInChildren<TextMeshProUGUI>();
                    }
                }
            }
        }
        catch
        {
            // OnValidate에서 예외가 발생해도 무시 (에디터 안정성을 위해)
        }
    }
#endif
}