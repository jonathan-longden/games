using System;
using System.Collections.Generic;
using System.Text;

namespace PuzzleGame.Core
{
    /// <summary>
    /// Breadth-first solver used by the validator, the tests and the editor dev
    /// panel ("auto-solve"). BFS gives the optimal (fewest moves) solution.
    /// Identical blocks are interchangeable, so block positions are sorted in the
    /// state key; echo memory is part of the key only when the level has echoes.
    /// </summary>
    public static class PuzzleSolver
    {
        public sealed class Result
        {
            public bool Solved;
            public bool Truncated;      // gave up after MaxStates
            public int Moves = -1;
            public List<Direction> Path = new List<Direction>();
            public int StatesExplored;

            public string PathString
            {
                get
                {
                    var sb = new StringBuilder(Path.Count);
                    foreach (var d in Path) sb.Append(d.ToChar());
                    return sb.ToString();
                }
            }
        }

        public const int DefaultMaxStates = 3_000_000;

        /// <param name="requiredPickups">
        /// Indices into LevelData.Pickups that must be collected before reaching the goal
        /// (e.g. the crystal for the mastery star). Null = just reach the goal.
        /// </param>
        public static Result Solve(LevelData level, IList<int> requiredPickups = null, int maxStates = DefaultMaxStates)
        {
            var engine = new PuzzleEngine(level);
            var start = engine.CreateInitialState();
            return Solve(engine, start, requiredPickups, maxStates);
        }

        public static Result Solve(PuzzleEngine engine, PuzzleState start, IList<int> requiredPickups = null,
            int maxStates = DefaultMaxStates)
        {
            var level = engine.Level;
            var result = new Result();
            int requiredMask = 0;
            if (requiredPickups != null)
                foreach (int i in requiredPickups) requiredMask |= 1 << i;

            bool trackMemory = level.HasEcho && level.Runes.Count > 0;
            var keyer = new StateKeyer(level, requiredMask, trackMemory);

            var visited = new HashSet<StateKey>();
            var parents = new List<int>();
            var dirs = new List<Direction>();
            var queue = new Queue<(PuzzleState state, int node)>();

            var s0 = start.Clone();
            s0.Won = false;
            visited.Add(keyer.Make(s0));
            parents.Add(-1);
            dirs.Add(Direction.None);
            queue.Enqueue((s0, 0));

            while (queue.Count > 0)
            {
                var (state, node) = queue.Dequeue();
                result.StatesExplored++;

                foreach (var d in DirectionUtil.All)
                {
                    var next = state.Clone();
                    if (!engine.Apply(next, d)) continue;

                    if (next.Won)
                    {
                        if ((PickupMask(next) & requiredMask) != requiredMask)
                            continue; // reached the exit too early for this objective
                        var path = new List<Direction> { d };
                        int n = node;
                        while (n > 0) { path.Add(dirs[n]); n = parents[n]; }
                        path.Reverse();
                        result.Solved = true;
                        result.Path = path;
                        result.Moves = path.Count;
                        return result;
                    }

                    if (!visited.Add(keyer.Make(next))) continue;
                    int id = parents.Count;
                    parents.Add(node);
                    dirs.Add(d);
                    if (parents.Count > maxStates)
                    {
                        result.Truncated = true;
                        return result;
                    }
                    queue.Enqueue((next, id));
                }
            }
            return result;
        }

        static int PickupMask(PuzzleState s)
        {
            int m = 0;
            for (int i = 0; i < s.PickupTaken.Length && i < 31; i++) if (s.PickupTaken[i]) m |= 1 << i;
            return m;
        }

        /// <summary>Order-independent, compact identity of a puzzle state.</summary>
        readonly struct StateKey : IEquatable<StateKey>
        {
            readonly ulong _a, _b;
            readonly string _long; // fallback for states that do not fit in 128 bits

            public StateKey(ulong a, ulong b, string longKey) { _a = a; _b = b; _long = longKey; }

            public bool Equals(StateKey o) => _a == o._a && _b == o._b && _long == o._long;
            public override bool Equals(object obj) => obj is StateKey k && Equals(k);
            public override int GetHashCode()
            {
                unchecked
                {
                    ulong h = _a * 0x9E3779B97F4A7C15UL ^ (_b + 0x632BE59BD9B4E019UL + (_a << 6) + (_a >> 2));
                    int hs = (int)(h ^ (h >> 32));
                    return _long == null ? hs : hs ^ _long.GetHashCode();
                }
            }
        }

        sealed class StateKeyer
        {
            readonly int _width;
            readonly int _requiredMask;
            readonly bool _trackMemory;
            readonly bool _fits;
            readonly int[] _tmp;

            public StateKeyer(LevelData level, int requiredMask, bool trackMemory)
            {
                _width = level.Width;
                _requiredMask = requiredMask;
                _trackMemory = trackMemory;
                int bytes = 1 + level.Blocks.Count + level.EchoBlocks.Count + 1 + (trackMemory ? 3 : 0);
                _fits = level.Width * level.Height < 255 && bytes <= 16 && (!trackMemory || level.EchoMemory <= 8);
                _tmp = new int[Math.Max(level.Blocks.Count, level.EchoBlocks.Count)];
            }

            int Cell(GridPos p) => p.Y * _width + p.X + 1;

            public StateKey Make(PuzzleState s)
            {
                if (!_fits) return new StateKey(0, 0, LongKey(s));
                ulong a = 0, b = 0;
                int shift = 0;
                void Put(int v)
                {
                    if (shift < 64) a |= (ulong)(uint)v << shift;
                    else b |= (ulong)(uint)v << (shift - 64);
                    shift += 8;
                }
                Put(Cell(s.Player));
                PutSorted(s.Blocks, Put);
                PutSorted(s.Echoes, Put);
                Put((PickupMask(s) & _requiredMask) & 0xFF);
                if (_trackMemory)
                {
                    int m = s.Memory.Count;
                    for (int i = 0; i < s.Memory.Count; i++) m = m * 5 + (int)s.Memory[i];
                    // count (<=8) and up to 8 base-5 digits fit comfortably in 24 bits
                    Put(m & 0xFF); Put((m >> 8) & 0xFF); Put((m >> 16) & 0xFF);
                }
                return new StateKey(a, b, null);
            }

            void PutSorted(GridPos[] arr, Action<int> put)
            {
                int n = arr.Length;
                for (int i = 0; i < n; i++) _tmp[i] = Cell(arr[i]);
                Array.Sort(_tmp, 0, n);
                for (int i = 0; i < n; i++) put(_tmp[i]);
            }

            string LongKey(PuzzleState s)
            {
                var sb = new StringBuilder(32);
                sb.Append((char)Cell(s.Player)).Append('|');
                var list = new List<int>();
                foreach (var p in s.Blocks) list.Add(Cell(p));
                list.Sort();
                foreach (int v in list) sb.Append((char)v);
                sb.Append('|');
                list.Clear();
                foreach (var p in s.Echoes) list.Add(Cell(p));
                list.Sort();
                foreach (int v in list) sb.Append((char)v);
                sb.Append('|').Append((char)((PickupMask(s) & _requiredMask) + 1));
                if (_trackMemory)
                {
                    sb.Append('|');
                    foreach (var d in s.Memory) sb.Append((char)('0' + (int)d));
                }
                return sb.ToString();
            }
        }
    }
}
