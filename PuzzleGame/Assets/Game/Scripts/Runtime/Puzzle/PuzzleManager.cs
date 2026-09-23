using System;
using System.Collections.Generic;
using PuzzleGame.Core;
using UnityEngine;

namespace PuzzleGame
{
    /// <summary>
    /// Runs one puzzle for the game: wraps a Core <see cref="PuzzleSession"/>
    /// (rules, undo, reset) and tells the board what to animate.
    ///
    /// Input is never delayed: every move is applied to the puzzle state the
    /// instant it arrives, and whatever is still animating (including an echo
    /// replay) is fast-forwarded to its end first.
    /// </summary>
    public sealed class PuzzleManager : MonoBehaviour
    {
        public GridManager Grid;
        public AudioManager Audio;

        PuzzleSession _session;

        public LevelData Level => _session?.Level;
        public PuzzleEngine Engine => _session?.Engine;
        public PuzzleState State => _session?.State;
        public bool Active { get; private set; }
        public bool UsedUndo => _session != null && _session.UsedUndo;
        public int UndoCount => _session?.UndoCount ?? 0;
        public bool CanUndo => AcceptsInput && _session.CanUndo;
        public bool CanReset => AcceptsInput && _session.CanReset;

        /// <summary>Raised after anything that changes what the HUD shows.</summary>
        public event Action StateChanged;
        /// <summary>Raised once when the exit is reached (after the win animation).</summary>
        public event Action<RunStats> Completed;

        readonly MoveOutcome _outcome = new MoveOutcome();
        bool _winPending;
        bool _inputLocked;

        public void Begin(LevelData level)
        {
            _session = new PuzzleSession(level);
            _winPending = false;
            _inputLocked = false;
            Active = true;
            Grid.Build(_session.Engine, _session.State);
            StateChanged?.Invoke();
        }

        public void End()
        {
            Active = false;
            Grid.Clear();
        }

        /// <summary>Pauses/resumes input without tearing the level down.</summary>
        public void SetInputLocked(bool locked) => _inputLocked = locked;

        public bool AcceptsInput => Active && !_inputLocked && !_winPending && _session != null && !_session.State.Won;

        /// <summary>Entry point for every control scheme (keys, swipes, on-screen pad).</summary>
        public void Move(Direction dir)
        {
            if (!AcceptsInput || dir == Direction.None) return;
            bool moved = _session.Move(dir, _outcome, out var before);
            Grid.ShowMove(before, _outcome, _session.State);
            if (moved) StateChanged?.Invoke();
            if (_session.State.Won) _winPending = true;
        }

        public void Undo()
        {
            if (!AcceptsInput) return;
            if (!_session.Undo())
            {
                Grid.NothingToUndo();
                return;
            }
            Grid.Snap(_session.State, true);
            Audio?.Play(Sfx.Undo, 0.7f);
            StateChanged?.Invoke();
        }

        /// <summary>Back to the start. Undoable, so an accidental reset never loses a solution.</summary>
        public void ResetPuzzle()
        {
            if (!AcceptsInput || !_session.Reset()) return;
            Grid.Snap(_session.State, true);
            Grid.ResetFlash();
            Audio?.Play(Sfx.Reset, 0.7f);
            StateChanged?.Invoke();
        }

        public bool PickupTaken(int index)
        {
            var s = State;
            return s != null && index >= 0 && index < s.PickupTaken.Length && s.PickupTaken[index];
        }

        void Update()
        {
            if (!Active || !_winPending || Grid.IsAnimating) return;
            _winPending = false;
            _inputLocked = true;
            var s = _session.State;
            var run = new RunStats
            {
                Moves = s.Moves,
                UsedUndo = _session.UsedUndo,
                CrystalCollected = PickupTaken(Level.CrystalIndex),
                ItemCollected = PickupTaken(Level.SpecialItemIndex),
            };
            Grid.PlayWin(() => Completed?.Invoke(run));
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
