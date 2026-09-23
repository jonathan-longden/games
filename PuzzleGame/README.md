# Echo Ascent: vertical slice

A portrait, grid-based puzzle game for Unity **2022.3 LTS** (C#). You push stone blocks onto pressure plates to open doors and reach the exit. From level 12 there is also the **Echo Block**, which replays your last few moves when you step on a rune. You climb a winding map of 20 levels (plus a bonus and a secret), earning stars, coins and hidden items.

All art and sound is generated in code. The project has no paid assets and no art files.

**Status: ready to playtest, not play-tested.** See [`docs/PLAYTEST.md`](docs/PLAYTEST.md) for the first-session script and [`docs/DIFFICULTY.md`](docs/DIFFICULTY.md) for the measured difficulty curve.

## 1. Opening the project

1. Install **Unity 2022.3 LTS** with Unity Hub (any 2022.3.x works; the project was authored against 2022.3.20f1). For phone builds, add **Android Build Support** (with OpenJDK and Android SDK & NDK) and/or **iOS Build Support**.
2. In Unity Hub, click **Add → Add project from disk** and select the `PuzzleGame/` folder (the one containing `Assets/`, `Packages/`, `ProjectSettings/`).
3. Open it. If Hub warns about a different 2022.3 patch version, choose to continue. Unity imports the packages it needs: uGUI, Test Framework and the Visual Studio integration. The first import takes a few minutes.
4. On first open, an editor script (`ProjectSetup.cs`) does the following:
   - sets portrait orientation and the product name,
   - creates `Assets/Scenes/Main.unity`,
   - adds that scene to Build Settings.

   If a scene with unsaved changes was already open, it asks you to use **Puzzle Game → Re-run Project Setup**.
5. Unity generates `.meta` files on this first import. Commit them.

## 2. Running it

**In the editor**
1. Open `Assets/Scenes/Main.unity`. Any scene works, because the game bootstraps itself from code.
2. In the **Game** view's resolution dropdown, choose or add **1080×1920** (portrait). Also try 1170×2532 (tall phone) and 1536×2048 (tablet).
3. Press **Play**. You get the main menu: tap **START**, then the map.

**Building to a phone**
1. **File → Build Settings**, select Android or iOS, then **Switch Platform**.
2. Check that `Assets/Scenes/Main.unity` is ticked in the scene list.
3. Android: connect a phone with USB debugging enabled, then **Build And Run**. iOS: **Build**, open the generated Xcode project, set a signing team, and run.

**Tests and tools**
- **Tests:** open **Window → General → Test Runner → EditMode → Run All**. There are 32 tests (section 7).
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
| Undo | **UNDO ×N** button: N is how many moves can be undone. Hold to rewind quickly | Z, U or Backspace |
| Reset | RESET button. A reset can itself be undone | R |
| Pause | Pause button (top left) | Esc or P |

**Input is never delayed.** A move is applied the moment a swipe crosses about 3.5 mm, or the moment the pad is pressed. Any animation still playing, including an echo replay, is fast-forwarded first.

Protections against mistakes:
- One flick gives one move. Continuing the same drag needs extra travel and a short pause before a second move fires.
- Diagonal-ish drags wait until the direction is clear.
- Blocked moves still answer: the player bumps, and a block that can't move jiggles.

There are no timers, lives or energy, and undo and reset are always free.

## 4. The levels and what each one teaches

The map also has two off-path levels: **B1** (bonus) and **S1** (secret). "Optimal" is the solver's minimum number of moves; it is also stated in `levels.txt` and checked by the tests.

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

Levels live in `Assets/Game/Resources/Levels/levels.txt` as readable ASCII boards. The format is documented in `LevelParser.cs`. Nothing about any specific level is hard-coded.

## 5. How the vertical slice presents things

- **Board hierarchy.** Walls are dark and matte, and floors are quiet. Only the player, the exit and the echo glow. Plates breathe gently until pressed. Closed doors show coloured bars without a glow, and when one of their plates changes they flash, so plate and door read as a pair. Blocks glow in the colour of the plate they rest on.
- **Teaching.** The first time blocks (L4), plates and doors (L10) or the echo (L12) appear, those objects are ringed three times on the first attempt, alongside a one-line hint. There are no tutorial popups.
- **Echo.**
  - The HUD strip shows the last N moves (newest on the right) and "repeats last N".
  - A faint violet trail shows where the echo would go.
  - One step from the rune, both light up ("plays on the rune"), and the trail becomes exact: it simulates that very step.
  - During a replay the strip highlights each step as it plays.
- **Level complete** (about 1.6 s, tap anywhere to finish instantly; the buttons work immediately):
  - stars appear one at a time (stars from earlier attempts show faded);
  - "18 MOVES", with "BEST: 18" or "NEW BEST";
  - coins count up and fly into a wallet counter;
  - "CRYSTAL FOUND" gets its own row, and bonus items get a large banner;
  - hard and boss levels carry a badge.
- **Map.**
  - Completed levels have gold rings and stars. The current level has the marker and a pulsing ring. Locked levels show a lock and "???".
  - Bonus levels are gold diamonds. The boss is larger, crimson and crowned.
  - The **secret** level stays invisible until you finish the level it branches from. Then it appears as a faint sealed "?" with no path highlighted; tapping it says it needs a key.
- **Boss.** The board has a crimson frame and halo with rising embers, and completing it shows "SPIRE CONQUERED".
- **Mobile.**
  - Touch targets are 110–200 canvas units (the D-pad buttons are 176), and gameplay text is at least 30 units on a 1080-wide canvas (a few secondary labels such as "MOVES" and the echo sub-label are 26–28).
  - The layout respects the safe area.
  - The canvas matches width on phones and height on wider screens (tablets).
  - The map's tap-versus-scroll threshold scales with screen DPI.

## 6. Progression system

- **Map.** It is a path that winds upward through four regions: Sunken Steps, Hall of Stones, Echo Vaults, Starlit Spire.
  - Finishing a level unlocks whatever requires it. You never have to replay to continue.
  - Level 12 branches to the bonus level B1, which rejoins the path at 14.
  - After 14, the secret stair S1 opens once you own the Key from level 11.
- **Stars (3 per level)** are cumulative. Each star, once earned, is kept forever, and different stars can come from different attempts.
  1. Complete the level.
  2. Finish within the move target.
  3. The mastery objective: collect the crystal, or finish in the optimal ("Perfect") number of moves.
- **Coins.**

  | Level kind | Completion | Target star | Mastery star |
  |---|---|---|---|
  | Normal | 100 | +50 | +100 |
  | Hard | 200 | +75 | +150 |
  | Boss | 400 | +75 | +150 |
  | Bonus / Secret | 300 | +50 | +100 |

  Each amount is paid once, the first time that star is earned, so there is nothing to grind. Every coin goes through `CurrencyManager`. Nothing spends coins yet.
- **Bonus items.** The Key (L11), Gem (B1), Puzzle Piece (L17) and Relic (L20) count only if you finish the level while carrying them.
- **Save.** `SaveStore` (Core, unit-tested) and `SaveManager` (Unity) handle it:
  - It loads the main file, then the backup, then falls back to a fresh save, so a corrupted or empty file can never stop the game starting.
  - Every load is migrated and repaired: duplicate level records are merged, duplicate or unknown items and unlocks are dropped, and negative coins are clamped. A damaged save therefore cannot pay a reward twice.
  - Writes go through a temp file and keep the previous save as a backup.

## 7. Tests

The 32 NUnit edit-mode tests are in `Assets/Game/Tests/Editor`. They cover:
- **Rules:** movement, pushing, no pulling, plates and doors, pickups, and the echo replay rules.
- **Levels:** all 22 parse, validate and solve. Each stated optimal matches the solver, each Perfect par equals the optimal, and mechanics are introduced in order.
- **Rewards:** every coin reward is paid exactly once across all 22 levels, items are found once, and stars accumulate across attempts.
- **Save:** progress, coins, items and the secret unlock survive a save/load round trip, and replaying after a reload pays nothing. Corrupted, empty and whitespace saves start fresh. A corrupted main file falls back to the backup. A failed write keeps the previous save. A damaged or duplicated save is repaired.
- **Undo and reset:** a randomised run with undo and reset (3 seeds × every level) checks every undo restores the exact previous state and that the board invariants always hold. Reset can be undone, and a finished level can't be un-finished.
- **Echo:** replays are deterministic. Across every reachable state of every echo level, the replay equals the memory strip and the armed trail ends exactly where the echo really ends.

## 8. Code map

```
Assets/Game/Scripts/
  Core/      pure C#, no UnityEngine (unit-tested)
             PuzzleEngine (rules), PuzzleSession (undo/reset), PuzzleState, MoveHistory,
             LevelData, LevelParser, PuzzleSolver (BFS), LevelValidator,
             SaveData, SaveStore (load/save policy), Progression
  Runtime/   GameManager (state machine + LevelManager + Bootstrap), PuzzleManager,
             PlayerController (input), Board/ (GridManager, BoardPieces), Managers/
             (Save, Currency, Progress, Reward, LevelDatabase), UI/ (UIManager, HUD,
             WorldMapManager, overlays), Visual/ (Theme, SpriteFactory, Tween, Fx),
             AudioManager
  Editor/    DevPanelWindow, LevelTools, ProjectSetup
Assets/Game/Tests/Editor/  NUnit edit-mode tests
docs/        DIFFICULTY.md, PLAYTEST.md
```

## 9. Known limitations and what still needs a real device

- **Not yet run inside Unity.** It was built in an environment without the Unity editor. What was verified:
  - The core compiles, and the 32 tests and the solver validation pass under .NET 8.
  - All runtime and editor scripts compile cleanly against Unity reference assemblies, with a small uGUI stand-in written from memory because the package wasn't downloadable.
- **Needs the Unity editor:**
  - a first Play-mode pass through every screen (layout, spacing, overlaps on the level-complete card, map scrolling);
  - the 32 tests inside Unity's Test Runner (the save tests then use the real `JsonUtility`);
  - a Play-mode check that tapping the level-complete screen skips cleanly;
  - a Play-mode check that coins land in the wallet.
- **Needs a physical phone:**
  - swipe threshold and double-move protection, and D-pad feel with thumbs;
  - one-handed reach for UNDO, RESET and pause;
  - notch and home-bar safe areas;
  - text legibility on a small phone;
  - frame rate on a low-end Android;
  - audio volume balance;
  - tablet layout.
- **Legacy uGUI text** with Unity's built-in font. It is clear but plain.
- **No music**, only synthesised sound effects. Haptics are not implemented.
- The echo trail is exact only when "armed" (one step from a rune). Farther away it shows what the current memory would do, which changes as you walk.
- Difficulty is **not** a clean staircase: levels 16 and 18 are spikes. See `docs/DIFFICULTY.md`. Levels were deliberately left unchanged for this milestone.
