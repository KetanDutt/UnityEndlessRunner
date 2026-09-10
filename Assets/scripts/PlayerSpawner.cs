using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Bootstraps the gameplay scene at runtime. The original scenes were serialized
/// as binary and contained no script references, so this component builds every
/// gameplay object programmatically:
///   * the Player (a fresh capsule with a CharacterController),
///   * the follow camera, death plane, directional light and UI event system,
///   * the baseManager (platform spawner), Score and DeathMenu controllers.
/// It is created by GameBootstrap on scene load, so the game works even though
/// the scene files themselves carry no MonoBehaviour wiring.
/// </summary>
public class PlayerSpawner : MonoBehaviour
{
    void Awake()
    {
        Game.ResetRun();
        NeutralizeSceneHazards();
        EnsurePlayer();
        EnsureCamera();
        EnsureDeathPlane();
        EnsureLight();
        EnsureEventSystem();
        EnsureManagers();
        EnsureGameUI();
        StartCountdown();
        ScenePolish.Apply();
    }

    /// <summary>Removes stray colliders tagged "Enemy" that may exist in the
    /// scene (for example a leftover static DeathPlane). The only "Enemy" tags
    /// we keep are the ones baseManager creates at runtime.</summary>
    void NeutralizeSceneHazards()
    {
        Collider[] colliders = FindObjectsOfType<Collider>();
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].CompareTag("Enemy"))
                colliders[i].tag = "Untagged";
        }
    }

    void EnsurePlayer()
    {
        if (FindObjectOfType<playerMotor>() != null) return;

        // Discard any legacy "Player" object from the scene (it has no
        // controller and may carry unknown components) and build a fresh one.
        GameObject legacy = GameObject.Find("Player");
        if (legacy != null) Destroy(legacy);

        GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.name = "Player";
        player.tag = "Player";
        player.transform.position = new Vector3(0f, 1.2f, 0f);
        player.transform.rotation = Quaternion.identity;
        player.transform.localScale = Vector3.one;

        // Give the player a clean, visible material.
        Renderer r = player.GetComponent<Renderer>();
        if (r != null)
        {
            Material mat = new Material(Shader.Find("Standard"));
            if (mat != null)
            {
                mat.color = new Color(0.18f, 0.72f, 1f);
                mat.SetFloat("_Glossiness", 0.4f);
                mat.SetFloat("_Metallic", 0.1f);
                r.material = mat;
            }
        }

        // Remove any colliders that would fight the CharacterController.
        Collider[] existing = player.GetComponents<Collider>();
        for (int i = 0; i < existing.Length; i++)
            Destroy(existing[i]);

        player.AddComponent<playerMotor>();
    }

    void EnsureCamera()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            GameObject go = new GameObject("Main Camera");
            go.tag = "MainCamera";
            cam = go.AddComponent<Camera>();
        }

        cam.transform.position = new Vector3(0f, 4.2f, -7.2f);
        cam.transform.rotation = Quaternion.Euler(12f, 180f, 0f);

        if (cam.GetComponent<cameraMotor>() == null)
            cam.gameObject.AddComponent<cameraMotor>();

        if (cam.GetComponent<AudioListener>() == null)
            cam.gameObject.AddComponent<AudioListener>();
    }

    void EnsureDeathPlane()
    {
        // Remove any legacy DeathPlane GameObject left over from the scene.
        GameObject legacy = GameObject.Find("DeathPlane");
        if (legacy != null) Destroy(legacy);

        if (FindObjectOfType<DeathPlane>() != null) return;

        GameObject go = new GameObject("DeathPlane");
        go.transform.position = new Vector3(0f, -8f, 0f);

        BoxCollider box = go.AddComponent<BoxCollider>();
        box.isTrigger = true;
        box.size = new Vector3(40f, 2f, 200f);

        go.AddComponent<DeathPlane>();
    }

    void EnsureLight()
    {
        if (FindObjectOfType<Light>() != null) return;

        GameObject go = new GameObject("Directional Light");
        Light light = go.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.05f;
        light.color = new Color(1f, 0.98f, 0.94f);
        go.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
    }

    void EnsureEventSystem()
    {
        if (FindObjectOfType<EventSystem>() != null) return;

        GameObject go = new GameObject("EventSystem");
        go.AddComponent<EventSystem>();
        go.AddComponent<StandaloneInputModule>();
    }

    void EnsureManagers()
    {
        if (FindObjectOfType<baseManager>() == null)
            gameObject.AddComponent<baseManager>();

        if (FindObjectOfType<Score>() == null)
            gameObject.AddComponent<Score>();

        if (FindObjectOfType<DeathMenu>() == null)
            gameObject.AddComponent<DeathMenu>();
    }

    void EnsureGameUI()
    {
        if (FindObjectOfType<GameUI>() == null)
            gameObject.AddComponent<GameUI>();
    }

    /// <summary>Freezes the run and plays the 3-2-1 style countdown. The player
    /// settles onto the track during it, then the run starts automatically.</summary>
    void StartCountdown()
    {
        Game.IsRunning = false;

        GameUI ui = FindObjectOfType<GameUI>();
        if (ui != null)
            ui.ShowCountdown(null);
        else
            Game.IsRunning = true;   // Safety net if the UI was somehow missing.
    }
}
