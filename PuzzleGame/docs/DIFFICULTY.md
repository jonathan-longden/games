# Difficulty review (vertical slice)

No puzzle was changed. This review **measures** the 20 main levels (plus B1 and S1) with the solver and compares them with the intended curve, so the playtest knows where to look.

## How each level was measured

- **Optimal:** the fewest possible moves, from the BFS solver. This number is also stored as `optimal:` in `levels.txt`, and the tests fail if the two ever disagree.
- **Pushes:** block or echo pushes in that optimal solution.
- **Echo triggers:** how many times the optimal solution steps on a rune.
- **Search:** the number of states the solver visits before finding the answer.

**Search** is only a rough stand-in for difficulty. A large search means many plausible-looking wrong moves. People don't solve puzzles by BFS, so it can't replace a real playtest.

| # | Kind | Teaches / uses | Optimal | Pushes | Echo triggers | Search | Star 3 |
|---|---|---|---|---|---|---|---|
| 1 | Normal | Movement, exit | 7 | 0 | – | 11 | Crystal (17) |
| 2 | Normal | Route length, move target | 8 | 0 | – | 20 | Crystal (12) |
| 3 | Normal | Reading a maze | 9 | 0 | – | 20 | Crystal (19) |
| 4 | Normal | **Pushing** | 5 | 1 | – | 8 | Crystal (11) |
| 5 | Normal | Blocks can't be pulled | 6 | 0 | – | 14 | Crystal (14) |
| 6 | Normal | Block on the exit; undo is free | 6 | 3 | – | 18 | Crystal (6) |
| 7 | Normal | One push at a time | 8 | 4 | – | 42 | Crystal (8) |
| 8 | Normal | Clearing a junction | 6 | 1 | – | 13 | Crystal (14) |
| 9 | Normal | Three blocks | 10 | 1 | – | 594 | Perfect 10 |
| 10 | Normal | **Plates and doors** | 12 | 2 | – | 26 | Crystal (16) |
| 11 | Normal | Doors close on release; KEY | 22 | 2 | – | 112 | Crystal (26) |
| 12 | Normal | **Echo** | 12 | 0 | 2 | 3,889 | Perfect 12 |
| B1 | Bonus | Echo holds a door open; GEM | 14 | 0 | 2 | 6,302 | Perfect 14 |
| 13 | Normal | Two plate colours | 12 | 2 | – | 666 | Crystal (14) |
| 14 | Normal | Echo pushes a block, two triggers | 9 | 0 | 2 | 610 | Perfect 9 |
| 15 | Normal | Two plates, one door | 16 | 4 | – | 1,951 | Crystal (18) |
| S1 | Secret | Two channels | 16 | 1 | – | 140 | Crystal (16) |
| 16 | Hard | Two-block Sokoban | **37** | **13** | – | 6,532 | Perfect 37 |
| 17 | Normal | Exact echo pushes, nudging; PIECE | 19 | 0 | 6 | 411 | Perfect 19 |
| 18 | Normal | Three-block Sokoban | **49** | **18** | – | **34,193** | Perfect 49 |
| 19 | Hard | Door only needs to be open while the echo passes | 29 | 6 | 2 | 1,281 | Perfect 29 |
| 20 | Boss | Echo exact push + two plates + don't re-trigger; RELIC | 31 | 3 | 4 | **206,416** | Perfect 31 |

## Against the intended curve

| Band | Intended | Actual | Verdict |
|---|---|---|---|
| 1–3 | Introduction | Movement only; the crystals add a detour choice | ✅ |
| 4–6 | Basic pushing | 4 and 6 need pushes. **5 can be finished without touching the block**; the block only guards the crystal | ⚠️ 5 may not teach "no pulling" unless the player goes for the crystal |
| 7–9 | Multiple blocks | ✅. 9 is solved with only 1 push despite having 3 blocks | ⚠️ 9 may feel like "walk around" |
| 10–11 | Plates and doors | ✅. 11 is a long walk (22 moves) | watch for tedium |
| 12 | Echo introduction | Echo-only room, 2 triggers | ✅ see Level 12 notes below |
| 13–15 | Echo + switches | Only **14** uses the echo. **13 and 15 are block/plate puzzles** | ⚠️ the echo goes quiet right after it is introduced |
| 16–18 | Complex echo puzzles | **16 and 18 are pure Sokoban with no echo**; 17 is the only echo level | ❌ biggest mismatch |
| 19 | Hard challenge | Echo + blocks, one real insight | ✅ |
| 20 | Boss | Combines everything; largest search space by far | ✅ (see Level 20 notes) |

### The staircase has a cliff

By search size and pushes, the climb goes: 15 (16 moves, 4 pushes) → **16 (37 moves, 13 pushes)** → 17 (19 moves) → **18 (49 moves, 18 pushes)** → 19 (29 moves).

That is a sawtooth, not a staircase. Level 16 is the first long Sokoban and arrives with no warm-up. Level 18 is the longest level in the game, and its Perfect star requires the exact 49-move optimum.

## Recommendations (for after the playtest, not applied)

1. **Watch 16 closely.** If testers stall there, first try swapping 16 and 17 in the map order (this changes no puzzle). A gentler two-block puzzle before 16 would be a later content change.
2. **Level 18's Perfect star (49 moves)** is likely unreachable for most players. Consider star 3 = crystal there instead of Perfect.
3. If the echo "goes quiet" in 13–18 (players forget how it works), the fix is new echo puzzles in that band, which is a content change for later. The current ordering is intentional to keep block skills sharp, but the brief asked for echo there.
4. Level 5: if testers skip the crystal, they never meet "can't pull". Moving the crystal is a content change for later. Keep it for now and observe.

## Level 12 (echo introduction) UX

What the player must understand, and how the level and UI show each point:

1. **What activates the echo.** The rune and the echo block are ringed three times when the level opens (first attempt only), and the hint says *"Step on the rune: the violet block repeats your last 3 moves."*
2. **What gets recorded.** The HUD's ECHO strip shows the last 3 moves as arrows, newest on the right. It updates on every move. Bumps into walls are not recorded, and the strip shows that too.
3. **What it will repeat.** Exactly the arrows in the strip. When the player stands next to the rune, the strip lights up and reads *"plays on the rune"*.
4. **Where it will move.** A faint violet trail on the board shows where the echo would go. One step from the rune the trail turns bright ("armed") and is exact, because it simulates that very step, including any push.
5. **How it helps.** The echo's room holds the plate that opens the exit door. The only way to press it is to "type" a path with your own moves.

The tests check that the memory strip and the armed trail always match the real replay, across every reachable state of every echo level.

**RESET ECHO:** not added. Undo already restores the memory exactly, and the strip shows it, so one more button would add complexity without adding capability. Revisit if testers ask "how do I clear it?"

## Level 20 (boss)

The size is only moderately larger (11×10). What makes it hard is that it needs every idea at once:

- program the echo to push its block exactly the right distance (exactness, from 17);
- press two plates in two rooms, where the second room is reached only through the echo-opened door (plates, doors);
- avoid re-triggering the rune on the way out with a memory that would drag the echo off its plate (echo understanding).

Presentation: crimson frame and halo around the board, slow crimson embers, "BOSS" label on the HUD, a crowned node with an aura on the map, and "SPIRE CONQUERED" on completion.
