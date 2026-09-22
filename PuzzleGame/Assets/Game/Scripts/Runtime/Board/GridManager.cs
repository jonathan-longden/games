using System;
using System.Collections.Generic;
using PuzzleGame.Core;
using UnityEngine;

namespace PuzzleGame
{
    /// <summary>
    /// Builds and animates the visual board for a level. It never decides rules:
    /// it is told what happened (a <see cref="MoveOutcome"/>) and shows it.
    /// Echo replays are shown step by step; any new input fast-forwards whatever
    /// is still animating so controls always feel immediate.
    /// </summary>
    public sealed class GridManager : MonoBehaviour
    {
        public const float MoveDuration = 0.1f;
        public const float EchoStepDuration = 0.13f;

        public Camera Cam;
        public Fx Fx;
        public AudioManager Audio;
        /// <summary>Screen-pixel rect the board must fit in (supplied by the UI).</summary>
        public Func<Rect> BoardArea;

        /// <summary>Fires with the replayed sequence and the index being replayed (-1 when a replay ends).</summary>
        public event Action<Direction[], int> EchoStep;

        PuzzleEngine _engine;
        LevelData _level;
        Transform _root;
        PlayerAvatar _player;
        readonly List<Block> _blocks = new List<Block>();
        readonly List<EchoBlock> _echoes = new List<EchoBlock>();
        readonly List<Switch> _switches = new List<Switch>();
        readonly List<Door> _doors = new List<Door>();
        readonly List<Pickup> _pickups = new List<Pickup>();
        readonly List<EchoRune> _runes = new List<EchoRune>();
        Goal _goal;
        readonly List<SpriteRenderer> _ghostDots = new List<SpriteRenderer>();
        readonly List<SpriteRenderer> _ghostEnds = new List<SpriteRenderer>();

        PuzzleState _shown;   // the state currently represented on screen (lags the logic during animations)

        sealed class Step
        {
            public float Duration;
            public Action Begin;
            public Action End;
        }

        readonly Queue<Step> _steps = new Queue<Step>();
        Step _current;
        float _stepTime;
        int _lastW, _lastH;

        public bool IsAnimating => _current != null || _steps.Count > 0;
        public bool IsReplayingEcho { get; private set; }
        public PlayerAvatar Player => _player;

        // ------------------------------------------------------------ build

        public void Build(PuzzleEngine engine, PuzzleState state)
        {
            Clear();
            _engine = engine;
            _level = engine.Level;
            _root = new GameObject("Board").transform;
            _root.SetParent(transform, false);

            BuildTiles();

            _goal = Create<Goal>("Goal", _level.Goal);
            _goal.Fx = Fx;
            _goal.Build();

            foreach (var r in _level.Runes)
            {
                var rune = Create<EchoRune>("Rune", r);
                rune.Build();
                _runes.Add(rune);
            }
            foreach (var s in _level.Switches)
            {
                var sw = Create<Switch>("Switch", s.Pos);
                sw.Build(s.Channel);
                _switches.Add(sw);
            }
            foreach (var d in _level.Doors)
            {
                var door = Create<Door>("Door", d.Pos);
                door.Build(d.Channel);
                _doors.Add(door);
            }
            foreach (var p in _level.Pickups)
            {
                var pk = Create<Pickup>("Pickup", p.Pos);
                pk.Build(p.Type);
                _pickups.Add(pk);
            }
            for (int i = 0; i < state.Blocks.Length; i++)
            {
                var b = Create<Block>("Block", state.Blocks[i]);
                b.Build();
                _blocks.Add(b);
            }
            for (int i = 0; i < state.Echoes.Length; i++)
            {
                var e = Create<EchoBlock>("EchoBlock", state.Echoes[i]);
                e.Build();
                _echoes.Add(e);
            }
            _player = Create<PlayerAvatar>("Player", state.Player);
            _player.Build();

            Snap(state, false);
            FitCamera();
        }

        T Create<T>(string name, GridPos cell) where T : BoardPiece
        {
            var go = new GameObject(name);
            go.transform.SetParent(_root, false);
            var c = go.AddComponent<T>();
            c.Place(cell, CellToWorld(cell));
            return c;
        }

