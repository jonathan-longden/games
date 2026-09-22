using System.Collections.Generic;
using PuzzleGame.Core;
using UnityEngine;
using UnityEngine.UI;

namespace PuzzleGame
{
    public sealed class PauseScreen : UIScreen
    {
        RectTransform _card;
        UIButton _sound;

        protected override void Build()
        {
            Scrim(() => Game.Resume());
            _card = Card(RT, "Card", new Vector2(760, 1060), new Vector2(0, 40));
            var title = UIFactory.Text(_card, "Title", "PAUSED", 76, Theme.Text, TextAnchor.MiddleCenter, FontStyle.Bold).Glow(Theme.Violet.WithAlpha(0.7f), 5);
            title.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -60), new Vector2(700, 100));

            float y = -220;
            Add("RESUME", Theme.ButtonPrimary, () => Game.Resume(), ref y, new Color(0.03f, 0.05f, 0.16f));
            Add("RESTART LEVEL", Theme.Button, () => Game.RestartLevel(), ref y, Theme.Text);
            Add("WORLD MAP", Theme.Button, () => Game.OpenMap(), ref y, Theme.Text);
            _sound = Add("SOUND: ON", Theme.Button, () =>
            {
                Game.ToggleSound();
                RefreshSound();
            }, ref y, Theme.Text);

