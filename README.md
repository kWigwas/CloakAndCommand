# Attack of the Radioactive Things

A **top-down stealth-action-tower-defense** game.

## Requirements

- **Unity Editor** `6000.3.6f1`
- **Windows**

Clone or copy the repository, then open the folder that contains `Assets`, `Packages`, and `ProjectSettings` as a Unity project.

## Running the game

1. Open the project in Unity.
2. Add any scenes you need to **File → Build Settings** if they are missing (the checked-in list is under `ProjectSettings/EditorBuildSettings.asset`).
3. Press **Play** with the scene you want to test loaded, or build a player via **File → Build Profiles**.

Typical entry flow: **MainMenu** → stealth hub / levels or TD maps, depending on your UI wiring.

## Gameplay overview

| Mode | Notes |
|------|--------|
| **Stealth** | Top-down movement, enemy AI (patrol / chase / specials), objectives driven by scene design. Hub-style scene: `Assets/Scenes/StealthLevels/StealthOpen.unity`. |
| **Tower defense** | Waves, paths, turrets; victory handled via `TDEnemyCount` and related TD scripts. Levels live under `Assets/Scenes/TDLevels/` (`TDLevel1`–`TDLevel4`, plus extras such as `TDMergePaths`). |
| **Menus** | `MainMenu`, `SettingsMenu` (often loaded **additively** over gameplay), `CreditsMenu`, `GameOverMenu`. |

### Progression (TD → credits)

TD stages **1–4** completion is stored in **PlayerPrefs** (`TD_Done_1` … `TD_Done_4`) when a TD level is won. When **all four** are complete, loading the stealth hub can redirect to **Credits** once (see `TDCompleteRedirect` on the hub scene and `TDProgress` in code). Adjust behavior in the Inspector if you want a different flow.

## Input

The project uses Unity’s **legacy Input Manager** (`ProjectSettings/InputManager.asset`). Gameplay reads axes and buttons such as **Horizontal**, **Vertical**, **Fire1**, **Fire2**, **Jump**, **Interact**, **Pause** through `PlayerControls` (`Assets/Scripts/Player/Controls.cs`). Change bindings in **Edit → Project Settings → Input Manager**.

## Saves and settings

| Mechanism | Purpose |
|-----------|---------|
| **`SaveManager`** | Per-scene layout JSON under the OS persistent data path (`SceneLayout_<SceneName>.json`), plus enemy state as implemented in `SaveManager` / `SceneEnemySave`. |
| **`gamesave.json`** | Written under persistent data path via `SaveGame` / `GameSettings` (audio levels, display flags, etc.). |
| **`PlayerPrefs`** | Additional keys (e.g. last level for retries, TD completion flags, settings mirrors). |

Exact paths on your machine: `Application.persistentDataPath` (log it once from a small debug script if needed).

## Repository layout (high level)

```
Assets/
  Scenes/           # Menus, stealth, TD, prototypes
  Scripts/          # Gameplay, UI, persistence, enemies, turrets
  Prefabs/          # Player, enemies, TD helpers, etc.
  Settings/         # URP and render settings
Packages/           # Unity package manifest
ProjectSettings/    # Editor version, input, build scenes, etc.
```

## Authors

- Mishaal Patel
- Hariharan Vallath
- Kego Wigwas
