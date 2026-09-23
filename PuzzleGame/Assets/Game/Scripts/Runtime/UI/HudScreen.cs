using System.Collections.Generic;
using PuzzleGame.Core;
using UnityEngine;
using UnityEngine.UI;

namespace PuzzleGame
{
    /// <summary>
    /// Gameplay HUD. Top: pause, level name, move counter, live star objectives and
    /// (on echo levels) the echo memory strip. Bottom: undo, D-pad, reset.
    /// The board is fitted into the space between <see cref="TopBlock"/> and <see cref="BottomBlock"/>.
    /// </summary>
    public sealed class HudScreen : UIScreen
    {
        public RectTransform TopBlock { get; private set; }
        public RectTransform BottomBlock { get; private set; }

        Text _levelLabel, _title, _moves, _hint;
        RectTransform _hintRect;
        CanvasGroup _hintGroup;
        readonly Image[] _objStar = new Image[3];
        readonly Text[] _objText = new Text[3];
        RectTransform _echoRoot;
        readonly List<Image> _echoSlots = new List<Image>();
        readonly List<Image> _echoArrows = new List<Image>();
        UIButton _undo, _reset, _pause;
        Text _undoLabel, _resetLabel, _echoSub;
        Image _echoRune;
        bool _echoArmed;
        readonly List<UIButton> _pad = new List<UIButton>();
        LevelData _level;
        int _lastMoves = -1;
        bool _undoHeld;
        float _undoTimer;
        bool _hintDismissed;

        const float TopHeight = 330f;
        const float TopHeightEcho = 440f;

