using System.Linq;
using UnityEngine;

public class SynergyUIController : MonoBehaviour
{
    [SerializeField] private Transform SynergyListParent;
    [SerializeField] private GameObject SynergyUIPrefab;

    private void OnEnable()
    {
        SynergyManager.instance.OnSynergyUpdated += RenderSynergyUI;
        RenderSynergyUI();
    }

    private void Destroy()
    {
        if (!Application.isPlaying) return;
        foreach (Transform child in SynergyListParent)
        {
            Destroy(child.gameObject);
        }
        SynergyManager.instance.OnSynergyUpdated -= RenderSynergyUI;
    }


    /// <summary>
    /// 시너지 UI를 렌더링합니다.
    /// 시너지를 티어별로 내림 차순으로 정렬하고,
    /// 그 다음 시너지 개수로 내림 차순으로 정렬합니다,
    /// 그 다음 이름으로 오름 차순으로 정렬합니다.
    /// </summary>
    public void RenderSynergyUI()
    {
        foreach (Transform child in SynergyListParent)
        {
            Destroy(child.gameObject);
        }

        var synergies = SynergyManager.instance.GetAllaySynergyData();
        synergies = synergies.OrderByDescending(s => s.tier)
            .ThenByDescending(s => s.count)
            .ThenBy(s => s.name).ToList();

        foreach (var synergy in synergies)
        {
            var synergyUI = Instantiate(SynergyUIPrefab, SynergyListParent);
            var synergyUIController = synergyUI.GetComponent<SynergyUI>();
            if (synergyUIController != null)
            {
                synergyUIController.SetSynergyData(synergy);
            }
            else
            {
                Debug.LogWarning("SynergyUIPrefab does not have a SynergyUI component.");
            }
        }
    }
}
