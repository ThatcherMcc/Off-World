using System.Collections;
using UnityEngine;
using System.Linq;
using System.Text.RegularExpressions;
using Cinemachine;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;

public class TerrainGeneration : MonoBehaviour
{
    ChunkGeneration chunkGen;
    GameObject player;
    private bool canSpawnPlayer = false;
    private GameObject spawner;
    private bool canSpawnSpawner = true;
    private GameObject ship;
    private bool canSpawnShip = false;
    private float waterLevel;

    private float landCount = 0;
    private float waterCount = 0;
    
    Mesh mesh;
    MeshFilter meshFilter;
    MeshRenderer meshRenderer;
    MeshCollider meshCollider;
    Vector2[] uv;
    

    private void Start()
    {
        chunkGen = GameObject.FindGameObjectWithTag("Manager").GetComponent<ChunkGeneration>();
        if (chunkGen == null )
        {
            return;
        }
        spawner = chunkGen.spawner;
        player = chunkGen.player;
        ship = chunkGen.ship;
        waterLevel = chunkGen.waterLevel;
        

        string name = transform.gameObject.name;
        string[] names = name.Split(',');
        float firstNum = float.Parse(Regex.Replace(names[0], @"[^\d.]", ""));
        //Debug.Log(firstNum);
        float secondNum = float.Parse(Regex.Replace(names[1], @"[^\d.]", ""));
        //Debug.Log(secondNum);
        if (chunkGen.chunks.x / 2f - 3 < firstNum && firstNum < chunkGen.chunks.x / 2f + 3 &&
            chunkGen.chunks.y / 2f - 3 < secondNum && secondNum < chunkGen.chunks.y / 2f + 3)
        {
            canSpawnPlayer = true;
            canSpawnShip = true;
            //Debug.Log("CANSPAWN");
        }

        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        meshCollider = GetComponent<MeshCollider>();

        meshRenderer.material = chunkGen.terrainMaterial;

        GenerateTerrain();
        chunkGen.totalLandCount += landCount;
        chunkGen.totalWaterCount += waterCount;
        Invoke("SetSpawnerPlayer", 6f);
    }

    void GenerateTerrain()
    {
        mesh = new Mesh();
        // each specific point in this chunk
        Vector3[] vertices = new Vector3[(int)((chunkGen.chunkResolution.x + 1) * (chunkGen.chunkResolution.y + 1))];
        // 
        uv = new Vector2[vertices.Length];
        int[] triangles;

        for (int i = 0, x = 0; x <= chunkGen.chunkResolution.x; x++)
        {
            for (int z = 0; z <= chunkGen.chunkResolution.y; z++)
            {
                float y = Noise(x, z, BaseNoise(x, z));
                if (y > waterLevel) {
                    landCount++;
                }
                else {
                    waterCount++;
                }
                vertices[i] = new Vector3(x * (128 / chunkGen.chunkResolution.x),
                    y,
                    z * (128 / chunkGen.chunkResolution.y)
                    );
                float doesSpawn = Mathf.PerlinNoise(x + transform.position.x + chunkGen.seed, z + transform.position.z + chunkGen.seed);
                doesSpawn -= Mathf.PerlinNoise((x + transform.position.x) * 0.5f + chunkGen.seed, (z + transform.position.z) * 0.5f + chunkGen.seed);
                
                // player spawning
                if (canSpawnPlayer && !chunkGen.playerSpawned && y > waterLevel + 15 && y < waterLevel + 40)
                {
                    PlayerSpawn(x, y, z);
                    i++;
                    continue;
                    //Debug.Log("SPAWNED");
                }
                // ship spawning
                if (canSpawnShip && !chunkGen.shipSpawned && chunkGen.playerSpawned && y > waterLevel + 15 && y < waterLevel + 40)
                {
                    ShipSpawn(x, y, z);
                    i++;
                    continue;
                }
                // tree spawning
                if (doesSpawn > chunkGen.treeThreshold && y > waterLevel + 20)
                {
                    TreeSpawn(x, y, z);
                }
                // spawner spawning w/ added noise
                doesSpawn += Mathf.PerlinNoise((x + transform.position.x) * 0.03f + chunkGen.seed, (z + transform.position.z) * 0.03f + chunkGen.seed) * 0.35f;
                // frog spawning
                if (canSpawnSpawner && doesSpawn > chunkGen.frogSpawnerThreshold && y < waterLevel + 10 && y > waterLevel)
                {
                    SpawnerSpawn(x, y, z);
                    //Debug.Log("Spawner made and added");
                }
                // wolf spawning
                if (canSpawnSpawner && doesSpawn > chunkGen.wolfSpawnerThreshold && y < waterLevel + 120 && y > waterLevel + 50)
                {
                    SpawnerSpawn(x, y, z);
                }
                i++;
            }
        }

        for (int i = 0; i < uv.Length; i++)
        {
            uv[i] = new Vector2(vertices[i].x, vertices[i].z);
        }

        triangles = new int[(int)(chunkGen.chunkResolution.x * chunkGen.chunkResolution.y * 6)];

        int tris = 0;
        int vert = 0;

        for (int x = 0; x < chunkGen.chunkResolution.y; x++)
        {
            for ( int z = 0; z < chunkGen.chunkResolution.x; z++)
            {
                triangles[tris + 0] = vert + 0;
                triangles[tris + 1] = (int)(vert + chunkGen.chunkResolution.x + 1);
                triangles[tris + 2] = vert + 1;
                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = (int)(vert + chunkGen.chunkResolution.x + 1);
                triangles[tris + 5] = (int)(vert + chunkGen.chunkResolution.x + 2);

                vert++;
                tris += 6;
            }

            vert++;
        }

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uv;
        mesh.RecalculateBounds();
        mesh.triangles = mesh.triangles.Reverse().ToArray();
        meshFilter.mesh = mesh;
        mesh.RecalculateNormals();
        meshCollider.sharedMesh = mesh;
    }

