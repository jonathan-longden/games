using System;
using System.Collections.Generic;

namespace PuzzleGame.Core
{
    /// <summary>
    /// Everything persisted between sessions. Serialized with Unity's JsonUtility,
    /// so only public fields of serializable types are stored.
    ///
    /// Versioning: bump <see cref="CurrentVersion"/> whenever the layout changes
    /// and add a step to <see cref="SaveMigrations.Migrate"/>. Old saves are
    /// upgraded on load, never discarded.
    /// </summary>
    [Serializable]
    public sealed class SaveData
    {
        public const int CurrentVersion = 1;

        public int version = CurrentVersion;
        public int coins;
        public int lifetimeCoins;
        public string currentLevel = "";          // where the player marker stands on the map
        public int highestUnlockedMain = 1;       // highest main-path level number unlocked
        public List<LevelRecord> levels = new List<LevelRecord>();
        public List<string> unlockedLevels = new List<string>();
        public List<ItemRecord> items = new List<ItemRecord>();
        public SettingsData settings = new SettingsData();
        public string lastSavedUtc = "";

        public LevelRecord Find(string id)
        {
            foreach (var r in levels) if (r.id == id) return r;
            return null;
        }

        public LevelRecord GetOrCreate(string id)
        {
            var r = Find(id);
            if (r != null) return r;
            r = new LevelRecord { id = id };
            levels.Add(r);
            return r;
        }

        public bool IsCompleted(string id)
        {
            var r = Find(id);
            return r != null && r.completed;
        }

        public bool IsUnlocked(string id) => unlockedLevels.Contains(id);

        public bool HasItem(ItemType type)
        {
            string t = type.ToString();
            foreach (var i in items) if (i.type == t) return true;
            return false;
        }

        public bool HasItemFrom(string levelId)
        {
            foreach (var i in items) if (i.levelId == levelId) return true;
            return false;
        }

        public int ItemCount(ItemType type)
        {
            string t = type.ToString();
            int n = 0;
            foreach (var i in items) if (i.type == t) n++;
            return n;
        }
    }

    [Serializable]
    public sealed class LevelRecord
    {
        public string id = "";
        public bool completed;
        public int stars;           // bitmask: Stars.Complete | Stars.Target | Stars.Mastery
        public int bestMoves;       // 0 = never completed
        public int timesCompleted;
        public bool crystalFound;
    }

    [Serializable]
    public sealed class ItemRecord
    {
        public string type = "";
        public string levelId = "";
    }

    [Serializable]
    public sealed class SettingsData
    {
        public bool sound = true;
    }

    public static class Stars
    {
        public const int Complete = 1;
        public const int Target = 2;
        public const int Mastery = 4;
        public const int All = 7;

        public static int Count(int mask)
        {
            int n = 0;
            if ((mask & Complete) != 0) n++;
            if ((mask & Target) != 0) n++;
            if ((mask & Mastery) != 0) n++;
            return n;
        }
    }

    public static class SaveMigrations
    {
        /// <summary>Upgrades an older save in place. Returns false if the save is from a newer build.</summary>
        public static bool Migrate(SaveData data)
        {
            if (data.version > SaveData.CurrentVersion) return false;
            // Example for the future:
            // if (data.version < 2) { ...convert fields...; data.version = 2; }
            if (data.levels == null) data.levels = new List<LevelRecord>();
            if (data.unlockedLevels == null) data.unlockedLevels = new List<string>();
            if (data.items == null) data.items = new List<ItemRecord>();
            if (data.settings == null) data.settings = new SettingsData();
            if (data.currentLevel == null) data.currentLevel = "";
            data.version = SaveData.CurrentVersion;
            return true;
        }
    }
}
