using System;
using System.Collections.Generic;
using UnityEngine;

public enum UnitTierType
{
    Normal,
    Special,
    Minion,
    Boss
}

public enum UnitFaction
{
    Royal,
    Resistance,
    Mercenary,
    Boss
}

public enum UnitAttackType
{
    Melee,
    Range
}
namespace SciptableObjects
{
    [Serializable]
    [CreateAssetMenu(fileName = "Unit Data", menuName = "Scriptable Object/Unit Data", order = 2)]
    public class UnitData : ScriptableObject
    {

        [SerializeField] private string unitName;
        public string UnitName { get { return unitName; } }

        [Multiline]
        [SerializeField] private string unitDescription = "";
        public string UnitDescription { get { return unitDescription; } }
        [SerializeField] private int unitTier;
        public int UnitTier { get { return unitTier; } }
        public Sprite unitIcon;
        public UnitFaction unitFaction;
        public UnitTierType unitType;
        public List<String> unitSynergyTags = new List<string>();


        public AnimationClip[] unitAnimations;


        [SerializeField] private float[] unitHP = new float[4];
        public float[] UnitHP { get { return unitHP; } }

        [SerializeField] private int unitMP;
        public int UnitMP { get { return unitMP; } }

        [SerializeField] private float unitSpeed;
        public float UnitSpeed { get { return unitSpeed; } }

        [SerializeField] private float[] unitDamage = new float[4];
        public float[] UnitDamage { get { return unitDamage; } }




        public UnitAttackType unitAttackType;

        [SerializeField] private bool unitHasKnockback = false;
        public bool UnitHasKnockback { get { return unitHasKnockback; } }

        [SerializeField] private bool unitUnstoppable = false;
        public bool UnitUnstoppable { get { return unitUnstoppable; } }

        [SerializeField] private float unitAttackSpeed;
        public float UnitAttackSpeed { get { return unitAttackSpeed; } }

        [SerializeField] private float unitAttackDistance;
        public float UnitAttackDistance { get { return unitAttackDistance; } }

        [SerializeField] private GameObject unitProjectile;
        public GameObject UnitProjectile { get { return unitProjectile; } }

        [SerializeField] private float unitProjectileMaxSpeed;
        public float UnitMaxProjectileSpeed { get { return unitProjectileMaxSpeed; } }

        [SerializeField] private float unitProjectileMaxHeight;
        public float UnitMaxProjectileHeight { get { return unitProjectileMaxHeight; } }

        [SerializeField] private AnimationCurve projectileTrajectoryAnimationCurve;
        public AnimationCurve ProjectileTrajectoryAnimationCurve { get { return projectileTrajectoryAnimationCurve; } }

        [SerializeField] private AnimationCurve projectileCorrectionAnimationCurve;
        public AnimationCurve ProjectileCorrectionAnimationCurve { get { return projectileCorrectionAnimationCurve; } }

        [SerializeField] private AnimationCurve projectileSpeedAnimationCurve;
        public AnimationCurve ProjectileSpeedAnimationCurve { get { return projectileSpeedAnimationCurve; } }

    }
}
