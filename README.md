# Unity Endless Runner

A polished, self-contained 3D endless runner built in **Unity 5.4.2f2**. Run down a
procedurally generated three-lane track, jump gaps and spike strips, collect
coins, grab magnet power-ups and chase your high score — with no external art or
audio assets required. Everything (player, track, UI, sound, particles) is built
from code at runtime.

## Quick start

1. Open the project folder in **Unity 5.4.2f2** (or a newer 5.x release).
2. Open `Assets/scenes/Menu.unity` and press Play.
3. Click **Play** (or press **Play**) to start a run.

> The game bootstraps itself on scene load — no scene wiring or prefab setup is
> required. See `docs/ARCHITECTURE.md` for how this works.

## Controls

| Action             | Keyboard             | Touch                |
| ------------------ | -------------------- | -------------------- |
| Jump               | Space / ↑ / W / mouse| Swipe up / tap & hold |
| Switch lane left   | ← / A                | Tap left side / swipe left |
| Switch lane right  | → / D                | Tap right side / swipe right |
| Pause / resume     | Esc / P              | Pause button (top-right) |
| Mute / unmute      | M                    | Pause menu button    |

## Features

- Procedurally generated track with **gaps**, **spikes**, **coins** and **magnet power-ups**
- Progressive difficulty that ramps up with distance
- Feel-good movement: **jump buffering**, **coyote time**, **variable jump height**, **squash & stretch**
- Score, coins, speed readout, **level-ups**, and persistent **best score**
- Smooth **tween-based UI** — countdown, level banners, floating coin popups, pause menu
- Procedural **sound effects** and **haptics** (no audio files)
- Procedural **particle effects** — dust, coin sparkles, level-up bursts, confetti
- Mobile-friendly: swipe/tap controls and vibration toggle

## Documentation

| Document | Contents |
| -------- | -------- |
| [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md) | How the game boots and a reference for every script |
| [`docs/FEATURES.md`](docs/FEATURES.md)         | Feature walkthrough and gameplay details |
| [`docs/CHANGELOG.md`](docs/CHANGELOG.md)       | Bug fixes and improvements in this pass |
| [`docs/ROADMAP.md`](docs/ROADMAP.md)           | Suggested future improvements |

## Project layout

```
Assets/
  scenes/        Menu.unity, GameScene.unity (binary-serialized scene shells)
  scripts/       All gameplay code (19 C# scripts)
  materials/     base / player / pyramid materials
ProjectSettings/ Unity project settings (binary serialized)
docs/            Documentation
```

## Notes

- The `Library/`, build outputs and IDE project files are generated and ignored
  by git (see `.gitignore`); Unity regenerates them on open.
- Scripts require no scene references: `GameBootstrap` wires everything up
  automatically on scene load.

## License

See [LICENSE](LICENSE).
