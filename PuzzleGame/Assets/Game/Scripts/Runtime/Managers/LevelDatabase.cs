using System.Collections.Generic;
using PuzzleGame.Core;
using UnityEngine;

namespace PuzzleGame
{
    /// <summary>
    /// All level definitions, loaded from Resources/Levels/levels.txt (a TextAsset).
    /// Level order in the file is the order of the world map.
    /// </summary>
    public sealed class LevelDatabase
    {
        public const string ResourcePath = "Levels/levels";

        readonly List<LevelData> _levels = new List<LevelData>();
        readonly Dictionary<string, LevelData> _byId = new Dictionary<string, LevelData>();

        public IReadOnlyList<LevelData> All => _levels;
        public int Count => _levels.Count;

        public static LevelDatabase LoadFromResources()
        {
            var asset = Resources.Load<TextAsset>(ResourcePath);
            if (asset == null)
            {
                Debug.LogError($"[Levels] Missing Resources/{ResourcePath}.txt");
                return new LevelDatabase();
            }
            return FromText(asset.text);
        }

        public static LevelDatabase FromText(string text)
        {
            var db = new LevelDatabase();
            foreach (var l in LevelParser.ParseAll(text))
            {
                db._levels.Add(l);
                db._byId[l.Id] = l;
            }
            return db;
        }

        public LevelData Get(string id) => id != null && _byId.TryGetValue(id, out var l) ? l : null;

        public LevelData First => _levels.Count > 0 ? _levels[0] : null;

        public List<LevelData> ToList() => new List<LevelData>(_levels);

        /// <summary>
        /// Mechanics this level shows for the first time on the main path (blocks in
        /// level 4, plates/doors in 10, echo in 12). Used to point at the new object
        /// once, instead of showing a tutorial popup.
        /// </summary>
        public Mechanics NewMechanics(LevelData level)
        {
            if (level == null || !level.IsMainPath) return Mechanics.None;
            var seen = Mechanics.None;
            foreach (var l in _levels)
                if (l.IsMainPath && l.Number < level.Number) seen |= l.Mechanics;
            return level.Mechanics & ~seen & ~Mechanics.Pickups;
        }
    }
}
