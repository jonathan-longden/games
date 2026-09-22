using System;
using System.Collections.Generic;
using UnityEngine;

namespace PuzzleGame
{
    public enum Ease
    {
        Linear,
        OutQuad,
        OutCubic,
        InQuad,
        InOutSine,
        OutBack,
        OutElastic,
    }

    /// <summary>
    /// Tiny tween runner (no external dependency). Uses unscaled time so UI keeps
    /// animating while gameplay is paused.
    /// </summary>
    public sealed class Tween : MonoBehaviour
    {
        sealed class Item
        {
            public object Owner;
            public float Delay;
            public float Duration;
            public float Elapsed;
            public Ease Ease;
            public Action<float> Update;
            public Action Done;
            public bool Dead;
        }

        static Tween _instance;
        readonly List<Item> _items = new List<Item>();
        readonly List<Item> _adding = new List<Item>();

        static Tween Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("Tween");
                    DontDestroyOnLoad(go);
                    _instance = go.AddComponent<Tween>();
                }
                return _instance;
            }
        }

        /// <summary>Runs <paramref name="update"/> with an eased 0..1 value.</summary>
        public static void Run(object owner, float duration, Action<float> update, Ease ease = Ease.OutCubic,
            Action done = null, float delay = 0f)
        {
            var it = new Item
            {
                Owner = owner, Duration = Mathf.Max(0.0001f, duration), Ease = ease,
                Update = update, Done = done, Delay = delay,
            };
            Instance._adding.Add(it);
        }

        public static void Delay(object owner, float seconds, Action done) =>
            Run(owner, 0.0001f, null, Ease.Linear, done, seconds);

        /// <summary>Stops all tweens of an owner. If <paramref name="complete"/>, jumps them to their end.</summary>
        public static void Kill(object owner, bool complete = false)
        {
            if (_instance == null || owner == null) return;
            foreach (var list in new[] { _instance._items, _instance._adding })
            {
                foreach (var it in list)
                {
                    if (it.Dead || !ReferenceEquals(it.Owner, owner)) continue;
                    it.Dead = true;
                    if (complete)
                    {
                        SafeInvoke(it.Update, 1f, it.Owner);
                        SafeInvoke(it.Done, it.Owner);
                    }
                }
            }
        }

        void Update()
        {
            if (_adding.Count > 0)
            {
                _items.AddRange(_adding);
                _adding.Clear();
            }
            float dt = Time.unscaledDeltaTime;
            for (int i = 0; i < _items.Count; i++)
            {
                var it = _items[i];
                if (it.Dead) continue;
                if (it.Owner is UnityEngine.Object uo && uo == null) { it.Dead = true; continue; }
                if (it.Delay > 0f) { it.Delay -= dt; continue; }
                it.Elapsed += dt;
                float t = Mathf.Clamp01(it.Elapsed / it.Duration);
                SafeInvoke(it.Update, Evaluate(it.Ease, t), it.Owner);
                if (t >= 1f)
                {
                    it.Dead = true;
                    SafeInvoke(it.Done, it.Owner);
                }
            }
            _items.RemoveAll(x => x.Dead);
        }

        static void SafeInvoke(Action<float> a, float v, object owner)
        {
            if (a == null) return;
            if (owner is UnityEngine.Object uo && uo == null) return;
            try { a(v); } catch (Exception e) { Debug.LogException(e); }
        }

        static void SafeInvoke(Action a, object owner)
        {
            if (a == null) return;
            if (owner is UnityEngine.Object uo && uo == null) return;
            try { a(); } catch (Exception e) { Debug.LogException(e); }
        }

        public static float Evaluate(Ease e, float t)
        {
            switch (e)
            {
                case Ease.OutQuad: return 1f - (1f - t) * (1f - t);
                case Ease.OutCubic: { float u = 1f - t; return 1f - u * u * u; }
                case Ease.InQuad: return t * t;
                case Ease.InOutSine: return -(Mathf.Cos(Mathf.PI * t) - 1f) * 0.5f;
                case Ease.OutBack:
                {
                    const float c1 = 1.70158f, c3 = c1 + 1f;
                    float u = t - 1f;
                    return 1f + c3 * u * u * u + c1 * u * u;
                }
                case Ease.OutElastic:
                {
                    if (t <= 0f || t >= 1f) return t;
                    const float c4 = 2f * Mathf.PI / 3f;
                    return Mathf.Pow(2f, -10f * t) * Mathf.Sin((t * 10f - 0.75f) * c4) + 1f;
                }
                default: return t;
            }
        }
    }
}
