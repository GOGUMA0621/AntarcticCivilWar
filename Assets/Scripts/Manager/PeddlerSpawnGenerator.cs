using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PeddlerSpawnGenerator : MonoBehaviour
{
    public GameObject pfPeddler;
    [SerializeField] int minOffset;
    [SerializeField] int maxOffset;

    [SerializeField] int spawnRate;

    private Peddler peddler;
    private Vector3 destinationPoint;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (peddler != null)
        {
            float peddlerDestiantionDistance = Vector3.Distance(peddler.transform.position, destinationPoint);
            if (peddlerDestiantionDistance <= .5f)
            {
                Destroy(peddler.gameObject);
            }
        }
    }

    IEnumerator spawnPeddler()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForSeconds(spawnRate);
        Vector3Int spawnTile = TilemapManager.instance.GetRandomEdgeSpawnPoint(minOffset, maxOffset);
        Vector3Int destinationTile = TilemapManager.instance.GetOppositeDestination(spawnTile,minOffset, maxOffset);

        peddler = Instantiate(pfPeddler, spawnTile, Quaternion.identity).GetComponent<Peddler>();
        yield return new WaitForSeconds(0.1f);
        peddler.SetTargetToMove((Vector3)destinationTile);
    }
}
