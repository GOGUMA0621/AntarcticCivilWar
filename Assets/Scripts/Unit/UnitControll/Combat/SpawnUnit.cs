using System.Collections.Generic;
using UnityEngine;

public class SpawnUnit : MonoBehaviour
{
    public LayerMask obstacleMask; // 충돌 감지할 레이어 (장애물)
    public float baseRadius = 2f; // 첫 번째 원의 반경
    public float radiusIncrement = 2f; // 각 원마다 반지름 증가량
    public float collisionCheckRadius = 0.5f; // 충돌 체크 반경

    public List<UnitGroupRef> levels = new List<UnitGroupRef>();

    public void SpawnUnits(UnitGroupSO unitToSpawn, Vector3 positionToSpawn, string tag)
    {
        List<(GameObject pfUnit, int count)> unitsToSpawn = new List<(GameObject, int)>();

        // 모든 유닛 타입을 리스트에 추가
        foreach (var unit in unitToSpawn.groupUnits)
        {
            if (unit.count > 0)
            {
                unitsToSpawn.Add((unit.pfUnit, unit.count));
            }
        }

        int totalUnits = 0; // 전체 유닛 개수 계산
        foreach (var unit in unitsToSpawn)
        {
            totalUnits += unit.count;
        }

        int spawnedCount = 0; // 소환된 유닛 수
        int maxAttempts = totalUnits * 3; // 무한 루프 방지용 최대 시도 횟수

        int totalRings = Mathf.CeilToInt(Mathf.Sqrt(totalUnits)); // 필요한 원 개수 계산
        int remainingUnits = totalUnits; // 남은 유닛 수

        int unitIndex = 0; // 현재 소환할 유닛 타입 인덱스

        for (int ring = 0; ring < totalRings && spawnedCount < totalUnits; ring++)
        {
            float currentRadius = baseRadius + (ring * radiusIncrement); // 현재 원의 반경
            int unitsInThisRing = Mathf.Min(6 * (ring + 1), remainingUnits); // 현재 원에 배치할 유닛 수

            for (int i = 0; i < maxAttempts && spawnedCount < totalUnits && unitsInThisRing > 0; i++)
            {
                float angle = (360f / unitsInThisRing) * (i % unitsInThisRing) * Mathf.Deg2Rad; // 원형 각도 계산
                Vector2 spawnPos = positionToSpawn + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle)) * currentRadius;

                var (unitPrefab, count) = unitsToSpawn[unitIndex]; // 현재 소환할 유닛 선택
                GameObject summonedUnit = Instantiate(unitPrefab, spawnPos, Quaternion.identity);
                summonedUnit.tag = tag;

                spawnedCount++; // 소환된 유닛 개수 증가
                remainingUnits--; // 남은 유닛 개수 감소
                unitsToSpawn[unitIndex] = (unitPrefab, count - 1); // 남은 개수 감소

                // 현재 타입의 유닛을 다 소환했다면 다음 유닛으로 이동
                if (unitsToSpawn[unitIndex].count <= 0 && unitIndex < unitsToSpawn.Count - 1)
                {
                    unitIndex++;
                }
            }
        }
    }
}

[System.Serializable]
public class UnitGroupRef
{
    public string groupName;
    public List<UnitGroupSO> units = new List<UnitGroupSO>();
}
