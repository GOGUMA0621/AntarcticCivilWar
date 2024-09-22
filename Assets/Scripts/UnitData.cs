using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Unit Data",menuName ="Scriptable Object/Unit Data",order =int.MaxValue)]
public class UnitData : ScriptableObject
{
    public enum UnitType
    {
        Normal,
        Special
    }
    [Header("¿Ø¥÷ ¡§∫∏")]
    [SerializeField] private string unitName;
    public string UnitName {  get { return unitName; } }

    [Multiline]
    [SerializeField]private string unitDescription = "";
    public string UnitDescription { get { return unitDescription; } }
    [Space]

    [Header("¿Ø¥÷ Ω∫≈»")]
    [SerializeField] private float unitHP;
    public float UnitHP { get { return unitHP; } }
    
    [SerializeField] private float unitSpeed;
    public float UnitSpeed { get { return unitSpeed; } }

    public enum UnitAttackType
    {
        Melee,
        Projectile
    }
    
    [SerializeField] private float unitDamage;
    public float UnitDamage { get { return unitDamage; } }

    [SerializeField] private float unitAttackSpeed;
    public float UnitAttackSpeed { get { return unitAttackSpeed; } }

    [SerializeField] private float unitAttackDistance;
    public float UnitAttackDistance { get { return unitAttackDistance; } }

    [SerializeField] private float unitSenseRadius;
    public float UnitSenseRadius { get {return unitSenseRadius; } }



}
