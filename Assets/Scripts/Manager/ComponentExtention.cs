using UnityEngine;
using UnityEditor;
using System.Threading.Tasks;

public static class ComponentExtention
{
    public static Transform GetTransform(this object obj)
    {
        return (obj as Component)?.transform;
    }
}
#if UNITY_EDITOR
public class UnitPrefabTools
{
    [MenuItem("Tools/Prefab Setting/모든 유닛 프리팹 SetUnit 실행", false, 5)]
    public static void SetUnitForAllPrefabs()
    {
        var allPrefabs = Resources.LoadAll<GameObject>("Units");
        foreach (var prefab in allPrefabs)
        {
            var unit = prefab.GetComponent<UnitController>();
            if (unit != null)
            {
                unit.SetUnit();
            }
        }
    }
}

public class ItemPrefabTools
{
    [MenuItem("Tools/Prefab Setting/모든 아이템 프리팹 초기화 실행", false, 6)]
    public static void StartForAllPrefabs()
    {
        var allPrefabs = Resources.LoadAll<GameObject>("Items");
        foreach (var prefab in allPrefabs)
        {
            var item = prefab.GetComponent<Item>();
            if (item != null)
            {
                item.Initialize(item.itemId);
                EditorUtility.SetDirty(prefab); // 변경 사항 저장
            }
        }
        AssetDatabase.SaveAssets(); // 모든 변경 사항 저장
        AssetDatabase.Refresh(); // 에디터 갱신
    }
}

public class FirebaseManagerTools
{
    [MenuItem("Tools/FirebaseManager 리셋", false, 22)]
    public static async Task ResetFirebaseManager()
    {
        await FirebaseManager.ItemLoadData();
        await FirebaseManager.UnitLoadData();
    }
}
#endif