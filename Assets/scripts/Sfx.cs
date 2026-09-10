using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Procedural sound effects and haptics. Every clip is synthesized once at
/// runtime with AudioClip.Create and cached, so the game needs no imported
/// audio assets and plays without per-shot garbage allocation. Hook points are
/// centralized here so real audio assets can replace them later.
/// </summary>
public static class Sfx
{
    private static AudioSource source;
    private static bool initialized;
    private static readonly Dictionary<string, AudioClip> cache = new Dictionary<string, AudioClip>();

    // ------------------------------------------------------------------
    // Public one-shot sounds.
    // ------------------------------------------------------------------

    public static void Jump()
    {
        Play(430f, 0.16f, 0.5f, 520f);
    }

    public static void Land()
    {
        Play(150f, 0.10f, 0.42f, -170f);
    }

    public static void Coin()
    {
        // Two quick notes: a bright "ding".
        Play(1180f, 0.12f, 0.45f, 260f);
        Play(1560f, 0.16f, 0.32f, 180f, 0.05f);
    }

    public static void LaneChange()
    {
        Play(220f, 0.09f, 0.3f, 700f);
    }

    public static void LevelUp()
    {
        float[] notes = new float[] { 520f, 660f, 780f, 1040f };
        PlayArpeggio(notes, 0.05f, 0.34f, 60f);
    }

    public static void Death()
    {
        Play(280f, 0.4f, 0.55f, -460f);
        Play(140f, 0.5f, 0.4f, -160f, 0.06f);
    }

    public static void Click()
    {
        Play(900f, 0.07f, 0.4f, -220f);
    }

    public static void CountdownTick()
    {
        Play(760f, 0.10f, 0.5f, 60f);
    }

    public static void Go()
    {
        Play(880f, 0.22f, 0.55f, 240f);
    }

    public static void PowerUp()
    {
        PlayArpeggio(new float[] { 660f, 880f, 1100f, 1320f }, 0.06f, 0.4f, 80f);
    }

    public static void NewBest()
    {
        PlayArpeggio(new float[] { 523f, 659f, 784f, 1046f, 1318f }, 0.07f, 0.4f, 40f);
    }

    public static void Pause()
    {
        Play(500f, 0.12f, 0.35f, -300f);
    }

    // ------------------------------------------------------------------
    // Haptics.
    // ------------------------------------------------------------------

    /// <summary>Short vibration, gated by the player's preference and platform.</summary>
    public static void Haptic()
    {
        if (!Application.isMobilePlatform) return;
        if (Game.Instance == null || !Game.Instance.VibrationEnabled) return;
        Handheld.Vibrate();
    }

    // ------------------------------------------------------------------
    // Internals.
    // ------------------------------------------------------------------

    private static void Play(float frequency, float duration, float volume, float slide, float delay = 0f)
    {
        if (Game.Instance != null && Game.Instance.Muted) return;
        EnsureSource();
        if (source == null) return;

        AudioClip clip = GetClip(frequency, duration, slide);
        if (clip == null) return;

        if (delay > 0f)
        {
            // PlayOneShot has no delay; use a scheduled start on the source.
            // We create a tiny throwaway host so scheduled clips survive scene
            // changes and overlap correctly.
            GameObject host = new GameObject("SfxTimed");
            AudioSource timed = host.AddComponent<AudioSource>();
            timed.playOnAwake = false;
            timed.clip = clip;
            timed.volume = volume;
            timed.PlayDelayed(delay);
            Object.Destroy(host, clip.length + delay + 0.2f);
        }
        else
        {
            source.PlayOneShot(clip, volume);
        }
    }

    private static void PlayArpeggio(float[] frequencies, float step, float volume, float slide)
    {
        for (int i = 0; i < frequencies.Length; i++)
        {
            Play(frequencies[i], step * 2.4f, volume, slide, i * step);
        }
    }

    private static AudioClip GetClip(float frequency, float duration, float slide)
    {
        string key = frequency + "|" + duration + "|" + slide;
        AudioClip clip;
        if (!cache.TryGetValue(key, out clip))
        {
            clip = Generate(frequency, duration, slide);
            cache[key] = clip;
        }
        return clip;
    }

    private static AudioClip Generate(float frequency, float duration, float slide)
    {
        int sampleRate = 44100;
        int samples = Mathf.Max(64, (int)(sampleRate * duration));
        float[] data = new float[samples];
        float phase = 0f;
        float phaseStep = 2f * Mathf.PI * frequency / sampleRate;
        float slideStep = 2f * Mathf.PI * slide / sampleRate / sampleRate;

        for (int i = 0; i < samples; i++)
        {
            // Sine tone with a fast attack and exponential decay, plus a hint
            // of a second harmonic for warmth.
            float t = (float)i / samples;
            float envelope = Mathf.Min(1f, t * 40f) * Mathf.Exp(-t * 5.5f);
            float sample = Mathf.Sin(phase) + 0.35f * Mathf.Sin(phase * 2f);
            data[i] = sample * envelope;
            phase += phaseStep;
            phaseStep += slideStep;
        }

        AudioClip clip = AudioClip.Create("sfx", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private static void EnsureSource()
    {
        if (initialized) return;
        initialized = true;

        GameObject host = new GameObject("Sfx");
        Object.DontDestroyOnLoad(host);
        source = host.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.spatialBlend = 0f;
    }
}
