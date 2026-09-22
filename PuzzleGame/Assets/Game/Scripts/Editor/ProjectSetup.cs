using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace PuzzleGame.EditorTools
{
    /// <summary>
    /// One-time project configuration when the project is opened: portrait
    /// orientation, product name, and a Main scene registered in Build Settings.
    /// The game itself bootstraps from code, so the scene can stay empty.
    /// Safe to run repeatedly; it only fills in what is missing.
    /// </summary>
    [InitializeOnLoad]
    public static class ProjectSetup
    {
        public const string ScenePath = "Assets/Scenes/Main.unity";
        const string SessionKey = "PuzzleGame.ProjectSetup.Done";

        static ProjectSetup()
        {
            if (SessionState.GetBool(SessionKey, false)) return;
            EditorApplication.delayCall += Run;
        }

        [MenuItem("Puzzle Game/Re-run Project Setup", priority = 60)]
        public static void RunFromMenu()
        {
            SessionState.SetBool(SessionKey, false);
            Run();
        }

        static void Run()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            SessionState.SetBool(SessionKey, true);

            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.allowedAutorotateToLandscapeLeft = false;
            PlayerSettings.allowedAutorotateToLandscapeRight = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            if (string.IsNullOrEmpty(PlayerSettings.productName) || PlayerSettings.productName == "PuzzleGame")
                PlayerSettings.productName = "Echo Ascent";
            if (PlayerSettings.companyName == "DefaultCompany")
                PlayerSettings.companyName = "EchoAscentTeam";

            EnsureScene();
        }

        static void EnsureScene()
        {
            if (!File.Exists(ScenePath))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(ScenePath) ?? "Assets/Scenes");
                var active = EditorSceneManager.GetActiveScene();
                bool untitled = string.IsNullOrEmpty(active.path);
                if (untitled && !active.isDirty)
                {
                    // Fresh project: replace the empty untitled scene with ours and keep it open.
                    var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
                    EditorSceneManager.SaveScene(scene, ScenePath);
                }
                else if (!untitled)
                {
                    var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Additive);
                    EditorSceneManager.SaveScene(scene, ScenePath);
                    EditorSceneManager.CloseScene(scene, true);
                }
                else
                {
                    Debug.Log("[Setup] Save your current scene, then run Puzzle Game > Re-run Project Setup to create " + ScenePath);
                    return;
                }
                AssetDatabase.Refresh();
                Debug.Log("[Setup] Created " + ScenePath + ". Press Play to start the game.");
            }

            var scenes = EditorBuildSettings.scenes.ToList();
            if (!scenes.Any(s => s.path == ScenePath))
            {
                scenes.Insert(0, new EditorBuildSettingsScene(ScenePath, true));
                EditorBuildSettings.scenes = scenes.ToArray();
            }

            var current = EditorSceneManager.GetActiveScene();
            if (string.IsNullOrEmpty(current.path) && !current.isDirty)
                EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        }
    }
}
