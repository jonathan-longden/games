using System;
using System.Collections.Generic;
using System.Globalization;

namespace PuzzleGame.Core
{
    /// <summary>
    /// Parses the plain-text level format (Assets/Game/Resources/Levels/levels.txt).
    ///
    /// <code>
    /// level 7                     // id (numbers for the main path, e.g. B1 / S1 for off-path)
    /// title: Two Stones
    /// kind: Normal                // Normal | Hard | Bonus | Secret | Boss
    /// difficulty: 2
    /// region: The Sunken Steps
    /// hint: Blocks can be pushed, never pulled.
    /// target: 22                  // star 2 move target
    /// mastery: crystal            // crystal | par N | noundo | none   (star 3)
    /// coins: 0                    // 0 = default for kind
    /// echo: 4                     // echo memory length
    /// requires: 6                 // level that unlocks this one
    /// requires-item: Key
    /// map: 7 -0.4                 // map row, lane (-1..1)
    /// links: 14                   // visual-only extra path links
    /// board:
    /// #######
    /// #P.B.G#
    /// #######
    /// end
    /// </code>
    ///
    /// Board legend:
    ///   #  wall        .  floor        (space) void
    ///   P  player      G  goal/exit    B  block
    ///   E  echo block  R  echo rune
    ///   x y z  switch (channel 0,1,2)  X Y Z  door (channel 0,1,2)
    ///   c  crystal     k key   g gem   p puzzle piece   r relic
    /// Lines starting with // are comments.
    /// </summary>
    public static class LevelParser
    {
        public sealed class ParseException : Exception
        {
            public ParseException(int line, string msg) : base($"levels line {line}: {msg}") { }
        }

        public static List<LevelData> ParseAll(string text)
        {
            var result = new List<LevelData>();
            var lines = text.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
            LevelData current = null;
            List<string> boardRows = null;
            int boardStartLine = 0;

            for (int i = 0; i < lines.Length; i++)
            {
                int lineNo = i + 1;
                string raw = lines[i];

                if (boardRows != null)
                {
                    if (raw.Trim() == "end")
                    {
                        BuildBoard(current, boardRows, boardStartLine);
                        result.Add(current);
                        current = null;
                        boardRows = null;
                    }
                    else
                    {
                        boardRows.Add(raw.TrimEnd());
                    }
                    continue;
                }

                string line = raw.Trim();
                if (line.Length == 0 || line.StartsWith("//")) continue;

                if (line.StartsWith("level "))
                {
                    if (current != null) throw new ParseException(lineNo, $"level '{current.Id}' has no board/end");
                    current = new LevelData { Id = line.Substring(6).Trim() };
                    if (int.TryParse(current.Id, out int n)) current.Number = n;
                    continue;
                }

                if (current == null) throw new ParseException(lineNo, "expected 'level <id>'");

                if (line == "board:")
                {
                    boardRows = new List<string>();
                    boardStartLine = lineNo + 1;
                    continue;
                }

                int colon = line.IndexOf(':');
                if (colon < 0) throw new ParseException(lineNo, $"expected 'key: value', got '{line}'");
                string key = line.Substring(0, colon).Trim().ToLowerInvariant();
                string value = line.Substring(colon + 1).Trim();
                ApplyField(current, key, value, lineNo);
            }

            if (boardRows != null) throw new ParseException(lines.Length, $"level '{current?.Id}' board missing 'end'");
            if (current != null) throw new ParseException(lines.Length, $"level '{current.Id}' has no board");
            return result;
        }

