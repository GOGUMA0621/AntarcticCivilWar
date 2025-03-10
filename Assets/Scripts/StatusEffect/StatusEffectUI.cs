using AYellowpaper.SerializedCollections;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatusEffectIconCache
{
    public GameObject statusIconContainer;
    public Image statusBuildupFill ;
    public Image statusActiveTimerFill;
    public Image statusIcon;

    public StatusEffectIconCache(GameObject statusIconContainer, Image statusBuildupFill, Image statusActiveTimerFill, Image statusIcon)
    {
        this.statusIconContainer = statusIconContainer;
        this.statusBuildupFill = statusBuildupFill;
        this.statusActiveTimerFill = statusActiveTimerFill;
        this.statusIcon = statusIcon;
    }
}

public class StatusEffectUI : MonoBehaviour
{
    [SerializeField] private GameObject statusEffectIconTemplate;
    [SerializeField] private SerializedDictionary<StatusEffectType, Sprite> statusEffectSpriteDict;


    private Dictionary<StatusEffectSO, StatusEffectIconCache> statusEffectToIconDict;

    private StatusEffectManager statusEffectManagerRef;

    private void Start()
    {
        statusEffectManagerRef = GetComponentInParent<StatusEffectManager>();

        statusEffectToIconDict = new Dictionary<StatusEffectSO, StatusEffectIconCache>();

        statusEffectManagerRef.ActiveStatus += OnActiveStatus;
        statusEffectManagerRef.UpdateStatusEffect += OnUpdateStatusEffect;
        statusEffectManagerRef.DeactiveStatusEffect += OnDeactiveStatusEffect;
    }

    private void Update()
    {
        
    }

    private StatusEffectIconCache CreateStatusIcon(StatusEffectSO statusEffect)
    {
        if (statusEffectToIconDict.ContainsKey(statusEffect))
        {
            statusEffectToIconDict[statusEffect].statusIconContainer.SetActive(true);
            return statusEffectToIconDict[statusEffect];
        }
        GameObject createdStatusIcon = Instantiate(statusEffectIconTemplate, transform);
        GameObject statusActiveTimer = createdStatusIcon.transform.Find("StatusActiveTimer").gameObject;
        Image statusBuildRadialFill = createdStatusIcon.GetComponent<Image>();
        statusBuildRadialFill.fillAmount = 0;

        Image statusActiveTimerRadial = statusActiveTimer.GetComponent<Image>();
        statusActiveTimerRadial.fillAmount = 0;

        Image statusIcon = createdStatusIcon.transform.Find("Icon").GetComponent<Image>();
        statusIcon.sprite = statusEffectSpriteDict[statusEffect.statusEffectType];

        createdStatusIcon.SetActive(true);
        return new StatusEffectIconCache(createdStatusIcon, statusBuildRadialFill, statusActiveTimerRadial, statusIcon);
    }

    private void OnActiveStatus( StatusEffectSO statusEffect, float buildAmount)
    {
        StatusEffectIconCache statusEffectIconCache = CreateStatusIcon(statusEffect);
        statusEffectToIconDict[statusEffect] = statusEffectIconCache;

        OnUpdateStatusEffect(statusEffect, buildAmount, 0);
    }

    private void OnUpdateStatusEffect(StatusEffectSO statusEffect, float buildAmount, float duration)
    {
        statusEffectToIconDict[statusEffect].statusBuildupFill.fillAmount = buildAmount;
        statusEffectToIconDict[statusEffect].statusActiveTimerFill.fillAmount = duration;
    }

    private void OnDeactiveStatusEffect(StatusEffectSO statusEffect)
    {
        statusEffectToIconDict[statusEffect].statusIconContainer.SetActive(false);
    }

}
