using System;
using System.Collections.Generic;
using PuzzleGame.Core;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

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
                "Swipe or use the pad to move.\nUndo is free. Reset can be undone too.",
                30, Theme.TextDim, TextAnchor.MiddleCenter);
            help.rectTransform.Place(new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 40), new Vector2(680, 120));
        }

        UIButton Add(string label, Color c, Action a, ref float y, Color text)
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
    /// LEVEL COMPLETE. Stars appear one at a time, coins fly into the wallet, and
    /// crystals and bonus items get their own moments. It all settles in about
    /// 1.6 s, and tapping anywhere finishes it instantly. The buttons work from
    /// the first frame.
    /// </summary>
    public sealed class CompleteScreen : UIScreen
    {
        const float RowGap = 18f;

        RectTransform _card, _walletRect;
        Text _title, _subtitle, _badge, _moves, _best, _coins, _coinNote, _crystal, _itemName, _itemTip, _unlock, _wallet;
        RectTransform _badgeRow, _movesRow, _objectivesRow, _coinRow, _crystalRow, _itemRow, _unlockRow;
        Image _itemIcon, _itemRays, _coinIcon;
        readonly Image[] _stars = new Image[3];
        readonly Image[] _starGlow = new Image[3];
        readonly Image[] _checks = new Image[3];
        readonly Text[] _objectives = new Text[3];
        UIButton _next, _replay, _map;
        readonly List<Image> _pool = new List<Image>();

        // Every animation of one showing shares this owner, so a tap can finish them all at once.
        object _anim = new object();
        bool _skipping;
        int _walletShown, _walletTarget;

        protected override void Build()
        {
            Scrim(Skip);
            _card = Card(RT, "Card", new Vector2(940, 1560), new Vector2(0, -10), Theme.Gold);
            var cardBg = _card.Find("Bg").gameObject.AddComponent<UIButton>();
            cardBg.Animate = false;
            cardBg.Clicked = Skip;

            _title = UIFactory.Text(_card, "Title", "LEVEL COMPLETE", 72, Theme.Gold, TextAnchor.MiddleCenter, FontStyle.Bold).Glow(new Color(0.6f, 0.3f, 0f, 0.6f), 5);
            _title.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -44), new Vector2(900, 100));
            _subtitle = UIFactory.Text(_card, "Subtitle", "", 36, Theme.TextDim, TextAnchor.MiddleCenter, FontStyle.Italic);
            _subtitle.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -136), new Vector2(900, 56));

            float[] sx = { -230, 0, 230 };
            float[] sy = { -300, -275, -300 };
            for (int i = 0; i < 3; i++)
            {
                _starGlow[i] = UIFactory.Image(_card, "StarGlow" + i, SpriteFactory.Glow, Color.clear);
                _starGlow[i].rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0.5f, 0.5f), new Vector2(sx[i], sy[i]), new Vector2(360, 360));
                _stars[i] = UIFactory.Image(_card, "Star" + i, SpriteFactory.Star, Theme.StarOff);
                _stars[i].rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0.5f, 0.5f), new Vector2(sx[i], sy[i]), new Vector2(i == 1 ? 220 : 180, i == 1 ? 220 : 180));
            }

            // Rows below the stars are stacked at show time, so absent rows leave no gaps.
            _badgeRow = Row("Badge", 60);
            var badgeBg = UIFactory.Image(_badgeRow, "Bg", SpriteFactory.Panel, Theme.Pink.WithAlpha(0.25f));
            badgeBg.rectTransform.Place(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(460, 60));
            _badge = UIFactory.Text(_badgeRow, "Text", "", 32, Theme.Pink, TextAnchor.MiddleCenter, FontStyle.Bold);
            _badge.rectTransform.Fill();

            _movesRow = Row("Moves", 120);
            _moves = UIFactory.Text(_movesRow, "Moves", "", 60, Theme.Text, TextAnchor.MiddleCenter, FontStyle.Bold);
            _moves.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0.5f, 1), Vector2.zero, new Vector2(860, 72));
            _moves.supportRichText = true;
            _best = UIFactory.Text(_movesRow, "Best", "", 34, Theme.TextDim, TextAnchor.MiddleCenter, FontStyle.Bold);
            _best.rectTransform.Place(new Vector2(0.5f, 0), new Vector2(0.5f, 0), Vector2.zero, new Vector2(860, 46));
            _best.supportRichText = true;

            _objectivesRow = Row("Objectives", 3 * 58);
            for (int i = 0; i < 3; i++)
            {
                float y = -29 - i * 58;
                _checks[i] = UIFactory.Image(_objectivesRow, "Check" + i, SpriteFactory.Check, Theme.Green);
                _checks[i].rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0.5f, 0.5f), new Vector2(-330, y), new Vector2(46, 46));
                _objectives[i] = UIFactory.Text(_objectivesRow, "Objective" + i, "", 34, Theme.Text, TextAnchor.MiddleLeft);
                _objectives[i].rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0, 0.5f), new Vector2(-285, y), new Vector2(640, 56));
            }

            _coinRow = Row("Coins", 100);
            _coinIcon = UIFactory.Image(_coinRow, "Icon", SpriteFactory.Coin, Theme.Gold);
            _coinIcon.rectTransform.Place(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-210, 6), new Vector2(84, 84));
            _coins = UIFactory.Text(_coinRow, "Text", "", 62, Theme.Gold, TextAnchor.MiddleLeft, FontStyle.Bold);
            _coins.rectTransform.Place(new Vector2(0.5f, 0.5f), new Vector2(0, 0.5f), new Vector2(-155, 6), new Vector2(560, 90));
            _coinNote = UIFactory.Text(_coinRow, "Note", "", 30, Theme.TextDim, TextAnchor.MiddleCenter, FontStyle.Bold);
            _coinNote.rectTransform.Place(new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, -26), new Vector2(860, 40));

            _crystalRow = Row("Crystal", 84);
            var cBg = UIFactory.Image(_crystalRow, "Bg", SpriteFactory.Panel, new Color(0.1f, 0.3f, 0.4f, 0.55f));
            cBg.rectTransform.Place(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(560, 80));
            var cIcon = UIFactory.Image(_crystalRow, "Icon", SpriteFactory.Crystal, Theme.Cyan);
            cIcon.rectTransform.Place(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-210, 0), new Vector2(56, 56));
            _crystal = UIFactory.Text(_crystalRow, "Text", "CRYSTAL FOUND", 40, Theme.Cyan, TextAnchor.MiddleLeft, FontStyle.Bold);
            _crystal.rectTransform.Place(new Vector2(0.5f, 0.5f), new Vector2(0, 0.5f), new Vector2(-165, 0), new Vector2(420, 70));

            // Bonus items are rarer than coins, so they get a much bigger moment.
            _itemRow = Row("Item", 170);
            _itemRays = UIFactory.Image(_itemRow, "Rays", SpriteFactory.Star, Theme.Pink.WithAlpha(0.25f));
            _itemRays.rectTransform.Place(new Vector2(0, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(120, 0), new Vector2(230, 230));
            var itemBg = UIFactory.Image(_itemRow, "Bg", SpriteFactory.Panel, new Color(0.28f, 0.1f, 0.38f, 0.9f));
            itemBg.rectTransform.Fill(20, 0, 20, 0);
            var itemOl = UIFactory.Image(_itemRow, "Outline", SpriteFactory.PanelOutline, Theme.Pink.WithAlpha(0.9f));
            itemOl.rectTransform.Fill(20, 0, 20, 0);
            _itemIcon = UIFactory.Image(_itemRow, "Icon", SpriteFactory.Gem, Theme.Pink);
            _itemIcon.rectTransform.Place(new Vector2(0, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(120, 0), new Vector2(120, 120));
            var itemHead = UIFactory.Text(_itemRow, "Head", "BONUS ITEM FOUND", 30, Theme.Text.WithAlpha(0.8f), TextAnchor.LowerLeft, FontStyle.Bold);
            itemHead.rectTransform.Place(new Vector2(0, 0.5f), new Vector2(0, 0), new Vector2(210, 30), new Vector2(620, 44));
            _itemName = UIFactory.Text(_itemRow, "Name", "", 54, Theme.Pink, TextAnchor.UpperLeft, FontStyle.Bold);
            _itemName.rectTransform.Place(new Vector2(0, 0.5f), new Vector2(0, 1), new Vector2(210, 30), new Vector2(620, 66));
            _itemTip = UIFactory.Text(_itemRow, "Tip", "", 28, Theme.TextDim, TextAnchor.UpperLeft, FontStyle.Italic);
            _itemTip.rectTransform.Place(new Vector2(0, 0.5f), new Vector2(0, 1), new Vector2(210, -36), new Vector2(640, 40));

            _unlockRow = Row("Unlock", 60);
            _unlock = UIFactory.Text(_unlockRow, "Text", "", 34, Theme.Violet, TextAnchor.MiddleCenter, FontStyle.Bold);
            _unlock.rectTransform.Fill();

            _next = UIFactory.Button(_card, "Next", "NEXT LEVEL", Theme.ButtonPrimary, () => Game.PlayNext(), null, 56, new Color(0.03f, 0.05f, 0.16f));
            ((RectTransform)_next.transform).Place(new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 200), new Vector2(760, 150));
            _replay = UIFactory.Button(_card, "Replay", "REPLAY", Theme.Button, () => Game.RestartLevel(), SpriteFactory.Restart, 44);
            ((RectTransform)_replay.transform).Place(new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(-195, 50), new Vector2(370, 124));
            _map = UIFactory.Button(_card, "Map", "MAP", Theme.Button, () => Game.OpenMap(), SpriteFactory.Home, 44);
            ((RectTransform)_map.transform).Place(new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(195, 50), new Vector2(370, 124));

            // Wallet in the corner: the coins visibly land somewhere.
            _walletRect = UIFactory.Rect("Wallet", RT).Place(new Vector2(1, 1), new Vector2(1, 1), new Vector2(-30, -30), new Vector2(280, 100));
            var wBg = UIFactory.Image(_walletRect, "Bg", SpriteFactory.Panel, Theme.Panel);
            wBg.rectTransform.Fill();
            _wallet = UIFactory.Chip(_walletRect, "Coins", SpriteFactory.Coin, Theme.Gold, "0", 46, new Vector2(0.5f, 0.5f), Vector2.zero, 230);
        }

        RectTransform Row(string name, float height)
        {
            var r = UIFactory.Rect(name, _card);
            r.anchorMin = r.anchorMax = new Vector2(0.5f, 1);
            r.pivot = new Vector2(0.5f, 1);
            r.sizeDelta = new Vector2(900, height);
            return r;
        }

        public void Show(CompletionResult r, bool hasNext)
        {
            var l = r.Level;
            _skipping = true;          // finish anything left from the previous showing, silently
            Tween.Kill(_anim, true);
            _anim = new object();
            _skipping = false;
            transform.SetAsLastSibling();

            _title.text = l.Kind == LevelKind.Boss ? "SPIRE CONQUERED" : l.Kind == LevelKind.Secret ? "SECRET SOLVED" : "LEVEL COMPLETE";
            _subtitle.text = l.DisplayName + "  ·  " + l.Title;

            bool[] ok = { true, (r.StarsThisRun & Stars.Target) != 0, (r.StarsThisRun & Stars.Mastery) != 0 };
            string[] txt = { "Complete the level", $"Finish in {l.MoveTarget} moves or fewer", l.MasteryDescription };
            for (int i = 0; i < 3; i++)
            {
                _objectives[i].text = txt[i];
                _objectives[i].color = ok[i] ? Theme.Text : Theme.TextDim;
                _checks[i].sprite = ok[i] ? SpriteFactory.Check : SpriteFactory.Cross;
                _checks[i].color = ok[i] ? Theme.Green : Theme.TextDim.WithAlpha(0.5f);
            }

            string movesColor = r.Moves <= r.Target ? "#EEF0FF" : "#FF6FA9";
            _moves.text = $"<color={movesColor}>{r.Moves}</color> MOVES";
            _best.text = r.NewBest && !r.FirstClear ? $"<color=#5EE6EB>NEW BEST: {r.BestMoves}</color>" : $"BEST: {r.BestMoves}";

            bool hard = l.Kind == LevelKind.Hard || l.Kind == LevelKind.Boss;
            _badge.text = l.Kind == LevelKind.Boss ? "BOSS LEVEL" : l.Kind == LevelKind.Hard ? "HARD LEVEL" : l.Kind == LevelKind.Bonus ? "BONUS LEVEL" : l.Kind == LevelKind.Secret ? "SECRET LEVEL" : "";
            _badge.color = HudScreen.KindColor(l.Kind);
            _badgeRow.Find("Bg").GetComponent<Image>().color = HudScreen.KindColor(l.Kind).WithAlpha(0.22f);

            _coins.text = r.TotalCoins > 0 ? "+0 COINS" : "";
            _coinIcon.enabled = r.TotalCoins > 0;
            _coinNote.text = r.TotalCoins > 0
                ? (hard && r.BaseCoins > 0 ? (l.Kind == LevelKind.Boss ? "Boss reward" : "Hard level reward") : "")
                : "All rewards for this level already collected";

            bool crystal = r.CrystalCollected;
            _itemRow.gameObject.SetActive(r.ItemFound != ItemType.None);
            if (r.ItemFound != ItemType.None)
            {
                var c = SpriteFactory.ItemColor(r.ItemFound);
                _itemIcon.sprite = SpriteFactory.ForItem(r.ItemFound);
                _itemIcon.color = c;
                _itemRays.color = c.WithAlpha(0.3f);
                _itemName.text = SpriteFactory.ItemName(r.ItemFound);
                _itemName.color = c;
                _itemTip.text = r.ItemFound == ItemType.Key ? "It might open something sealed on the path..." : "Added to your collection.";
            }

            bool secret = false;
            string unlockText = "";
            foreach (var id in r.NewlyUnlocked)
            {
                var ul = Game.Levels.Get(id);
                if (ul == null) continue;
                if (ul.Kind == LevelKind.Bonus) unlockText = "A hidden branch opened on the path!";
                else if (ul.Kind == LevelKind.Secret) { unlockText = "Something sealed has opened..."; secret = true; }
            }
            _unlock.text = unlockText;

            // Stack the rows that apply, top to bottom.
            float y = -425;
            Stack(_badgeRow, _badge.text.Length > 0, ref y);
            Stack(_movesRow, true, ref y);
            Stack(_objectivesRow, true, ref y);
            Stack(_coinRow, true, ref y);
            Stack(_crystalRow, crystal, ref y);
            Stack(_itemRow, r.ItemFound != ItemType.None, ref y);
            Stack(_unlockRow, unlockText.Length > 0, ref y);

            _next.gameObject.SetActive(hasNext);
            ((RectTransform)_replay.transform).anchoredPosition = new Vector2(-195, hasNext ? 50 : 110);
            ((RectTransform)_map.transform).anchoredPosition = new Vector2(195, hasNext ? 50 : 110);

            _walletTarget = Game.Currency.Coins;
            _walletShown = _walletTarget - r.TotalCoins;
            _wallet.text = _walletShown.ToString();

            UIFactory.Fade(gameObject, true, 0.15f);
            Pop(_card, 0f, 0.3f, 0.85f);
            AnimateStars(r);
            AnimateCoins(r.TotalCoins);
            if (crystal)
            {
                Pop(_crystalRow, 0.55f, 0.3f, 0.5f);
                Later(0.55f, () => Sound(Sfx.Pickup, 0.8f));
            }
            if (r.ItemFound != ItemType.None)
            {
                Pop(_itemRow, 0.85f, 0.45f, 0.2f);
                Later(0.85f, () => Sound(Sfx.Item, 1f));
            }
            if (unlockText.Length > 0)
            {
                Pop(_unlockRow, 1.15f, 0.35f, 0.5f);
                Later(1.15f, () => Sound(secret ? Sfx.Secret : Sfx.Unlock, 0.8f));
            }
        }

        void Stack(RectTransform row, bool visible, ref float y)
        {
            row.gameObject.SetActive(visible);
            if (!visible) return;
            row.anchoredPosition = new Vector2(0, y);
            row.localScale = Vector3.one;
            y -= row.sizeDelta.y + RowGap;
        }

        /// <summary>Tap anywhere: every running animation jumps to its end, silently.</summary>
        void Skip()
        {
            _skipping = true;
            Tween.Kill(_anim, true);
            _walletShown = _walletTarget;
            _wallet.text = _walletTarget.ToString();
            foreach (var p in _pool) p.gameObject.SetActive(false);
            _skipping = false;
        }

        void Sound(Sfx s, float volume, float pitch = 1f)
        {
            if (!_skipping) Game.Audio.Play(s, volume, pitch);
        }

        void Later(float delay, Action a) => Tween.Delay(_anim, delay, a);

        void Pop(Transform t, float delay, float duration, float from)
        {
            t.localScale = Vector3.one * from;
            Tween.Run(_anim, duration, v => { if (t) t.localScale = Vector3.one * Mathf.LerpUnclamped(from, 1f, v); }, Ease.OutBack, null, delay);
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
                star.transform.localScale = Vector3.one;
                glow.color = Color.clear;
                if (thisRun)
                {
                    star.color = Theme.StarOff;
                    int idx = i;
                    float delay = 0.2f + i * 0.2f;
                    Later(delay, () =>
                    {
                        star.color = Theme.Gold;
                        glow.color = Theme.Gold.WithAlpha(0.45f);
                        Sound(Sfx.Star, 0.9f, 1f + idx * 0.12f);
                        if (!_skipping) Sparks(star.rectTransform.anchoredPosition);
                    });
                    Pop(star.transform, delay, 0.35f, 0.2f);
                }
                else
                {
                    // Earned on an earlier attempt: stars are cumulative, so show it, faded.
                    star.color = before ? Theme.Gold.WithAlpha(0.4f) : Theme.StarOff;
                }
            }
        }

        void AnimateCoins(int total)
        {
            if (total <= 0) return;
            var label = _coins;
            Tween.Run(_anim, 0.5f, v => label.text = $"+{Mathf.RoundToInt(total * v)} COINS", Ease.OutCubic, null, 0.7f);

            // Coins fly from the reward row into the wallet, each one ticking the counter up.
            const int flying = 8;
            int per = total / flying, rest = total - per * flying;
            for (int i = 0; i < flying; i++)
            {
                int amount = per + (i == flying - 1 ? rest : 0);
                float delay = 0.8f + i * 0.06f;
                var coin = Pooled();
                coin.gameObject.SetActive(false);
                var from = (Vector2)_card.InverseTransformPoint(_coinIcon.rectTransform.position);
                var to = (Vector2)_card.InverseTransformPoint(_walletRect.position) + new Vector2(-190, -50);
                var mid = (from + to) * 0.5f + new Vector2(Random.Range(-160f, 160f), 180f);
                var rt = coin.rectTransform;
                Tween.Run(_anim, 0.45f, v =>
                {
                    if (!coin) return;
                    coin.gameObject.SetActive(v < 1f && !_skipping);
                    float u = 1f - v;
                    rt.anchoredPosition = u * u * from + 2f * u * v * mid + v * v * to;
                    rt.localScale = Vector3.one * Mathf.Lerp(1f, 0.6f, v);
                }, Ease.InQuad, () =>
                {
                    if (coin) coin.gameObject.SetActive(false);
                    _walletShown = Mathf.Min(_walletTarget, _walletShown + amount);
                    _wallet.text = _walletShown.ToString();
                    Sound(Sfx.Coin, 0.3f, 1f + Random.Range(-0.05f, 0.1f));
                }, delay);
            }
            Later(0.8f + flying * 0.06f + 0.5f, () =>
            {
                _walletShown = _walletTarget;
                _wallet.text = _walletTarget.ToString();
                var w = _walletRect;
                Tween.Run(_anim, 0.2f, v => w.localScale = Vector3.one * (1f + 0.12f * Mathf.Sin(v * Mathf.PI)), Ease.Linear);
            });
        }

        Image Pooled()
        {
            foreach (var p in _pool)
                if (!p.gameObject.activeSelf && p.sprite == SpriteFactory.Coin) return p;
            var img = UIFactory.Image(_card, "FlyingCoin", SpriteFactory.Coin, Theme.Gold);
            img.rectTransform.anchorMin = img.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            img.rectTransform.sizeDelta = new Vector2(64, 64);
            _pool.Add(img);
            return img;
        }

        void Sparks(Vector2 at)
        {
            for (int i = 0; i < 10; i++)
            {
                Image s = null;
                foreach (var x in _pool) if (!x.gameObject.activeSelf && x.sprite == SpriteFactory.Star) { s = x; break; }
                if (s == null)
                {
                    s = UIFactory.Image(_card, "Spark", SpriteFactory.Star, Theme.Gold);
                    _pool.Add(s);
                }
                s.gameObject.SetActive(true);
                var rt = s.rectTransform;
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1);
                rt.sizeDelta = Vector2.one * Random.Range(22f, 40f);
                float a = Random.Range(0f, Mathf.PI * 2f);
                var dir = new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * Random.Range(140f, 260f);
                var start = at;
                var img = s;
                Tween.Run(_anim, 0.55f, v =>
                {
                    rt.anchoredPosition = start + dir * v;
                    img.color = Theme.Gold.WithAlpha(1f - v);
                    rt.localRotation = Quaternion.Euler(0, 0, v * 180f);
                }, Ease.OutCubic, () => img.gameObject.SetActive(false));
            }
        }

        void Update()
        {
            if (_itemRow != null && _itemRow.gameObject.activeSelf)
                _itemRays.rectTransform.localRotation = Quaternion.Euler(0, 0, Time.unscaledTime * 25f);
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
            _card = Card(RT, "Card", new Vector2(880, 1140), new Vector2(0, 0), Theme.Cyan);

            _kind = UIFactory.Text(_card, "Kind", "", 32, Theme.Gold, TextAnchor.MiddleCenter, FontStyle.Bold);
            _kind.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -40), new Vector2(800, 44));
            _name = UIFactory.Text(_card, "Name", "", 44, Theme.Cyan, TextAnchor.MiddleCenter, FontStyle.Bold);
            _name.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -90), new Vector2(800, 60));
            _title = UIFactory.Text(_card, "Title", "", 66, Theme.Text, TextAnchor.MiddleCenter, FontStyle.Bold);
            _title.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -160), new Vector2(840, 90));
            _region = UIFactory.Text(_card, "Region", "", 32, Theme.TextDim, TextAnchor.MiddleCenter, FontStyle.Italic);
            _region.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -240), new Vector2(800, 50));

            for (int i = 0; i < 3; i++)
            {
                float y = -340 - i * 104;
                var row = UIFactory.Rect("Row" + i, _card).Place(new Vector2(0.5f, 1), new Vector2(0.5f, 0.5f), new Vector2(0, y), new Vector2(760, 92));
                var bg = UIFactory.Image(row, "Bg", SpriteFactory.Panel, new Color(0.12f, 0.14f, 0.34f, 0.8f));
                bg.rectTransform.Fill();
                _stars[i] = UIFactory.Image(row, "Star", SpriteFactory.Star, Theme.StarOff);
                _stars[i].rectTransform.Place(new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(20, 0), new Vector2(64, 64));
                _rows[i] = UIFactory.Text(row, "Text", "", 36, Theme.Text, TextAnchor.MiddleLeft);
                _rows[i].rectTransform.Fill(104, 0, 16, 0);
            }

            _best = UIFactory.Text(_card, "Best", "", 34, Theme.TextDim, TextAnchor.MiddleCenter);
            _best.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0.5f, 0.5f), new Vector2(0, -672), new Vector2(760, 50));

            _itemRow = UIFactory.Rect("Item", _card).Place(new Vector2(0.5f, 1), new Vector2(0.5f, 0.5f), new Vector2(0, -760), new Vector2(760, 90));
            _itemIcon = UIFactory.Image(_itemRow, "Icon", SpriteFactory.Key, Theme.Gold);
            _itemIcon.rectTransform.Place(new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(20, 0), new Vector2(74, 74));
            _item = UIFactory.Text(_itemRow, "Text", "", 34, Theme.Text, TextAnchor.MiddleLeft, FontStyle.Bold);
            _item.rectTransform.Fill(116, 0, 10, 0);

            _play = UIFactory.Button(_card, "Play", "PLAY", Theme.ButtonPrimary, () =>
            {
                if (_level != null) Game.PlayLevel(_level);
            }, null, 64, new Color(0.03f, 0.05f, 0.16f));
            ((RectTransform)_play.transform).Place(new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 60), new Vector2(640, 170));

            var close = UIFactory.Button(_card, "Close", "", Theme.PanelLight, () => Hide(), SpriteFactory.Cross);
            ((RectTransform)close.transform).Place(new Vector2(1, 1), new Vector2(0.5f, 0.5f), new Vector2(-30, -30), new Vector2(110, 110));
        }

        public void Show(LevelData l)
        {
            _level = l;
            var p = Game.Progress;
            int stars = p.StarsFor(l);
            var rec = p.Record(l);

            _kind.text = l.Kind == LevelKind.Normal ? "" : l.Kind.ToString().ToUpperInvariant() + " LEVEL";
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
                _item.text = found ? SpriteFactory.ItemName(item) + " found" : "Something rare is hidden here";
                _item.color = found ? Theme.Text : Theme.TextDim;
            }

            transform.SetAsLastSibling();
            UIFactory.Fade(gameObject, true, 0.15f);
            UIFactory.PopIn(_card, 0f, 0.3f, 0.8f);
        }

        public void Hide() => UIFactory.Fade(gameObject, false, 0.12f);
    }
}
