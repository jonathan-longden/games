using System;
using System.Collections.Generic;

namespace PuzzleGame.Core
{
    /// <summary>Turns SaveData into text and back (JsonUtility in Unity; anything in tests).</summary>
    public interface ISaveCodec
    {
        string Encode(SaveData data);
        /// <summary>May throw or return null for unreadable text.</summary>
        SaveData Decode(string text);
    }

    /// <summary>Minimal file access, so the save logic can be tested without a disk.</summary>
    public interface ISaveFiles
    {
        bool Exists(string name);
        string Read(string name);
        void Write(string name, string text);
        void Copy(string from, string to);
        void Delete(string name);
        void Move(string from, string to);
    }

    /// <summary>
    /// Load/save policy, independent of Unity:
    ///  * Load tries the main file, then the backup, then starts fresh. It never
    ///    throws, so a corrupted or empty save can never stop the game starting.
    ///  * Every loaded save is migrated and then repaired (<see cref="Sanitize"/>),
    ///    so duplicated records can never pay a reward twice.
    ///  * Save writes a temp file first and keeps the previous save as the backup.
    /// </summary>
    public sealed class SaveStore
    {
        public const string MainFile = "save.json";
        public const string BackupFile = "save.bak";
        public const string TempFile = "save.json.tmp";

        readonly ISaveCodec _codec;
        readonly ISaveFiles _files;

        public SaveData Data { get; private set; } = new SaveData();
        /// <summary>"main", "backup" or "new": where the last Load came from.</summary>
        public string LoadedFrom { get; private set; } = "new";
        public string LastError { get; private set; } = "";

        public SaveStore(ISaveCodec codec, ISaveFiles files)
        {
            _codec = codec;
            _files = files;
        }

        public void Load()
        {
            LastError = "";
            var main = TryRead(MainFile);
            if (main != null) { Data = main; LoadedFrom = "main"; return; }
            var backup = TryRead(BackupFile);
            if (backup != null) { Data = backup; LoadedFrom = "backup"; return; }
            Data = new SaveData();
            LoadedFrom = "new";
        }

        SaveData TryRead(string name)
        {
            try
            {
                if (!_files.Exists(name)) return null;
                string text = _files.Read(name);
                if (string.IsNullOrWhiteSpace(text)) { LastError = name + " is empty"; return null; }
                var data = _codec.Decode(text);
                if (data == null) { LastError = name + " could not be decoded"; return null; }
                if (!SaveMigrations.Migrate(data)) data.version = SaveData.CurrentVersion; // newer build: keep what we understand
                Sanitize(data);
                return data;
            }
            catch (Exception e)
            {
                LastError = name + ": " + e.Message;
                return null;
            }
        }

        /// <returns>False if writing failed (the previous save is left untouched).</returns>
        public bool Save(DateTime utcNow)
        {
            try
            {
                Data.version = SaveData.CurrentVersion;
                Data.lastSavedUtc = utcNow.ToString("o");
                _files.Write(TempFile, _codec.Encode(Data));
                if (_files.Exists(MainFile))
                {
                    _files.Copy(MainFile, BackupFile);
                    _files.Delete(MainFile);
                }
                _files.Move(TempFile, MainFile);
                return true;
            }
            catch (Exception e)
            {
                LastError = e.Message;
                return false;
            }
        }

        public void Replace(SaveData data) => Data = data ?? new SaveData();

        /// <summary>
        /// Repairs anything a hand-edited, merged or damaged save could contain:
        /// merges duplicate level records (keeping the best of each), removes
        /// duplicate items and unlock entries, clamps impossible numbers.
        /// </summary>
        public static void Sanitize(SaveData d)
        {
            if (d.coins < 0) d.coins = 0;
            if (d.lifetimeCoins < d.coins) d.lifetimeCoins = d.coins;
            if (d.highestUnlockedMain < 1) d.highestUnlockedMain = 1;
            if (d.currentLevel == null) d.currentLevel = "";

            var merged = new List<LevelRecord>();
            var byId = new Dictionary<string, LevelRecord>();
            foreach (var r in d.levels)
            {
                if (r == null || string.IsNullOrEmpty(r.id)) continue;
                r.stars &= Stars.All;
                if (r.stars != 0) r.completed = true;
                if (r.completed) r.stars |= Stars.Complete;
                if (r.bestMoves < 0) r.bestMoves = 0;
                if (r.timesCompleted < 0) r.timesCompleted = 0;
                if (byId.TryGetValue(r.id, out var existing))
                {
                    existing.completed |= r.completed;
                    existing.stars |= r.stars;
                    existing.crystalFound |= r.crystalFound;
                    existing.timesCompleted = Math.Max(existing.timesCompleted, r.timesCompleted);
                    if (r.bestMoves > 0 && (existing.bestMoves == 0 || r.bestMoves < existing.bestMoves)) existing.bestMoves = r.bestMoves;
                    continue;
                }
                byId[r.id] = r;
                merged.Add(r);
            }
            d.levels = merged;

            var items = new List<ItemRecord>();
            var fromLevel = new HashSet<string>();
            foreach (var i in d.items)
            {
                if (i == null || string.IsNullOrEmpty(i.type)) continue;
                if (!Enum.TryParse(i.type, out ItemType t) || t == ItemType.None) continue;
                if (!fromLevel.Add(i.levelId ?? "")) continue; // at most one item per level
                items.Add(i);
            }
            d.items = items;

            var unlocked = new List<string>();
            var seen = new HashSet<string>();
            foreach (var id in d.unlockedLevels)
                if (!string.IsNullOrEmpty(id) && seen.Add(id)) unlocked.Add(id);
            d.unlockedLevels = unlocked;
        }
    }
}
