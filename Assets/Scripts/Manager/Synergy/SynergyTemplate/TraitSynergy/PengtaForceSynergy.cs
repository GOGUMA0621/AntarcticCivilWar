using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SynergyTag("Pengta_Force", "펭타 포스",SynergyType.Trait)]
public class PengtaForceSynergy : MonoBehaviour, ISynergy
{
    public string Tag => "Pengta_Force";

    public string Name => "펭타 포스";

    public bool allowDuplicate => false;

    public string synergyDescription => "";

    public Sprite synergyIcon => Resources.Load<Sprite>($"Synergy/{Name}");

    public int currentTier => lastTier;

    public int[] tierThresholds => new int[] { 1, 3, 5, 7, 9 };

    private int lastTier = 0;

    private UnitController unit;

    public void Initialize(UnitController unit)
    {
        this.unit = unit;
    }

    public void OnCountUpdate(int count)
    {

    }

}
