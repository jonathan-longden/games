# Difficulty review

This review **measures** the 20 main levels (plus B1 and S1) with the solver, so the playtest knows where to look. The lineup was reordered and extended for the "puzzle first" milestone: the first five levels each teach one idea, and the echo arrives at level 6 instead of 12.

## How each level was measured

- **Optimal:** the fewest possible moves, from the BFS solver. This number is also stored as `optimal:` in `levels.txt`, and the tests fail if the two ever disagree.
- **Pushes:** block or echo pushes in that optimal solution.
- **Echo triggers:** how many times the optimal solution steps on a rune.
- **Search:** the number of states the solver visits before finding the answer.

**Search** is only a rough stand-in for difficulty. A large search means many plausible-looking wrong moves. People don't solve puzzles by BFS, so it can't replace a real playtest.

| # | Kind | Title | Optimal | Pushes | Echo triggers | Search | Star 3 |
|---|---|---|---|---|---|---|---|
| 1 | Normal | The First Choice | 6 | 1 | – | 14 | Crystal (14) |
| 2 | Normal | No Going Back | 10 | 0 | – | 69 | Perfect 10 |
| 3 | Normal | The Key | 22 | 2 | – | 112 | Crystal (26) |
| 4 | Normal | Two Switches | 16 | 4 | – | 1,951 | Crystal (18) |
| 5 | Normal | The Trap | 16 | 1 | – | 140 | Crystal (16) |
| B1 | Bonus | Stone Garden | 10 | 1 | – | 594 | Perfect 10 |
| 6 | Normal | The Echo | 12 | 0 | 2 | 3,889 | Perfect 12 |
| 7 | Normal | Echo and Stone | 9 | 0 | 2 | 610 | Perfect 9 |
| 8 | Normal | Held Open | 14 | 0 | 2 | 6,302 | Perfect 14 |
| 9 | Normal | Two Seals | 12 | 2 | – | 666 | Crystal (14) |
| S1 | Secret | The Gallery | 49 | 18 | – | 34,193 | Perfect 49 |
| 10 | Normal | Measured Steps | 19 | 0 | 6 | 411 | Perfect 19 |
| 11 | Normal | Echo Stair | 17 | 3 | 3 | 49,055 | Crystal (19) |
| 12 | Normal | Borrowed Steps | 19 | 5 | 4 | 60,024 | Perfect 19 |
| 13 | Normal | The Parapet | 20 | 4 | 4 | 74,847 | Perfect 20 |
| 14 | Hard | Switchback | 37 | 13 | – | 6,532 | Perfect 37 |
| 15 | Normal | Windward Gate | 24 | 3 | 5 | 31,927 | Crystal (28) |
| 16 | Normal | Seven Echoes | 27 | 3 | 7 | 30,749 | Perfect 27 |
| 17 | Normal | The Far Plate | 28 | 5 | 4 | 61,458 | Crystal (30) |
| 18 | Normal | Two Rooms | 29 | 6 | 4 | 133,229 | Crystal (31) |
| 19 | Hard | The Loop | 29 | 6 | 2 | 1,281 | Perfect 29 |
| 20 | Boss | Heart of the Spire | 31 | 3 | 4 | 206,416 | Perfect 31 |

## The first five levels, checked

Each opening level was checked by exploring every reachable state and marking the ones from which the exit can no longer be reached ("dead").

| # | Intended lesson | What the analysis shows |
|---|---|---|
| 1 The First Choice | The push direction changes which routes remain | Pushing the block up closes the short corridor to the exit; pushing it right blocks the row towards the crystal, so you go round below it. Every first push stays solvable, so level 1 cannot be lost. The crystal needs the long route (14 moves against 6). |
| 2 No Going Back | An irreversible mistake makes undo meaningful | Pushing the block down twice is a dead state (2 moves in). The solution walks around the block, so Perfect (10) means "don't push at all". |
| 3 The Key | An optional item that needs planning | The exit takes 22 moves, and the **Key** route takes 28. The door closes when the plate is released. |
| 4 Two Switches | The order of the blocks matters | One door needs both plates held. |
| 5 The Trap | The obvious solution is wrong | 3 of the 4 possible first pushes, including the obvious one, lead to dead states. Only pushing down first works. |

## Against the intended curve

| Band | Intended | Actual | Verdict |
|---|---|---|---|
| 1–5 | One idea per level | Blocks from level 1; plates and doors from level 3 | ✅ |
| 6–10 | Echo introduced and practised | 6, 7, 8 and 10 use the echo; 9 is a two-colour plate puzzle | ✅ |
| 11–18 | Escalating echo puzzles | 11–13 and 15–18 are new echo levels (2–7 triggers). They were found by a generator and verified by the solver: each is unsolvable without the echo. | ⚠️ generated, not hand-tuned (see below) |
| 14 | – | Switchback is a pure two-block Sokoban (37 moves, 13 pushes) | ⚠️ a change of pace; watch for a stall |
| 19 | Hard challenge | Echo and blocks, one real insight | ✅ |
| 20 | Boss | Combines everything; largest search space | ✅ |

### Risks to watch in the playtest

1. **Level 3 is long for a third level** (22 moves). It is the only plate-and-door puzzle that also holds the Key. If testers stall, a shorter plate introduction before it would be the fix.
2. **Levels 11–18 were generated.** Every one requires the echo, but none was designed around a single "aha". They share one template (a sealed echo room above, the player's room below), so they may feel samey. Watch whether testers plan or just try things.
3. **Level 14 (Switchback)** is still the first long Sokoban. If testers stall, try swapping it with 15. That changes no puzzle.
4. **S1 (The Gallery)** is the longest puzzle (49 moves) and its Perfect star requires the exact optimum. It is optional.

## Level 6 (echo introduction) UX

What the player must understand, and how the level and UI show each point:

1. **What activates the echo.** The rune and the echo block are ringed three times when the level opens (first attempt only), and the hint says *"Step on the rune: your echo repeats your last 3 moves."*
2. **What gets recorded.** The HUD's ECHO strip shows the last 3 moves as arrows, newest on the right. It updates on every move. Bumps into walls are not recorded, and the strip shows that too.
3. **What it will repeat.** Exactly the arrows in the strip. When the player stands next to the rune, the strip lights up and reads *"plays on the rune"*.
4. **Where it will move.** A faint violet trail on the board shows where the echo would go. One step from the rune the trail turns bright ("armed") and is exact, because it simulates that very step, including any push.
5. **How it helps.** The echo's room holds the plate that opens the exit door. The only way to press it is to "type" a path with your own moves.

The tests check that the memory strip and the armed trail always match the real replay, across every reachable state of every echo level.

**RESET ECHO:** not added. Undo already restores the memory exactly, and the strip shows it, so one more button would add complexity without adding capability. Revisit if testers ask "how do I clear it?"

## Level 20 (boss)

The size is only moderately larger (11×10). What makes it hard is that it needs every idea at once:

- program the echo to push its block exactly the right distance (exactness, from level 10);
- press two plates in two rooms, where the second room is reached only through the echo-opened door (plates, doors);
- avoid re-triggering the rune on the way out with a memory that would drag the echo off its plate (echo understanding).

Presentation: crimson frame and halo around the board, slow crimson embers, "BOSS" label on the HUD, a crowned node with an aura on the map, and "SPIRE CONQUERED" on completion.
