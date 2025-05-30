using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum FormationType { Grid, Circle, Random }

namespace UnitFormation
{
    public static class UnitFormationUtility
    {
        /// <summary>
        /// 중심(center) 기준으로 유닛 수(unitCount)만큼 포메이션 위치를 반환합니다.
        /// </summary>
        public static List<Vector3> GetFormationPositions(Vector3 center, int unitCount, FormationType type)
        {
            List<Vector3> positions = new();

            switch (type)
            {
                case FormationType.Grid:
                    {
                        int rowSize = Mathf.CeilToInt(Mathf.Sqrt(unitCount));
                        float spacing = 1.2f;
                        for (int i = 0; i < unitCount; i++)
                        {
                            int row = i / rowSize;
                            int col = i % rowSize;
                            Vector3 offset = new Vector3((col - (rowSize - 1) / 2f) * spacing, (row - (rowSize - 1) / 2f) * spacing, 0);
                            positions.Add(center + offset);
                        }
                        break;
                    }
                case FormationType.Circle:
                    {
                        float radius = Mathf.Sqrt(unitCount) * 0.6f;
                        for (int i = 0; i < unitCount; i++)
                        {
                            float angle = (i / (float)unitCount) * Mathf.PI * 2f;
                            Vector3 offset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * radius;
                            positions.Add(center + offset);
                        }
                        break;
                    }
                case FormationType.Random:
                    {
                        for (int i = 0; i < unitCount; i++)
                        {
                            Vector3 offset = new Vector3(Random.Range(-2f, 2f), Random.Range(-2f, 2f), 0);
                            positions.Add(center + offset);
                        }
                        break;
                    }
            }

            return positions;
        }

        /// <summary>
        /// 포메이션 월드 좌표를 그리드로 변환하고, 겹치지 않는 walkable 노드에 할당합니다.
        /// </summary>
        public static List<Vector2Int> GetFormationGridPositions(
            Vector3 center,
            int unitCount,
            FormationType type,
            System.Func<Vector3, Vector2Int> worldToGridFunc,
            System.Func<Vector2Int, bool> isWalkableFunc,
            int maxRange = 10)
        {
            var worldPositions = GetFormationPositions(center, unitCount, type);
            var assigned = new HashSet<Vector2Int>();
            var result = new List<Vector2Int>();

            foreach (var worldPos in worldPositions)
            {
                Vector2Int gridPos = worldToGridFunc(worldPos);
                Vector2Int walkable = FindClosestWalkable(
                    gridPos,
                    pos => isWalkableFunc(pos) && !assigned.Contains(pos),
                    maxRange
                );
                assigned.Add(walkable);
                result.Add(walkable);
            }
            return result;
        }

        /// <summary>
        /// origin에서 가장 가까운 walkable 위치를 BFS로 찾습니다.
        /// isWalkableFunc: 해당 좌표가 이동 가능한지 검사하는 함수 필요.
        /// </summary>
        public static Vector2Int FindClosestWalkable(Vector2Int origin, System.Func<Vector2Int, bool> isWalkableFunc, int maxRange = 10)
        {
            Queue<Vector2Int> queue = new();
            HashSet<Vector2Int> visited = new();

            Vector2Int[] directions = new Vector2Int[]
            {
                Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right,
                new(1, 1), new(-1, 1), new(1, -1), new(-1, -1)
            };

            queue.Enqueue(origin);
            visited.Add(origin);

            while (queue.Count > 0)
            {
                Vector2Int current = queue.Dequeue();

                if (isWalkableFunc(current))
                    return current;

                foreach (var dir in directions)
                {
                    Vector2Int next = current + dir;
                    if (!visited.Contains(next) && WithinRange(origin, next, maxRange))
                    {
                        visited.Add(next);
                        queue.Enqueue(next);
                    }
                }
            }

            return origin; // 못 찾으면 원점 반환
        }

        private static bool WithinRange(Vector2Int origin, Vector2Int pos, int range)
        {
            return Mathf.Abs(pos.x - origin.x) <= range && Mathf.Abs(pos.y - origin.y) <= range;
        }
    }
}



