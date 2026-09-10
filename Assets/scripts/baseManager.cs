using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Procedurally spawns and recycles the ground the player runs on. Platforms are
/// built from primitives at runtime, so the game does not depend on any
/// serialized prefab assets (the original project's prefabs were binary and
/// could not be safely edited). Each segment is a continuous slab that may
/// contain a full-width gap, a spike strip on one lane, collectible coins and
/// the occasional magnet power-up. Hazards ramp up gradually with distance.
/// </summary>
public class baseManager : MonoBehaviour
{
    [Header("Track")]
    public float spwnZ = -10f;
    public float safezone = 26f;
    public float baselength = 22f;
    public int amtbase = 3;               // length of the safe starting runway
    public float trackWidth = 11f;        // total playable width (3 lanes)

    [Header("Hazards")]
    public float gapChance = 0.22f;
    public float spikeChance = 0.25f;
    public float minGapWidth = 2.6f;
    public float maxGapWidth = 4.4f;
    public float gapChanceMax = 0.42f;
    public float spikeChanceMax = 0.5f;
    public float rampDistance = 600f;     // meters over which difficulty ramps up

    [Header("Pickups")]
    public float coinChance = 0.6f;
    public float powerupChance = 0.10f;

    private playerMotor player;
    private readonly List<GameObject> spawned = new List<GameObject>();
    private float nextZ;
    private int segmentCount;

    // Shared runtime materials (created once, reused).
    private Material groundMatA;
    private Material groundMatB;
    private Material dividerMat;
    private Material railMat;
    private Material spikeMat;
    private Material powerupMat;

    void Awake()
    {
        player = FindObjectOfType<playerMotor>();
        nextZ = spwnZ;
        CreateMaterials();
    }

    void Start()
    {
        BuildStartArea();
        BuildStartGate();
    }

    void Update()
    {
        if (player == null)
        {
            player = FindObjectOfType<playerMotor>();
            if (player == null) return;
        }
        if (Game.GameOver || Game.IsPaused) return;

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

        groundMatA = MakeMaterial(shader, new Color(0.20f, 0.24f, 0.32f), Color.black, 0f);
        groundMatB = MakeMaterial(shader, new Color(0.26f, 0.30f, 0.40f), Color.black, 0f);
        dividerMat = MakeMaterial(shader, new Color(0.55f, 0.75f, 1f), new Color(0.4f, 0.7f, 1f), 0.5f);
        railMat = MakeMaterial(shader, new Color(0.30f, 0.55f, 0.95f), new Color(0.2f, 0.5f, 1f), 0.7f);
        spikeMat = MakeMaterial(shader, new Color(0.85f, 0.22f, 0.22f), new Color(1f, 0.25f, 0.15f), 0.5f);
        powerupMat = MakeMaterial(shader, new Color(0.2f, 0.8f, 1f), new Color(0.1f, 0.8f, 1f), 1.4f);
    }

