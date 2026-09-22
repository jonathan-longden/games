using System.Collections.Generic;
using System.Text;

namespace PuzzleGame.Core
{
    /// <summary>
    /// Development-time checks for level data: structural validity plus a full
    /// solver run. Used by edit-mode tests and the editor menu
    /// "Puzzle Game/Validate All Levels".
    /// </summary>
    public static class LevelValidator
    {
        public sealed class Report
        {
            public string LevelId;
            public readonly List<string> Errors = new List<string>();
            public readonly List<string> Warnings = new List<string>();
            public int OptimalMoves = -1;
            public string OptimalPath = "";
            public int CrystalMoves = -1;
            public int ItemMoves = -1;
            public int StatesExplored;

            public bool Ok => Errors.Count == 0;

            public override string ToString()
            {
                var sb = new StringBuilder();
                sb.Append($"[{LevelId}] {(Ok ? "OK" : "FAIL")}  optimal={OptimalMoves}");
                if (CrystalMoves >= 0) sb.Append($" crystal={CrystalMoves}");
                if (ItemMoves >= 0) sb.Append($" item={ItemMoves}");
                sb.Append($" states={StatesExplored}");
                foreach (var e in Errors) sb.Append("\n   ERROR: ").Append(e);
                foreach (var w in Warnings) sb.Append("\n   warn:  ").Append(w);
                return sb.ToString();
            }
        }

        public static List<Report> ValidateAll(IList<LevelData> levels, bool solve = true)
        {
            var reports = new List<Report>();
            var ids = new HashSet<string>();
            foreach (var l in levels)
            {
                var r = Validate(l, solve);
                if (!ids.Add(l.Id)) r.Errors.Add("duplicate level id");
                reports.Add(r);
            }
            // Progression graph checks.
            for (int i = 0; i < levels.Count; i++)
            {
                var l = levels[i];
                if (!string.IsNullOrEmpty(l.Requires) && !ids.Contains(l.Requires))
                    reports[i].Errors.Add($"requires unknown level '{l.Requires}'");
                foreach (var link in l.MapLinks)
                    if (!ids.Contains(link)) reports[i].Errors.Add($"map link to unknown level '{link}'");
                if (i > 0 && string.IsNullOrEmpty(l.Requires))
                    reports[i].Errors.Add("only the first level may have no 'requires'");
            }
            return reports;
        }

