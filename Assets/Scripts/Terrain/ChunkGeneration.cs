using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;

public class ChunkGeneration : MonoBehaviour
{
    [Header("Chunk Properties")]
    public Vector2 chunks; // how many x by y chunks
    public Vector2 chunkResolution; // points in each chunk. Ex: x = 4, z = 4. Each chunk will have 16 faces (technically 25 bc the +1 to each).
    private Vector2 chunkMiddle;
    private List<GameObject> chunkList = new List<GameObject>();
    private bool goodSeed = false;

    [Header("Terrain Properties")]
    public Material terrainMaterial;
    // water details
    public GameObject water; // flat plane for water
    public float waterLevel; // y level water should be instantiated at
    // tree details
    public GameObject[] trees; // list of my tree prefabs
    public float treeThreshold;
    // spawner details
    public GameObject spawner;
    public float spawnerThreshold; 
    // ship details
    public GameObject ship;
    public GameObject currentShip;
    public bool shipSpawned = false;
    // mob thresholds
    public float frogSpawnerThreshold;
    public float wolfSpawnerThreshold;

    public GameObject player;
    public GameObject currentPlayer;
    public bool playerSpawned = false;

    public int chunksChunkLoaded;

    public NavMeshSurface[] navMeshes;

    public float landThresholdMin;
    public float landThresholdMax;
    public float totalLandCount = 0;
    public float totalWaterCount = 0;

    public int seed;
    public bool useRandomSeed;

    private void Start()
    {
        navMeshes = GetComponents<NavMeshSurface>();
        StartCoroutine(FindValidSeedThenGenerate());
    }


    private IEnumerator FindValidSeedThenGenerate()
    {
        int attempts = 0;
        while (!goodSeed)
        {
            attempts++;

            if (useRandomSeed)
            {
                seed = Random.Range(0, 1000000);
            }

            chunkMiddle = new Vector2((chunks.x / 2f) * 128, (chunks.y / 2f) * 128);
            waterLevel = Mathf.PerlinNoise(seed, seed) * 15;

            float landCount = 0;
            float waterCount = 0;

            SimulateTerrain(seed, ref landCount, ref waterCount);

            float landRatio = landCount / (landCount + waterCount);

            if (landRatio >= landThresholdMin && landRatio <= landThresholdMax)
            {
                goodSeed = true;
                Debug.Log($"Valid seed found after {attempts} attempts: {seed}, Land ratio: {landRatio}");

                totalLandCount = 0;
                totalWaterCount = 0;

                // Now that we have a good seed, generate the actual chunks
                yield return StartCoroutine(GenerateChunks());
                // Create water
                GameObject current = Instantiate(water,
                    new Vector3(((128 * chunks.x) / 2) - chunkMiddle.x, waterLevel,
                    ((128 * chunks.y) / 2) - chunkMiddle.y),
                    Quaternion.identity
                );
                current.transform.localScale = new Vector3(12.9f, 12.9f, 12.9f) * chunks.x;
                // Build navigation mesh
                //foreach (NavMeshSurface navMesh in navMeshes)
                //{
                //    if (navMesh != null)
                //    {
                //       navMesh.BuildNavMesh();
                //    }
                //}
            }
        }
    }

    private void SimulateTerrain(int testSeed, ref float landCount, ref float waterCount)
    {
        // This function simulates the terrain generation from TerrainGeneration.cs
        // without actually creating GameObjects
        for (int chunkX = 0; chunkX < chunks.x; chunkX++)
        {
            for (int chunkZ = 0; chunkZ < chunks.y; chunkZ++)
            {
                Vector3 chunkPosition = new Vector3(
                    chunkX * (chunkResolution.x) * (128 / chunkResolution.x) - chunkMiddle.x,
                    0,
                    chunkZ * (chunkResolution.y) * (128 / chunkResolution.y) - chunkMiddle.y
                );

                // Simulate each point in the chunk
                for (int x = 0; x <= chunkResolution.x; x++)
                {
                    for (int z = 0; z <= chunkResolution.y; z++)
                    {
                        float y = CalculateHeight(x, z, chunkPosition);

                        if (y > waterLevel)
                        {
                            landCount++;
                        }
                        else
                        {
                            waterCount++;
                        }
                    }
                }
            }
        }
    }