        void BuildTiles()
        {
            var tiles = new GameObject("Tiles").transform;
            tiles.SetParent(_root, false);
            for (int y = 0; y < _level.Height; y++)
            {
                for (int x = 0; x < _level.Width; x++)
                {
                    var p = new GridPos(x, y);
                    var t = _level.TileAt(p);
                    if (t == Tile.Void) continue;
                    var pos = CellToWorld(p);
                    if (t == Tile.Floor)
                    {
                        bool odd = ((x + y) & 1) == 1;
                        Sprite(tiles, "Shadow", SpriteFactory.Tile, Theme.BoardShadow, Layers.Shadow, pos + new Vector3(0, -0.12f), 1.02f);
                        Sprite(tiles, "Floor", SpriteFactory.Tile, Theme.FloorEdge, Layers.Floor, pos, 1f);
                        Sprite(tiles, "FloorTop", SpriteFactory.Tile, odd ? Theme.FloorA : Theme.FloorB, Layers.FloorTop, pos + new Vector3(0, 0.02f), 0.9f);
                    }
                    else
                    {
                        Sprite(tiles, "Shadow", SpriteFactory.Tile, Theme.BoardShadow, Layers.Shadow, pos + new Vector3(0, -0.14f), 1.02f);
                        Sprite(tiles, "Wall", SpriteFactory.Tile, Theme.WallBase, Layers.Floor, pos, 1f);
                        Sprite(tiles, "WallTop", SpriteFactory.Tile, Theme.WallTop, Layers.FloorTop, pos + new Vector3(0, 0.06f), 0.84f);
                        // A faint rim where the wall meets open floor above it reads as depth.
                        var above = new GridPos(x, y - 1);
                        if (_level.TileAt(above) != Tile.Wall)
                            Sprite(tiles, "Rim", SpriteFactory.Square, Theme.WallRim.WithAlpha(0.6f), Layers.FloorTop + 1,
                                pos + new Vector3(0, 0.44f), 1f, new Vector3(0.62f, 0.05f, 1f));
                    }
                }
            }
        }

        static void Sprite(Transform parent, string name, Sprite s, Color c, int order, Vector3 pos, float scale, Vector3? scale3 = null)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localScale = scale3 ?? Vector3.one * scale;
            var r = go.AddComponent<SpriteRenderer>();
            r.sprite = s;
            r.color = c;
            r.sortingOrder = order;
        }

        public void Clear()
        {
            _steps.Clear();
            _current = null;
            IsReplayingEcho = false;
            if (_root != null) Destroy(_root.gameObject);
            _root = null;
            _blocks.Clear();
            _echoes.Clear();
            _switches.Clear();
            _doors.Clear();
            _pickups.Clear();
            _runes.Clear();
            _ghostDots.Clear();
            _ghostEnds.Clear();
            _player = null;
            _goal = null;
            Fx?.Clear();
        }

        public Vector3 CellToWorld(GridPos p)
        {
            if (_level == null) return Vector3.zero;
            return new Vector3(p.X - (_level.Width - 1) * 0.5f, -(p.Y - (_level.Height - 1) * 0.5f), 0f);
        }

        // ------------------------------------------------------------ state display

        /// <summary>Jumps every piece to the given state (level start, undo, reset).</summary>
        public void Snap(PuzzleState state, bool softly)
        {
            FastForward();
            _shown = state.Clone();
            float d = softly ? 0.08f : 0f;
            _player.SlideTo(state.Player, CellToWorld(state.Player), d);
            _player.Face(state.Facing);
            _player.ResetVisual();
            _player.SetHidden(false);
            for (int i = 0; i < _blocks.Count; i++) _blocks[i].SlideTo(state.Blocks[i], CellToWorld(state.Blocks[i]), d);
            for (int i = 0; i < _echoes.Count; i++) _echoes[i].SlideTo(state.Echoes[i], CellToWorld(state.Echoes[i]), d);
            RefreshMechanisms(false);
            RefreshPickups();
            RefreshGhost();
        }

