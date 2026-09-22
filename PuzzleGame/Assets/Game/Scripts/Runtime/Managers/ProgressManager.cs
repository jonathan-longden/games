using System.Collections.Generic;
using PuzzleGame.Core;

namespace PuzzleGame
{
    /// <summary>
    /// Everything about where the player is in the world: unlocks, completion,
    /// stars and collected items. Thin, save-backed wrapper over <see cref="ProgressRules"/>.
    /// </summary>
    public sealed class ProgressManager
    {
        readonly SaveManager _save;
        readonly LevelDatabase _db;
        readonly List<LevelData> _levels;

        /// <summary>Level ids unlocked since the map was last shown (for the unlock animation).</summary>
        public readonly List<string> PendingUnlockAnimations = new List<string>();

        public ProgressManager(SaveManager save, LevelDatabase db)
        {
            _save = save;
            _db = db;
            _levels = db.ToList();
            // Always make sure the first level (and anything whose requirements are met) is open.
            ProgressRules.RefreshUnlocks(_levels, Data);
            if (string.IsNullOrEmpty(Data.currentLevel) || db.Get(Data.currentLevel) == null)
                Data.currentLevel = db.First?.Id ?? "";
            _save.MarkDirty();
        }

        SaveData Data => _save.Data;

        public bool IsUnlocked(LevelData l) => l != null && Data.IsUnlocked(l.Id);
        public bool IsCompleted(LevelData l) => l != null && Data.IsCompleted(l.Id);
        public LevelRecord Record(LevelData l) => l == null ? null : Data.Find(l.Id);
        public int StarsFor(LevelData l) => Record(l)?.stars ?? 0;
        public int TotalStars => ProgressRules.TotalStars(Data);
        public int MaxStars => ProgressRules.MaxStars(_levels);
        public bool HasItemFrom(LevelData l) => l != null && Data.HasItemFrom(l.Id);
        public int ItemCount(ItemType t) => Data.ItemCount(t);
        public int CompletedCount
        {
            get
            {
                int n = 0;
                foreach (var r in Data.levels) if (r.completed) n++;
                return n;
            }
        }

        public LevelData CurrentLevel => _db.Get(Data.currentLevel) ?? _db.First;

        public void SetCurrent(LevelData l)
        {
            if (l == null || Data.currentLevel == l.Id) return;
            Data.currentLevel = l.Id;
            _save.MarkDirty();
        }

        public CompletionResult RecordCompletion(LevelData level, RunStats run, int forcedStars = 0)
        {
            var res = ProgressRules.RecordCompletion(_levels, Data, level, run, forcedStars);
            PendingUnlockAnimations.AddRange(res.NewlyUnlocked);
            _save.MarkDirty();
            return res;
        }

        public LevelData NextLevel(LevelData current) => ProgressRules.NextLevel(_levels, Data, current);

        /// <summary>Human text explaining why a level is locked.</summary>
        public string LockReason(LevelData l)
        {
            if (l == null) return "";
            var req = _db.Get(l.Requires);
            if (l.RequiresItem != ItemType.None && !Data.HasItem(l.RequiresItem))
            {
                if (l.Kind == LevelKind.Secret) return $"A sealed stair. It needs a {SpriteFactory.ItemName(l.RequiresItem).ToLowerInvariant()}...";
                return $"Requires the {SpriteFactory.ItemName(l.RequiresItem)}";
            }
            if (req != null && !Data.IsCompleted(req.Id)) return $"Complete {Pretty(req)} to unlock";
            return "Locked";
        }

        static string Pretty(LevelData l) => l.Kind == LevelKind.Bonus ? "the bonus level" :
            l.Kind == LevelKind.Secret ? "the secret level" : "Level " + l.Number;

#if UNITY_EDITOR
        // ------------------------------------------------------------ development only

        public void DevUnlockAll()
        {
            foreach (var l in _levels)
                if (!Data.IsUnlocked(l.Id)) Data.unlockedLevels.Add(l.Id);
            foreach (var l in _levels)
                if (l.IsMainPath && l.Number > Data.highestUnlockedMain) Data.highestUnlockedMain = l.Number;
            _save.MarkDirty();
        }

        public void DevUnlockAllItems()
        {
            foreach (var l in _levels)
            {
                if (l.SpecialItem == ItemType.None || Data.HasItemFrom(l.Id)) continue;
                Data.items.Add(new ItemRecord { type = l.SpecialItem.ToString(), levelId = l.Id });
            }
            PendingUnlockAnimations.AddRange(ProgressRules.RefreshUnlocks(_levels, Data));
            _save.MarkDirty();
        }

        public void DevSetStars(LevelData l, int count)
        {
            var rec = Data.GetOrCreate(l.Id);
            if (count <= 0)
            {
                rec.completed = false;
                rec.stars = 0;
            }
            else
            {
                rec.completed = true;
                rec.stars = count >= 3 ? Stars.All : count == 2 ? Stars.Complete | Stars.Target : Stars.Complete;
                if (rec.bestMoves == 0) rec.bestMoves = l.MoveTarget;
            }
            PendingUnlockAnimations.AddRange(ProgressRules.RefreshUnlocks(_levels, Data));
            _save.MarkDirty();
        }
#endif
    }
}