    private float CalculateHeight(float x, float z, Vector3 chunkPosition)
    {
        // This replicates the height calculation from TerrainGeneration.cs
        // Matches the BaseNoise and Noise functions from TerrainGeneration
        Vector2 noiseVector = new Vector2(
            (x * (128 / chunkResolution.x)) + chunkPosition.x + seed,
            (z * (128 / chunkResolution.y)) + chunkPosition.z + seed
        );

        // Calculate base noise (replicates the BaseNoise function)
        float continentalness = SmoothClamp(Mathf.PerlinNoise(noiseVector.x * 0.0005f, noiseVector.y * 0.0005f), 1);
        float peaksAndValleys = Mathf.PerlinNoise(noiseVector.x * 0.003f, noiseVector.y * 0.003f);
        float erosion = Mathf.PerlinNoise(noiseVector.x * 0.001f, noiseVector.y * 0.001f);

        float baseNoise = continentalness + peaksAndValleys - erosion * 1.2f;
        baseNoise = SmoothClamp(baseNoise, 1f);

        // Calculate final height (replicates the Noise function)
        float y = baseNoise * 100f;
        float multiplier = 1 + Mathf.Pow(baseNoise, 5f) * 1.4f;
        y *= multiplier;
        y -= (Mathf.PerlinNoise(noiseVector.x * 0.003f, noiseVector.y * 0.003f) * 5) * baseNoise;

        return y;
    }

    // Simulate terrain generation to count land vs water without creating GameObjects
    private IEnumerator SimulateTerrainCounts()
    {
        int samplesPerBatch = 100;
        int sampleCount = 0;

        // This is a simplified simulation - you'll need to adjust this to match your actual terrain generation logic
        for (int x = 0; x < chunks.x * chunkResolution.x; x++)
        {
            for (int z = 0; z < chunks.y * chunkResolution.y; z++)
            {
                float worldX = x * (128 / chunkResolution.x) - chunkMiddle.x;
                float worldZ = z * (128 / chunkResolution.y) - chunkMiddle.y;

                // Simulate your terrain height calculation
                float height = SimulateHeightAtPosition(worldX, worldZ);

                // Count as land or water based on height vs waterLevel
                if (height > waterLevel)
                {
                    totalLandCount++;
                }
                else
                {
                    totalWaterCount++;
                }

                sampleCount++;
                if (sampleCount >= samplesPerBatch)
                {
                    sampleCount = 0;
                    yield return null; // Prevent freezing during long calculations
                }
            }
        }
    }

    // This method should mimic your actual terrain height calculation
    private float SimulateHeightAtPosition(float x, float z)
    {
        // NOTE: Replace this with your actual terrain height calculation algorithm
        // This is a placeholder that uses Perlin noise similar to what your TerrainGeneration might use
        float height = Mathf.PerlinNoise(
            (x + seed) * 0.01f,
            (z + seed) * 0.01f
        ) * 30;

        return height;
    }

    private float SmoothClamp(float value, float threshold)
    {
        if (value <= threshold)
        {
            return value;
        }
        float excess = value - threshold;
        return threshold + Mathf.Log(1 + excess);
    }

    public IEnumerator GenerateChunks()
    {
        for (int i = 0, x = 0; x < chunks.x; x++)
        {
            for (int z = 0; z < chunks.y; z++)
            {
                GameObject current = new GameObject("Terrain" + " (" + new Vector2(x, z) + ")", 
                    typeof(TerrainGeneration),
                    typeof(MeshRenderer),
                    typeof(MeshFilter),
                    typeof(MeshCollider)
                    );
                current.transform.parent = transform;
                current.transform.position = new Vector3(x * (chunkResolution.x) * (128 / chunkResolution.x) - chunkMiddle.x,
                    0f,
                    z * (chunkResolution.y) * (128 / chunkResolution.y) - chunkMiddle.y);
                chunkList.Add(current);
                i++;
                if (i == chunksChunkLoaded)
                {
                    i = 0;
                    yield return new WaitForSeconds(Time.deltaTime * 2);
                }
            }
        }
    }

    // Destroy Functions
    private void DestroyChunks()
    {
        foreach (GameObject current in chunkList)
        {
            Destroy(current);
        }
        chunkList.Clear();
    } // Destroys All Terrain Chunks
    private void DestroyPlayer()
    {
        if (currentPlayer != null)
        {
            Destroy(currentPlayer);
            playerSpawned = false;
        }
    } // Destroys Player
    private void DestroyShip()
    {
        if (currentShip != null)
        {
            Destroy(currentShip);
            shipSpawned = false;
        }
    } // Destroys The Ship
}
