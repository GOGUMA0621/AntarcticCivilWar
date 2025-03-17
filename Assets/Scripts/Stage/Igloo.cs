using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Igloo : MonoBehaviour, IDamageAble, IStructure
{
    public event System.Action<GameObject> OnDestroyed;

    private bool isDestroyed = false;
    public float health;
    private float currentHealth;
    [SerializeField] float destroyTime;
    

    [SerializeField] Sprite[] brokenIgloos = new Sprite[5];
    private SpriteRenderer spriteRenderer;
    private int stateInfo = 0;
    private SpawnUnit spawnUnit;
    private int spawnLevel = 0;

    private Dictionary<int, List<(int level, float probability)>> probabilityTable = new Dictionary<int, List<(int level, float probability)>>()
    {
        { 25, new List<(int, float)> { (1, 60f), (2, 40f) } },
        { 40, new List<(int, float)> { (2, 40f), (3, 40f), (4, 20f) } },
        { 60, new List<(int, float)> { (3, 30f), (4, 50f), (5, 20f) } },
        { 85, new List<(int, float)> { (4, 50f), (5, 50f) } },
        { 100, new List<(int, float)> { (5, 100f) } }
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
        spawnLevel = RandomLevelNum(1);
        Debug.Log(spawnLevel);
    }

    private void RandomNumUnitList(int level)
    {

        switch (level)
        {
            case 1:
                spawnUnit.SpawnUnits(spawnUnit.level01[Random.Range(0, spawnUnit.level01.Count)], this.transform.position, "Mercenary");
                break;

            case 2:
                spawnUnit.SpawnUnits(spawnUnit.level02[Random.Range(0, spawnUnit.level02.Count)], this.transform.position, "Mercenary");
                break;

            case 3:
                spawnUnit.SpawnUnits(spawnUnit.level03[Random.Range(0, spawnUnit.level03.Count)], this.transform.position, "Mercenary");
                break;

            case 4:
                spawnUnit.SpawnUnits(spawnUnit.level04[Random.Range(0, spawnUnit.level04.Count)], this.transform.position, "Mercenary");
                break;

            case 5:
                spawnUnit.SpawnUnits(spawnUnit.level05[Random.Range(0, spawnUnit.level05.Count)], this.transform.position, "Mercenary");
                break;

            default:
                break;
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
        if (currentHealth <= 0f)
        {
            OnDestroyed?.Invoke(this.gameObject);
            if (IglooSpawnGenerator.igloos.Contains(this.gameObject))
            {
                IglooSpawnGenerator.igloos.Remove(this.gameObject);
            }
            isDestroyed = true;
            StartCoroutine(DestoryIgloo(destroyTime));
        }
    }

    private IEnumerator DestoryIgloo(float time)
    {
        spriteRenderer.enabled = false;
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
            RandomNumUnitList(spawnLevel);
            stateInfo = newState;
        }

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
