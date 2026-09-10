# Architecture

This document explains how the game boots, how a run works, and what each script
does.

## The big idea: self-bootstrapping runtime

The project's scene files (`Menu.unity`, `GameScene.unity`) are binary-serialized
and carry **no script references**. Instead of relying on scene wiring, the game
rebuilds everything it needs at runtime:

```
GameBootstrap (RuntimeInitializeOnLoadMethod, AfterSceneLoad)
    └─ Game.Instance                  ← persistent settings object (DontDestroyOnLoad)
    └─ Game.ResetRun()                ← fresh per-run state
    └─ scene name contains "menu" ?
         ├─ yes → create mainmenu     ← wires the menu UI
         └─ no  → create PlayerSpawner ← builds the whole game scene
```

`PlayerSpawner.Awake()` then constructs, in order:

1. `Game.ResetRun()` and neutralises any stray `Enemy` colliders left in the scene.
2. **Player** — a capsule primitive with a `CharacterController` and a `playerMotor`.
3. **Camera** — a follow camera with a `cameraMotor` and an `AudioListener`.
4. **Death plane** — a kill volume that follows the player below the track.
5. **Directional light**, **EventSystem** (for UI clicks), and the managers:
   `baseManager`, `Score`, `DeathMenu`, `GameUI`.
6. `ScenePolish.Apply()` — sky colour, ambient light, fog.
7. `StartCountdown()` — freezes the run and plays the 3-2-1-style countdown.

Because everything is built from primitives and code, the game never depends on
the legacy prefab assets (which have been removed).

## Run flow

1. **Countdown** — `Game.IsRunning = false`; the player settles onto the track,
   then "GO!" sets `IsRunning = true`.
2. **Running** — `playerMotor` moves forward, `baseManager` spawns and recycles
   track segments ahead of/behind the player, `Score` accumulates distance and
   coin points, and difficulty ramps with distance.
3. **Level ups** — every `pointsPerLevel` points the speed cap increases and a
   level banner + burst plays.
4. **Death** — hitting a spike (`Enemy` tag) or falling triggers
   `playerMotor.Death()`, which finalises the score, updates the best score, and
   shows the `DeathMenu` (restart / menu).
5. **Pause** — `Esc`/`P` (or the button) pauses via `Game.Pause()` (`timeScale = 0`);
   UI animations use unscaled time so the menu still animates.

## Key patterns

- **Static singletons** (`Game`, `Score.Current`, `GameUI.Current`,
  `playerMotor.Player`) let systems find each other without scene wiring.
- **Static active lists** (`CoinSpin.Active`, `Powerup.Active`) give the player a
  cheap proximity pickup check — more reliable than trigger callbacks against a
  `CharacterController`.
- **Centralized effects** — `Sfx` and `Vfx` are static facades, so real audio/art
  assets can be dropped in later without touching gameplay code.
- **Tweens** — `Tween` + `Easing` drive all UI animation consistently, with an
  `unscaled` flag for pause-safe animation.
- **Visual separation** — the player's root object holds only the
  `CharacterController` (Unity forbids scaling/rotating its transform); all
  squash/stretch/lean goes on a child `Visual`.

## Script reference

| Script | Responsibility |
| ------ | -------------- |
| `GameBootstrap` | Static entry point; decides scene and spawns `mainmenu` or `PlayerSpawner`. |
| `Game` | Persistent state & settings: best score, mute/vibration prefs, per-run state (speed, running, pause, magnet). Saved via `PlayerPrefs`. |
| `PlayerSpawner` | Builds the entire gameplay scene at runtime (player, camera, hazards, managers, UI). |
| `playerMotor` | Player controller: forward acceleration, jump (buffering, coyote, variable height), lane switching, squash & stretch, death detection. |
| `cameraMotor` | Smooth follow camera, speed-based FOV, and screen shake. |
| `baseManager` | Procedurally spawns/recycles track segments with gaps, spikes, coins and power-ups; ramps difficulty with distance. |
| `Score` | Score/coins/level tracking, level-ups, best-score finalisation, UI updates. |
| `DeathMenu` | Death screen: final score, best score, restart / menu buttons. |
| `GameUI` | In-game HUD and feedback: score/coins/speed, countdown, level banners, floating coin popups, pause menu. |
| `UiFactory` | Helpers that create canvas/text/buttons/panels/outlines from code. |
| `mainmenu` | Main menu: Play button + best-score text. |
| `CoinFactory` / `CoinSpin` | Builds the coin primitive; spins, bobs, and magnet-pulls coins; registers them in `CoinSpin.Active`. |
| `Powerup` | The magnet power-up orb: spins, bobs, registers in `Powerup.Active`. |
| `Sfx` | Procedural, cached sound effects and haptics. |
| `Vfx` | Procedural particle effects (dust, sparkles, bursts, confetti). |
| `Tween` | Coroutine tween helpers (move, scale, fade, delay) with easing. |
| `Easing` | Easing function library. |
| `ScenePolish` | One-shot sky/ambient/fog/camera polish pass. |
| `DeathPlane` | Kill volume below the track; robust fallback height check. |
