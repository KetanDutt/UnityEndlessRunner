using UnityEngine;

/// <summary>
/// Pure-math easing functions used across the project for smooth tweens.
/// No dependencies, no allocation.
/// </summary>
public static class Easing
{
    public static float Clamp01(float t)
    {
        if (t < 0f) return 0f;
        if (t > 1f) return 1f;
        return t;
    }

    public static float Linear(float t)
    {
        return t;
    }

    public static float QuadIn(float t)
    {
        return t * t;
    }

    public static float QuadOut(float t)
    {
        return -t * (t - 2f);
    }

    public static float QuadInOut(float t)
    {
        return t < 0.5f ? 2f * t * t : -2f * t * t + 4f * t - 1f;
    }

    public static float CubicIn(float t)
    {
        return t * t * t;
    }

    public static float CubicOut(float t)
    {
        float u = t - 1f;
        return u * u * u + 1f;
    }

    public static float CubicInOut(float t)
    {
        if (t < 0.5f) return 4f * t * t * t;
        float u = t - 1f;
        return 4f * u * u * u + 1f;
    }

    public static float SineOut(float t)
    {
        return Mathf.Sin(t * Mathf.PI * 0.5f);
    }

    public static float BackOut(float t)
    {
        const float s = 1.70158f;
        float u = t - 1f;
        return u * u * ((s + 1f) * u + s) + 1f;
    }

    public static float BackIn(float t)
    {
        const float s = 1.70158f;
        return t * t * ((s + 1f) * t - s);
    }

    public static float BounceOut(float t)
    {
        if (t < 1f / 2.75f) return 7.5625f * t * t;
        if (t < 2f / 2.75f)
        {
            t -= 1.5f / 2.75f;
            return 7.5625f * t * t + 0.75f;
        }
        if (t < 2.5f / 2.75f)
        {
            t -= 2.25f / 2.75f;
            return 7.5625f * t * t + 0.9375f;
        }
        t -= 2.625f / 2.75f;
        return 7.5625f * t * t + 0.984375f;
    }

    public static float SmoothStep(float t)
    {
        t = Clamp01(t);
        return t * t * (3f - 2f * t);
    }

    /// <summary>
    /// Frame-rate independent exponential damping factor.
    /// Usage: value = Mathf.Lerp(value, target, Easing.Damp(rate, Time.deltaTime));
    /// </summary>
    public static float Damp(float rate, float deltaTime)
    {
        return 1f - Mathf.Exp(-rate * deltaTime);
    }
}
