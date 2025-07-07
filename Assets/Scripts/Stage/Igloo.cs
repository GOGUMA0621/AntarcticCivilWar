using AYellowpaper.SerializedCollections;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Igloo : MonoBehaviour, IDamageAble, IStructure
{
    public event System.Action<GameObject> OnDestroyed;

    [SerializeField] private GameObject pfReward;

    private bool isDestroyed = false;
    public float health;
    private float currentHealth;
    [SerializeField] float destroyTime;
    

    [SerializeField] Sprite[] brokenIgloos = new Sprite[5];
    private UnitGroupSO selectedGroup;
    private SpriteRenderer spriteRenderer;
    private int stateInfo = 0;
    private SpawnUnit spawnUnit;
    private int spawnLevel = 0;
    private RectTransform rewardChestViewport;

    private List<float> triggerThresholds = new List<float> { 0.66f, 0.33f, 0.0f };
    private HashSet<float> triggeredThresholds = new HashSet<float>();

    private Dictionary<int, List<(int level, float probability)>> probabilityTable = new Dictionary<int, List<(int level, float probability)>>()
    {
        { 25, new List<(int, float)> { (1, 60f), (2, 40f) } },
        { 40, new List<(int, float)> { (2, 40f), (3, 40f), (4, 20f) } },
        { 60, new List<(int, float)> { (3, 30f), (4, 50f), (5, 20f) } },
        { 85, new List<(int, float)> { (4, 50f), (5, 50f) } },
        { 100, new List<(int, float)> { (5, 100f) } }
    };

    
    [SerializeField,SerializedDictionary("Level", "Reward")] 
    private SerializedDictionary<int, List<UnitGroupSO>> rewardTable = new SerializedDictionary<int, List<UnitGroupSO>>()
    {
        { 1, new List<UnitGroupSO>() },
        { 2, new List<UnitGroupSO>() },
        { 3, new List<UnitGroupSO>() },
        { 4, new List<UnitGroupSO>() },
        { 5, new List<UnitGroupSO>() }
    };


    private void Start()
    {
        spawnUnit = GetComponent<SpawnUnit>();
        stateInfo = brokenIgloos.Length;
        spriteRenderer = GetComponent<SpriteRenderer>();
        currentHealth = health;
        spriteRenderer.enabled = true;
    }

    private void Awake()
    {
        spawnLevel = 1;

        RewardUI rewardUi = FindAnyObjectByType<RewardUI>();
        rewardChestViewport = rewardUi.rewardChestViewport;
        Debug.Log(spawnLevel);
    }

    private void RandomNumUnitList(int level)
    {
        if (level > 0 || level < 5)
        {
            selectedGroup = spawnUnit.levels[level].units[Random.Range(0, spawnUnit.levels[level].units.Count)];
            spawnUnit.SpawnUnits(selectedGroup, this.transform.position, "Mercenary");
        }
    }

    private int RandomLevelNum(float level)
    {
        foreach(var entry in probabilityTable)
        {
            if(level <= entry.Key)
            {
                return ChooseLevel(entry.Value);
            }
        }
        return 1;
    }

    private int ChooseLevel(List<(int level, float probability)> levelProbabilities)
    {
        float total = 0f;
        foreach (var item in levelProbabilities)
        {
            total += item.probability;
        }

        float randomPoint = UnityEngine.Random.Range(0, total);
        float cumulative = 0f;

        foreach (var item in levelProbabilities)
        {
            cumulative += item.probability;
            if(randomPoint < cumulative)
            {
                return item.level;
            }
        }

        return levelProbabilities[0].level;
    }

    public void ReceiveDamage(DamageData damage)
    {
        currentHealth -= damage.damage;
        UpdateIglooState();
        CheckHealthTriggers();

        if (currentHealth <= 0f)
        {
            OnDestroyed?.Invoke(this.gameObject);
            if (IglooSpawnGenerator.igloos.Contains(this.gameObject))
            {
                IglooSpawnGenerator.igloos.Remove(this.gameObject);
            }
            isDestroyed = true;
            StartCoroutine(DestroyIgloo(destroyTime));
        }
    }

    private IEnumerator DestroyIgloo(float time)
    {
        spriteRenderer.enabled = false;
        SpawnReward();
        yield return new WaitForSeconds(time);
        Destroy(gameObject);
    }

    private void UpdateIglooState()
    {
        int maxState = brokenIgloos.Length;
        int newState = maxState - Mathf.FloorToInt(GetNormalizedHealth() * maxState);

        if (newState != stateInfo && newState >= 0 && newState < maxState)
        {
            spriteRenderer.sprite = brokenIgloos[newState];
            stateInfo = newState;
        }

    }

    private void CheckHealthTriggers()
    {
        float healthPercent = GetNormalizedHealth();
        foreach (var threshold in triggerThresholds)
        {
            if (!triggeredThresholds.Contains(threshold) && healthPercent <= threshold)
            {
                triggeredThresholds.Add(threshold);
                RandomNumUnitList(spawnLevel);
            }
        }
    }

    private void SpawnReward()
    {
        RewardChest rewardChest = Instantiate(pfReward, this.transform.position, Quaternion.identity).GetComponent<RewardChest>();
        rewardChest.ConnectReward(GetUnitReward(spawnLevel), null, rewardChestViewport);
    }

    private UnitGroupSO[] GetUnitReward(int level)
    {
        UnitGroupSO[] unitGroups = new UnitGroupSO[rewardTable[level].Count];
        for (int i = 0; i < unitGroups.Length; i++)
        {
            unitGroups[i] = rewardTable[level][i];
        }
        return unitGroups;
    }

    private float GetNormalizedHealth()
    {
        return currentHealth / health;
    }

    public bool IsDestroyed()
    {
        return isDestroyed;
    }
}
