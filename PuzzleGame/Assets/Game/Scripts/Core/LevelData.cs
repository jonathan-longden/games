using System;
using System.Collections.Generic;

namespace PuzzleGame.Core
{
    public enum LevelKind
    {
        Normal,
        Hard,
        Bonus,
        Secret,
        Boss,
    }

    /// <summary>What the third star asks for.</summary>
    public enum MasteryType
    {
        None,
        Crystal,  // collect the level's crystal, then finish
        Par,      // finish in MasteryValue moves or fewer (the optimal solution)
        NoUndo,   // finish without using undo
    }

    /// <summary>Things lying on the floor that the player can pick up.</summary>
    public enum PickupType
    {
        Crystal,      // per-level mastery collectible (not kept in the inventory)
        Key,
        Gem,
        PuzzlePiece,
        Relic,
    }

    /// <summary>Special bonus items that persist in the player's collection.</summary>
    public enum ItemType
    {
        None = 0,
        Key,
        Gem,
        PuzzlePiece,
        Relic,
    }

    [Flags]
    public enum Mechanics
    {
        None = 0,
        Blocks = 1 << 0,
        Switches = 1 << 1,
        Doors = 1 << 2,
        Echo = 1 << 3,
        Pickups = 1 << 4,
    }

    [Serializable]
    public struct SwitchDef
    {
        public GridPos Pos;
        public int Channel;
        public SwitchDef(GridPos pos, int channel) { Pos = pos; Channel = channel; }
    }

    [Serializable]
    public struct DoorDef
    {
        public GridPos Pos;
        public int Channel;
        public DoorDef(GridPos pos, int channel) { Pos = pos; Channel = channel; }
    }

    [Serializable]
    public struct PickupDef
    {
        public GridPos Pos;
        public PickupType Type;
        public PickupDef(GridPos pos, PickupType type) { Pos = pos; Type = type; }
    }

    /// <summary>
    /// Immutable-by-convention definition of one puzzle. Everything the engine needs
    /// is here; nothing about a specific level is hard-coded anywhere else.
    /// </summary>
    [Serializable]
    public sealed class LevelData
    {
        // Identity / presentation
        public string Id = "";
        public int Number;              // display number for main-path levels, 0 for off-path levels
        public string Title = "";
        public string Hint = "";        // one short line shown when the level starts
        public string Region = "";
        public LevelKind Kind = LevelKind.Normal;
        public int Difficulty = 1;      // 1..5, display only

        // Board
        public int Width;
        public int Height;
        public Tile[] Tiles = Array.Empty<Tile>();
        public GridPos PlayerStart;
        public GridPos Goal;
        public List<GridPos> Blocks = new List<GridPos>();
        public List<GridPos> EchoBlocks = new List<GridPos>();
        public List<GridPos> Runes = new List<GridPos>();
        public List<SwitchDef> Switches = new List<SwitchDef>();
        public List<DoorDef> Doors = new List<DoorDef>();
        public List<PickupDef> Pickups = new List<PickupDef>();
        public int EchoMemory = 4;      // how many recent moves an echo block remembers

        // Goals and rewards
        public int MoveTarget;          // star 2
        public MasteryType Mastery = MasteryType.None; // star 3
        public int MasteryValue;
        public int CoinReward;          // 0 = use the default for this Kind

        // Progression / world map
        public string Requires = "";    // id of the level that must be completed first
        public ItemType RequiresItem = ItemType.None;
        public List<string> MapLinks = new List<string>(); // extra purely-visual path links
        public int MapRow;
        public float MapLane;

        public bool InBounds(GridPos p) => p.X >= 0 && p.Y >= 0 && p.X < Width && p.Y < Height;

        public Tile TileAt(GridPos p) => InBounds(p) ? Tiles[p.Y * Width + p.X] : Tile.Void;

        public bool IsMainPath => Kind == LevelKind.Normal || Kind == LevelKind.Hard || Kind == LevelKind.Boss;

        public Mechanics Mechanics
        {
            get
            {
                var m = Mechanics.None;
                if (Blocks.Count > 0) m |= Mechanics.Blocks;
                if (Switches.Count > 0) m |= Mechanics.Switches;
                if (Doors.Count > 0) m |= Mechanics.Doors;
                if (EchoBlocks.Count > 0) m |= Mechanics.Echo;
                if (Pickups.Count > 0) m |= Mechanics.Pickups;
                return m;
            }
        }

        public bool HasEcho => EchoBlocks.Count > 0;

        /// <summary>The special collection item hidden in this level, if any.</summary>
        public ItemType SpecialItem
        {
            get
            {
                foreach (var p in Pickups)
                {
                    var t = ToItem(p.Type);
                    if (t != ItemType.None) return t;
                }
                return ItemType.None;
            }
        }

        public int CrystalIndex
        {
            get
            {
                for (int i = 0; i < Pickups.Count; i++)
                    if (Pickups[i].Type == PickupType.Crystal) return i;
                return -1;
            }
        }

        public int SpecialItemIndex
        {
            get
            {
                for (int i = 0; i < Pickups.Count; i++)
                    if (ToItem(Pickups[i].Type) != ItemType.None) return i;
                return -1;
            }
        }

        public string DisplayName
        {
            get
            {
                switch (Kind)
                {
                    case LevelKind.Bonus: return "BONUS";
                    case LevelKind.Secret: return "SECRET";
                    default: return "LEVEL " + Number;
                }
            }
        }

        public string MasteryDescription
        {
            get
            {
                switch (Mastery)
                {
                    case MasteryType.Crystal: return "Find the crystal";
                    case MasteryType.Par: return "Perfect: " + MasteryValue + " moves or fewer";
                    case MasteryType.NoUndo: return "Finish without undo";
                    default: return "Complete the level";
                }
            }
        }

        public static ItemType ToItem(PickupType t)
        {
            switch (t)
            {
                case PickupType.Key: return ItemType.Key;
                case PickupType.Gem: return ItemType.Gem;
                case PickupType.PuzzlePiece: return ItemType.PuzzlePiece;
                case PickupType.Relic: return ItemType.Relic;
                default: return ItemType.None;
            }
        }
    }
}
