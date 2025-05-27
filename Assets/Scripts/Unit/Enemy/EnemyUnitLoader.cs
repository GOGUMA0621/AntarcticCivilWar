using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;


public class EnemyUnitLoader : MonoBehaviour
{
    private EnemyPlacementData data;
    [SerializeField] private TextAsset enemyDataFile;
    [SerializeField] private PlacementGridManager grid;
    
    private void Awake()
    {
        if (enemyDataFile == null)
        {
            Debug.LogError("Enemy data file is not assigned in the inspector.");
            return;
        }
        // var grids = FindObjectsOfType<PlacementGridManager>();
        // foreach (var grid in grids)
        // {
        //     if (grid != null && grid.tag == "Enemy")
        //     {
        //         this.grid = grid;
        //         break;
        //     }
        // }

        RebuildSceneFromData();
    }

    private void RebuildSceneFromData()
    {
        data = JsonUtility.FromJson<EnemyPlacementData>(enemyDataFile.text);

        foreach (var enemy in data.enemies)
        {
            GameObject prefab = null;

            if (!string.IsNullOrEmpty(enemy.assetPath))
            {
                var path = enemy.assetPath.Replace("Assets/Resources/", "").Replace(".prefab", "");
                prefab = Resources.Load<GameObject>(path);
            }

            if (prefab != null)
            {
                var enemyPos = GridUtility.GridToWorld(enemy.gridPos, grid.origin, grid.cellSize);
                GameObject go = Instantiate(prefab, enemyPos, Quaternion.identity);
                go.transform.position = GridUtility.GridToWorld(enemy.gridPos, grid.origin, grid.cellSize);
                go.name = prefab.name + $"({enemy.gridPos.x},{enemy.gridPos.y})";
                go.tag = "Enemy";
                if (go.TryGetComponent<UnitController>(out var goUnit))
                {
                    SynergyManager.instance.RegisterUnit(goUnit, false);
                    goUnit.unit.originPrefab = prefab;
                    goUnit.unit.spriteRenderer.flipX = true;
                    foreach (var allay in UnitManager.instance.allayList)
                    {
                        if (allay != null)
                        {
                            goUnit.unit.detectTarget.AddTarget(allay.gameObject);
                        }
                    }
                }
                else
                {
                    Debug.LogWarning($"유닛 컴포넌트가 없음: {enemy.unitId}, path: {enemy.assetPath}");
                }
            }
            else
            {
                Debug.LogWarning($"프리팹 로드 실패: {enemy.unitId}, path: {enemy.assetPath}");
            }
        }
    }
}
