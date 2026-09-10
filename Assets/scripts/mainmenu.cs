using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Main menu controller. Shows the best score and starts the game when the play
/// button is pressed. Auto-wires the play button and best-score text by name,
/// and builds them if the scene does not provide them.
/// </summary>
public class mainmenu : MonoBehaviour
{
    public Button playButton;
    public Text highScoreText;
    public string gameScene = "GameScene";

    void Start()
    {
        AutoWire();
        RefreshHighScore();
    }

    void AutoWire()
    {
        if (playButton == null)
        {
            GameObject go = GameObject.Find("PlayButton");
            if (go != null) playButton = go.GetComponent<Button>();
        }
        if (playButton == null)
        {
            GameObject go = GameObject.Find("Play");
            if (go != null) playButton = go.GetComponent<Button>();
        }

        if (playButton == null)
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                GameObject cgo = GameObject.Find("Canvas");
                if (cgo != null) canvas = cgo.GetComponent<Canvas>();
            }
            if (canvas == null)
                canvas = UiFactory.CreateCanvas("Canvas");

            playButton = UiFactory.CreateButton(canvas.transform, "PlayButton", "Play", PlayGame);
            ((RectTransform)playButton.transform).anchoredPosition = new Vector2(0f, -40f);
        }
        else
        {
            // Replace any broken serialized listener with a clean one.
            playButton.onClick = new Button.ButtonClickedEvent();
        }

        playButton.onClick.AddListener(PlayGame);

        if (highScoreText == null)
        {
            GameObject go = GameObject.Find("highscoretext");
            if (go != null) highScoreText = go.GetComponent<Text>();
        }
        if (highScoreText == null)
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                GameObject cgo = GameObject.Find("Canvas");
                if (cgo != null) canvas = cgo.GetComponent<Canvas>();
            }
            if (canvas != null)
                highScoreText = UiFactory.CreateText(canvas.transform, "highscoretext", "", 30,
                                                     new Vector2(0f, 80f), TextAnchor.MiddleCenter);
        }
    }

    void RefreshHighScore()
    {
        if (highScoreText != null)
            highScoreText.text = "Best Score: " + Game.HighScore;
    }

    public void PlayGame()
    {
        Sfx.Click();
        Game.ResetRun();
        SceneManager.LoadScene(gameScene);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
