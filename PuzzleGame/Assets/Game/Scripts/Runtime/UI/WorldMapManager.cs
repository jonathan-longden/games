using System.Collections.Generic;
using PuzzleGame.Core;
using UnityEngine;
using UnityEngine.UI;

namespace PuzzleGame
{
    /// <summary>
    /// The world map: a glowing path winding upward through the dark, one node per
    /// level, with branches to bonus and secret levels. Layout comes entirely from
    /// level data (map row / lane / requires / links), so new levels appear automatically.
    /// </summary>
    public sealed class WorldMapManager : UIScreen
    {
        const float RowSpacing = 250f;
        const float BottomPad = 480f;
        const float TopPad = 1000f;
        const float LaneWidth = 330f;
        const float NodeSize = 150f;

        sealed class Node
        {
            public LevelData Level;
            public RectTransform Root;
            public Image Glow, Body, Ring, Lock;
            public Text Number, Title;
            public readonly Image[] Stars = new Image[3];
            public Vector2 Pos;
        }

        sealed class Segment
        {
            public string From, To;
            public bool VisualOnly;
            public readonly List<Image> Dots = new List<Image>();
        }

        ScrollRect _scroll;
        RectTransform _viewport, _content, _marker, _fog;
        Text _fogText;
        Image _markerBody;
        readonly Dictionary<string, Node> _nodes = new Dictionary<string, Node>();
        readonly List<Segment> _segments = new List<Segment>();
        Vector2 _startPos;
        float _contentHeight;
        bool _built;
        Text _stars, _coins;
        readonly Dictionary<ItemType, Text> _items = new Dictionary<ItemType, Text>();
        readonly Dictionary<ItemType, Image> _itemIcons = new Dictionary<ItemType, Image>();
        UIButton _continue;
        LevelData _continueLevel;
        readonly HashSet<string> _animating = new HashSet<string>();

        protected override void Build()
        {
            _viewport = UIFactory.Rect("Viewport", RT).Fill();
            _viewport.gameObject.AddComponent<RectMask2D>();
            var hit = _viewport.gameObject.AddComponent<Image>();
            hit.color = Color.clear; // catches drags anywhere on the map
            _content = UIFactory.Rect("Content", _viewport);
            _content.anchorMin = new Vector2(0, 0);
            _content.anchorMax = new Vector2(1, 0);
            _content.pivot = new Vector2(0.5f, 0);
            _scroll = RT.gameObject.AddComponent<ScrollRect>();
            _scroll.viewport = _viewport;
            _scroll.content = _content;
            _scroll.horizontal = false;
            _scroll.vertical = true;
            _scroll.movementType = ScrollRect.MovementType.Elastic;
            _scroll.inertia = true;
            _scroll.decelerationRate = 0.12f;
            _scroll.scrollSensitivity = 45f;

            BuildTopBar();

            _continue = UIFactory.Button(RT, "Continue", "PLAY", Theme.ButtonPrimary, () =>
            {
                if (_continueLevel != null) UI.ShowLevelInfo(_continueLevel);
            }, null, 50, new Color(0.03f, 0.05f, 0.16f));
            ((RectTransform)_continue.transform).Place(new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 50), new Vector2(640, 140));
        }

