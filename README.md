# Arena Archer - VR Archery Challenge

A virtual reality archery game for **Meta Quest 3**, built with **Unity 6** and the **XR Interaction Toolkit**.
The player stands on a platform in the middle of a Roman arena, shoots at fixed and flying targets, fights off gladiators and unlocks better bows and arrows level after level.

![Gameplay](Docs/Images/gameplay.png)

---

## Table of contents
1. [Features](#features)
2. [Controls](#controls)
3. [How to play](#how-to-play)
4. [Game rules](#game-rules)
5. [Installation](#installation)
6. [Project structure](#project-structure)
7. [Save system](#save-system)

---

## Features
- **Natural archery gesture**: grab the bow, take an arrow from the quiver over your shoulder, nock it on the string, draw and release.
- **Physics-based trajectory**: the arrow speed depends on how far the string is drawn, the bow and the arrow type, and the arrow is affected by gravity.
- **4 bows and 4 arrow types** with different stats, unlocked through progression.
- **4 levels** with different content: fixed targets, flying targets, gladiators, random target sizes.
- **3 difficulties** (Easy, Normal, Hard) plus a **Training** mode with no timer and no progression.
- **Score zones** on targets (10 points in the centre, fewer towards the edge) and a real-time score gauge with tiers.
- **Gladiators** that walk down from the gates and attack the player, who has a health bar.
- **Aim help** (optional, toggled in the menu): a dotted trail showing the direction of the shot.
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
- **Turn**: right joystick.
- **Recenter the view**: hold the **Meta** button.

---

## How to play

### 1. Start a game
When the game starts, the player faces the **lectern**, which holds the main menu. Press the buttons with the tip of the controller to choose:
- the **level** (only unlocked levels can be selected) ;
- the **difficulty**: Training, Easy, Normal or Hard ;
- the **bow** and the **arrow** (only unlocked equipment can be selected) ;
- the **volume** and the **aim help** (ON / OFF).

Then press **Play**. The lectern disappears and the game starts.

On the very first launch, only **Level 1**, the **Training Bow** and the **Wooden Arrow** are available.

![Main menu](Docs/Images/menu.png)

### 2. During the game
The HUD follows the player's view:
- **top**: the time left (or "Training" in training mode) and the health bar. It flashes and the edges of the view turn red, when a gladiator hits the player.
- **left**: the score and the score gauge, with marks for each tier to reach ;

Targets respawn somewhere a moment after being hit. Flying targets move around the arena. Gladiators come down from the gates, walk to the platform and attack.

If **aim help** is ON, a dotted trail shows the direction of the shot while the string is drawn. The dots fade with distance, so they give the direction without showing the exact landing point.

In **Training**, there is no timer: the game only ends with the **Quit** button on the lectern.

### 3. End of the game
The game ends when the time is up, or when the player's health reaches 0. The lectern comes back with:
- the **result**: goal reached (with the tier), goal missed, or defeated ;
- the **score** and the **best score** for this level and difficulty ;
- the **new equipment** unlocked, if the highest unlocked level was won.

The player can then type a name with the **virtual keyboard** and press **Save**. The score is added to the **leaderboard of this level**, which is shown next to the lectern.

![End of the game](Docs/Images/results.png)

### 4. Play again
Two targets appear on the results panel. **Shoot an arrow** at one of them:
- **Replay**: starts the same level again, with the same settings ;
- **Menu**: goes back to the main menu, to change the level, difficulty or equipment.

---

## Game rules

### Levels: what is in the arena
Defined in `LevelData` assets (`Assets/Asset/Levels`).

- **Level 1, Initiation**: 4 fixed targets, 1 gladiator.
- **Level 2, Moving Targets**: 3 fixed targets, 2 flying targets, 1 gladiator, random target sizes (×0.5 to ×1.2).
- **Level 3, Gladiators**: 3 fixed targets, 2 flying targets, 1 gladiator, smaller random target sizes (×0.5 to ×1).
-
