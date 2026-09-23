using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using PuzzleGame.Core;

namespace PuzzleGame.Tests
{
    /// <summary>
    /// Edit-mode tests (Window > General > Test Runner > EditMode > Run All).
    /// They cover the rules engine, undo, echo blocks, progression/rewards and,
    /// most importantly, that every shipped level is valid and solvable.
    /// </summary>
    public class PuzzleTests
    {
        static string LevelsText
        {
            get
            {
#if UNITY_5_3_OR_NEWER
                string path = Path.Combine(UnityEngine.Application.dataPath, "Game/Resources/Levels/levels.txt");
#else
                string path = System.Environment.GetEnvironmentVariable("LEVELS_PATH");
#endif
                return File.ReadAllText(path);
            }
        }

        static LevelData Parse(string board, string extra = "")
        {
            string text = "level T\ntitle: Test\ntarget: 99\nmastery: noundo\n" + extra + "board:\n" + board.Trim('\n') + "\nend\n";
            return LevelParser.ParseAll(text)[0];
        }

        static PuzzleState Play(PuzzleEngine e, string moves)
        {
            var s = e.CreateInitialState();
            foreach (char c in moves) e.Apply(s, DirectionUtil.FromChar(c));
            return s;
        }

        // ------------------------------------------------------------ shipped levels

        [Test]
        public void AllLevels_ParseAndHaveUniqueIds()
        {
            var levels = LevelParser.ParseAll(LevelsText);
            Assert.GreaterOrEqual(levels.Count, 20, "the foundation ships at least 20 levels");
            Assert.AreEqual(levels.Count, levels.Select(l => l.Id).Distinct().Count());
            Assert.AreEqual(20, levels.Count(l => l.IsMainPath), "20 main-path levels");
            for (int i = 1; i <= 20; i++)
                Assert.IsTrue(levels.Any(l => l.Number == i && l.IsMainPath), "missing level " + i);
        }

        [Test]
        public void AllLevels_AreValidAndSolvable()
        {
            var levels = LevelParser.ParseAll(LevelsText);
            var reports = LevelValidator.ValidateAll(levels);
            var failures = reports.Where(r => !r.Ok).Select(r => r.ToString()).ToList();
            Assert.IsEmpty(failures, string.Join("\n", failures));
            foreach (var r in reports)
                Assert.Greater(r.OptimalMoves, 0, r.LevelId);
        }

        [Test]
        public void MechanicsAreIntroducedInOrder()
        {
            var levels = LevelParser.ParseAll(LevelsText).Where(l => l.IsMainPath).OrderBy(l => l.Number).ToList();
            int FirstWith(Mechanics m) => levels.First(l => (l.Mechanics & m) != 0).Number;
            Assert.AreEqual(4, FirstWith(Mechanics.Blocks), "blocks start at level 4");
            Assert.AreEqual(10, FirstWith(Mechanics.Switches), "switches start at level 10");
            Assert.AreEqual(12, FirstWith(Mechanics.Echo), "echo blocks start at level 12");
            Assert.IsTrue(levels.Take(3).All(l => l.Blocks.Count == 0), "levels 1-3 are movement only");
        }

        // ------------------------------------------------------------ rules

        [Test]
        public void Player_MovesAndIsStoppedByWalls()
        {
            var e = new PuzzleEngine(Parse("#####\n#P.G#\n#####"));
            var s = e.CreateInitialState();
            Assert.IsFalse(e.Apply(s, Direction.Up), "wall");
            Assert.AreEqual(0, s.Moves, "bumps cost nothing");
            Assert.IsTrue(e.Apply(s, Direction.Right));
            Assert.AreEqual(1, s.Moves);
            Assert.IsTrue(e.Apply(s, Direction.Right));
            Assert.IsTrue(s.Won);
        }

        [Test]
        public void Block_IsPushedButNeverPulled()
        {
            var e = new PuzzleEngine(Parse("#######\n#PB..G#\n#######"));
            var s = Play(e, "R");
            Assert.AreEqual(new GridPos(3, 1), s.Blocks[0]);
            Assert.AreEqual(new GridPos(2, 1), s.Player);
            e.Apply(s, Direction.Left);
            Assert.AreEqual(new GridPos(3, 1), s.Blocks[0], "moving away does not pull");
        }