        void BuildTopBar()
        {
            var bar = UIFactory.Rect("TopBar", RT).Band(1f, 1f, 0, 170);
            var bg = UIFactory.Image(bar, "Bg", SpriteFactory.FadeUp, new Color(0.03f, 0.03f, 0.1f, 0.95f));
            bg.rectTransform.Fill(0, -60, 0, 0); // dark at the top, fading out downward

            var home = UIFactory.Button(bar, "Home", "", Theme.Button, () => Game.OpenMainMenu(), SpriteFactory.Home);
            ((RectTransform)home.transform).Place(new Vector2(0, 1), new Vector2(0, 1), new Vector2(28, -24), new Vector2(120, 120));

            _stars = UIFactory.Chip(bar, "Stars", SpriteFactory.Star, Theme.Gold, "0", 40, new Vector2(0, 1), new Vector2(290, -84), 220);
            _coins = UIFactory.Chip(bar, "Coins", SpriteFactory.Coin, Theme.Gold, "0", 40, new Vector2(0, 1), new Vector2(510, -84), 200);
            float x = 660;
            foreach (ItemType t in new[] { ItemType.Key, ItemType.Gem, ItemType.PuzzlePiece, ItemType.Relic })
            {
                var label = UIFactory.Chip(bar, t.ToString(), SpriteFactory.ForItem(t), SpriteFactory.ItemColor(t), "0", 34, new Vector2(0, 1), new Vector2(x, -84), 100);
                _items[t] = label;
                _itemIcons[t] = label.transform.parent.Find("Icon").GetComponent<Image>();
                x += 105;
            }
        }

        // ------------------------------------------------------------ layout

