using System.Collections.Generic;
using System.Linq;

public class GridManager : SingleTonBehaviour<GridManager>
{

    public PlacementGridManager allayGrid { get; private set; }
    public PlacementGridManager enemyGrid { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        // 씬의 모든 Grid 컴포넌트 찾기
    }

    public void RegisterAllayGrid(PlacementGridManager grid)
    {
        allayGrid = grid;
    }

    public void RegisterEnemyGrid(PlacementGridManager grid)
    {
        enemyGrid = grid;
    }
}
