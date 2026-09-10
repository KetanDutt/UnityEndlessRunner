using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A floating pick-up (currently the magnet). Powerups register themselves in
/// a static list so the player can collect them with a cheap proximity check,
/// mirroring the coin pattern. The visual is a spinning, bobbing, glowing orb.
/// </summary>
public class Powerup : MonoBehaviour
{
    public static readonly List<Powerup> Active = new List<Powerup>();

    public float spinSpeed = 120f;
    public float bobHeight = 0.25f;
    public float bobSpeed = 3f;
    public float collectRadius = 1.25f;

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
        const float duration = 0.14f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            transform.localScale = start * (1f - Easing.BackIn(Mathf.Clamp01(t)));
            yield return null;
        }
        Destroy(gameObject);
    }
}
