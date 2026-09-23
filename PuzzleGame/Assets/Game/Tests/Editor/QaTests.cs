using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using PuzzleGame.Core;

namespace PuzzleGame.Tests
{
    /// <summary>
    /// Vertical-slice QA: rewards are paid exactly once, progress survives
    /// save/load (and damaged saves), undo/reset never corrupt state, and the
    /// echo is deterministic and always matches what the HUD/trail preview.
    /// </summary>
    public class QaTests
    {
        static List<LevelData> Levels()
        {
#if UNITY_5_3_OR_NEWER
            string path = Path.Combine(UnityEngine.Application.dataPath, "Game/Resources/Levels/levels.txt");
#else
            string path = Environment.GetEnvironmentVariable("LEVELS_PATH");
#endif
            return LevelParser.ParseAll(File.ReadAllText(path));
        }

        // ------------------------------------------------------------ test doubles

        sealed class MemoryFiles : ISaveFiles
        {
            public readonly Dictionary<string, string> Files = new Dictionary<string, string>();
            public bool FailWrites;
            public bool Exists(string name) => Files.ContainsKey(name);
            public string Read(string name) => Files[name];
            public void Write(string name, string text)
            {
                if (FailWrites) throw new IOException("disk full");
                Files[name] = text;
            }
            public void Copy(string from, string to) => Files[to] = Files[from];
            public void Delete(string name) => Files.Remove(name);
            public void Move(string from, string to) { Files[to] = Files[from]; Files.Remove(from); }
        }

        /// <summary>JsonUtility inside Unity (the real codec); System.Text.Json elsewhere.</summary>
        sealed class TestCodec : ISaveCodec
        {
#if UNITY_5_3_OR_NEWER
            public string Encode(SaveData d) => UnityEngine.JsonUtility.ToJson(d);
            public SaveData Decode(string t) => UnityEngine.JsonUtility.FromJson<SaveData>(t);
#else
            static readonly System.Text.Json.JsonSerializerOptions Opt = new System.Text.Json.JsonSerializerOptions { IncludeFields = true };
            public string Encode(SaveData d) => System.Text.Json.JsonSerializer.Serialize(d, Opt);
            public SaveData Decode(string t) => System.Text.Json.JsonSerializer.Deserialize<SaveData>(t, Opt);
#endif
        }

        static SaveStore Reload(MemoryFiles files)
        {
            var s = new SaveStore(new TestCodec(), files);
            s.Load();
            return s;
        }

        static RunStats Perfect(LevelData l) => new RunStats
        {
            Moves = l.Mastery == MasteryType.Par ? l.MasteryValue : l.MoveTarget,
            CrystalCollected = l.CrystalIndex >= 0,
            ItemCollected = l.SpecialItemIndex >= 0,
        };

        // ------------------------------------------------------------ level data

        [Test]
        public void StatedMinimumMoves_MatchTheSolver_AndParIsOptimal()
        {
            foreach (var l in Levels())
            {
                var sol = PuzzleSolver.Solve(l);
                Assert.IsTrue(sol.Solved, l.Id + " has no solution");
                Assert.AreEqual(sol.Moves, l.StatedOptimal, $"level {l.Id}: stated optimal");
                Assert.GreaterOrEqual(l.MoveTarget, sol.Moves, $"level {l.Id}: target below optimal");
                if (l.Mastery == MasteryType.Par) Assert.AreEqual(sol.Moves, l.MasteryValue, $"level {l.Id}: par should be the optimal");
            }
        }

        // ------------------------------------------------------------ rewards

        [Test]
        public void EveryReward_IsAwardedExactlyOnce()
        {
            var levels = Levels();
            var save = new SaveData();
            ProgressRules.RefreshUnlocks(levels, save);
            int paid = 0, expected = 0;
            foreach (var l in levels)
            {
                expected += ProgressRules.BaseCoins(l) + ProgressRules.StarBonus(l, Stars.Target) + ProgressRules.StarBonus(l, Stars.Mastery);

                // Star 1 only, then all three, then a replay: pays base, then the two bonuses, then nothing.
                var r1 = ProgressRules.RecordCompletion(levels, save, l, new RunStats { Moves = l.MoveTarget + 1 });
                Assert.AreEqual(ProgressRules.BaseCoins(l), r1.TotalCoins, l.Id);
                var r2 = ProgressRules.RecordCompletion(levels, save, l, Perfect(l));
                Assert.AreEqual(Stars.All, r2.NewStars, l.Id);
                Assert.AreEqual(ProgressRules.StarBonus(l, Stars.Target) + ProgressRules.StarBonus(l, Stars.Mastery), r2.TotalCoins, l.Id);
                var r3 = ProgressRules.RecordCompletion(levels, save, l, Perfect(l));
                Assert.AreEqual(0, r3.TotalCoins, l.Id + " replay must pay nothing");
                Assert.AreEqual(ItemType.None, r3.ItemFound, l.Id + " item must not be found twice");
                paid += r1.TotalCoins + r2.TotalCoins + r3.TotalCoins;
            }
            Assert.AreEqual(expected, paid);
            Assert.AreEqual(levels.Count * 3, ProgressRules.TotalStars(save));
            Assert.AreEqual(4, save.items.Count, "key, gem, puzzle piece, relic");
            Assert.AreEqual(levels.Count, save.unlockedLevels.Count, "everything unlocked, nothing twice");
        }

