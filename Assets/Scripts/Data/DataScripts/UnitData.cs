using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


public enum UnitType
{
    Normal,
    Special
}

public enum UnitAttackType
{
    Melee,
    Range
}

[CreateAssetMenu(fileName = "Unit Data",menuName ="Scriptable Object/Unit Data",order = 2)]
public class UnitData : ScriptableObject
{
    [Header("À¯´Ö Á¤º¸")]
    [SerializeField] private string unitName;
    public string UnitName {  get { return unitName; } }

    [Multiline]
    [SerializeField] private string unitDescription = "";
    public string UnitDescription { get { return unitDescription; } }
    public UnitType unitType;
    
    [Space]

    [Header("À¯´Ö ½ºÅÈ")]
    [SerializeField] private float unitHP;
    public float UnitHP { get { return unitHP; } }
    
    [SerializeField] private float unitSpeed;
    public float UnitSpeed { get { return unitSpeed; } }
 
    [SerializeField] private float unitDamage;
    public float UnitDamage { get { return unitDamage; } }

    [Space]

    [Header("À¯´Ö °ø°Ý")]
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

    [Header("¾Ö´Ï¸ÞÀÌ¼Ç")]
    [SerializeField]private AnimatorOverrideController animatorOverrideController;
    public AnimatorOverrideController AnimatorOverrideController { get { return animatorOverrideController; } }

}
