using SciptableObjects;
using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Text;

#if UNITY_EDITOR
[CustomEditor(typeof(UnitController))]
public class UnitControllerEditor : Editor
{
    private SerializedProperty unitProp;
    private UnitData prevData;
    private string prevDataHash;

    private void OnEnable()
    {
        unitProp = serializedObject.FindProperty("unit");
        UpdatePrevDataHash();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUI.BeginChangeCheck();
        DrawDefaultInspector();
        if (EditorGUI.EndChangeCheck())
        {
            var controller = (UnitController)target;
            var currentData = controller.unit?.data;

            string currentDataHash = GetDataHash(currentData);
            if (currentData != prevData || currentDataHash != prevDataHash)
            {
                ((UnitController)target).SetUnit();
                prevData = currentData;
                prevDataHash = currentDataHash;
            }
        }

        // 버튼은 DrawDefaultInspector() 아래에 위치해야 Inspector에 잘 보입니다.
        if (GUILayout.Button("Set Unit"))
        {
            ((UnitController)target).SetUnit();
        }

        if (GUILayout.Button("Reattach Synergy"))
        {
            ((UnitController)target).ReattachSynergy();
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void UpdatePrevDataHash()
    {
        var controller = (UnitController)target;
        prevData = controller.unit?.data;
        prevDataHash = GetDataHash(prevData);
    }

    private string GetDataHash(UnitData data)
    {
        if (data == null) return "";
        StringBuilder sb = new StringBuilder();

        if (data.UnitHP != null)
            sb.Append(string.Join(",", data.UnitHP));
        if (data.UnitDamage != null)
            sb.Append(string.Join(",", data.UnitDamage));
        // 필요하다면 추가 필드도 여기에 더하세요

        return sb.ToString();
    }
}
#endif
