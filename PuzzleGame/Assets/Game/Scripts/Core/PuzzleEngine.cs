using System;
using System.Collections.Generic;

namespace PuzzleGame.Core
{
    /// <summary>
    /// The rules of the game. Stateless apart from lookup tables built from a
    /// <see cref="LevelData"/>; all mutable data lives in <see cref="PuzzleState"/>.
    /// The same engine drives gameplay, the solver and the level validator, so
    /// "the validator says it is solvable" means exactly "the game can be solved".
    ///
    /// Rules:
    ///  * The player moves one cell. Walls, void and closed doors stop it (a bump,
    ///    which costs no move).
    ///  * Walking into a block or echo block pushes it one cell if the cell beyond
    ///    is free. Nothing can be pulled; only one object can be pushed at a time.
    ///  * A switch is pressed while a block or echo block rests on it.
    ///  * A door is open while every switch on its channel is pressed. A door that
    ///    something is standing in stays open (it cannot crush anything).
    ///  * Walking onto a pickup collects it.
    ///  * ECHO: every successful player move is remembered (the last N moves).
    ///    Stepping onto an echo rune makes every echo block replay those remembered
    ///    moves, one step at a time. An echo step that is blocked is skipped.
    ///    Echo blocks can push one ordinary block. The step onto the rune is
    ///    remembered after the replay, so what the memory shows is what will play.
    ///  * Reaching the goal completes the level.
    /// </summary>
    public sealed class PuzzleEngine
    {
        public readonly LevelData Level;

        readonly int _w, _h;
        readonly bool[] _solid;        // wall or void
        readonly int[] _doorChannel;   // -1 = no door
        readonly bool[] _rune;
        readonly int[] _pickupAt;      // -1 = none
        readonly int _channelCount;

        public PuzzleEngine(LevelData level)
        {
            Level = level;
            _w = level.Width;
            _h = level.Height;
            int n = _w * _h;
            _solid = new bool[n];
            _doorChannel = new int[n];
            _rune = new bool[n];
            _pickupAt = new int[n];
            for (int i = 0; i < n; i++)
            {
                _solid[i] = level.Tiles[i] != Tile.Floor;
                _doorChannel[i] = -1;
                _pickupAt[i] = -1;
            }
            foreach (var d in level.Doors) _doorChannel[Idx(d.Pos)] = d.Channel;
            foreach (var r in level.Runes) _rune[Idx(r)] = true;
            for (int i = 0; i < level.Pickups.Count; i++) _pickupAt[Idx(level.Pickups[i].Pos)] = i;
            int maxCh = -1;
            foreach (var s in level.Switches) maxCh = Math.Max(maxCh, s.Channel);
            foreach (var d in level.Doors) maxCh = Math.Max(maxCh, d.Channel);
            _channelCount = maxCh + 1;
        }

        int Idx(GridPos p) => p.Y * _w + p.X;
        bool InBounds(GridPos p) => p.X >= 0 && p.Y >= 0 && p.X < _w && p.Y < _h;

        public PuzzleState CreateInitialState()
        {
            return new PuzzleState
            {
                Player = Level.PlayerStart,
                Blocks = Level.Blocks.ToArray(),
                Echoes = Level.EchoBlocks.ToArray(),
                PickupTaken = new bool[Level.Pickups.Count],
            };
        }

        // ------------------------------------------------------------------ queries

        public int BlockAt(PuzzleState s, GridPos p)
        {
            for (int i = 0; i < s.Blocks.Length; i++) if (s.Blocks[i] == p) return i;
            return -1;
        }

        public int EchoAt(PuzzleState s, GridPos p)
        {
            for (int i = 0; i < s.Echoes.Length; i++) if (s.Echoes[i] == p) return i;
            return -1;
        }

        public bool IsOccupied(PuzzleState s, GridPos p) =>
            s.Player == p || BlockAt(s, p) >= 0 || EchoAt(s, p) >= 0;

        public bool IsSwitchPressed(PuzzleState s, int switchIndex)
        {
            var p = Level.Switches[switchIndex].Pos;
            return BlockAt(s, p) >= 0 || EchoAt(s, p) >= 0;
        }

        public bool IsChannelActive(PuzzleState s, int channel)
        {
            bool any = false;
            for (int i = 0; i < Level.Switches.Count; i++)
            {
                if (Level.Switches[i].Channel != channel) continue;
                any = true;
                if (!IsSwitchPressed(s, i)) return false;
            }
            return any;
        }

        public bool IsDoorOpen(PuzzleState s, int doorIndex)
        {
            var d = Level.Doors[doorIndex];
            return IsChannelActive(s, d.Channel) || IsOccupied(s, d.Pos);
        }

        /// <summary>True if nothing may enter this cell: wall, void or a closed door.</summary>
        public bool IsSolid(PuzzleState s, GridPos p)
        {
            if (!InBounds(p)) return true;
            int i = Idx(p);
            if (_solid[i]) return true;
            int ch = _doorChannel[i];
            if (ch >= 0 && !IsChannelActive(s, ch) && !IsOccupied(s, p)) return true;
            return false;
        }

        public bool IsRune(GridPos p) => InBounds(p) && _rune[Idx(p)];

        public int ChannelCount => _channelCount;

        // ------------------------------------------------------------------ rules

