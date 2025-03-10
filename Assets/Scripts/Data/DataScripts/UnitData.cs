using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditorInternal;
using UnityEngine;

public enum UnitTierType
{
    Normal,
    Special,
    Minion
}

public enum UnitSynergyType
{
    None,
    Circus,
    Summoner,

}

public enum UnitAttackType
{
    Melee,
    Range
}
namespace SciptableObjects
{
    [Serializable]
    [CreateAssetMenu(fileName = "Unit Data",menuName ="Scriptable Object/Unit Data",order = 2)]
    public class UnitData : ScriptableObject
    {

        [SerializeField] private string unitName;
        public string UnitName {  get { return unitName; } }

        [Multiline]
        [SerializeField] private string unitDescription = "";
        public string UnitDescription { get { return unitDescription; } }
        [SerializeField] private int unitPower;
        public int UnitPower { get { return unitPower; } }
        public UnitTierType unitType;
        public UnitSynergyType unitSynergyType;
    


  
        [SerializeField] private float unitHP;
        public float UnitHP { get { return unitHP; } }

        [SerializeField] private int unitMP;
        public int UnitMP { get { return unitMP; } }
    
        [SerializeField] private float unitSpeed;
        public float UnitSpeed { get { return unitSpeed; } }
 
        [SerializeField] private float unitDamage;
        public float UnitDamage { get { return unitDamage; } }



        
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

        [SerializeField] private float unitProjectileMaxSpeed;
        public float UnitMaxProjectileSpeed { get {return unitProjectileMaxSpeed; } }

        [SerializeField] private float unitProjectileMaxHeight;
        public float UnitMaxProjectileHeight { get { return unitProjectileMaxHeight; } }

        [SerializeField] private AnimationCurve projectileTrajectoryAnimationCurve;
        public AnimationCurve ProjectileTrajectoryAnimationCurve { get { return projectileTrajectoryAnimationCurve; } }

        [SerializeField] private AnimationCurve projectileCorrectionAnimationCurve;
        public AnimationCurve ProjectileCorrectionAnimationCurve { get { return projectileCorrectionAnimationCurve; } }

        [SerializeField] private AnimationCurve projectileSpeedAnimationCurve;
        public AnimationCurve ProjectileSpeedAnimationCurve { get {return projectileSpeedAnimationCurve; } }

    }
}
