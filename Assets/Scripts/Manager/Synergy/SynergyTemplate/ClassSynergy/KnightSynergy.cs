using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SynergyTag("Knight", SynergyType.ClassType)]
public class KnightSynergy : MonoBehaviour, ISynergy
{
    public string Tag => "Knight";

    public bool allowDuplicate => throw new System.NotImplementedException();

    public string synergyDescription => throw new System.NotImplementedException();

    public Sprite synergyIcon => throw new System.NotImplementedException();

    public string Name => "기사";

    public int currentTier => lastTier;

    public int[] tierThresholds => throw new System.NotImplementedException();

    private int lastTier = 0;

    public void Initialize(UnitController unit)
    {
        throw new System.NotImplementedException();
    }

    public void OnCountUpdate(int count)
    {
        throw new System.NotImplementedException();
    }
}
