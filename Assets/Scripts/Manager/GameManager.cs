using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("보스")]
    [SerializeField] private GameObject bossTimerUI;
    [SerializeField] private Image bossTimerGauge;

    [Space]
    [Header("보스 UI")]
    [SerializeField] private GameObject bossHPBar;
    [SerializeField] private Image bossHPGauge;
    [SerializeField] private GameObject bossPrefab;

    public float bossSpawnTime = 30f;

    [Space]
    [Header("")]
    // [SerializeField] private PeddlerSpawnGenerator peddlerSpawnGenerator;
    public float peddlerSpawnTime = 30f;
    public float peddlerSpawnTimer = 0f;
    public int minOffset = 2;
    public int maxOffset = 5;

    [Space]
    [Header("Igloo")]
    [SerializeField] private IglooSpawnGenerator iglooSpawnGenerator;
    [SerializeField] private GameObject iglooPrefab;
    //[SerializeField] private float iglooSpawnTime = 30f;
    //private float iglooSpawnTimer = 0f;
    // private Peddler peddler;

    private bool isGeneratedBoss = false;

    private float timer = 0f;
    private BossController boss;

    private void Start()
    {
        bossTimerUI.SetActive(true);
        bossHPBar.SetActive(false);
    }

    private void Update()
    {
        if (bossTimerUI.activeSelf)
        {
            BossTimer(bossSpawnTime);
            bossTimerGauge.fillAmount = 1 - GetNormalizedBossTimer();
        }
        if(boss != null)
        {
            UpdateBossHPBar();
            if (boss.IsDestroyed())
            {
                bossHPBar.SetActive(false);
                boss = null;
                isGeneratedBoss = false;
            }
        }
        //if(peddler == null)
        //{
        //    peddlerSpawnTimer += Time.deltaTime;
        //    if(peddlerSpawnTimer >= peddlerSpawnTime)
        //    {
        //        peddlerSpawnTimer = 0f;
        //        Vector3Int spawnTile = TilemapManager.instance.GetRandomEdgeSpawnPoint(minOffset, maxOffset);
        //        peddler = Instantiate(peddlerSpawnGenerator.pfPeddler, spawnTile, Quaternion.identity).GetComponent<Peddler>();
        //        var targetTile = (Vector3)TilemapManager.instance.GetOppositeDestination(spawnTile, minOffset, maxOffset);
        //        peddler.SetTargetToMove(targetTile);
        //    }
        //}
    }

    private void BossTimer(float time)
    {
        timer += Time.deltaTime;
        if (timer >= time && !isGeneratedBoss)
        {
            timer = 0f;
            isGeneratedBoss = true;
            EnableBossHPBar();
        }
    }

    private void EnableBossHPBar()
    {
        bossTimerUI.SetActive(false);
        bossHPBar.SetActive(true);
        bossHPGauge.fillAmount = 1f;
        Vector3Int spawnTile = TilemapManager.instance.GetRandomSpawnPoint();
        GameObject bossObj = Instantiate(bossPrefab, (Vector3)spawnTile, Quaternion.identity);
        boss = bossObj.GetComponent<BossController>();
    }

    private void UpdateBossHPBar()
    {
        float normalized = boss.GetNormalizedHealth();
        Debug.Log(normalized.ToString());
        bossHPGauge.fillAmount = normalized;
    }

    private float GetNormalizedBossTimer()
    {
        return timer / bossSpawnTime;
    }
}