        /// <summary>Queues the animation of one player input.</summary>
        public void ShowMove(PuzzleState before, MoveOutcome o, PuzzleState after)
        {
            FastForward();
            _shown = before.Clone();

            if (!o.Moved)
            {
                if (o.Bumped)
                {
                    _player.Bump(o.Dir);
                    Audio?.Play(Sfx.Bump, o.BlockedPush ? 0.8f : 0.5f);
                }
                return;
            }

            var pushed = o.Pushed;
            int pickup = o.CollectedPickup;
            _steps.Enqueue(new Step
            {
                Duration = MoveDuration,
                Begin = () =>
                {
                    _player.Face(o.Dir);
                    _player.SlideTo(o.PlayerTo, CellToWorld(o.PlayerTo), MoveDuration);
                    _shown.Player = o.PlayerTo;
                    if (pushed.HasValue)
                    {
                        var m = pushed.Value;
                        PieceFor(m)?.SlideTo(m.To, CellToWorld(m.To), MoveDuration);
                        ApplyToShown(m);
                        Audio?.Play(Sfx.Push, 0.7f);
                        Fx?.Dust(CellToWorld(m.To), DirVec(o.Dir), Theme.StoneLight.WithAlpha(0.5f));
                    }
                    else
                    {
                        Audio?.Play(Sfx.Move, 0.6f);
                    }
                },
                End = () =>
                {
                    if (pickup >= 0)
                    {
                        _shown.PickupTaken[pickup] = true;
                        var type = _level.Pickups[pickup].Type;
                        Audio?.Play(type == PickupType.Crystal ? Sfx.Pickup : Sfx.Item);
                        var col = type == PickupType.Crystal ? Theme.Cyan : SpriteFactory.ItemColor(LevelData.ToItem(type));
                        Fx?.Burst(CellToWorld(o.PlayerTo), col, 16, 3.5f, 0.2f, 0.6f);
                        Fx?.Ring(CellToWorld(o.PlayerTo), col);
                        RefreshPickups();
                    }
                    RefreshMechanisms(true);
                },
            });

            if (o.EchoTriggered)
            {
                var sequence = new Direction[o.EchoFrames.Count];
                for (int i = 0; i < sequence.Length; i++) sequence[i] = o.EchoFrames[i].Dir;
                _steps.Enqueue(new Step
                {
                    Duration = 0.06f,
                    Begin = () =>
                    {
                        IsReplayingEcho = true;
                        foreach (var r in _runes) if (r.Cell == o.PlayerTo) r.Flash();
                        Audio?.Play(Sfx.Echo, 0.8f);
                        foreach (var e in _echoes)
                        {
                            e.Flash();
                            Fx?.Ring(e.transform.position, Theme.Violet, 1.3f, 0.4f);
                        }
                    },
                });
                foreach (var frame in o.EchoFrames)
                {
                    var f = frame;
                    _steps.Enqueue(new Step
                    {
                        Duration = EchoStepDuration,
                        Begin = () =>
                        {
                            EchoStep?.Invoke(sequence, f.MemoryIndex);
                            foreach (var m in f.Moves)
                            {
                                PieceFor(m)?.SlideTo(m.To, CellToWorld(m.To), EchoStepDuration * 0.9f);
                                ApplyToShown(m);
                                if (m.Kind == EntityKind.Echo)
                                    Fx?.Dust(CellToWorld(m.To), DirVec(f.Dir), Theme.Violet.WithAlpha(0.6f));
                            }
                            if (f.Moves.Count > 0) Audio?.Play(Sfx.Move, 0.45f, 1.35f);
                        },
                        End = () => RefreshMechanisms(true),
                    });
                }
                _steps.Enqueue(new Step
                {
                    Duration = 0.01f,
                    Begin = () =>
                    {
                        IsReplayingEcho = false;
                        EchoStep?.Invoke(sequence, -1);
                    },
                });
            }

            var final = after.Clone();
            _steps.Enqueue(new Step
            {
                Duration = 0f,
                Begin = () =>
                {
                    _shown = final;
                    RefreshMechanisms(true);
                    RefreshPickups();
                    RefreshGhost();
                },
            });

            // Start immediately so the first frame of motion happens this frame.
            Advance(0f);
        }

