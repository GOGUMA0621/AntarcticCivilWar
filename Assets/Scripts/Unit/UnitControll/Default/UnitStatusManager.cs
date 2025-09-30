using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitStatusManager : MonoBehaviour
{
    [SerializeField] private UnitController unitController;

    [Header("Unit Level Sprites")]
    [SerializeField] private SpriteRenderer LevelIcon;
    [Space(5)]
    [SerializeField] private Sprite unitLevel1;
    [SerializeField] private Sprite unitLevel2;
    [SerializeField] private Sprite unitLevel3;
    [SerializeField] private Sprite unitLevel4;

    [Header("Status Bars")]
    [SerializeField] private SpriteRenderer HPBar;
    [SerializeField] private SpriteRenderer MPBar;

    private Material hpBarMaterial;
    private Material mpBarMaterial;

    private int unitLevel = 1;
    private float maxHP;
    private float maxMP;

    // Start is called before the first frame update
    void Awake()
    {
        hpBarMaterial = new Material(HPBar.material);
        mpBarMaterial = new Material(MPBar.material);
        HPBar.material = hpBarMaterial;
        MPBar.material = mpBarMaterial;

        unitLevel = unitController.unitLevel;
        UpdateUnitLevelVisual();

        maxHP = unitController.UnitStats.maxHP;
        maxMP = unitController.UnitStats.maxMP;

        UpdateHealthBar();
        UpdateManaBar();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateHealthBar();
        UpdateManaBar();
        UpdateUnitLevelVisual();
    }

    private void UpdateHealthBar()
    {
        if( maxHP != unitController.UnitStats.maxHP)
        {
            maxHP = unitController.UnitStats.maxHP;
            hpBarMaterial.SetFloat("_MaxHP", maxHP);
        }
        float healthPercent = unitController.GetNormalizedHealth();
        hpBarMaterial.SetFloat("_Progress", healthPercent);
    }

    private void UpdateManaBar()
    {
        float manaPercent = unitController.GetNormalizedMana();
        mpBarMaterial.SetFloat("_Progress", manaPercent);
    }

    private void UpdateUnitLevelVisual()
    {
        if (unitController.unitLevel != unitLevel)
        {
            unitLevel = unitController.unitLevel;
            switch (unitLevel)
            {
                case 1:
                    LevelIcon.sprite = unitLevel1;
                    break;
                case 2:
                    LevelIcon.sprite = unitLevel2;
                    break;
                case 3:
                    LevelIcon.sprite = unitLevel3;
                    break;
                case 4:
                    if(unitLevel4 != null)
                        LevelIcon.sprite = unitLevel4;
                    break;
                default:
                    Debug.LogWarning("지원되지 않는 유닛 레벨: " + unitLevel);
                    break;
            }
        }
    }
}
