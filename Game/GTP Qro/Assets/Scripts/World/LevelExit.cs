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

        private void OnTriggerEnter(Collider other)
        {
            if (used || other.GetComponentInParent<PlayerStealth>() == null) return;
            if (GameManager.Instance == null || GameManager.Instance.State != GameState.Exploracion) return;

            if (locked)
            {
                GameUI.Instance?.Hud.ShowThought(lockedThought, 2.5f);
                return;
            }

            used = true;
            GameManager.Instance.CompleteLevel(nextScene);
        }
    }
}
