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

        // Clean up the default collider: we use a trigger for pickup.
        Collider c = coin.GetComponent<Collider>();
        if (c != null)
        {
            c.isTrigger = true;
        }

        Material mat = new Material(Shader.Find("Standard"));
        if (mat != null)
        {
            mat.color = new Color(1f, 0.85f, 0.25f);
            mat.SetFloat("_Glossiness", 0.85f);
            mat.SetFloat("_Metallic", 0.6f);
        }
        Renderer r = coin.GetComponent<Renderer>();
        if (r != null) r.material = mat;

        coin.tag = "Coin";
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

    public static readonly System.Collections.Generic.List<CoinSpin> Active = new System.Collections.Generic.List<CoinSpin>();

    private Vector3 basePosition;
    private bool registered;

    void OnEnable()
    {
        basePosition = transform.position;
        if (!registered)
        {
            Active.Add(this);
            registered = true;
        }
    }

    void OnDisable()
    {
        if (registered)
        {
            Active.Remove(this);
            registered = false;
        }
    }

    void OnDestroy()
    {
        if (registered)
        {
            Active.Remove(this);
            registered = false;
        }
    }

    void Update()
    {
        transform.Rotate(0f, spinSpeed * Time.deltaTime, 0f, Space.World);
        Vector3 pos = basePosition;
        pos.y += Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = pos;
    }

    public void Collect()
    {
        Destroy(gameObject);
    }
}