        public static Report Validate(LevelData l, bool solve = true)
        {
            var r = new Report { LevelId = l.Id };

            if (l.Width <= 0 || l.Height <= 0) r.Errors.Add("empty board");
            if (l.Width > 12) r.Warnings.Add($"board is {l.Width} wide; may be cramped in portrait");
            if (l.Height > 14) r.Warnings.Add($"board is {l.Height} tall; may be cramped in portrait");
            if (string.IsNullOrEmpty(l.Title)) r.Warnings.Add("no title");

            var used = new Dictionary<GridPos, string>();
            void Claim(GridPos p, string what)
            {
                if (l.TileAt(p) != Tile.Floor) r.Errors.Add($"{what} at {p} is not on a floor cell");
                if (used.TryGetValue(p, out var other)) r.Errors.Add($"{what} at {p} overlaps {other}");
                else used[p] = what;
            }

            Claim(l.PlayerStart, "player start");
            Claim(l.Goal, "goal");
            foreach (var b in l.Blocks) Claim(b, "block");
            foreach (var e in l.EchoBlocks) Claim(e, "echo block");
            foreach (var s in l.Switches) Claim(s.Pos, "switch");
            foreach (var d in l.Doors) Claim(d.Pos, "door");
            foreach (var ru in l.Runes) Claim(ru, "rune");
            foreach (var p in l.Pickups) Claim(p.Pos, p.Type.ToString());

            // Channels must pair up.
            var switchCh = new HashSet<int>();
            var doorCh = new HashSet<int>();
            foreach (var s in l.Switches) switchCh.Add(s.Channel);
            foreach (var d in l.Doors) doorCh.Add(d.Channel);
            foreach (int c in switchCh) if (!doorCh.Contains(c)) r.Errors.Add($"switch channel {c} controls no door");
            foreach (int c in doorCh) if (!switchCh.Contains(c)) r.Errors.Add($"door channel {c} has no switch");
            foreach (int c in switchCh)
            {
                int n = 0;
                foreach (var s in l.Switches) if (s.Channel == c) n++;
                if (n > l.Blocks.Count + l.EchoBlocks.Count)
                    r.Errors.Add($"channel {c} has {n} switches but only {l.Blocks.Count + l.EchoBlocks.Count} blocks");
            }

            if (l.EchoBlocks.Count > 0 && l.Runes.Count == 0) r.Errors.Add("echo block but no rune to trigger it");
            if (l.Runes.Count > 0 && l.EchoBlocks.Count == 0) r.Errors.Add("rune but no echo block");
            if (l.EchoBlocks.Count > 0 && (l.EchoMemory < 1 || l.EchoMemory > 8)) r.Errors.Add("echo memory must be 1..8");

            int crystals = 0, items = 0;
            foreach (var p in l.Pickups)
            {
                if (p.Type == PickupType.Crystal) crystals++;
                else items++;
            }
            if (crystals > 1) r.Errors.Add("more than one crystal");
            if (items > 1) r.Errors.Add("more than one special item");
            if (l.Mastery == MasteryType.Crystal && crystals == 0) r.Errors.Add("mastery is 'crystal' but there is no crystal");
            if (l.Mastery != MasteryType.Crystal && crystals > 0) r.Warnings.Add("crystal present but mastery is not 'crystal'");
            if (l.Mastery == MasteryType.None && (l.Kind == LevelKind.Normal || l.Kind == LevelKind.Hard))
                r.Errors.Add("normal levels need a mastery objective (star 3)");
            if (l.MoveTarget <= 0) r.Errors.Add("move target must be positive");
            if (!string.IsNullOrEmpty(l.Requires) && l.Requires == l.Id) r.Errors.Add("level requires itself");

            if (!r.Ok || !solve) return r;

            // Reachability of the goal on the empty board is a cheap early signal.
            if (!FloodReachable(l, l.PlayerStart, l.Goal))
                r.Errors.Add("goal is not connected to the start at all");

            var sol = PuzzleSolver.Solve(l);
            r.StatesExplored = sol.StatesExplored;
            if (sol.Truncated) r.Errors.Add("solver gave up (state space too large)");
            else if (!sol.Solved) r.Errors.Add("level has NO solution");
            else
            {
                r.OptimalMoves = sol.Moves;
                r.OptimalPath = sol.PathString;
                if (l.MoveTarget < sol.Moves)
                    r.Errors.Add($"move target {l.MoveTarget} is below the optimal solution ({sol.Moves})");
                else if (l.MoveTarget > sol.Moves * 2 + 10)
                    r.Warnings.Add($"move target {l.MoveTarget} is very generous (optimal {sol.Moves})");
                if (l.Mastery == MasteryType.Par)
                {
                    if (l.MasteryValue < sol.Moves)
                        r.Errors.Add($"par {l.MasteryValue} is impossible (optimal {sol.Moves})");
                    else if (l.MasteryValue > sol.Moves)
                        r.Warnings.Add($"par {l.MasteryValue} is above optimal {sol.Moves}");
                    if (l.MasteryValue >= l.MoveTarget)
                        r.Warnings.Add("par is not tighter than the move target");
                }
            }

            int ci = l.CrystalIndex;
            if (ci >= 0)
            {
                var cs = PuzzleSolver.Solve(l, new[] { ci });
                r.StatesExplored += cs.StatesExplored;
                if (!cs.Solved) r.Errors.Add(cs.Truncated ? "solver gave up on crystal objective" : "crystal cannot be collected in a winning run");
                else r.CrystalMoves = cs.Moves;
            }

            int ii = l.SpecialItemIndex;
            if (ii >= 0)
            {
                var its = PuzzleSolver.Solve(l, new[] { ii });
                r.StatesExplored += its.StatesExplored;
                if (!its.Solved) r.Errors.Add(its.Truncated ? "solver gave up on special item" : "special item cannot be collected in a winning run");
                else r.ItemMoves = its.Moves;
            }

            return r;
        }

        static bool FloodReachable(LevelData l, GridPos from, GridPos to)
        {
            var seen = new HashSet<GridPos> { from };
            var q = new Queue<GridPos>();
            q.Enqueue(from);
            while (q.Count > 0)
            {
                var p = q.Dequeue();
                if (p == to) return true;
                foreach (var d in DirectionUtil.All)
                {
                    var n = p.Step(d);
                    if (l.TileAt(n) == Tile.Floor && seen.Add(n)) q.Enqueue(n);
                }
            }
            return false;
        }
    }
}
