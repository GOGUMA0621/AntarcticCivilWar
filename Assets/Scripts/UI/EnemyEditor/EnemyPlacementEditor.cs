using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using System;
using static UnityEngine.EventSystems.EventTrigger;


#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class EnemyUnitData
{
    public string unitId;
    public Vector2Int gridPos;
    public string assetPath;
    public int level; // 유닛 레벨
}

[System.Serializable]
public class EnemyPlacementData
{
    public List<EnemyUnitData> enemies = new();
}

#if UNITY_EDITOR
[ExecuteInEditMode]
public class EditorSpawnMarker : MonoBehaviour 
{
    private void Awake()
    {
        if(Application.isPlaying)
        {
            Destroy(this);
        }
    }
}

public class EnemyPlacementEditor : EditorWindow
{
    private GameObject selectedPrefab;
    private EnemyPlacementData data = new();
    private Dictionary<Vector2Int, Sprite> spriteCache = new();
    private Vector2 scroll;
    private bool showFileList = false;

    private string fileNameToSave = "enemy_placement";
    private string fileNameToLoad = "enemy_placement";
    private const string saveFolder = "Assets/EnemyJson/";
    private const string penguins_Path = "Penguins";

    private PlacementGridManager grid;

    private class PrefabInfo
    {
        public string displayName;
        public GameObject prefab;
    }

    private List<PrefabInfo> prefabList = new();

    private int selectedLevel = 1; // 선택한 레벨 저장 변수


    [MenuItem("Tools/적 유닛 배치 툴")]
    public static void OpenWindow()
    {
        var window = GetWindow<EnemyPlacementEditor>("적 유닛 배치 툴");
        window.minSize = new Vector2(425, 800); // 최소 창 크기
        window.maxSize = new Vector2(850, 800);
    }

    private void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
        var grid = FindObjectsByType<PlacementGridManager>(FindObjectsSortMode.None);
        foreach (var g in grid)
        {
            if (g != null && g.CompareTag("Enemy"))
            {
                this.grid = g;
                break;
            }
        }

        if (grid == null)
        {
            Debug.LogWarning("PlacementGridManager with tag 'Enemy'를 찾지 못했습니다.");
        }

