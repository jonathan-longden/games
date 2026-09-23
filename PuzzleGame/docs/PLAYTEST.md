# Echo Ascent: 15-minute first playtest

This script is for one tester who has **never seen Echo Ascent**, on a **real phone**. It has not been run yet. Nothing in this repo claims the game has been play-tested.

## Before the session (facilitator, 5 minutes, not counted)

1. Build to a phone (see README, "Building to a phone"), or run the Unity Editor with the Game view at 1080×1920 and a mouse. Only the phone gives valid touch data.
2. Start from a fresh save. In the editor, use Puzzle Game → Dev Panel → Delete Save File. On a phone, uninstall and reinstall.
3. Turn sound **on** and set the phone volume to about 50%.
4. Have this sheet, a timer, and a way to record the screen and the tester's hands if they agree.

## Rules for the facilitator

- Say only this: *"This is a puzzle game. Please think aloud. I can't help, but you can stop whenever you want."*
- **Do not explain anything:** controls, undo, the echo, stars. What the tester doesn't discover is the finding.
- If the tester is stuck for **3 minutes** on one level, say *"feel free to move on or try anything"*. Nothing more.
- Write down exact quotes, especially "Oh!", "Wait…", "Did that work?", "Why…?".

## Script

| Time | What happens | What to watch |
|---|---|---|
| 0:00 | Hand over the phone on the main menu. | Do they find PLAY without prompting? |
| 0:30 | Map appears. | Do they understand it's a path? Do they tap Level 1 or the PLAY button? |
| 1:00 | Levels 1–3 | First swipe: did it register? Do they try the D-pad? Do they go for the crystal? Do they notice the move counter and stars? |
| 3:30 | Levels 4–6 | Do they push the block without being told? In 6, do they get stuck with the block on the exit, and do they find **UNDO** on their own? |
| 6:00 | Levels 7–9 | Any accidental double moves? Any "I didn't mean to do that"? |
| 8:00 | Levels 10–11 | Do they link the plate to the door (colour, and the door glowing when the plate changes)? In 11, is the long walk frustrating? Do they notice the **KEY**? |
| 10:00 | **Level 12 (Echo)** | The most important 3 minutes. See the Level 12 checklist below. |
| 13:00 | Keep playing (13+, or B1 if they spot it) | Do they notice the bonus branch and wonder how to reach it? |
| 14:00 | Stop. Debrief (below). | |

If the tester is much faster, let them continue up the path. If much slower, stop at 15:00 wherever they are, and still do the Level 12 debrief questions if they reached it.

## Level 12 checklist (tick as observed, do not ask)

- [ ] Noticed the rune and the violet block when they were ringed at the start
- [ ] Read the hint line
- [ ] Looked at the ECHO memory strip while moving
- [ ] Stepped on the rune **on purpose** (not by accident)
- [ ] Reacted to the replay ("oh, it copies me")
- [ ] Noticed the faint trail, or the trail brightening next to the rune
- [ ] Planned a move sequence *before* stepping on the rune
- [ ] Solved it. Time: ____ Undo presses: ____ Resets: ____

## What to record for every level

| Level | Time to finish | Moves (and target) | Stars | Undo presses | Resets | Blocked swipes / bumps | Quit / skipped? | Quotes |
|---|---|---|---|---|---|---|---|---|
| 1 | | | | | | | | |
| … | | | | | | | | |

The Dev Panel shows moves and the undo count live if the session is run in the editor.

## Specific observations to record

**Controls and feel**
- Swipes that did nothing (the tester swiped again or said "huh?"). Where on the screen, and how fast.
- Moves that happened twice when the tester meant once (**accidental double moves**).
- Swipe or D-pad preference, and whether the D-pad was ever pressed by accident.
- Any sense of delay after a swipe or a pad press.
- Thumb reach: can they hit UNDO, RESET and pause one-handed?
- Text they squinted at or could not read.

**Understanding**
- The first moment they realise blocks can't be pulled.
- The first time they use **UNDO**, and whether they then use it freely (experimenting) or avoid it.
- Whether they ever used RESET, and whether they discovered that reset can be undone.
- Plate–door link: did they know which plate opens which door?
- **Echo:** can they explain in their own words what it does, what it copies, and when? (See debrief.)

**Motivation**
- Did they replay a level for a missing star without being asked?
- Reaction to the level-complete screen: too long, too short, satisfying? Did they tap to skip?
- Did they notice the coins flying into the wallet? Did they care about coins at all?
- Reaction to finding a bonus item (Key in 11).
- Did they notice the **BONUS** node after 12, and did they ask how to get there?
- Did they look up the map to see what's next?

**Difficulty**
- Where they were stuck longest, and whether it felt "I don't get the rules" or "I don't see the solution". These need different fixes.
- Any level they called boring or tedious (watch 11).

## Debrief questions (2 minutes, after play)

1. "What was the violet block doing?" (Record the exact words.)
2. "What do the stars mean? How would you get all three?"
3. "Was there anything you wanted to do but couldn't?"
4. "Was there a moment you felt clever?" (Which level?)
5. "Would you open this again tomorrow? Why or why not?"

## After the session

- Save the screen recording with the sheet.
- File each problem under one of these, so fixes go to the right place: **controls**, **readability**, **rules unclear**, **puzzle too hard or easy**, **reward and motivation**.
- Compare where they got stuck with `docs/DIFFICULTY.md`: predicted spikes are levels 16 and 18.
