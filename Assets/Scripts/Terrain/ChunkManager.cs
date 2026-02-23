using System.Collections.Generic;
using UnityEngine;

public class ChunkManager : MonoBehaviour
{
    // Track player and put 3 radiuses around him
    // 1. Largest. This is the available space for mobs to spawn
    // 2. Smallest. This takes a piece around the player so that mobs don't spawn in the players face.
    // 3. Medium. Between 2 and 3 is the space mobs can live. From 3 to 1 is the space mobs will be checked for death

    [System.Serializable]
    public class SpawnEntry
    {
        [Tooltip("Label for this entry (e.g. \"Wolf\", \"Bug\") — for your reference only.")]
        public string name;
        [Tooltip("Creature prefab to spawn (e.g. Wolf, JumpingAlien, Bug).")]
        public GameObject prefab;
        [Tooltip("Relative spawn chance within this table. Higher = more likely. E.g. Wolf 2, Bug 1 → Wolf twice as likely.")]
        public float weight = 1f;
        [Tooltip("Max number of this creature type alive at once. Despawned ones free a slot.")]
        public int maxCount = 20;
    }

    [System.Serializable]
    public class SpawnTable
    {
        [Tooltip("Label for this table (e.g. \"Lowland\", \"Hills\") — for your reference only.")]
        public string tableName;

        [Tooltip("Spawn only at world Y >= this. Land is above water (~15–100+); use e.g. 20 for \"on land\".")]
        public float minHeight;
        [Tooltip("Spawn only at world Y <= this. Use a high value (e.g. 500) for no upper limit.")]
        public float maxHeight;

        [Tooltip("Spawn only at least this far from the player.")]
        public float minDistanceFromPlayer = 0f;
        [Tooltip("Spawn only at most this far from the player. Use 9999 for no limit.")]
        public float maxDistanceFromPlayer = 9999f;

        [Tooltip("Creatures in this table. One is chosen by weight when a spawn point matches this table.")]
        public List<SpawnEntry> entries = new List<SpawnEntry>();
    }

    [Header("Player & Rings")]
    [Tooltip("Set automatically when ChunkGeneration spawns the player; or assign in Inspector for non-procedural scenes.")]
    public Transform playerPos;
    public float smallestRadius = 40f;       // no-spawn bubble around player
    public float eventHorizonRadius = 85f;   // living ring outer edge / start despawn band
    public float fullRadius = 120f;          // beyond this: hard despawn

    [Header("Ship (no-spawn zone)")]
    [Tooltip("Set automatically when ChunkGeneration spawns the ship; or assign in Inspector.")]
    public Transform shipPos;
    public float shipNoSpawnRadius = 30f;   // nothing spawns within this distance of the ship

    [Header("Spawning")]
    [Tooltip("Max total creatures (all types) alive at once.")]
    public int globalMobCap = 40;
    [Tooltip("Seconds between spawn/despawn checks.")]
    public float spawnCheckInterval = 1f;
    [Tooltip("Spawn attempts per check (more = faster refill, slightly more CPU).")]
    public int spawnAttemptsPerTick = 3;
    [Tooltip("Layers to raycast for ground (terrain). Set to your terrain layer so spawns land on ground.")]
    public LayerMask groundLayerMask = ~0;

    [Header("Despawning")]
    [Range(0f, 100f)]
    public float despawnChance = 2.5f;       // % per check for mobs beyond eventHorizon

    [Header("Spawn Tables")]
    public List<SpawnTable> spawnTables = new List<SpawnTable>();

    private float _lastSpawnCheckTime;

    // All spawned mobs (any type)
    private readonly List<GameObject> _spawnedMobs = new List<GameObject>();

    // Counts per prefab (for per-type caps)
    private readonly Dictionary<GameObject, int> _spawnedCountsByPrefab = new Dictionary<GameObject, int>();

    // Map instance -> prefab so we can decrement counts on despawn
    private readonly Dictionary<GameObject, GameObject> _prefabByInstance = new Dictionary<GameObject, GameObject>();

    /// <summary>Called by ChunkGeneration once the player has spawned. No per-frame lookup.</summary>
    public void SetPlayer(Transform player)
    {
        playerPos = player;
    }

    /// <summary>Called by ChunkGeneration once the ship has spawned. Mobs will not spawn within shipNoSpawnRadius of the ship.</summary>
    public void SetShip(Transform ship)
    {
        shipPos = ship;
    }

    private void Update()
    {
        if (playerPos == null)
            return;

        if (Time.time - _lastSpawnCheckTime >= spawnCheckInterval)
        {
            _lastSpawnCheckTime = Time.time;
            ManageSpawning();
            ManageDespawning();
        }
    }

