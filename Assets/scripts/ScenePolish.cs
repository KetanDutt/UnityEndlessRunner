using UnityEngine;

/// <summary>
/// Small runtime polish pass: gives the game a pleasant sky colour, ambient
/// light, and gentle distance fog so it looks intentional even without any
/// imported skybox assets. Everything is procedural and safe to call repeatedly.
/// </summary>
public static class ScenePolish
{
    private static bool applied;

    public static void Apply()
    {
        if (applied) return;
        applied = true;

        // Soft sky colour instead of the default dark grey.
        RenderSettings.skybox = null;
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.45f, 0.55f, 0.70f);
        RenderSettings.ambientEquatorColor = new Color(0.28f, 0.32f, 0.42f);
        RenderSettings.ambientGroundColor = new Color(0.10f, 0.11f, 0.15f);
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogColor = new Color(0.45f, 0.55f, 0.70f);
        RenderSettings.fogDensity = 0.008f;

        Camera cam = Camera.main;
        if (cam != null)
        {
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.45f, 0.55f, 0.70f);
            cam.farClipPlane = 120f;
        }
    }
}
