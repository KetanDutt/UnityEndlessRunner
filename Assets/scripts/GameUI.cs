using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// In-game HUD and feedback layer. Builds (or reuses) the canvas, shows the
/// score / coins / speed, and drives the countdown, level banners, floating
/// score popups, toasts and the pause menu. Everything animates with tweens.
/// </summary>
public class GameUI : MonoBehaviour
{
    public static GameUI Current { get; private set; }

    private Canvas canvas;
    private RectTransform canvasRect;

    private Text scoreText;
    private Text coinText;
    private Text speedText;
    private Text bannerText;
    private Text countdownText;
    private Text toastText;
    private RectTransform popupLayer;

    private GameObject bannerGO;
    private CanvasGroup bannerGroup;
    private GameObject countdownGO;
    private CanvasGroup countdownGroup;
    private GameObject toastGO;
    private CanvasGroup toastGroup;

    private GameObject pausePanelGO;
    private CanvasGroup pauseGroup;
    private Text pauseMuteLabel;

    private Coroutine bannerRoutine;
    private Coroutine toastRoutine;

    void Awake()
    {
        Current = this;
        BuildCanvas();
        BuildHud();
    }

    void OnDestroy()
    {
        if (Current == this) Current = null;
    }

    void Update()
    {
        // Pause / mute input is handled here so it keeps working regardless of
        // what the player is doing (but not after death or during countdown).
        if (Game.GameOver || !Game.IsRunning) return;

        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
            TogglePauseUI();

        if (Input.GetKeyDown(KeyCode.M))
            ToggleMute();
    }

    // ------------------------------------------------------------------
    // Construction.
    // ------------------------------------------------------------------

    void BuildCanvas()
    {
        canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject cgo = GameObject.Find("Canvas");
            if (cgo != null) canvas = cgo.GetComponent<Canvas>();
        }
        if (canvas == null)
            canvas = UiFactory.CreateCanvas("Canvas");

        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        if (canvas.GetComponent<CanvasScaler>() == null)
        {
            CanvasScaler scaler = canvas.gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(800f, 600f);
        }
        if (canvas.GetComponent<GraphicRaycaster>() == null)
            canvas.gameObject.AddComponent<GraphicRaycaster>();

        canvasRect = canvas.GetComponent<RectTransform>();

