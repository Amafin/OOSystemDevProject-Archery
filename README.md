# Arena Archer — VR Archery Challenge

A virtual reality archery game for **Meta Quest 3**, built with **Unity 6** and the **XR Interaction Toolkit**.
The player stands on a platform in the middle of a Roman arena, shoots at fixed and flying targets, fights off gladiators and unlocks better bows and arrows level after level.

---

## Table of contents
1. [Features](#features)
2. [Controls](#controls)
3. [Game rules](#game-rules)
4. [Installation](#installation)
5. [Project structure](#project-structure)
6. [Save system](#save-system)

---

## Features
- **Natural archery gesture**: grab the bow, take an arrow from the quiver over your shoulder, nock it on the string, draw and release.
- **Physics-based trajectory**: the arrow speed depends on how far the string is drawn, the bow and the arrow type, and the arrow is affected by gravity.
- **4 bows and 4 arrow types** with different stats, unlocked through progression.
- **4 levels** with different content: fixed targets, flying targets, gladiators, random target sizes.
- **3 difficulties** (Easy, Normal, Hard) plus a **Training** mode with no timer and no progression.
- **Score zones** on targets (10 points in the centre, fewer towards the edge) and a real-time score gauge with tiers.
- **Gladiators** that walk down from the gates and attack the player, who has a health bar.
- **Aim help** (optional, toggled in the menu): a dotted trail showing where the arrow will go.
- **Virtual keyboard** to enter the player's name.
- **Leaderboard per level**, saved in a JSON file.
- **VR menu** on a lectern: level, difficulty, bow, arrow, volume, aim help, reset progress.

---

## Controls
Default bindings with Meta Quest Touch controllers:

- **Grab the bow**: hold **Grip** near the bow.
- **Take an arrow**: move the free hand **behind your head** (the quiver) and hold **Grip**.
- **Nock the arrow**: bring the back of the arrow close to the string.
- **Draw**: keep **Grip** pressed and pull the string back.
- **Shoot**: release **Grip**.
- **Use the menu**: press the buttons with the tip of the controller (the small grey poke point), like pressing a physical button.
- **Turn**: right joystick
- **Recenter the view**: hold the **Meta** button.

---

## Game rules

### Levels: what is in the arena
Defined in `LevelData` assets (`Assets/Asset/Levels`).

- **Level 1, Initiation**: 4 fixed targets, 1 gladiator.
- **Level 2, Moving Targets**: 3 fixed targets, 2 flying targets, 1 gladiator, random target sizes (×0.5 to ×1.2).
- **Level 3, Gladiators**: 3 fixed targets, 2 flying targets, 1 gladiator, smaller random target sizes (×0.5 to ×1).
- **Level 4, Arena Champion**: 2 fixed targets, 3 flying targets, 2 gladiators at the same time, the smallest random target sizes (×0.4 to ×1).
- **Training**: 4 fixed targets, no gladiators, no timer, no progression.

Random sizes are multiplied by the base target size of the difficulty and change each time a target respawns.

Winning the highest unlocked level (reaching at least the first score tier without dying) **unlocks the next level and the next bow and arrow**.

### Difficulty: how hard it is
Defined in `DifficultySettings` assets (`Assets/.../Difficulties`). Difficulty controls:
- the game duration ;
- the score tiers to reach ;
- the base size of the targets ;
- the speed and distance of the flying targets ;
- the speed and frequency of the gladiators.

It can be combined with any level.

- **Easy**: longer games, bigger targets.
- **Normal**: the reference settings.
- **Hard**: shorter games, smaller and faster targets.
- **Training**: 4 fixed targets, no timer, no progression, scores are not saved.

### Equipment
Defined in `BowData` and `ArrowType` assets (`Assets/.../Equipment`).

- **Bows** differ by their maximum arrow speed.
- **Arrows** differ by their speed multiplier and gravity factor.

A higher level means a faster and straighter shot.

Final arrow speed = `bow max speed × draw (0 to 1) × arrow speed multiplier`.

### Score and health
- **Targets**: 1 to 10 points depending on the ring hit.
- **Gladiators**: points when killed (2 arrows needed).
- **Player**: 100 HP. Each gladiator hit removes HP. At 0 HP the game ends ("Defeated").

---

## Installation

### Requirements
- **Unity 6000.5.10f1**: use exactly this version. Install it with the **Android Build Support** module (including OpenJDK and Android SDK & NDK).
- A **Meta Quest 3** in developer mode, to test on the headset.
- 
---

**Reset the progression:** use the **Reset progress** button in the menu or uninstall the app from the headset.

---

## Project structure
```
Assets/
  Scenes/              Main scene
  Scripts/             All game scripts (see Architecture)
  Asset/
    Levels/            LevelData assets
    Difficulties/      DifficultySettings assets
    Equipment/         BowData and ArrowType assets
    Prefabs/           Bows, arrows, targets, gladiator, aim dot
    FBX/               3D models
    Animator/          Gladiator animations and Animator Controller
    Arena/, Target/    Environment and target assets
  Materials/, Textures/
  Samples/, XR/, XRI/, TextMesh Pro/   Unity packages (not modified)
```

### Main design choices
- **Managers (singletons)**: `LevelManager`, `ScoreManager`, `EquipmentManager`, `MenuManager`, `TargetSpawner`, `GladiatorSpawner`, `PlayerHealth` and `PlayerRig` each have one responsibility. They are reachable through a static `Instance`.
- **Data / logic separation (ScriptableObjects)**: levels (`LevelData`), difficulties (`DifficultySettings`), bows (`BowData`) and arrows (`ArrowType`) are assets. Designers can tune the game in the Inspector without changing code.
- **Polymorphism through an interface**: `IArrowHittable` is implemented by `Target`, `Gladiator`, `MenuTarget` and `ReplayTarget`. The arrow only calls `OnArrowHit()` on what it touches, without knowing its type.
- **Events for loose coupling**: `Target.OnHit` lets the `TargetSpawner` respawn a target, and `Gladiator.OnRemoved` lets the `GladiatorSpawner` keep its count. The objects do not need a reference to their spawner.
- **State machine**: the `Gladiator` uses an `enum State { Entering, Chasing, Attacking, Dead }` with a `switch`.
- **Composition**: a flying target is a normal target plus a `TargetMover` component.

### Scripts by role
- **Core**: `LevelManager` (game loop, timer, end of game), `ScoreManager` (score, tiers, gauge), `SaveSystem` and `SaveData` (JSON save).
- **Player**: `PlayerRig` (hands, controllers, haptics), `PlayerHealth` (HP, damage feedback), `FaceTargetOnStart` (faces the lectern at launch).
- **Archery**: `Bow` (string, draw, nocking, shooting), `Arrow` (flight, impact), `ArrowQuiver` (gives an arrow behind the head), `AimPreview` (aim help), `EquipmentManager` (selected equipment, unlocks).
- **Targets**: `Target` (score zones), `TargetMover` (flying movement), `TargetSpawner` (spawn and respawn), `IArrowHittable`.
- **Enemies**: `Gladiator` (AI with NavMesh, attack), `GladiatorSpawner`.
- **UI**: `MenuManager`, `Leaderboard`, `NameEntry`, `VirtualKeyboard`, `MenuTarget`, `ReplayTarget`.
- **Data**: `LevelData`, `DifficultySettings`, `BowData`, `ArrowType`.

### Physics layers
- **Arrow** collides with Default, Environment, GroundLayer, Target and Enemy. It ignores Player, Bow, Arrow and WallLayer.
- **WallLayer** (invisible walls) blocks the player and gladiators, but not arrows.
- **Player** does not collide with Enemy, Bow or Arrow.

---

## Save system
The save file is a JSON file written with `JsonUtility` in `Application.persistentDataPath/save.json`:
- **Windows editor**: `%userprofile%/AppData/LocalLow/<Company>/<Product>/save.json`
- **Quest**: internal storage of the app

```
Scores are sorted from best to worst and the leaderboard shows the scores of the selected level.
