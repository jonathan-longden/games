using System.Collections.Generic;

namespace PuzzleGame.Core
{
    /// <summary>Facts about a single finished run, gathered by the puzzle manager.</summary>
    public struct RunStats
    {
        public int Moves;
        public bool UsedUndo;
        public bool CrystalCollected;
        public bool ItemCollected;
    }

    /// <summary>What a completed run earned. Drives the level-complete screen.</summary>
    public sealed class CompletionResult
    {
        public LevelData Level;
        public int Moves;
        public int Target;
        public int StarsThisRun;     // mask earned by this run
        public int PreviousStars;    // mask before this run
        public int NewStars;         // mask after (union)
        public int BaseCoins;
        public int TargetCoins;
        public int MasteryCoins;
        public bool FirstClear;
        public bool NewBest;
        public int BestMoves;                        // best ever, including this run
        public bool CrystalCollected;                // this run picked up the level's crystal
        public ItemType ItemFound = ItemType.None;   // newly added to the collection
        public readonly List<string> NewlyUnlocked = new List<string>();

        public int TotalCoins => BaseCoins + TargetCoins + MasteryCoins;
        /// <summary>Stars earned for the first time by this run.</summary>
        public int FreshStars => NewStars & ~PreviousStars;
    }

    /// <summary>
    /// Pure progression rules: stars, coins, items and unlocks. No Unity code,
    /// so it is unit-tested directly.
    ///
    /// Stars are kept per level as a union: a star, once earned, is never lost,
    /// and each star can be earned on a different attempt. Coins are paid the
    /// first time each star is earned, so replaying is always worthwhile when a
    /// star is missing and there is nothing to grind otherwise.
    /// </summary>
    public static class ProgressRules
    {
        public const int Star2Coins = 50;
        public const int Star3Coins = 100;

        public static int BaseCoins(LevelData l)
        {
            if (l.CoinReward > 0) return l.CoinReward;
            switch (l.Kind)
            {
                case LevelKind.Hard: return 200;
                case LevelKind.Bonus: return 300;
                case LevelKind.Secret: return 300;
                case LevelKind.Boss: return 400;
                default: return 100;
            }
        }

        public static int StarBonus(LevelData l, int star)
        {
            bool big = l.Kind == LevelKind.Hard || l.Kind == LevelKind.Boss;
            if (star == Stars.Target) return big ? 75 : Star2Coins;
            if (star == Stars.Mastery) return big ? 150 : Star3Coins;
            return 0;
        }

        public static int EvaluateStars(LevelData l, RunStats run)
        {
            int mask = Stars.Complete;
            if (run.Moves <= l.MoveTarget) mask |= Stars.Target;
            bool mastery;
            switch (l.Mastery)
            {
                case MasteryType.Crystal: mastery = run.CrystalCollected; break;
                case MasteryType.Par: mastery = run.Moves <= l.MasteryValue; break;
                case MasteryType.NoUndo: mastery = !run.UsedUndo; break;
                default: mastery = true; break;
            }
            if (mastery) mask |= Stars.Mastery;
            return mask;
        }

        public static bool MeetsRequirements(LevelData l, SaveData save)
        {
            if (!string.IsNullOrEmpty(l.Requires) && !save.IsCompleted(l.Requires)) return false;
            if (l.RequiresItem != ItemType.None && !save.HasItem(l.RequiresItem)) return false;
            return true;
        }

        /// <summary>Adds every level whose requirements are now met. Returns the newly unlocked ids.</summary>
        public static List<string> RefreshUnlocks(IList<LevelData> levels, SaveData save)
        {
            var fresh = new List<string>();
            foreach (var l in levels)
            {
                if (save.IsUnlocked(l.Id)) continue;
                if (!MeetsRequirements(l, save)) continue;
                save.unlockedLevels.Add(l.Id);
                fresh.Add(l.Id);
                if (l.IsMainPath && l.Number > save.highestUnlockedMain) save.highestUnlockedMain = l.Number;
            }
            return fresh;
        }

        /// <summary>Applies a finished run to the save and returns what it earned.</summary>
        /// <param name="forcedStars">Development only: overrides the evaluated star mask when non-zero.</param>
        public static CompletionResult RecordCompletion(IList<LevelData> levels, SaveData save, LevelData level, RunStats run,
            int forcedStars = 0)
        {
            var rec = save.GetOrCreate(level.Id);
            var res = new CompletionResult
            {
                Level = level,
                Moves = run.Moves,
                Target = level.MoveTarget,
                PreviousStars = rec.stars,
                StarsThisRun = forcedStars != 0 ? (forcedStars | Stars.Complete) : EvaluateStars(level, run),
                FirstClear = !rec.completed,
            };
            res.NewStars = res.PreviousStars | res.StarsThisRun;
            int fresh = res.FreshStars;
            if ((fresh & Stars.Complete) != 0) res.BaseCoins = BaseCoins(level);
            if ((fresh & Stars.Target) != 0) res.TargetCoins = StarBonus(level, Stars.Target);
            if ((fresh & Stars.Mastery) != 0) res.MasteryCoins = StarBonus(level, Stars.Mastery);

            res.NewBest = rec.bestMoves == 0 || run.Moves < rec.bestMoves;
            rec.completed = true;
            rec.stars = res.NewStars;
            rec.timesCompleted++;
            if (res.NewBest) rec.bestMoves = run.Moves;
            res.BestMoves = rec.bestMoves;
            res.CrystalCollected = run.CrystalCollected && level.CrystalIndex >= 0;
            if (run.CrystalCollected) rec.crystalFound = true;

            if (run.ItemCollected && level.SpecialItem != ItemType.None && !save.HasItemFrom(level.Id))
            {
                save.items.Add(new ItemRecord { type = level.SpecialItem.ToString(), levelId = level.Id });
                res.ItemFound = level.SpecialItem;
            }

            // Coins are NOT added here: the RewardManager pays them through the CurrencyManager wallet.
            res.NewlyUnlocked.AddRange(RefreshUnlocks(levels, save));
            return res;
        }

        public static int TotalStars(SaveData save)
        {
            int n = 0;
            foreach (var r in save.levels) n += Stars.Count(r.stars);
            return n;
        }

        public static int MaxStars(IList<LevelData> levels) => levels.Count * 3;

        /// <summary>
        /// The level "NEXT LEVEL" should open: the next main-path level after a main
        /// level; otherwise the first unlocked, unfinished main level. Null = none.
        /// </summary>
        public static LevelData NextLevel(IList<LevelData> levels, SaveData save, LevelData current)
        {
            if (current != null && current.IsMainPath)
            {
                foreach (var l in levels)
                    if (l.IsMainPath && l.Number == current.Number + 1 && save.IsUnlocked(l.Id)) return l;
            }
            foreach (var l in levels)
                if (l.IsMainPath && save.IsUnlocked(l.Id) && !save.IsCompleted(l.Id) && l != current) return l;
            return null;
        }
    }
}