        // The legacy scene ships a full-screen "bgimage" that would cover the
        // 3D view; it is superseded by our own HUD so remove it.
        GameObject bg = GameObject.Find("bgimage");
        if (bg != null) Destroy(bg);
    }

    void BuildHud()
    {
        Transform root = canvas.transform;

        // Score (top center).
        GameObject scoreGo = GameObject.Find("scoretext");
        if (scoreGo == null) scoreGo = GameObject.Find("Scoretext");
        if (scoreGo != null) scoreText = scoreGo.GetComponent<Text>();
        if (scoreText == null)
        {
            scoreText = UiFactory.CreateTextAnchored(root, "scoretext", "Score: 0", 34,
                                                     new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                                                     new Vector2(0f, -20f), new Vector2(400f, 40f),
                                                     TextAnchor.UpperCenter, Color.white);
        }
        RectTransform srt = scoreText.rectTransform;
        srt.anchorMin = new Vector2(0.5f, 1f);
        srt.anchorMax = new Vector2(0.5f, 1f);
        srt.pivot = new Vector2(0.5f, 1f);
        srt.anchoredPosition = new Vector2(0f, -20f);
        srt.sizeDelta = new Vector2(400f, 40f);
        scoreText.font = UiFactory.DefaultFont;
        scoreText.fontSize = 34;
        scoreText.alignment = TextAnchor.UpperCenter;
        scoreText.color = Color.white;
        scoreText.horizontalOverflow = HorizontalWrapMode.Overflow;
        scoreText.verticalOverflow = VerticalWrapMode.Overflow;
        UiFactory.AddOutline(scoreText.gameObject, new Color(0f, 0f, 0f, 0.6f));

        // Coins (top left).
        coinText = UiFactory.CreateTextAnchored(root, "CoinText", "Coins: 0", 24,
                                                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
                                                new Vector2(20f, -20f), new Vector2(240f, 30f),
                                                TextAnchor.UpperLeft, new Color(1f, 0.85f, 0.35f, 1f));
        UiFactory.AddOutline(coinText.gameObject, new Color(0f, 0f, 0f, 0.6f));

        // Speed (bottom left).
        speedText = UiFactory.CreateTextAnchored(root, "SpeedText", "", 18,
                                                 new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f),
                                                 new Vector2(20f, 20f), new Vector2(200f, 26f),
                                                 TextAnchor.LowerLeft, new Color(1f, 1f, 1f, 0.75f));
        UiFactory.AddOutline(speedText.gameObject, new Color(0f, 0f, 0f, 0.5f));

        // Pause button (top right).
        Button pause = UiFactory.CreateButtonStyled(root, "PauseButton", "II", TogglePauseUI,
                                                    new Color(0.12f, 0.14f, 0.2f, 0.85f), new Vector2(64f, 48f));
        RectTransform prt = (RectTransform)pause.transform;
        prt.anchorMin = new Vector2(1f, 1f);
        prt.anchorMax = new Vector2(1f, 1f);
        prt.pivot = new Vector2(1f, 1f);
        prt.anchoredPosition = new Vector2(-20f, -16f);

        // Popup layer (full screen).
        GameObject popupGo = new GameObject("PopupLayer");
        popupGo.transform.SetParent(root, false);
        popupLayer = popupGo.AddComponent<RectTransform>();
        popupLayer.anchorMin = Vector2.zero;
        popupLayer.anchorMax = Vector2.one;
        popupLayer.offsetMin = Vector2.zero;
        popupLayer.offsetMax = Vector2.zero;

        // Banner (center).
        bannerGO = new GameObject("Banner");
        bannerGO.transform.SetParent(root, false);
        RectTransform brt = bannerGO.AddComponent<RectTransform>();
        brt.anchorMin = new Vector2(0.5f, 0.5f);
        brt.anchorMax = new Vector2(0.5f, 0.5f);
        brt.pivot = new Vector2(0.5f, 0.5f);
        brt.sizeDelta = new Vector2(500f, 80f);
        brt.anchoredPosition = new Vector2(0f, 90f);
        bannerGroup = bannerGO.AddComponent<CanvasGroup>();
        bannerGroup.alpha = 0f;
        bannerText = bannerGO.AddComponent<Text>();
        bannerText.font = UiFactory.DefaultFont;
        bannerText.fontSize = 44;
        bannerText.alignment = TextAnchor.MiddleCenter;
        bannerText.color = new Color(1f, 0.95f, 0.6f, 1f);
        bannerText.horizontalOverflow = HorizontalWrapMode.Overflow;
        bannerText.verticalOverflow = VerticalWrapMode.Overflow;
        UiFactory.AddOutline(bannerGO, new Color(0f, 0f, 0f, 0.7f));
        bannerGO.SetActive(false);

        // Countdown (center).
        countdownGO = new GameObject("Countdown");
        countdownGO.transform.SetParent(root, false);
        RectTransform crt = countdownGO.AddComponent<RectTransform>();
        crt.anchorMin = new Vector2(0.5f, 0.5f);
        crt.anchorMax = new Vector2(0.5f, 0.5f);
        crt.pivot = new Vector2(0.5f, 0.5f);
        crt.sizeDelta = new Vector2(500f, 120f);
        crt.anchoredPosition = Vector2.zero;
        countdownGroup = countdownGO.AddComponent<CanvasGroup>();
        countdownGroup.alpha = 0f;
        countdownText = countdownGO.AddComponent<Text>();
        countdownText.font = UiFactory.DefaultFont;
        countdownText.fontSize = 64;
        countdownText.alignment = TextAnchor.MiddleCenter;
        countdownText.color = Color.white;
        countdownText.horizontalOverflow = HorizontalWrapMode.Overflow;
        countdownText.verticalOverflow = VerticalWrapMode.Overflow;
        UiFactory.AddOutline(countdownGO, new Color(0f, 0f, 0f, 0.7f));
        countdownGO.SetActive(false);

        // Toast (bottom center).
        toastGO = new GameObject("Toast");
        toastGO.transform.SetParent(root, false);
        RectTransform trt = toastGO.AddComponent<RectTransform>();
        trt.anchorMin = new Vector2(0.5f, 0f);
        trt.anchorMax = new Vector2(0.5f, 0f);
        trt.pivot = new Vector2(0.5f, 0f);
        trt.sizeDelta = new Vector2(600f, 34f);
        trt.anchoredPosition = new Vector2(0f, 120f);
        toastGroup = toastGO.AddComponent<CanvasGroup>();
        toastGroup.alpha = 0f;
        toastText = toastGO.AddComponent<Text>();
        toastText.font = UiFactory.DefaultFont;
        toastText.fontSize = 22;
        toastText.alignment = TextAnchor.MiddleCenter;
        toastText.color = new Color(1f, 1f, 1f, 0.95f);
        toastText.horizontalOverflow = HorizontalWrapMode.Overflow;
        toastText.verticalOverflow = VerticalWrapMode.Overflow;
        UiFactory.AddOutline(toastGO, new Color(0f, 0f, 0f, 0.6f));
        toastGO.SetActive(false);

        BuildPausePanel(root);
    }

    void BuildPausePanel(Transform root)
    {
        pausePanelGO = new GameObject("PausePanel");
        pausePanelGO.transform.SetParent(root, false);
        RectTransform prt = pausePanelGO.AddComponent<RectTransform>();
        prt.anchorMin = Vector2.zero;
        prt.anchorMax = Vector2.one;
        prt.offsetMin = Vector2.zero;
        prt.offsetMax = Vector2.zero;

        Image dim = pausePanelGO.AddComponent<Image>();
        dim.color = new Color(0f, 0f, 0f, 0.6f);
        pauseGroup = pausePanelGO.AddComponent<CanvasGroup>();
        pauseGroup.alpha = 0f;

        // Centered box.
        GameObject box = new GameObject("Box");
        box.transform.SetParent(pausePanelGO.transform, false);
        RectTransform brt = box.AddComponent<RectTransform>();
        brt.anchorMin = new Vector2(0.5f, 0.5f);
        brt.anchorMax = new Vector2(0.5f, 0.5f);
        brt.pivot = new Vector2(0.5f, 0.5f);
        brt.sizeDelta = new Vector2(340f, 420f);
        brt.anchoredPosition = Vector2.zero;
        Image boxImg = box.AddComponent<Image>();
        boxImg.color = new Color(0.09f, 0.11f, 0.17f, 0.97f);

        Text title = UiFactory.CreateTextAnchored(box.transform, "Title", "PAUSED", 40,
                                                  new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                                                  new Vector2(0f, -28f), new Vector2(300f, 60f),
                                                  TextAnchor.UpperCenter, Color.white);
        UiFactory.AddOutline(title.gameObject, new Color(0f, 0f, 0f, 0.6f));

        Button resume = UiFactory.CreateButtonStyled(box.transform, "Resume", "Resume", ResumeClicked,
                                                     new Color(0.2f, 0.65f, 0.35f, 1f), new Vector2(220f, 56f));
        SetAnchoredPosition((RectTransform)resume.transform, new Vector2(0f, -110f));

        Button restart = UiFactory.CreateButtonStyled(box.transform, "Restart", "Restart", RestartClicked,
                                                      new Color(0.15f, 0.45f, 0.85f, 1f), new Vector2(220f, 56f));
        SetAnchoredPosition((RectTransform)restart.transform, new Vector2(0f, -180f));

        Button menu = UiFactory.CreateButtonStyled(box.transform, "Menu", "Menu", MenuClicked,
                                                   new Color(0.55f, 0.35f, 0.15f, 1f), new Vector2(220f, 56f));
        SetAnchoredPosition((RectTransform)menu.transform, new Vector2(0f, -250f));

        Button mute = UiFactory.CreateButtonStyled(box.transform, "Mute", "Sound: On", MuteClicked,
                                                   new Color(0.25f, 0.27f, 0.35f, 1f), new Vector2(220f, 48f));
        SetAnchoredPosition((RectTransform)mute.transform, new Vector2(0f, -322f));
        pauseMuteLabel = mute.GetComponentInChildren<Text>();

        pausePanelGO.SetActive(false);

        RefreshMuteLabel();
    }

    static void SetAnchoredPosition(RectTransform rt, Vector2 pos)
    {
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
    }

    // ------------------------------------------------------------------
    // HUD updates.
    // ------------------------------------------------------------------

    public void SetScore(int value)
    {
        if (scoreText != null) scoreText.text = "Score: " + value;
    }

    public void SetCoins(int value)
    {
        if (coinText != null) coinText.text = "Coins: " + value;
    }

    public void ScorePulse()
    {
        if (scoreText == null) return;
        RectTransform rt = scoreText.rectTransform;
        StartCoroutine(Tween.Scale(rt, Vector3.one * 1.18f, Vector3.one, 0.16f, Easing.BackOut, false));
    }

    void LateUpdate()
    {
        if (speedText != null)
            speedText.text = Mathf.RoundToInt(Game.Speed) + " m/s";
    }

    // ------------------------------------------------------------------
    // Feedback: banners, toasts, popups.
    // ------------------------------------------------------------------

    public void ShowBanner(string text)
    {
        if (bannerRoutine != null) StopCoroutine(bannerRoutine);
        bannerRoutine = StartCoroutine(BannerRoutine(text));
    }

    IEnumerator BannerRoutine(string text)
    {
        bannerText.text = text;
        bannerGO.SetActive(true);
        bannerGroup.alpha = 0f;
        bannerText.rectTransform.localScale = Vector3.one * 0.5f;

        yield return Tween.Animate(0.25f, Easing.BackOut, delegate(float k)
        {
            bannerGroup.alpha = k;
            bannerText.rectTransform.localScale = Vector3.one * Mathf.LerpUnclamped(0.5f, 1f, k);
        }, null, false);

        yield return Tween.Delay(0.7f, null, false);

        yield return Tween.Animate(0.2f, Easing.QuadIn, delegate(float k)
        {
            bannerGroup.alpha = 1f - k;
        }, null, false);

        bannerGO.SetActive(false);
        bannerRoutine = null;
    }

    public void ShowToast(string text)
    {
        toastText.text = text;
        toastGO.SetActive(true);
        if (toastRoutine != null) StopCoroutine(toastRoutine);
        toastRoutine = StartCoroutine(ToastRoutine());
    }

    IEnumerator ToastRoutine()
    {
        toastGroup.alpha = 0f;
        yield return Tween.Fade(toastGroup, 0f, 1f, 0.15f, Easing.QuadOut, true);
        yield return Tween.Delay(1.2f, null, true);
        yield return Tween.Fade(toastGroup, 1f, 0f, 0.4f, Easing.QuadIn, true);
        toastGO.SetActive(false);
        toastRoutine = null;
    }

    /// <summary>Static convenience for playerMotor coin pickups.</summary>
    public static void SpawnCoinPopup(string text, Vector3 worldPos)
    {
        if (Current != null) Current.CoinPopup(text, worldPos);
    }

    public void CoinPopup(string text, Vector3 worldPos)
    {
        Camera cam = Camera.main;
        if (cam == null || canvasRect == null) return;

        Vector3 sp = cam.WorldToScreenPoint(worldPos);
        if (sp.z < 0f) return;

        Vector2 local;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect, new Vector2(sp.x, sp.y), null, out local))
        {
            SpawnFloatingText(local, text, new Color(1f, 0.85f, 0.3f, 1f), 28);
        }
    }

    public void SpawnFloatingText(Vector2 pos, string text, Color color, int size)
    {
        Text t = UiFactory.CreateTextAnchored(popupLayer, "Popup", text, size,
                                              new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                                              pos, new Vector2(220f, size * 1.6f),
                                              TextAnchor.MiddleCenter, color);
        UiFactory.AddOutline(t.gameObject, new Color(0f, 0f, 0f, 0.6f));
        StartCoroutine(FloatRoutine(t));
    }

    IEnumerator FloatRoutine(Text t)
    {
        Vector2 start = t.rectTransform.anchoredPosition;
        Color c = t.color;
        float duration = 0.7f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float k = elapsed / duration;
            t.rectTransform.anchoredPosition = start + Vector2.up * (k * 46f);
            c.a = 1f - Easing.QuadIn(Mathf.Clamp01(k * 1.6f));
            t.color = c;
            yield return null;
        }
        Destroy(t.gameObject);
    }

    // ------------------------------------------------------------------
    // Countdown.
    // ------------------------------------------------------------------

    public void ShowCountdown(Action onComplete)
    {
        if (countdownGroup == null)
        {
            if (onComplete != null) onComplete();
            return;
        }
        StartCoroutine(CountdownRoutine(onComplete));
    }

    IEnumerator CountdownRoutine(Action onComplete)
    {
        Game.IsRunning = false;

        countdownGO.SetActive(true);
        countdownText.text = "GET READY";
        countdownGroup.alpha = 0f;
        countdownText.rectTransform.localScale = Vector3.one * 0.6f;

        yield return Tween.Animate(0.3f, Easing.BackOut, delegate(float k)
        {
            countdownGroup.alpha = k;
            countdownText.rectTransform.localScale = Vector3.one * Mathf.LerpUnclamped(0.6f, 1f, k);
        }, null, true);

        Sfx.CountdownTick();
        yield return Tween.Delay(0.5f, null, true);

        countdownText.text = "GO!";
        countdownText.rectTransform.localScale = Vector3.one * 0.5f;
        Sfx.Go();
        yield return Tween.Animate(0.25f, Easing.BackOut, delegate(float k)
        {
            countdownText.rectTransform.localScale = Vector3.one * Mathf.LerpUnclamped(0.5f, 1.4f, k);
        }, null, true);

        yield return Tween.Delay(0.35f, null, true);
        yield return Tween.Fade(countdownGroup, 1f, 0f, 0.2f, Easing.QuadOut, true);

        countdownGO.SetActive(false);
        Game.IsRunning = true;
        if (onComplete != null) onComplete();
    }

    // ------------------------------------------------------------------
    // Pause & mute.
    // ------------------------------------------------------------------

    public void TogglePauseUI()
    {
        if (Game.GameOver) return;
        if (Game.IsPaused) HidePause();
        else ShowPause();
    }

    public void ShowPause()
    {
        if (Game.IsPaused) return;
        Game.Pause();
        pausePanelGO.SetActive(true);
        pauseGroup.alpha = 0f;
        StartCoroutine(Tween.Fade(pauseGroup, 0f, 1f, 0.15f, Easing.QuadOut, true));
        Sfx.Pause();
    }

    public void HidePause()
    {
        if (!Game.IsPaused) return;
        Game.Resume();
        Sfx.Click();
        StartCoroutine(HidePauseRoutine());
    }

    IEnumerator HidePauseRoutine()
    {
        yield return Tween.Fade(pauseGroup, 1f, 0f, 0.12f, Easing.QuadIn, true);
        pausePanelGO.SetActive(false);
    }

    public void ToggleMute()
    {
        Game.Instance.Muted = !Game.Instance.Muted;
        RefreshMuteLabel();
        ShowToast("Sound: " + (Game.Instance.Muted ? "Off" : "On"));
        Sfx.Click();
    }

    void RefreshMuteLabel()
    {
        if (pauseMuteLabel != null)
            pauseMuteLabel.text = "Sound: " + (Game.Instance.Muted ? "Off" : "On");
    }

    // ------------------------------------------------------------------
    // Pause menu buttons.
    // ------------------------------------------------------------------

    void ResumeClicked()
    {
        HidePause();
    }

    void RestartClicked()
    {
        Sfx.Click();
        Game.Resume();
        Game.ResetRun();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void MenuClicked()
    {
        Sfx.Click();
        Game.Resume();
        SceneManager.LoadScene("Menu");
    }

    void MuteClicked()
    {
        ToggleMute();
    }
}
