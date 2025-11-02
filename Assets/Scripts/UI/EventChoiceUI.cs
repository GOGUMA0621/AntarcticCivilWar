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
            return;
        }
        
        // Button 컴포넌트 찾기
        if (choiceButton == null)
        {
            choiceButton = GetComponent<Button>();
            if (choiceButton == null)
            {
                choiceButton = GetComponentInChildren<Button>();
            }
        }
        
        // TextMeshPro 컴포넌트 찾기
        if (choiceTextTMP == null)
        {
            choiceTextTMP = GetComponentInChildren<TextMeshProUGUI>();
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
        if (choiceButton != null)
        {
            // 안전한 이벤트 설정
            if (choiceButton.onClick != null)
            {
                choiceButton.onClick.RemoveAllListeners();
                choiceButton.onClick.AddListener(OnChoiceClicked);
            }
        }
    }

    /// <summary>
    /// 선택지 클릭 시 호출
    /// </summary>
    protected virtual void OnChoiceClicked()
    {
        // 콜백 호출
        onChoiceCallback?.Invoke(choiceIndex);
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