        void ApplyToShown(EntityMove m)
        {
            switch (m.Kind)
            {
                case EntityKind.Block: _shown.Blocks[m.Index] = m.To; break;
                case EntityKind.Echo: _shown.Echoes[m.Index] = m.To; break;
                case EntityKind.Player: _shown.Player = m.To; break;
            }
        }

        BoardPiece PieceFor(EntityMove m)
        {
            switch (m.Kind)
            {
                case EntityKind.Block: return m.Index < _blocks.Count ? _blocks[m.Index] : null;
                case EntityKind.Echo: return m.Index < _echoes.Count ? _echoes[m.Index] : null;
                default: return _player;
            }
        }

        void RefreshMechanisms(bool animate)
        {
            if (_engine == null || _shown == null) return;
            bool anyOn = false, anyOff = false, anyOpen = false, anyClose = false;
            for (int i = 0; i < _switches.Count; i++)
            {
                bool pressed = _engine.IsSwitchPressed(_shown, i);
                if (pressed != _switches[i].Pressed)
                {
                    if (pressed) anyOn = true; else anyOff = true;
                    _switches[i].SetPressed(pressed, animate);
                    if (animate && pressed) Fx?.Ring(_switches[i].transform.position, Theme.Channel(_level.Switches[i].Channel), 1.4f, 0.4f);
                }
            }
            for (int i = 0; i < _doors.Count; i++)
            {
                bool open = _engine.IsDoorOpen(_shown, i);
                if (open != _doors[i].Open)
                {
                    if (open) anyOpen = true; else anyClose = true;
                    _doors[i].SetOpen(open, animate);
                }
            }
            for (int i = 0; i < _blocks.Count; i++) _blocks[i].SetCharged(ChannelUnder(_shown.Blocks[i]));
            for (int i = 0; i < _echoes.Count; i++) _echoes[i].SetCharged(ChannelUnder(_shown.Echoes[i]));

            if (!animate || Audio == null) return;
            if (anyOn) Audio.Play(Sfx.SwitchOn, 0.8f);
            else if (anyOff) Audio.Play(Sfx.SwitchOff, 0.6f);
            if (anyOpen) Audio.Play(Sfx.DoorOpen, 0.7f);
            else if (anyClose) Audio.Play(Sfx.DoorClose, 0.6f);
        }

        Color? ChannelUnder(GridPos p)
        {
            foreach (var s in _level.Switches)
                if (s.Pos == p) return Theme.Channel(s.Channel);
            return null;
        }

        void RefreshPickups()
        {
            for (int i = 0; i < _pickups.Count; i++) _pickups[i].SetTaken(_shown.PickupTaken[i]);
        }

        /// <summary>
        /// Faint trail showing exactly where each echo block would travel if a rune
        /// were stepped on now. The echo is powerful; it should never be a surprise.
        /// </summary>
        void RefreshGhost()
        {
            if (_engine == null || _echoes.Count == 0) return;
            var paths = _engine.PreviewEcho(_shown);
            int dot = 0, end = 0;
            foreach (var path in paths)
            {
                for (int i = 1; i < path.Count; i++)
                {
                    var a = CellToWorld(path[i - 1]);
                    var b = CellToWorld(path[i]);
                    for (int k = 1; k <= 3; k++)
                    {
                        var r = GhostDot(dot++);
                        r.transform.localPosition = Vector3.Lerp(a, b, k / 3f);
                        r.transform.localScale = Vector3.one * (k == 3 ? 0.16f : 0.1f);
                    }
                }
                if (path.Count > 1 && path[path.Count - 1] != path[0])
                {
                    var r = GhostEnd(end++);
                    r.transform.localPosition = CellToWorld(path[path.Count - 1]);
                }
            }
            for (int i = dot; i < _ghostDots.Count; i++) _ghostDots[i].enabled = false;
            for (int i = end; i < _ghostEnds.Count; i++) _ghostEnds[i].enabled = false;
        }

        SpriteRenderer GhostDot(int i)
        {
            while (_ghostDots.Count <= i)
            {
                var go = new GameObject("GhostDot");
                go.transform.SetParent(_root, false);
                var r = go.AddComponent<SpriteRenderer>();
                r.sprite = SpriteFactory.Circle64;
                r.color = Theme.Violet.WithAlpha(0.55f);
                r.sortingOrder = Layers.Ghost;
                _ghostDots.Add(r);
            }
            _ghostDots[i].enabled = true;
            return _ghostDots[i];
        }

