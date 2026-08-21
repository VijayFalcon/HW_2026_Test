# Doofus Diaries

Created by Vijay Aravindh Srinivasan.

A small Unity C# (URP) project where Doofus, a cube, roams a dark, disco
lit room made of square platforms called Pulpits. Each Pulpit collapses
after a countdown, and Doofus falls into the pit below if he is still
standing on one when it goes.

## The game

Doofus starts on a Pulpit in the middle of an enclosed room. There is no
sunlight and no skybox, just four colored point lights cycling through
hues for a disco feel, and fog that swallows everything a short distance
out so falling into the gap below the tiles reads as falling into
darkness.

Only two Pulpits ever exist at the same time. A new one spawns adjacent
to the most recently spawned one (never in the same spot, and never on
top of the other active tile) once that tile has been alive for a set
number of seconds. Each Pulpit then has its own countdown before it
collapses. When a Pulpit disappears from under Doofus, real physics
(gravity through a Rigidbody, not a scripted check) takes over and he
drops into the pit.

## Rules

1. Move Doofus with WASD or the arrow keys. Movement is free in any
   direction, not locked to a single lane.
2. Walk onto a new Pulpit before it collapses to score a point.
3. If Doofus is standing on a Pulpit when it collapses, or he walks off
   the edge of one, he falls and the run ends immediately.
4. Reach the score target for the selected difficulty before that
   happens, and the run ends in a win instead.

## Difficulty

Three difficulty levels can be chosen from the Start screen, each with
its own survival target:

- Easy, target 50 Pulpits.
- Medium, target 100 Pulpits.
- Hard, target 200 Pulpits.

Crossing the target for the selected difficulty while still alive ends
the run as a win. Falling before reaching it ends the run as a game
over. Both outcomes show the final score and the best score reached so
far, which is saved between sessions.

## UI

- **Start screen.** Title, a short instruction line, three difficulty
  buttons (Easy, Medium, Hard) that highlight whichever one is currently
  selected, and a Start button.
- **HUD.** A running score counter anchored to the top of the screen,
  showing the current score against the target for the selected
  difficulty, updated live every time Doofus lands on a new Pulpit.
- **End screen.** Shown on both game over and win, with the final score,
  the best score, and a Restart button that resets everything and drops
  Doofus back onto a fresh Pulpit.

All of the UI is built from code at runtime rather than hand laid out in
the Editor, so there is no separate scene file to keep in sync with the
scripts.

## Music

A looping soundtrack starts the moment a run begins (pressing Start, or
Restart after a run ends) and plays for as long as the run continues. It
is loaded automatically from `Assets/Resources/Audio/Soundtrack.mp3`, so
adding your own track just means dropping an mp3 in with that exact name,
no Inspector wiring needed. If the file is not present, the game runs
normally, just without music, instead of throwing an error.

## Camera

The camera holds a fixed offset above and behind Doofus and smoothly
chases him as he moves, so upcoming Pulpits are always visible before he
needs to jump to them.

## A note on the JSON, and why platforms are procedural

The assignment asks for character movement and platform placement to be
read from the JSON file provided. The file at
`doofus_game/doofus_diary.json` turns out to be a config file, not a
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
reads as a procedural, endless hopper. Pulpits spawn on a timer and each
one self destructs after a random lifetime in `[min, max]` seconds, and
the player's move speed comes from `player_data.speed`. That is what this
project implements. Platform placement is procedural, a grid filled and
emptied over time, rather than a fixed list, because the data that is
meant to drive it is timing data, not coordinates. The file is bundled at
`Assets/StreamingAssets/doofus_diary.json` and loaded at runtime.

## Requirements coverage

- **Level 1, movement and platform placement from JSON.** `PulpitSpawner`
  spawns square tiles onto a grid, at most two exist at once, per the
  original brief, and each `Pulpit` self destructs after a random
  `[min, max]` lifetime read from the JSON. `PlayerController` moves
  Doofus freely (any direction, not locked to a lane) via a Rigidbody at
  `player_data.speed`. When a tile disappears from underfoot, gravity
  (not scripted logic) makes Doofus fall.
- **Level 2, score on successful move, difficulty targets.**
  `PlayerController.OnLandedOnNewTile` fires the first time Doofus's
  collider physically touches a tile different from the last one it
  landed on. `GameManager` forwards that to
  `ScoreManager.RegisterTileEntered`, which also tracks and persists a
  best score. `Difficulty`/`DifficultyTargets` define the three survival
  targets (Easy 50, Medium 100, Hard 200). `GameManager.SelectedDifficulty`
  determines `TargetScore`, and reaching it while `Playing` transitions to
  a new `Won` state, distinct from `GameOver`.
- **Level 3, Start and end of run screens, difficulty picker.**
  `UIManager` builds a Start screen with a difficulty picker (Easy,
  Medium, Hard buttons that set `GameManager.SelectedDifficulty`,
  highlighting whichever is currently selected) and a Start button, an
  in-game HUD (live score against target), and a shared end-of-run screen
  (title, final score, best score, Restart button) entirely at runtime
  via `UIFactory`, driven by `GameManager`'s state machine
  (`Start -> Playing -> GameOver/Won -> Playing ...`).
