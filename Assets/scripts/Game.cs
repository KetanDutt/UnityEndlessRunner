using UnityEngine;

/// <summary>
/// Persistent settings and best-score storage. Values survive scene changes and
/// are saved to PlayerPrefs so the best score persists between sessions.
/// This singleton is created lazily so it also works when the scene does not
/// reference it explicitly.
/// </summary>
public class Game : MonoBehaviour
{
    private const string BestScoreKey = "EndlessRunner.BestScore";
    private const string MutedKey = "EndlessRunner.Muted";
    private const string VibrationKey = "EndlessRunner.Vibration";

    public const int Lanes = 3;
    public const float LaneWidth = 3.4f;

    private static Game instance;

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

    private int bestScore;
    private bool muted;
    private bool vibrationEnabled;

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

    public static float Speed { get; set; }
    public static bool IsRunning { get; set; }
    public static bool GameOver { get; set; }
    public static int CurrentScore { get; set; }

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

    public static void ResetRun()
    {
        CurrentScore = 0;
        GameOver = false;
        IsRunning = true;
    }

    public static int HighScore
    {
        get { return Instance.bestScore; }
    }
}