        SpriteRenderer GhostEnd(int i)
        {
            while (_ghostEnds.Count <= i)
            {
                var go = new GameObject("GhostEnd");
                go.transform.SetParent(_root, false);
                go.transform.localScale = Vector3.one * 0.86f;
                var r = go.AddComponent<SpriteRenderer>();
                r.sprite = SpriteFactory.TileOutline;
                r.color = Theme.Violet.WithAlpha(0.5f);
                r.sortingOrder = Layers.Ghost;
                _ghostEnds.Add(r);
            }
            _ghostEnds[i].enabled = true;
            return _ghostEnds[i];
        }

        // ------------------------------------------------------------ animation queue

        /// <summary>Completes every queued animation instantly.</summary>
        public void FastForward()
        {
            int guard = 0;
            while ((_current != null || _steps.Count > 0) && guard++ < 1000)
            {
                if (_current == null)
                {
                    _current = _steps.Dequeue();
                    _current.Begin?.Invoke();
                }
                var c = _current;
                _current = null;
                c.End?.Invoke();
            }
            _player?.FinishMotion();
            foreach (var b in _blocks) b.FinishMotion();
            foreach (var e in _echoes) e.FinishMotion();
            IsReplayingEcho = false;
        }

        void Advance(float dt)
        {
            int guard = 0;
            while (guard++ < 64)
            {
                if (_current == null)
                {
                    if (_steps.Count == 0) return;
                    _current = _steps.Dequeue();
                    _stepTime = 0f;
                    _current.Begin?.Invoke();
                }
                _stepTime += dt;
                dt = 0f;
                if (_stepTime < _current.Duration) return;
                var c = _current;
                _current = null;
                c.End?.Invoke();
            }
        }

        void Update()
        {
            Advance(Time.unscaledDeltaTime);
            if (Screen.width != _lastW || Screen.height != _lastH) FitCamera();
        }

        // ------------------------------------------------------------ effects

        public void PlayWin(Action done)
        {
            FastForward();
            if (_goal == null || _player == null)
            {
                done?.Invoke();
                return;
            }
            _goal.Celebrate();
            Fx?.Burst(_goal.transform.position, Theme.Gold, 26, 5f, 0.24f, 0.8f);
            Fx?.Ring(_goal.transform.position, Theme.Gold, 2.4f, 0.6f);
            Audio?.Play(Sfx.Win);
            _player.EnterPortal(() =>
            {
                if (_player != null) _player.SetHidden(true);
                done?.Invoke();
            });
        }

        static Vector2 DirVec(Direction d)
        {
            switch (d)
            {
                case Direction.Up: return Vector2.up;
                case Direction.Down: return Vector2.down;
                case Direction.Left: return Vector2.left;
                case Direction.Right: return Vector2.right;
                default: return Vector2.zero;
            }
        }

        // ------------------------------------------------------------ camera

        public void FitCamera()
        {
            _lastW = Screen.width;
            _lastH = Screen.height;
            if (Cam == null || _level == null) return;
            float sw = Mathf.Max(1, Screen.width), sh = Mathf.Max(1, Screen.height);
            Rect area = BoardArea != null ? BoardArea() : new Rect(0, sh * 0.3f, sw, sh * 0.5f);
            if (area.width < 10 || area.height < 10) area = new Rect(0, sh * 0.3f, sw, sh * 0.5f);

            float boardW = _level.Width + 0.4f;
            float boardH = _level.Height + 0.6f;
            float ortho = Mathf.Max(boardW * sh / (2f * area.width), boardH * sh / (2f * area.height));
            // Do not blow tiny boards up to giant tiles.
            ortho = Mathf.Max(ortho, 7.2f * sh / (2f * sw));
            Cam.orthographicSize = ortho;
            float worldPerPixel = 2f * ortho / sh;
            var c = area.center;
            Cam.transform.position = new Vector3((sw * 0.5f - c.x) * worldPerPixel, (sh * 0.5f - c.y) * worldPerPixel, -10f);
        }
    }
}
