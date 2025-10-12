using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class UnitController : MonoBehaviour
{
    public AstarPathfinder asterGrid;
    private AstarPathfinder cachedAstarGrid;

    public AstarPathfinder GetAstarGrid()
    {
        if (cachedAstarGrid == null)
        {
            cachedAstarGrid = FindObjectOfType<AstarPathfinder>();
        }
        return cachedAstarGrid;
    }
}
