using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class UnitPrefabsLoader
{
    private static Dictionary<string, GameObject> prefabCache = new();
    private static bool isInitialized = false;
    private static GameObject defaultPrefab;
    private static Dictionary<int, Sprite> tierSprites = new();
    private static Dictionary<int, Sprite> tierShopSprites = new();

    public static void Initialize()
    {
        if (isInitialized) return;

        GameObject[] prefabs = Resources.LoadAll<GameObject>("Penguins");
        prefabCache.Clear();

        foreach (var prefab in prefabs)
        {
            string name = prefab.name;
            if (!prefabCache.ContainsKey(name))
                prefabCache.Add(name, prefab);

            if (name == "Resistance_Normal_Penguin")
                defaultPrefab = prefab;
        }

        if (defaultPrefab == null)
            Debug.LogWarning("defaultPrefab is not set.");

        tierSprites[1] = Resources.Load<Sprite>("Frame/common");
        tierSprites[2] = Resources.Load<Sprite>("Frame/rare");
        tierSprites[3] = Resources.Load<Sprite>("Frame/epic");
        tierSprites[4] = Resources.Load<Sprite>("Frame/legend");
        tierSprites[5] = Resources.Load<Sprite>("Frame/special");

        tierShopSprites[1] = Resources.Load<Sprite>("Shop/common");
        tierShopSprites[2] = Resources.Load<Sprite>("Shop/rare");
        tierShopSprites[3] = Resources.Load<Sprite>("Shop/epic");
        tierShopSprites[4] = Resources.Load<Sprite>("Shop/legend");
        tierShopSprites[5] = Resources.Load<Sprite>("Shop/special");

        Debug.Log($"UnitPrefabLoader initialized: {prefabCache.Count} unit prefabs loaded.");
        isInitialized = true;
    }

    public static GameObject GetPrefab(string unitName)
    {
        if (!isInitialized) Initialize();

        if (prefabCache.TryGetValue(unitName, out GameObject prefab))
            return prefab;

        Debug.LogWarning($"'{unitName}' 유닛을 찾을 수 없습니다.");
        return defaultPrefab;
    }

    public static UnitController GetUnitController(string unitName)
    {
        GameObject prefab = GetPrefab(unitName);
        if (prefab == null) return null;

        UnitController unitController = prefab.GetComponent<UnitController>();
        if (unitController == null)
        {
            Debug.LogWarning($"UnitController 컴포넌트가 없는 '{unitName}' 프리팹");
            return null;
        }

        return unitController;
    }

    public static Sprite GetSprite(string unitName)
    {
        GameObject prefab = GetPrefab(unitName);
        if (prefab == null) return null;

        SpriteRenderer sr = prefab.GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            Debug.LogWarning($"SpriteRenderer 컴포넌트가 없는 '{unitName}' 프리팹");
            return null;
        }

        return sr.sprite;
    }

    public static Sprite GetTierSprite(int tier)
    {
        if (!isInitialized) Initialize();

        if (tierSprites.TryGetValue(tier, out Sprite sprite))
            return sprite;

        Debug.LogWarning($"타입 {tier}의 아이콘이 존재하지 않습니다.");
        return null;
    }

    public static Sprite GetShopTierSprite(int tier)
    {
        if (!isInitialized) Initialize();

        if (tierShopSprites.TryGetValue(tier, out Sprite sprite))
            return sprite;

        Debug.LogWarning($"상점 {tier}의 아이콘이 존재하지 않습니다.");
        return null;
    }
}
