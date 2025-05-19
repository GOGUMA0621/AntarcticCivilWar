using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

public class EnemyPlacementEditor : EditorWindow
{
    private GameObject selectedPrefab;
    private EnemyPlacementData data = new();
    private Vector2 scroll;
    private const string savePath = "Assets/enemy_placement.json";

    private float cellSize = 1f;
  
    [MenuItem("Tools/적 유닛 배치 툴")]
    public static void OpenWindow()
    {
        GetWindow<EnemyPlacementEditor>("적 유닛 배치 툴");
    }

    private void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    private void OnGUI()
    {
        GUILayout.Label("적 유닛 배치 툴", EditorStyles.boldLabel);

        selectedPrefab = (GameObject)EditorGUILayout.ObjectField("유닛 프리팹", selectedPrefab, typeof(GameObject), false);
        cellSize = EditorGUILayout.FloatField("그리드 셀 크기", cellSize);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("현재 배치 목록", EditorStyles.boldLabel);

        scroll = EditorGUILayout.BeginScrollView(scroll);
        foreach (var enemy in data.enemies.ToList())
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"{enemy.unitId} ({enemy.gridPos.x}, {enemy.gridPos.y})");
            if (GUILayout.Button("삭제", GUILayout.Width(50)))
            {
                data.enemies.Remove(enemy);
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
            }
        }

        if (GUILayout.Button("전체삭제"))
        {
            data.enemies.Clear();

            foreach (var obj in GameObject.FindGameObjectsWithTag("Unit"))
            {
                DestroyImmediate(obj);
            }
        }
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        var grid = GameObject.FindObjectOfType<PlacementGridManager>();
        Vector3 gridOrigin = grid.origin;

        if (grid == null)
        {
            Debug.LogWarning("PlacementGridManager xx");
            return;
        }

        Event e = Event.current;
        if (e.type == EventType.MouseDown && e.button == 0 && selectedPrefab != null && !e.alt)
        {
            Vector2 mousePos = e.mousePosition;
            Ray ray = HandleUtility.GUIPointToWorldRay(mousePos);
            Vector3 worldPos = ray.origin;

            Vector2Int gridPos = GridUtility.WorldToGrid(worldPos, gridOrigin, cellSize);

            if (!grid.IsInsideGrid(gridPos))
            {
                Debug.LogWarning($"[{gridPos}]는 그리드 범위를 벗어났습니다.");
                return;
            }

            if (data.enemies.Any(eu => eu.gridPos == gridPos))
            {
                Debug.LogWarning($"[{gridPos}] 위치에는 이미 유닛이 배치되어 있습니다.");
                return;
            }

            Vector3 snapped = GridUtility.GridToWorld(gridPos, gridOrigin, cellSize);
            GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(selectedPrefab);
            Undo.RegisterCreatedObjectUndo(go, "Place Enemy Unit");
            go.transform.position = snapped;

            data.enemies.Add(new EnemyUnitData
            {
                unitId = selectedPrefab.name,
                gridPos = gridPos
            });

            e.Use();

        }
    }
}


[System.Serializable]
public class EnemyUnitData
{
    public string unitId;
    public Vector2Int gridPos;
}

[System.Serializable]
public class EnemyPlacementData
{
    public List<EnemyUnitData> enemies = new();
}