        [Test]
        public void Block_CannotPushTwoOrIntoWall()
        {
            var e = new PuzzleEngine(Parse("######\n#PBB.#\n#...G#\n######"));
            var s = e.CreateInitialState();
            Assert.IsFalse(e.Apply(s, Direction.Right), "two blocks in a row");
            var e2 = new PuzzleEngine(Parse("#####\n#PB##\n#..G#\n#####"));
            var s2 = e2.CreateInitialState();
            Assert.IsFalse(e2.Apply(s2, Direction.Right), "block against wall");
        }

        [Test]
        public void Door_OpensWhileSwitchPressed_AndClosesWhenReleased()
        {
            // Block row: push the block onto the switch, then off it again.
            var e = new PuzzleEngine(Parse("########\n#PB.x..#\n#.######\n#..X..G#\n########"));
            var s = Play(e, "R");
            Assert.IsFalse(e.IsDoorOpen(s, 0));
            e.Apply(s, Direction.Right);
            Assert.IsTrue(e.IsSwitchPressed(s, 0));
            Assert.IsTrue(e.IsDoorOpen(s, 0));
            e.Apply(s, Direction.Right);
            Assert.IsFalse(e.IsDoorOpen(s, 0), "released");
        }

        [Test]
        public void Door_WithTwoSwitches_NeedsBoth()
        {
            var e = new PuzzleEngine(Parse("#######\n#PB.x.#\n#.B.x.#\n#..X.G#\n#######"));
            var s = Play(e, "R");
            e.Apply(s, Direction.Right);
            Assert.IsTrue(e.IsSwitchPressed(s, 0));
            Assert.IsFalse(e.IsDoorOpen(s, 0), "one of two");
        }

        [Test]
        public void Pickups_AreCollectedByThePlayerOnly()
        {
            var e = new PuzzleEngine(Parse("#######\n#PBc.G#\n#######"));
            var s = Play(e, "R");
            Assert.IsFalse(s.PickupTaken[0], "a block rolling over it does not collect");
            e.Apply(s, Direction.Right);
            Assert.IsTrue(s.PickupTaken[0]);
        }

        // ------------------------------------------------------------ echo

        [Test]
        public void Echo_ReplaysRememberedMovesWhenRuneIsStepped()
        {
            // Echo in its own corridor; player walks R,R then steps D onto the rune.
            var e = new PuzzleEngine(Parse("########\n#E....##\n########\n#P..G..#\n#..R...#\n########", "echo: 3\n"));
            var s = Play(e, "RR");
            CollectionAssert.AreEqual(new[] { Direction.Right, Direction.Right }, s.Memory);
            var o = new MoveOutcome();
            // step down-left onto the rune: from (3,3) go L to (2,3)? rune is at (3,4): step D.
            e.Apply(s, Direction.Down, o);
            Assert.IsTrue(o.EchoTriggered);
            Assert.AreEqual(new GridPos(3, 1), s.Echoes[0], "echo moved right twice");
            CollectionAssert.AreEqual(new[] { Direction.Right, Direction.Right, Direction.Down }, s.Memory,
                "the rune step is remembered after the replay");
        }

        [Test]
        public void Echo_SkipsBlockedStepsAndCanPushBlocks()
        {
            var e = new PuzzleEngine(Parse("#########\n#E.B.x.##\n#########\n#P..R..G#\n#########", "echo: 4\n"));
            var s = Play(e, "RR");      // memory R,R ; player at (3,3)
            e.Apply(s, Direction.Right); // onto rune at (4,3): replays R,R
            Assert.AreEqual(new GridPos(3, 1), s.Echoes[0]);
            Assert.AreEqual(new GridPos(4, 1), s.Blocks[0], "echo pushed the block once");
        }

        [Test]
        public void Echo_PreviewMatchesTheRealReplay()
        {
            var level = Parse("#########\n#E......#\n#########\n#P..R..G#\n#########", "echo: 3\n");
            var e = new PuzzleEngine(level);
            var s = Play(e, "RR");
            var preview = e.PreviewEcho(s);
            Assert.IsTrue(preview.Armed, "one step from the rune");
            Assert.AreEqual(Direction.Right, preview.TriggerDir);
            e.Apply(s, Direction.Right);
            Assert.AreEqual(preview.End(0), s.Echoes[0]);
        }

        // ------------------------------------------------------------ undo

