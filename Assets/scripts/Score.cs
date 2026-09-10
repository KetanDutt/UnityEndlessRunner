using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Tracks the current run's score, coin count and level, drives the speed
/// level-ups and updates the on-screen UI. Display is delegated to GameUI when
/// it exists (the normal case), with a minimal fallback for scenes without one.
/// </summary>
public class Score : MonoBehaviour
{
    public static Score Current { get; private set; }

    [Header("UI (fallback)")]
    public Text scoreText;
    public Text endScoreText;

    [Header("Leveling")]
    public int pointsPerLevel = 25;
    public int scorePerCoin = 5;
    public float speedBoostOnLevelUp = 0.5f;
    public float speedPerLevel = 0.5f;
    public float maxLevelSpeed = 30f;

    private int score;
    private int coins;
    private int level;
    private playerMotor player;

    void Awake()
    {
        Current = this;
    }

    void OnDestroy()
    {
        if (Current == this) Current = null;
    }

    void Start()
    {
        if (player == null)
            player = FindObjectOfType<playerMotor>();

        // When GameUI exists (the normal case) it owns the score display; only
        // build a fallback text when running without one.
        if (GameUI.Current == null)
        {
            if (scoreText == null)
            {
                GameObject go = GameObject.Find("scoretext");
                if (go == null) go = GameObject.Find("Scoretext");
                if (go != null) scoreText = go.GetComponent<Text>();
            }
            if (scoreText == null)
                scoreText = CreateFallbackScoreText();
        }

        ResetScore();
    }

    Text CreateFallbackScoreText()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject cgo = GameObject.Find("Canvas");
            if (cgo != null) canvas = cgo.GetComponent<Canvas>();
        }
        if (canvas == null)
            canvas = UiFactory.CreateCanvas("Canvas");

        return UiFactory.CreateText(canvas.transform, "ScoreText", "Score: 0", 30,
                                    new Vector2(0f, 270f), TextAnchor.MiddleCenter);
    }

    public void ResetScore()
    {
        score = 0;
        coins = 0;
        level = 0;
        Refresh();
    }

    public int GetScore() { return score; }
    public int GetCoins() { return coins; }
    public int GetLevel() { return level; }

    /// <summary>Adds distance score (1 point per meter).</summary>
    public void AddDistanceScore(int meters)
    {
        AddScore(meters, false);
    }

    public void AddCoin()
    {
        coins++;
        AddScore(scorePerCoin, true);

        if (GameUI.Current != null)
            GameUI.Current.SetCoins(coins);
    }

    /// <summary>Central scoring entry point. Handles multi-level jumps.</summary>
    private void AddScore(int amount, bool fromCoin)
    {
        score += amount;

        // A single pick-up can cross several level thresholds at once.
        while (score >= (level + 1) * pointsPerLevel)
        {
            level++;
            OnLevelUp();
        }

        Refresh();

        if (fromCoin && GameUI.Current != null)
            GameUI.Current.ScorePulse();
    }

    void OnLevelUp()
    {
        if (player != null)
        {
            player.speed = Mathf.Clamp(player.speed + speedBoostOnLevelUp,
                                       player.minSpeed, maxLevelSpeed);
            player.maxSpeed = Mathf.Clamp(player.maxSpeed + speedPerLevel,
                                          player.minSpeed, maxLevelSpeed);
        }

        Vfx.LevelUpBurst(player != null ? player.transform.position + Vector3.up
                                        : Vector3.up * 2f);
        Sfx.LevelUp();
        Sfx.Haptic();

        if (GameUI.Current != null)
            GameUI.Current.ShowBanner("LEVEL " + level + "!");
    }

    void Refresh()
    {
        if (scoreText != null)
            scoreText.text = "Score: " + score;

        if (GameUI.Current != null)
            GameUI.Current.SetScore(score);
    }

    /// <summary>Called by playerMotor on death: stores the best score and
    /// updates the death screen text.</summary>
    public void FinalizeScore()
    {
        bool isNewBest = score > Game.HighScore && score > 0;
        if (isNewBest)
        {
            Game.Instance.BestScore = score;
            Game.LastRunWasNewBest = true;
            Sfx.NewBest();
            Vfx.Confetti(player != null ? player.transform.position + Vector3.up
                                        : Vector3.up * 2f);
        }
        else
        {
            Game.LastRunWasNewBest = false;
        }

        Game.CurrentScore = score;
        Game.LastRunCoins = coins;

        if (endScoreText == null)
        {
            GameObject go = GameObject.Find("EndScore");
            if (go != null) endScoreText = go.GetComponent<Text>();
        }

        if (endScoreText != null)
        {
            string text = "Score: " + score + "\nCoins: " + coins + "\nBest: " + Game.HighScore;
            if (isNewBest) text += "\nNEW BEST!";
            endScoreText.text = text;
        }
    }
}