    static Material MakeMaterial(Shader shader, Color color, Color emission, float emissionStrength)
    {
        Material mat = new Material(shader);
        mat.color = color;
        mat.SetFloat("_Glossiness", 0.35f);
        mat.SetFloat("_Metallic", 0.05f);
        if (emissionStrength > 0f)
        {
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", emission * emissionStrength);
        }
        return mat;
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

    void BuildStartGate()
    {
        Material mat = railMat;
        GameObject gate = new GameObject("StartGate");

        MakePillar(gate.transform, new Vector3(-2.6f, 0f, 0f), mat);
        MakePillar(gate.transform, new Vector3(2.6f, 0f, 0f), mat);

        GameObject bar = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bar.name = "GateBar";
        RemoveCollider(bar);
        bar.transform.SetParent(gate.transform, false);
        bar.transform.localPosition = new Vector3(0f, 3.2f, 0f);
        bar.transform.localScale = new Vector3(5.6f, 0.25f, 0.25f);
        Renderer br = bar.GetComponent<Renderer>();
        if (br != null) br.material = mat;
    }

    void MakePillar(Transform parent, Vector3 pos, Material mat)
    {
        GameObject pillar = GameObject.CreatePrimitive(PrimitiveType.Cube);
        pillar.name = "GatePillar";
        RemoveCollider(pillar);
        pillar.transform.SetParent(parent, false);
        pillar.transform.localPosition = pos;
        pillar.transform.localScale = new Vector3(0.3f, 3.2f, 0.3f);
        Renderer r = pillar.GetComponent<Renderer>();
        if (r != null) r.material = mat;
    }

    static void RemoveCollider(GameObject go)
    {
        Collider c = go.GetComponent<Collider>();
        if (c != null) Destroy(c);
    }

    void SpawnSegment(float z, bool safe = false)
    {
        GameObject segment = new GameObject("Segment" + segmentCount++);
        segment.transform.position = new Vector3(0f, 0f, z);
        spawned.Add(segment);

        float difficulty = Mathf.Clamp01(Mathf.Max(0f, z) / rampDistance);
        float gapP = Mathf.Lerp(gapChance, gapChanceMax, difficulty);
        float spikeP = Mathf.Lerp(spikeChance, spikeChanceMax, difficulty);

        bool hasGap = false;
        float gapCenter = 0f;
        float gapWidth = 0f;

        if (!safe && Random.value < gapP)
        {
            hasGap = true;
            gapWidth = Random.Range(minGapWidth, maxGapWidth);
            // Keep the gap away from the segment edges so both landings exist.
            float margin = gapWidth * 0.5f + 2.5f;
            gapCenter = Random.Range(margin, baselength - margin);
        }

        int colorIndex = segmentCount % 2;

        if (!hasGap)
        {
            MakeSlab(segment, new Vector3(0f, 0f, baselength * 0.5f), baselength, colorIndex);
        }
        else
        {
            float leftLen = gapCenter - gapWidth * 0.5f;
            float rightStart = gapCenter + gapWidth * 0.5f;
            float rightLen = baselength - rightStart;

            if (leftLen > 1f)
                MakeSlab(segment, new Vector3(0f, 0f, leftLen * 0.5f), leftLen, colorIndex);
            if (rightLen > 1f)
                MakeSlab(segment, new Vector3(0f, 0f, rightStart + rightLen * 0.5f), rightLen, colorIndex);
        }

        int spikeLane = -1;
        if (!safe && !hasGap && Random.value < spikeP)
        {
            spikeLane = Random.Range(0, Game.Lanes);
            float zOffset = Random.Range(4f, baselength - 4f);
            MakeSpikes(segment, spikeLane, zOffset);
        }

        if (!safe && !hasGap && Random.value < coinChance)
        {
            int lane = Random.Range(0, Game.Lanes);
            if (lane == spikeLane) lane = (lane + 1) % Game.Lanes;   // don't hide coins inside spikes
            MakeCoins(segment, lane);
        }

        if (!safe && !hasGap && Random.value < powerupChance)
        {
            int lane = Random.Range(0, Game.Lanes);
            if (lane == spikeLane) lane = (lane + 1) % Game.Lanes;
            MakePowerup(segment, lane);
        }
    }

    void MakeSlab(GameObject parent, Vector3 localCenter, float length, int colorIndex)
    {
        GameObject slab = GameObject.CreatePrimitive(PrimitiveType.Cube);
        slab.name = "Slab";
        slab.transform.SetParent(parent.transform, false);
        slab.transform.localPosition = localCenter + new Vector3(0f, -0.3f, 0f); // top at y = 0
        slab.transform.localScale = new Vector3(trackWidth, 0.6f, length);

        Renderer r = slab.GetComponent<Renderer>();
        Material mat = colorIndex == 0 ? groundMatA : groundMatB;
        if (r != null && mat != null) r.material = mat;

        // Lane dividers (visual only).
        MakeStrip(slab, new Vector3(-Game.LaneWidth * 0.5f, 0.31f, 0f), new Vector3(0.08f, 0.02f, length), dividerMat);
        MakeStrip(slab, new Vector3(Game.LaneWidth * 0.5f, 0.31f, 0f), new Vector3(0.08f, 0.02f, length), dividerMat);

        // Edge rails (visual only).
        float railX = trackWidth * 0.5f - 0.3f;
        MakeStrip(slab, new Vector3(-railX, 0.475f, 0f), new Vector3(0.25f, 0.35f, length), railMat);
        MakeStrip(slab, new Vector3(railX, 0.475f, 0f), new Vector3(0.25f, 0.35f, length), railMat);
    }

    void MakeStrip(GameObject parent, Vector3 localPos, Vector3 localScale, Material mat)
    {
        GameObject strip = GameObject.CreatePrimitive(PrimitiveType.Cube);
        strip.name = "Strip";
        RemoveCollider(strip);
        strip.transform.SetParent(parent.transform, false);
        strip.transform.localPosition = localPos;
        strip.transform.localScale = localScale;
        Renderer r = strip.GetComponent<Renderer>();
        if (r != null && mat != null) r.material = mat;
    }

    void MakeSpikes(GameObject parent, int lane, float zOffset)
    {
        float x = (lane - 1) * Game.LaneWidth;
        int count = Mathf.Max(2, Mathf.CeilToInt(Game.LaneWidth * 0.6f));
        for (int i = 0; i < count; i++)
        {
            float spikeX = x + (i - (count - 1) * 0.5f) * 0.9f;
            GameObject spike = GameObject.CreatePrimitive(PrimitiveType.Cube);
            spike.name = "Spike";
            spike.tag = "Enemy";
            spike.transform.SetParent(parent.transform, false);
            spike.transform.localPosition = new Vector3(spikeX, 0.5f, zOffset);
            spike.transform.localScale = new Vector3(0.55f, 1.0f, 0.55f);
            spike.transform.localRotation = Quaternion.Euler(45f, 45f, 0f);

            Renderer r = spike.GetComponent<Renderer>();
            if (r != null && spikeMat != null) r.material = spikeMat;
        }
    }

    void MakeCoins(GameObject parent, int lane)
    {
        GameObject coinPrefab = CoinFactory.CreateCoinPrefab();
        if (coinPrefab == null) return;

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

    void MakePowerup(GameObject parent, int lane)
    {
        float x = (lane - 1) * Game.LaneWidth;
        float z = Random.Range(6f, baselength - 6f);

        GameObject orb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        orb.name = "Magnet";
        RemoveCollider(orb);
        orb.transform.SetParent(parent.transform, false);
        orb.transform.localPosition = new Vector3(x, 1.1f, z);
        orb.transform.localScale = Vector3.one * 0.55f;

        Renderer r = orb.GetComponent<Renderer>();
        if (r != null && powerupMat != null) r.material = powerupMat;

        orb.AddComponent<Powerup>();
    }
}
