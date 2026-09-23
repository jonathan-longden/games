using PuzzleGame.Core;
using UnityEngine;

namespace PuzzleGame
{
    public enum GameState
    {
        Boot,
        MainMenu,
        Map,
        Playing,
        Paused,
        LevelComplete,
    }

    /// <summary>
    /// Composition root and state machine. Creates every manager, wires them
    /// together and moves between Main Menu, Map, Playing, Paused and Level Complete.
    /// Everything is built in code, so the game runs from any (even empty) scene.
    /// </summary>
    public sealed class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public GameState State { get; private set; } = GameState.Boot;

        // Data / rules
        public SaveManager Save { get; private set; }
        public LevelDatabase Levels { get; private set; }
        public CurrencyManager Currency { get; private set; }
        public ProgressManager Progress { get; private set; }
        public RewardManager Rewards { get; private set; }
        public LevelManager LevelManager { get; private set; }

        // Scene objects
        public Camera Camera { get; private set; }
        public AudioManager Audio { get; private set; }
        public UIManager UI { get; private set; }
        public GridManager Grid { get; private set; }
        public PuzzleManager Puzzle { get; private set; }
        public PlayerController Input { get; private set; }
        public Fx Fx { get; private set; }

        public CompletionResult LastResult { get; private set; }
        public bool SoundOn => Save.Data.settings.sound;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() => Instance = null; // supports "Enter Play Mode" without domain reload

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            Application.targetFrameRate = 60;
            Screen.orientation = ScreenOrientation.Portrait;
            UnityEngine.Input.multiTouchEnabled = false;
            Init();
        }

        void Init()
        {
            Save = new SaveManager();
            Save.Load();
            Levels = LevelDatabase.LoadFromResources();
            Currency = new CurrencyManager(Save);
            Progress = new ProgressManager(Save, Levels);
            Rewards = new RewardManager(Progress, Currency, Save);

            Camera = SetupCamera();
            var bg = new GameObject("Background").AddComponent<Background>();
            bg.transform.SetParent(transform, false);
            bg.Init(Camera);

            Fx = Child<Fx>("Fx");
            Audio = Child<AudioManager>("Audio");
            Audio.Muted = !SoundOn;

            Grid = Child<GridManager>("Grid");
            Grid.Cam = Camera;
            Grid.Fx = Fx;
            Grid.Audio = Audio;

            Puzzle = Child<PuzzleManager>("Puzzle");
            Puzzle.Grid = Grid;
            Puzzle.Audio = Audio;
            Puzzle.Completed += OnPuzzleCompleted;

            Input = Child<PlayerController>("Input");
            Input.Puzzle = Puzzle;
            Input.PauseRequested = TogglePause;

            UI = Child<UIManager>("UIRoot");
            UI.Build(this);
            Grid.BoardArea = UI.BoardScreenRect;

            LevelManager = new LevelManager(this);
            Save.SaveIfDirty();
            OpenMainMenu();
        }

        T Child<T>(string name) where T : Component
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            return go.AddComponent<T>();
        }

        Camera SetupCamera()
        {
            var cam = Camera.main;
            if (cam == null)
            {
                var go = new GameObject("Main Camera") { tag = "MainCamera" };
                cam = go.AddComponent<Camera>();
            }
            cam.orthographic = true;
            cam.orthographicSize = 8f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Theme.BgBottom;
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 50f;
            cam.transform.position = new Vector3(0, 0, -10);
            cam.transform.rotation = Quaternion.identity;
            if (cam.GetComponent<AudioListener>() == null && FindObjectOfType<AudioListener>() == null)
                cam.gameObject.AddComponent<AudioListener>();
            DontDestroyOnLoad(cam.gameObject);
            return cam;
        }

        // ------------------------------------------------------------ navigation

        public void OpenMainMenu()
        {
            LevelManager.Unload();
            State = GameState.MainMenu;
            UI.ShowMainMenu();
            Save.SaveIfDirty();
        }

        public void OpenMap(LevelData focus = null)
        {
            LevelManager.Unload();
            State = GameState.Map;
            UI.ShowMap(focus ?? LevelManager.Current);
            Save.SaveIfDirty();
        }

        public void PlayLevel(LevelData level)
        {
            if (level == null || !Progress.IsUnlocked(level)) return;
            State = GameState.Playing;
            LevelManager.Load(level);
        }

        public void RestartLevel()
        {
            if (LevelManager.Current != null) PlayLevel(LevelManager.Current);
        }

        public void PlayNext()
        {
            var next = Progress.NextLevel(LevelManager.Current);
            if (next != null) PlayLevel(next);
            else OpenMap();
        }

        public void Pause()
        {
            if (State != GameState.Playing) return;
            State = GameState.Paused;
            Puzzle.SetInputLocked(true);
            UI.ShowPause(true);
        }

        public void Resume()
        {
            if (State != GameState.Paused) return;
            State = GameState.Playing;
            Puzzle.SetInputLocked(false);
            UI.ShowPause(false);
        }

        public void TogglePause()
        {
            if (State == GameState.Playing) Pause();
            else if (State == GameState.Paused) Resume();
        }

        public void ToggleSound()
        {
            Save.Data.settings.sound = !Save.Data.settings.sound;
            Audio.Muted = !SoundOn;
            Save.Save();
        }

        void OnPuzzleCompleted(RunStats run)
        {
            var level = LevelManager.Current;
            if (level == null) return;
            ShowCompletion(Rewards.GrantCompletion(level, run));
        }

        void ShowCompletion(CompletionResult result)
        {
            LastResult = result;
            Progress.SetCurrent(result.Level);
            State = GameState.LevelComplete;
            Puzzle.SetInputLocked(true);
            UI.ShowComplete(result, Progress.NextLevel(result.Level) != null);
        }

        void OnApplicationPause(bool paused)
        {
            if (paused) Save?.SaveIfDirty();
        }

        void OnApplicationQuit()
        {
            Save?.SaveIfDirty();
        }

