using System;
using System.Collections.Generic;
using UnityEngine;

namespace PuzzleGame
{
    /// <summary>
    /// Every sprite in the game is generated here at startup from signed distance
    /// functions, so the project has no art dependencies and everything stays crisp.
    /// Sprites are white; colour comes from SpriteRenderer.color / Image.color.
    /// World sprites are 1 unit wide (one grid cell).
    /// </summary>
    public static class SpriteFactory
    {
        static readonly Dictionary<string, Sprite> Cache = new Dictionary<string, Sprite>();

        public delegate float Sdf(Vector2 p); // p in [-1,1]^2, y up; negative inside

        public static void ClearCache()
        {
            Cache.Clear();
        }

        /// <summary>Renders an SDF into an anti-aliased white sprite.</summary>
        public static Sprite Make(string key, int size, Sdf sdf, float softness = 1f, Vector4 border = default, float ppu = -1)
        {
            if (Cache.TryGetValue(key, out var cached) && cached != null) return cached;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = key,
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
            };
            var px = new Color32[size * size];
            float pixel = 2f / size;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    var p = new Vector2((x + 0.5f) / size * 2f - 1f, (y + 0.5f) / size * 2f - 1f);
                    float d = sdf(p);
                    float a = Mathf.Clamp01(0.5f - d / (pixel * softness));
                    px[y * size + x] = new Color32(255, 255, 255, (byte)(a * 255f));
                }
            }
            tex.SetPixels32(px);
            tex.Apply(false, true);
            var sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f),
                ppu > 0 ? ppu : size, 0, SpriteMeshType.FullRect, border);
            sprite.name = key;
            Cache[key] = sprite;
            return sprite;
        }

        /// <summary>Soft radial glow (alpha falls off smoothly to the edge).</summary>
        public static Sprite Glow
        {
            get
            {
                const string key = "glow";
                if (Cache.TryGetValue(key, out var s) && s != null) return s;
                const int size = 128;
                var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };
                var px = new Color32[size * size];
                for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float dx = (x + 0.5f) / size * 2f - 1f, dy = (y + 0.5f) / size * 2f - 1f;
                    float r = Mathf.Sqrt(dx * dx + dy * dy);
                    float a = Mathf.Clamp01(1f - r);
                    a = a * a * (3f - 2f * a);
                    a *= a;
                    px[y * size + x] = new Color32(255, 255, 255, (byte)(a * 255f));
                }
                tex.SetPixels32(px);
                tex.Apply(false, true);
                s = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
                s.name = key;
                Cache[key] = s;
                return s;
            }
        }

        /// <summary>Vertical gradient used for the sky behind everything.</summary>
        public static Sprite Gradient(Color top, Color bottom)
        {
            string key = "gradient" + ColorUtility.ToHtmlStringRGBA(top) + ColorUtility.ToHtmlStringRGBA(bottom);
            if (Cache.TryGetValue(key, out var s) && s != null) return s;
            const int h = 256;
            var tex = new Texture2D(1, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };
            for (int y = 0; y < h; y++)
            {
                float t = y / (float)(h - 1);
                t = t * t * (3f - 2f * t);
                tex.SetPixel(0, y, Color.Lerp(bottom, top, t));
            }
            tex.Apply(false, true);
            s = Sprite.Create(tex, new Rect(0, 0, 1, h), new Vector2(0.5f, 0.5f), 1);
            s.name = key;
            Cache[key] = s;
            return s;
        }

        /// <summary>White, transparent at the bottom and opaque at the top (fog, bar backgrounds).</summary>
        public static Sprite FadeUp => Gradient(Color.white, new Color(1f, 1f, 1f, 0f));

        // ------------------------------------------------------------ SDF helpers

        public static float Circle(Vector2 p, Vector2 c, float r) => (p - c).magnitude - r;

        public static float Box(Vector2 p, Vector2 c, Vector2 half, float radius)
        {
            var q = new Vector2(Mathf.Abs(p.x - c.x), Mathf.Abs(p.y - c.y)) - half + new Vector2(radius, radius);
            var mq = new Vector2(Mathf.Max(q.x, 0), Mathf.Max(q.y, 0));
            return mq.magnitude + Mathf.Min(Mathf.Max(q.x, q.y), 0) - radius;
        }

        public static float Segment(Vector2 p, Vector2 a, Vector2 b, float thickness)
        {
            var pa = p - a;
            var ba = b - a;
            float h = Mathf.Clamp01(Vector2.Dot(pa, ba) / Vector2.Dot(ba, ba));
            return (pa - ba * h).magnitude - thickness;
        }

        public static float Polygon(Vector2 p, Vector2[] v)
        {
            float d = Vector2.Dot(p - v[0], p - v[0]);
            float s = 1f;
            for (int i = 0, j = v.Length - 1; i < v.Length; j = i, i++)
            {
                var e = v[j] - v[i];
                var w = p - v[i];
                var b = w - e * Mathf.Clamp01(Vector2.Dot(w, e) / Vector2.Dot(e, e));
                d = Mathf.Min(d, Vector2.Dot(b, b));
                bool c1 = p.y >= v[i].y, c2 = p.y < v[j].y, c3 = e.x * w.y > e.y * w.x;
                if ((c1 && c2 && c3) || (!c1 && !c2 && !c3)) s = -s;
            }
            return s * Mathf.Sqrt(d);
        }

        public static float Ring(Vector2 p, Vector2 c, float r, float thickness) => Mathf.Abs((p - c).magnitude - r) - thickness;

        static float Union(float a, float b) => Mathf.Min(a, b);
        static float Subtract(float a, float b) => Mathf.Max(a, -b);

        static Vector2[] StarPoints(float outer, float inner, int points = 5, float rot = 90f)
        {
            var v = new Vector2[points * 2];
            for (int i = 0; i < v.Length; i++)
            {
                float ang = (rot + i * 180f / points) * Mathf.Deg2Rad;
                float r = (i % 2 == 0) ? outer : inner;
                v[i] = new Vector2(Mathf.Cos(ang) * r, Mathf.Sin(ang) * r);
            }
            // Polygon() expects consistent winding; reverse to clockwise order is not required
            return v;
        }

        // ------------------------------------------------------------ named sprites

        public static Sprite Tile => Make("tile", 128, p => Box(p, Vector2.zero, new Vector2(0.94f, 0.94f), 0.22f));
        public static Sprite TileOutline => Make("tileOutline", 128, p => Mathf.Abs(Box(p, Vector2.zero, new Vector2(0.9f, 0.9f), 0.22f)) - 0.05f);
        public static Sprite TileInset => Make("tileInset", 128, p => Box(p, Vector2.zero, new Vector2(0.78f, 0.78f), 0.2f));
        public static Sprite Square => Make("square", 64, p => Box(p, Vector2.zero, new Vector2(1f, 1f), 0f));
        public static Sprite Circle64 => Make("circle", 128, p => Circle(p, Vector2.zero, 0.94f));
        public static Sprite RingThin => Make("ringThin", 128, p => Ring(p, Vector2.zero, 0.84f, 0.07f));
        public static Sprite RingThick => Make("ringThick", 128, p => Ring(p, Vector2.zero, 0.78f, 0.14f));
        public static Sprite Dot => Make("dot", 32, p => Circle(p, Vector2.zero, 0.8f));

        public static Sprite Star => Make("star", 128, p => Polygon(p, StarPoints(0.96f, 0.42f)) - 0.02f);
        public static Sprite Diamond => Make("diamond", 128, p => Polygon(p, new[]
            { new Vector2(0, 0.95f), new Vector2(-0.7f, 0), new Vector2(0, -0.95f), new Vector2(0.7f, 0) }));

        public static Sprite Crystal => Make("crystal", 128, p => Polygon(p, new[]
        {
            new Vector2(0, 0.95f), new Vector2(-0.55f, 0.35f), new Vector2(-0.4f, -0.55f),
            new Vector2(0, -0.95f), new Vector2(0.4f, -0.55f), new Vector2(0.55f, 0.35f),
        }));

        public static Sprite Arrow => Make("arrow", 128, p => Union(
            Polygon(p, new[] { new Vector2(0, 0.8f), new Vector2(-0.62f, 0.12f), new Vector2(0.62f, 0.12f) }),
            Box(p, new Vector2(0, -0.3f), new Vector2(0.2f, 0.45f), 0.06f)));

        public static Sprite Chevron => Make("chevron", 128, p => Union(
            Segment(p, new Vector2(-0.55f, -0.25f), new Vector2(0, 0.3f), 0.14f),
            Segment(p, new Vector2(0.55f, -0.25f), new Vector2(0, 0.3f), 0.14f)));

        public static Sprite Lock => Make("lock", 128, p => Union(
            Subtract(Box(p, new Vector2(0, -0.28f), new Vector2(0.62f, 0.5f), 0.14f), Circle(p, new Vector2(0, -0.2f), 0.13f)),
            Subtract(Ring(p, new Vector2(0, 0.25f), 0.38f, 0.1f), Box(p, new Vector2(0, -0.2f), new Vector2(1f, 0.45f), 0f))));

        public static Sprite Key => Make("key", 128, p => Union(Union(
            Ring(p, new Vector2(-0.45f, 0.2f), 0.3f, 0.11f),
            Box(p, new Vector2(0.25f, 0.2f), new Vector2(0.45f, 0.1f), 0.04f)),
            Union(Box(p, new Vector2(0.55f, -0.02f), new Vector2(0.08f, 0.16f), 0.03f),
                  Box(p, new Vector2(0.28f, -0.02f), new Vector2(0.07f, 0.12f), 0.03f))));

        public static Sprite Gem => Make("gem", 128, p => Polygon(p, new[]
        {
            new Vector2(-0.8f, 0.3f), new Vector2(-0.45f, 0.72f), new Vector2(0.45f, 0.72f),
            new Vector2(0.8f, 0.3f), new Vector2(0, -0.85f),
        }));

        public static Sprite Piece => Make("piece", 128, p => Subtract(Union(
            Box(p, new Vector2(-0.1f, -0.1f), new Vector2(0.62f, 0.62f), 0.08f),
            Circle(p, new Vector2(-0.1f, 0.68f), 0.24f)),
            Circle(p, new Vector2(0.62f, -0.1f), 0.24f)));

        public static Sprite Relic => Make("relic", 128, p => Subtract(
            Polygon(p, new[] { new Vector2(0, 0.9f), new Vector2(-0.85f, -0.65f), new Vector2(0.85f, -0.65f) }),
            Subtract(Circle(p, new Vector2(0, -0.12f), 0.3f), Circle(p, new Vector2(0, -0.12f), 0.13f))));

        public static Sprite Coin => Make("coin", 128, p => Subtract(Circle(p, Vector2.zero, 0.92f),
            Ring(p, Vector2.zero, 0.62f, 0.07f)));

        public static Sprite Check => Make("check", 128, p => Union(
            Segment(p, new Vector2(-0.6f, 0.02f), new Vector2(-0.18f, -0.45f), 0.14f),
            Segment(p, new Vector2(-0.18f, -0.45f), new Vector2(0.62f, 0.5f), 0.14f)));

        public static Sprite Cross => Make("cross", 128, p => Union(
            Segment(p, new Vector2(-0.5f, -0.5f), new Vector2(0.5f, 0.5f), 0.13f),
            Segment(p, new Vector2(-0.5f, 0.5f), new Vector2(0.5f, -0.5f), 0.13f)));

        public static Sprite Pause => Make("pause", 128, p => Union(
            Box(p, new Vector2(-0.3f, 0), new Vector2(0.15f, 0.6f), 0.06f),
            Box(p, new Vector2(0.3f, 0), new Vector2(0.15f, 0.6f), 0.06f)));

        public static Sprite Undo => Make("undo", 128, p =>
        {
            var c = new Vector2(0.08f, -0.08f);
            float arc = Ring(p, c, 0.5f, 0.12f);
            // keep the arc away from the lower-left quadrant (where the arrow head sits)
            arc = Subtract(arc, Polygon(p, new[] { c, new Vector2(-1.2f, 0.2f), new Vector2(-1.2f, -1.2f), new Vector2(0.4f, -1.2f) }));
            float head = Polygon(p, new[] { new Vector2(-0.72f, -0.05f), new Vector2(-0.12f, -0.05f), new Vector2(-0.42f, -0.52f) });
            return Union(arc, head);
        });

        public static Sprite Restart => Make("restart", 128, p =>
        {
            float arc = Ring(p, Vector2.zero, 0.55f, 0.12f);
            arc = Subtract(arc, Polygon(p, new[] { Vector2.zero, new Vector2(0.3f, 1.2f), new Vector2(1.2f, 1.2f), new Vector2(1.2f, 0.2f) }));
            float head = Polygon(p, new[] { new Vector2(0.28f, 0.82f), new Vector2(0.28f, 0.2f), new Vector2(0.78f, 0.52f) });
            return Union(arc, head);
        });

        public static Sprite Rune => Make("rune", 128, p => Union(
            Subtract(Polygon(p, new[] { new Vector2(0, 0.9f), new Vector2(-0.9f, 0), new Vector2(0, -0.9f), new Vector2(0.9f, 0) }),
                     Polygon(p, new[] { new Vector2(0, 0.68f), new Vector2(-0.68f, 0), new Vector2(0, -0.68f), new Vector2(0.68f, 0) })),
            Circle(p, Vector2.zero, 0.22f)));

        public static Sprite Bars => Make("bars", 128, p =>
        {
            float d = Box(p, new Vector2(0, 0.78f), new Vector2(0.9f, 0.1f), 0.05f);
            d = Union(d, Box(p, new Vector2(0, -0.78f), new Vector2(0.9f, 0.1f), 0.05f));
            for (int i = -1; i <= 1; i++)
                d = Union(d, Box(p, new Vector2(i * 0.52f, 0), new Vector2(0.11f, 0.85f), 0.05f));
            return d;
        });

        public static Sprite Home => Make("home", 128, p => Union(
            Polygon(p, new[] { new Vector2(0, 0.8f), new Vector2(-0.8f, 0.05f), new Vector2(0.8f, 0.05f) }),
            Box(p, new Vector2(0, -0.35f), new Vector2(0.5f, 0.42f), 0.05f)));

        public static Sprite Speaker => Make("speaker", 128, p => Union(Union(
            Box(p, new Vector2(-0.52f, 0), new Vector2(0.18f, 0.24f), 0.04f),
            Polygon(p, new[] { new Vector2(-0.4f, 0.24f), new Vector2(0.05f, 0.62f), new Vector2(0.05f, -0.62f), new Vector2(-0.4f, -0.24f) })),
            Subtract(Ring(p, new Vector2(0.05f, 0), 0.52f, 0.07f), Box(p, new Vector2(-0.5f, 0), new Vector2(0.55f, 1f), 0))));

        /// <summary>9-sliceable rounded panel for UI.</summary>
        public static Sprite Panel => Make("panel", 96, p => Box(p, Vector2.zero, new Vector2(1f, 1f), 0.5f),
            1f, new Vector4(32, 32, 32, 32), 96);

        /// <summary>Rounded panel outline for UI.</summary>
        public static Sprite PanelOutline => Make("panelOutline", 96, p =>
            Mathf.Abs(Box(p, Vector2.zero, new Vector2(0.96f, 0.96f), 0.46f)) - 0.035f,
            1f, new Vector4(32, 32, 32, 32), 96);

        public static Sprite ForItem(Core.ItemType t)
        {
            switch (t)
            {
                case Core.ItemType.Key: return Key;
                case Core.ItemType.Gem: return Gem;
                case Core.ItemType.PuzzlePiece: return Piece;
                case Core.ItemType.Relic: return Relic;
                default: return Star;
            }
        }

        public static Color ItemColor(Core.ItemType t)
        {
            switch (t)
            {
                case Core.ItemType.Key: return Theme.Gold;
                case Core.ItemType.Gem: return Theme.Pink;
                case Core.ItemType.PuzzlePiece: return Theme.Green;
                case Core.ItemType.Relic: return Theme.Violet;
                default: return Color.white;
            }
        }

        public static string ItemName(Core.ItemType t)
        {
            switch (t)
            {
                case Core.ItemType.PuzzlePiece: return "PUZZLE PIECE";
                default: return t.ToString().ToUpperInvariant();
            }
        }
    }
}
