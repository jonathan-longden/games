# Echo Ascent — mobile puzzle game foundation

A portrait, grid-based puzzle game for Unity **2022.3 LTS** (C#). You push stone blocks onto pressure plates to open doors and reach the exit. From level 12 there is also the **Echo Block**, which replays your last few moves when you step on a rune. You climb a winding map of levels, earning stars, coins and hidden items.

All art and sound is generated in code. The project has no paid assets and no art files.

## 1. Opening the project

1. Install **Unity 2022.3 LTS** with Unity Hub (any 2022.3.x works; the project was authored against 2022.3.20f1). Add Android and/or iOS Build Support if you want to build to a phone.
2. In Unity Hub, click **Add → Add project from disk** and select the `PuzzleGame/` folder (the one containing `Assets/`, `Packages/`, `ProjectSettings/`).
3. Open it. If Hub warns about a different 2022.3 patch version, choose to continue. Unity imports the packages it needs: uGUI, Test Framework and the Visual Studio integration.
4. On first open, an editor script (`ProjectSetup.cs`) does the following:
   - sets portrait orientation and the product name,
   - creates `Assets/Scenes/Main.unity`,
   - adds that scene to Build Settings.
   
   If a scene with unsaved changes was already open, it asks you to use **Puzzle Game → Re-run Project Setup**.

## 2. Running it

- Open `Assets/Scenes/Main.unity` and press **Play**. The game bootstraps itself from code (`Bootstrap` in `GameManager.cs`), so any scene works, including an empty one.
- In the Game view, pick a portrait resolution such as **1080×1920**, or add one with the "+" button. Free Aspect works too, but portrait is the intended layout.
- To build: **File → Build Settings → Android/iOS → Build**.
- **Tests:** open **Window → General → Test Runner → EditMode → Run All**. There are 17 tests:
  - the rules, echo blocks, undo and rewards,
  - a check that every shipped level is valid and solvable.
- **Level check:** **Puzzle Game → Validate All Levels** runs the solver on every level and prints optimal solutions.
- **Dev tools:** **Puzzle Game → Dev Panel** (Ctrl/Cmd+Shift+D). It can:
  - jump to a level, reload it, or auto-solve it (optionally collecting the crystal),
  - complete a level with 1, 2 or 3 stars, or set the stars on any level,
  - give coins, unlock all levels, unlock all items,
  - reset progress, delete the save file, open the save folder.
  
  The panel is in an Editor-only assembly, and every runtime hook it calls is wrapped in `#if UNITY_EDITOR`, so none of it reaches a release build.

## 3. Controls

| Action | Phone | Editor / desktop |
|---|---|---|
| Move | Swipe anywhere on the board, or the on-screen D-pad (hold to repeat) | Arrow keys / WASD (hold to repeat), or click-drag |
| Undo | UNDO button (hold to rewind quickly) | Z, U or Backspace |
| Reset | RESET button. A reset can itself be undone | R |
| Pause | Pause button (top left) | Esc or P |

A move fires as soon as a swipe crosses the threshold, not when you lift your finger. New input skips whatever animation is still playing. The one exception is an echo replay: up to two inputs are queued while it plays so you can watch it. There are no timers, lives or energy.

## 4. The levels and what each one teaches

The map also has two off-path levels: **B1** (bonus) and **S1** (secret). "Optimal" is the solver's minimum number of moves.

| # | Title | Teaches | Optimal / target | Star 3 |
|---|---|---|---|---|
| 1 | First Steps | Moving; finding the exit | 7 / 9 | Crystal down a side corridor |
| 2 | Two Roads | Routes differ in length; move targets | 8 / 10 | Crystal on the long road |
| 3 | The Long Way Round | Reading a maze before walking | 9 / 11 | Crystal in a far corner |
| 4 | Nudge | Walking into a block pushes it | 5 / 7 | Crystal — pushing the block the wrong way buries it |
| 5 | Doorway | Blocks can't be pulled; clear a doorway | 6 / 8 | Crystal behind the block |
| 6 | Squatter | A block can sit on the exit — push it off. Undo is free | 6 / 8 | Crystal — push up, not down |
| 7 | Two Stones | You can only push one block at a time | 8 / 10 | Crystal |
| 8 | Crossroads | Freeing a junction held by two blocks | 6 / 8 | Crystal in the south alcove |
| 9 | Stone Garden | Three blocks; order matters | 10 / 12 | Perfect (10) |
| 10 | Pressure Plate | A block on a plate opens its door | 12 / 15 | Crystal |
| 11 | Hold the Door | Doors close when the plate is released; plan the walk around. **KEY** | 22 / 27 | Crystal |
| 12 | The Echo | **Echo Block introduced**: it repeats your last 3 moves when you step on the rune | 12 / 15 | Perfect (12) |
| B1 | Gem Grotto *(bonus, after 12)* | Using the echo twice: hold one door open, explore, then send it to the other plate. **GEM** | 14 / 17 | Perfect (14) |
| 13 | Two Seals | Two plate colours, two doors | 12 / 15 | Crystal |
| 14 | Echo and Stone | The echo pushes a block; memory is short, so trigger it twice | 9 / 11 | Perfect (9) |
| 15 | Twin Plates | One door that needs two plates | 16 / 19 | Crystal |
| S1 | The Sealed Stair *(secret: after 14 and needs the Key)* | Two-channel block puzzle | 16 / 19 | Crystal |
| 16 | Switchback *(Hard)* | A real two-block Sokoban with order dependencies | 37 / 43 | Perfect (37) |
| 17 | Measured Steps | Exact echo pushes (overshooting is fatal); nudge by stepping on and off the rune. **PUZZLE PIECE** | 19 / 22 | Perfect (19) |
| 18 | Gallery | Three blocks onto three plates | 49 / 55 | Perfect (49) |
| 19 | The Loop *(Hard)* | A door only needs to stay open while the echo passes through it | 29 / 34 | Perfect (29) |
| 20 | Heart of the Spire *(Boss)* | Everything: echo-driven door, two-plate exit, and don't re-trigger the rune carelessly. **RELIC** | 31 / 36 | Perfect (31) |

Levels live in `Assets/Game/Resources/Levels/levels.txt` as readable ASCII boards. The format is documented in `LevelParser.cs`, and each board has fields for:

- the move target and mastery objective (star 3),
- the level kind (Normal / Hard / Bonus / Secret / Boss) and difficulty,
- the reward and echo memory length,
- unlock requirements and map position.

Nothing about any specific level is hard-coded.

## 5. Progression system

- **Map.** It is a path that winds upward through four regions: Sunken Steps, Hall of Stones, Echo Vaults, Starlit Spire.
  - Finishing a level unlocks whatever requires it. You never have to replay to continue.
  - Level 12 branches to the bonus level B1, which rejoins the path at 14.
  - After 14, a sealed secret stair (S1) opens only once you own the Key from level 11.
  - Fog hides the path more than about a row above your progress.
- **Stars (3 per level).**
  1. Complete the level.
  2. Finish within the move target.
  3. The mastery objective: collect the crystal, or finish in the optimal ("Perfect") number of moves.
  
  Stars are kept as a union: each star, once earned, is kept forever, and different stars can come from different attempts.
- **Coins.**

  | Level kind | Completion | Target star | Mastery star |
  |---|---|---|---|
  | Normal | 100 | +50 | +100 |
  | Hard | 200 | +75 | +150 |
  | Boss | 400 | +75 | +150 |
  | Bonus / Secret | 300 | +50 | +100 |

  Each amount is paid once, the first time that star is earned, so there is nothing to grind. Every coin goes through `CurrencyManager`. Nothing spends coins yet: there is no shop.
- **Bonus items.** The Key (L11), Gem (B1), Puzzle Piece (L17) and Relic (L20) are optional pickups. An item counts only if you finish the level while carrying it. Items are shown on the map's top bar, and the Key already opens the secret level.
- **Save.** `SaveManager` writes versioned JSON to `Application.persistentDataPath/save.json`. It stores:
  - coins, current map position and highest unlocked level,
  - per-level completion, stars, best moves and whether the crystal was found,
  - unlocked levels (including bonus and secret) and collected items,
  - the sound setting.
  
  Each write goes to a temp file first, and the previous save is kept as a backup. `SaveMigrations` upgrades old versions instead of discarding them. The game saves on completion, on navigation and when the app is paused or quit.

## 6. Code map

```
Assets/Game/Scripts/
  Core/      pure C#, no UnityEngine (unit-tested)
             PuzzleEngine (rules), PuzzleState, MoveHistory (undo), LevelData,
             LevelParser, PuzzleSolver (BFS), LevelValidator, SaveData, Progression
  Runtime/   GameManager (state machine + LevelManager + Bootstrap), PuzzleManager,
             PlayerController (input), Board/GridManager + BoardPieces (Block, Switch,
             Door, Goal, EchoBlock, EchoRune, Pickup, PlayerAvatar), Managers/ (Save,
             Currency, Progress, Reward, LevelDatabase), UI/ (UIManager, HUD,
             WorldMapManager, overlays), Visual/ (Theme, SpriteFactory, Tween, Fx),
             AudioManager
  Editor/    DevPanelWindow, LevelTools, ProjectSetup
Assets/Game/Tests/Editor/  NUnit edit-mode tests
```

**Echo design.**
- Every successful move is appended to a short memory (the last N moves; N is set per level).
- Stepping onto a rune makes every echo block replay that memory one step at a time.
- A blocked step is skipped. An echo can push one ordinary block.
- The step onto the rune is recorded *after* the replay. The HUD's memory strip therefore always shows exactly what will play, and a faint ghost trail on the board shows where the echo will end up.

## 7. Known limitations

- **Not yet run inside Unity.** It was built in an environment without the Unity editor. What was verified:
  - The core compiles, and the 17 edit-mode tests and the solver validation pass under .NET 8.
  - All runtime and editor scripts compile cleanly against Unity reference assemblies, with a small uGUI stand-in because the package wasn't downloadable.
  - Layout, feel and on-device behaviour still need a first check in the editor. Expect some spacing to tune.
- **No `.meta` files are committed.** Unity generates them on first import. Commit them afterwards.
- **Legacy uGUI text** with Unity's built-in font. It is clear but plain. Switching to TextMeshPro and a display font is an easy upgrade.
- **No music**, only synthesised sound effects. Haptics are not implemented.
- The echo ghost trail assumes one rune per level (true for all current levels).
- Levels 1–9 are deliberately gentle. Their challenge mostly lives in the move target and crystal.
- **Stars are a union across attempts** (see Progression). If you want "all three stars in one run", that is a one-line change in `ProgressRules.RecordCompletion`.

## 8. What to play-test before adding content

1. **Controls on a real phone.** Check that swipe threshold, D-pad size and hold-repeat never cause an accidental double move. Check that undo feels instant.
2. **Is the echo readable?** Do players get level 12 without help? Is the memory strip plus ghost trail enough, or does the rule "the rune step is remembered after the replay" confuse them?
3. **Difficulty curve.** Watch where players stall or quit, especially 11 (the long walk around), 16 (first real Sokoban), 17 (exactness) and 19–20. Consider swapping 16 and 17 if 16 is a wall.
4. **Move targets and Perfect pars.** Is star 2 attainable on a first "good" solve? Does "Perfect = optimal" feel fair on 16 and 18, or only achievable by solvers?
5. **Reward pacing.** Does the level-complete screen feel quick and satisfying? Do players want to replay for missing stars, and do they notice the bonus branch and the sealed secret?
6. **Map pull.** Does the fogged path make people want to see what's next?
7. **Save robustness.** Force-quit mid-level, right after completion, and while paused.
