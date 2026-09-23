using PuzzleGame.Core;
using UnityEngine;

namespace PuzzleGame
{
    /// <summary>Sorting orders for board layers (all in the default sorting layer).</summary>
    public static class Layers
    {
        public const int Shadow = -2;
        public const int Floor = 0;
        public const int FloorTop = 1;
        public const int Rune = 3;
        public const int Switch = 4;
        public const int Goal = 5;
        public const int Door = 7;
        public const int Pickup = 9;
        public const int Ghost = 11;
        public const int PieceShadow = 12;
        public const int Block = 14;
        public const int Echo = 17;
        public const int Player = 22;
    }

    /// <summary>Base for everything drawn on a grid cell. Visual only: rules live in PuzzleEngine.</summary>
    public abstract class BoardPiece : MonoBehaviour
    {
        public GridPos Cell;
        protected readonly object MoveOwner = new object();
        Transform _visual;

        /// <summary>Child that holds the sprites; animated separately from the logical position.</summary>
        protected Transform Visual
        {
            get
            {
                if (_visual == null)
                {
                    _visual = new GameObject("Visual").transform;
                    _visual.SetParent(transform, false);
                }
                return _visual;
            }
        }

        protected SpriteRenderer Layer(string name, Sprite sprite, Color color, int order, float scale = 1f,
            Vector2 offset = default, Transform parent = null)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent != null ? parent : Visual, false);
            go.transform.localPosition = offset;
            go.transform.localScale = Vector3.one * scale;
            var r = go.AddComponent<SpriteRenderer>();
            r.sprite = sprite;
            r.color = color;
            r.sortingOrder = order;
            return r;
        }

        public void Place(GridPos cell, Vector3 world)
        {
            Tween.Kill(MoveOwner, true);
            Cell = cell;
            transform.localPosition = world;
        }

        /// <summary>Slides to a new cell. The logical cell updates immediately.</summary>
        public virtual void SlideTo(GridPos cell, Vector3 world, float duration, Ease ease = Ease.OutCubic)
        {
            Tween.Kill(MoveOwner, true);
            Cell = cell;
            var from = transform.localPosition;
            if (duration <= 0f)
            {
                transform.localPosition = world;
                return;
            }
            Tween.Run(MoveOwner, duration, t => { if (this) transform.localPosition = Vector3.LerpUnclamped(from, world, t); }, ease);
        }

        public void FinishMotion() => Tween.Kill(MoveOwner, true);

        readonly object _jiggleOwner = new object();

        /// <summary>A short shove-and-return: "I was pushed but cannot move".</summary>
        public void Jiggle(Direction d)
        {
            var v = Visual;
            Vector3 dir;
            switch (d)
            {
                case Direction.Up: dir = Vector3.up; break;
                case Direction.Down: dir = Vector3.down; break;
                case Direction.Left: dir = Vector3.left; break;
                default: dir = Vector3.right; break;
            }
            Tween.Kill(_jiggleOwner, true);
            Tween.Run(_jiggleOwner, 0.14f, t => { if (this) v.localPosition = dir * (0.07f * Mathf.Sin(t * Mathf.PI)); },
                Ease.Linear, () => { if (this) v.localPosition = Vector3.zero; });
        }

        protected void Punch(float amount = 0.15f, float duration = 0.25f)
        {
            var v = Visual;
            Tween.Run(this, duration, t =>
            {
                if (!this) return;
                float s = 1f + amount * Mathf.Sin(t * Mathf.PI) * (1f - t);
                v.localScale = new Vector3(s, s, 1f);
            }, Ease.Linear, () => { if (this) v.localScale = Vector3.one; });
        }
    }

    public sealed class PlayerAvatar : BoardPiece
    {
        SpriteRenderer _body, _glow, _eyeL, _eyeR, _shadow;
        Transform _bodyRoot;
        Direction _facing = Direction.Down;
        float _squash;
        bool _hidden;

        public void Build()
        {
            _shadow = Layer("Shadow", SpriteFactory.Circle64, new Color(0, 0, 0, 0.35f), Layers.PieceShadow, 1f, new Vector2(0, -0.28f));
            _shadow.transform.localScale = new Vector3(0.5f, 0.16f, 1f);
            _bodyRoot = new GameObject("Body").transform;
            _bodyRoot.SetParent(Visual, false);
            _glow = Layer("Glow", SpriteFactory.Glow, Theme.Cyan.WithAlpha(0.45f), Layers.Player - 1, 1.5f, default, _bodyRoot);
            _body = Layer("Core", SpriteFactory.Circle64, Theme.Player, Layers.Player, 0.62f, default, _bodyRoot);
            var eyeCol = new Color(0.1f, 0.12f, 0.3f, 1f);
            _eyeL = Layer("EyeL", SpriteFactory.Circle64, eyeCol, Layers.Player + 1, 0.11f, default, _bodyRoot);
            _eyeR = Layer("EyeR", SpriteFactory.Circle64, eyeCol, Layers.Player + 1, 0.11f, default, _bodyRoot);
            UpdateEyes();
        }

        public void Face(Direction d)
        {
            if (d == Direction.None) return;
            _facing = d;
            UpdateEyes();
        }

        void UpdateEyes()
        {
            Vector2 look;
            switch (_facing)
            {
                case Direction.Up: look = new Vector2(0, 0.1f); break;
                case Direction.Left: look = new Vector2(-0.1f, 0); break;
                case Direction.Right: look = new Vector2(0.1f, 0); break;
                default: look = new Vector2(0, -0.06f); break;
            }
            float spread = (_facing == Direction.Left || _facing == Direction.Right) ? 0.09f : 0.12f;
            _eyeL.transform.localPosition = new Vector3(-spread + look.x, 0.04f + look.y, 0);
            _eyeR.transform.localPosition = new Vector3(spread + look.x, 0.04f + look.y, 0);
            // Looking up: eyes smaller (seen from behind-ish)
            float s = _facing == Direction.Up ? 0.08f : 0.11f;
            _eyeL.transform.localScale = _eyeR.transform.localScale = Vector3.one * s;
        }

        public override void SlideTo(GridPos cell, Vector3 world, float duration, Ease ease = Ease.OutCubic)
        {
            base.SlideTo(cell, world, duration, ease);
            _squash = 1f;
        }

        public void Bump(Direction d)
        {
            Face(d);
            var dir = Dir(d) * 0.12f;
            Tween.Kill(this, true);
            Tween.Run(this, 0.16f, t =>
            {
                if (!this) return;
                _bodyRoot.localPosition = dir * Mathf.Sin(t * Mathf.PI);
            }, Ease.Linear, () => { if (this) _bodyRoot.localPosition = Vector3.zero; });
        }

        public void EnterPortal(System.Action done)
        {
            Tween.Kill(this, true);
            Tween.Run(this, 0.45f, t =>
            {
                if (!this) return;
                float s = 1f - t;
                _bodyRoot.localScale = new Vector3(s, s, 1f);
                _bodyRoot.localRotation = Quaternion.Euler(0, 0, t * 360f);
                _shadow.color = new Color(0, 0, 0, 0.35f * s);
            }, Ease.InQuad, done);
        }

        public void ResetVisual()
        {
            Tween.Kill(this);
            _bodyRoot.localScale = Vector3.one;
            _bodyRoot.localRotation = Quaternion.identity;
            _bodyRoot.localPosition = Vector3.zero;
            _shadow.color = new Color(0, 0, 0, 0.35f);
        }

        public void SetHidden(bool hidden)
        {
            _hidden = hidden;
            Visual.gameObject.SetActive(!hidden);
        }

        static Vector3 Dir(Direction d)
        {
            switch (d)
            {
                case Direction.Up: return Vector3.up;
                case Direction.Down: return Vector3.down;
                case Direction.Left: return Vector3.left;
                case Direction.Right: return Vector3.right;
                default: return Vector3.zero;
            }
        }

        void Update()
        {
            if (_hidden || _body == null) return;
            float time = Time.unscaledTime;
            _squash = Mathf.MoveTowards(_squash, 0f, Time.unscaledDeltaTime * 7f);
            float bob = Mathf.Sin(time * 3.2f) * 0.025f;
            float sq = Mathf.Sin(_squash * Mathf.PI) * 0.14f;
            _body.transform.localScale = new Vector3(0.62f * (1f + sq), 0.62f * (1f - sq), 1f);
            _body.transform.localPosition = new Vector3(0, bob, 0);
            _glow.color = Theme.Cyan.WithAlpha(0.35f + 0.12f * Mathf.Sin(time * 2.1f));
        }
    }

    /// <summary>A pushable stone block.</summary>
    public sealed class Block : BoardPiece
    {
        SpriteRenderer _glyph, _charge;

        public void Build()
        {
            Layer("Shadow", SpriteFactory.Tile, new Color(0, 0, 0, 0.4f), Layers.PieceShadow, 0.86f, new Vector2(0.03f, -0.1f));
            Layer("Body", SpriteFactory.Tile, Theme.StoneDark, Layers.Block, 0.86f);
            Layer("Top", SpriteFactory.Tile, Theme.Stone, Layers.Block + 1, 0.74f, new Vector2(0, 0.05f));
            _charge = Layer("Charge", SpriteFactory.Glow, Color.clear, Layers.Block - 1, 1.5f);
            _glyph = Layer("Glyph", SpriteFactory.Rune, Theme.StoneDark.WithAlpha(0.7f), Layers.Block + 2, 0.36f, new Vector2(0, 0.05f));
        }

        /// <summary>Glows in the colour of the switch it is resting on.</summary>
        public void SetCharged(Color? channel)
        {
            if (channel.HasValue)
            {
                _glyph.color = channel.Value;
                _charge.color = channel.Value.WithAlpha(0.55f);
            }
            else
            {
                _glyph.color = Theme.StoneDark.WithAlpha(0.7f);
                _charge.color = Color.clear;
            }
        }
    }

    /// <summary>The violet block that replays the player's remembered moves.</summary>
    public sealed class EchoBlock : BoardPiece
    {
        SpriteRenderer _glow, _ring, _charge;

        public void Build()
        {
            Layer("Shadow", SpriteFactory.Tile, new Color(0, 0, 0, 0.35f), Layers.PieceShadow, 0.86f, new Vector2(0.03f, -0.1f));
            _glow = Layer("Glow", SpriteFactory.Glow, Theme.Violet.WithAlpha(0.5f), Layers.Echo - 1, 1.7f);
            Layer("Body", SpriteFactory.Tile, new Color(0.36f, 0.24f, 0.72f, 0.95f), Layers.Echo, 0.86f);
            Layer("Top", SpriteFactory.Tile, Theme.EchoBody, Layers.Echo + 1, 0.74f, new Vector2(0, 0.05f));
            _ring = Layer("Ring", SpriteFactory.RingThin, new Color(1, 1, 1, 0.75f), Layers.Echo + 2, 0.46f, new Vector2(0, 0.05f));
            _charge = Layer("Charge", SpriteFactory.Circle64, Color.clear, Layers.Echo + 2, 0.16f, new Vector2(0, 0.05f));
        }

        public void SetCharged(Color? channel)
        {
            _charge.color = channel.HasValue ? channel.Value : Color.clear;
        }

        public void Flash() => Punch(0.2f, 0.2f);

        void Update()
        {
            if (_glow == null) return;
            float t = Time.unscaledTime;
            _glow.color = Theme.Violet.WithAlpha(0.22f + 0.1f * Mathf.Sin(t * 2.4f));
            _ring.transform.localRotation = Quaternion.Euler(0, 0, t * 40f);
        }
    }

    /// <summary>A pressure plate. Pressed while a block or echo block rests on it.</summary>
    public sealed class Switch : BoardPiece
    {
        SpriteRenderer _ring, _core, _glow;
        Color _color;
        public bool Pressed { get; private set; }

        public void Build(int channel)
        {
            _color = Theme.Channel(channel);
            _glow = Layer("Glow", SpriteFactory.Glow, Color.clear, Layers.Switch - 1, 1.6f);
            Layer("Plate", SpriteFactory.Circle64, new Color(0.08f, 0.09f, 0.24f, 0.9f), Layers.Switch, 0.78f);
            _ring = Layer("Ring", SpriteFactory.RingThick, _color.WithAlpha(0.55f), Layers.Switch + 1, 0.74f);
            _core = Layer("Core", SpriteFactory.Circle64, _color.WithAlpha(0.3f), Layers.Switch + 1, 0.26f);
            SetPressed(false, false);
        }

        public void SetPressed(bool pressed, bool animate)
        {
            Pressed = pressed;
            _ring.color = _color.WithAlpha(pressed ? 1f : 0.55f);
            _core.color = _color.WithAlpha(pressed ? 1f : 0.3f);
            _glow.color = pressed ? _color.WithAlpha(0.6f) : Color.clear;
            if (animate) Punch(0.25f, 0.3f);
        }

        void Update()
        {
            if (_glow == null || Pressed) return;
            // Unpressed plates breathe gently so they read as "interactive".
            float a = 0.45f + 0.15f * Mathf.Sin(Time.unscaledTime * 2.5f + Cell.X + Cell.Y);
            _ring.color = _color.WithAlpha(a);
        }
    }

    /// <summary>A door; open while every switch of its channel is pressed.</summary>
    public sealed class Door : BoardPiece
    {
        SpriteRenderer _base, _bars, _glow, _frame;
        Color _color;
        float _openAmount = -1f;
        public bool Open { get; private set; }

        public void Build(int channel)
        {
            _color = Theme.Channel(channel);
            _glow = Layer("Glow", SpriteFactory.Glow, Color.clear, Layers.Door - 1, 1.3f);
            _frame = Layer("Frame", SpriteFactory.TileOutline, _color.WithAlpha(0.35f), Layers.Door, 0.96f);
            _base = Layer("Base", SpriteFactory.Tile, Theme.WallBase, Layers.Door, 0.9f);
            _bars = Layer("Bars", SpriteFactory.Bars, _color, Layers.Door + 1, 0.72f);
            SetOpen(false, false);
        }

        public void SetOpen(bool open, bool animate)
        {
            Open = open;
            float target = open ? 1f : 0f;
            if (!animate || _openAmount < 0f)
            {
                Apply(target);
                return;
            }
            float from = _openAmount;
            Tween.Kill(this);
            Tween.Run(this, 0.22f, t => Apply(Mathf.Lerp(from, target, t)), Ease.OutCubic);
        }

        void Apply(float o)
        {
            if (!this) return;
            _openAmount = o;
            _bars.transform.localScale = new Vector3(0.72f, 0.72f * (1f - 0.85f * o), 1f);
            _bars.transform.localPosition = new Vector3(0, 0.3f * o, 0);
            _bars.color = _color.WithAlpha(1f - 0.75f * o);
            _base.color = Theme.WallBase.WithAlpha(1f - 0.85f * o);
            // Closed doors do not glow: the coloured bars already say "locked by this colour".
            _glow.color = _color.WithAlpha(0.3f * _nudge);
            _frame.color = _color.WithAlpha(0.25f + 0.35f * o);
        }

        float _nudge;
        readonly object _nudgeOwner = new object();

        /// <summary>A brief glow when one of this door's plates changes, linking plate and door.</summary>
        public void Nudge()
        {
            Tween.Kill(_nudgeOwner);
            Tween.Run(_nudgeOwner, 0.5f, t =>
            {
                if (!this) return;
                _nudge = Mathf.Sin(t * Mathf.PI);
                _glow.color = _color.WithAlpha(0.3f * _nudge);
            }, Ease.Linear);
        }
    }

    /// <summary>The exit portal.</summary>
    public sealed class Goal : BoardPiece
    {
        SpriteRenderer _glow, _ring, _ring2, _core;
        float _sparkTimer;
        public Fx Fx;

        public void Build()
        {
            _glow = Layer("Glow", SpriteFactory.Glow, Theme.Gold.WithAlpha(0.35f), Layers.Goal - 1, 1.55f);
            Layer("Plate", SpriteFactory.Circle64, new Color(0.06f, 0.05f, 0.16f, 0.95f), Layers.Goal, 0.8f);
            _ring = Layer("Ring", SpriteFactory.RingThin, Theme.Gold, Layers.Goal + 1, 0.8f);
            _ring2 = Layer("Ring2", SpriteFactory.Star, Theme.Gold.WithAlpha(0.5f), Layers.Goal + 1, 0.5f);
            _core = Layer("Core", SpriteFactory.Glow, Color.white.WithAlpha(0.9f), Layers.Goal + 2, 0.5f);
        }

        public void Celebrate() => Punch(0.35f, 0.4f);

        void Update()
        {
            if (_ring == null) return;
            float t = Time.unscaledTime;
            _ring.transform.localScale = Vector3.one * (0.78f + 0.05f * Mathf.Sin(t * 3f));
            _ring2.transform.localRotation = Quaternion.Euler(0, 0, -t * 30f);
            _glow.color = Theme.Gold.WithAlpha(0.28f + 0.08f * Mathf.Sin(t * 2f));
            _core.transform.localScale = Vector3.one * (0.45f + 0.06f * Mathf.Sin(t * 4.3f));
            _sparkTimer -= Time.unscaledDeltaTime;
            if (_sparkTimer <= 0f && Fx != null)
            {
                _sparkTimer = 0.3f;
                Fx.Sparkle(transform.position, Theme.Gold, 0.3f);
            }
        }
    }

    /// <summary>The floor glyph that makes echo blocks replay the remembered moves.</summary>
    public sealed class EchoRune : BoardPiece
    {
        SpriteRenderer _glyph, _glow;
        float _flash;

        public void Build()
        {
            _glow = Layer("Glow", SpriteFactory.Glow, Theme.Violet.WithAlpha(0.3f), Layers.Rune - 1, 1.4f);
            _glyph = Layer("Glyph", SpriteFactory.Rune, Theme.Violet, Layers.Rune, 0.7f);
        }

        public void Flash() => _flash = 1f;

        bool _armed;

        /// <summary>Armed = the player is one step away, so stepping on now will play the echo.</summary>
        public void SetArmed(bool armed) => _armed = armed;

        void Update()
        {
            if (_glyph == null) return;
            if (_armed) _flash = Mathf.Max(_flash, 0.35f + 0.25f * Mathf.Sin(Time.unscaledTime * 6f));
            float t = Time.unscaledTime;
            _flash = Mathf.MoveTowards(_flash, 0f, Time.unscaledDeltaTime * 2.5f);
            _glyph.transform.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(t * 0.8f) * 8f);
            _glyph.color = Color.Lerp(Theme.Violet.WithAlpha(0.65f + 0.2f * Mathf.Sin(t * 2f)), Color.white, _flash);
            _glow.color = Theme.Violet.WithAlpha(0.25f + 0.5f * _flash);
            _glow.transform.localScale = Vector3.one * (1.4f + _flash);
        }
    }

    /// <summary>Crystal (star 3) or special bonus item lying on the floor.</summary>
    public sealed class Pickup : BoardPiece
    {
        SpriteRenderer _icon, _glow, _ring;
        Transform _float;
        public PickupType Type;

        public void Build(PickupType type)
        {
            Type = type;
            _float = new GameObject("Float").transform;
            _float.SetParent(Visual, false);
            bool crystal = type == PickupType.Crystal;
            var item = LevelData.ToItem(type);
            Color c = crystal ? Theme.Cyan : SpriteFactory.ItemColor(item);
            _glow = Layer("Glow", SpriteFactory.Glow, c.WithAlpha(0.4f), Layers.Pickup - 1, crystal ? 1.1f : 1.5f, default, _float);
            if (!crystal)
                _ring = Layer("Ring", SpriteFactory.RingThin, c.WithAlpha(0.6f), Layers.Pickup, 0.8f, default, _float);
            _icon = Layer("Icon", crystal ? SpriteFactory.Crystal : SpriteFactory.ForItem(item), crystal ? Color.white : c,
                Layers.Pickup + 1, crystal ? 0.42f : 0.5f, default, _float);
            if (crystal)
                Layer("Tint", SpriteFactory.Crystal, Theme.Cyan.WithAlpha(0.55f), Layers.Pickup + 2, 0.3f, new Vector2(-0.02f, -0.02f), _float);
        }

        public void SetTaken(bool taken) => Visual.gameObject.SetActive(!taken);

        void Update()
        {
            if (_float == null) return;
            float t = Time.unscaledTime;
            _float.localPosition = new Vector3(0, 0.05f + Mathf.Sin(t * 2.6f + Cell.X) * 0.06f, 0);
            if (_ring != null) _ring.transform.localRotation = Quaternion.Euler(0, 0, t * 60f);
            _glow.transform.localScale = Vector3.one * ((Type == PickupType.Crystal ? 1.1f : 1.5f) + 0.1f * Mathf.Sin(t * 3f));
        }
    }
}
