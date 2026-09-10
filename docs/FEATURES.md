# Features

A walkthrough of the gameplay and polish systems.

## Core gameplay

- **Three-lane running** — the player runs forward automatically; the track is
  divided into three lanes (`Game.Lanes`, `Game.LaneWidth`) with visual lane
  dividers and edge rails.
- **Obstacles** — full-width **gaps** to jump and per-lane **spike strips** that
  kill on contact.
- **Pickups** — spinning **coins** (5 points each) and a **magnet power-up** that
  pulls nearby coins toward the player for a few seconds.
- **Progressive difficulty** — gap and spike frequency ramp from low to high over
  `rampDistance` (600 m) so the game starts easy and gets intense.

## Movement feel

- **Jump buffering** — pressing jump slightly before landing still triggers a jump.
- **Coyote time** — a short grace window lets you jump just after running off a ledge.
- **Variable jump height** — releasing the jump early cancels part of the ascent.
- **Squash & stretch** — the visual child squashes on landing and stretches while
  rising, with lane lean and a subtle run bob.
- **Speed & FOV** — the camera widens its field of view as speed increases,
  selling the sense of acceleration.

## Scoring & progression

- **Distance score** — 1 point per metre travelled.
- **Coin score** — 5 points per coin.
- **Level-ups** — every 25 points the speed cap increases (up to a max) and a
  "LEVEL n!" banner with a particle burst plays.
- **Best score** — saved to `PlayerPrefs`; a new best triggers confetti and a
  "NEW BEST!" callout on the death screen.

## UI & feedback

- **HUD** — score (top), coin count (top-left), speed (bottom-left), pause button
  (top-right).
- **Countdown** — "GET READY → GO!" with sound before each run.
- **Level banners** — animated centre-screen banners with a black outline for
  legibility.
- **Floating coin popups** — "+5" text floats up from each collected coin.
- **Pause menu** — Resume / Restart / Menu / Sound toggle, with a dimmed backdrop
  and tweened fade.
- **Death screen** — final score, coins, best score, and Restart / Menu buttons.

## Audio & haptics

- All sound effects are **synthesised at runtime** with `AudioClip.Create` and
  cached — no audio assets needed.
- Sounds cover jump, land, coin, lane change, level-up, death, countdown, power-up,
  new best, pause and UI clicks.
- **Haptics** fire on land, death, level-up and power-up on mobile, gated by a
  vibration preference.

## Visual effects

- Procedural `ParticleSystem` bursts: jump/land dust, coin sparkles, level-up
  bursts, death explosion, magnet burst and multi-coloured confetti.
- A `ScenePolish` pass sets sky colour, ambient light, fog and camera clear flags
  so the game looks intentional without a skybox.

## Settings

- **Mute** (`M` or pause menu) and **vibration** toggles persist across sessions
  via `PlayerPrefs`.
