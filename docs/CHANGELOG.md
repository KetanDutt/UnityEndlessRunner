# Changelog

A record of the bugs fixed, improvements made and features added in this pass.

## Bug fixes

- **`CoinFactory` — undefined "Coin" tag (crash).** Coins were assigned
  `coin.tag = "Coin"`, but no `Coin` tag exists in the (binary) `TagManager`,
  which throws a `UnityException` at runtime. The tag assignment was removed and
  coins are now tracked through a static `CoinSpin.Active` list.
- **`cameraMotor` — wrong `Mathf.Clamp` argument order.** The follow code used
  `Mathf.Clamp(0, 7, 10)`, which always returns the middle value `7`, so the
  camera never actually followed the player vertically. The clamp now correctly
  clamps the player's y position.
- **`cameraMotor` — UTF-16 encoding.** The file was the only UTF-16 source in the
  project (appearing as mojibake in most tools); it was rewritten as UTF-8.
- **`playerMotor` — destroying its own `CharacterController`.** The cleanup pass
  removed all `Collider` components, but a `CharacterController` *is* a `Collider`,
  so movement would have broken (or thrown). The controller is now excluded.
- **`playerMotor` — scaling/rotating a `CharacterController`.** Unity forbids
  scaling or rotating a `CharacterController`'s transform; squash/stretch and lean
  previously applied to the root. Visual polish now applies to a child `Visual`.
- **`Score` — level-ups could be skipped.** The old `if (score >= level * N)` only
  advanced one level per call; a large score gain could skip levels. It now loops
  until the threshold is fully caught up.
- **`Score` / `DeathMenu` — fragile text lookup.** End-of-run and best-score text
  are now wired reliably (falling back to built UI), and a new-best flag is
  tracked explicitly instead of re-deriving it.

## New features

- **Magnet power-up** (`Powerup.cs`) — pulls nearby coins in; spawned by
  `baseManager` and collected via a proximity check.
- **In-game HUD & feedback layer** (`GameUI.cs`) — score/coins/speed, countdown,
  level banners, floating coin popups, toasts, and a full pause menu.
- **UI factory extensions** (`UiFactory.cs`) — anchored text, styled buttons with
  pressed/highlight states, panels, shadows and outlines.
- **Tween helper** (`Tween.cs`) — reusable, easing-driven, pause-safe coroutine
  animations.

## Improvements

- **Track generation** (`baseManager`) — difficulty ramps with distance, shared
  runtime materials (fewer allocations), lane dividers/rails, start gate, and
  smarter placement (coins and power-ups avoid spike lanes).
- **SFX** (`Sfx.cs`) — richer procedural sounds, clip caching, scheduled
  arpeggios, and preference-gated haptics.
- **VFX** (`Vfx.cs`) — more varied bursts with gravity, sparkles and confetti.
- **Camera** (`cameraMotor.cs`) — speed-based FOV, smoother damping, screen shake.
- **State management** (`Game.cs`) — a persistent singleton owning settings and
  per-run state, with safe reset/pause/resume.
- **Movement** — jump buffering, coyote time, variable jump height, lane switching
  via `CharacterController.Move` (no transform desync).

## Cleanup

- Removed tracked generated artifacts: `Library/` (~79 MB), `android.apk` (~19 MB),
  `Assembly-CSharp*.csproj`, `sky.sln`, `sky.userprefs`.
- Removed unused assets with no scene/script references: `Assets/prefabs2/`
  (legacy prefabs + `Vehicle_3` FBX, ~14 MB) and `Assets/skybox/` (~12 MB).
- Added a root `.gitignore` covering Unity/IDE/build/OS artifacts.
- Added this documentation set and an updated `README.md`.
