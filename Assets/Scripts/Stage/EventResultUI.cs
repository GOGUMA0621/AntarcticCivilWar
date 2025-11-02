using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class EventResultUI : MonoBehaviour
{
    [Header("UI 참조")]
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private TextMeshProUGUI resultText;
    [SerializeField] private TextMeshProUGUI rewardText;
    [SerializeField] private Button continueButton;
    [SerializeField] private float displayDuration = 3f;

    private void Awake()
    {
        if (resultPanel != null)
            resultPanel.SetActive(false);

        if (continueButton != null)
            continueButton.onClick.AddListener(HideResult);
    }

    /// <summary>
    /// 선택 결과 표시 (확률 기반 결과 지원)
    /// </summary>
    public void ShowResult(EventChoice choice, EventOutcome outcome = null)
    {
        if (resultPanel == null)
        {
            Debug.LogError("ResultPanel이 설정되지 않았습니다.");
            return;
        }

        // 결과 텍스트 설정
        if (resultText != null)
        {
            if (outcome != null)
                resultText.text = outcome.resultText;
            else
                resultText.text = choice.defaultResultText;
        }

        // 보상 텍스트 생성
        if (rewardText != null)
        {
            if (outcome != null)
                rewardText.text = GenerateRewardTextFromOutcome(outcome);
            else
                rewardText.text = GenerateRewardTextFromChoice(choice);
        }

        // UI 표시
        resultPanel.SetActive(true);

        // 자동으로 숨기기 (옵션)
        if (displayDuration > 0)
            StartCoroutine(AutoHideAfterDelay());
    }

    /// <summary>
    /// EventOutcome으로부터 보상 텍스트 생성
    /// </summary>
    private string GenerateRewardTextFromOutcome(EventOutcome outcome)
    {
        string rewards = "";

        if (outcome.goldReward != 0)
        {
            if (outcome.goldReward > 0)
                rewards += $"골드 +{outcome.goldReward}\n";
            else
                rewards += $"골드 {outcome.goldReward}\n";
        }

        if (outcome.healthChange != 0)
        {
            if (outcome.healthChange > 0)
                rewards += $"체력 +{outcome.healthChange}\n";
            else
                rewards += $"체력 {outcome.healthChange}\n";
        }

        if (outcome.unitRewards.Count > 0)
        {
            rewards += "유닛 획득:\n";
            foreach (var unit in outcome.unitRewards)
            {
                if (unit != null)
                    rewards += $"• {unit.name}\n";
            }
        }

        if (outcome.itemRewards.Count > 0)
        {
            rewards += "아이템 획득:\n";
            foreach (var item in outcome.itemRewards)
            {
                if (item != null)
                    rewards += $"• {item.name}\n";
            }
        }

        return string.IsNullOrEmpty(rewards) ? "보상 없음" : rewards.TrimEnd('\n');
    }

    /// <summary>
    /// EventChoice의 기본값으로부터 보상 텍스트 생성
    /// </summary>
    private string GenerateRewardTextFromChoice(EventChoice choice)
    {
        string rewards = "";

        if (choice.defaultGoldReward != 0)
        {
            if (choice.defaultGoldReward > 0)
                rewards += $"골드 +{choice.defaultGoldReward}\n";
            else
                rewards += $"골드 {choice.defaultGoldReward}\n";
        }

        if (choice.defaultHealthChange != 0)
        {
            if (choice.defaultHealthChange > 0)
                rewards += $"체력 +{choice.defaultHealthChange}\n";
            else
                rewards += $"체력 {choice.defaultHealthChange}\n";
        }

        if (choice.defaultUnitRewards.Count > 0)
        {
            rewards += "유닛 획득:\n";
            foreach (var unit in choice.defaultUnitRewards)
            {
                if (unit != null)
                    rewards += $"• {unit.name}\n";
            }
        }

        if (choice.defaultItemRewards.Count > 0)
        {
            rewards += "아이템 획득:\n";
            foreach (var item in choice.defaultItemRewards)
            {
                if (item != null)
                    rewards += $"• {item.name}\n";
            }
        }

        return string.IsNullOrEmpty(rewards) ? "보상 없음" : rewards.TrimEnd('\n');
    }

    /// <summary>
    /// 일정 시간 후 자동으로 숨기기
    /// </summary>
    private IEnumerator AutoHideAfterDelay()
    {
        yield return new WaitForSeconds(displayDuration);
        HideResult();
    }

    /// <summary>
    /// 결과 UI 숨기기
    /// </summary>
    public void HideResult()
    {
        if (resultPanel != null)
            resultPanel.SetActive(false);
    }
}