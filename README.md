# Doofus Diaries

A small Unity C# project where "Doofus" hops between collapsing platforms
("pulpits") arranged in a ring, scoring a point for every successful hop.

## A note on the JSON, and why platforms are procedural

The assignment asks for "Character Movement and Platform placements read
from the JSON file provided." The file at
`doofus_game/doofus_diary.json` turns out to be a **config file**, not a
level layout:

```json
{
   "player_data" : { "speed" : 3 },
   "pulpit_data" : {
     "min_pulpit_destroy_time" : 4,
     "max_pulpit_destroy_time" : 5,
     "pulpit_spawn_time" : 2.5
   }
}
```

There is no array of platform positions in it. Given the field names
(`pulpit_spawn_time`, `min/max_pulpit_destroy_time`), the intended design
reads as a procedural, endless hopper: pulpits spawn on a timer and each one
self-destructs after a random lifetime in `[min, max]` seconds, and the
player's move speed comes from `player_data.speed`. That's what this project
implements: platform *placement* is procedural (a ring of slots, filled and
emptied over time) rather than a fixed list, because the data that's meant
to drive it is timing data, not coordinates. The file is bundled at
`Assets/StreamingAssets/doofus_diary.json` and loaded at runtime.

## Requirements coverage

- **Level 1 -- movement & platform placement from JSON:** `PulpitSpawner`
  spawns pulpits into ring slots every `pulpit_spawn_time` seconds; each
  `Pulpit` self-destructs after a random `[min, max]` lifetime.
  `PlayerController` moves the player between adjacent occupied slots at
  `player_data.speed`.
- **Level 2 -- score on successful move:** `PlayerController.OnMovedToPulpit`
  fires only after a landing on a slot different from the one just left;
  `GameManager` forwards that to `ScoreManager.RegisterSuccessfulMove`,
  which also tracks and persists a best score.
- **Level 3 -- Start / Game Over screens:** `UIManager` builds a Start
  screen (title + Start button), an in-game HUD (live score), and a Game
  Over screen (final score, best score, Restart button) entirely at
  runtime via `UIFactory`, driven by `GameManager`'s state machine
  (`Start -> Playing -> GameOver -> Playing ...`).

## Project structure

```
Assets/
  StreamingAssets/
    doofus_diary.json        the provided config, read at runtime
  Scripts/
    Core/
      GameConfig.cs           raw JSON shape + validated GameConfig struct
      ConfigLoader.cs         reads/validates the JSON, falls back gracefully
      ScoreManager.cs         plain C# class: current + best score
      GameManager.cs          state machine, wires everything together
      GameBootstrap.cs        builds the whole scene at runtime (see below)
    Pulpit/
      Pulpit.cs                single platform: lifetime countdown + collapse
      PulpitSpawner.cs         ring of slots, spawns pulpits over time
    Player/
      PlayerController.cs      input, movement, fall/death detection
    UI/
      UIFactory.cs              tiny helpers for building uGUI from code
      UIManager.cs              Start / HUD / Game Over screens
```

Each `MonoBehaviour` is single-purpose and communicates with the others
through C# events (`OnMovedToPulpit`, `OnFell`, `OnPulpitCollapsed`,
`OnStateChanged`, `OnScoreChanged`) rather than direct references, so e.g.
the UI layer has zero knowledge of pulpits/movement and vice versa.

## Why there's no hand-authored `.unity` scene

This project was built in a sandbox without a Unity Editor available to open
and test it visually. Rather than hand-write scene/prefab YAML I could not
verify, `GameBootstrap` builds the entire scene from code the moment any
scene loads (`[RuntimeInitializeOnLoadMethod]`): camera, light, EventSystem,
the pulpit ring, the player capsule, and the UI canvas. This removes an
entire class of "forgot to wire a reference in the Inspector" bugs, at the
cost of a plain gray/primitive look rather than authored art. Swapping in
real prefabs/art later just means setting `PulpitSpawner.PulpitPrefabTemplate`
and replacing the capsule in `GameBootstrap`.

**Because I could not run the Unity Editor myself, the code has not been
compiled or play-tested. I read every script back carefully for
correctness, but please treat first launch as the first real test and check
the Console for warnings/errors.**

## Setup

1. Create a new Unity project (3D template; developed against 2022.3 LTS
   conventions, but it only uses long-stable APIs, so most recent 2021+/6000
   versions should work).
2. Copy this repo's `Assets/Scripts` and `Assets/StreamingAssets` folders
   into the new project's `Assets` folder.
3. Open any scene (even the default empty `SampleScene`) and press Play.
   No manual GameObject/prefab setup is required.

## Controls

- **Left Arrow / A** -- hop to the pulpit on the left, if one is there.
- **Right Arrow / D** -- hop to the pulpit on the right, if one is there.
- Moving toward an empty slot is simply ignored (no crash, no penalty).

## Edge cases handled

- Missing `doofus_diary.json` file, unreadable file, or invalid JSON -> logs
  a warning and falls back to sane defaults.
- JSON missing `player_data` / `pulpit_data` objects, or individual numeric
  fields, or fields that are zero/negative -> per-field fallback with a
  logged warning; the game never crashes on bad data.
- `min_pulpit_destroy_time > max_pulpit_destroy_time` -> the two are swapped
  with a warning instead of producing an invalid `Random.Range`.
- Player presses a direction with no pulpit in that slot -> move is silently
  rejected.
- Rapid/duplicate input while already mid-hop -> ignored until landing.
- The destination pulpit collapses while the player is mid-flight to it ->
  counted as a fall (game over), not a silent teleport-to-nowhere.
- The pulpit currently under the player collapses while they're standing
  still on it -> game over; but a pulpit collapsing *after* the player has
  already left it is not fatal.
- Restarting rebuilds the ring from a clean slate (`PulpitSpawner.ResetRing`)
  so leftover pulpits/timers from the previous run can't leak into the new
  one.
- `GameBootstrap` checks for an existing `GameManager`/`Camera`/`Light`/
  `EventSystem` before creating its own, so it can't double up if something
  else already set up the scene.
- Best score persists across sessions via `PlayerPrefs` and is only
  overwritten when actually beaten.

## Commit history

Commits are split by level, per the assignment's suggestion:
1. Project scaffolding + config loading (JSON parsing/validation).
2. Level 1 -- pulpit spawning/lifecycle + player movement.
3. Level 2 -- score manager wired to successful moves.
4. Level 3 -- Start/HUD/Game Over UI + runtime bootstrap.
