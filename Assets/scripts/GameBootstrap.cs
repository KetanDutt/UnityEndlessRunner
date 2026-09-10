using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Entry point that runs automatically after every scene load (no scene wiring
/// required). It decides which scene just loaded and bootstraps it:
///   * Menu     -> wires the Play button and best-score text,
///   * GameScene -> creates the PlayerSpawner that builds the gameplay objects.
/// </summary>
public static class GameBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void OnSceneLoaded()
    {
        // Ensure the persistent Game settings object exists (loads best score).
        var game = Game.Instance;

        Game.ResetRun();

        string scene = SceneManager.GetActiveScene().name;
        if (string.IsNullOrEmpty(scene)) scene = "";

        if (scene.IndexOf("menu", System.StringComparison.OrdinalIgnoreCase) >= 0)
            SetupMenu();
        else
            SetupGame();
    }

    static void SetupMenu()
    {
        GameObject host = new GameObject("MainMenu");
        host.AddComponent<mainmenu>();
    }

    static void SetupGame()
    {
        if (Object.FindObjectOfType<PlayerSpawner>() != null) return;

        GameObject host = new GameObject("GameManager");
        host.AddComponent<PlayerSpawner>();
    }
}
