using System;
using System.Collections.Generic;
using PuzzleGame.Core;
using UnityEngine;

namespace PuzzleGame
{
    /// <summary>
    /// Runs one puzzle: owns the engine and state, applies inputs, keeps the undo
    /// history and tells the board what to animate. Logic is applied instantly;
    /// animation catches up (and is fast-forwarded by the next input).
    /// </summary>
    public sealed class PuzzleManager : MonoBehaviour
    {
        public GridManager Grid;
        public AudioManager Audio;

        public LevelData Level { get; private set; }
        public PuzzleEngine Engine { get; private set; }
        public PuzzleState State { get; private set; }
        public bool Active { get; private set; }
        public bool UsedUndo { get; private set; }
        public int UndoCount => _history.Count;

        /// <summary>Raised after anything that changes what the HUD shows.</summary>
        public event Action StateChanged;
        /// <summary>Raised once when the exit is reached (after the win animation).</summary>
        public event Action<RunStats> Completed;

        readonly MoveHistory _history = new MoveHistory(1000);
        readonly MoveOutcome _outcome = new MoveOutcome();
        readonly Queue<Direction> _buffer = new Queue<Direction>();
        bool _winPending;
        bool _inputLocked;

        public void Begin(LevelData level)
        {
            Level = level;
            Engine = new PuzzleEngine(level);
            State = Engine.CreateInitialState();
            _history.Clear();
            _buffer.Clear();
            UsedUndo = false;
            _winPending = false;
            _inputLocked = false;
            Active = true;
            Grid.Build(Engine, State);
            StateChanged?.Invoke();
        }

        public void End()
        {
            Active = false;
            _buffer.Clear();
            Grid.Clear();
        }

        /// <summary>Pauses/resumes input without tearing the level down.</summary>
        public void SetInputLocked(bool locked)
        {
            _inputLocked = locked;
            if (locked) _buffer.Clear();
        }

        public bool AcceptsInput => Active && !_inputLocked && !_winPending && State != null && !State.Won;

        /// <summary>Entry point for every control scheme (keys, swipes, on-screen pad).</summary>
        public void Move(Direction dir)
        {
            if (!AcceptsInput || dir == Direction.None) return;
            // While an echo replay is playing, hold one input so the replay can be read;
            // everything else is applied immediately.
            if (Grid.IsReplayingEcho)
            {
                if (_buffer.Count < 2) _buffer.Enqueue(dir);
                return;
            }
            Apply(dir);
        }

        void Apply(Direction dir)
        {
            var before = State.Clone();
            bool moved = Engine.Apply(State, dir, _outcome);
            if (moved) _history.Record(before);
            Grid.ShowMove(before, _outcome, State);
            if (moved) StateChanged?.Invoke();
            if (State.Won) _winPending = true;
        }

        public void Undo()
        {
            if (!Active || _winPending || _inputLocked || State == null) return;
            var prev = _history.Undo();
            if (prev == null)
            {
                Audio?.Play(Sfx.Bump, 0.4f);
                return;
            }
            _buffer.Clear();
            State = prev;
            UsedUndo = true;
            Grid.Snap(State, true);
            Audio?.Play(Sfx.Undo, 0.7f);
            StateChanged?.Invoke();
        }

        /// <summary>Back to the start. Undoable, so an accidental reset never loses a solution.</summary>
        public void ResetPuzzle()
        {
            if (!Active || _winPending || _inputLocked || State == null) return;
            if (State.Moves == 0 && _history.Count == 0) return;
            _buffer.Clear();
            _history.Record(State);
            State = Engine.CreateInitialState();
            Grid.Snap(State, true);
            Audio?.Play(Sfx.Reset, 0.7f);
            StateChanged?.Invoke();
        }

        public bool PickupTaken(int index) => State != null && index >= 0 && index < State.PickupTaken.Length && State.PickupTaken[index];

        void Update()
        {
            if (!Active) return;

            if (_buffer.Count > 0 && !Grid.IsReplayingEcho && AcceptsInput)
                Apply(_buffer.Dequeue());

            if (_winPending && !Grid.IsAnimating)
            {
                _winPending = false;
                _inputLocked = true;
                var run = new RunStats
                {
                    Moves = State.Moves,
                    UsedUndo = UsedUndo,
                    CrystalCollected = PickupTaken(Level.CrystalIndex),
                    ItemCollected = PickupTaken(Level.SpecialItemIndex),
                };
                Grid.PlayWin(() => Completed?.Invoke(run));
            }
        }

#if UNITY_EDITOR
        // ------------------------------------------------------------ development only

        List<Direction> _autoPath;
        float _autoTimer;

        /// <summary>Solves from the current state and plays the solution (dev panel).</summary>
        public bool DevAutoSolve(bool withCrystal)
        {
            if (!AcceptsInput) return false;
            int[] req = null;
            if (withCrystal && Level.CrystalIndex >= 0 && !PickupTaken(Level.CrystalIndex)) req = new[] { Level.CrystalIndex };
            var sol = PuzzleSolver.Solve(Engine, State, req);
            if (!sol.Solved)
            {
                Debug.LogWarning($"[Dev] No solution from the current state (truncated={sol.Truncated}). Try Reset.");
                return false;
            }
            Debug.Log($"[Dev] Solution ({sol.Moves} moves): {sol.PathString}");
            _autoPath = sol.Path;
            _autoTimer = 0f;
            return true;
        }

        void LateUpdate()
        {
            if (_autoPath == null || _autoPath.Count == 0) return;
            if (!AcceptsInput) { _autoPath = null; return; }
            _autoTimer -= Time.unscaledDeltaTime;
            if (_autoTimer > 0f || Grid.IsAnimating) return;
            _autoTimer = 0.12f;
            var d = _autoPath[0];
            _autoPath.RemoveAt(0);
            Move(d);
        }
#endif
    }
}