        protected override void Build()
        {
            // ---------------------------------------------------------------- top
            TopBlock = UIFactory.Rect("Top", RT).Band(1f, 1f, 0, TopHeight);

            _pause = UIFactory.Button(TopBlock, "Pause", "", Theme.Button, () => Game.Pause(), SpriteFactory.Pause);
            ((RectTransform)_pause.transform).Place(new Vector2(0, 1), new Vector2(0, 1), new Vector2(32, -28), new Vector2(130, 130));

            _levelLabel = UIFactory.Text(TopBlock, "LevelLabel", "LEVEL 1", 34, Theme.Cyan, TextAnchor.MiddleCenter, FontStyle.Bold);
            _levelLabel.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -30), new Vector2(560, 50));
            _title = UIFactory.Text(TopBlock, "Title", "", 54, Theme.Text, TextAnchor.MiddleCenter, FontStyle.Bold);
            _title.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -80), new Vector2(620, 80));
            _title.horizontalOverflow = HorizontalWrapMode.Overflow;

            var movesBox = UIFactory.Rect("Moves", TopBlock).Place(new Vector2(1, 1), new Vector2(1, 1), new Vector2(-32, -28), new Vector2(210, 130));
            var mbg = UIFactory.Image(movesBox, "Bg", SpriteFactory.Panel, Theme.PanelLight.WithAlpha(0.8f));
            mbg.rectTransform.Fill();
            var ml = UIFactory.Text(movesBox, "Label", "MOVES", 26, Theme.TextDim, TextAnchor.MiddleCenter, FontStyle.Bold);
            ml.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -8), new Vector2(200, 36));
            _moves = UIFactory.Text(movesBox, "Value", "0", 50, Theme.Text, TextAnchor.MiddleCenter, FontStyle.Bold);
            _moves.rectTransform.Place(new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 10), new Vector2(210, 70));
            _moves.supportRichText = true;

            // live objectives
            float[] xs = { -330, 0, 330 };
            for (int i = 0; i < 3; i++)
            {
                var chip = UIFactory.Rect("Objective" + i, TopBlock).Place(new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(xs[i], -190), new Vector2(320, 70));
                var bg = UIFactory.Image(chip, "Bg", SpriteFactory.Panel, new Color(0.1f, 0.12f, 0.3f, 0.7f));
                bg.rectTransform.Fill();
                _objStar[i] = UIFactory.Image(chip, "Star", SpriteFactory.Star, Theme.StarOff);
                _objStar[i].rectTransform.Place(new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(14, 0), new Vector2(46, 46));
                _objText[i] = UIFactory.Text(chip, "Text", "", 31, Theme.Text, TextAnchor.MiddleLeft, FontStyle.Bold);
                _objText[i].rectTransform.Fill(70, 0, 8, 0);
                _objText[i].horizontalOverflow = HorizontalWrapMode.Overflow;
            }

            // echo memory strip
            _echoRoot = UIFactory.Rect("Echo", TopBlock).Place(new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -270), new Vector2(900, 100));
            _echoRune = UIFactory.Image(_echoRoot, "Rune", SpriteFactory.Rune, Theme.Violet);
            _echoRune.rectTransform.Place(new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0), new Vector2(64, 64));
            var el = UIFactory.Text(_echoRoot, "Label", "ECHO", 32, Theme.Violet, TextAnchor.LowerLeft, FontStyle.Bold);
            el.rectTransform.Place(new Vector2(0, 0.5f), new Vector2(0, 0), new Vector2(76, 0), new Vector2(210, 44));
            _echoSub = UIFactory.Text(_echoRoot, "Sub", "", 26, Theme.TextDim, TextAnchor.UpperLeft, FontStyle.Bold);
            _echoSub.rectTransform.Place(new Vector2(0, 0.5f), new Vector2(0, 1), new Vector2(76, 0), new Vector2(230, 44));
            _echoSub.horizontalOverflow = HorizontalWrapMode.Overflow;

            // hint line (just above the board)
            _hintRect = UIFactory.Rect("Hint", RT).Place(new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -TopHeight), new Vector2(980, 90));
            _hint = UIFactory.Text(_hintRect, "Text", "", 34, Theme.Text.WithAlpha(0.85f), TextAnchor.MiddleCenter, FontStyle.Italic);
            _hint.rectTransform.Fill();
            _hintGroup = UIFactory.Group(_hintRect.gameObject);
            _hintGroup.blocksRaycasts = false;

            // ---------------------------------------------------------------- bottom
            BottomBlock = UIFactory.Rect("Bottom", RT).Band(0f, 0f, 0, 560);

            var padRoot = UIFactory.Rect("Pad", BottomBlock).Place(new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 40), new Vector2(560, 500));
            var padGlow = UIFactory.Image(padRoot, "Glow", SpriteFactory.Glow, Theme.Cyan.WithAlpha(0.08f));
            padGlow.rectTransform.Fill(-80, -80, -80, -80);
            PadButton(padRoot, Direction.Up, new Vector2(0, 170));
            PadButton(padRoot, Direction.Down, new Vector2(0, -170));
            PadButton(padRoot, Direction.Left, new Vector2(-190, 0));
            PadButton(padRoot, Direction.Right, new Vector2(190, 0));

            _undo = SideButton(BottomBlock, "UNDO", SpriteFactory.Undo, new Vector2(0, 0), new Vector2(40, 60), new Vector2(0, 0), out _undoLabel);
            _undo.Down = () => { _undoHeld = true; _undoTimer = 0.35f; Game.Puzzle.Undo(); };
            _undo.Up = () => _undoHeld = false;
            _reset = SideButton(BottomBlock, "RESET", SpriteFactory.Restart, new Vector2(1, 0), new Vector2(-40, 60), new Vector2(1, 0), out _resetLabel);
            _reset.Clicked = () => Game.Puzzle.ResetPuzzle();
        }

        void PadButton(RectTransform parent, Direction d, Vector2 pos)
        {
            var b = UIFactory.Button(parent, "Pad" + d, "", Theme.Button.WithAlpha(0.92f), null, SpriteFactory.Arrow, 44, Theme.Cyan);
            var rt = (RectTransform)b.transform;
            rt.Place(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), pos, new Vector2(176, 176));
            ((RectTransform)b.Icon.transform).Fill(44, 44, 44, 44);
            b.Icon.rectTransform.localRotation = Quaternion.Euler(0, 0, Angle(d));
            b.Down = () => Game.Input.Press(d);
            b.Up = () => Game.Input.Release(d);
            _pad.Add(b);
        }

        UIButton SideButton(RectTransform parent, string label, Sprite icon, Vector2 anchor, Vector2 pos, Vector2 pivot, out Text text)
        {
            var b = UIFactory.Button(parent, label, "", Theme.PanelLight, null, icon, 40, Theme.Text);
            var rt = (RectTransform)b.transform;
            rt.Place(anchor, pivot, pos, new Vector2(200, 200));
            ((RectTransform)b.Icon.transform).Place(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 20), new Vector2(92, 92));
            text = UIFactory.Text(rt, "Label", label, 32, Theme.Text, TextAnchor.MiddleCenter, FontStyle.Bold);
            text.rectTransform.Place(new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 12), new Vector2(200, 46));
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            return b;
        }

        /// <summary>Undo and reset always show whether they will do anything.</summary>
        void RefreshButtons()
        {
            var p = Game.Puzzle;
            int n = p.UndoCount;
            _undoLabel.text = n > 0 ? $"UNDO ×{n}" : "UNDO";
            SetEnabledLook(_undo, _undoLabel, n > 0);
            SetEnabledLook(_reset, _resetLabel, p.CanReset);
        }

        static void SetEnabledLook(UIButton b, Text label, bool on)
        {
            b.Interactable = on;
            var c = on ? Theme.PanelLight : Theme.PanelLight.WithAlpha(0.45f);
            if (b.Background != null && b.Background.color != c) b.SetColor(c);
            if (b.Icon != null) b.Icon.color = on ? Theme.Text : Theme.TextDim.WithAlpha(0.45f);
            label.color = on ? Theme.Text : Theme.TextDim.WithAlpha(0.45f);
        }

        static float Angle(Direction d)
        {
            switch (d)
            {
                case Direction.Left: return 90f;
                case Direction.Down: return 180f;
                case Direction.Right: return -90f;
                default: return 0f;
            }
        }

        // ------------------------------------------------------------ show / refresh

        public void Show(LevelData level)
        {
            _level = level;
            _lastMoves = -1;
            _levelLabel.text = level.Kind == LevelKind.Normal ? level.DisplayName : level.DisplayName + "  ·  " + level.Kind.ToString().ToUpperInvariant();
            if (level.Kind == LevelKind.Bonus || level.Kind == LevelKind.Secret) _levelLabel.text = level.DisplayName;
            _levelLabel.color = KindColor(level.Kind);
            _title.text = level.Title;

            bool echo = level.HasEcho;
            TopBlock.sizeDelta = new Vector2(0, echo ? TopHeightEcho : TopHeight);
            _hintRect.anchoredPosition = new Vector2(0, -(echo ? TopHeightEcho : TopHeight));
            _echoRoot.gameObject.SetActive(echo);
            if (echo) BuildEchoSlots(level.EchoMemory);
            _echoArmed = false;

            _hint.text = level.Hint;
            _hintDismissed = false;
            _hintGroup.alpha = 0f;
            Tween.Kill(_hintGroup);
            if (!string.IsNullOrEmpty(level.Hint))
            {
                Tween.Run(_hintGroup, 0.4f, t => _hintGroup.alpha = t, Ease.OutQuad, null, 0.25f);
            }

            SetInteractable(true);
            UIFactory.Fade(gameObject, true, 0.2f);
            Refresh();
        }

        public static Color KindColor(LevelKind k)
        {
            switch (k)
            {
                case LevelKind.Hard: return Theme.Pink;
                case LevelKind.Boss: return Theme.Crimson;
                case LevelKind.Bonus: return Theme.Gold;
                case LevelKind.Secret: return Theme.Violet;
                default: return Theme.Cyan;
            }
        }

        public void SetInteractable(bool on)
        {
            var g = UIFactory.Group(gameObject);
            g.interactable = on;
            g.blocksRaycasts = on;
            _undoHeld = false;
        }

        void BuildEchoSlots(int count)
        {
            foreach (var s in _echoSlots) Destroy(s.gameObject);
            _echoSlots.Clear();
            _echoArrows.Clear();
            float size = 86f, gap = 16f;
            float total = count * size + (count - 1) * gap;
            float x0 = 105f - total * 0.5f + size * 0.5f;
            for (int i = 0; i < count; i++)
            {
                var slot = UIFactory.Image(_echoRoot, "Slot" + i, SpriteFactory.Panel, new Color(0.2f, 0.14f, 0.42f, 0.8f));
                slot.rectTransform.Place(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(x0 + i * (size + gap), 0), new Vector2(size, size));
                var arrow = UIFactory.Image(slot.rectTransform, "Arrow", SpriteFactory.Arrow, Theme.Violet);
                arrow.rectTransform.Fill(18, 18, 18, 18);
                _echoSlots.Add(slot);
                _echoArrows.Add(arrow);
            }
        }

        public void Refresh()
        {
            if (_level == null || Game.Puzzle.State == null) return;
            var s = Game.Puzzle.State;
            int target = _level.MoveTarget;
            bool within = s.Moves <= target;
            _moves.text = $"{s.Moves}<size=30><color=#8A90C8> / {target}</color></size>";
            _moves.color = within ? Theme.Text : Theme.Pink;
            if (s.Moves != _lastMoves && _lastMoves >= 0 && s.Moves > _lastMoves)
            {
                var tr = _moves.transform;
                Tween.Kill(tr);
                Tween.Run(tr, 0.15f, t => tr.localScale = Vector3.one * (1f + 0.12f * Mathf.Sin(t * Mathf.PI)), Ease.Linear);
            }
            _lastMoves = s.Moves;
            if (s.Moves > 0 && !_hintDismissed && Game.Puzzle.Level == _level)
            {
                // The hint stays a few seconds after the first move, then gets out of the way.
                _hintDismissed = true;
                Tween.Kill(_hintGroup);
                float from = Mathf.Max(_hintGroup.alpha, 0.01f);
                Tween.Run(_hintGroup, 0.3f, t => _hintGroup.alpha = Mathf.Lerp(from, 1f, t), Ease.OutQuad);
                Tween.Run(_hintGroup, 1.2f, t => _hintGroup.alpha = 1f - t, Ease.InQuad, null, 3f);
            }

            // Objectives: lit while still achievable in this run.
            SetObjective(0, "Finish", false, true);
            SetObjective(1, $"Max {target} moves", within, !within);
            bool mastery;
            string mtext;
            switch (_level.Mastery)
            {
                case MasteryType.Crystal:
                    mastery = Game.Puzzle.PickupTaken(_level.CrystalIndex);
                    mtext = mastery ? "Crystal!" : "Crystal";
                    SetObjective(2, mtext, mastery, false);
                    break;
                case MasteryType.Par:
                    mastery = s.Moves <= _level.MasteryValue;
                    SetObjective(2, $"Perfect {_level.MasteryValue}", mastery, !mastery);
                    break;
                case MasteryType.NoUndo:
                    mastery = !Game.Puzzle.UsedUndo;
                    SetObjective(2, "No undo", mastery, !mastery);
                    break;
                default:
                    SetObjective(2, "Complete", false, false);
                    break;
            }

            if (_level.HasEcho) RefreshEcho(s);
        }

        void SetObjective(int i, string text, bool lit, bool failed)
        {
            _objText[i].text = text;
            _objText[i].color = failed ? Theme.TextDim.WithAlpha(0.5f) : Theme.Text;
            _objStar[i].color = lit ? Theme.Gold : (failed ? new Color(1, 1, 1, 0.08f) : Theme.StarOff);
        }

        void RefreshEcho(PuzzleState s)
        {
            int n = _echoSlots.Count;
            for (int i = 0; i < n; i++)
            {
                // Right-align: the newest move is always the rightmost slot.
                int mi = s.Memory.Count - n + i;
                bool has = mi >= 0;
                _echoArrows[i].enabled = has;
                if (has) _echoArrows[i].rectTransform.localRotation = Quaternion.Euler(0, 0, Angle(s.Memory[mi]));
                // Armed (one step from the rune): the strip lights up, because this is
                // exactly the sequence the next step will play.
                _echoSlots[i].color = _echoArmed && has
                    ? new Color(0.42f, 0.28f, 0.85f, 1f)
                    : new Color(0.2f, 0.14f, 0.42f, has ? 0.95f : 0.45f);
                _echoArrows[i].color = _echoArmed && has ? Color.white : Theme.Violet;
            }
            int len = _level != null ? _level.EchoMemory : n;
            _echoSub.text = _echoArmed ? "plays on the rune" : $"repeats last {len}";
            _echoSub.color = _echoArmed ? Theme.Text : Theme.TextDim;
            if (!_echoArmed) _echoRune.color = Theme.Violet;
        }

        void OnEchoArmed(bool armed)
        {
            _echoArmed = armed;
            if (_level != null && _level.HasEcho && Game.Puzzle.State != null) RefreshEcho(Game.Puzzle.State);
        }

        /// <summary>
        /// While an echo replays, the strip shows the sequence being replayed and
        /// highlights the step in progress. (The logical memory has already moved on.)
        /// </summary>
        void OnEchoStep(Direction[] sequence, int index)
        {
            if (_level == null || !_level.HasEcho || Game.Puzzle.State == null) return;
            if (index < 0 || sequence == null)
            {
                RefreshEcho(Game.Puzzle.State);
                return;
            }
            int n = _echoSlots.Count;
            for (int i = 0; i < n; i++)
            {
                int mi = sequence.Length - n + i;
                bool has = mi >= 0;
                _echoArrows[i].enabled = has;
                if (has) _echoArrows[i].rectTransform.localRotation = Quaternion.Euler(0, 0, Angle(sequence[mi]));
                bool playing = has && mi == index;
                _echoSlots[i].color = playing ? Theme.Violet : new Color(0.2f, 0.14f, 0.42f, has ? 0.95f : 0.45f);
                _echoArrows[i].color = playing ? Color.white : Theme.Violet.WithAlpha(mi < index ? 0.45f : 1f);
                if (playing)
                {
                    var tr = _echoSlots[i].transform;
                    Tween.Run(tr, 0.12f, t => tr.localScale = Vector3.one * (1f + 0.15f * Mathf.Sin(t * Mathf.PI)), Ease.Linear);
                }
            }
        }

        void OnEnable()
        {
            // AddComponent triggers OnEnable before Init(); nothing to hook up yet.
            if (UI == null || Game == null) return;
            Game.Puzzle.StateChanged += Refresh;
            Game.Grid.EchoStep += OnEchoStep;
            Game.Grid.EchoArmedChanged += OnEchoArmed;
        }

        void OnDisable()
        {
            if (UI == null || Game == null) return;
            Game.Puzzle.StateChanged -= Refresh;
            Game.Grid.EchoStep -= OnEchoStep;
            Game.Grid.EchoArmedChanged -= OnEchoArmed;
        }

        void Update()
        {
            if (_undoHeld)
            {
                _undoTimer -= Time.unscaledDeltaTime;
                if (_undoTimer <= 0f)
                {
                    _undoTimer = 0.12f;
                    Game.Puzzle.Undo();
                }
            }
            if (_undoLabel != null && Game.Puzzle.State != null) RefreshButtons();
            if (_echoArmed && _echoRune != null)
                _echoRune.color = Color.Lerp(Theme.Violet, Color.white, 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 6f));
        }
    }
}
