using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("보스 타이머 UI")]
    [SerializeField] private GameObject bossTimerUI;
    [SerializeField] private Image bossTimerGauge;

    [Space]
    [Header("보스 HP UI")]
    [SerializeField] private GameObject bossHPBar;
    [SerializeField] private Image bossHPGauge;
    [SerializeField] private GameObject bossPrefab;

    public float bossSpawnTime = 30f;

    [Space]
    [Header("행상인 타이머")]
    [SerializeField] private PeddlerSpawnGenerator peddlerSpawnGenerator;
    public float peddlerSpawnTime = 30f;
    public float peddlerSpawnTimer = 0f;
    public int minOffset = 2;
    public int maxOffset = 5;

    [Space]
    [Header("이글루 생성")]
    [SerializeField] private IglooSpawnGenerator iglooSpawnGenerator;
    [SerializeField] private GameObject iglooPrefab;
    [SerializeField] private float iglooSpawnTime = 30f;
    private float iglooSpawnTimer = 0f;
    private Peddler peddler;

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
        if(boss != null && isGeneratedBoss)
        {
            UpdateBossHPBar();
            if (boss.IsDestroyed())
            {
                bossHPBar.SetActive(false);
                boss = null;
                isGeneratedBoss = false;
            }
        }
        if(peddler == null)
        {
            peddlerSpawnTimer += Time.deltaTime;
            if(peddlerSpawnTimer >= peddlerSpawnTime)
            {
                peddlerSpawnTimer = 0f;
                Vector3Int spawnTile = TilemapManager.instance.GetRandomEdgeSpawnPoint(minOffset, maxOffset);
                peddler = Instantiate(peddlerSpawnGenerator.pfPeddler, spawnTile, Quaternion.identity).GetComponent<Peddler>();
                peddler.SetTargetToMove((Vector3)TilemapManager.instance.GetOppositeDestination(spawnTile, minOffset, maxOffset));
            }
        }
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
        boss = Instantiate(bossPrefab, spawnTile, Quaternion.identity).GetComponent<BossController>();
    }

    private void UpdateBossHPBar()
    {
        bossHPGauge.fillAmount = boss.GetNormalizedHealth();
    }

    private float GetNormalizedBossTimer()
    {
        return timer / bossSpawnTime;
    }
}
