using PuzzleGame.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PuzzleGame
{
    /// <summary>
    /// Builds the whole interface in code (one Screen Space Overlay canvas) and
    /// switches between screens. Screens only call back into the GameManager;
    /// they hold no game state of their own.
    /// </summary>
    public sealed class UIManager : MonoBehaviour
    {
        public GameManager Game { get; private set; }
        public Canvas Canvas { get; private set; }
        public RectTransform Root { get; private set; }   // safe-area root

        CanvasScaler _scaler;
        public MainMenuScreen MainMenu { get; private set; }
        public WorldMapManager Map { get; private set; }
        public HudScreen Hud { get; private set; }
        public PauseScreen Pause { get; private set; }
        public CompleteScreen Complete { get; private set; }
        public LevelInfoPopup LevelInfo { get; private set; }

        Text _toast;
        CanvasGroup _toastGroup;
        RectTransform _toastRect;

        public void Build(GameManager game)
        {
            Game = game;

            if (FindObjectOfType<EventSystem>() == null)
            {
                var es = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
                DontDestroyOnLoad(es);
            }

            var canvasGo = new GameObject("UI", typeof(RectTransform));
            canvasGo.transform.SetParent(transform, false);
            canvasGo.layer = 5;
            Canvas = canvasGo.AddComponent<Canvas>();
            Canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            Canvas.sortingOrder = 10;
            Canvas.pixelPerfect = false;
            _scaler = canvasGo.AddComponent<CanvasScaler>();
            _scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            _scaler.referenceResolution = new Vector2(1080, 1920);
            _scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            _scaler.referencePixelsPerUnit = 100;
            canvasGo.AddComponent<GraphicRaycaster>();
            UpdateScalerMatch();

            Root = UIFactory.Rect("SafeArea", canvasGo.transform).Fill();
            Root.gameObject.AddComponent<SafeArea>();

            MainMenu = CreateScreen<MainMenuScreen>("MainMenu");
            Map = CreateScreen<WorldMapManager>("Map");
            Hud = CreateScreen<HudScreen>("Hud");
            Pause = CreateScreen<PauseScreen>("Pause");
            Complete = CreateScreen<CompleteScreen>("Complete");
            LevelInfo = CreateScreen<LevelInfoPopup>("LevelInfo");

            BuildToast();
            UIButton.ClickSound = () => Game.Audio.Play(Sfx.Click, 0.6f);
        }

        T CreateScreen<T>(string name) where T : UIScreen
        {
            var rt = UIFactory.Rect(name, Root).Fill();
            var s = rt.gameObject.AddComponent<T>();
            s.Init(this);
            rt.gameObject.SetActive(false);
            return s;
        }

        void BuildToast()
        {
            _toastRect = UIFactory.Rect("Toast", Root).Place(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -80), new Vector2(900, 120));
            var bg = UIFactory.Image(_toastRect, "Bg", SpriteFactory.Panel, new Color(0.05f, 0.06f, 0.18f, 0.94f));
            bg.rectTransform.Fill();
            var outline = UIFactory.Image(_toastRect, "Outline", SpriteFactory.PanelOutline, Theme.Violet.WithAlpha(0.6f));
            outline.rectTransform.Fill();
            _toast = UIFactory.Text(_toastRect, "Text", "", 38, Theme.Text);
            _toast.rectTransform.Fill(30, 10, 30, 10);
            _toastGroup = UIFactory.Group(_toastRect.gameObject);
            _toastGroup.alpha = 0f;
            _toastGroup.blocksRaycasts = false;
            _toastRect.gameObject.SetActive(false);
        }

        /// <summary>A short message that fades in and out on its own.</summary>
        public void Toast(string message, float seconds = 2.2f, float y = -80f)
        {
            _toast.text = message;
            _toastRect.anchoredPosition = new Vector2(0, y);
            _toastRect.SetAsLastSibling();
            _toastRect.gameObject.SetActive(true);
            Tween.Kill(_toastGroup);
            float from = _toastGroup.alpha;
            Tween.Run(_toastGroup, 0.15f, t => _toastGroup.alpha = Mathf.Lerp(from, 1f, t), Ease.OutQuad);
            Tween.Run(_toastGroup, 0.3f, t => _toastGroup.alpha = 1f - t, Ease.OutQuad,
                () => _toastRect.gameObject.SetActive(false), seconds);
        }

        public void HideAll()
        {
            MainMenu.gameObject.SetActive(false);
            Map.gameObject.SetActive(false);
            Hud.gameObject.SetActive(false);
            Pause.gameObject.SetActive(false);
            Complete.gameObject.SetActive(false);
            LevelInfo.gameObject.SetActive(false);
        }

        public void ShowMainMenu()
        {
            HideAll();
            MainMenu.Show();
        }

        public void ShowMap(LevelData focus)
        {
            HideAll();
            Map.Show(focus);
        }

        public void ShowHud(LevelData level)
        {
            HideAll();
            Hud.Show(level);
        }

        public void ShowPause(bool show)
        {
            if (show) Pause.Show();
            else Pause.Hide();
        }

        public void ShowComplete(CompletionResult result, bool hasNext)
        {
            Pause.gameObject.SetActive(false);
            Hud.SetInteractable(false);
            Complete.Show(result, hasNext);
        }

        public void ShowLevelInfo(LevelData level) => LevelInfo.Show(level);

        /// <summary>
        /// Screen-pixel rectangle between the HUD's top and bottom blocks; the
        /// board camera fits the puzzle inside it.
        /// </summary>
        public Rect BoardScreenRect()
        {
            if (Hud == null || !Hud.isActiveAndEnabled) return new Rect(0, Screen.height * 0.28f, Screen.width, Screen.height * 0.5f);
            var corners = new Vector3[4];
            Hud.TopBlock.GetWorldCorners(corners);
            float top = corners[0].y;               // bottom edge of the top block
            Hud.BottomBlock.GetWorldCorners(corners);
            float bottom = corners[1].y;            // top edge of the bottom block
            Root.GetWorldCorners(corners);
            float left = corners[0].x, right = corners[2].x;
            float pad = Screen.width * 0.02f;
            return new Rect(left + pad, bottom + pad, (right - left) - pad * 2, Mathf.Max(10, top - bottom - pad * 2));
        }

        void UpdateScalerMatch()
        {
            // Portrait phones: keep the canvas 1080 units wide. Wider screens (tablets,
            // editor game view): fit by height so nothing gets cut off.
            float aspect = Screen.width / (float)Mathf.Max(1, Screen.height);
            _scaler.matchWidthOrHeight = aspect > 1080f / 1920f + 0.01f ? 1f : 0f;
        }

        void Update()
        {
            if (_scaler != null) UpdateScalerMatch();
        }
    }

    /// <summary>Base class for code-built screens.</summary>
    public abstract class UIScreen : MonoBehaviour
    {
        protected UIManager UI;
        protected GameManager Game => UI.Game;
        protected RectTransform RT => (RectTransform)transform;

        public void Init(UIManager ui)
        {
            UI = ui;
            Build();
        }

        protected abstract void Build();

        /// <summary>Full-screen dark scrim (optionally closing the screen when tapped).</summary>
        protected Image Scrim(System.Action onTap = null)
        {
            var img = UIFactory.Image(RT, "Scrim", SpriteFactory.Square, Theme.Scrim, true);
            img.rectTransform.Fill(-400, -400, -400, -400);
            if (onTap != null)
            {
                var b = img.gameObject.AddComponent<UIButton>();
                b.Background = null;
                b.Animate = false;
                b.Clicked = onTap;
            }
            return img;
        }

        /// <summary>Rounded card with a subtle outline.</summary>
        protected RectTransform Card(Transform parent, string name, Vector2 size, Vector2 pos, Color? accent = null)
        {
            var rt = UIFactory.Rect(name, parent).Place(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), pos, size);
            var glow = UIFactory.Image(rt, "Glow", SpriteFactory.Glow, (accent ?? Theme.Violet).WithAlpha(0.18f));
            glow.rectTransform.Fill(-160, -160, -160, -160);
            var bg = UIFactory.Image(rt, "Bg", SpriteFactory.Panel, Theme.Panel, true);
            bg.rectTransform.Fill();
            var ol = UIFactory.Image(rt, "Outline", SpriteFactory.PanelOutline, (accent ?? Theme.Violet).WithAlpha(0.55f));
            ol.rectTransform.Fill();
            return rt;
        }
    }
}