    float Noise(float x, float z, float baseNoise)
    {
        Vector2 noiseVector = new Vector2((x * (128 / chunkGen.chunkResolution.x)) + transform.position.x + chunkGen.seed,
            (z * (128 / chunkGen.chunkResolution.y)) + transform.position.z + chunkGen.seed);
        // Base Plate of Noise
        float y = baseNoise * 100f;

        // Mountains
        float multiplier = 1 + Mathf.Pow(baseNoise, 5f) * 1.4f;
        y *= multiplier;

        //y += (Mathf.PerlinNoise(noiseVector.x * 0.001f, noiseVector.y * 0.001f) * 10) * BaseNoise;
        // Hills
        y -= (Mathf.PerlinNoise(noiseVector.x * 0.003f, noiseVector.y * 0.003f) * 5) * baseNoise;

        return y;
    }

    float BaseNoise(float x, float z)
    {
        Vector2 noiseVector = new Vector2((x * (128 / chunkGen.chunkResolution.x)) + transform.position.x + chunkGen.seed,
            (z * (128 / chunkGen.chunkResolution.y)) + transform.position.z + chunkGen.seed);
        float continentalness = Continentalness(noiseVector);
        float peaksandvalleys = PeaksAndValleys(noiseVector);
        float erosion = Erosion(noiseVector);
        // Base Plate of Noise. Quite Spread out but not too much
        float y = continentalness;
        y += peaksandvalleys;
        y -= erosion * 1.2f;

        // Adding more noise
        //y += Mathf.PerlinNoise(noiseVector.x * 0.0015f, noiseVector.y * 0.0015f) * 1f;
        // Minusing Noise (Water). Multiply by 2 so it can stack up to the first 2 additions
        //y -= Mathf.PerlinNoise(noiseVector.x * 0.0009f, noiseVector.y * 0.0009f) * 1.5f;
        //y -= Mathf.PerlinNoise(noiseVector.x * 0.004f, noiseVector.y * 0.004f) * 0.5f;
        y = SmoothClamp(y, 1f);
        return y;
        //26213
        //811283
    }
    float Continentalness(Vector2 noiseVector)
    {
        float y = Mathf.PerlinNoise(noiseVector.x * 0.0005f, noiseVector.y * 0.0005f);
        return SmoothClamp(y, 1);
    }
    float PeaksAndValleys(Vector2 noiseVector)
    {
        return Mathf.PerlinNoise(noiseVector.x * 0.003f, noiseVector.y * 0.003f);
    }
    float Erosion(Vector2 noiseVector)
    {
        return Mathf.PerlinNoise(noiseVector.x * 0.001f, noiseVector.y * 0.001f);
    }
    float SmoothClamp(float value, float threshold)
    {
        if (value <= threshold)
        {
            return value;
        }
        float excess = value - threshold;
        return threshold + Mathf.Log(1 + excess);
        //return threshold + (excess / (1 + excess));
    }

