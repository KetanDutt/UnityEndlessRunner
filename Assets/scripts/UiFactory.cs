using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Helpers that create a complete UI (canvas, text, buttons) from code. Used as
/// a fallback when the scene does not already contain the expected UI elements,
/// so the game remains playable no matter how the scene was authored.
/// </summary>
public static class UiFactory
{
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

    public static Text CreateText(Transform parent, string name, string text, int fontSize, Vector2 anchoredPosition, TextAnchor alignment)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);

        RectTransform rt = go.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(600f, fontSize * 2f);
        rt.anchoredPosition = anchoredPosition;

        Text label = go.AddComponent<Text>();
        label.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        label.text = text;
        label.fontSize = fontSize;
        label.alignment = alignment;
        label.color = Color.white;
        return label;
    }

    public static Button CreateButton(Transform parent, string name, string label, UnityEngine.Events.UnityAction onClick)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);

        RectTransform rt = go.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(240f, 64f);

        Image image = go.AddComponent<Image>();
        image.color = new Color(0.15f, 0.45f, 0.85f, 1f);

        Button button = go.AddComponent<Button>();
        button.targetGraphic = image;
        if (onClick != null) button.onClick.AddListener(onClick);

        GameObject textGo = new GameObject("Text");
        textGo.transform.SetParent(go.transform, false);
        RectTransform trt = textGo.AddComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = Vector2.zero;
        trt.offsetMax = Vector2.zero;

        Text text = textGo.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.text = label;
        text.fontSize = 28;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;

        return button;
    }
}
