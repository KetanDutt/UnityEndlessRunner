using UnityEngine;

/// <summary>
/// Procedural sound effects. All clips are synthesized at runtime with
/// AudioClip.Create, so the game needs no imported audio assets. Hook points are
/// centralized here so real audio assets can replace them later.
/// </summary>
public static class Sfx
{
    private static AudioSource source;
    private static bool initialized;

    public static void Play(float frequency, float duration, float volume, float slide)
    {
        if (Game.Instance.Muted) return;

        EnsureSource();
        if (source == null) return;

        int sampleRate = 44100;
        int samples = Mathf.Max(64, (int)(sampleRate * duration));
        float[] data = new float[samples];
        float phase = 0f;
        float phaseStep = 2f * Mathf.PI * frequency / sampleRate;
        float slideStep = 2f * Mathf.PI * slide / sampleRate / sampleRate;

        for (int i = 0; i < samples; i++)
        {
            // Simple sine tone with a short attack and exponential decay.
            float t = (float)i / samples;
            float envelope = Mathf.Min(1f, t * 40f) * Mathf.Exp(-t * 6f);
            data[i] = Mathf.Sin(phase) * envelope;
            phase += phaseStep;
            phaseStep += slideStep;
        }

        AudioClip clip = AudioClip.Create("sfx", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        source.PlayOneShot(clip, volume);
    }

    public static void Jump()
    {
        Play(420f, 0.18f, 0.5f, 500f);
    }

    public static void Land()
    {
        Play(160f, 0.10f, 0.4f, -180f);
    }

    public static void Coin()
    {
        Play(1200f, 0.14f, 0.45f, 400f);
    }

    public static void LevelUp()
    {
        Play(660f, 0.16f, 0.5f, 400f);
    }

    public static void Death()
    {
        Play(300f, 0.4f, 0.55f, -420f);
    }

    public static void Click()
    {
        Play(900f, 0.08f, 0.4f, -200f);
    }

    static void EnsureSource()
    {
        if (initialized) return;
        initialized = true;

        GameObject host = new GameObject("Sfx");
        Object.DontDestroyOnLoad(host);
        source = host.AddComponent<AudioSource>();
        source.playOnAwake = false;
    }
}
