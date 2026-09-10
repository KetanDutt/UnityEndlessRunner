using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Builds a reusable spinning coin prefab entirely from primitives, so coins
/// work without any imported art assets.
/// </summary>
public static class CoinFactory
{
    private static GameObject cachedPrefab;

    public static GameObject CreateCoinPrefab()
    {
        if (cachedPrefab != null) return cachedPrefab;

        GameObject coin = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        coin.name = "Coin";
        coin.transform.localScale = new Vector3(0.7f, 0.06f, 0.7f);

        // Clean up the default collider: we use proximity for pickup and a
        // collider here would only get in the way of the player.
        Collider c = coin.GetComponent<Collider>();
        if (c != null)
            c.isTrigger = true;

        Material mat = new Material(Shader.Find("Standard"));
        if (mat != null)
        {
            mat.color = new Color(1f, 0.85f, 0.25f);
            mat.SetFloat("_Glossiness", 0.85f);
            mat.SetFloat("_Metallic", 0.6f);
            // Subtle emissive glow so coins read well against the track.
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", new Color(1f, 0.7f, 0.15f, 1f) * 0.35f);
        }
        Renderer r = coin.GetComponent<Renderer>();
        if (r != null) r.material = mat;

        // NOTE: no tag is assigned here. The "Coin" tag does not exist in the
        // project's (binary) TagManager, and assigning an undefined tag throws
        // a UnityException at runtime. Coins are tracked via CoinSpin.Active.
        coin.AddComponent<CoinSpin>();

        // Keep the prefab out of the runtime scene until instantiated.
        coin.SetActive(false);

        cachedPrefab = coin;
        return cachedPrefab;
    }
}

/// <summary>
/// Simple endless spinning animation for coins. Coins register themselves in a
/// static list so the player can pick them up with a cheap proximity check —
/// more reliable than trigger callbacks with a CharacterController.
/// </summary>
public class CoinSpin : MonoBehaviour
{
    public float spinSpeed = 180f;
    public float bobHeight = 0.15f;
    public float bobSpeed = 3f;
    public float collectRadius = 1.1f;

    public static readonly List<CoinSpin> Active = new List<CoinSpin>();

    private Vector3 basePosition;
    private float phaseOffset;
    private bool registered;
    private bool collected;

    void OnEnable()
    {
        basePosition = transform.position;
        phaseOffset = Random.Range(0f, Mathf.PI * 2f);
        if (!registered)
        {
            Active.Add(this);
            registered = true;
        }
    }

    void OnDisable()
    {
        RemoveFromActive();
    }

    void OnDestroy()
    {
        RemoveFromActive();
    }

    void RemoveFromActive()
    {
        if (registered)
        {
            Active.Remove(this);
            registered = false;
        }
    }

    void Update()
    {
        if (collected) return;

        transform.Rotate(0f, spinSpeed * Time.deltaTime, 0f, Space.World);

        Vector3 pos = basePosition;
        pos.y += Mathf.Sin(Time.time * bobSpeed + phaseOffset) * bobHeight;

        // Magnet: when the player has an active magnet, pull the coin in.
        if (Game.MagnetTime > 0f)
        {
            Transform player = playerMotor.Player;
            if (player != null)
            {
                Vector3 toPlayer = player.position - pos;
                float dist = toPlayer.magnitude;
                if (dist < 7f && dist > 0.001f)
                {
                    float pull = 16f * (1f - dist / 7f) + 6f;
                    pos += toPlayer.normalized * pull * Time.deltaTime;
                }
            }
        }

        transform.position = pos;
    }

    public void Collect()
    {
        if (collected) return;
        collected = true;
        RemoveFromActive();
        StartCoroutine(Pop());
    }

    private System.Collections.IEnumerator Pop()
    {
        Vector3 start = transform.localScale;
        float t = 0f;
        const float duration = 0.12f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            transform.localScale = start * (1f - Easing.BackIn(Mathf.Clamp01(t)));
            yield return null;
        }
        Destroy(gameObject);
    }
}
