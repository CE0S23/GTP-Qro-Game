using UnityEngine;

namespace GTP
{
    /// <summary>Salida del nivel. Puede iniciar bloqueada hasta que un evento la desbloquee.</summary>
    [RequireComponent(typeof(Collider))]
    public sealed class LevelExit : MonoBehaviour
    {
        [SerializeField] private string nextScene = SceneNames.Barrio;
        [SerializeField] private bool startsLocked;
        [SerializeField] private string lockedThought = "Todavía no puedo irme.";
        [SerializeField] private Renderer indicator;

        private bool locked;
        private bool used;

        public bool IsLocked => locked;

        public void Configure(string scene, bool locked, string lockedMessage, Renderer marker)
        {
            nextScene = scene;
            startsLocked = locked;
            lockedThought = lockedMessage;
            indicator = marker;
        }

        private void Awake()
        {
            locked = startsLocked;
            RefreshIndicator();
        }

        /// <summary>Evento: habilita la salida.</summary>
        public void Unlock()
        {
            locked = false;
            RefreshIndicator();
        }

        private void RefreshIndicator()
        {
            RendererColor.Set(indicator, locked ? new Color(0.7f, 0.15f, 0.15f) : new Color(0.15f, 0.9f, 0.35f));
        }

        private bool playerInside;

        private void OnTriggerEnter(Collider other)
        {
            if (!IsPlayer(other) || used) return;
            playerInside = true;
            if (locked)
            {
                if (IsPlaying()) GameUI.Instance?.Hud.ShowThought(lockedThought, 2.5f);
                return;
            }
            TryComplete();
        }

        private void OnTriggerExit(Collider other)
        {
            if (IsPlayer(other)) playerInside = false;
        }

        // Corrección (Hito 03): si la salida se desbloquea mientras el jugador ya está dentro de la zona,
        // o el jugador entró durante un diálogo, se completa sin tener que salir y volver a entrar.
        // (Se usa Update y no OnTriggerStay porque el Rigidbody quieto puede "dormirse" y dejar de reportarlo.)
        private void Update()
        {
            if (playerInside && !used && !locked) TryComplete();
        }

        private void TryComplete()
        {
            if (!IsPlaying()) return;
            used = true;
            GameEvents.LevelCompleted.Raise(transform.position);
            GameManager.Instance.CompleteLevel(nextScene);
        }

        private static bool IsPlayer(Collider other) => other.GetComponentInParent<PlayerStealth>() != null;

        private static bool IsPlaying() =>
            GameManager.Instance != null && GameManager.Instance.State == GameState.Exploracion;
    }
}
