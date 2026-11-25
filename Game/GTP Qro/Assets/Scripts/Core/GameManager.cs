using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace GTP
{
    /// <summary>
    /// Punto de coordinación global (Singleton persistente). Coordina estados, carga de escenas,
    /// pausa, intentos y Game Over. No contiene lógica de jugador, NPC, UI ni narrativa:
    /// cada sistema conserva su propia responsabilidad y se comunica por eventos.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        /// <summary>El jugador perdió un intento y debe volver al punto de control.</summary>
        public static event Action PlayerRespawned;
        public event Action<GameState> StateChanged;

        public GameConfig Config { get; private set; }
        public GameSession Session { get; private set; }
        public GameState State { get; private set; } = GameState.Menu;
        public int LastStateChangeFrame { get; private set; } = -1;
        public bool IsInvulnerable => Time.time < invulnerableUntil;
        public bool CanPlayerAct => State == GameState.Exploracion && Time.frameCount != LastStateChangeFrame;

        private float invulnerableUntil;
        private GameState stateBeforePause;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            if (Instance != null) return;
            var root = new GameObject("GameManager");
            root.AddComponent<GameManager>();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            Config = GameConfig.LoadOrDefault();
            NewSession();
            GameUI.Create(transform, Session);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy()
        {
            if (Instance != this) return;
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Instance = null;
        }

        private bool sceneHandled;

        private void Start()
        {
            // Respaldo: permite dar Play directamente en cualquier escena aunque sceneLoaded no se haya recibido.
            if (!sceneHandled)
                OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
        }

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame && Time.frameCount != LastStateChangeFrame)
            {
                if (State == GameState.Exploracion) Pause();
                else if (State == GameState.Pausa) Resume();
            }

            if (State == GameState.Exploracion)
                Session.DrainBattery(Config.batteryDrainPerMinute / 60f * Time.deltaTime);
        }

        // ---------------- Sesión ----------------
        private void NewSession()
        {
            if (Session != null)
                Session.BatteryDepleted -= OnBatteryDepleted;
            Session = new GameSession(Config.startLives, Config.startBattery);
            Session.BatteryDepleted += OnBatteryDepleted;
            GameUI.Instance?.BindSession(Session);
        }

        private void OnBatteryDepleted()
        {
            TriggerGameOver("Tu celular se apagó. Sin mapa ni forma de pedir ayuda, la noche te gana.");
        }

        // ---------------- Estados ----------------
        public void SetState(GameState newState)
        {
            if (State == newState) return;
            State = newState;
            LastStateChangeFrame = Time.frameCount;
            Time.timeScale = newState == GameState.Exploracion || newState == GameState.Menu || newState == GameState.Final ? 1f : 0f;
            StateChanged?.Invoke(newState);
        }

        public void Pause()
        {
            stateBeforePause = State;
            SetState(GameState.Pausa);
            GameUI.Instance.Pause.Show();
        }

        public void Resume()
        {
            GameUI.Instance.Pause.Hide();
            SetState(stateBeforePause == GameState.Pausa ? GameState.Exploracion : stateBeforePause);
        }

        // ---------------- Intentos / Game Over ----------------
        /// <summary>Una amenaza atrapó o detectó al jugador (cono, foco, asaltante, persecución).</summary>
        public void ReportPlayerCaught(string reason)
        {
            if (State != GameState.Exploracion || IsInvulnerable) return;

            Session.AddScore(-Config.detectionPenalty);
            bool hasLivesLeft = Session.LoseLife();
            if (!hasLivesLeft)
            {
                TriggerGameOver(reason);
                return;
            }

            invulnerableUntil = Time.time + Config.invulnerabilityAfterRespawn;
            GameUI.Instance.Hud.ShowThought(reason + "  (-1 intento)", 3f);
            PlayerRespawned?.Invoke();
        }

        public void TriggerGameOver(string reason)
        {
            if (State == GameState.GameOver || State == GameState.Final) return;
            SetState(GameState.GameOver);
            GameUI.Instance.GameOver.Show(reason, Session.Score);
        }

        // ---------------- Escenas ----------------
        public void StartNewGame()
        {
            NewSession();
            LoadLevel(SceneNames.Cuarto);
        }

        public void RetryLevel()
        {
            Session.RestoreLevelStart();
            LoadLevel(SceneManager.GetActiveScene().name, markLevelStart: false);
        }

        public void GoToMenu()
        {
            GameUI.Instance.HideAllOverlays();
            LoadLevel(SceneNames.Menu);
        }

        public void CompleteLevel(string nextScene)
        {
            Session.AddScore(Config.levelCompletePoints);
            LoadLevel(nextScene);
        }

        private bool pendingMarkLevelStart = true;

        public void LoadLevel(string sceneName, bool markLevelStart = true)
        {
            pendingMarkLevelStart = markLevelStart;
            GameUI.Instance.HideAllOverlays();
            GameUI.Instance.Hud.SetObjective("", null);
            Time.timeScale = 1f;
            SceneManager.LoadScene(sceneName);
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            sceneHandled = true;
            bool isMenu = scene.name == SceneNames.Menu || FindAnyObjectByType<PlayerMovement>() == null;
            GameUI.Instance.Hud.SetVisible(!isMenu);

            if (isMenu)
            {
                SetState(GameState.Menu);
                return;
            }

            if (pendingMarkLevelStart)
                Session.MarkLevelStart();
            pendingMarkLevelStart = true;

            invulnerableUntil = Time.time + 1f;
            SetState(GameState.Exploracion);
            LastStateChangeFrame = Time.frameCount;
        }
    }
}