    private void PlayerSpawn(float x, float y, float z)
    {
        GameObject currentPlayer = Instantiate(player, new Vector3(x * (128 / chunkGen.chunkResolution.x) + transform.position.x,
                       y + transform.position.y + 5f,
                       z * (128 / chunkGen.chunkResolution.y) + transform.position.z),
                       Quaternion.Euler(0, Random.Range(0, 360), 0));

        Transform orientation = currentPlayer.transform.Find("OrientationCam").transform;
        CinemachineVirtualCamera virtualCam = GameObject.FindGameObjectWithTag("VirtualCamera").GetComponent<CinemachineVirtualCamera>();
        virtualCam.Follow = orientation;
        currentPlayer.GetComponent<PlayerHealth>().healthbar = GameObject.FindGameObjectWithTag("UserInterface").transform.GetChild(1).GetChild(0).GetComponent<Healthbar>();
        chunkGen.playerSpawned = true;
        chunkGen.currentPlayer = currentPlayer;
    }
    private void ShipSpawn(float x, float y, float z)
    {
        GameObject currentShip = Instantiate(ship, new Vector3(x * (128 / chunkGen.chunkResolution.x) + transform.position.x,
                       y + transform.position.y + 5f,
                       z * (128 / chunkGen.chunkResolution.y) + transform.position.z),
                       Quaternion.Euler(0, Random.Range(0, 360), 0));

        currentShip.transform.GetChild(0).GetChild(0).GetComponent<TeleportDoor>().player = player;
        currentShip.transform.GetChild(0).GetChild(1).GetComponent<TeleportDoor>().player = player;
        chunkGen.shipSpawned = true;
        chunkGen.currentShip = currentShip;
    }
    private void TreeSpawn(float x, float y, float z)
    {
        float whatSpawns = Mathf.PerlinNoise(x + transform.position.x + (chunkGen.seed * 5), z + transform.position.z + (chunkGen.seed * 3));
        whatSpawns = whatSpawns * chunkGen.trees.Length;
        whatSpawns = Mathf.RoundToInt(whatSpawns);

        float offset = Random.Range(-2f, 2f);
        offset = offset / 2;

        int whatTree = Random.Range(0, 2);

        GameObject current = Instantiate(chunkGen.trees[whatTree], new Vector3(x * (128 / chunkGen.chunkResolution.x) + transform.position.x + offset,
           y + transform.position.y - 0.85f,
           z * (128 / chunkGen.chunkResolution.y) + transform.position.z + offset),
           Quaternion.Euler(0, Random.Range(0, 360), 0));

        current.transform.parent = transform;
    }
    private void SpawnerSpawn(float x, float y, float z)
    {
        GameObject current = Instantiate(spawner, new Vector3(x * (128 / chunkGen.chunkResolution.x) + transform.position.x,
                       y + transform.position.y + 1f,
                       z * (128 / chunkGen.chunkResolution.y) + transform.position.z),
                       Quaternion.identity);
        current.transform.parent = transform;
        current.GetComponent<Spawner>().waterLevel = waterLevel;
        spawner = current;
        canSpawnSpawner = false;
    }

    private void SetSpawnerPlayer()
    {
        if (player != null)
        {
            Debug.Log("terrain player Not null");
            spawner.GetComponent<Spawner>().player = chunkGen.currentPlayer;
            Debug.Log(spawner.name + " Spawner assigned??");
            
        }
        
    }
    Vector3 GetTerrainNormal(int x, int z)
    {
        // Get the neighboring vertices to calculate the slope
        float yL = Noise(x - 4, z, BaseNoise(x - 4, z));
        float yR = Noise(x + 4, z, BaseNoise(x + 4, z));
        float yD = Noise(x, z - 4, BaseNoise(x, z - 4));
        float yU = Noise(x, z + 4, BaseNoise(x, z + 4));

        // Calculate vectors from the neighbors
        Vector3 leftRight = new Vector3(2, yR - yL, 0);  // Left to Right vector
        Vector3 downUp = new Vector3(0, yU - yD, 2);     // Down to Up vector

        // Compute the cross product to get the normal
        Vector3 normal = Vector3.Cross(leftRight, downUp).normalized;

        return normal;
    }

}
