using System.IO;
using System.Linq;
using PuzzleGame.Core;
using UnityEditor;
using UnityEngine;

namespace PuzzleGame.EditorTools
{
    /// <summary>
    /// Editor-only development panel (Window: Puzzle Game > Dev Panel).
    /// Everything here lives in an Editor assembly and calls runtime methods that
    /// are wrapped in #if UNITY_EDITOR, so none of it can reach a release build.
    /// </summary>
    public sealed class DevPanelWindow : EditorWindow
    {
        int _levelIndex;
        int _starsLevelIndex;
        int _stars = 3;
        int _coins = 1000;
        Vector2 _scroll;

        [MenuItem("Puzzle Game/Dev Panel %#d", priority = 0)]
        public static void Open()
        {
            var w = GetWindow<DevPanelWindow>("Puzzle Dev");
            w.minSize = new Vector2(300, 420);
        }

        string[] LevelIds()
        {
            var gm = GameManager.Instance;
            if (gm != null && gm.Levels != null) return gm.Levels.All.Select(l => l.Id).ToArray();
            var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(LevelTools.LevelsPath);
            if (asset == null) return new string[0];
            try { return LevelParser.ParseAll(asset.text).Select(l => l.Id).ToArray(); }
            catch { return new string[0]; }
        }

        void OnInspectorUpdate() => Repaint();

        void OnGUI()
        {
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            var gm = GameManager.Instance;
            bool playing = Application.isPlaying && gm != null;

            EditorGUILayout.LabelField("Echo Ascent - Development", EditorStyles.boldLabel);
            if (!playing)
            {
                EditorGUILayout.HelpBox("Enter Play Mode to use the runtime tools.\n(Works from any scene: the game bootstraps itself.)", MessageType.Info);
            }
            else
            {
                var lvl = gm.LevelManager.Current;
                EditorGUILayout.LabelField("State", gm.State.ToString());
                EditorGUILayout.LabelField("Level", lvl != null ? $"{lvl.Id}  {lvl.Title}" : "-");
                EditorGUILayout.LabelField("Coins", gm.Currency.Coins.ToString());
                EditorGUILayout.LabelField("Stars", $"{gm.Progress.TotalStars} / {gm.Progress.MaxStars}");
                if (gm.Puzzle.State != null && gm.State == GameState.Playing)
                    EditorGUILayout.LabelField("Moves", $"{gm.Puzzle.State.Moves} (undo stack {gm.Puzzle.UndoCount})");
            }

            var ids = LevelIds();
            using (new EditorGUI.DisabledScope(!playing))
            {
                Section("Levels");
                if (ids.Length > 0)
                {
                    _levelIndex = Mathf.Clamp(_levelIndex, 0, ids.Length - 1);
                    EditorGUILayout.BeginHorizontal();
                    _levelIndex = EditorGUILayout.Popup("Jump to level", _levelIndex, ids);
                    if (GUILayout.Button("Play", GUILayout.Width(60))) gm.DevJumpToLevel(ids[_levelIndex]);
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Reload Level")) gm.DevReloadLevel();
                if (GUILayout.Button("Auto-Solve")) gm.DevAutoSolve(false);
                if (GUILayout.Button("Auto-Solve + Crystal")) gm.DevAutoSolve(true);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Complete level:");
                if (GUILayout.Button("1 star")) gm.DevCompleteLevel(1);
                if (GUILayout.Button("2 stars")) gm.DevCompleteLevel(2);
                if (GUILayout.Button("3 stars")) gm.DevCompleteLevel(3);
                EditorGUILayout.EndHorizontal();

                if (ids.Length > 0)
                {
                    _starsLevelIndex = Mathf.Clamp(_starsLevelIndex, 0, ids.Length - 1);
                    EditorGUILayout.BeginHorizontal();
                    _starsLevelIndex = EditorGUILayout.Popup("Set stars", _starsLevelIndex, ids);
                    _stars = EditorGUILayout.IntSlider(_stars, 0, 3, GUILayout.Width(120));
                    if (GUILayout.Button("Apply", GUILayout.Width(60))) gm.DevSetStars(ids[_starsLevelIndex], _stars);
                    EditorGUILayout.EndHorizontal();
                }

                Section("Progress");
                EditorGUILayout.BeginHorizontal();
                _coins = EditorGUILayout.IntField("Give coins", _coins);
                if (GUILayout.Button("Give", GUILayout.Width(60))) gm.DevGiveCoins(_coins);
                EditorGUILayout.EndHorizontal();
                if (GUILayout.Button("Unlock All Levels")) gm.DevUnlockAllLevels();
                if (GUILayout.Button("Unlock All Items")) gm.DevUnlockAllItems();
                GUI.color = new Color(1f, 0.7f, 0.7f);
                if (GUILayout.Button("Reset Progress") &&
                    EditorUtility.DisplayDialog("Reset progress", "Erase all stars, coins, items and unlocks?", "Reset", "Cancel"))
                    gm.DevResetProgress();
                GUI.color = Color.white;
            }

            Section("Data");
            if (GUILayout.Button("Validate All Levels (solver)")) LevelTools.ValidateAll();
            using (new EditorGUI.DisabledScope(playing))
            {
                if (GUILayout.Button("Delete Save File (edit mode)")) DeleteSave();
            }
            if (GUILayout.Button("Open Save Folder")) EditorUtility.RevealInFinder(Application.persistentDataPath);

            EditorGUILayout.EndScrollView();
        }

        static void Section(string title)
        {
            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
        }

        static void DeleteSave()
        {
            foreach (var name in new[] { "save.json", "save.bak", "save.json.tmp" })
            {
                var p = Path.Combine(Application.persistentDataPath, name);
                if (File.Exists(p)) File.Delete(p);
            }
            Debug.Log("[Dev] Save files deleted from " + Application.persistentDataPath);
        }
    }
}
