using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

[System.Serializable]
public struct UnitGroup
{
    public GameObject pfUnit;
    public int count;
}

[CreateAssetMenu( fileName = "UnitGroupSO", menuName = "Scriptable Object/UnitGroupSO")]
public class UnitGroupSO : ScriptableObject
{
    public int index;

    public List<UnitGroup> groupUnits = new List<UnitGroup>();

    private int previousIndex; // 이전 인덱스 값 저장
    private double lastEditTime; // 마지막 편집 시간 저장
    private const double editDelay = 1f; // 1초 후 최종 적용

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (previousIndex == index || index < 0)
            return;

        // 마지막 수정 시간 갱신
        lastEditTime = EditorApplication.timeSinceStartup;

        // EditorApplication.update를 사용하여 일정 시간이 지난 후 실행
        EditorApplication.update -= CheckAndApplyNameChange;
        EditorApplication.update += CheckAndApplyNameChange;

        previousIndex = index; // 변경된 index 저장
    }

    private void CheckAndApplyNameChange()
    {
        // 사용자가 마지막으로 값을 변경한 후 일정 시간이 지났는지 확인
        if (EditorApplication.timeSinceStartup - lastEditTime < editDelay)
            return;

        // 최종적으로 이름 변경 적용
        ApplyNameChange();

        // 업데이트 함수 제거 (더 이상 실행되지 않도록 함)
        EditorApplication.update -= CheckAndApplyNameChange;
    }

    private void ApplyNameChange()
    {
        string assetPath = AssetDatabase.GetAssetPath(this);
        if (string.IsNullOrEmpty(assetPath)) return;

        string directory = Path.GetDirectoryName(assetPath);
        string newName = index.ToString();
        string newPath = Path.Combine(directory, newName + ".asset");

        if (assetPath != newPath && !File.Exists(newPath))
        {
            AssetDatabase.RenameAsset(assetPath, newName);
            AssetDatabase.SaveAssets();
            Debug.Log($"ScriptableObject renamed to: {newName}");
        }
    }
#endif
}

