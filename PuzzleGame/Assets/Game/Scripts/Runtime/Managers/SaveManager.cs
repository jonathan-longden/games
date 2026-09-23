using System;
using System.IO;
using PuzzleGame.Core;
using UnityEngine;

namespace PuzzleGame
{
    /// <summary>
    /// Persists <see cref="SaveData"/> as JSON in persistentDataPath. The policy
    /// (main -> backup -> fresh, temp-file writes, repair of damaged data) lives in
    /// Core's <see cref="SaveStore"/> and is unit-tested; this class only supplies
    /// JsonUtility and the real file system.
    /// </summary>
    public sealed class SaveManager
    {
        readonly SaveStore _store;

        public SaveData Data => _store.Data;
        public bool Dirty { get; private set; }

        public SaveManager(string directory = null)
        {
            _store = new SaveStore(new JsonCodec(), new DiskFiles(directory ?? Application.persistentDataPath));
        }

        public void Load()
        {
            _store.Load();
            if (_store.LoadedFrom != "main" && !string.IsNullOrEmpty(_store.LastError))
                Debug.LogWarning($"[Save] Loaded from {_store.LoadedFrom}: {_store.LastError}");
            Dirty = false;
        }

        public void MarkDirty() => Dirty = true;

        public void Save()
        {
            if (_store.Save(DateTime.UtcNow)) Dirty = false;
            else Debug.LogError("[Save] Failed to save: " + _store.LastError);
        }

        public void SaveIfDirty()
        {
            if (Dirty) Save();
        }

        /// <summary>Wipes all progress (dev tools), keeping the sound setting.</summary>
        public void ResetAll()
        {
            var sound = Data?.settings?.sound ?? true;
            var fresh = new SaveData();
            fresh.settings.sound = sound;
            _store.Replace(fresh);
            Save();
        }

        sealed class JsonCodec : ISaveCodec
        {
            public string Encode(SaveData data) => JsonUtility.ToJson(data, true);
            public SaveData Decode(string text) => JsonUtility.FromJson<SaveData>(text);
        }

        sealed class DiskFiles : ISaveFiles
        {
            readonly string _dir;
            public DiskFiles(string dir) { _dir = dir; }
            string P(string name) => Path.Combine(_dir, name);
            public bool Exists(string name) => File.Exists(P(name));
            public string Read(string name) => File.ReadAllText(P(name));
            public void Write(string name, string text)
            {
                Directory.CreateDirectory(_dir);
                File.WriteAllText(P(name), text);
            }
            public void Copy(string from, string to) => File.Copy(P(from), P(to), true);
            public void Delete(string name) => File.Delete(P(name));
            public void Move(string from, string to) => File.Move(P(from), P(to));
        }
    }
}
