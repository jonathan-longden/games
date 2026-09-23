namespace PuzzleGame.Core
{
    /// <summary>
    /// One attempt at a level: the engine, the live state and the undo history.
    /// Pure logic (no Unity), so undo/reset behaviour is unit-tested exactly as
    /// the game runs it. The Unity PuzzleManager only adds input and animation.
    ///
    /// Undo and reset are free and unlimited:
    ///  * every successful move can be undone, one step at a time;
    ///  * reset returns to the start and is itself recorded, so an accidental
    ///    reset can be undone too.
    /// </summary>
    public sealed class PuzzleSession
    {
        public const int HistoryCapacity = 1000;

        public readonly LevelData Level;
        public readonly PuzzleEngine Engine;
        public PuzzleState State { get; private set; }
        public bool UsedUndo { get; private set; }

        readonly MoveHistory _history = new MoveHistory(HistoryCapacity);

        public PuzzleSession(LevelData level)
        {
            Level = level;
            Engine = new PuzzleEngine(level);
            State = Engine.CreateInitialState();
        }

        public int UndoCount => _history.Count;
        public bool CanUndo => _history.CanUndo;

        /// <summary>True when the board differs from the level start (so a reset would do something).</summary>
        public bool CanReset => State.Moves > 0;

        /// <summary>
        /// Applies one input. Returns true if anything moved; <paramref name="before"/>
        /// receives the state prior to the move (for animation).
        /// </summary>
        public bool Move(Direction dir, MoveOutcome outcome, out PuzzleState before)
        {
            before = State.Clone();
            if (State.Won)
            {
                outcome?.Clear();
                return false;
            }
            bool moved = Engine.Apply(State, dir, outcome);
            if (moved) _history.Record(before);
            return moved;
        }

        public bool Undo()
        {
            if (State.Won) return false;
            var prev = _history.Undo();
            if (prev == null) return false;
            State = prev;
            UsedUndo = true;
            return true;
        }

        public bool Reset()
        {
            if (State.Won || !CanReset) return false;
            _history.Record(State);
            State = Engine.CreateInitialState();
            return true;
        }
    }
}