    private void ManageSpawning()
    {
        if (_spawnedMobs.Count >= globalMobCap)
            return;

        for (int i = 0; i < spawnAttemptsPerTick; i++)
        {
            if (!TryGetRandomSpawnPosition(out var spawnPos))
                continue;

            var table = GetTableForPosition(spawnPos);
            if (table == null)
                continue;

            var prefab = ChoosePrefabFromTable(table);
            if (prefab == null)
                continue;

            SpawnMob(prefab, spawnPos);

            if (_spawnedMobs.Count >= globalMobCap)
                break;
        }
    }

    private bool TryGetRandomSpawnPosition(out Vector3 spawnPos)
    {
        spawnPos = Vector3.zero;

        // pick a random point in the living ring (between inner and event horizon)
        float radius = Random.Range(smallestRadius + 1f, eventHorizonRadius);
        Vector2 offset2D = Random.insideUnitCircle.normalized * radius;

        // start ray from above, roughly above player height
        Vector3 rayOrigin = playerPos.position + new Vector3(offset2D.x, 50f, offset2D.y);

        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, 200f, groundLayerMask))
        {
            // ensure final point is still outside the no-spawn bubble just in case
            if (Vector3.Distance(hit.point, playerPos.position) <= smallestRadius)
                return false;

            // reject if within ship no-spawn radius
            if (shipPos != null && Vector3.Distance(hit.point, shipPos.position) <= shipNoSpawnRadius)
                return false;

            spawnPos = hit.point;
            return true;
        }

        return false;
    }

    private SpawnTable GetTableForPosition(Vector3 pos)
    {
        float height = pos.y;
        float dist = Vector3.Distance(pos, playerPos.position);

        foreach (var table in spawnTables)
        {
            if (height >= table.minHeight && height <= table.maxHeight &&
                dist >= table.minDistanceFromPlayer && dist <= table.maxDistanceFromPlayer)
            {
                return table;
            }
        }

        return null;
    }

    private GameObject ChoosePrefabFromTable(SpawnTable table)
    {
        List<SpawnEntry> valid = new List<SpawnEntry>();
        float totalWeight = 0f;

        foreach (var entry in table.entries)
        {
            if (entry.prefab == null || entry.weight <= 0f || entry.maxCount <= 0)
                continue;

            int currentCount = _spawnedCountsByPrefab.TryGetValue(entry.prefab, out int c) ? c : 0;
            if (currentCount >= entry.maxCount)
                continue;

            valid.Add(entry);
            totalWeight += entry.weight;
        }

        if (valid.Count == 0 || totalWeight <= 0f)
            return null;

        float r = Random.value * totalWeight;

        foreach (var e in valid)
        {
            if (r < e.weight)
                return e.prefab;

            r -= e.weight;
        }

        // fallback
        return valid[valid.Count - 1].prefab;
    }

    private void SpawnMob(GameObject prefab, Vector3 position)
    {
        GameObject instance = Instantiate(prefab, position, Quaternion.identity);

        _spawnedMobs.Add(instance);

        if (!_spawnedCountsByPrefab.ContainsKey(prefab))
            _spawnedCountsByPrefab[prefab] = 0;
        _spawnedCountsByPrefab[prefab]++;

        _prefabByInstance[instance] = prefab;

        // Optional: if your enemies implement IEnemy, wire the player
        if (instance.TryGetComponent<IEnemy>(out var enemy))
        {
            enemy.player = playerPos;
        }
    }

    private void ManageDespawning()
    {
        for (int i = _spawnedMobs.Count - 1; i >= 0; i--)
        {
            var mob = _spawnedMobs[i];

            if (mob == null)
            {
                RemoveMobAtIndex(i, destroy: false);
                continue;
            }

            float dist = Vector3.Distance(mob.transform.position, playerPos.position);

            if (dist > fullRadius)
            {
                RemoveMobAtIndex(i, destroy: true);
            }
            else if (dist > eventHorizonRadius)
            {
                float chance = Random.Range(0f, 100f);
                if (chance <= despawnChance)
                {
                    RemoveMobAtIndex(i, destroy: true);
                }
            }
        }
    }

    private void RemoveMobAtIndex(int index, bool destroy)
    {
        GameObject mob = _spawnedMobs[index];

        if (mob != null)
        {
            if (_prefabByInstance.TryGetValue(mob, out var prefab))
            {
                if (_spawnedCountsByPrefab.ContainsKey(prefab))
                {
                    _spawnedCountsByPrefab[prefab] = Mathf.Max(0, _spawnedCountsByPrefab[prefab] - 1);
                }
                _prefabByInstance.Remove(mob);
            }

            if (destroy)
                Destroy(mob);
        }

        _spawnedMobs.RemoveAt(index);
    }

    private void OnDrawGizmos()
    {
        if (playerPos != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(playerPos.position, smallestRadius);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(playerPos.position, eventHorizonRadius);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(playerPos.position, fullRadius);
        }

        if (shipPos != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(shipPos.position, shipNoSpawnRadius);
        }
    }
}