#if UNITY_EDITOR
        // ------------------------------------------------------------ development only
        // Called by the editor Dev Panel (Assets/Game/Scripts/Editor). Compiled out of builds.

        public void DevJumpToLevel(string id)
        {
            var l = Levels.Get(id);
            if (l == null) { Debug.LogWarning($"[Dev] No level '{id}'"); return; }
            if (!Save.Data.IsUnlocked(l.Id)) Save.Data.unlockedLevels.Add(l.Id);
            UI.HideAll();
            PlayLevel(l);
        }

        public void DevResetProgress()
        {
            Save.ResetAll();
            Progress = new ProgressManager(Save, Levels);
            Rewards = new RewardManager(Progress, Currency, Save);
            Save.Save();
            OpenMainMenu();
            Debug.Log("[Dev] Progress reset.");
        }

        public void DevGiveCoins(int amount)
        {
            Currency.Add(amount);
            Save.Save();
            RefreshScreen();
        }

        public void DevUnlockAllLevels()
        {
            Progress.DevUnlockAll();
            Save.Save();
            RefreshScreen();
        }

        public void DevUnlockAllItems()
        {
            Progress.DevUnlockAllItems();
            Save.Save();
            RefreshScreen();
        }

        /// <summary>Completes the level being played with the given number of stars.</summary>
        public void DevCompleteLevel(int stars)
        {
            var level = LevelManager.Current;
            if (level == null || (State != GameState.Playing && State != GameState.Paused)) { Debug.LogWarning("[Dev] Not playing a level."); return; }
            int mask = stars >= 3 ? Stars.All : stars == 2 ? Stars.Complete | Stars.Target : Stars.Complete;
            var run = new RunStats
            {
                Moves = level.MoveTarget,
                CrystalCollected = stars >= 3 && level.CrystalIndex >= 0,
                ItemCollected = level.SpecialItemIndex >= 0,
            };
            UI.ShowPause(false);
            ShowCompletion(Rewards.GrantCompletion(level, run, mask));
        }

        public void DevSetStars(string id, int stars)
        {
            var l = Levels.Get(id);
            if (l == null) return;
            Progress.DevSetStars(l, stars);
            Save.Save();
            RefreshScreen();
        }

        public void DevReloadLevel()
        {
            if (LevelManager.Current != null) { UI.HideAll(); PlayLevel(LevelManager.Current); }
        }

        public bool DevAutoSolve(bool withCrystal) => State == GameState.Playing && Puzzle.DevAutoSolve(withCrystal);

        void RefreshScreen()
        {
            if (State == GameState.Map) UI.ShowMap(null);
            else if (State == GameState.MainMenu) UI.ShowMainMenu();
        }
#endif
    }

    /// <summary>Loads and unloads the level being played (HUD + board + puzzle).</summary>
    public sealed class LevelManager
    {
        readonly GameManager _game;

        public LevelData Current { get; private set; }

        public LevelManager(GameManager game) { _game = game; }

        public void Load(LevelData level)
        {
            Current = level;
            _game.UI.ShowHud(level);
            Canvas.ForceUpdateCanvases(); // HUD rects must exist before the camera fits the board
            _game.Puzzle.Begin(level);
            _game.Grid.FitCamera();
            if (!_game.Progress.IsCompleted(level)) IntroduceNewMechanic(level);
        }

        /// <summary>
        /// Teach through the level, not a popup: the first time a mechanic appears,
        /// the new objects are ringed a few times while the one-line hint shows.
        /// </summary>
        void IntroduceNewMechanic(LevelData level)
        {
            var fresh = _game.Levels.NewMechanics(level);
            if ((fresh & Mechanics.Echo) != 0)
            {
                var cells = new System.Collections.Generic.List<GridPos>(level.Runes);
                cells.AddRange(level.EchoBlocks);
                _game.Grid.Spotlight(cells, Theme.Violet);
            }
            else if ((fresh & Mechanics.Switches) != 0)
            {
                var cells = new System.Collections.Generic.List<GridPos>();
                foreach (var s in level.Switches) cells.Add(s.Pos);
                foreach (var d in level.Doors) cells.Add(d.Pos);
                _game.Grid.Spotlight(cells, Theme.Cyan);
            }
            else if ((fresh & Mechanics.Blocks) != 0)
            {
                _game.Grid.Spotlight(level.Blocks, Theme.StoneLight);
            }
        }

        public void Unload()
        {
            _game.Puzzle.End();
        }
    }

    /// <summary>Starts the game from any scene: no scene setup or prefabs required.</summary>
    public static class Bootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            if (GameManager.Instance != null) return;
            new GameObject("Game").AddComponent<GameManager>();
        }
    }
}
