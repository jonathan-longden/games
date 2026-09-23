using System;
using System.Collections.Generic;
using UnityEngine;

namespace PuzzleGame
{
    public enum Sfx
    {
        Move,
        Push,
        Bump,
        SwitchOn,
        SwitchOff,
        DoorOpen,
        DoorClose,
        Pickup,
        Item,
        Echo,
        Undo,
        Reset,
        Win,
        Star,
        Coin,
        Click,
        Unlock,
        Secret,
    }

    /// <summary>
    /// All sounds are synthesised at startup (no audio assets needed): short,
    /// soft, bell-like tones that suit a calm thinking game.
    /// </summary>
    public sealed class AudioManager : MonoBehaviour
    {
        const int Rate = 44100;
        readonly Dictionary<Sfx, AudioClip> _clips = new Dictionary<Sfx, AudioClip>();
        readonly List<AudioSource> _sources = new List<AudioSource>();
        int _next;
        float _lastMoveTime;

        public bool Muted { get; set; }

        void Awake()
        {
            for (int i = 0; i < 10; i++)
            {
                var s = gameObject.AddComponent<AudioSource>();
                s.playOnAwake = false;
                s.spatialBlend = 0f;
                _sources.Add(s);
            }
            Build();
        }

        public void Play(Sfx sfx, float volume = 1f, float pitch = 1f)
        {
            if (Muted) return;
            if (!_clips.TryGetValue(sfx, out var clip)) return;
            if (sfx == Sfx.Move)
            {
                // Rapid inputs: keep steps from stacking into noise.
                if (Time.unscaledTime - _lastMoveTime < 0.03f) return;
                _lastMoveTime = Time.unscaledTime;
                pitch *= UnityEngine.Random.Range(0.94f, 1.06f);
            }
            var src = _sources[_next];
            _next = (_next + 1) % _sources.Count;
            src.pitch = pitch;
            src.volume = volume;
            src.clip = clip;
            src.Play();
        }

        // ------------------------------------------------------------ synthesis

        void Build()
        {
            _clips[Sfx.Move] = Make("move", 0.07f, t => Tone(t, 330, 0.045f) * 0.35f + Noise(t, 0.02f) * 0.08f);
            _clips[Sfx.Push] = Make("push", 0.16f, t => Tone(t, 110, 0.12f, 0.8f) * 0.6f + Noise(t, 0.07f) * 0.25f);
            _clips[Sfx.Bump] = Make("bump", 0.1f, t => Tone(t, 82, 0.07f) * 0.5f);
            _clips[Sfx.SwitchOn] = Make("switchOn", 0.3f, t => Bell(t, 660, 0.25f) * 0.4f + Bell(t - 0.07f, 990, 0.22f) * 0.4f);
            _clips[Sfx.SwitchOff] = Make("switchOff", 0.25f, t => Bell(t, 740, 0.2f) * 0.35f + Bell(t - 0.06f, 494, 0.18f) * 0.35f);
            _clips[Sfx.DoorOpen] = Make("doorOpen", 0.35f, t => Sweep(t, 200, 520, 0.35f) * 0.35f + Noise(t, 0.2f) * 0.05f);
            _clips[Sfx.DoorClose] = Make("doorClose", 0.3f, t => Sweep(t, 420, 160, 0.3f) * 0.35f);
            _clips[Sfx.Pickup] = Make("pickup", 0.5f, t => Bell(t, 1319, 0.3f) * 0.3f + Bell(t - 0.06f, 1760, 0.3f) * 0.3f + Bell(t - 0.12f, 2349, 0.35f) * 0.25f);
            _clips[Sfx.Item] = Make("item", 0.9f, t => Bell(t, 784, 0.5f) * 0.3f + Bell(t - 0.1f, 988, 0.5f) * 0.3f + Bell(t - 0.2f, 1175, 0.5f) * 0.3f + Bell(t - 0.3f, 1568, 0.6f) * 0.3f);
            _clips[Sfx.Echo] = Make("echo", 0.45f, t => Shimmer(t, 523, 0.4f) * 0.35f);
            _clips[Sfx.Undo] = Make("undo", 0.1f, t => Sweep(t, 620, 380, 0.09f) * 0.3f);
            _clips[Sfx.Reset] = Make("reset", 0.3f, t => Sweep(t, 700, 220, 0.28f) * 0.3f);
            _clips[Sfx.Win] = Make("win", 1.1f, t => Bell(t, 523, 0.7f) * 0.25f + Bell(t - 0.09f, 659, 0.7f) * 0.25f + Bell(t - 0.18f, 784, 0.7f) * 0.25f + Bell(t - 0.3f, 1047, 0.8f) * 0.3f);
            _clips[Sfx.Star] = Make("star", 0.6f, t => Bell(t, 1319, 0.5f) * 0.35f + Bell(t, 1976, 0.3f) * 0.12f);
            _clips[Sfx.Coin] = Make("coin", 0.25f, t => Bell(t, 1568, 0.12f) * 0.25f + Bell(t - 0.05f, 2093, 0.18f) * 0.25f);
            _clips[Sfx.Click] = Make("click", 0.05f, t => Tone(t, 900, 0.03f) * 0.25f);
            // Rising, slightly unresolved: "something hidden just opened".
            _clips[Sfx.Secret] = Make("secret", 1.4f, t => Shimmer(t, 392, 1.1f) * 0.25f + Bell(t - 0.25f, 587, 0.9f) * 0.2f
                + Bell(t - 0.5f, 831, 0.9f) * 0.2f + Bell(t - 0.75f, 1109, 1.0f) * 0.22f);
            _clips[Sfx.Unlock] = Make("unlock", 0.9f, t => Bell(t, 392, 0.8f) * 0.25f + Bell(t, 587, 0.8f) * 0.2f + Bell(t - 0.12f, 784, 0.7f) * 0.25f);
        }