        [Test]
        public void StarsFromDifferentAttempts_Accumulate()
        {
            var levels = Levels();
            var l = levels.First(x => x.Mastery == MasteryType.Crystal && x.CrystalIndex >= 0);
            var save = new SaveData();
            ProgressRules.RefreshUnlocks(levels, save);
            ProgressRules.RecordCompletion(levels, save, l, new RunStats { Moves = 999, CrystalCollected = true });
            Assert.AreEqual(Stars.Complete | Stars.Mastery, save.Find(l.Id).stars);
            ProgressRules.RecordCompletion(levels, save, l, new RunStats { Moves = l.MoveTarget });
            Assert.AreEqual(Stars.All, save.Find(l.Id).stars);
        }

        // ------------------------------------------------------------ save / load

        [Test]
        public void Progress_SurvivesSaveAndLoad_WithoutDuplicatingRewards()
        {
            var levels = Levels();
            var files = new MemoryFiles();
            var store = Reload(files);
            Assert.AreEqual("new", store.LoadedFrom);
            ProgressRules.RefreshUnlocks(levels, store.Data);

            // Play through level 14 perfectly (collects the Key in 11), which reveals the secret level.
            int coins = 0;
            foreach (var l in levels.Where(x => x.IsMainPath && x.Number <= 14))
                coins += ProgressRules.RecordCompletion(levels, store.Data, l, Perfect(l)).TotalCoins;
            store.Data.coins = coins;
            Assert.IsTrue(store.Data.IsUnlocked("S1"), "Key + level 14 unlock the secret");
            Assert.IsTrue(store.Save(DateTime.UtcNow));

            var loaded = Reload(files);
            Assert.AreEqual("main", loaded.LoadedFrom);
            Assert.AreEqual(coins, loaded.Data.coins);
            Assert.IsTrue(loaded.Data.IsUnlocked("S1"), "secret unlock persists");
            Assert.IsTrue(loaded.Data.HasItem(ItemType.Key), "items persist");
            Assert.AreEqual(ProgressRules.TotalStars(store.Data), ProgressRules.TotalStars(loaded.Data), "stars persist");

            // Replaying everything after a reload pays nothing and finds nothing.
            foreach (var l in levels.Where(x => x.IsMainPath && x.Number <= 14))
            {
                var r = ProgressRules.RecordCompletion(levels, loaded.Data, l, Perfect(l));
                Assert.AreEqual(0, r.TotalCoins, l.Id);
                Assert.AreEqual(ItemType.None, r.ItemFound, l.Id);
            }
            loaded.Save(DateTime.UtcNow);
            var again = Reload(files);
            Assert.AreEqual(1, again.Data.ItemCount(ItemType.Key));
            Assert.AreEqual(again.Data.levels.Count, again.Data.levels.Select(r => r.id).Distinct().Count());
        }

        [Test]
        public void CorruptedOrEmptySave_StartsSafely()
        {
            var files = new MemoryFiles();
            files.Files[SaveStore.MainFile] = "{ this is not json";
            Assert.AreEqual("new", Reload(files).LoadedFrom);

            files.Files[SaveStore.MainFile] = "";
            Assert.AreEqual("new", Reload(files).LoadedFrom);

            files.Files[SaveStore.MainFile] = "   ";
            var s = Reload(files);
            Assert.AreEqual("new", s.LoadedFrom);
            Assert.AreEqual(0, s.Data.coins);
            Assert.IsNotNull(s.Data.levels);
        }

        [Test]
        public void CorruptedMainSave_FallsBackToTheBackup()
        {
            var files = new MemoryFiles();
            var store = Reload(files);
            store.Data.coins = 100;
            store.Save(DateTime.UtcNow);
            store.Data.coins = 250;
            store.Save(DateTime.UtcNow); // previous (100) becomes the backup
            files.Files[SaveStore.MainFile] = "\0\0\0garbage";
            var loaded = Reload(files);
            Assert.AreEqual("backup", loaded.LoadedFrom);
            Assert.AreEqual(100, loaded.Data.coins);
        }

