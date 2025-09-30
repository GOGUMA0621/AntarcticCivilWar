using UnityEditor;
using UnityEngine;

public class UnitPrefabTools
{
    [MenuItem("Assets/모든 유닛 프리팹 SetUnit 실행", false, 20)]
    public static void RunSetUnitOnAllUnitPrefabs()
    {
        // Assets 폴더 내 모든 프리팹 경로 가져오기
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" });
        int count = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) continue;

            var unitController = prefab.GetComponent<UnitController>();
            if (unitController != null)
            {
                unitController.SetUnit();
                EditorUtility.SetDirty(prefab);
                count++;
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"SetUnit() 실행 완료: {count}개 프리팹");
    }
}