            var help = UIFactory.Text(_card, "Help",
                "Swipe or use the pad to move.\nUndo is free. Reset can be undone too.\nKeyboard: arrows/WASD, Z undo, R reset.",
                28, Theme.TextDim, TextAnchor.MiddleCenter);
            help.rectTransform.Place(new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 40), new Vector2(680, 140));
        }

        UIButton Add(string label, Color c, System.Action a, ref float y, Color text)
        {
            var b = UIFactory.Button(_card, label, label, c, a, null, 44, text);
            ((RectTransform)b.transform).Place(new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, y), new Vector2(600, 130));
            y -= 158;
            return b;
        }

        void RefreshSound()
        {
            _sound.Label.text = Game.SoundOn ? "SOUND: ON" : "SOUND: OFF";
        }

        public void Show()
        {
            RefreshSound();
            transform.SetAsLastSibling();
            UIFactory.Fade(gameObject, true, 0.15f);
            UIFactory.PopIn(_card, 0f, 0.3f, 0.85f);
        }

        public void Hide() => UIFactory.Fade(gameObject, false, 0.12f);
    }

    /// <summary>
    /// "LEVEL COMPLETE": stars, moves vs target, coins, bonus item and the three
    /// buttons. Everything animates in quickly and can be skipped by tapping a button.
    /// </summary>
    public sealed class CompleteScreen : UIScreen
    {
        RectTransform _card;
        Text _title, _subtitle, _moves, _coins, _best, _item, _unlock;
        RectTransform _coinRow, _itemRow, _unlockRow;
        Image _itemIcon;
        readonly Image[] _stars = new Image[3];
        readonly Image[] _starGlow = new Image[3];
        readonly Image[] _checks = new Image[3];
        readonly Text[] _objectives = new Text[3];
        UIButton _next, _replay, _map;
        readonly List<Image> _sparks = new List<Image>();

        protected override void Build()
        {
            Scrim();
            _card = Card(RT, "Card", new Vector2(940, 1500), new Vector2(0, 20), Theme.Gold);

            _title = UIFactory.Text(_card, "Title", "LEVEL COMPLETE", 72, Theme.Gold, TextAnchor.MiddleCenter, FontStyle.Bold).Glow(new Color(0.6f, 0.3f, 0f, 0.6f), 5);
            _title.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -50), new Vector2(900, 100));
            _subtitle = UIFactory.Text(_card, "Subtitle", "", 38, Theme.TextDim, TextAnchor.MiddleCenter, FontStyle.Italic);
            _subtitle.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -140), new Vector2(900, 60));

            float[] sx = { -230, 0, 230 };
            float[] sy = { -300, -270, -300 };
            for (int i = 0; i < 3; i++)
            {
                _starGlow[i] = UIFactory.Image(_card, "StarGlow" + i, SpriteFactory.Glow, Color.clear);
                _starGlow[i].rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0.5f, 0.5f), new Vector2(sx[i], sy[i] - 20), new Vector2(360, 360));
                _stars[i] = UIFactory.Image(_card, "Star" + i, SpriteFactory.Star, Theme.StarOff);
                _stars[i].rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0.5f, 0.5f), new Vector2(sx[i], sy[i] - 20), new Vector2(i == 1 ? 220 : 180, i == 1 ? 220 : 180));
            }

            for (int i = 0; i < 3; i++)
            {
                float y = -470 - i * 66;
                _checks[i] = UIFactory.Image(_card, "Check" + i, SpriteFactory.Check, Theme.Green);
                _checks[i].rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0.5f, 0.5f), new Vector2(-330, y), new Vector2(48, 48));
                _objectives[i] = UIFactory.Text(_card, "Objective" + i, "", 36, Theme.Text, TextAnchor.MiddleLeft);
                _objectives[i].rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0, 0.5f), new Vector2(-285, y), new Vector2(640, 60));
            }

            _moves = UIFactory.Text(_card, "Moves", "", 52, Theme.Text, TextAnchor.MiddleCenter, FontStyle.Bold);
            _moves.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0.5f, 0.5f), new Vector2(0, -700), new Vector2(800, 70));
            _moves.supportRichText = true;
            _best = UIFactory.Text(_card, "Best", "NEW BEST!", 30, Theme.Cyan, TextAnchor.MiddleCenter, FontStyle.Bold);
            _best.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0.5f, 0.5f), new Vector2(0, -752), new Vector2(600, 40));

            _coinRow = UIFactory.Rect("CoinRow", _card).Place(new Vector2(0.5f, 1), new Vector2(0.5f, 0.5f), new Vector2(0, -830), new Vector2(700, 90));
            var coinIcon = UIFactory.Image(_coinRow, "Icon", SpriteFactory.Coin, Theme.Gold);
            coinIcon.rectTransform.Place(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-200, 0), new Vector2(78, 78));
            _coins = UIFactory.Text(_coinRow, "Text", "", 58, Theme.Gold, TextAnchor.MiddleLeft, FontStyle.Bold);
            _coins.rectTransform.Place(new Vector2(0.5f, 0.5f), new Vector2(0, 0.5f), new Vector2(-145, 0), new Vector2(500, 90));

            _itemRow = UIFactory.Rect("ItemRow", _card).Place(new Vector2(0.5f, 1), new Vector2(0.5f, 0.5f), new Vector2(0, -935), new Vector2(820, 100));
            var itemBg = UIFactory.Image(_itemRow, "Bg", SpriteFactory.Panel, new Color(0.25f, 0.1f, 0.35f, 0.8f));
            itemBg.rectTransform.Fill();
            _itemIcon = UIFactory.Image(_itemRow, "Icon", SpriteFactory.Gem, Theme.Pink);
            _itemIcon.rectTransform.Place(new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(24, 0), new Vector2(76, 76));
            _item = UIFactory.Text(_itemRow, "Text", "", 38, Theme.Text, TextAnchor.MiddleLeft, FontStyle.Bold);
            _item.rectTransform.Fill(120, 0, 20, 0);

            _unlockRow = UIFactory.Rect("UnlockRow", _card).Place(new Vector2(0.5f, 1), new Vector2(0.5f, 0.5f), new Vector2(0, -1030), new Vector2(820, 60));
            _unlock = UIFactory.Text(_unlockRow, "Text", "", 34, Theme.Violet, TextAnchor.MiddleCenter, FontStyle.Bold);
            _unlock.rectTransform.Fill();

            _next = UIFactory.Button(_card, "Next", "NEXT LEVEL", Theme.ButtonPrimary, () => Game.PlayNext(), null, 56, new Color(0.03f, 0.05f, 0.16f));
            ((RectTransform)_next.transform).Place(new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 200), new Vector2(760, 150));
            _replay = UIFactory.Button(_card, "Replay", "REPLAY", Theme.Button, () => Game.RestartLevel(), SpriteFactory.Restart, 44);
            ((RectTransform)_replay.transform).Place(new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(-195, 50), new Vector2(370, 120));
            _map = UIFactory.Button(_card, "Map", "MAP", Theme.Button, () => Game.OpenMap(), SpriteFactory.Home, 44);
            ((RectTransform)_map.transform).Place(new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(195, 50), new Vector2(370, 120));
        }

        public void Show(CompletionResult r, bool hasNext)
        {
            var l = r.Level;
            transform.SetAsLastSibling();
            _title.text = l.Kind == LevelKind.Boss ? "SPIRE CONQUERED" : (l.Kind == LevelKind.Secret ? "SECRET SOLVED" : "LEVEL COMPLETE");
            _subtitle.text = (l.Kind == LevelKind.Normal || l.Kind == LevelKind.Hard ? l.DisplayName + "  ·  " : l.DisplayName + "  ·  ") + l.Title;

            // Objectives with this run's results.
            bool[] ok =
            {
                true,
                (r.StarsThisRun & Stars.Target) != 0,
                (r.StarsThisRun & Stars.Mastery) != 0,
            };
            string[] txt = { "Complete the level", $"Finish in {l.MoveTarget} moves or fewer", l.MasteryDescription };
            for (int i = 0; i < 3; i++)
            {
                _objectives[i].text = txt[i];
                _objectives[i].color = ok[i] ? Theme.Text : Theme.TextDim;
                _checks[i].sprite = ok[i] ? SpriteFactory.Check : SpriteFactory.Cross;
                _checks[i].color = ok[i] ? Theme.Green : Theme.TextDim.WithAlpha(0.5f);
            }

            string movesColor = r.Moves <= r.Target ? "#EEF0FF" : "#FF6FA9";
            _moves.text = $"MOVES: <color={movesColor}>{r.Moves}</color> <color=#8A90C8>/ {r.Target}</color>";
            _best.gameObject.SetActive(r.NewBest && !r.FirstClear);

            _coins.text = r.TotalCoins > 0 ? "+0 COINS" : "Rewards already claimed";
            _coins.fontSize = r.TotalCoins > 0 ? 58 : 36;
            _coins.color = r.TotalCoins > 0 ? Theme.Gold : Theme.TextDim;

            _itemRow.gameObject.SetActive(r.ItemFound != ItemType.None);
            if (r.ItemFound != ItemType.None)
            {
                _itemIcon.sprite = SpriteFactory.ForItem(r.ItemFound);
                _itemIcon.color = SpriteFactory.ItemColor(r.ItemFound);
                _item.text = "BONUS ITEM FOUND: " + SpriteFactory.ItemName(r.ItemFound);
            }

            string unlockText = "";
            foreach (var id in r.NewlyUnlocked)
            {
                var ul = Game.Levels.Get(id);
                if (ul == null) continue;
                if (ul.Kind == LevelKind.Bonus) unlockText = "A hidden branch opened on the path!";
                else if (ul.Kind == LevelKind.Secret) unlockText = "Something sealed has opened...";
            }
            _unlock.text = unlockText;
            _unlockRow.gameObject.SetActive(unlockText.Length > 0);

            _next.gameObject.SetActive(hasNext);
            ((RectTransform)_replay.transform).anchoredPosition = new Vector2(-195, hasNext ? 50 : 120);
            ((RectTransform)_map.transform).anchoredPosition = new Vector2(195, hasNext ? 50 : 120);

            UIFactory.Fade(gameObject, true, 0.15f);
            UIFactory.PopIn(_card, 0f, 0.35f, 0.8f);
            AnimateStars(r);
            AnimateCoins(r.TotalCoins);
            if (r.ItemFound != ItemType.None)
            {
                UIFactory.PopIn(_itemRow, 1.0f, 0.4f, 0.3f);
                Tween.Delay(_itemRow, 1.0f, () => Game.Audio.Play(Sfx.Item));
            }
        }

        void AnimateStars(CompletionResult r)
        {
            int[] bits = { Stars.Complete, Stars.Target, Stars.Mastery };
            for (int i = 0; i < 3; i++)
            {
                var star = _stars[i];
                var glow = _starGlow[i];
                bool thisRun = (r.StarsThisRun & bits[i]) != 0;
                bool before = (r.PreviousStars & bits[i]) != 0;
                Tween.Kill(star.transform);
                star.transform.localScale = Vector3.one;
                glow.color = Color.clear;
                if (thisRun)
                {
                    star.color = Theme.StarOff;
                    int idx = i;
                    float delay = 0.25f + i * 0.22f;
                    Tween.Delay(star, delay, () =>
                    {
                        star.color = Theme.Gold;
                        glow.color = Theme.Gold.WithAlpha(0.45f);
                        Game.Audio.Play(Sfx.Star, 0.9f, 1f + idx * 0.12f);
                        Sparks(star.rectTransform.anchoredPosition);
                    });
                    UIFactory.PopIn(star.transform, delay, 0.4f, 0.2f);
                }
                else
                {
                    // A star earned on an earlier attempt still counts; show it faded.
                    star.color = before ? Theme.Gold.WithAlpha(0.4f) : Theme.StarOff;
                }
            }
        }

        void AnimateCoins(int total)
        {
            if (total <= 0) return;
            var t = _coins;
            int last = -1;
            Tween.Kill(t);
            Tween.Run(t, 0.7f, v =>
            {
                int n = Mathf.RoundToInt(total * v);
                t.text = $"+{n} COINS";
                if (n / 25 != last / 25) Game.Audio.Play(Sfx.Coin, 0.35f, 1f + v * 0.3f);
                last = n;
            }, Ease.OutCubic, null, 0.8f);
        }

        void Sparks(Vector2 at)
        {
            for (int i = 0; i < 10; i++)
            {
                Image s = null;
                foreach (var x in _sparks) if (!x.gameObject.activeSelf) { s = x; break; }
                if (s == null)
                {
                    s = UIFactory.Image(_card, "Spark", SpriteFactory.Star, Theme.Gold);
                    _sparks.Add(s);
                }
                s.gameObject.SetActive(true);
                var rt = s.rectTransform;
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1);
                rt.sizeDelta = Vector2.one * Random.Range(22f, 40f);
                float a = Random.Range(0f, Mathf.PI * 2f);
                var dir = new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * Random.Range(140f, 260f);
                var start = at;
                var img = s;
                Tween.Run(img, 0.55f, v =>
                {
                    rt.anchoredPosition = start + dir * v;
                    img.color = Theme.Gold.WithAlpha(1f - v);
                    rt.localRotation = Quaternion.Euler(0, 0, v * 180f);
                }, Ease.OutCubic, () => img.gameObject.SetActive(false));
            }
        }
    }

    /// <summary>Tapping a level on the map: its objectives, stars, best, hidden item, PLAY.</summary>
    public sealed class LevelInfoPopup : UIScreen
    {
        RectTransform _card;
        Text _kind, _name, _title, _region, _best, _item;
        readonly Image[] _stars = new Image[3];
        readonly Text[] _rows = new Text[3];
        Image _itemIcon;
        RectTransform _itemRow;
        UIButton _play;
        LevelData _level;

        protected override void Build()
        {
            Scrim(() => Hide());
            _card = Card(RT, "Card", new Vector2(860, 1120), new Vector2(0, 0), Theme.Cyan);

            _kind = UIFactory.Text(_card, "Kind", "", 30, Theme.Gold, TextAnchor.MiddleCenter, FontStyle.Bold);
            _kind.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -40), new Vector2(800, 44));
            _name = UIFactory.Text(_card, "Name", "", 44, Theme.Cyan, TextAnchor.MiddleCenter, FontStyle.Bold);
            _name.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -90), new Vector2(800, 60));
            _title = UIFactory.Text(_card, "Title", "", 66, Theme.Text, TextAnchor.MiddleCenter, FontStyle.Bold);
            _title.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -160), new Vector2(820, 90));
            _region = UIFactory.Text(_card, "Region", "", 30, Theme.TextDim, TextAnchor.MiddleCenter, FontStyle.Italic);
            _region.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -240), new Vector2(800, 50));

            for (int i = 0; i < 3; i++)
            {
                float y = -340 - i * 100;
                var row = UIFactory.Rect("Row" + i, _card).Place(new Vector2(0.5f, 1), new Vector2(0.5f, 0.5f), new Vector2(0, y), new Vector2(740, 88));
                var bg = UIFactory.Image(row, "Bg", SpriteFactory.Panel, new Color(0.12f, 0.14f, 0.34f, 0.8f));
                bg.rectTransform.Fill();
                _stars[i] = UIFactory.Image(row, "Star", SpriteFactory.Star, Theme.StarOff);
                _stars[i].rectTransform.Place(new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(20, 0), new Vector2(62, 62));
                _rows[i] = UIFactory.Text(row, "Text", "", 36, Theme.Text, TextAnchor.MiddleLeft);
                _rows[i].rectTransform.Fill(100, 0, 16, 0);
            }

            _best = UIFactory.Text(_card, "Best", "", 34, Theme.TextDim, TextAnchor.MiddleCenter);
            _best.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0.5f, 0.5f), new Vector2(0, -660), new Vector2(760, 50));

            _itemRow = UIFactory.Rect("Item", _card).Place(new Vector2(0.5f, 1), new Vector2(0.5f, 0.5f), new Vector2(0, -745), new Vector2(740, 90));
            _itemIcon = UIFactory.Image(_itemRow, "Icon", SpriteFactory.Key, Theme.Gold);
            _itemIcon.rectTransform.Place(new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(20, 0), new Vector2(70, 70));
            _item = UIFactory.Text(_itemRow, "Text", "", 34, Theme.Text, TextAnchor.MiddleLeft, FontStyle.Bold);
            _item.rectTransform.Fill(110, 0, 10, 0);

            _play = UIFactory.Button(_card, "Play", "PLAY", Theme.ButtonPrimary, () =>
            {
                if (_level != null) Game.PlayLevel(_level);
            }, null, 64, new Color(0.03f, 0.05f, 0.16f));
            ((RectTransform)_play.transform).Place(new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 60), new Vector2(620, 160));

            var close = UIFactory.Button(_card, "Close", "", Theme.PanelLight, () => Hide(), SpriteFactory.Cross);
            ((RectTransform)close.transform).Place(new Vector2(1, 1), new Vector2(0.5f, 0.5f), new Vector2(-30, -30), new Vector2(96, 96));
        }

        public void Show(LevelData l)
        {
            _level = l;
            var p = Game.Progress;
            int stars = p.StarsFor(l);
            var rec = p.Record(l);

            _kind.text = l.Kind == LevelKind.Normal ? "" : l.Kind.ToString().ToUpperInvariant();
            _kind.color = HudScreen.KindColor(l.Kind);
            _name.text = l.DisplayName;
            _name.color = HudScreen.KindColor(l.Kind);
            _title.text = l.Title;
            _region.text = l.Region;

            string[] txt = { "Complete the level", $"Finish in {l.MoveTarget} moves or fewer", l.MasteryDescription };
            int[] bits = { Stars.Complete, Stars.Target, Stars.Mastery };
            for (int i = 0; i < 3; i++)
            {
                bool has = (stars & bits[i]) != 0;
                _stars[i].color = has ? Theme.Gold : Theme.StarOff;
                _rows[i].text = txt[i];
                _rows[i].color = has ? Theme.Text : Theme.Text.WithAlpha(0.8f);
            }

            _best.text = rec != null && rec.completed ? $"Best: {rec.bestMoves} moves" : "Not solved yet";

            var item = l.SpecialItem;
            _itemRow.gameObject.SetActive(item != ItemType.None);
            if (item != ItemType.None)
            {
                bool found = p.HasItemFrom(l);
                _itemIcon.sprite = SpriteFactory.ForItem(item);
                _itemIcon.color = found ? SpriteFactory.ItemColor(item) : SpriteFactory.ItemColor(item).WithAlpha(0.45f);
                _item.text = found ? SpriteFactory.ItemName(item) + " found" : "A " + SpriteFactory.ItemName(item).ToLowerInvariant() + " is hidden here";
                _item.color = found ? Theme.Text : Theme.TextDim;
            }

            transform.SetAsLastSibling();
            UIFactory.Fade(gameObject, true, 0.15f);
            UIFactory.PopIn(_card, 0f, 0.3f, 0.8f);
        }

        public void Hide() => UIFactory.Fade(gameObject, false, 0.12f);
    }
}
