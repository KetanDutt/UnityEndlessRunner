# Roadmap / Suggested improvements

Ideas for the next pass, roughly ordered from quick wins to larger projects.

## Quick wins

- **Object pooling** — coins, particles and track segments are currently created
  and destroyed each cycle. A simple pool would remove GC spikes on long runs.
- **Replace procedural UI with real sprites/fonts** — the code already
  centralises UI creation in `UiFactory` and sounds in `Sfx`, so assets can be
  swapped in without touching gameplay.
- **Animation curves** — expose squash/stretch, banner and popup timing as
  `AnimationCurve` fields instead of hard-coded easing for easier tuning.
- **Analytics & crash reporting** — track runs, deaths and high scores to inform
  balancing.

## Gameplay

- **More power-ups** — shield, double coins, slow-motion, or a "ghost" that
  skips gaps. `Powerup` already provides the spawn/collect pattern.
- **More obstacle variety** — moving spikes, low/high barriers that require a
  jump vs. a slide, or sequential lanes that force quick lane changes.
- **Missions / daily challenges** — "collect 50 coins", "run 1 km", etc., for
  replayability.
- **Character & track themes** — unlockable skins and visual themes driven by
  the existing material system.
- **Economy** — a coin wallet (persisted like the best score) to spend on skins
  and power-ups.

## Engineering

- **Rebuild the scenes in the editor** — the current scenes are binary shells;
  re-authoring `Menu.unity` and `GameScene.unity` in a supported Unity editor
  would make the project friendlier to edit (the runtime bootstrap is already
  scene-agnostic, so it can stay as a safety net).
- **Upgrade Unity** — the project targets 5.4.2f2; a newer LTS would unlock
  newer UI/particle/physics APIs. Most of the code is version-agnostic.
- **Automated tests** — add EditMode tests for `Easing`, `Tween`, `Score`
  thresholds, and the lane-clamping logic.
- **Asset validation** — a CI script that checks every `.cs` has a unique
  `.meta` GUID and that no script references an undefined tag (the two classes
  of bug fixed in this pass).
- **Addressables / AssetBundles** — if real assets are added, load them through
  addressable bundles for cleaner memory and live updates.

## Performance

- **Frame budgeting** — cap particle counts, reduce overdraw from full-screen UI,
  and merge the track's strip/rail cubes into fewer meshes.
- **Physics layers** — put coins/power-ups on a layer that only the player
  checks, and use simple distance tests (already the case) instead of triggers.
- **Batching** — share one material per track segment and use static batching
  for the (mostly static) environment.
