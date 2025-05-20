using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum FormationType { Grid, Circle, Random }

namespace UnitFormation
{
    public static class UnitFormationUtility
    {
        public static List<Vector3> GetFormationPositions(Vector3 center, int unitCount, FormationType type)
        {
            List<Vector3> positions = new();

            switch (type)
            {
                case FormationType.Grid:
                    int rowSize = Mathf.CeilToInt(Mathf.Sqrt(unitCount));
                    float spacing = 1.2f;
                    for (int i = 0; i < unitCount; i++)
                    {
                        int row = i / rowSize;
                        int col = i % rowSize;
                        Vector3 offset = new Vector3((col - rowSize / 2f) * spacing, (row - rowSize / 2f) * spacing);
                        positions.Add(center + offset);
                    }
                    break;

                case FormationType.Circle:
                    float radius = Mathf.Sqrt(unitCount) * 0.6f;
                    for (int i = 0; i < unitCount; i++)
                    {
                        float angle = (i / (float)unitCount) * Mathf.PI * 2f;
                        Vector3 offset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
                        positions.Add(center + offset);
                    }
                    break;

                case FormationType.Random:
                    for (int i = 0; i < unitCount; i++)
                    {
                        Vector3 offset = new Vector3(Random.Range(-2f, 2f), Random.Range(-2f, 2f));
                        positions.Add(center + offset);
                    }
                    break;
            }

            return positions;
        }

        public static Vector2Int FindClosestWalkable(Vector2Int origin, int maxRange = 10)
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

            int steps = 0;

            // while (queue.Count > 0 && steps++ < 1000)
            // {
            //     Vector2Int current = queue.Dequeue();

            //     if ()
            //         return current;

            //     foreach (var dir in directions)
            //     {
            //         Vector2Int next = current + dir;
            //         if (!visited.Contains(next) && WithinRange(origin, next, maxRange))
            //         {
            //             visited.Add(next);
            //             queue.Enqueue(next);
            //         }
            //     }
            // }

            return origin;
        }

        private static bool WithinRange(Vector2Int origin, Vector2Int pos, int range)
        {
            return Mathf.Abs(pos.x - origin.x) <= range && Mathf.Abs(pos.y - origin.y) <= range;
        }
    }
}



