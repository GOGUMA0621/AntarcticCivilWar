#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(UnitController))]
[CanEditMultipleObjects]
public class UnitControllerCustomEditor : Editor
{
    public override void OnInspectorGUI()
    {

        UnitController unitController = (UnitController)target;
        if (GUILayout.Button("Reattach Synergy"))
        {
            unitController.ReattachSynergy();
        }
        DrawDefaultInspector();
    }
}

#endif
