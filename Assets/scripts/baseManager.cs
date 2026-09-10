using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Procedurally spawns and recycles the ground the player runs on. Platforms are
/// built from primitives at runtime, so the game does not depend on any serialized
/// prefab assets (the original project's prefabs were binary and could not be
/// safely edited). Each segment is a continuous slab that may contain:
///   * a full-width gap the player must jump over,
///   * a spike strip on one lane the player must dodge or jump over,
///   * a line of collectible coins.
/// Segments ahead of the player are created on demand and recycled behind them.
/// </summary>
public class baseManager : MonoBehaviour
{
    [Header("Track")]
    public float spwnZ = -10f;
    public float safezone = 24f;
    public float baselength = 22f;
    public int amtbase = 7;               // length of the safe starting runway
    public float trackWidth = 11f;        // total playable width (3 lanes)

    [Header("Hazards")]
    public float gapChance = 0.28f;
    public float spikeChance = 0.35f;
    public float minGapWidth = 2.6f;
    public float maxGapWidth = 4.4f;

    [Header("Coins")]
    public float coinChance = 0.6f;

    private playerMotor player;
    private readonly List<GameObject> spawned = new List<GameObject>();
    private float nextZ;
    private int segmentCount;

    // Shared runtime materials (created once, reused).
    private Material groundMat;
    private Material slabMat;
    private Material spikeMat;

    void Awake()
    {
        player = FindObjectOfType<playerMotor>();
        nextZ = spwnZ;
        CreateMaterials();
    }

    void Start()
    {
        BuildStartArea();
    }

    void Update()
    {
        if (player == null)
        {
            player = FindObjectOfType<playerMotor>();
            if (player == null) return;
        }
        if (Game.GameOver) return;

        while (nextZ < player.transform.position.z + safezone)
        {
            SpawnSegment(nextZ);
            nextZ += baselength;
        }

        float behind = player.transform.position.z - baselength * 2f;
        for (int i = spawned.Count - 1; i >= 0; i--)
        {
            GameObject segment = spawned[i];
            if (segment == null)
            {
                spawned.RemoveAt(i);
                continue;
            }
            if (segment.transform.position.z + baselength < behind)
            {
                spawned.RemoveAt(i);
                Destroy(segment);
            }
        }
    }

    void CreateMaterials()
    {
        Shader shader = Shader.Find("Standard");
        if (shader == null) shader = Shader.Find("Diffuse");
        if (shader == null) return;

        if (groundMat == null)
        {
            groundMat = new Material(shader);
            groundMat.color = new Color(0.22f, 0.26f, 0.34f);
        }
        if (slabMat == null)
        {
            slabMat = new Material(shader);
            slabMat.color = new Color(0.30f, 0.36f, 0.46f);
        }
        if (spikeMat == null)
        {
            spikeMat = new Material(shader);
            spikeMat.color = new Color(0.85f, 0.22f, 0.22f);
        }
    }

    /// <summary>Builds a hazard-free runway directly under the player.</summary>
    void BuildStartArea()
    {
        for (int i = 0; i < amtbase; i++)
        {
            SpawnSegment(nextZ, true);
            nextZ += baselength;
        }
    }

    void SpawnSegment(float z, bool safe = false)
    {
        GameObject segment = new GameObject("Segment" + segmentCount++);
        segment.transform.position = new Vector3(0f, 0f, z);
        spawned.Add(segment);

        bool hasGap = false;
        float gapCenter = 0f;
        float gapWidth = 0f;

        if (!safe && Random.value < gapChance)
        {
            hasGap = true;
            gapWidth = Random.Range(minGapWidth, maxGapWidth);
            // Keep the gap away from the segment edges so both landings exist.
            float margin = gapWidth * 0.5f + 2.5f;
            gapCenter = Random.Range(margin, baselength - margin);
        }

        if (!hasGap)
        {
            MakeSlab(segment, new Vector3(0f, 0f, baselength * 0.5f), baselength, groundMat);
        }
        else
        {
            float leftLen = gapCenter - gapWidth * 0.5f;
            float rightStart = gapCenter + gapWidth * 0.5f;
            float rightLen = baselength - rightStart;

            if (leftLen > 1f)
                MakeSlab(segment, new Vector3(0f, 0f, leftLen * 0.5f), leftLen, groundMat);
            if (rightLen > 1f)
                MakeSlab(segment, new Vector3(0f, 0f, rightStart + rightLen * 0.5f), rightLen, groundMat);
        }

        if (!safe && Random.value < spikeChance)
        {
            int lane = Random.Range(0, Game.Lanes);
            float zOffset = Random.Range(4f, baselength - 4f);
            MakeSpikes(segment, lane, zOffset);
        }

        if (!safe && Random.value < coinChance)
        {
            MakeCoins(segment);
        }
    }

    void MakeSlab(GameObject parent, Vector3 localCenter, float length, Material mat)
    {
        GameObject slab = GameObject.CreatePrimitive(PrimitiveType.Cube);
        slab.name = "Slab";
        slab.transform.SetParent(parent.transform, false);
        slab.transform.localPosition = localCenter + new Vector3(0f, -0.3f, 0f); // top at y = 0
        slab.transform.localScale = new Vector3(trackWidth, 0.6f, length);

        Renderer r = slab.GetComponent<Renderer>();
        if (r != null && mat != null) r.material = mat;
    }

    void MakeSpikes(GameObject parent, int lane, float zOffset)
    {
        float x = (lane - 1) * Game.LaneWidth;
        int count = Mathf.RoundToInt(Game.LaneWidth * 0.55f);
        for (int i = 0; i < count; i++)
        {
            float spikeX = x + (i - (count - 1) * 0.5f) * 0.8f;
            GameObject spike = GameObject.CreatePrimitive(PrimitiveType.Cube);
            spike.name = "Spike";
            spike.tag = "Enemy";
            spike.transform.SetParent(parent.transform, false);
            spike.transform.localPosition = new Vector3(spikeX, 0.5f, zOffset);
            spike.transform.localScale = new Vector3(0.6f, 1.0f, 0.6f);
            spike.transform.localRotation = Quaternion.Euler(45f, 45f, 0f);

            Renderer r = spike.GetComponent<Renderer>();
            if (r != null && spikeMat != null) r.material = spikeMat;
        }
    }

    void MakeCoins(GameObject parent)
    {
        GameObject coinPrefab = CoinFactory.CreateCoinPrefab();
        if (coinPrefab == null) return;

        int lane = Random.Range(0, Game.Lanes);
        float x = (lane - 1) * Game.LaneWidth;
        int count = 4;
        for (int i = 0; i < count; i++)
        {
            Vector3 pos = new Vector3(x, 1.1f, 4f + i * 2.2f);
            GameObject coin = (GameObject)Instantiate(coinPrefab);
            coin.transform.SetParent(parent.transform, false);
            coin.transform.localPosition = pos;
            coin.SetActive(true);
        }
    }
}
