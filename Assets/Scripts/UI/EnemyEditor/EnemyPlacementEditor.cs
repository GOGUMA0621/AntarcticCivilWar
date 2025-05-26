using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using System;

[System.Serializable]
public class EnemyUnitData
{
    public string unitId;
    public Vector2Int gridPos;
    public string assetPath;
}

[System.Serializable]
public class EnemyPlacementData
{
    public List<EnemyUnitData> enemies = new();
}

public class EnemyPlacementEditor : EditorWindow
{
    private GameObject selectedPrefab;
    private GameObject bard;
    private GameObject blast;
    private GameObject circus;
    private GameObject cooking;
    private GameObject resistance;
    private EnemyPlacementData data = new();
    private Vector2 scroll;
    private const string savePath = "Assets/EnemyJson/enemy_placement.json";
    private PlacementGridManager grid;

    private Dictionary<Vector2Int, Sprite> spriteCache = new();

    [MenuItem("Tools/적 유닛 배치 툴")]
    public static void OpenWindow()
    {
        var window = GetWindow<EnemyPlacementEditor>("적 유닛 배치 툴");
        window.minSize = new Vector2(425, 600); // 최소 창 크기
        window.maxSize = new Vector2(425, 1000);
    }

    private void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
        grid = GameObject.FindObjectOfType<PlacementGridManager>();

        string path = "Assets/Resources/Penguins";
        bard = LoadPrefabByName("BardPenguin", path);
        blast = LoadPrefabByName("BlastPenguin", path);
        circus = LoadPrefabByName("CircusDagger", path);
        cooking = LoadPrefabByName("CookingPenguin", path);
        resistance = LoadPrefabByName("Resistance_Normal_Penguin", path);