        static void ApplyField(LevelData l, string key, string value, int lineNo)
        {
            try
            {
                switch (key)
                {
                    case "title": l.Title = value; break;
                    case "hint": l.Hint = value; break;
                    case "region": l.Region = value; break;
                    case "kind": l.Kind = (LevelKind)Enum.Parse(typeof(LevelKind), value, true); break;
                    case "difficulty": l.Difficulty = ParseInt(value); break;
                    case "target": l.MoveTarget = ParseInt(value); break;
                    case "coins": l.CoinReward = ParseInt(value); break;
                    case "echo": l.EchoMemory = ParseInt(value); break;
                    case "requires": l.Requires = value; break;
                    case "requires-item": l.RequiresItem = (ItemType)Enum.Parse(typeof(ItemType), value, true); break;
                    case "number": l.Number = ParseInt(value); break;
                    case "mastery":
                    {
                        var parts = value.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        switch (parts.Length > 0 ? parts[0].ToLowerInvariant() : "none")
                        {
                            case "crystal": l.Mastery = MasteryType.Crystal; break;
                            case "par":
                                l.Mastery = MasteryType.Par;
                                l.MasteryValue = parts.Length > 1 ? ParseInt(parts[1]) : 0;
                                break;
                            case "noundo": l.Mastery = MasteryType.NoUndo; break;
                            case "none": l.Mastery = MasteryType.None; break;
                            default: throw new FormatException("unknown mastery " + value);
                        }
                        break;
                    }
                    case "map":
                    {
                        var parts = value.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        l.MapRow = ParseInt(parts[0]);
                        l.MapLane = parts.Length > 1 ? float.Parse(parts[1], CultureInfo.InvariantCulture) : 0f;
                        break;
                    }
                    case "links":
                        foreach (var s in value.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries))
                            l.MapLinks.Add(s);
                        break;
                    default:
                        throw new FormatException("unknown field '" + key + "'");
                }
            }
            catch (ParseException) { throw; }
            catch (Exception e)
            {
                throw new ParseException(lineNo, $"level '{l.Id}' field '{key}': {e.Message}");
            }
        }

        static int ParseInt(string s) => int.Parse(s, NumberStyles.Integer, CultureInfo.InvariantCulture);

        static void BuildBoard(LevelData l, List<string> rows, int firstLine)
        {
            if (rows.Count == 0) throw new ParseException(firstLine, $"level '{l.Id}' has an empty board");
            int w = 0;
            foreach (var r in rows) w = Math.Max(w, r.Length);
            int h = rows.Count;
            l.Width = w;
            l.Height = h;
            l.Tiles = new Tile[w * h];
            l.PlayerStart = GridPos.Invalid;
            l.Goal = GridPos.Invalid;

            for (int y = 0; y < h; y++)
            {
                string row = rows[y];
                for (int x = 0; x < w; x++)
                {
                    char c = x < row.Length ? row[x] : ' ';
                    var p = new GridPos(x, y);
                    Tile t = Tile.Floor;
                    switch (c)
                    {
                        case ' ': t = Tile.Void; break;
                        case '#': t = Tile.Wall; break;
                        case '.': break;
                        case 'P':
                            if (l.PlayerStart.IsValid) throw new ParseException(firstLine + y, $"level '{l.Id}' has two players");
                            l.PlayerStart = p; break;
                        case 'G':
                            if (l.Goal.IsValid) throw new ParseException(firstLine + y, $"level '{l.Id}' has two goals");
                            l.Goal = p; break;
                        case 'B': l.Blocks.Add(p); break;
                        case 'E': l.EchoBlocks.Add(p); break;
                        case 'R': l.Runes.Add(p); break;
                        case 'x': l.Switches.Add(new SwitchDef(p, 0)); break;
                        case 'y': l.Switches.Add(new SwitchDef(p, 1)); break;
                        case 'z': l.Switches.Add(new SwitchDef(p, 2)); break;
                        case 'X': l.Doors.Add(new DoorDef(p, 0)); break;
                        case 'Y': l.Doors.Add(new DoorDef(p, 1)); break;
                        case 'Z': l.Doors.Add(new DoorDef(p, 2)); break;
                        case 'c': l.Pickups.Add(new PickupDef(p, PickupType.Crystal)); break;
                        case 'k': l.Pickups.Add(new PickupDef(p, PickupType.Key)); break;
                        case 'g': l.Pickups.Add(new PickupDef(p, PickupType.Gem)); break;
                        case 'p': l.Pickups.Add(new PickupDef(p, PickupType.PuzzlePiece)); break;
                        case 'r': l.Pickups.Add(new PickupDef(p, PickupType.Relic)); break;
                        default:
                            throw new ParseException(firstLine + y, $"level '{l.Id}' unknown board char '{c}'");
                    }
                    l.Tiles[y * w + x] = t;
                }
            }

            if (!l.PlayerStart.IsValid) throw new ParseException(firstLine, $"level '{l.Id}' has no player (P)");
            if (!l.Goal.IsValid) throw new ParseException(firstLine, $"level '{l.Id}' has no goal (G)");
        }
    }
}
