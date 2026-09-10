using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Death screen controller. Slides the death panel in, shows the final score and
/// best score, and offers Restart / Menu buttons. It auto-wires the panel, the
/// EndScore text and the buttons by name, and builds its own panel if the scene
/// does not provide one.
/// </summary>
public class DeathMenu : MonoBehaviour
{
    public GameObject deathPanel;
    public Text endScoreText;
    public Button restartButton;
    public Button menuButton;

    public float slideInTime = 0.35f;

    private CanvasGroup canvasGroup;
    private bool visible;

    void Start()
    {
        AutoWire();
        if (deathPanel != null)
            deathPanel.SetActive(false);
    }

    void AutoWire()
    {
        if (deathPanel == null)
            deathPanel = GameObject.Find("deathmenu");

        if (deathPanel == null)
            BuildFallbackPanel();

        if (deathPanel != null)
        {
            canvasGroup = deathPanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null) canvasGroup = deathPanel.AddComponent<CanvasGroup>();

            if (endScoreText == null)
            {
                Transform t = deathPanel.transform.Find("EndScore");
                if (t != null) endScoreText = t.GetComponent<Text>();
                if (endScoreText == null)
                {
                    GameObject go = GameObject.Find("EndScore");
                    if (go != null) endScoreText = go.GetComponent<Text>();
                }
            }

            if (restartButton == null)
                restartButton = FindButton(deathPanel.transform, new string[] { "RePlay", "Restart", "Play" });

            if (menuButton == null)
                menuButton = FindButton(deathPanel.transform, new string[] { "Menu", "MainMenu" });
        }

        if (restartButton != null)
        {
            restartButton.onClick = new Button.ButtonClickedEvent();
            restartButton.onClick.AddListener(RestartGame);
        }
        if (menuButton != null)
        {
            menuButton.onClick = new Button.ButtonClickedEvent();
            menuButton.onClick.AddListener(GoToMenu);
        }
    }

    void BuildFallbackPanel()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject cgo = GameObject.Find("Canvas");
            if (cgo != null) canvas = cgo.GetComponent<Canvas>();
        }
        if (canvas == null)
            canvas = UiFactory.CreateCanvas("Canvas");

        GameObject panel = new GameObject("deathmenu");
        panel.transform.SetParent(canvas.transform, false);

        RectTransform rt = panel.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        Image bg = panel.AddComponent<Image>();
        bg.color = new Color(0.06f, 0.07f, 0.1f, 0.9f);

        endScoreText = UiFactory.CreateText(panel.transform, "EndScore", "Score: 0\nBest: 0", 36,
                                            new Vector2(0f, 80f), TextAnchor.MiddleCenter);

        restartButton = UiFactory.CreateButton(panel.transform, "RePlay", "Restart", RestartGame);
        ((RectTransform)restartButton.transform).anchoredPosition = new Vector2(0f, -40f);

        menuButton = UiFactory.CreateButton(panel.transform, "Menu", "Menu", GoToMenu);
        ((RectTransform)menuButton.transform).anchoredPosition = new Vector2(0f, -130f);

        deathPanel = panel;
    }

    Button FindButton(Transform root, string[] names)
    {
        for (int i = 0; i < names.Length; i++)
        {
            Transform child = root.Find(names[i]);
            if (child != null)
            {
                Button b = child.GetComponent<Button>();
                if (b != null) return b;
                b = child.GetComponentInChildren<Button>();
                if (b != null) return b;
            }
        }

        Button[] all = root.GetComponentsInChildren<Button>(true);
        for (int i = 0; i < all.Length; i++)
        {
            for (int n = 0; n < names.Length; n++)
                if (all[i].name == names[n]) return all[i];
        }
        return null;
    }

    public void Show()
    {
        if (visible) return;
        visible = true;

        if (deathPanel != null)
        {
            deathPanel.SetActive(true);
            StartCoroutine(SlideIn());
        }
    }

    IEnumerator SlideIn()
    {
        if (canvasGroup == null) yield break;

        canvasGroup.alpha = 0f;
        float startY = -220f;
        RectTransform rt = deathPanel.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, startY);
            Vector2 target = new Vector2(rt.anchoredPosition.x, 0f);
            float t = 0f;
            while (t < 1f)
            {
                t += Time.unscaledDeltaTime / slideInTime;
                float eased = Easing.BackOut(Mathf.Clamp01(t));
                rt.anchoredPosition = Vector2.Lerp(
                    new Vector2(rt.anchoredPosition.x, startY), target, eased);
                canvasGroup.alpha = eased;
                yield return null;
            }
        }
        else
        {
            float t = 0f;
            while (t < 1f)
            {
                t += Time.unscaledDeltaTime / slideInTime;
                canvasGroup.alpha = Mathf.Clamp01(t);
                yield return null;
            }
        }
    }

    public void RestartGame()
    {
        Sfx.Click();
        Time.timeScale = 1f;
        Game.ResetRun();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GoToMenu()
    {
        Sfx.Click();
        Time.timeScale = 1f;
        SceneManager.LoadScene("Menu");
    }
}
