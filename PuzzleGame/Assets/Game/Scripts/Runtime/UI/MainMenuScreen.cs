using PuzzleGame.Core;
using UnityEngine;
using UnityEngine.UI;

namespace PuzzleGame
{
    public sealed class MainMenuScreen : UIScreen
    {
        RectTransform _title;
        Image _halo, _star;
        Text _stats, _coins, _playLabel;
        UIButton _play, _sound;

        protected override void Build()
        {
            _halo = UIFactory.Image(RT, "Halo", SpriteFactory.Glow, Theme.Violet.WithAlpha(0.35f));
            _halo.rectTransform.Place(new Vector2(0.5f, 0.66f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1300, 1300));
            _star = UIFactory.Image(RT, "Rune", SpriteFactory.Rune, Theme.Cyan.WithAlpha(0.14f));
            _star.rectTransform.Place(new Vector2(0.5f, 0.66f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(760, 760));

            _title = UIFactory.Rect("Title", RT).Place(new Vector2(0.5f, 0.66f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1000, 520));
            var echo = UIFactory.Text(_title, "Echo", "ECHO", 190, Theme.Cyan, TextAnchor.MiddleCenter, FontStyle.Bold).Glow(Theme.Violet.WithAlpha(0.8f), 8);
            echo.rectTransform.Place(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 95), new Vector2(1000, 220));
            var ascent = UIFactory.Text(_title, "Ascent", "A S C E N T", 92, Theme.Text, TextAnchor.MiddleCenter, FontStyle.Bold).Glow(new Color(0, 0, 0, 0.5f), 5);
            ascent.rectTransform.Place(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -60), new Vector2(1000, 130));
            var tag = UIFactory.Text(_title, "Tag", "Think.  Push.  Echo.  Climb.", 40, Theme.TextDim, TextAnchor.MiddleCenter, FontStyle.Italic);
            tag.rectTransform.Place(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -175), new Vector2(1000, 60));

            _play = UIFactory.Button(RT, "Play", "PLAY", Theme.ButtonPrimary, () => Game.OpenMap(), null, 70, new Color(0.03f, 0.05f, 0.16f));
            ((RectTransform)_play.transform).Place(new Vector2(0.5f, 0.3f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(620, 190));
            _playLabel = _play.Label;

            _stats = UIFactory.Chip(RT, "Stars", SpriteFactory.Star, Theme.Gold, "", 46, new Vector2(0.5f, 0.2f), new Vector2(-150, 0), 260);
            _coins = UIFactory.Chip(RT, "Coins", SpriteFactory.Coin, Theme.Gold, "", 46, new Vector2(0.5f, 0.2f), new Vector2(170, 0), 260);

            _sound = UIFactory.Button(RT, "Sound", "", Theme.Button, () =>
            {
                Game.ToggleSound();
                RefreshSound();
            }, SpriteFactory.Speaker);
            ((RectTransform)_sound.transform).Place(new Vector2(1, 0), new Vector2(1, 0), new Vector2(-40, 40), new Vector2(130, 130));

            var ver = UIFactory.Text(RT, "Version", "foundation build " + Application.version, 28, Theme.TextDim.WithAlpha(0.6f), TextAnchor.LowerLeft);
            ver.rectTransform.Place(new Vector2(0, 0), new Vector2(0, 0), new Vector2(40, 50), new Vector2(600, 50));
        }

        public void Show()
        {
            var p = Game.Progress;
            _playLabel.text = p.CompletedCount == 0 ? "START" : "CONTINUE";
            _stats.text = $"{p.TotalStars} / {p.MaxStars}";
            _coins.text = Game.Currency.Coins.ToString();
            RefreshSound();
            UIFactory.Fade(gameObject, true, 0.3f);
            UIFactory.PopIn(_title, 0f, 0.6f, 0.85f);
            UIFactory.PopIn(_play.transform, 0.15f, 0.45f, 0.7f);
        }

        void RefreshSound()
        {
            if (_sound.Icon != null) _sound.Icon.color = Game.SoundOn ? Theme.Text : Theme.TextDim.WithAlpha(0.35f);
        }

        void Update()
        {
            float t = Time.unscaledTime;
            _star.rectTransform.localRotation = Quaternion.Euler(0, 0, t * 6f);
            _halo.color = Theme.Violet.WithAlpha(0.28f + 0.08f * Mathf.Sin(t * 0.9f));
            _title.anchoredPosition = new Vector2(0, Mathf.Sin(t * 1.1f) * 10f);
            float s = 1f + 0.025f * Mathf.Sin(t * 2.2f);
            if (_play.transform.localScale.x > 0.99f) _play.transform.localScale = new Vector3(s, s, 1);
        }
    }
}
