using System.Collections.Generic;

namespace PuzzleGame.Core
{
    /// <summary>
    /// Undo stack. Before every successful move the previous state is pushed.
    /// Puzzle states are tiny, so full snapshots are simpler and safer than
    /// reversible commands (and they cover echo replays for free).
    /// </summary>
    public sealed class MoveHistory
    {
        readonly List<PuzzleState> _stack = new List<PuzzleState>();
        readonly int _capacity;

        public MoveHistory(int capacity = 512) { _capacity = capacity; }

        public int Count => _stack.Count;
        public bool CanUndo => _stack.Count > 0;

        public void Record(PuzzleState before)
        {
            _stack.Add(before.Clone());
            if (_stack.Count > _capacity) _stack.RemoveAt(0);
        }

        /// <summary>Returns the previous state, or null if there is nothing to undo.</summary>
        public PuzzleState Undo()
        {
            if (_stack.Count == 0) return null;
            var s = _stack[_stack.Count - 1];
            _stack.RemoveAt(_stack.Count - 1);
            return s;
        }

        public void Clear() => _stack.Clear();
    }
}