        [Test]
        public void FailedWrite_KeepsThePreviousSave()
        {
            var files = new MemoryFiles();
            var store = Reload(files);
            store.Data.coins = 70;
            store.Save(DateTime.UtcNow);
            files.FailWrites = true;
            store.Data.coins = 9999;
            Assert.IsFalse(store.Save(DateTime.UtcNow));
            files.FailWrites = false;
            Assert.AreEqual(70, Reload(files).Data.coins);
        }

        [Test]
        public void DamagedSave_IsRepaired_SoRewardsCannotBeClaimedTwice()
        {
            var levels = Levels();
            var l = levels.First();
            var bad = new SaveData { coins = -50 };
            bad.levels.Add(new LevelRecord { id = l.Id, completed = true, stars = Stars.Complete, bestMoves = 20 });
            bad.levels.Add(new LevelRecord { id = l.Id, completed = true, stars = Stars.Target, bestMoves = 9 });
            bad.items.Add(new ItemRecord { type = "Key", levelId = "11" });
            bad.items.Add(new ItemRecord { type = "Key", levelId = "11" });
            bad.items.Add(new ItemRecord { type = "Banana", levelId = "3" });
            bad.unlockedLevels.AddRange(new[] { "1", "1", "", "2" });

            var files = new MemoryFiles();
            files.Files[SaveStore.MainFile] = new TestCodec().Encode(bad);
            var s = Reload(files).Data;

            Assert.AreEqual(0, s.coins);
            Assert.AreEqual(1, s.levels.Count(r => r.id == l.Id), "duplicate level records merged");
            Assert.AreEqual(Stars.Complete | Stars.Target, s.Find(l.Id).stars);
            Assert.AreEqual(9, s.Find(l.Id).bestMoves);
            Assert.AreEqual(1, s.items.Count, "duplicate and unknown items removed");
            CollectionAssert.AreEqual(new[] { "1", "2" }, s.unlockedLevels);

            var r = ProgressRules.RecordCompletion(levels, s, l, new RunStats { Moves = l.MoveTarget });
            Assert.AreEqual(0, r.BaseCoins + r.TargetCoins, "merged stars are not paid again");
        }

        // ------------------------------------------------------------ undo / reset

        static string Snapshot(PuzzleState s) =>
            $"{s.Player}|{string.Join(",", s.Blocks)}|{string.Join(",", s.Echoes)}|{string.Join(",", s.PickupTaken)}|" +
            $"{string.Join("", s.Memory.Select(d => d.ToChar()))}|{s.Moves}|{s.Pushes}|{s.Won}";

        static void AssertInvariants(PuzzleEngine e, PuzzleState s, string ctx)
        {
            var l = e.Level;
            var cells = new List<GridPos> { s.Player };
            cells.AddRange(s.Blocks);
            cells.AddRange(s.Echoes);
            Assert.AreEqual(cells.Count, cells.Distinct().Count(), ctx + ": two things share a cell");
            foreach (var c in cells) Assert.AreEqual(Tile.Floor, l.TileAt(c), ctx + ": something is off the floor at " + c);
            for (int i = 0; i < l.Doors.Count; i++)
                if (cells.Contains(l.Doors[i].Pos)) Assert.IsTrue(e.IsDoorOpen(s, i), ctx + ": a closed door holds something");
            Assert.LessOrEqual(s.Memory.Count, Math.Max(0, l.EchoMemory), ctx + ": memory too long");
        }

        [Test]
        public void RandomPlayWithUndoAndReset_NeverCorruptsState([Values(1, 2, 3)] int seed)
        {
            foreach (var l in Levels())
            {
                var rnd = new Random(seed * 7919 + l.Id.GetHashCode());
                var session = new PuzzleSession(l);
                var expected = new Stack<string>();
                var outcome = new MoveOutcome();
                for (int step = 0; step < 250 && !session.State.Won; step++)
                {
                    double roll = rnd.NextDouble();
                    string ctx = $"level {l.Id} seed {seed} step {step}";
                    if (roll < 0.2)
                    {
                        bool had = expected.Count > 0;
                        Assert.AreEqual(had, session.Undo(), ctx + ": undo availability");
                        if (had) Assert.AreEqual(expected.Pop(), Snapshot(session.State), ctx + ": undo restored the wrong state");
                    }
                    else if (roll < 0.23)
                    {
                        string before = Snapshot(session.State);
                        if (session.Reset())
                        {
                            expected.Push(before);
                            Assert.AreEqual(0, session.State.Moves, ctx);
                        }
                    }
                    else
                    {
                        string before = Snapshot(session.State);
                        if (session.Move(DirectionUtil.All[rnd.Next(4)], outcome, out _)) expected.Push(before);
                        else Assert.AreEqual(before, Snapshot(session.State), ctx + ": a blocked move changed the state");
                    }
                    Assert.AreEqual(expected.Count, session.UndoCount, ctx + ": undo count");
                    AssertInvariants(session.Engine, session.State, ctx);
                }
            }
        }

