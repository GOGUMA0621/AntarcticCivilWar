using UnityEngine;
using UnityEditor;

public static class ComponentExtention
{
    public static Transform GetTransform(this object obj)
    {
        return (obj as Component)?.transform;
    }
}

public class UnitPrefabTools
{
    [MenuItem("Assets/모든 유닛 프리팹 SetUnit 실행",false, 20)]
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
