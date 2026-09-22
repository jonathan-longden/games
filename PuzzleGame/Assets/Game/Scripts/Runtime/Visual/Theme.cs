using UnityEngine;

namespace PuzzleGame
{
    /// <summary>The whole palette in one place: dark indigo world, glowing accents.</summary>
    public static class Theme
    {
        static Color Hex(string hex)
        {
            ColorUtility.TryParseHtmlString(hex, out var c);
            return c;
        }

        // World
        public static readonly Color BgTop = Hex("#070A1F");
        public static readonly Color BgBottom = Hex("#1B1F52");
        public static readonly Color BoardShadow = new Color(0.02f, 0.02f, 0.08f, 0.55f);
        public static readonly Color FloorA = Hex("#2A3067");
        public static readonly Color FloorB = Hex("#2E356F");
        public static readonly Color FloorEdge = Hex("#3A428A");
        public static readonly Color WallBase = Hex("#12153A");
        public static readonly Color WallTop = Hex("#232960");
        public static readonly Color WallRim = Hex("#3B4390");

        // Accents
        public static readonly Color Cyan = Hex("#5EE6EB");
        public static readonly Color Gold = Hex("#FFC94D");
        public static readonly Color Violet = Hex("#A57CFF");
        public static readonly Color Pink = Hex("#FF6FA9");
        public static readonly Color Green = Hex("#6CF0A0");
        public static readonly Color Crimson = Hex("#FF5A6E");

        // Objects
        public static readonly Color Stone = Hex("#8C8FB8");
        public static readonly Color StoneDark = Hex("#5B5E8C");
        public static readonly Color StoneLight = Hex("#C4C7E8");
        public static readonly Color EchoBody = new Color(0.65f, 0.49f, 1f, 0.85f);
        public static readonly Color Player = Hex("#F4F1FF");

        // UI
        public static readonly Color Text = Hex("#EEF0FF");
        public static readonly Color TextDim = Hex("#8A90C8");
        public static readonly Color Panel = new Color(0.07f, 0.08f, 0.2f, 0.94f);
        public static readonly Color PanelLight = Hex("#262C66");
        public static readonly Color Button = Hex("#2E3680");
        public static readonly Color ButtonPrimary = Hex("#3FC7D4");
        public static readonly Color Scrim = new Color(0.01f, 0.01f, 0.06f, 0.72f);
        public static readonly Color StarOff = new Color(1f, 1f, 1f, 0.16f);

        /// <summary>Switch/door colour per channel.</summary>
        public static Color Channel(int channel)
        {
            switch (channel)
            {
                case 0: return Cyan;
                case 1: return Pink;
                case 2: return Green;
                default: return Gold;
            }
        }

        public static Color WithAlpha(this Color c, float a) => new Color(c.r, c.g, c.b, a);
    }
}
