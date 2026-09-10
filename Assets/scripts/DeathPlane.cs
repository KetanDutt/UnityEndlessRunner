using UnityEngine;

/// <summary>
/// A kill volume that follows the player horizontally. It kills the player when
/// they fall off the track or into a gap. Detection uses both a trigger callback
/// and a direct height check, so it is robust across Unity physics versions.
/// </summary>
public class DeathPlane : MonoBehaviour
{
    public Transform player;
    public float yOffset = -8f;
    public float killY = -5f;   // Any player below this height dies.

    void Awake()
    {
        ResolvePlayer();
    }

    void Update()
    {
        if (player == null)
        {
            ResolvePlayer();
            if (player == null) return;
        }

        Vector3 pos = transform.position;
        pos.x = player.position.x;
        pos.z = player.position.z;
        pos.y = yOffset;
        transform.position = pos;

        // Fallback: kill when the player drops below the kill height.
        if (Game.IsRunning && !Game.GameOver && player.position.y < killY)
        {
            playerMotor pm = player.GetComponent<playerMotor>();
            if (pm != null) pm.Death();
        }
    }

    void ResolvePlayer()
    {
        if (player == null)
        {
            playerMotor pm = FindObjectOfType<playerMotor>();
            if (pm != null) player = pm.transform;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && Game.IsRunning && !Game.GameOver)
        {
            playerMotor pm = other.GetComponent<playerMotor>();
            if (pm != null) pm.Death();
        }
    }
}
