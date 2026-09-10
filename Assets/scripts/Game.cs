using UnityEngine;

/// <summary>
/// Persistent, cross-scene game state and settings.
///
/// A single instance survives scene loads (DontDestroyOnLoad) and stores:
///   * the best score (saved to PlayerPrefs),
///   * audio / vibration preferences,
///   * the current run's transient state (score, coins, pause, magnet timer...).
///
/// It is created lazily by <see cref="Instance"/> so it works even when a scene
/// does not reference it explicitly (the binary-serialized scenes in this
/// project carry no script references).
/// </summary>
public class Game : MonoBehaviour
{
    private const string BestScoreKey = "EndlessRunner.BestScore";
    private const string MutedKey = "EndlessRunner.Muted";
    private const string VibrationKey = "EndlessRunner.Vibration";

    public const int Lanes = 3;
    public const float LaneWidth = 3.4f;

    private static Game instance;

    private int bestScore;
    private bool muted;
    private bool vibrationEnabled;

    /// <summary>Global (cross-scene) singleton.</summary>
    public static Game Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<Game>();
                if (instance == null)
                {
                    GameObject go = new GameObject("Game");
                    instance = go.AddComponent<Game>();
                }
            }
            return instance;
        }
    }

    // ------------------------------------------------------------------
    // Transient run state (reset by ResetRun).
    // ------------------------------------------------------------------

    /// <summary>Current forward speed in world units per second.</summary>
    public static float Speed { get; set; }

    /// <summary>True while the player is allowed to run (false during the countdown).</summary>
    public static bool IsRunning { get; set; }

    /// <summary>True once the run has ended.</summary>
    public static bool GameOver { get; set; }

    /// <summary>True while the game is paused.</summary>
    public static bool IsPaused { get; private set; }

    /// <summary>Score of the run that just ended (kept for the death screen).</summary>
    public static int CurrentScore { get; set; }

    /// <summary>Coins collected in the run that just ended.</summary>
    public static int LastRunCoins { get; set; }

    /// <summary>True if the last run set a new high score.</summary>
    public static bool LastRunWasNewBest { get; set; }

    /// <summary>Remaining magnet power-up time in seconds.</summary>
    public static float MagnetTime { get; set; }

    // ------------------------------------------------------------------
    // Persistent settings.
    // ------------------------------------------------------------------

    public bool Muted
    {
        get { return muted; }
        set
        {
            muted = value;
            PlayerPrefs.SetInt(MutedKey, muted ? 1 : 0);
        }
    }

    public bool VibrationEnabled
    {
        get { return vibrationEnabled; }
        set
        {
            vibrationEnabled = value;
            PlayerPrefs.SetInt(VibrationKey, vibrationEnabled ? 1 : 0);
        }
    }

    /// <summary>Best score. Setting a lower value is ignored.</summary>
    public int BestScore
    {
        get { return bestScore; }
        set
        {
            if (value <= bestScore) return;
            bestScore = value;
            PlayerPrefs.SetInt(BestScoreKey, bestScore);
        }
    }

    /// <summary>Convenience getter for the best score.</summary>
    public static int HighScore
    {
        get { return Instance.bestScore; }
    }

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        bestScore = PlayerPrefs.GetInt(BestScoreKey, 0);
        muted = PlayerPrefs.GetInt(MutedKey, 0) != 0;
        vibrationEnabled = PlayerPrefs.GetInt(VibrationKey, 1) != 0;

        Application.targetFrameRate = 60;
    }

    /// <summary>
    /// Resets all per-run state. Safe to call at any time (scene load,
    /// restart, etc.) — it also un-pauses and restores the time scale.
    /// </summary>
    public static void ResetRun()
    {
        CurrentScore = 0;
        LastRunCoins = 0;
        LastRunWasNewBest = false;
        GameOver = false;
        IsPaused = false;
        IsRunning = true;   // the game scene immediately overrides this with its countdown.
        MagnetTime = 0f;
        Time.timeScale = 1f;
    }

    public static void Pause()
    {
        if (GameOver || IsPaused) return;
        IsPaused = true;
        Time.timeScale = 0f;
    }

    public static void Resume()
    {
        if (!IsPaused) return;
        IsPaused = false;
        Time.timeScale = 1f;
    }

    public static void TogglePause()
    {
        if (IsPaused) Resume();
        else Pause();
    }
}
