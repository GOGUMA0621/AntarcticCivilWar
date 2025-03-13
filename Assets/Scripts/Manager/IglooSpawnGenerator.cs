using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IglooSpawnGenerator : MonoBehaviour
{
    public GameObject pfIgloo;

    [SerializeField] private float iglooRange;
    [SerializeField] private float iglooSpawnRate;

    public static List<GameObject> igloos = new List<GameObject>();
    private bool isReadyToSpawn = true;

    private void Start()
    {
    }

    private void Update()
    {
        if (isReadyToSpawn && igloos.Count <= 4)
        {
            StartCoroutine(SpawnIgloo());
        }
    }

    IEnumerator SpawnIgloo()
    {
        isReadyToSpawn = false;
        Vector3 iglooSpawnPosition = SpawnIglooPosition(iglooRange);
        if (iglooSpawnPosition != Vector3.zero)
        {
            Debug.Log("이글루 소환중");
            yield return new WaitForSeconds(iglooSpawnRate);
            GameObject igloo = Instantiate(pfIgloo, SpawnIglooPosition(iglooRange), Quaternion.identity);
            igloos.Add(igloo);
            isReadyToSpawn = true;
        }

        Debug.Log($"{isReadyToSpawn}, {igloos.Count}");
    }

    private Vector3 SpawnIglooPosition(float radius)
    {
        Vector3 positionToSpawnIgloo = TilemapManager.instance.GetRandomSpawnPoint();
        if(Physics2D.OverlapCircle(positionToSpawnIgloo, radius))
        {
            SpawnIglooPosition(radius);
        }

        return positionToSpawnIgloo;
    }
}
