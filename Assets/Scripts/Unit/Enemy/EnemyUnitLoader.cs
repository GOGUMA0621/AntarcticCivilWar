using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;


public class EnemyUnitLoader : MonoBehaviour
{
    private EnemyPlacementData data;
    public TextAsset enemyDataFile;
    [SerializeField] private PlacementGridManager grid;

    private void Awake()
    {
        // 이전 동작 유지: 에디터/인스펙터에 TextAsset이 있으면 Awake에서 로드
        if (enemyDataFile != null)
        {
            RebuildSceneFromData();
        }
    }

    // 기존 내부 메서드 유지(호출 가능)
    private void RebuildSceneFromData()
    {
        if (enemyDataFile == null)
        {
            Debug.LogWarning("EnemyUnitLoader: enemyDataFile is null, skipping RebuildSceneFromData.");
            return;
        }

        data = JsonUtility.FromJson<EnemyPlacementData>(enemyDataFile.text);

        // 기존 로직 유지
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
                    UnitManager.instance.enemyList.Add(goUnit);
                    goUnit.unit.originPrefab = prefab;
                    goUnit.unit.spriteRenderer.flipX = true;
                    foreach (var allay in UnitManager.instance.allayList)
                    {
                        if (allay != null)
                        {
                            goUnit.unit.detectTarget.AddTarget(allay.GetComponent<IDamageAble>());
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

    // 새로 추가: 런타임에 TextAsset과 Grid를 전달해서 로드하게 함
    public void LoadFromTextAsset(TextAsset textAsset, PlacementGridManager targetGrid)
    {
        if (textAsset == null)
        {
            Debug.LogError("EnemyUnitLoader.LoadFromTextAsset: provided TextAsset is null.");
            return;
        }
        if (targetGrid == null)
        {
            Debug.LogError("EnemyUnitLoader.LoadFromTextAsset: provided PlacementGridManager is null.");
            return;
        }

        enemyDataFile = textAsset;
        grid = targetGrid;
        RebuildSceneFromData();
    }
}
