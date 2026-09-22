using System;
using System.IO;
using PuzzleGame.Core;
using UnityEngine;

namespace PuzzleGame
{
    /// <summary>
    /// Loads and saves <see cref="SaveData"/> as JSON in persistentDataPath.
    /// Writes go to a temp file first and the previous save is kept as a backup,
    /// so a crash mid-write can never destroy progress.
    /// </summary>
    public sealed class SaveManager
    {
        const string FileName = "save.json";
        const string BackupName = "save.bak";

        public SaveData Data { get; private set; }
        public bool Dirty { get; private set; }

        readonly string _dir;
        string MainPath => Path.Combine(_dir, FileName);
        string BackupPath => Path.Combine(_dir, BackupName);
        string TempPath => Path.Combine(_dir, FileName + ".tmp");

        public SaveManager(string directory = null)
        {
            _dir = directory ?? Application.persistentDataPath;
        }

        public void Load()
        {
            Data = TryRead(MainPath) ?? TryRead(BackupPath) ?? new SaveData();
            Dirty = false;
        }

        SaveData TryRead(string path)
        {
            try
            {
                if (!File.Exists(path)) return null;
                string json = File.ReadAllText(path);
                if (string.IsNullOrWhiteSpace(json)) return null;
                var data = JsonUtility.FromJson<SaveData>(json);
                if (data == null) return null;
                if (!SaveMigrations.Migrate(data))
                {
                    Debug.LogWarning($"[Save] {path} was written by a newer version ({data.version}); loading what we can.");
                    data.version = SaveData.CurrentVersion;
                }
                return data;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Save] Could not read {path}: {e.Message}");
                return null;
            }
        }

        public void MarkDirty() => Dirty = true;

        public void Save()
        {
            if (Data == null) return;
            try
            {
                Directory.CreateDirectory(_dir);
                Data.version = SaveData.CurrentVersion;
                Data.lastSavedUtc = DateTime.UtcNow.ToString("o");
                string json = JsonUtility.ToJson(Data, true);
                File.WriteAllText(TempPath, json);
                if (File.Exists(MainPath)) File.Copy(MainPath, BackupPath, true);
                if (File.Exists(MainPath)) File.Delete(MainPath);
                File.Move(TempPath, MainPath);
                Dirty = false;
            }
            catch (Exception e)
            {
                Debug.LogError($"[Save] Failed to save: {e}");
            }
        }

        public void SaveIfDirty()
        {
            if (Dirty) Save();
        }

        /// <summary>Wipes all progress (dev tools / settings).</summary>
        public void ResetAll()
        {
            var sound = Data?.settings?.sound ?? true;
            Data = new SaveData();
            Data.settings.sound = sound;
            Save();
        }
    }
}
