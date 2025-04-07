using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using SciptableObjects;
using Unity.VisualScripting;

[CustomEditor(typeof(SciptableObjects.UnitData))]
[CanEditMultipleObjects]
#if UNITY_EDITOR
public class UnitDataEditor : Editor
{
    private UnitData unitData;

    private SerializedProperty unitType;
    private SerializedProperty unitIcon;
    private SerializedProperty unitFaction;
    private SerializedProperty attackType;
    private SerializedProperty unitSynergyType;
    private SerializedProperty unitName;
    private SerializedProperty unitDescription;
    private SerializedProperty unitPower;
    private SerializedProperty unitHP;
    private SerializedProperty unitMP;
    private SerializedProperty unitSpeed;
    private SerializedProperty unitDamage;
    private SerializedProperty unitHasKnockback;
    private SerializedProperty unitUnstoppable;
    private SerializedProperty unitAttackSpeed;
    private SerializedProperty unitAttackDistance;
    private SerializedProperty unitSenseRadius;
    private SerializedProperty unitProjectile;
    private SerializedProperty unitProjectileMaxSpeed;
    private SerializedProperty unitProjectileMaxHeight;
    private SerializedProperty projectileTrajectoryAnimationCurve;
    private SerializedProperty projectileCorrectionAnimationCurve;
    private SerializedProperty projectileSpeedAnimationCurve;

    private void OnEnable()
    {
        unitData = (target as UnitData);

        unitIcon = serializedObject.FindProperty("unitIcon");
        unitType = serializedObject.FindProperty("unitType");
        unitFaction = serializedObject.FindProperty("unitFaction");
        attackType = serializedObject.FindProperty("unitAttackType");
        unitSynergyType = serializedObject.FindProperty("unitSynergyType");
        unitName = serializedObject.FindProperty("unitName");
        unitDescription = serializedObject.FindProperty("unitDescription");
        unitPower = serializedObject.FindProperty("unitPower");
        unitHP = serializedObject.FindProperty("unitHP");
        unitMP = serializedObject.FindProperty("unitMP");
        unitSpeed = serializedObject.FindProperty("unitSpeed");
        unitDamage = serializedObject.FindProperty("unitDamage");
        unitHasKnockback = serializedObject.FindProperty("unitHasKnockback");
        unitUnstoppable = serializedObject.FindProperty("unitUnstoppable");
        unitAttackSpeed = serializedObject.FindProperty("unitAttackSpeed");
        unitAttackDistance = serializedObject.FindProperty("unitAttackDistance");
        unitSenseRadius = serializedObject.FindProperty("unitSenseRadius");
        unitProjectile = serializedObject.FindProperty("unitProjectile");
        unitProjectileMaxSpeed = serializedObject.FindProperty("unitProjectileMaxSpeed");
        unitProjectileMaxHeight = serializedObject.FindProperty("unitProjectileMaxHeight");
        projectileTrajectoryAnimationCurve = serializedObject.FindProperty("projectileTrajectoryAnimationCurve");
        projectileCorrectionAnimationCurve = serializedObject.FindProperty("projectileCorrectionAnimationCurve");
        projectileSpeedAnimationCurve = serializedObject.FindProperty("projectileSpeedAnimationCurve");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        GUIStyle style = EditorStyles.helpBox;
        EditorGUILayout.PropertyField(unitIcon);
        EditorGUILayout.LabelField("일반",EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(unitFaction);
        EditorGUILayout.PropertyField(unitType);
        EditorGUILayout.PropertyField(unitSynergyType);
        EditorGUILayout.PropertyField(unitName);
        EditorGUILayout.PropertyField(unitDescription);
        EditorGUILayout.PropertyField(unitPower);

        EditorGUILayout.LabelField("전투", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        {
            EditorGUILayout.PropertyField(unitHP);
            EditorGUILayout.PropertyField(unitMP);
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.PropertyField(unitSpeed);
        EditorGUILayout.Space(5);
        EditorGUILayout.PropertyField(this.attackType);
        var attackType = (UnitAttackType)this.attackType.intValue;

        EditorGUILayout.PropertyField(unitDamage);
        EditorGUILayout.PropertyField(unitAttackDistance);
        EditorGUILayout.PropertyField(unitAttackSpeed);
        EditorGUILayout.PropertyField(unitSenseRadius);

        switch (attackType)
        {
            case UnitAttackType.Melee:
                {

                }
                break;

            case UnitAttackType.Range:
                {
                    EditorGUILayout.Space(10);
                    EditorGUILayout.LabelField("투사체", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(unitProjectile);
                    EditorGUILayout.PropertyField(unitProjectileMaxSpeed);
                    EditorGUILayout.PropertyField(unitProjectileMaxHeight);
                    EditorGUILayout.PropertyField(projectileTrajectoryAnimationCurve);
                    EditorGUILayout.PropertyField(projectileCorrectionAnimationCurve);
                    EditorGUILayout.PropertyField(projectileSpeedAnimationCurve);
                }
                break;
        }
        EditorGUILayout.Space(10);
        EditorGUILayout.PropertyField(unitHasKnockback);
        EditorGUILayout.PropertyField(unitUnstoppable);

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
