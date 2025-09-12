using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

// filepath: e:\Github\AntarcticCivilWar\Assets\Scripts\Stage\RoundMapGenerator.cs
public class RoundMapGenerator : MonoBehaviour
{
    public int width = 5;   // 맵의 가로 크기
    public int height = 3;  // 맵의 세로 크기
    public int originX = 0; // 시작 X좌표
    public int originY = 0; // 시작 Y좌표
    public RuleTile tilePrefab; // 사용할 룰타일
    public Grid grid; // Unity Grid 컴포넌트
    public Tilemap tilemap; // Tilemap 컴포넌트

    void Start()
    {
        GenerateMap();
    }

    public void GenerateMap()
    {
        if (tilemap != null)
            tilemap.ClearAllTiles();

        int totalTiles = width * height;
        int minTiles = Mathf.CeilToInt(totalTiles * 0.4f); // 최소 40% 타일 개수
        HashSet<Vector3Int> placedTiles = new HashSet<Vector3Int>();
        int tryCount = 0;

        while (placedTiles.Count < minTiles && tryCount < 100)
        {
            int chunkWidth = Random.Range(2, Mathf.Min(4, width + 1));  // 2~3
            int chunkHeight = Random.Range(2, Mathf.Min(4, height + 1)); // 2~3

            int chunkStartX = Random.Range(originX, originX + width - chunkWidth + 1);
            int chunkStartY = Random.Range(originY, originY + height - chunkHeight + 1);

            for (int y = 0; y < chunkHeight; y++)
            {
                for (int x = 0; x < chunkWidth; x++)
                {
                    Vector3Int cellPos = new Vector3Int(chunkStartX + x, chunkStartY + y, 0);
                    if (!placedTiles.Contains(cellPos))
                    {
                        tilemap.SetTile(cellPos, tilePrefab);
                        placedTiles.Add(cellPos);
                    }
                }
            }
            tryCount++;
        }
    }

    void OnDrawGizmos()
    {
        if (grid == null) return;

        Gizmos.color = Color.gray;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Vector3Int cellPos = new Vector3Int(originX + x, originY + y, 0);
                Vector3 worldPos = grid.CellToWorld(cellPos) + new Vector3(0.5f, 0.5f, 0);
                Gizmos.DrawWireCube(worldPos, Vector3.one);
            }
        }
    }
}