        void BuildContent()
        {
            _built = true;
            var levels = Game.Levels.All;
            int maxRow = 0;
            foreach (var l in levels) maxRow = Mathf.Max(maxRow, l.MapRow);
            _contentHeight = BottomPad + maxRow * RowSpacing + TopPad;
            _content.sizeDelta = new Vector2(0, _contentHeight);
            _content.anchoredPosition = Vector2.zero;

            Decorate(maxRow);

            _startPos = new Vector2(0, BottomPad - RowSpacing * 0.9f);
            foreach (var l in levels) _nodes[l.Id] = null;

            // Paths first so they render beneath nodes.
            foreach (var l in levels)
            {
                var to = NodePos(l);
                if (string.IsNullOrEmpty(l.Requires)) AddSegment("", l.Id, _startPos, to, false);
                else
                {
                    var from = Game.Levels.Get(l.Requires);
                    if (from != null) AddSegment(from.Id, l.Id, NodePos(from), to, false);
                }
                foreach (var link in l.MapLinks)
                {
                    var target = Game.Levels.Get(link);
                    if (target != null) AddSegment(l.Id, target.Id, to, NodePos(target), true);
                }
            }

            BuildStart();
            foreach (var l in levels) _nodes[l.Id] = BuildNode(l);
            BuildRegionLabels();

            _fog = UIFactory.Rect("Fog", _content);
            _fog.anchorMin = new Vector2(0, 0);
            _fog.anchorMax = new Vector2(1, 0);
            _fog.pivot = new Vector2(0.5f, 0);
            var fogImg = UIFactory.Image(_fog, "Gradient", SpriteFactory.FadeUp, new Color(0.03f, 0.03f, 0.1f, 0.92f));
            fogImg.rectTransform.Fill(-100, 0, -100, 0);
            _fogText = UIFactory.Text(_fog, "Teaser", "The path climbs on into the dark...", 38, Theme.TextDim.WithAlpha(0.8f), TextAnchor.MiddleCenter, FontStyle.Italic);
            _fogText.rectTransform.Place(new Vector2(0.5f, 0), new Vector2(0.5f, 0.5f), new Vector2(0, 260), new Vector2(900, 80));

            _marker = UIFactory.Rect("Marker", _content);
            _marker.anchorMin = _marker.anchorMax = new Vector2(0.5f, 0);
            _marker.sizeDelta = new Vector2(90, 90);
            var mg = UIFactory.Image(_marker, "Glow", SpriteFactory.Glow, Theme.Cyan.WithAlpha(0.6f));
            mg.rectTransform.Fill(-50, -50, -50, -50);
            _markerBody = UIFactory.Image(_marker, "Body", SpriteFactory.Circle64, Theme.Player);
            _markerBody.rectTransform.Fill();
            var eyeCol = new Color(0.1f, 0.12f, 0.3f);
            var e1 = UIFactory.Image(_marker, "EyeL", SpriteFactory.Circle64, eyeCol);
            e1.rectTransform.Place(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-16, 2), new Vector2(16, 16));
            var e2 = UIFactory.Image(_marker, "EyeR", SpriteFactory.Circle64, eyeCol);
            e2.rectTransform.Place(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(16, 2), new Vector2(16, 16));
        }

        Vector2 NodePos(LevelData l) => new Vector2(l.MapLane * LaneWidth, BottomPad + (l.MapRow - 1) * RowSpacing);

        void Decorate(int maxRow)
        {
            var rnd = new System.Random(11);
            for (int i = 0; i < 90; i++)
            {
                float y = (float)rnd.NextDouble() * _contentHeight;
                float x = ((float)rnd.NextDouble() - 0.5f) * 1000f;
                bool star = rnd.NextDouble() < 0.35;
                var img = UIFactory.Image(_content, "Deco", star ? SpriteFactory.Star : SpriteFactory.Glow,
                    (i % 4 == 0 ? Theme.Violet : Theme.Cyan).WithAlpha(0.05f + (float)rnd.NextDouble() * 0.18f));
                float s = star ? 10f + (float)rnd.NextDouble() * 18f : 30f + (float)rnd.NextDouble() * 120f;
                img.rectTransform.anchorMin = img.rectTransform.anchorMax = new Vector2(0.5f, 0);
                img.rectTransform.anchoredPosition = new Vector2(x, y);
                img.rectTransform.sizeDelta = new Vector2(s, s);
            }
            // Large soft region glows give each band of the climb its own colour.
            Color[] regionCols = { Theme.Cyan, Theme.Violet, Theme.Pink, Theme.Gold };
            for (int r = 0; r * 5 <= maxRow; r++)
            {
                var g = UIFactory.Image(_content, "RegionGlow", SpriteFactory.Glow, regionCols[r % regionCols.Length].WithAlpha(0.07f));
                g.rectTransform.anchorMin = g.rectTransform.anchorMax = new Vector2(0.5f, 0);
                g.rectTransform.anchoredPosition = new Vector2(r % 2 == 0 ? -250 : 250, BottomPad + (r * 5 + 2) * RowSpacing);
                g.rectTransform.sizeDelta = new Vector2(1400, 1400);
            }
        }

        void AddSegment(string from, string to, Vector2 a, Vector2 b, bool visualOnly)
        {
            var seg = new Segment { From = from, To = to, VisualOnly = visualOnly };
            float len = Vector2.Distance(a, b);
            float step = visualOnly ? 44f : 30f;
            int n = Mathf.Max(2, Mathf.FloorToInt(len / step));
            // A gentle curve so the path meanders instead of zig-zagging.
            var mid = (a + b) * 0.5f + new Vector2((b.y - a.y) * 0.12f * (a.x <= b.x ? 1 : -1), 0);
            for (int i = 1; i < n; i++)
            {
                float t = i / (float)n;
                var p = Bezier(a, mid, b, t);
                var dot = UIFactory.Image(_content, "Dot", SpriteFactory.Circle64, Theme.TextDim.WithAlpha(0.2f));
                dot.rectTransform.anchorMin = dot.rectTransform.anchorMax = new Vector2(0.5f, 0);
                dot.rectTransform.anchoredPosition = p;
                float size = visualOnly ? 12f : 16f;
                dot.rectTransform.sizeDelta = new Vector2(size, size);
                seg.Dots.Add(dot);
            }
            _segments.Add(seg);
        }

        static Vector2 Bezier(Vector2 a, Vector2 c, Vector2 b, float t)
        {
            float u = 1f - t;
            return u * u * a + 2f * u * t * c + t * t * b;
        }

        void BuildStart()
        {
            var rt = UIFactory.Rect("Start", _content);
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0);
            rt.anchoredPosition = _startPos;
            rt.sizeDelta = new Vector2(260, 100);
            var glow = UIFactory.Image(rt, "Glow", SpriteFactory.Glow, Theme.Cyan.WithAlpha(0.25f));
            glow.rectTransform.Fill(-120, -120, -120, -120);
            var bg = UIFactory.Image(rt, "Bg", SpriteFactory.Panel, Theme.PanelLight);
            bg.rectTransform.Fill();
            var ol = UIFactory.Image(rt, "Outline", SpriteFactory.PanelOutline, Theme.Cyan.WithAlpha(0.7f));
            ol.rectTransform.Fill();
            var t = UIFactory.Text(rt, "Text", "START", 40, Theme.Cyan, TextAnchor.MiddleCenter, FontStyle.Bold);
            t.rectTransform.Fill();
        }

        Node BuildNode(LevelData l)
        {
            var n = new Node { Level = l, Pos = NodePos(l) };
            float size = l.Kind == LevelKind.Boss ? NodeSize * 1.3f : NodeSize;
            n.Root = UIFactory.Rect("Node " + l.Id, _content);
            n.Root.anchorMin = n.Root.anchorMax = new Vector2(0.5f, 0);
            n.Root.anchoredPosition = n.Pos;
            n.Root.sizeDelta = new Vector2(size, size);

            bool diamond = l.Kind == LevelKind.Bonus || l.Kind == LevelKind.Secret;
            n.Glow = UIFactory.Image(n.Root, "Glow", SpriteFactory.Glow, Color.clear);
            n.Glow.rectTransform.Fill(-size * 0.6f, -size * 0.6f, -size * 0.6f, -size * 0.6f);
            n.Body = UIFactory.Image(n.Root, "Body", diamond ? SpriteFactory.Diamond : SpriteFactory.Circle64, Theme.PanelLight, true);
            n.Body.rectTransform.Fill(diamond ? -22 : 0, diamond ? -22 : 0, diamond ? -22 : 0, diamond ? -22 : 0);
            n.Ring = UIFactory.Image(n.Root, "Ring", diamond ? SpriteFactory.Rune : SpriteFactory.RingThick, Theme.TextDim);
            n.Ring.rectTransform.Fill(diamond ? -22 : -4, diamond ? -22 : -4, diamond ? -22 : -4, diamond ? -22 : -4);

            string label = l.Kind == LevelKind.Bonus ? "B" : l.Kind == LevelKind.Secret ? "?" : l.Number.ToString();
            n.Number = UIFactory.Text(n.Root, "Number", label, l.Kind == LevelKind.Boss ? 72 : 58, Theme.Text, TextAnchor.MiddleCenter, FontStyle.Bold);
            n.Number.rectTransform.Fill();
            n.Lock = UIFactory.Image(n.Root, "Lock", SpriteFactory.Lock, Theme.TextDim.WithAlpha(0.8f));
            n.Lock.rectTransform.Place(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(size * 0.42f, size * 0.42f));

            for (int i = 0; i < 3; i++)
            {
                var s = UIFactory.Image(n.Root, "Star" + i, SpriteFactory.Star, Theme.StarOff);
                s.rectTransform.Place(new Vector2(0.5f, 0), new Vector2(0.5f, 1), new Vector2((i - 1) * 40f, i == 1 ? -2 : 6), new Vector2(38, 38));
                n.Stars[i] = s;
            }

            bool right = l.MapLane <= 0.1f;
            n.Title = UIFactory.Text(n.Root, "Title", l.Title, 30, Theme.TextDim, right ? TextAnchor.MiddleLeft : TextAnchor.MiddleRight);
            n.Title.rectTransform.Place(new Vector2(right ? 1 : 0, 0.5f), new Vector2(right ? 0 : 1, 0.5f), new Vector2(right ? 24 : -24, 0), new Vector2(330, 90));

            if (l.Kind == LevelKind.Boss)
            {
                var crown = UIFactory.Image(n.Root, "Crown", SpriteFactory.Star, Theme.Crimson);
                crown.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0.5f, 0), new Vector2(0, 6), new Vector2(64, 64));
            }

            var btn = n.Body.gameObject.AddComponent<UIButton>();
            btn.Background = null;
            btn.Animate = false;
            btn.Clicked = () => OnNodeTapped(n);
            return n;
        }

        void BuildRegionLabels()
        {
            string last = null;
            foreach (var l in Game.Levels.All)
            {
                if (!l.IsMainPath || string.IsNullOrEmpty(l.Region) || l.Region == last) continue;
                last = l.Region;
                var t = UIFactory.Text(_content, "Region", l.Region.ToUpperInvariant(), 34, Theme.Text.WithAlpha(0.28f), TextAnchor.MiddleCenter, FontStyle.Bold);
                t.rectTransform.anchorMin = t.rectTransform.anchorMax = new Vector2(0.5f, 0);
                t.rectTransform.sizeDelta = new Vector2(900, 60);
                t.rectTransform.anchoredPosition = new Vector2(0, NodePos(l).y - RowSpacing * 0.5f);
                t.transform.SetAsFirstSibling();
            }
        }

        // ------------------------------------------------------------ state

        public void Show(LevelData focus)
        {
            if (!_built) BuildContent();

            var pending = new List<string>(Game.Progress.PendingUnlockAnimations);
            Game.Progress.PendingUnlockAnimations.Clear();
            _animating.Clear();
            foreach (var id in pending) _animating.Add(id);

            Refresh();
            var current = Game.Progress.CurrentLevel;
            PlaceMarker(current);

            UIFactory.Fade(gameObject, true, 0.25f);
            Canvas.ForceUpdateCanvases();
            ScrollTo((focus ?? current), false);

            if (pending.Count > 0) PlayUnlocks(pending);
        }

        void Refresh()
        {
            var p = Game.Progress;
            _stars.text = $"{p.TotalStars}/{p.MaxStars}";
            _coins.text = Game.Currency.Coins.ToString();
            foreach (var kv in _items)
            {
                int c = p.ItemCount(kv.Key);
                kv.Value.text = c.ToString();
                kv.Value.color = c > 0 ? Theme.Text : Theme.TextDim.WithAlpha(0.5f);
                _itemIcons[kv.Key].color = SpriteFactory.ItemColor(kv.Key).WithAlpha(c > 0 ? 1f : 0.25f);
            }

            float highestUnlocked = BottomPad;
            foreach (var n in _nodes.Values)
            {
                bool unlocked = p.IsUnlocked(n.Level) && !_animating.Contains(n.Level.Id);
                ApplyNode(n, unlocked);
                if (unlocked) highestUnlocked = Mathf.Max(highestUnlocked, n.Pos.y);
            }
            foreach (var s in _segments) ApplySegment(s);

            // Fog hides everything more than a row above the highest unlocked level.
            float fogY = highestUnlocked + RowSpacing * 1.4f;
            _fog.anchoredPosition = new Vector2(0, fogY);
            _fog.sizeDelta = new Vector2(0, Mathf.Max(600, _contentHeight - fogY + 200));
            _fog.SetAsLastSibling();
            _marker.SetAsLastSibling();
            _fogText.text = fogY >= _contentHeight - TopPad * 0.5f
                ? "More of the spire awaits in future updates..."
                : "The path climbs on into the dark...";

            _continueLevel = p.NextLevel(null) ?? p.CurrentLevel;
            if (_continueLevel != null)
            {
                _continue.gameObject.SetActive(true);
                bool done = p.IsCompleted(_continueLevel);
                _continue.Label.text = done ? "REPLAY " + _continueLevel.DisplayName : "PLAY " + _continueLevel.DisplayName;
            }
        }

        void ApplyNode(Node n, bool unlocked)
        {
            var l = n.Level;
            var p = Game.Progress;
            bool completed = p.IsCompleted(l);
            int stars = p.StarsFor(l);
            bool secretHidden = l.Kind == LevelKind.Secret && !unlocked;
            Color accent = HudScreen.KindColor(l.Kind);

            n.Lock.enabled = !unlocked && !secretHidden;
            n.Number.enabled = unlocked || secretHidden;
            n.Number.text = secretHidden ? "?" : (l.Kind == LevelKind.Bonus ? "B" : l.Kind == LevelKind.Secret ? "S" : l.Number.ToString());
            n.Title.text = unlocked ? l.Title : (secretHidden ? "" : "???");
            n.Title.color = unlocked ? Theme.Text.WithAlpha(0.75f) : Theme.TextDim.WithAlpha(0.4f);

            if (!unlocked)
            {
                n.Body.color = new Color(0.07f, 0.08f, 0.2f, secretHidden ? 0.5f : 0.95f);
                n.Ring.color = (secretHidden ? Theme.Violet : Theme.TextDim).WithAlpha(secretHidden ? 0.3f : 0.35f);
                n.Number.color = Theme.Violet.WithAlpha(0.5f);
                n.Glow.color = Color.clear;
            }
            else if (completed)
            {
                n.Body.color = new Color(0.14f, 0.15f, 0.36f, 1f);
                n.Ring.color = l.Kind == LevelKind.Normal ? Theme.Gold : accent;
                n.Number.color = Theme.Gold;
                n.Glow.color = Theme.Gold.WithAlpha(0.18f);
            }
            else
            {
                n.Body.color = new Color(0.12f, 0.2f, 0.45f, 1f);
                n.Ring.color = l.Kind == LevelKind.Normal ? Theme.Cyan : accent;
                n.Number.color = Theme.Text;
                n.Glow.color = accent.WithAlpha(0.4f);
            }

            int[] bits = { Stars.Complete, Stars.Target, Stars.Mastery };
            for (int i = 0; i < 3; i++)
            {
                n.Stars[i].enabled = unlocked;
                n.Stars[i].color = (stars & bits[i]) != 0 ? Theme.Gold : Theme.StarOff;
            }
        }

        void ApplySegment(Segment s)
        {
            var p = Game.Progress;
            var to = Game.Levels.Get(s.To);
            bool toUnlocked = to != null && p.IsUnlocked(to) && !_animating.Contains(to.Id);
            bool fromDone = string.IsNullOrEmpty(s.From) || p.IsCompleted(Game.Levels.Get(s.From));
            bool toDone = to != null && p.IsCompleted(to);
            if (s.VisualOnly)
            {
                var from = Game.Levels.Get(s.From);
                bool fromUnlocked = from != null && p.IsUnlocked(from) && !_animating.Contains(from.Id);
                toUnlocked = fromUnlocked && toUnlocked;
            }
            Color c;
            if (!toUnlocked) c = Theme.TextDim.WithAlpha(0.14f);
            else if (toDone && fromDone) c = Theme.Gold.WithAlpha(0.85f);
            else c = (s.VisualOnly ? Theme.Violet : Theme.Cyan).WithAlpha(0.9f);
            foreach (var d in s.Dots) d.color = c;
        }

        void PlaceMarker(LevelData l)
        {
            if (l == null || !_nodes.TryGetValue(l.Id, out var n) || n == null) return;
            _marker.anchoredPosition = n.Pos + new Vector2(0, NodeSize * 0.75f);
        }

        void PlayUnlocks(List<string> ids)
        {
            float delay = 0.45f;
            LevelData moveTo = null;
            foreach (var id in ids)
            {
                var l = Game.Levels.Get(id);
                if (l == null || !_nodes.TryGetValue(id, out var n) || n == null) continue;
                if (moveTo == null && l.IsMainPath) moveTo = l;
                Tween.Delay(n.Root, delay, () =>
                {
                    _animating.Remove(id);
                    Game.Audio.Play(Sfx.Unlock, 0.8f);
                    Refresh();
                    UIFactory.PopIn(n.Root, 0f, 0.5f, 0.4f);
                    Burst(n.Pos, HudScreen.KindColor(l.Kind));
                    if (l.Kind == LevelKind.Bonus || l.Kind == LevelKind.Secret)
                        UI.Toast(l.Kind == LevelKind.Bonus ? "A hidden branch appears!" : "A secret stair is revealed!", 2.4f, 300);
                });
                delay += 0.45f;
            }
            if (moveTo != null)
            {
                var target = moveTo;
                var startPos = _marker.anchoredPosition;
                var endPos = _nodes[target.Id].Pos + new Vector2(0, NodeSize * 0.75f);
                Tween.Run(_marker, 0.7f, t =>
                {
                    _marker.anchoredPosition = Vector2.Lerp(startPos, endPos, t) + new Vector2(0, Mathf.Sin(t * Mathf.PI) * 60f);
                }, Ease.InOutSine, () => Game.Progress.SetCurrent(target), delay + 0.1f);
                ScrollTo(target, true, delay);
            }
        }

        void Burst(Vector2 at, Color c)
        {
            var ring = UIFactory.Image(_content, "Burst", SpriteFactory.RingThin, c);
            ring.rectTransform.anchorMin = ring.rectTransform.anchorMax = new Vector2(0.5f, 0);
            ring.rectTransform.anchoredPosition = at;
            Tween.Run(ring, 0.6f, t =>
            {
                ring.rectTransform.sizeDelta = Vector2.one * Mathf.Lerp(80f, 420f, t);
                ring.color = c.WithAlpha(1f - t);
            }, Ease.OutCubic, () => Destroy(ring.gameObject));
        }

        void ScrollTo(LevelData l, bool animate, float delay = 0f)
        {
            if (l == null || !_nodes.TryGetValue(l.Id, out var n) || n == null) return;
            float viewH = _viewport.rect.height;
            float target = -(n.Pos.y - viewH * 0.42f);
            target = Mathf.Clamp(target, -(_contentHeight - viewH), 0f);
            _scroll.StopMovement();
            if (!animate)
            {
                _content.anchoredPosition = new Vector2(0, target);
                return;
            }
            float from = _content.anchoredPosition.y;
            Tween.Run(_content, 0.8f, t =>
            {
                _content.anchoredPosition = new Vector2(0, Mathf.Lerp(from, target, t));
            }, Ease.InOutSine, null, delay);
        }

        void OnNodeTapped(Node n)
        {
            var l = n.Level;
            if (Game.Progress.IsUnlocked(l) && !_animating.Contains(l.Id))
            {
                UI.ShowLevelInfo(l);
                return;
            }
            UI.Toast(Game.Progress.LockReason(l));
            Game.Audio.Play(Sfx.Bump, 0.5f);
            var root = n.Root;
            var basePos = n.Pos;
            Tween.Run(root, 0.3f, t => root.anchoredPosition = basePos + new Vector2(Mathf.Sin(t * Mathf.PI * 6f) * 12f * (1f - t), 0),
                Ease.Linear, () => root.anchoredPosition = basePos);
        }

        void Update()
        {
            if (_marker == null) return;
            float t = Time.unscaledTime;
            _markerBody.rectTransform.anchoredPosition = new Vector2(0, Mathf.Sin(t * 3f) * 8f);
            foreach (var n in _nodes.Values)
            {
                if (n == null || !Game.Progress.IsUnlocked(n.Level) || Game.Progress.IsCompleted(n.Level) || _animating.Contains(n.Level.Id)) continue;
                n.Glow.color = HudScreen.KindColor(n.Level.Kind).WithAlpha(0.3f + 0.2f * Mathf.Sin(t * 2.5f));
            }
        }
    }
}
