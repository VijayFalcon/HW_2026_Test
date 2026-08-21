# Doofus Diaries

A small Unity C# (URP) project where "Doofus" roams a dark, disco-lit room
made of square platforms ("pulpits") that collapse over time, falling into
the pit below if caught standing on one when it goes.

## A note on the JSON, and why platforms are procedural

The assignment asks for "Character Movement and Platform placements read
from the JSON file provided." The file at `doofus_game/doofus_diary.json`
turns out to be a **config file**, not a level layout:

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
implements: platform *placement* is procedural (a grid, filled and emptied
over time) rather than a fixed list, because the data that's meant to drive
it is timing data, not coordinates. The file is bundled at
`Assets/StreamingAssets/doofus_diary.json` and loaded at runtime.

## Requirements coverage

- **Level 1 -- movement & platform placement from JSON:** `PulpitSpawner`
  spawns square tiles onto a grid -- at most two exist at once, per the
  original brief -- and each `Pulpit` self-destructs after a random
  `[min, max]` lifetime read from the JSON. `PlayerController` moves Doofus
  freely (any direction, not locked to a lane) via a Rigidbody at
  `player_data.speed`; when a tile disappears from underfoot, gravity (not
  scripted logic) makes Doofus fall.
- **Level 2 -- score on successful move:** `PlayerController.OnLandedOnNewTile`
  fires the first time Doofus's collider physically touches a tile
  different from the last one it landed on; `GameManager` forwards that to
  `ScoreManager.RegisterTileEntered`, which also tracks and persists a best
  score.
- **Level 3 -- Start / Game Over screens:** `UIManager` builds a Start
  screen (title + Start button), an in-game HUD (live score), and a Game
  Over screen (final score, best score, Restart button) entirely at
  runtime via `UIFactory`, driven by `GameManager`'s state machine
  (`Start -> Playing -> GameOver -> Playing ...`).

Difficulty levels with survival targets (Easy/Medium/Hard) and background
music are planned as follow-up passes on top of this, by request -- not yet
in this build.

## The world

- **Tiles** are 9x9 (world units) green squares laid out on a grid, matching
  the "9x9 platform" prop description. A new tile spawns adjacent to the
  most recently spawned one (never on top of the other active tile) once
  that tile has been alive for `pulpit_spawn_time` seconds -- so there's a
  brief window with two tiles active, then the older one collapses, in
  keeping with "only two Pulpits can exist simultaneously."
- **Doofus** is a cube (per the brief: "a simple cube will work too"),
  driven by a physical `Rigidbody` rather than a scripted hop -- gravity is
  what makes falling through a missing tile actually happen.
- **The room** is fully enclosed (four walls + a ceiling) with no skybox and
  near-black ambient light -- not the outdoor/sunny look of the first pass.
  The only illumination is four colored point lights that cycle hue over
  time (`DiscoLightController`), for a disco-club feel. Fog fades everything
  to black past a short distance, so falling into the gap below the tiles
  reads as falling into darkness without needing any actual pit geometry.
- **The camera** (`CameraFollow`) holds a fixed offset above and behind
  Doofus and smoothly chases him, so the player can always see tiles
  appearing ahead.

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
      CameraFollow.cs         fixed-offset chase camera
      DiscoLightController.cs cycles the room's colored point lights
    Pulpit/
      Pulpit.cs                single tile: lifetime countdown + collapse
      PulpitSpawner.cs         grid of tiles, spawns/caps them over time
    Player/
      PlayerController.cs      free Rigidbody movement, landing/fall detection
      PitVolume.cs             marker component for the "fell into the pit" trigger
    UI/
      UIFactory.cs              tiny helpers for building uGUI from code
      UIManager.cs              Start / HUD / Game Over screens
```

Each `MonoBehaviour` is single-purpose and communicates with the others
through C# events (`OnLandedOnNewTile`, `OnFell`, `OnPulpitCollapsed`,
`OnStateChanged`, `OnScoreChanged`) rather than direct references, so e.g.
the UI layer has zero knowledge of tiles/movement and vice versa.

## Why there's no hand-authored `.unity` scene

This project was built in a sandbox without a Unity Editor available to open
and test it visually. Rather than hand-write scene/prefab YAML I could not
verify, `GameBootstrap` builds the entire scene from code the moment any
scene loads (`[RuntimeInitializeOnLoadMethod]`): the room, the disco lights,
the pit trigger, the tile grid, the player, the follow camera, and the UI
canvas. This removes an entire class of "forgot to wire a reference in the
Inspector" bugs, at the cost of a plain primitive/no-art look. Swapping in
real prefabs/art later just means setting
`PulpitSpawner.PulpitPrefabTemplate` and replacing the cube in
`GameBootstrap.BuildPlayer`.

**Because I could not run the Unity Editor myself, the code has not been
compiled or play-tested by me -- it was compiled and run for the first time
on your machine.** I read every script back carefully for correctness, but
please treat each pass here as a draft to verify in-Editor, and flag
anything that doesn't behave as described.

## Setup

1. Create a new Unity project (URP template; only long-stable APIs are used,
   so most recent 2021+/6000 versions should work).
2. Copy this repo's `Assets/Scripts` and `Assets/StreamingAssets` folders
   into the new project's `Assets` folder.
3. Make sure the **Unity UI** package (`com.unity.ugui`) is installed
   (Window > Package Manager > Unity Registry > search "Unity UI") -- it
   isn't always pulled in automatically for a project that never creates a
   Canvas through the Editor's UI menu, which this one doesn't.
4. Open any scene (even the default empty `SampleScene`) and press Play.
   No manual GameObject/prefab setup is required.

## Controls

- **WASD or Arrow keys** -- move Doofus freely in any direction.
- Unity's default "Horizontal"/"Vertical" input axes are used, so both key
  sets work without extra setup.

## Edge cases handled

- Missing `doofus_diary.json` file, unreadable file, or invalid JSON -> logs
  a warning and falls back to sane defaults.
- JSON missing `player_data` / `pulpit_data` objects, or individual numeric
  fields, or fields that are zero/negative -> per-field fallback with a
  logged warning; the game never crashes on bad data.
- `min_pulpit_destroy_time > max_pulpit_destroy_time` -> the two are swapped
  with a warning instead of producing an invalid `Random.Range`.
- Every neighboring grid cell for the next tile occupied or out of bounds ->
  the occupancy rule is relaxed (with a warning) rather than leaving Doofus
  with nowhere new to go; if even that fails, the spawn is skipped instead
  of throwing.
- The tile currently under Doofus collapses -> its collider/renderer are
  disabled the same frame, so gravity takes over immediately (real physics,
  not a scripted "you fell" check).
- Restarting rebuilds the grid from a clean slate (`PulpitSpawner.ResetGrid`)
  so leftover tiles/timers from the previous run can't leak into the new one.
- `GameBootstrap` checks for an existing `GameManager`/`Camera`/`EventSystem`
  before creating its own, so it can't double up if something else already
  set up the scene.
- Wall/tile materials are recolored on whichever default material Unity
  assigns, rather than constructed with an explicit `Shader.Find("Standard")`
  -- the latter renders magenta/missing under URP, which this project uses.
- Best score persists across sessions via `PlayerPrefs` and is only
  overwritten when actually beaten.

## Commit history

Commits are split by level/pass, per the assignment's suggestion. See
`git log --oneline` for the full list; broadly: scaffolding and config
loading, Level 1 (tiles + movement), Level 2 (scoring), Level 3 (UI), then
a Level 1 rework pass (free movement, physics falling, the enclosed
disco room, and the follow camera) once the first draft was reviewed.