        /// <summary>
        /// Applies one player input to <paramref name="s"/> in place.
        /// Returns false (and leaves the state untouched) if nothing moved.
        /// <paramref name="outcome"/> is optional and receives animation data.
        /// </summary>
        public bool Apply(PuzzleState s, Direction dir, MoveOutcome outcome = null)
        {
            outcome?.Clear();
            if (outcome != null) outcome.Dir = dir;
            if (s.Won || dir == Direction.None) return false;

            s.Facing = dir;
            GridPos from = s.Player;
            GridPos to = from.Step(dir);

            if (IsSolid(s, to))
            {
                if (outcome != null) outcome.Bumped = true;
                return false;
            }

            int bi = BlockAt(s, to);
            int ei = bi < 0 ? EchoAt(s, to) : -1;
            if (bi >= 0 || ei >= 0)
            {
                GridPos beyond = to.Step(dir);
                if (IsSolid(s, beyond) || IsOccupied(s, beyond))
                {
                    if (outcome != null) { outcome.Bumped = true; outcome.BlockedPush = true; }
                    return false;
                }
                if (bi >= 0)
                {
                    s.Blocks[bi] = beyond;
                    if (outcome != null) outcome.Pushed = new EntityMove(EntityKind.Block, bi, to, beyond);
                }
                else
                {
                    s.Echoes[ei] = beyond;
                    if (outcome != null) outcome.Pushed = new EntityMove(EntityKind.Echo, ei, to, beyond);
                }
                s.Pushes++;
            }

            s.Player = to;
            s.Moves++;
            if (outcome != null)
            {
                outcome.Moved = true;
                outcome.PlayerFrom = from;
                outcome.PlayerTo = to;
            }

            int pk = _pickupAt[Idx(to)];
            if (pk >= 0 && !s.PickupTaken[pk])
            {
                s.PickupTaken[pk] = true;
                if (outcome != null) outcome.CollectedPickup = pk;
            }

            if (s.Echoes.Length > 0 && _rune[Idx(to)] && s.Memory.Count > 0)
            {
                if (outcome != null) outcome.EchoTriggered = true;
                ReplayEcho(s, outcome);
            }

            if (Level.EchoMemory > 0 && s.Echoes.Length > 0)
            {
                s.Memory.Add(dir);
                while (s.Memory.Count > Level.EchoMemory) s.Memory.RemoveAt(0);
            }

            if (s.Player == Level.Goal)
            {
                s.Won = true;
                if (outcome != null) outcome.Won = true;
            }
            return true;
        }

        readonly List<int> _order = new List<int>();

        void ReplayEcho(PuzzleState s, MoveOutcome outcome)
        {
            // Copy: the memory must not change while it plays.
            var seq = s.Memory.ToArray();
            for (int step = 0; step < seq.Length; step++)
            {
                Direction d = seq[step];
                EchoFrame frame = null;
                if (outcome != null)
                {
                    frame = new EchoFrame { Dir = d, MemoryIndex = step };
                    outcome.EchoFrames.Add(frame);
                }

                // Front-most echo first, so a line of echoes moves together.
                _order.Clear();
                for (int i = 0; i < s.Echoes.Length; i++) _order.Add(i);
                if (_order.Count > 1)
                {
                    var echoes = s.Echoes;
                    _order.Sort((a, b) => Forward(echoes[b], d).CompareTo(Forward(echoes[a], d)));
                }

                foreach (int i in _order)
                {
                    GridPos from = s.Echoes[i];
                    GridPos to = from.Step(d);
                    if (IsSolid(s, to) || s.Player == to || EchoAt(s, to) >= 0) continue;
                    int bi = BlockAt(s, to);
                    if (bi >= 0)
                    {
                        GridPos beyond = to.Step(d);
                        if (IsSolid(s, beyond) || IsOccupied(s, beyond)) continue;
                        s.Blocks[bi] = beyond;
                        frame?.Moves.Add(new EntityMove(EntityKind.Block, bi, to, beyond));
                    }
                    s.Echoes[i] = to;
                    frame?.Moves.Add(new EntityMove(EntityKind.Echo, i, from, to));
                }
            }
        }

        static int Forward(GridPos p, Direction d)
        {
            switch (d)
            {
                case Direction.Up: return -p.Y;
                case Direction.Down: return p.Y;
                case Direction.Left: return -p.X;
                case Direction.Right: return p.X;
                default: return 0;
            }
        }

        /// <summary>
        /// Where each echo block would travel if a rune were triggered right now
        /// (used to draw the ghost trail, so the player never has to guess).
        /// </summary>
        public List<GridPos>[] PreviewEcho(PuzzleState s)
        {
            var result = new List<GridPos>[s.Echoes.Length];
            for (int i = 0; i < result.Length; i++) result[i] = new List<GridPos> { s.Echoes[i] };
            if (s.Echoes.Length == 0 || s.Memory.Count == 0) return result;

            var sim = s.Clone();
            // When the replay really happens the player is standing on the rune.
            if (Level.Runes.Count == 1 && EchoAt(sim, Level.Runes[0]) < 0 && BlockAt(sim, Level.Runes[0]) < 0)
                sim.Player = Level.Runes[0];
            var outcome = new MoveOutcome();
            ReplayEcho(sim, outcome);
            foreach (var frame in outcome.EchoFrames)
            {
                foreach (var m in frame.Moves)
                    if (m.Kind == EntityKind.Echo) result[m.Index].Add(m.To);
            }
            return result;
        }
    }
}