        [Test]
        public void Reset_CanBeUndone()
        {
            var l = Levels().First(x => x.Number == 5);
            var s = new PuzzleSession(l);
            var o = new MoveOutcome();
            s.Move(Direction.Right, o, out _);
            s.Move(Direction.Right, o, out _);
            string beforeReset = Snapshot(s.State);
            Assert.IsTrue(s.Reset());
            Assert.AreEqual(0, s.State.Moves);
            Assert.IsFalse(s.Reset(), "nothing to reset at the start");
            Assert.IsTrue(s.Undo());
            Assert.AreEqual(beforeReset, Snapshot(s.State), "undo brings back the board from before the reset");
        }

        [Test]
        public void MovesAreNotAcceptedAfterWinning()
        {
            var l = Levels().First(x => x.Number == 1);
            var s = new PuzzleSession(l);
            var o = new MoveOutcome();
            foreach (var d in PuzzleSolver.Solve(l).Path) s.Move(d, o, out _);
            Assert.IsTrue(s.State.Won);
            Assert.IsFalse(s.Move(Direction.Down, o, out _));
            Assert.IsFalse(s.Undo(), "a finished level cannot be un-finished");
        }

        // ------------------------------------------------------------ echo

        [Test]
        public void EchoReplay_IsDeterministic()
        {
            foreach (var l in Levels().Where(x => x.HasEcho))
            {
                var path = PuzzleSolver.Solve(l).Path;
                var rnd = new Random(l.Id.GetHashCode());
                var noise = Enumerable.Range(0, 120).Select(_ => DirectionUtil.All[rnd.Next(4)]).ToList();
                foreach (var seq in new[] { path, noise })
                {
                    string a = Run(l, seq), b = Run(l, seq);
                    Assert.AreEqual(a, b, "level " + l.Id);
                }
            }

            string Run(LevelData l, List<Direction> seq)
            {
                var e = new PuzzleEngine(l);
                var s = e.CreateInitialState();
                var log = new System.Text.StringBuilder();
                var o = new MoveOutcome();
                foreach (var d in seq)
                {
                    e.Apply(s, d, o);
                    foreach (var f in o.EchoFrames)
                        foreach (var m in f.Moves) log.Append(m.Kind).Append(m.Index).Append(m.To);
                }
                return log + "#" + Snapshot(s);
            }
        }

        /// <summary>
        /// Over every reachable state (bounded) of every echo level: whenever a move
        /// triggers the echo, (1) the replayed moves are exactly the memory the HUD
        /// showed before the move, and (2) the armed board trail predicted exactly
        /// where each echo block ended up.
        /// </summary>
        [Test]
        public void EchoPreviewAndMemoryBar_AlwaysMatchTheRealReplay()
        {
            foreach (var l in Levels().Where(x => x.HasEcho))
            {
                var e = new PuzzleEngine(l);
                var seen = new HashSet<string>();
                var queue = new Queue<PuzzleState>();
                queue.Enqueue(e.CreateInitialState());
                int triggers = 0;
                while (queue.Count > 0 && seen.Count < 6000)
                {
                    var s = queue.Dequeue();
                    if (!seen.Add(Snapshot(s).Split('|').Take(5).Aggregate((a, b) => a + "|" + b))) continue;
                    var preview = e.PreviewEcho(s);
                    foreach (var d in DirectionUtil.All)
                    {
                        var next = s.Clone();
                        var o = new MoveOutcome();
                        if (!e.Apply(next, d, o)) continue;
                        if (o.EchoTriggered)
                        {
                            triggers++;
                            CollectionAssert.AreEqual(s.Memory, o.EchoFrames.Select(f => f.Dir).ToList(),
                                $"level {l.Id}: replay differs from the memory bar");
                            if (l.Runes.Count == 1)
                            {
                                Assert.IsTrue(preview.Armed, $"level {l.Id}: trail was not armed one step from the rune");
                                Assert.AreEqual(d, preview.TriggerDir);
                                for (int i = 0; i < next.Echoes.Length; i++)
                                    Assert.AreEqual(next.Echoes[i], preview.End(i), $"level {l.Id}: trail end differs from the real replay");
                            }
                        }
                        if (!next.Won) queue.Enqueue(next);
                    }
                }
                Assert.Greater(triggers, 0, "level " + l.Id + " never triggered its echo");
            }
        }
    }
}
