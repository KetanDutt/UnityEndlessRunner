using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Helpers that create a complete UI (canvas, text, buttons, panels) from code.
/// Used as a fallback when a scene does not already contain the expected UI
/// elements, so the game remains playable no matter how the scene was authored.
/// </summary>
public static class UiFactory
{
    public static Font DefaultFont
    {
        get { return Resources.GetBuiltinResource<Font>("Arial.ttf"); }
    }

    public static Canvas CreateCanvas(string name)
    {
        GameObject go = new GameObject(name);
        Canvas canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = go.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(800f, 600f);

        go.AddComponent<GraphicRaycaster>();
        return canvas;
    }

    public static Text CreateText(Transform parent, string name, string text, int fontSize,
                                  Vector2 anchoredPosition, TextAnchor alignment)
    {
        return CreateTextAnchored(parent, name, text, fontSize,
                                  new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                                  new Vector2(0.5f, 0.5f), anchoredPosition,
                                  new Vector2(600f, fontSize * 2f), alignment, Color.white);
    }

    public static Text CreateTextAnchored(Transform parent, string name, string text, int fontSize,
                                          Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
                                          Vector2 anchoredPosition, Vector2 sizeDelta,
                                          TextAnchor alignment, Color color)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);

        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = pivot;
        rt.anchoredPosition = anchoredPosition;
        rt.sizeDelta = sizeDelta;

        Text label = go.AddComponent<Text>();
        label.font = DefaultFont;
        label.text = text;
        label.fontSize = fontSize;
        label.alignment = alignment;
        label.color = color;
        label.horizontalOverflow = HorizontalWrapMode.Overflow;
        label.verticalOverflow = VerticalWrapMode.Overflow;
        return label;
    }

    public static Button CreateButton(Transform parent, string name, string label,
                                      UnityEngine.Events.UnityAction onClick)
    {
        return CreateButtonStyled(parent, name, label, onClick,
                                  new Color(0.15f, 0.45f, 0.85f, 1f), new Vector2(240f, 64f));
    }

    public static Button CreateButtonStyled(Transform parent, string name, string label,
                                            UnityEngine.Events.UnityAction onClick,
                                            Color color, Vector2 size)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);

        RectTransform rt = go.AddComponent<RectTransform>();
        rt.sizeDelta = size;

        Image image = go.AddComponent<Image>();
        image.color = color;

        Button button = go.AddComponent<Button>();
        button.targetGraphic = image;

        // Pleasant pressed/highlighted states.
        ColorBlock cb = button.colors;
        cb.normalColor = color;
        cb.highlightedColor = Color.Lerp(color, Color.white, 0.18f);
        cb.pressedColor = Color.Lerp(color, Color.black, 0.25f);
        cb.colorMultiplier = 1f;
        cb.fadeDuration = 0.08f;
        button.colors = cb;

        if (onClick != null) button.onClick.AddListener(onClick);

        GameObject textGo = new GameObject("Text");
        textGo.transform.SetParent(go.transform, false);
        RectTransform trt = textGo.AddComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = Vector2.zero;
        trt.offsetMax = Vector2.zero;

        Text text = textGo.AddComponent<Text>();
        text.font = DefaultFont;
        text.text = label;
        text.fontSize = 28;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;

        return button;
    }

    /// <summary>Creates a plain coloured panel that fills its RectTransform.</summary>
    public static Image CreatePanel(Transform parent, string name, Color color)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);

        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        Image img = go.AddComponent<Image>();
        img.color = color;
        return img;
    }

    /// <summary>Adds a drop shadow for legibility on bright backgrounds.</summary>
    public static void AddShadow(GameObject go, Color color)
    {
        Shadow shadow = go.GetComponent<Shadow>();
        if (shadow == null) shadow = go.AddComponent<Shadow>();
        shadow.effectColor = color;
        shadow.effectDistance = new Vector2(1.5f, -1.5f);
    }

    public static void AddOutline(GameObject go, Color color)
    {
        Outline outline = go.GetComponent<Outline>();
        if (outline == null) outline = go.AddComponent<Outline>();
        outline.effectColor = color;
        outline.effectDistance = new Vector2(1.5f, -1.5f);
    }
}
