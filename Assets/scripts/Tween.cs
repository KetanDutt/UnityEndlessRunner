using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Lightweight coroutine-based tween helpers. Everything here is allocation-free
/// during playback (delegates are captured once per tween) and uses the Easing
/// functions for a consistent, polished feel across the game.
///
/// All tweens support an `unscaled` flag so UI can keep animating while
/// Time.timeScale is 0 (for example while the game is paused).
/// </summary>
public static class Tween
{
    /// <summary>Core driver: calls onUpdate(k) every frame with the eased progress 0..1.</summary>
    public static IEnumerator Animate(float duration, Func<float, float> ease,
                                      Action<float> onUpdate, Action onComplete, bool unscaled)
    {
        if (onUpdate == null)
        {
            if (onComplete != null) onComplete();
            yield break;
        }

        float t = 0f;
        while (t < 1f)
        {
            t += (unscaled ? Time.unscaledDeltaTime : Time.deltaTime) / Mathf.Max(duration, 0.0001f);
            float k = Mathf.Clamp01(t);
            onUpdate(ease != null ? ease(k) : k);
            yield return null;
        }
        if (onComplete != null) onComplete();
    }

    public static IEnumerator Delay(float seconds, Action action, bool unscaled)
    {
        if (seconds <= 0f)
        {
            if (action != null) action();
            yield break;
        }
        if (unscaled) yield return new WaitForSecondsRealtime(seconds);
        else yield return new WaitForSeconds(seconds);
        if (action != null) action();
    }

    public static IEnumerator MoveLocal(Transform t, Vector3 from, Vector3 to, float duration,
                                        Func<float, float> ease, bool unscaled)
    {
        if (t == null) yield break;
        return Animate(duration, ease, delegate(float k)
        {
            if (t != null) t.localPosition = Vector3.LerpUnclamped(from, to, k);
        }, null, unscaled);
    }

    public static IEnumerator Scale(Transform t, Vector3 from, Vector3 to, float duration,
                                    Func<float, float> ease, bool unscaled)
    {
        if (t == null) yield break;
        return Animate(duration, ease, delegate(float k)
        {
            if (t != null) t.localScale = Vector3.LerpUnclamped(from, to, k);
        }, null, unscaled);
    }

    public static IEnumerator MoveAnchored(RectTransform rt, Vector2 from, Vector2 to, float duration,
                                           Func<float, float> ease, bool unscaled)
    {
        if (rt == null) yield break;
        return Animate(duration, ease, delegate(float k)
        {
            if (rt != null) rt.anchoredPosition = Vector2.LerpUnclamped(from, to, k);
        }, null, unscaled);
    }

    public static IEnumerator Fade(CanvasGroup cg, float from, float to, float duration,
                                   Func<float, float> ease, bool unscaled)
    {
        if (cg == null) yield break;
        return Animate(duration, ease, delegate(float k)
        {
            if (cg != null) cg.alpha = Mathf.LerpUnclamped(from, to, k);
        }, null, unscaled);
    }

    public static IEnumerator FadeGraphic(Graphic g, float from, float to, float duration,
                                          Func<float, float> ease, bool unscaled)
    {
        if (g == null) yield break;
        Color c = g.color;
        return Animate(duration, ease, delegate(float k)
        {
            if (g != null)
            {
                c.a = Mathf.LerpUnclamped(from, to, k);
                g.color = c;
            }
        }, null, unscaled);
    }

    public static IEnumerator ColorGraphic(Graphic g, Color from, Color to, float duration,
                                           Func<float, float> ease, bool unscaled)
    {
        if (g == null) yield break;
        return Animate(duration, ease, delegate(float k)
        {
            if (g != null) g.color = Color.LerpUnclamped(from, to, k);
        }, null, unscaled);
    }
}