- **Background music.** `MusicController` plays a looping soundtrack the
  moment a run actually starts (`GameState.Playing`), restarting cleanly
  on both the first Start and any Restart. It loads the clip via
  `Resources.Load<AudioClip>("Audio/Soundtrack")`, so it is picked up
  automatically with zero Inspector wiring, see **Music** above. If no
  clip is present, the game just runs silently instead of throwing.

Still open: a distinct, polished win screen instead of the shared
end-of-run panel with a swapped title.

## Project structure

```
Assets/
  StreamingAssets/
    doofus_diary.json        the provided config, read at runtime
  Resources/
    Audio/
      Soundtrack.mp3          your soundtrack goes here (not bundled)
  Scripts/
    Core/
      GameConfig.cs           raw JSON shape + validated GameConfig struct
      ConfigLoader.cs         reads/validates the JSON, falls back gracefully
      ScoreManager.cs         plain C# class: current + best score
      GameManager.cs          state machine, wires everything together
      GameBootstrap.cs        builds the whole scene at runtime (see below)
      CameraFollow.cs         fixed-offset chase camera
      DiscoLightController.cs cycles the room's colored point lights
      MusicController.cs      loops the soundtrack once a run starts
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

Each `MonoBehaviour` is single purpose and communicates with the others
through C# events (`OnLandedOnNewTile`, `OnFell`, `OnPulpitCollapsed`,
`OnStateChanged`, `OnScoreChanged`) rather than direct references, so for
example the UI layer has zero knowledge of tiles or movement and vice
versa.

## Why there's no hand-authored `.unity` scene

This project was built in a sandbox without a Unity Editor available to
open and test it visually. Rather than hand-write scene or prefab YAML I
could not verify, `GameBootstrap` builds the entire scene from code the
moment any scene loads (`[RuntimeInitializeOnLoadMethod]`), the room, the
disco lights, the pit trigger, the tile grid, the player, the follow
camera, and the UI canvas. This removes an entire class of "forgot to
wire a reference in the Inspector" bugs, at the cost of a plain
primitive, no-art look. Swapping in real prefabs or art later just means
setting `PulpitSpawner.PulpitPrefabTemplate` and replacing the cube in
`GameBootstrap.BuildPlayer`.

Because I could not run the Unity Editor myself, the code was compiled
and play tested for the first time on my own machine after building it
this way, not in the sandbox it was written in. I read every script back
carefully for correctness, but please treat each pass here as a draft to
verify in-Editor, and flag anything that does not behave as described.

## Setup

1. Create a new Unity project (URP template, only long-stable APIs are
   used, so most recent 2021+/6000 versions should work).
2. Copy this repo's `Assets/Scripts`, `Assets/StreamingAssets`, and
   `Assets/Resources` folders into the new project's `Assets` folder.
3. Make sure the Unity UI package (`com.unity.ugui`) is installed
   (Window > Package Manager > Unity Registry > search "Unity UI"). It
   is not always pulled in automatically for a project that never
   creates a Canvas through the Editor's UI menu, which this one
   doesn't.
4. Open any scene (even the default empty `SampleScene`) and press Play.
   No manual GameObject or prefab setup is required.

## Controls

- WASD or arrow keys move Doofus freely in any direction.
- Unity's default "Horizontal"/"Vertical" input axes are used, so both
  key sets work without extra setup.

## Edge cases handled

- Missing `doofus_diary.json` file, unreadable file, or invalid JSON
  logs a warning and falls back to sane defaults.
- JSON missing `player_data` or `pulpit_data` objects, or individual
  numeric fields, or fields that are zero or negative, falls back per
  field with a logged warning; the game never crashes on bad data.
- `min_pulpit_destroy_time` greater than `max_pulpit_destroy_time`, the
  two are swapped with a warning instead of producing an invalid
  `Random.Range`.
- Every neighboring grid cell for the next tile occupied or out of
  bounds, the occupancy rule is relaxed (with a warning) rather than
  leaving Doofus with nowhere new to go. If even that fails, the spawn
  is skipped instead of throwing.
- The tile currently under Doofus collapses, its collider and renderer
  are disabled the same frame, so gravity takes over immediately (real
  physics, not a scripted "you fell" check).
- Restarting rebuilds the grid from a clean slate
  (`PulpitSpawner.ResetGrid`) so leftover tiles or timers from the
  previous run cannot leak into the new one.
- `GameBootstrap` checks for an existing `GameManager`, `Camera`, or
  `EventSystem` before creating its own, so it cannot double up if
  something else already set up the scene.
- Wall and tile materials are recolored on whichever default material
  Unity assigns, rather than constructed with an explicit
  `Shader.Find("Standard")`, which renders magenta or missing under
  URP, which this project uses.
- Best score persists across sessions via `PlayerPrefs` and is only
  overwritten when actually beaten.
- Missing soundtrack file, the game runs silently instead of throwing,
  see **Music** above.

## Commit history

Commits are split by level and pass, per the assignment's suggestion. See
`git log --oneline` for the full list. Broadly: scaffolding and config
loading, Level 1 (tiles and movement), Level 2 (scoring), Level 3 (UI),
a Level 1 rework pass (free movement, physics falling, the enclosed disco
room, and the follow camera), a HUD anchoring fix, and finally the
background music pass.
