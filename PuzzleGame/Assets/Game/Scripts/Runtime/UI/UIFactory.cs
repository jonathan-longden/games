using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PuzzleGame
{
    /// <summary>Helpers for building the uGUI hierarchy in code (no prefabs needed).</summary>
    public static class UIFactory
    {
        static Font _font;

        public static Font Font
        {
            get
            {
                if (_font != null) return _font;
                // Unity 2022.3 ships LegacyRuntime.ttf; older versions used Arial.ttf.
                try { _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); } catch { _font = null; }
                if (_font == null)
                {
                    try { _font = Resources.GetBuiltinResource<Font>("Arial.ttf"); } catch { _font = null; }
                }
                return _font;
            }
        }

        public static RectTransform Rect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.layer = 5; // UI
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            return rt;
        }

        /// <summary>Stretch to fill the parent, with optional insets (left, bottom, right, top).</summary>
        public static RectTransform Fill(this RectTransform rt, float l = 0, float b = 0, float r = 0, float t = 0)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(l, b);
            rt.offsetMax = new Vector2(-r, -t);
            return rt;
        }

        /// <summary>Anchor at a point of the parent (0..1), with pivot, position offset and size.</summary>
        public static RectTransform Place(this RectTransform rt, Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 size)
        {
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = pivot;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            return rt;
        }

        /// <summary>Horizontal band: stretches across the parent, anchored at a vertical position.</summary>
        public static RectTransform Band(this RectTransform rt, float anchorY, float pivotY, float y, float height, float sideInset = 0)
        {
            rt.anchorMin = new Vector2(0, anchorY);
            rt.anchorMax = new Vector2(1, anchorY);
            rt.pivot = new Vector2(0.5f, pivotY);
            rt.anchoredPosition = new Vector2(0, y);
            rt.sizeDelta = new Vector2(-sideInset * 2f, height);
            return rt;
        }

        public static Image Image(Transform parent, string name, Sprite sprite, Color color, bool raycast = false)
        {
            var rt = Rect(name, parent);
            var img = rt.gameObject.AddComponent<Image>();
            img.sprite = sprite;
            img.color = color;
            img.raycastTarget = raycast;
            if (sprite != null && sprite.border != Vector4.zero) img.type = UnityEngine.UI.Image.Type.Sliced;
            return img;
        }

        public static Text Text(Transform parent, string name, string text, int size, Color color,
            TextAnchor align = TextAnchor.MiddleCenter, FontStyle style = FontStyle.Normal)
        {
            var rt = Rect(name, parent);
            var t = rt.gameObject.AddComponent<Text>();
            t.font = Font;
            t.text = text;
            t.fontSize = size;
            t.color = color;
            t.alignment = align;
            t.fontStyle = style;
            t.raycastTarget = false;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.supportRichText = true;
            return t;
        }

        /// <summary>Icon followed by a label, e.g. [star] 12 / 66. Returns the label.</summary>
        public static Text Chip(Transform parent, string name, Sprite icon, Color iconColor, string text, int fontSize,
            Vector2 anchor, Vector2 pos, float width)
        {
            float h = fontSize * 1.3f;
            var rt = Rect(name, parent).Place(anchor, new Vector2(0.5f, 0.5f), pos, new Vector2(width, h));
            var ic = Image(rt, "Icon", icon, iconColor);
            ic.preserveAspect = true;
            ic.rectTransform.Place(new Vector2(0, 0.5f), new Vector2(0, 0.5f), Vector2.zero, new Vector2(h * 0.85f, h * 0.85f));
            var t = Text(rt, "Label", text, fontSize, Theme.Text, TextAnchor.MiddleLeft, FontStyle.Bold);
            t.rectTransform.Fill(h * 0.85f + 12f, 0, 0, 0);
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            return t;
        }

        public static Text Glow(this Text t, Color color, float distance = 3f)
        {
            var sh = t.gameObject.AddComponent<Shadow>();
            sh.effectColor = color;
            sh.effectDistance = new Vector2(0, -distance);
            return t;
        }

        /// <summary>A rounded button with a label and/or icon, press animation and click sound.</summary>
        public static UIButton Button(Transform parent, string name, string label, Color bg, Action onClick,
            Sprite icon = null, int fontSize = 44, Color? textColor = null)
        {
            var rt = Rect(name, parent);
            var img = rt.gameObject.AddComponent<Image>();
            img.sprite = SpriteFactory.Panel;
            img.type = UnityEngine.UI.Image.Type.Sliced;
            img.color = bg;
            img.raycastTarget = true;

            var hi = UIFactory.Image(rt, "Highlight", SpriteFactory.Panel, new Color(1, 1, 1, 0.08f));
            hi.rectTransform.Fill(6, 0, 6, 6);
            hi.rectTransform.anchorMin = new Vector2(0, 0.5f);

            var btn = rt.gameObject.AddComponent<UIButton>();
            btn.Background = img;
            btn.BaseColor = bg;
            btn.Clicked = onClick;

            if (icon != null)
            {
                var ic = UIFactory.Image(rt, "Icon", icon, textColor ?? Theme.Text);
                ic.preserveAspect = true;
                btn.Icon = ic;
                if (string.IsNullOrEmpty(label)) ic.rectTransform.Fill(18, 18, 18, 18);
                else ic.rectTransform.Place(new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(28, 0), new Vector2(fontSize * 1.1f, fontSize * 1.1f));
            }
            if (!string.IsNullOrEmpty(label))
            {
                var t = Text(rt, "Label", label, fontSize, textColor ?? Theme.Text, TextAnchor.MiddleCenter, FontStyle.Bold);
                t.rectTransform.Fill(icon != null ? fontSize * 1.2f : 8, 4, 8, 0);
                btn.Label = t;
            }
            return btn;
        }

        public static CanvasGroup Group(GameObject go)
        {
            var g = go.GetComponent<CanvasGroup>();
            return g != null ? g : go.AddComponent<CanvasGroup>();
        }

        /// <summary>Fades a screen/panel in or out (and toggles raycasts).</summary>
        public static void Fade(GameObject go, bool show, float duration = 0.18f, Action done = null)
        {
            var g = Group(go);
            Tween.Kill(g);
            if (show)
            {
                go.SetActive(true);
                g.blocksRaycasts = true;
                g.interactable = true;
                float from = g.alpha;
                Tween.Run(g, duration, t => g.alpha = Mathf.Lerp(from, 1f, t), Ease.OutQuad, done);
            }
            else
            {
                g.blocksRaycasts = false;
                g.interactable = false;
                float from = g.alpha;
                Tween.Run(g, duration, t => g.alpha = Mathf.Lerp(from, 0f, t), Ease.OutQuad, () =>
                {
                    go.SetActive(false);
                    done?.Invoke();
                });
            }
        }

        public static void PopIn(Transform t, float delay = 0f, float duration = 0.35f, float from = 0.6f)
        {
            t.localScale = Vector3.one * from;
            Tween.Run(t, duration, v => t.localScale = Vector3.one * Mathf.LerpUnclamped(from, 1f, v), Ease.OutBack, null, delay);
        }
    }

    /// <summary>
    /// Button with immediate press feedback and optional hold callbacks
    /// (the D-pad uses Down/Up for hold-to-repeat).
    /// </summary>
    public sealed class UIButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler, IPointerClickHandler
    {
        public Image Background;
        public Image Icon;
        public Text Label;
        public Color BaseColor;
        public Action Clicked;
        public Action Down;
        public Action Up;
        public bool Interactable = true;
        public bool Animate = true;
        public static Action ClickSound;

        bool _pressed;

        public void OnPointerDown(PointerEventData e)
        {
            if (!Interactable) return;
            _pressed = true;
            SetPressedVisual(true);
            Down?.Invoke();
        }

        public void OnPointerUp(PointerEventData e)
        {
            if (!_pressed) return;
            _pressed = false;
            SetPressedVisual(false);
            Up?.Invoke();
        }

        public void OnPointerExit(PointerEventData e)
        {
            if (!_pressed) return;
            _pressed = false;
            SetPressedVisual(false);
            Up?.Invoke();
        }

        public void OnPointerClick(PointerEventData e)
        {
            if (!Interactable || Clicked == null) return;
            ClickSound?.Invoke();
            Clicked();
        }

        void SetPressedVisual(bool down)
        {
            if (!Animate) return;
            transform.localScale = down ? new Vector3(0.93f, 0.93f, 1f) : Vector3.one;
            if (Background != null)
                Background.color = down ? Color.Lerp(BaseColor, Color.white, 0.18f) : BaseColor;
        }

        public void SetColor(Color c)
        {
            BaseColor = c;
            if (Background != null) Background.color = c;
        }

        void OnDisable()
        {
            if (_pressed)
            {
                _pressed = false;
                Up?.Invoke();
            }
            transform.localScale = Vector3.one;
            if (Background != null) Background.color = BaseColor;
        }
    }

    /// <summary>Keeps its RectTransform inside Screen.safeArea (notches, home bars).</summary>
    public sealed class SafeArea : MonoBehaviour
    {
        Rect _last;
        Vector2Int _lastScreen;

        void Update()
        {
            var sa = Screen.safeArea;
            var scr = new Vector2Int(Screen.width, Screen.height);
            if (sa == _last && scr == _lastScreen) return;
            _last = sa;
            _lastScreen = scr;
            var rt = (RectTransform)transform;
            if (Screen.width <= 0 || Screen.height <= 0) return;
            rt.anchorMin = new Vector2(sa.xMin / Screen.width, sa.yMin / Screen.height);
            rt.anchorMax = new Vector2(sa.xMax / Screen.width, sa.yMax / Screen.height);
            rt.offsetMin = rt.offsetMax = Vector2.zero;
        }
    }
}
