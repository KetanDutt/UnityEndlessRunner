using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Tracks the current run's score, levels up the player's speed, and updates the
/// on-screen score text. Auto-wires its UI by name and falls back to creating a
/// score text if the scene does not provide one. The old levelup() threw a
/// NullReferenceException because it called GetComponent&lt;playerMotor&gt;() on a UI
/// GameObject; the player reference is now resolved properly.
/// </summary>
public class Score : MonoBehaviour
{
    [Header("UI")]
    public Text scoreText;       // In-game score counter.
    public Text endScoreText;    // Death screen final score.

    [Header("Leveling")]
    public int pointsPerLevel = 25;
    public int scorePerCoin = 5;
    public float speedBoostOnLevelUp = 0.5f;
    public float speedPerLevel = 0.5f;
    public float maxLevelSpeed = 30f;

    private int score;
    private int level;
    private playerMotor player;

    void Start()
    {
        if (player == null)
            player = FindObjectOfType<playerMotor>();

        if (scoreText == null)
        {
            GameObject go = GameObject.Find("scoretext");
            if (go != null) scoreText = go.GetComponent<Text>();
        }
        if (scoreText == null)
        {
            GameObject go = GameObject.Find("Scoretext");
            if (go != null) scoreText = go.GetComponent<Text>();
        }
        if (scoreText == null)
            scoreText = CreateFallbackScoreText();

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
        level = 0;
        RefreshText();
    }

    public int GetScore() { return score; }
    public int GetLevel() { return level; }

    public void AddDistanceScore(int meters)
    {
        score += meters;
        RefreshText();
    }

    public void AddCoin()
    {
        AddScore(scorePerCoin);
    }

    public void AddScore(int amount)
    {
        score += amount;

        if (score >= (level + 1) * pointsPerLevel)
        {
            level++;
            OnLevelUp();
        }

        RefreshText();
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
    }

    void RefreshText()
    {
        if (scoreText != null)
            scoreText.text = "Score: " + score;
    }

    /// <summary>Called by playerMotor on death: stores the best score and
    /// updates the death screen text.</summary>
    public void FinalizeScore()
    {
        Game.Instance.BestScore = score;
        Game.CurrentScore = score;

        if (endScoreText == null)
        {
            GameObject go = GameObject.Find("EndScore");
            if (go != null) endScoreText = go.GetComponent<Text>();
        }

        if (endScoreText != null)
            endScoreText.text = "Score: " + score + "\nBest: " + Game.HighScore;
    }
}
