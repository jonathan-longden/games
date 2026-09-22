using System.Collections.Generic;
using UnityEngine;

namespace PuzzleGame
{
    /// <summary>Lightweight pooled sprite particles for world-space effects.</summary>
    public sealed class Fx : MonoBehaviour
    {
        sealed class Particle
        {
            public Transform T;
            public SpriteRenderer R;
            public Vector3 Vel;
            public float Life, MaxLife, Size0, Size1, Gravity, Drag;
            public Color Color;
            public bool Active;
        }

        readonly List<Particle> _pool = new List<Particle>();
        public const int SortingOrder = 40;

        Particle Get()
        {
            foreach (var p in _pool)
                if (!p.Active) return p;
            var go = new GameObject("p");
            go.transform.SetParent(transform, false);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sortingOrder = SortingOrder;
            var np = new Particle { T = go.transform, R = sr };
            _pool.Add(np);
            return np;
        }

        void Spawn(Vector3 pos, Vector3 vel, Color c, float size0, float size1, float life, Sprite sprite, float gravity = 0f, float drag = 2f)
        {
            var p = Get();
            p.Active = true;
            p.T.gameObject.SetActive(true);
            p.T.position = pos;
            p.T.localScale = Vector3.one * size0;
            p.T.rotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));
            p.R.sprite = sprite;
            p.R.color = c;
            p.Vel = vel;
            p.Life = 0;
            p.MaxLife = life;
            p.Size0 = size0;
            p.Size1 = size1;
            p.Gravity = gravity;
            p.Drag = drag;
            p.Color = c;
        }

        public void Burst(Vector3 pos, Color c, int count = 12, float speed = 3f, float size = 0.18f, float life = 0.5f)
        {
            for (int i = 0; i < count; i++)
            {
                float a = Random.Range(0f, Mathf.PI * 2f);
                float s = speed * Random.Range(0.4f, 1f);
                var v = new Vector3(Mathf.Cos(a), Mathf.Sin(a)) * s;
                Spawn(pos, v, c, size * Random.Range(0.6f, 1.2f), 0f, life * Random.Range(0.7f, 1.2f),
                    i % 3 == 0 ? SpriteFactory.Star : SpriteFactory.Glow, 0f, 3f);
            }
        }

        public void Dust(Vector3 pos, Vector2 dir, Color c)
        {
            for (int i = 0; i < 4; i++)
            {
                var side = new Vector3(-dir.y, dir.x) * Random.Range(-1f, 1f);
                var v = (new Vector3(-dir.x, -dir.y) * 0.8f + side) * Random.Range(0.5f, 1.2f);
                Spawn(pos, v, c, 0.22f, 0.02f, 0.35f, SpriteFactory.Glow, 0f, 4f);
            }
        }

        public void Ring(Vector3 pos, Color c, float size = 1.6f, float life = 0.45f)
        {
            Spawn(pos, Vector3.zero, c, 0.3f, size, life, SpriteFactory.RingThin, 0f, 0f);
        }

        public void Sparkle(Vector3 pos, Color c, float spread = 0.4f)
        {
            var off = new Vector3(Random.Range(-spread, spread), Random.Range(-spread, spread));
            Spawn(pos + off, new Vector3(0, Random.Range(0.3f, 0.8f)), c, Random.Range(0.08f, 0.16f), 0f,
                Random.Range(0.6f, 1.1f), SpriteFactory.Glow, 0f, 0.5f);
        }

        public void Clear()
        {
            foreach (var p in _pool)
            {
                p.Active = false;
                p.T.gameObject.SetActive(false);
            }
        }

        void Update()
        {
            float dt = Time.unscaledDeltaTime;
            foreach (var p in _pool)
            {
                if (!p.Active) continue;
                p.Life += dt;
                float t = p.Life / p.MaxLife;
                if (t >= 1f)
                {
                    p.Active = false;
                    p.T.gameObject.SetActive(false);
                    continue;
                }
                p.Vel += Vector3.down * (p.Gravity * dt);
                p.Vel *= Mathf.Max(0f, 1f - p.Drag * dt);
                p.T.position += p.Vel * dt;
                p.T.localScale = Vector3.one * Mathf.Lerp(p.Size0, p.Size1, t);
                var c = p.Color;
                c.a *= 1f - t * t;
                p.R.color = c;
            }
        }
    }

    /// <summary>
    /// The atmospheric sky: a gradient that always fills the camera plus slow
    /// drifting motes. Rendered behind everything, visible through the UI.
    /// </summary>
    public sealed class Background : MonoBehaviour
    {
        Camera _cam;
        SpriteRenderer _gradient;
        readonly List<(Transform t, SpriteRenderer r, float speed, float phase, float depth)> _motes =
            new List<(Transform, SpriteRenderer, float, float, float)>();
        SpriteRenderer _haloA, _haloB;

        public void Init(Camera cam)
        {
            _cam = cam;
            var g = new GameObject("Sky");
            g.transform.SetParent(transform, false);
            _gradient = g.AddComponent<SpriteRenderer>();
            _gradient.sprite = SpriteFactory.Gradient(Theme.BgTop, Theme.BgBottom);
            _gradient.sortingOrder = -100;

            _haloA = MakeHalo("HaloA", Theme.Violet.WithAlpha(0.10f));
            _haloB = MakeHalo("HaloB", Theme.Cyan.WithAlpha(0.07f));

            var rnd = new System.Random(7);
            for (int i = 0; i < 46; i++)
            {
                var m = new GameObject("Mote");
                m.transform.SetParent(transform, false);
                var r = m.AddComponent<SpriteRenderer>();
                r.sprite = SpriteFactory.Glow;
                r.sortingOrder = -90;
                float depth = (float)rnd.NextDouble();
                var col = i % 5 == 0 ? Theme.Violet : (i % 3 == 0 ? Theme.Cyan : Color.white);
                r.color = col.WithAlpha(0.12f + 0.3f * depth);
                m.transform.localScale = Vector3.one * (0.06f + 0.16f * depth);
                m.transform.localPosition = new Vector3((float)rnd.NextDouble(), (float)rnd.NextDouble(), 0);
                _motes.Add((m.transform, r, 0.01f + 0.03f * depth, (float)rnd.NextDouble() * 10f, depth));
            }
        }

        SpriteRenderer MakeHalo(string name, Color c)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            var r = go.AddComponent<SpriteRenderer>();
            r.sprite = SpriteFactory.Glow;
            r.color = c;
            r.sortingOrder = -95;
            return r;
        }

        void LateUpdate()
        {
            if (_cam == null) return;
            float h = _cam.orthographicSize * 2f;
            float w = h * _cam.aspect;
            var c = _cam.transform.position;
            transform.position = new Vector3(c.x, c.y, 10f);
            _gradient.transform.localScale = new Vector3(w * 1.05f, h * 1.05f / 256f, 1f);

            float time = Time.unscaledTime;
            _haloA.transform.localPosition = new Vector3(Mathf.Sin(time * 0.05f) * w * 0.25f, h * 0.28f, 0);
            _haloA.transform.localScale = Vector3.one * w * 1.4f;
            _haloB.transform.localPosition = new Vector3(Mathf.Cos(time * 0.04f) * w * 0.3f, -h * 0.3f, 0);
            _haloB.transform.localScale = Vector3.one * w * 1.2f;

            foreach (var m in _motes)
            {
                // Motes live in normalized screen space and wrap vertically.
                var p = m.t.localPosition;
                float ny = Mathf.Repeat(m.phase * 0.1f + time * m.speed, 1f);
                float nx = Mathf.Repeat(m.phase * 0.37f + Mathf.Sin(time * 0.2f + m.phase) * 0.02f, 1f);
                m.t.localPosition = new Vector3((nx - 0.5f) * w, (ny - 0.5f) * h, 0);
                var col = m.r.color;
                col.a = (0.1f + 0.3f * m.depth) * (0.6f + 0.4f * Mathf.Sin(time * 1.3f + m.phase * 3f));
                m.r.color = col;
            }
        }
    }
}
