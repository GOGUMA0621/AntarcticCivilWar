using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;
using static UnityEngine.UI.CanvasScaler;

public class Fabric : MonoBehaviour
{
    [Header("Prefab to Spawn")]
    [SerializeField] private GameObject[] prefabs;

    [Header("Level")]
    [SerializeField] private float Level;
    float Make_Mob = 0;

    public TextMeshPro textIgloo;
    private Unit _unit;
    PlayerController playerController;
    UnitController unitcontroller;

    void Start()
    {
        textIgloo.enabled = false;
        _unit = GetComponent<Unit>();

        playerController = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
        unitcontroller = GetComponent<UnitController>();
    }

    // Update is called once per frame
    void Update()
    {
        float distanceToPlayer = Vector3.Distance(this.transform.position, playerController.playerPos);

        if (distanceToPlayer <= 5)
        {
            ClosePlayer();
            if (Input.GetKeyDown(KeyCode.F))
            {
                this.tag = "A.Fabric";
            }
        }
        else
        {
            FarPlayer();
        }

        if (unitcontroller.isUnitDie == true)
        {
            IsBreak();

        }

    }



    private void ClosePlayer()
    {
        textIgloo.enabled = true;
    }

    private void FarPlayer()
    {
        textIgloo.enabled = false;
    }

    private void IsBreak()
    {
        if (Make_Mob == Level)
        {
            gameObject.SetActive(false);
        }
        if (Make_Mob <= Level)
        {
            SpawnRandomPrefab(transform.position, Quaternion.identity);
            Make_Mob = Make_Mob + 1;
        }
    }
    public void SpawnRandomPrefab(Vector3 position, Quaternion rotation)
    {
        if (prefabs != null && prefabs.Length > 0)
        {
            
            int randomIndex = UnityEngine.Random.Range(0, prefabs.Length);

            GameObject prefabToSpawn = prefabs[randomIndex];

            if (prefabToSpawn != null)
            {
                Instantiate(prefabToSpawn, position, rotation);
            }
        }
    }
}