        LoadAllPrefabs();
    }

    private void LoadAllPrefabs()
    {
        prefabList?.Clear();
        
        GameObject[] loaded = Resources.LoadAll<GameObject>(penguins_Path);

        foreach(var prefab in loaded)
        {
            if(prefab != null)
            {
                prefabList.Add(new PrefabInfo
                {
                    displayName = prefab.name,
                    prefab = prefab
                });
            }
        }
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }


    private void OnGUI()
    {
        EditorGUILayout.Space(10);

        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.BeginVertical(GUILayout.Width(425));

        GUILayout.Label("유닛 선택", EditorStyles.boldLabel);

        foreach (var info in prefabList)
        {
            if(GUILayout.Button(info.displayName))
            {
                selectedPrefab = info.prefab;
            }
        }

        // 여기 추가!
        GUILayout.Label("유닛 레벨 선택", EditorStyles.boldLabel);
        selectedLevel = EditorGUILayout.IntSlider("레벨", selectedLevel, 1, 3);

        if (selectedPrefab != null)
        {
            GUILayout.Label("선택한 배치유닛", EditorStyles.boldLabel);

            SpriteRenderer sr = selectedPrefab.GetComponent<SpriteRenderer>();
            if (sr != null && sr.sprite != null)
            {
                Rect spriteRect = GUILayoutUtility.GetRect(50, 50, GUILayout.Width(80), GUILayout.Height(80));
                DrawUnit(sr.sprite, spriteRect);
            }

            EditorGUILayout.HelpBox($"선택된 유닛: {selectedPrefab.name}", MessageType.Info);
        }

        EditorGUILayout.Space(20);
        GUILayout.Label("배치 미리보기", EditorStyles.boldLabel);
        DrawGridPreview();
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical();

        GUILayout.Label("저장할 파일 이름", EditorStyles.boldLabel);
        fileNameToSave = EditorGUILayout.TextField(fileNameToSave);
        EditorGUILayout.HelpBox("영문, 숫자만 사용하는 것을 권장합니다.", MessageType.Warning);
        
        if (GUILayout.Button("저장"))
        {
           if(!Directory.Exists(saveFolder))
                Directory.CreateDirectory(saveFolder);

            string path = saveFolder + fileNameToSave + ".json";
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(path, json);
            AssetDatabase.Refresh();
            Debug.Log($"{path} 저장됨");
        }

        GUILayout.Space(10);
        GUILayout.Label("불러올 파일 이름", EditorStyles.boldLabel);
        fileNameToLoad = EditorGUILayout.TextField(fileNameToLoad);

        if (GUILayout.Button("불러오기"))
        {
            string path = saveFolder + fileNameToLoad + ".json";
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                data = JsonUtility.FromJson<EnemyPlacementData>(json);
                RebuildSceneFromData();
                Debug.Log($"{path} 불러옴");
            }
            else
            {
                Debug.LogWarning($"{path} 파일 없음");
            }
        }
        GUILayout.Space(10);

        string label = showFileList ? "목록 닫기" : "저장된 파일 목록 보기";
        if (GUILayout.Button($"{label}"))
        {
            showFileList = !showFileList;
        }
        GUILayout.Space(10);
        if (showFileList)
        {
            string[] files = Directory.GetFiles(saveFolder, "*.json");
            foreach (var file in files)
            {
                string fileName = Path.GetFileNameWithoutExtension(file);
                if (GUILayout.Button(fileName))
                    fileNameToLoad = fileName;
            }
        }

        GUILayout.Space(10);

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

        EditorGUILayout.Space(10);
        if (GUILayout.Button("배치된 유닛 전체삭제"))
        {
            ClearAllEnemyObjectInScene();

            spriteCache.Clear();

            data.enemies.Clear();
        }

        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
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
                    var goUnit = go.TryGetComponent<UnitController>(out var goUnitComp) ? goUnitComp : null;
                    if (goUnit != null)
                    {
                        goUnit.GoPlace();
                        SynergyManager.instance.RegisterUnit(goUnit, false);
                    }
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


            data.enemies.Add(new EnemyUnitData
            {
                unitId = selectedPrefab.name,
                gridPos = gridPos,
                assetPath = AssetDatabase.GetAssetPath(selectedPrefab),
                level = selectedLevel // 유닛 레벨 저장
            });

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
        ClearAllEnemyObjectInScene();

        spriteCache?.Clear();

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

                go.tag = "Enemy";

                if(go.GetComponent<EditorSpawnMarker>() == null)
                    go.AddComponent<EditorSpawnMarker>();

                // 저장된 레벨 적용
                var unit = go.GetComponent<Unit>();
                if (unit != null && unit.controller != null)
                {
                    unit.controller.unitLevel = enemy.level;
                }

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

        //Debug.Log($"spriteCache에 등록된 스프라이트 수: {spriteCache.Count}");
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
            assetPath = AssetDatabase.GetAssetPath(selectedPrefab),
            level = selectedLevel // 유닛 레벨 저장
        });

        e.Use();
    }

    private GameObject PlaceUnitPrefabAt(Vector2Int gridPos, GameObject prefab)
    {
        GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        go.transform.position = GridUtility.GridToWorld(gridPos, grid.origin, grid.cellSize);
        go.name = prefab.name + $"({gridPos.x},{gridPos.y})";
        go.tag = "Enemy";
        go.AddComponent<EditorSpawnMarker>();

        // 유닛 레벨 적용
        var unit = go.GetComponent<Unit>();
        if (unit != null && unit.controller != null)
        {
            unit.controller.unitLevel = selectedLevel; // UnitData에 level 필드가 있어야 함
        }

        SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
        if (sr != null && sr.sprite != null)
        {
            spriteCache[gridPos] = sr.sprite;
        }

        return go;
    }

    private void ClearAllEnemyObjectInScene()
    {
        foreach (var enemy in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            bool isToolCreated = enemy.name.Contains("(") && enemy.name.Contains(")");
               

            if(isToolCreated)
            {
#if UNITY_EDITOR
                DestroyImmediate(enemy);
#else
            Destroy(enemy);
#endif
            }
        }
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
#endif