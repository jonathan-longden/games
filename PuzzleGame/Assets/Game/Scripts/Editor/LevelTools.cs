using System.Diagnostics;
using System.Text;
using PuzzleGame.Core;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace PuzzleGame.EditorTools
{
    /// <summary>Menu commands for level data.</summary>
    public static class LevelTools
    {
        public const string LevelsPath = "Assets/Game/Resources/Levels/levels.txt";

        [MenuItem("Puzzle Game/Validate All Levels", priority = 20)]
        public static void ValidateAll()
        {
            var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(LevelsPath);
            if (asset == null)
            {
                Debug.LogError("Missing " + LevelsPath);
                return;
            }

            var sw = Stopwatch.StartNew();
            var levels = LevelParser.ParseAll(asset.text);
            var reports = LevelValidator.ValidateAll(levels);
            int failed = 0, warnings = 0;
            var sb = new StringBuilder();
            for (int i = 0; i < reports.Count; i++)
            {
                var r = reports[i];
                var l = levels[i];
                if (!r.Ok) failed++;
                warnings += r.Warnings.Count;
                sb.AppendLine($"{r}  target={l.MoveTarget}  mastery={l.MasteryDescription}");
                if (r.Ok) sb.AppendLine($"   solution: {r.OptimalPath}");
            }
            string summary = $"Validated {levels.Count} levels in {sw.ElapsedMilliseconds} ms: {failed} failing, {warnings} warnings.";
            if (failed > 0) Debug.LogError(summary + "\n" + sb);
            else Debug.Log(summary + "\n" + sb);
            EditorUtility.DisplayDialog("Level validation", summary + (failed > 0 ? "\n\nSee the Console for details." : ""), "OK");
        }

        [MenuItem("Puzzle Game/Open Save Folder", priority = 40)]
        public static void OpenSaveFolder() => EditorUtility.RevealInFinder(Application.persistentDataPath);
    }
}
