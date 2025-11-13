using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkManager : MonoBehaviour
{
    // Track player and put 3 radiuses around him
    // 1. Largest. This is the available space for mobs to spawn
    // 2. Smallest. This takes a piece around the player so that mobs don't spawn in the players face.
    // 3. Medium. Between 2 and 3 is the space mobs can live. From 3 to 1 is the space mobs will be checked for death

    // If between 2 and 3. look to spawn.

    // When spawning, check these
    // Below mob cap
    // y-height of spawn spot
    // mob has to fit and cant spawn on a place with a tree or anything else.

    // with this, make a simple test of spawn a max of 10 frogs around the player.

    public Transform playerPos;
    public float smallestRadius = 40f;
    public float eventHorizonRadius = 85f;
    public float fullRadius = 120f;

    public GameObject frogPrefab;
    public float frogCount = 0;
    public float maxFrogCount = 10;

    public List<GameObject> spawnedFrogs = new List<GameObject>();
    private float spawnCheckInterval = 1f; // Check for spawning every 1 second
    private float lastSpawnCheckTime = 0f;

    public float despawnChance = 2.47f;

    void Update()
    {
        if (Time.time - lastSpawnCheckTime >= spawnCheckInterval)
        {
            lastSpawnCheckTime = Time.time;
            ManageSpawning();
            ManageDespawning();
        }
    }

    private void ManageSpawning()
    {
        if (spawnedFrogs.Count < maxFrogCount && playerPos != null && frogPrefab != null)
        {
            Vector2 randomCircle = Random.insideUnitSphere * Random.Range(smallestRadius + 1, fullRadius);
            Vector3 potentialSpawnPos = playerPos.position + new Vector3(randomCircle.x, 0f, randomCircle.y);

            if (Vector3.Distance(potentialSpawnPos, playerPos.position) > smallestRadius) {
                GameObject newFrog = Instantiate(frogPrefab, potentialSpawnPos, Quaternion.identity);
                spawnedFrogs.Add(newFrog);
            }
        }
    }

    private void ManageDespawning()
    {
        for (int i = spawnedFrogs.Count - 1; i >= 0; i--)
        {
            if (spawnedFrogs[i] == null)
            {
                spawnedFrogs.RemoveAt(i);
                continue;
            }

            float dist = Vector3.Distance(spawnedFrogs[i].transform.position, playerPos.position);
            if (dist > eventHorizonRadius)
            {
                if (dist > fullRadius)
                {
                    Destroy(spawnedFrogs[i]);
                    spawnedFrogs.RemoveAt(i);
                }

                float chance = Random.Range(0f, 100);
                if (chance <= despawnChance)
                {
                    Destroy(spawnedFrogs[i]);
                    spawnedFrogs.RemoveAt(i);
                }
            }
                
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(playerPos.position, smallestRadius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(playerPos.position, eventHorizonRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(playerPos.position, fullRadius);
    }
}
