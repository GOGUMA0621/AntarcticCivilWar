using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SynergyTag("Circus", "서커스", SynergyType.Trait)]
public class CircusSynergy : MonoBehaviour, ISynergy
{
    public string Tag => "Circus";

    public string Name => "서커스";

    public bool allowDuplicate => false;

    public string synergyDescription =>
    "서커스 단장을 필드에 배치 할 수 있습니다.\n" +
    "서커스 단장이 스킬 사용시 캐릭터들이 쇼타임을 발동합니다.";

    public Sprite synergyIcon => Resources.Load<Sprite>($"Synergy/{Name}");

    public int[] tierThresholds => new int[] { 1, 4, 6, 7 };

    public int currentTier => lastTier;
    private int lastTier = -1;
    private UnitController unit;
    private UnitController circusLeader;
    private List<IShowtime> activeShowtimeSkills = new List<IShowtime>();
    public readonly SynergyTierEffect[] CircusTierEffects = new SynergyTierEffect[]
    {
        new SynergyTierEffect{ RequiredCount = 1, Description = "서커스 단장을 획득합니다.", StatModifiers = new() { } },
        new SynergyTierEffect{ RequiredCount = 4, Description = "서커스 단장의 공격 속도 20% 증가", StatModifiers = new() { { StatType.AttackSpeed, 0.2f } } },
        new SynergyTierEffect{ RequiredCount = 6, Description = "서커스 단장의 초당 마나 회복 5 추가", StatModifiers = new() { { StatType.ManaGain, 5f } } },
        new SynergyTierEffect{ RequiredCount = 7, Description = "서커스 단장의 받는 피해감소 25% 추가", StatModifiers = new() { { StatType.Endurance, 0.25f } } },
    };

    public void Initialize(UnitController unit)
    {
        this.unit = unit;
    }
    private int GetTier(int count)
    {
        int tier = -1;
        for (int i = 0; i < tierThresholds.Length; i++)
        {
            if (count >= tierThresholds[i])
                tier = i + 1;
        }
        return tier;
    }

    public void OnCountUpdate(int count)
    {
        int tier = GetTier(count);
        lastTier = tier;



        switch (tier)
        {
            case 1:
                TrySpawnCircusLeader();
                Debug.Log(currentTier);
                break;
            case 2:
                circusLeader.AddModifierStat(new StatModifier(Tag, StatType.AttackSpeed, 0.2f, ModifierMethod.MultiplicativePercent));
                circusLeader.unitLevel = 2;
                circusLeader.SetUnit(); // 스탯 갱신
                break;
            case 3:
                circusLeader.AddModifierStat(new StatModifier(Tag, StatType.ManaGain, 5f, ModifierMethod.Additive));
                circusLeader.unitLevel = 3;
                circusLeader.SetUnit(); // 스탯 갱신
                break;
            case 4:
                circusLeader.AddModifierStat(new StatModifier(Tag, StatType.Endurance, 0.25f, ModifierMethod.MultiplicativePercent));
                circusLeader.unitLevel = 4;
                circusLeader.SetUnit(); // 스탯 갱신
                break;
            default:
                if (circusLeader != null)
                {
                    circusLeader.RemoveModifierStats(Tag);
                    SynergyManager.instance.UnregisterUnit(circusLeader, true);
                    Destroy(circusLeader.gameObject);
                }

                break;
        }
    }

    private void TrySpawnCircusLeader()
    {
        if (unit == null)
            return;

        // 이미 단장이 소환되어 있는지 확인
        if (UnitManager.instance.HasUnit("서커스 단장"))
            return;

        // 아군 그리드 매니저에서 정 가운데 좌표 구하기
        var allayGrid = GridManager.instance.allayGrid;
        if (allayGrid == null)
        {
            Debug.LogWarning("아군 그리드 매니저를 찾을 수 없습니다!");
            return;
        }

        Vector2Int centerGridPos = allayGrid.GetCenterGridPos();

        // 소환 위치 결정
        Vector2Int spawnPos = centerGridPos;
        // 만약 해당 위치에 유닛이 있으면, 주변 비어있는 그리드 탐색
        if (!allayGrid.CanPlace(centerGridPos))
        {
            var emptyGrid = allayGrid.GetNearestEmptyGrid(centerGridPos);
            if (emptyGrid != null)
                spawnPos = emptyGrid.Value;
            else
            {
                Debug.LogWarning("서커스 단장을 소환할 공간이 없습니다!");
                return; 
            }
        }

        // 단장 프리팹 소환
        GameObject prefab = Resources.Load<GameObject>("Penguins/prefabs/Units/Resistance/CircusLeader"); // 실제 경로에 맞게 수정
        if (prefab != null)
        {
            Debug.Log("서커스 단장 소환: " + spawnPos);
            GameObject leader = GameObject.Instantiate(prefab, allayGrid.GetGridWorldPos(spawnPos), Quaternion.identity);
            allayGrid.PlaceUnit(leader.GetComponent<Unit>(), spawnPos);
            circusLeader = leader.GetComponent<UnitController>();
            circusLeader.GoPlace();
            SynergyManager.instance.RegisterUnit(circusLeader, true);
        }
    }
}