        RebuildSceneFromData();
    }

    private GameObject LoadPrefabByName(string penguinName, string folder)
    {
        string[] guids = AssetDatabase.FindAssets($"{penguinName} t:Prefab", new[] { folder });

        foreach (var guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);

            if (prefab != null && prefab.name == penguinName)
                return prefab;
        }

        return null;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    private void OnGUI()
    {
        GUILayout.Label("적 유닛 배치 툴", EditorStyles.boldLabel);

        EditorGUILayout.Space(20);

        GUILayout.Label("유닛 선택", EditorStyles.boldLabel);

        EditorGUILayout.BeginVertical();

        if (GUILayout.Button("Bard")) selectedPrefab = bard;
        if (GUILayout.Button("Blast")) selectedPrefab = blast;
        if (GUILayout.Button("Circus")) selectedPrefab = circus;
        if (GUILayout.Button("Cooking")) selectedPrefab = cooking;
        if (GUILayout.Button("Resistance")) selectedPrefab = resistance;

        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(20);

        if (selectedPrefab != null)
        {
            GUILayout.Label("선택한 배치유닛", EditorStyles.boldLabel);

            SpriteRenderer sr = selectedPrefab.GetComponent<SpriteRenderer>();
            if (sr != null && sr.sprite != null)
            {
                Rect spriteRect = GUILayoutUtility.GetRect(50,50, GUILayout.Width(80), GUILayout.Height(80));
                DrawUnit(sr.sprite, spriteRect);
            }
        }

        EditorGUILayout.Space(20);
        GUILayout.Label("배치 미리보기", EditorStyles.boldLabel);
        DrawGridPreview();

        if (selectedPrefab != null)
        {
            EditorGUILayout.HelpBox($"선택된 유닛: {selectedPrefab.name}", MessageType.Info);
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("현재 배치 목록", EditorStyles.boldLabel);

        scroll = EditorGUILayout.BeginScrollView(scroll);
        foreach (var enemy in data.enemies.ToList())
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"{enemy.unitId} ({enemy.gridPos.x}, {enemy.gridPos.y})");
            if (GUILayout.Button("삭제", GUILayout.Width(50)))
            {
                RemoveUnitAt(enemy.gridPos);
                break;
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space();

        if (GUILayout.Button("저장"))
        {
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(savePath, json);
            AssetDatabase.Refresh();
            Debug.Log("적 배치 정보 저장됨: " + savePath);
        }

        if (GUILayout.Button("불러오기"))
        {
            if (File.Exists(savePath))
            {
                string json = File.ReadAllText(savePath);
                data = JsonUtility.FromJson<EnemyPlacementData>(json);
                RebuildSceneFromData();
            }
            else
            {
                Debug.LogWarning("저장된 json파일이 없음");
            }
        }

        if (GUILayout.Button("전체삭제"))
        {
            foreach (var enemy in data.enemies.ToList())
            {
                RemoveUnitAt(enemy.gridPos);
            }
            data.enemies.Clear();
        }


    }


    private void OnSceneGUI(SceneView sceneView)
    {
        if (grid == null) return;
        Event e = Event.current;

        Handles.color = Color.cyan;
        foreach (var enemy in data.enemies)
        {
            Vector3 pos = GridUtility.GridToWorld(enemy.gridPos, grid.origin, grid.cellSize);
            Handles.DrawWireCube(pos, Vector3.one * grid.cellSize * 0.9f);

            Vector3 newPos = Handles.PositionHandle(pos, Quaternion.identity);
            if (newPos != pos)
            {
                Vector2Int newGridPos = GridUtility.WorldToGrid(newPos, grid.origin, grid.cellSize);
                if (grid.IsInsideGrid(newGridPos) && !data.enemies.Any(eu => eu.gridPos == newGridPos))
                {
                    enemy.gridPos = newGridPos;
                    GameObject go = FindUnitObject(enemy);
                    if (go != null) go.transform.position = GridUtility.GridToWorld(newGridPos, grid.origin, grid.cellSize);
                }
            }
        }

        if (e.type == EventType.MouseDown && e.button == 0 && selectedPrefab != null && !e.alt)
        {
            Vector2 mousePos = e.mousePosition;
            Ray ray = HandleUtility.GUIPointToWorldRay(mousePos);
            Vector3 worldPos = ray.origin;

            Vector2Int gridPos = GridUtility.WorldToGrid(worldPos, grid.origin, grid.cellSize);

            if (!grid.IsInsideGrid(gridPos) || data.enemies.Any(eu => eu.gridPos == gridPos))
            {
                return;
            }

            GameObject go = PlaceUnitPrefabAt(gridPos, selectedPrefab);

            e.Use();
        }

    }

    private void RemoveUnitAt(Vector2Int gridPos)
    {
        var enemy = data.enemies.FirstOrDefault(u => u.gridPos == gridPos);
        if (enemy != null)
        {
            data.enemies.Remove(enemy);
            GameObject obj = FindUnitObject(enemy);
            if (obj) DestroyImmediate(obj);
            spriteCache.Remove(gridPos);
        }
    }

    private void RebuildSceneFromData()
    {
        spriteCache.Clear();

        foreach (var enemy in data.enemies)
        {
            GameObject prefab = null;

            if (!string.IsNullOrEmpty(enemy.assetPath))
            {
                prefab = AssetDatabase.LoadAssetAtPath<GameObject>(enemy.assetPath);
            }

            if (prefab == null)
            {
                prefab = AssetDatabase.FindAssets(enemy.unitId)
                    .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                    .Select(path => AssetDatabase.LoadAssetAtPath<GameObject>(path))
                    .FirstOrDefault(p => p.name == enemy.unitId);
            }

            if (prefab != null)
            {
                GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                go.transform.position = GridUtility.GridToWorld(enemy.gridPos, grid.origin, grid.cellSize);
                go.name = prefab.name + $"({enemy.gridPos.x},{enemy.gridPos.y})";

                SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
                if (sr != null && sr.sprite != null)
                {
                    spriteCache[enemy.gridPos] = sr.sprite;
                }
            }
            else
            {
                Debug.LogWarning($"프리팹 로드 실패: {enemy.unitId}, path: {enemy.assetPath}");
            }
        }
    }

    private GameObject FindUnitObject(EnemyUnitData enemy)
    {
        string name = enemy.unitId + $"({enemy.gridPos.x},{enemy.gridPos.y})";
        return GameObject.Find(name);
    }

    private void DrawGridPreview()
    {
        float cellSize = 40f; // 에디터 창에 그릴 셀의 크기
        int gridWidth = grid.width;
        int gridHeight = grid.height;

        Rect boxRect = GUILayoutUtility.GetRect(gridWidth * cellSize, gridHeight * cellSize);
        GUI.Box(boxRect, GUIContent.none);

        HandleGridClick(boxRect, cellSize, gridWidth, gridHeight);
        DrawGridLines(boxRect, cellSize, gridWidth, gridHeight);
        DrawUnitAtGrid(boxRect, cellSize, gridWidth, gridHeight);

        Debug.Log($"spriteCache에 등록된 스프라이트 수: {spriteCache.Count}");
    }

    private void HandleGridClick(Rect boxRect, float cellSize, int gridWidth, int gridHeight)
    {
        Event e = Event.current;
        if (e.type != EventType.MouseDown || e.button != 0 || selectedPrefab == null)
            return;

        Vector2 mousePos = e.mousePosition;
        if (!boxRect.Contains(mousePos))
            return;

        int gx = Mathf.FloorToInt((mousePos.x - boxRect.x) / cellSize);
        int gy = gridHeight - 1 - Mathf.FloorToInt((mousePos.y - boxRect.y) / cellSize);
        Vector2Int gridPos = new(gx, gy);

        if (data.enemies.Any(eu => eu.gridPos == gridPos))
            return;

        GameObject go = PlaceUnitPrefabAt(gridPos, selectedPrefab);

        data.enemies.Add(new EnemyUnitData
        {
            unitId = selectedPrefab.name,
            gridPos = gridPos,
            assetPath = AssetDatabase.GetAssetPath(selectedPrefab)
        });

        e.Use();
    }

    private GameObject PlaceUnitPrefabAt(Vector2Int gridPos, GameObject prefab)
    {
        GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        go.transform.position = GridUtility.GridToWorld(gridPos, grid.origin, grid.cellSize);
        go.name = prefab.name + $"({gridPos.x},{gridPos.y})";

        SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
        if (sr != null && sr.sprite != null)
        {
            spriteCache[gridPos] = sr.sprite;
        }

        return go;
    }

    private void DrawGridLines(Rect boxRect, float cellSize, int gridWidth, int gridHeight)
    {
        Handles.color = Color.gray;
        for (int x = 0; x <= gridWidth; x++)
        {
            Handles.DrawLine(
                new Vector2(boxRect.x + x * cellSize, boxRect.y),
                new Vector2(boxRect.x + x * cellSize, boxRect.y + gridHeight * cellSize));
        }

        for (int y = 0; y <= gridHeight; y++)
        {
            Handles.DrawLine(
                new Vector2(boxRect.x, boxRect.y + y * cellSize),
                new Vector2(boxRect.x + gridWidth * cellSize, boxRect.y + y * cellSize));
        }
    }

    private void DrawUnitAtGrid(Rect boxRect, float cellSize, int gridWidth, int gridHeight)
    {
        foreach (var enemy in data.enemies)
        {
            int gx = enemy.gridPos.x;
            int gy = enemy.gridPos.y;

            Rect unitRect = new Rect(
                boxRect.x + gx * cellSize + 2,
                boxRect.y + (gridHeight - gy - 1) * cellSize + 2,
                cellSize - 4,
                cellSize - 4);

            if (spriteCache.TryGetValue(enemy.gridPos, out Sprite sprite))
            {
                DrawUnit(sprite, unitRect);
            }

            DrawPositionLabel(unitRect, enemy.gridPos);
        }
    }

    private void DrawUnit(Sprite sprite, Rect rect)
    {
        Texture2D tex = sprite.texture;
        Rect texCoords = new Rect(
            sprite.textureRect.x / tex.width,
            sprite.textureRect.y / tex.height,
            sprite.textureRect.width / tex.width,
            sprite.textureRect.height / tex.height);

        GUI.DrawTextureWithTexCoords(rect, tex, texCoords);
    }

    private void DrawPositionLabel(Rect unitRect, Vector2Int gridPos)
    {
        var style = new GUIStyle(EditorStyles.whiteLabel)
        {
            alignment = TextAnchor.LowerCenter,
            fontSize = 13
        };
        style.normal.textColor = new Color(0.5f, 1f, 0.2f);

        var shadowStyle = new GUIStyle(style);
        shadowStyle.normal.textColor = Color.black;

        GUI.Label(new Rect(unitRect.x + 1, unitRect.y + 1, unitRect.width, unitRect.height), $"{gridPos}", shadowStyle);
        GUI.Label(unitRect, $"{gridPos}", style);
    }
}