        [Test]
        public void Undo_RestoresEveryPreviousState()
        {
            var e = new PuzzleEngine(Parse("#######\n#PB.x.#\n#..X.G#\n#######"));
            var h = new MoveHistory();
            var s = e.CreateInitialState();
            var snapshots = new List<string>();
            foreach (var d in new[] { Direction.Right, Direction.Right, Direction.Down, Direction.Right })
            {
                var before = s.Clone();
                if (e.Apply(s, d))
                {
                    h.Record(before);
                    snapshots.Add($"{before.Player}{before.Blocks[0]}{before.Moves}");
                }
            }
            for (int i = snapshots.Count - 1; i >= 0; i--)
            {
                s = h.Undo();
                Assert.AreEqual(snapshots[i], $"{s.Player}{s.Blocks[0]}{s.Moves}");
            }
            Assert.IsNull(h.Undo());
        }

        // ------------------------------------------------------------ progression

        [Test]
        public void Stars_AndCoins_ArePaidOncePerStar()
        {
            var a = Parse("#####\n#P.G#\n#####", "target: 2\nmastery: par 2\n");
            a.Id = "1";
            var b = Parse("#####\n#P.G#\n#####");
            b.Id = "2";
            b.Requires = "1";
            var levels = new List<LevelData> { a, b };
            var save = new SaveData();
            ProgressRules.RefreshUnlocks(levels, save);
            Assert.IsTrue(save.IsUnlocked("1"));
            Assert.IsFalse(save.IsUnlocked("2"));

            var r1 = ProgressRules.RecordCompletion(levels, save, a, new RunStats { Moves = 5 });
            Assert.AreEqual(Stars.Complete, r1.StarsThisRun);
            Assert.AreEqual(100, r1.TotalCoins);
            CollectionAssert.Contains(r1.NewlyUnlocked, "2");

            var r2 = ProgressRules.RecordCompletion(levels, save, a, new RunStats { Moves = 2 });
            Assert.AreEqual(Stars.All, r2.NewStars);
            Assert.AreEqual(50 + 100, r2.TotalCoins, "only the newly earned stars pay");

            var r3 = ProgressRules.RecordCompletion(levels, save, a, new RunStats { Moves = 2 });
            Assert.AreEqual(0, r3.TotalCoins, "no grinding");
            Assert.AreEqual(3, ProgressRules.TotalStars(save));
        }

        [Test]
        public void Items_AreKeptAndUnlockSecretLevels()
        {
            var a = Parse("######\n#Pk.G#\n######");
            a.Id = "1";
            var secret = Parse("#####\n#P.G#\n#####", "kind: Secret\n");
            secret.Id = "S1";
            secret.Requires = "1";
            secret.RequiresItem = ItemType.Key;
            var levels = new List<LevelData> { a, secret };
            var save = new SaveData();
            ProgressRules.RefreshUnlocks(levels, save);

            var r = ProgressRules.RecordCompletion(levels, save, a, new RunStats { Moves = 3, ItemCollected = false });
            Assert.IsFalse(save.IsUnlocked("S1"), "needs the key");
            r = ProgressRules.RecordCompletion(levels, save, a, new RunStats { Moves = 3, ItemCollected = true });
            Assert.AreEqual(ItemType.Key, r.ItemFound);
            Assert.IsTrue(save.IsUnlocked("S1"));
            Assert.AreEqual(1, save.ItemCount(ItemType.Key));
        }

        [Test]
        public void NextLevel_FollowsTheMainPath()
        {
            var levels = LevelParser.ParseAll(LevelsText);
            var save = new SaveData();
            ProgressRules.RefreshUnlocks(levels, save);
            var l1 = levels.First(l => l.Number == 1);
            ProgressRules.RecordCompletion(levels, save, l1, new RunStats { Moves = 99 });
            Assert.AreEqual(2, ProgressRules.NextLevel(levels, save, l1).Number);
        }

        [Test]
        public void SaveMigration_FillsMissingFields()
        {
            var old = new SaveData { version = 0, levels = null, items = null, unlockedLevels = null, settings = null };
            Assert.IsTrue(SaveMigrations.Migrate(old));
            Assert.AreEqual(SaveData.CurrentVersion, old.version);
            Assert.IsNotNull(old.levels);
            Assert.IsNotNull(old.settings);
            Assert.IsFalse(SaveMigrations.Migrate(new SaveData { version = SaveData.CurrentVersion + 1 }));
        }
    }
}
