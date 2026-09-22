using System;
using System.Collections.Generic;

namespace PuzzleGame.Core
{
    /// <summary>
    /// The complete mutable state of a puzzle in progress. Small enough that the
    /// undo system simply stores a clone per move.
    /// </summary>
    public sealed class PuzzleState
    {
        public GridPos Player;
        public Direction Facing = Direction.Down;
        public GridPos[] Blocks;
        public GridPos[] Echoes;
        public bool[] PickupTaken;
        /// <summary>Most recent successful player moves, oldest first, at most EchoMemory long.</summary>
        public List<Direction> Memory = new List<Direction>();
        public int Moves;
        public int Pushes;
        public bool Won;

        public PuzzleState Clone()
        {
            return new PuzzleState
            {
                Player = Player,
                Facing = Facing,
                Blocks = (GridPos[])Blocks.Clone(),
                Echoes = (GridPos[])Echoes.Clone(),
                PickupTaken = (bool[])PickupTaken.Clone(),
                Memory = new List<Direction>(Memory),
                Moves = Moves,
                Pushes = Pushes,
                Won = Won,
            };
        }
    }

    public enum EntityKind
    {
        Player,
        Block,
        Echo,
    }

    [Serializable]
    public struct EntityMove
    {
        public EntityKind Kind;
        public int Index;
        public GridPos From;
        public GridPos To;

        public EntityMove(EntityKind kind, int index, GridPos from, GridPos to)
        {
            Kind = kind; Index = index; From = from; To = to;
        }
    }

    /// <summary>One step of an echo replay: the direction replayed and everything that moved.</summary>
    public sealed class EchoFrame
    {
        public Direction Dir;
        public int MemoryIndex;
        public readonly List<EntityMove> Moves = new List<EntityMove>();
    }

    /// <summary>Everything that happened during a single player input, for animation/audio.</summary>
    public sealed class MoveOutcome
    {
        public Direction Dir;
        public bool Moved;
        public bool Bumped;
        public bool BlockedPush;          // tried to push something that could not move
        public GridPos PlayerFrom;
        public GridPos PlayerTo;
        public EntityMove? Pushed;        // block or echo pushed by the player
        public int CollectedPickup = -1;
        public bool EchoTriggered;
        public readonly List<EchoFrame> EchoFrames = new List<EchoFrame>();
        public bool Won;

        public void Clear()
        {
            Dir = Direction.None;
            Moved = Bumped = BlockedPush = EchoTriggered = Won = false;
            Pushed = null;
            CollectedPickup = -1;
            EchoFrames.Clear();
        }
    }
}