        static AudioClip Make(string name, float seconds, Func<float, float> f)
        {
            int n = Mathf.CeilToInt(seconds * Rate);
            var data = new float[n];
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)Rate;
                float fade = Mathf.Clamp01((seconds - t) / 0.01f); // avoid clicks at the end
                data[i] = Mathf.Clamp(f(t) * fade, -1f, 1f);
            }
            var clip = AudioClip.Create(name, n, 1, Rate, false);
            clip.SetData(data, 0);
            return clip;
        }

        static float Env(float t, float decay, float attack = 0.004f)
        {
            if (t < 0f) return 0f;
            float a = Mathf.Clamp01(t / attack);
            return a * Mathf.Exp(-t / Mathf.Max(0.001f, decay) * 3f);
        }

        static float Tone(float t, float freq, float decay, float drop = 0f)
        {
            if (t < 0f) return 0f;
            float f = freq * (1f - drop * Mathf.Clamp01(t / (decay * 2f)) * 0.5f);
            return Mathf.Sin(2f * Mathf.PI * f * t) * Env(t, decay);
        }

        static float Bell(float t, float freq, float decay)
        {
            if (t < 0f) return 0f;
            float e = Env(t, decay, 0.002f);
            return (Mathf.Sin(2f * Mathf.PI * freq * t) + 0.3f * Mathf.Sin(2f * Mathf.PI * freq * 2.01f * t) * Mathf.Exp(-t * 12f)) * e;
        }

        static float Sweep(float t, float f0, float f1, float dur)
        {
            if (t < 0f) return 0f;
            float k = Mathf.Clamp01(t / dur);
            // integrate frequency for a smooth glide
            float phase = 2f * Mathf.PI * (f0 * t + (f1 - f0) * t * k * 0.5f);
            return Mathf.Sin(phase) * Env(t, dur * 0.8f, 0.01f);
        }

        static float Shimmer(float t, float freq, float decay)
        {
            float e = Env(t, decay, 0.03f);
            float v = Mathf.Sin(2f * Mathf.PI * freq * t) + Mathf.Sin(2f * Mathf.PI * freq * 1.5f * t + Mathf.Sin(t * 30f)) * 0.6f
                      + Mathf.Sin(2f * Mathf.PI * freq * 2.003f * t) * 0.3f;
            return v * e * 0.5f;
        }

        static readonly System.Random Rng = new System.Random(3);

        static float Noise(float t, float decay)
        {
            return ((float)Rng.NextDouble() * 2f - 1f) * Env(t, decay, 0.001f);
        }
    }
}
