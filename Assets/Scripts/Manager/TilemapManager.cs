using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


public class TilemapManager : MonoBehaviour
{
    public static TilemapManager instance;

    private Vector3Int minBounds;
    private Vector3Int maxBounds;
    

    public Tilemap tilemap;
    [Tooltip("스폰에 부합하지 않는 타일")]
    public TileBase[] restrictedTiles;

    private HashSet<TileBase> restrictedTileSet;


    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        restrictedTileSet = new HashSet<TileBase>(restrictedTiles);

        FindTileMapEdge(tilemap);
    }

    public Vector3 GetValidPoint(Vector3Int spawnTile)
    {
        bool isValid = false;
        while (!isValid)
        {
            TileBase tileAtSpawn = tilemap.GetTile(spawnTile);

            if (tileAtSpawn != null && !restrictedTileSet.Contains(tileAtSpawn))
            {
                isValid = true;
                Vector3 validPoint = tilemap.CellToWorld(spawnTile);
                return validPoint;
            }
        }

        return Vector3.zero;
    }

    public Vector3Int GetRandomSpawnPoint()
    {
        Vector3Int spawnTile;
        spawnTile = new Vector3Int(Random.Range(minBounds.x, maxBounds.x),Random.Range(minBounds.y,maxBounds.y), 0);
        GetValidPoint(spawnTile);
        if (spawnTile == Vector3Int.zero)
        {
            GetRandomSpawnPoint();
        }
        return spawnTile;
    }

    public Vector3Int GetRandomEdgeSpawnPoint(int minOffset, int maxOffset)
    {
        int side = Random.Range(0, 4);
        int randomOffset = Random.Range(minOffset, maxOffset + 1);
        Vector3Int spawnTile;

        switch (side)
        {
            case 0: //왼쪽
                spawnTile = new Vector3Int(minBounds.x, Random.Range(minBounds.y, maxBounds.y), 0);
                spawnTile.x += randomOffset;
                break;

            case 1: //오른쪽
                spawnTile = new Vector3Int(maxBounds.x, Random.Range(minBounds.y, maxBounds.y), 0);
                spawnTile.x -= randomOffset;
                break;

            case 2: // 위쪽
                spawnTile = new Vector3Int(Random.Range(minBounds.x, maxBounds.x), maxBounds.y, 0);
                spawnTile.y -= randomOffset;
                break;

            case 3: //아래쪽
                spawnTile = new Vector3Int(Random.Range(minBounds.x, maxBounds.y), minBounds.y, 0);
                spawnTile.y += randomOffset;
                break;

            default:
                spawnTile = Vector3Int.zero;
                break;

        }

        GetValidPoint(spawnTile);
        if (spawnTile == Vector3Int.zero)
        {
            GetRandomEdgeSpawnPoint(minOffset, maxOffset);
        }
        
        return spawnTile;
    }

    public Vector3Int GetOppositeDestination(Vector3Int spawnTile,int minOffset , int maxOffset)
    {
        Vector3Int minBounds = GetMinBounds();
        Vector3Int maxBounds = GetMaxBounds();

        Vector3Int destinationTile;

        // 테두리에 가까운지 확인
        bool nearLeft = spawnTile.x - minBounds.x <= maxOffset;
        bool nearRight = maxBounds.x - spawnTile.x <= maxOffset;
        bool nearBottom = spawnTile.y - minBounds.y <= maxOffset;
        bool nearTop = maxBounds.y - spawnTile.y <= maxOffset;

        if (nearLeft) // 왼쪽 테두리 근처 → 오른쪽으로 이동
        {
            destinationTile = new Vector3Int(maxBounds.x, Random.Range(minBounds.y, maxBounds.y), 0);
        }
        else if (nearRight) // 오른쪽 테두리 근처 → 왼쪽으로 이동
        {
            destinationTile = new Vector3Int(minBounds.x, Random.Range(minBounds.y, maxBounds.y), 0);
        }
        else if (nearBottom) // 아래쪽 테두리 근처 → 위쪽으로 이동
        {
            destinationTile = new Vector3Int(Random.Range(minBounds.x, maxBounds.x), maxBounds.y, 0);
        }
        else if (nearTop) // 위쪽 테두리 근처 → 아래쪽으로 이동
        {
            destinationTile = new Vector3Int(Random.Range(minBounds.x, maxBounds.y), minBounds.y, 0);
        }
        else
        {
            // 내부라면 가까운 경계를 찾기
            int distanceLeft = spawnTile.x - minBounds.x;
            int distanceRight = maxBounds.x - spawnTile.x;
            int distanceBottom = spawnTile.y - minBounds.y;
            int distanceTop = maxBounds.y - spawnTile.y;

            // 가장 가까운 경계를 찾음
            int minDistance = Mathf.Min(distanceLeft, distanceRight, distanceBottom, distanceTop);

            if (minDistance == distanceLeft) destinationTile = new Vector3Int(maxBounds.x, spawnTile.y, 0);
            else if (minDistance == distanceRight) destinationTile = new Vector3Int(minBounds.x, spawnTile.y, 0);
            else if (minDistance == distanceBottom) destinationTile = new Vector3Int(spawnTile.x, maxBounds.y, 0);
            else destinationTile = new Vector3Int(spawnTile.x, minBounds.y, 0);
        }

        GetValidPoint(destinationTile);
        if(destinationTile == Vector3Int.zero)
        {
            GetOppositeDestination(spawnTile, minOffset, maxOffset);
        }

        return destinationTile; // 월드 좌표 변환
    }


    private void FindTileMapEdge(Tilemap tilemap)
    {
        BoundsInt bounds = tilemap.cellBounds;
        for (int x = bounds.xMin; x <= bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y <= bounds.yMax; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);
                if (tilemap.GetTile(pos) != null)
                {
                    if (x < minBounds.x) minBounds.x = x;
                    if (x > maxBounds.x) maxBounds.x = x;
                    if (y < minBounds.y) minBounds.y = y;
                    if (y > maxBounds.y) maxBounds.y = y;
                }
            }
        }
    }

    public Vector3Int GetMinBounds() => minBounds;
    public Vector3Int GetMaxBounds() => maxBounds;
}
