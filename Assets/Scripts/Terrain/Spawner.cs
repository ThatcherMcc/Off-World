using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject[] mobs; // list of mob prefabs it pulls from
    private GameObject mob; // current mob in spawner
    private GameObject currentMobInstance; // To hold the spawned instance
    //private int numMobs; // number of mobs to spawn, usually 1
    private float count = 0; // counter used to wait a few seconds before spawning

    public GameObject player; // player character
    private float distanceToSpawn = 300; // how large the dist between player and spawner needs to be to spawn
    private bool spawned = false; // is current mob spawned

    public float waterLevel;
    public MobType mobType;


    // Start is called before the first frame update
    void Start()
    {
        mob = GetMobByType();
        //Debug.Log(mob);
    }

    private void Update()
    {
        if (player != null)
        {
            float distance = Vector3.Distance(player.transform.position, transform.position);
            if (!spawned && distance < distanceToSpawn && count >= 10)
            {
                SpawnMob();
            } else if (distance > distanceToSpawn + 20 && spawned)
            {
                ClearMob();
            }
            count += Time.deltaTime;
        }
        
    }

    private GameObject GetMobByType()
    {
        float y = transform.position.y;
        if (y < waterLevel + 11 && y > waterLevel + 1)
        {
            //numMobs = 1;
            mobType = MobType.Frog;
            return mobs.Length > 0 ? mobs[0] : null;
        }
        else if (y < waterLevel + 121 && y > waterLevel + 51)
        {
            //numMobs = 1;
            mobType = MobType.Wolf;
            return mobs.Length > 1 ? mobs[1] : null;
        }
        else
        {
            mobType = MobType.Null;
            return null;
        }
    }

    private void SpawnMob()
    { 
        spawned = true;
        currentMobInstance = Instantiate(mob, transform.position, Quaternion.identity);
        if (currentMobInstance.TryGetComponent<IEnemy>(out var currmob))
        {
            currmob.player = player.transform;
        }
    }

    private void ClearMob()
    {
        if (currentMobInstance != null)
        {
            Destroy(currentMobInstance);
            currentMobInstance = null;
        }
        spawned = false;
    }

    public enum MobType
    {
        Frog,
        Wolf,
        Null
    }


    private void OnDrawGizmos()
    {
        if (mobType == MobType.Frog)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, distanceToSpawn);
        } else if (mobType == MobType.Wolf)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, distanceToSpawn);
        } else
        {
            Gizmos.color = Color.black;
            Gizmos.DrawWireSphere(transform.position, distanceToSpawn);
        }
        
    }
}
