using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public interface IUnitSkill
{
    void Execute(UnitController unit);
}

public enum UnitType
{
    Normal,
    Special,
    Boss
}

public enum UnitAttackType
{
    Melee,
    Range,
    None
}

[CreateAssetMenu(fileName = "Unit Data",menuName ="Scriptable Object/Unit Data",order = 2)]
public class UnitData : ScriptableObject
{
    [Header("¿Ø¥÷ ¡§∫∏")]
    [SerializeField] private string unitName;
    public string UnitName {  get { return unitName; } }

    [Multiline]
    [SerializeField] private string unitDescription = "";
    public string UnitDescription { get { return unitDescription; } }
    public UnitType unitType;
    
    [Space]

    [Header("¿Ø¥÷ Ω∫≈»")]
    [SerializeField] private float unitHP;
    public float UnitHP { get { return unitHP; } }

    [SerializeField] private float unitMax_MP;
    public float UnitMax_MP { get { return unitMP; } }

    [SerializeField] private float unitMP;
    public float UnitMP { get { return unitMP; } }

    [SerializeField] private float unitSpeed;
    public float UnitSpeed { get { return unitSpeed; } }
 
    [SerializeField] private float unitDamage;
    public float UnitDamage { get { return unitDamage; } }

    [Space]

    [Header("¿Ø¥÷ ∞¯∞›")]
    public UnitAttackType unitAttackType;

    [SerializeField] private bool unitHasKnockback = false;
    public bool UnitHasKnockback { get { return unitHasKnockback; } }

    [SerializeField] private bool unitUnstoppable = false;
    public bool UnitUnstoppable { get{ return unitUnstoppable; } }

    [SerializeField] private float unitAttackSpeed;
    public float UnitAttackSpeed { get { return unitAttackSpeed; } }

    [SerializeField] private float unitAttackDistance;
    public float UnitAttackDistance { get { return unitAttackDistance; } }

    [SerializeField] private float unitSenseRadius;
    public float UnitSenseRadius { get {return unitSenseRadius; } }
    
    [SerializeField] private GameObject unitProjectile;
    public GameObject UnitProjectile { get { return unitProjectile; } }

    [SerializeField] private float unitProjectileSpeed;
    public float UnitProjectileSpeed { get {return unitProjectileSpeed; } }

    [Header("¿Ø¥÷ Ω∫≈≥")]
    [SerializeField] private ScriptableObject ManaSkill;
    public IUnitSkill manaSkill => ManaSkill as IUnitSkill;

    [SerializeField] private float m_SkillDelay;
    public float M_SkillDelay { get { return m_SkillDelay; } }

    [SerializeField] private ScriptableObject UniqeSkill;
    public IUnitSkill uniqeSkill => UniqeSkill as IUnitSkill;

    [Header("æ÷¥œ∏ﬁ¿Ãº«")]
    [SerializeField]private AnimatorOverrideController animatorOverrideController;
    public AnimatorOverrideController AnimatorOverrideController { get { return animatorOverrideController; } }


}
