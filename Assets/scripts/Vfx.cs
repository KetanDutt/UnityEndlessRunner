using UnityEngine;

/// <summary>
/// Runtime particle effects. Everything is built procedurally so the game needs
/// no external VFX assets and stays self-contained.
/// </summary>
public static class Vfx
{
    private static Material particleMaterial;
    private static bool materialLookedUp;

    private static Material ParticleMaterial
    {
        get
        {
            if (!materialLookedUp)
            {
                materialLookedUp = true;
                Shader shader = Shader.Find("Particles/Additive");
                if (shader == null) shader = Shader.Find("Sprites/Default");
                if (shader != null) particleMaterial = new Material(shader);
            }
            return particleMaterial;
        }
    }

    /// <summary>Emits a single short burst of particles at a world position.</summary>
    public static void Burst(Vector3 position, Color color, int count, float speed,
                             float size, float life, float gravity = 0f)
    {
        GameObject host = new GameObject("VFX");
        host.transform.position = position;

        ParticleSystem ps = host.AddComponent<ParticleSystem>();
        ParticleSystem.MainModule main = ps.main;
        main.playOnAwake = false;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = count;
        main.startLifetime = life;
        main.startSpeed = speed;
        main.startSize = size;
        main.startColor = color;
        main.gravityModifier = gravity;

        ParticleSystem.EmissionModule emission = ps.emission;
        emission.rateOverTime = 0f;

        // Slight random spread.
        ParticleSystem.ShapeModule shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.3f;

        ParticleSystemRenderer renderer = host.GetComponent<ParticleSystemRenderer>();
        if (renderer != null && ParticleMaterial != null)
            renderer.material = ParticleMaterial;

        ps.Emit(count);
        Object.Destroy(host, life + 0.6f);
    }

    public static void JumpDust(Vector3 position)
    {
        Burst(position + Vector3.down * 0.8f, new Color(0.9f, 0.9f, 0.95f, 0.9f), 10, 3f, 0.25f, 0.45f);
    }

    public static void LandDust(Vector3 position)
    {
        Burst(position + Vector3.down * 0.9f, new Color(0.85f, 0.85f, 0.9f, 0.85f), 16, 4.2f, 0.3f, 0.5f);
        Burst(position + Vector3.down * 0.8f, new Color(0.5f, 0.55f, 0.65f, 0.7f), 8, 2f, 0.4f, 0.4f, 1.5f);
    }

    public static void CoinSparkle(Vector3 position)
    {
        Burst(position, new Color(1f, 0.85f, 0.25f, 1f), 14, 3.5f, 0.18f, 0.55f, 0.5f);
    }

    public static void LevelUpBurst(Vector3 position)
    {
        Burst(position, new Color(0.4f, 0.9f, 1f, 1f), 24, 6f, 0.3f, 0.7f, -0.4f);
        Burst(position, new Color(0.7f, 1f, 1f, 1f), 12, 3f, 0.2f, 0.5f);
    }

    public static void DeathExplosion(Vector3 position)
    {
        Burst(position, new Color(1f, 0.45f, 0.2f, 1f), 30, 7f, 0.35f, 0.8f, 1.5f);
        Burst(position, new Color(0.9f, 0.9f, 0.9f, 1f), 18, 4f, 0.25f, 0.6f);
        Burst(position, new Color(1f, 0.75f, 0.3f, 1f), 20, 9f, 0.2f, 0.9f, -0.5f);
    }

    public static void MagnetBurst(Vector3 position)
    {
        Burst(position, new Color(0.55f, 0.95f, 1f, 1f), 22, 5f, 0.25f, 0.6f, -0.3f);
        Burst(position, new Color(0.2f, 0.6f, 1f, 1f), 12, 2.5f, 0.3f, 0.5f);
    }

    /// <summary>Multi-coloured celebration used for new high scores.</summary>
    public static void Confetti(Vector3 position)
    {
        Color[] colors = new Color[]
        {
            new Color(1f, 0.4f, 0.4f, 1f),
            new Color(0.4f, 0.9f, 1f, 1f),
            new Color(1f, 0.9f, 0.3f, 1f),
            new Color(0.5f, 1f, 0.5f, 1f),
            new Color(0.8f, 0.5f, 1f, 1f)
        };
        for (int i = 0; i < colors.Length; i++)
        {
            Burst(position + Vector3.up * 1.5f + Random.insideUnitSphere * 0.8f,
                  colors[i], 18, 6.5f, 0.22f, 1.4f, 4f);
        }
    }
}
