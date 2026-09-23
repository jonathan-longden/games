using PuzzleGame.Core;
using UnityEngine;
using UnityEngine.EventSystems;

namespace PuzzleGame
{
    /// <summary>
    /// Turns raw input into puzzle moves:
    ///  * Swipes (touch or mouse drag) anywhere that is not a button. A move fires
    ///    as soon as the finger passes the threshold, not on release, and a long
    ///    drag can continue into further moves.
    ///  * Keyboard: arrows / WASD, hold to repeat. Z or U undo, R reset, Esc pause.
    ///  * On-screen D-pad buttons call <see cref="Press"/> (hold to repeat too).
    /// </summary>
    public sealed class PlayerController : MonoBehaviour
    {
        public PuzzleManager Puzzle;
        public System.Action PauseRequested;

        const float RepeatDelay = 0.24f;
        const float RepeatInterval = 0.11f;

        // keyboard hold-repeat
        Direction _heldKey;
        float _keyTimer;

        // swipe
        bool _tracking;
        int _fingerId = -1;
        Vector2 _origin;
        bool _swipedThisTouch;

        // on-screen pad hold-repeat
        Direction _heldPad;
        float _padTimer;

        float _lastSwipeTime;

        /// <summary>
        /// About 3.5 mm of finger travel: short enough to feel instant, long enough
        /// that a tap or a resting thumb never moves the player.
        /// </summary>
        static float SwipeThreshold
        {
            get
            {
                float px = Screen.dpi > 0f ? Screen.dpi * 0.14f : Mathf.Min(Screen.width, Screen.height) * 0.045f;
                return Mathf.Clamp(px, 20f, 110f);
            }
        }

        void Update()
        {
            if (Puzzle == null || !Puzzle.Active) { _tracking = false; return; }
            ReadKeyboard();
            ReadPointer();
            RepeatPad();
        }

        // ------------------------------------------------------------ keyboard

        void ReadKeyboard()
        {
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P)) PauseRequested?.Invoke();
            if (!Puzzle.AcceptsInput) { _heldKey = Direction.None; return; }
            if (Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.U) || Input.GetKeyDown(KeyCode.Backspace)) Puzzle.Undo();
            if (Input.GetKeyDown(KeyCode.R)) Puzzle.ResetPuzzle();

            var pressed = KeyDown();
            if (pressed != Direction.None)
            {
                _heldKey = pressed;
                _keyTimer = RepeatDelay;
                Puzzle.Move(pressed);
                return;
            }
            if (_heldKey != Direction.None)
            {
                if (!KeyHeld(_heldKey)) { _heldKey = Direction.None; return; }
                _keyTimer -= Time.unscaledDeltaTime;
                if (_keyTimer <= 0f)
                {
                    _keyTimer = RepeatInterval;
                    Puzzle.Move(_heldKey);
                }
            }
        }

        static Direction KeyDown()
        {
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)) return Direction.Up;
            if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S)) return Direction.Down;
            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) return Direction.Left;
            if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) return Direction.Right;
            return Direction.None;
        }

        static bool KeyHeld(Direction d)
        {
            switch (d)
            {
                case Direction.Up: return Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W);
                case Direction.Down: return Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S);
                case Direction.Left: return Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A);
                case Direction.Right: return Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D);
                default: return false;
            }
        }

        // ------------------------------------------------------------ swipe

        void ReadPointer()
        {
            if (Input.touchCount > 0)
            {
                for (int i = 0; i < Input.touchCount; i++)
                {
                    var t = Input.GetTouch(i);
                    if (t.phase == TouchPhase.Began && !_tracking)
                    {
                        if (OverUI(t.fingerId)) continue;
                        Begin(t.position, t.fingerId);
                    }
                    else if (_tracking && t.fingerId == _fingerId)
                    {
                        if (t.phase == TouchPhase.Moved || t.phase == TouchPhase.Stationary) Track(t.position);
                        else if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled) _tracking = false;
                    }
                }
                return;
            }

            // Mouse (editor / desktop): click-drag behaves like a swipe.
            if (Input.GetMouseButtonDown(0))
            {
                if (!OverUI(-1)) Begin(Input.mousePosition, -1);
            }
            else if (_tracking && Input.GetMouseButton(0))
            {
                Track(Input.mousePosition);
            }
            else if (Input.GetMouseButtonUp(0))
            {
                _tracking = false;
            }
        }

        void Begin(Vector2 pos, int finger)
        {
            _tracking = true;
            _fingerId = finger;
            _origin = pos;
            _swipedThisTouch = false;
        }

        void Track(Vector2 pos)
        {
            var delta = pos - _origin;
            // The first move of a touch fires as soon as the finger passes the threshold.
            // Continuing the same drag can chain further moves, but only after more travel
            // and a short pause, so one flick never becomes an accidental double move.
            float threshold = _swipedThisTouch ? SwipeThreshold * 1.8f : SwipeThreshold;
            if (delta.magnitude < threshold) return;
            if (_swipedThisTouch && Time.unscaledTime - _lastSwipeTime < 0.12f) return;
            float ax = Mathf.Abs(delta.x), ay = Mathf.Abs(delta.y);
            // Diagonal-ish drags wait for a clearer direction (unless they are long).
            if (Mathf.Max(ax, ay) < Mathf.Min(ax, ay) * 1.3f && delta.magnitude < threshold * 2f) return;
            Direction d;
            if (ax > ay) d = delta.x > 0 ? Direction.Right : Direction.Left;
            else d = delta.y > 0 ? Direction.Up : Direction.Down;
            Puzzle.Move(d);
            _swipedThisTouch = true;
            _lastSwipeTime = Time.unscaledTime;
            _origin = pos;
        }

        static bool OverUI(int pointerId)
        {
            var es = EventSystem.current;
            if (es == null) return false;
            return pointerId >= 0 ? es.IsPointerOverGameObject(pointerId) : es.IsPointerOverGameObject();
        }

        // ------------------------------------------------------------ on-screen pad

        public void Press(Direction d)
        {
            _heldPad = d;
            _padTimer = RepeatDelay;
            Puzzle?.Move(d);
        }

        public void Release(Direction d)
        {
            if (_heldPad == d) _heldPad = Direction.None;
        }

        void RepeatPad()
        {
            if (_heldPad == Direction.None) return;
            _padTimer -= Time.unscaledDeltaTime;
            if (_padTimer > 0f) return;
            _padTimer = RepeatInterval;
            Puzzle.Move(_heldPad);
        }
    }
}
