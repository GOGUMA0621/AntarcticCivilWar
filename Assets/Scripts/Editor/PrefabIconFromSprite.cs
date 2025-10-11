using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System;

public static class PrefabIconFromSprite
{
    public static void SetIcon(UnityEngine.Object target, Sprite sprite)
    {
        if (sprite == null || target == null) return;
        var tex = ExtractSpriteTexture(sprite);
        if (tex != null)
        {
            EditorGUIUtility.SetIconForObject(target, tex);
            EditorUtility.SetDirty(target);
            AssetDatabase.SaveAssets();
        }
    }

    private static Texture2D ExtractSpriteTexture(Sprite sprite)
    {
        var src = sprite.texture;
        var r = sprite.textureRect;
        int x = Mathf.FloorToInt(r.x);
        int y = Mathf.FloorToInt(r.y);
        int w = Mathf.FloorToInt(r.width);
        int h = Mathf.FloorToInt(r.height);
        try
        {
            var pixels = src.GetPixels(x, y, w, h);
            var newTex = new Texture2D(w, h, src.format, false);
            newTex.SetPixels(pixels);
            newTex.Apply();
            return newTex;
        }
        catch
        {
            return null;
        }
    }
}

public static class PrefabIconFromPreview
{
    public static void SetIconUsingPreview(UnityEngine.Object target, Sprite sprite)
    {
        var preview = AssetPreview.GetAssetPreview(sprite);
        if (preview != null)
        {
            EditorGUIUtility.SetIconForObject(target, preview);
            EditorUtility.SetDirty(target);
            AssetDatabase.SaveAssets();
        }
        else
        {
            // AssetPreview가 아직 준비 안됐으면 재시도
            EditorApplication.delayCall += () => SetIconUsingPreview(target, sprite);
        }
    }
}

public static class SetItemPrefabIcons
{
    [MenuItem("Tools/Prefab Setting/아이템 아이콘 일괄 적용",false, 7)]
    public static void ApplyIconsForResourcesItems()
    {
        string folder = "Assets/Resources/Items";
        var guids = AssetDatabase.FindAssets("t:Prefab", new[] { folder });
        int applied = 0;

        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) continue;

            var contents = PrefabUtility.LoadPrefabContents(path);
            if (contents == null) continue;

            Sprite sprite = FindSpriteFromItemComponent(contents);
            if (sprite == null)
                sprite = contents.GetComponentInChildren<SpriteRenderer>(true)?.sprite;

            if (sprite != null)
            {
                Texture2D tex = TryExtractSpriteTexture(sprite);
                if (tex != null)
                {
                    EditorGUIUtility.SetIconForObject(prefab, tex);
                    EditorUtility.SetDirty(prefab);
                    applied++;
                }
                else
                {
                    // AssetPreview 폴백: 비동기라 바로 안나올 수 있으므로 업데이트 루틴으로 대기
                    TrySetIconUsingPreview(prefab, sprite, () => applied++);
                }
            }
            else
            {
                Debug.Log($"[SetItemPrefabIcons] No sprite found for prefab, skipped: {prefab.name}");
            }

            PrefabUtility.UnloadPrefabContents(contents);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"Item prefab icon apply started/completed. Applied (immediate): {applied}");
    }

    private static Sprite FindSpriteFromItemComponent(GameObject root)
    {
        // Item 컴포넌트의 'icon'/'sprite' 직렬화 필드 찾기
        var comps = root.GetComponentsInChildren<Component>(true);
        foreach (var c in comps)
        {
            if (c == null) continue;
            var so = new SerializedObject(c);
            var prop = so.GetIterator();
            while (prop.NextVisible(true))
            {
                if (prop.propertyType == SerializedPropertyType.ObjectReference && prop.objectReferenceValue is Sprite s)
                {
                    string name = prop.name.ToLower();
                    if (name.Contains("icon") || name.Contains("sprite"))
                        return s;
                    // 후보로 저장은 생략: 우선 icon/sprite 우선
                }
            }
        }
        return null;
    }

    private static Texture2D TryExtractSpriteTexture(Sprite sprite)
    {
        try
        {
            var tex = sprite.texture;
            string texPath = AssetDatabase.GetAssetPath(tex);
            var importer = AssetImporter.GetAtPath(texPath) as TextureImporter;

            bool changedImporter = false;
            bool originalReadable = false;
            if (importer != null && !importer.isReadable)
            {
                // 일시적으로 isReadable 켜고 재임포트 (주의: import 설정 변경)
                originalReadable = importer.isReadable;
                importer.isReadable = true;
                importer.SaveAndReimport();
                changedImporter = true;
            }

            var r = sprite.textureRect;
            int x = Mathf.FloorToInt(r.x);
            int y = Mathf.FloorToInt(r.y);
            int w = Mathf.FloorToInt(r.width);
            int h = Mathf.FloorToInt(r.height);

            var pixels = tex.GetPixels(x, y, w, h);
            var newTex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            newTex.SetPixels(pixels);
            newTex.Apply();
            newTex.name = sprite.name + "_icon";

            // (선택) 원래 importer 설정 복원: 복원하지 않으면 프로젝트 전체에 isReadable 변경이 남음.
            if (changedImporter && importer != null)
            {
                importer.isReadable = originalReadable;
                importer.SaveAndReimport();
            }

            return newTex;
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"TryExtractSpriteTexture failed for {sprite.name}: {ex.Message}");
            return null;
        }
    }

    private static void TrySetIconUsingPreview(UnityEngine.Object prefab, Sprite sprite, Action onApplied = null)
    {
        // 비동기 AssetPreview 사용: 준비될 때까지 업데이트에 등록해서 재시도
        int attempts = 0;
        EditorApplication.CallbackFunction updater = null;
        updater = () =>
        {
            attempts++;
            var preview = AssetPreview.GetAssetPreview(sprite);
            if (preview != null)
            {
                EditorGUIUtility.SetIconForObject(prefab, preview);
                EditorUtility.SetDirty(prefab);
                AssetDatabase.SaveAssets();
                onApplied?.Invoke();
                EditorApplication.update -= updater;
                Debug.Log($"Icon (from preview) applied for {prefab.name}");
            }
            else if (attempts > 30)
            {
                EditorApplication.update -= updater;
                Debug.LogWarning($"AssetPreview not ready for sprite {sprite.name}, gave up after {attempts} attempts.");
            }
        };
        EditorApplication.update += updater;
    }
}
