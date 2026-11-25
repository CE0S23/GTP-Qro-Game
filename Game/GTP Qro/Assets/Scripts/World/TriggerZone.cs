using UnityEngine;
using UnityEngine.Events;

namespace GTP
{
    /// <summary>
    /// Zona de evento genérica (GDD: "al entrar en la zona se activa..."). Al entrar el jugador
    /// dispara un UnityEvent configurable en el Inspector: activar asalto, guardar checkpoint, etc.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public sealed class TriggerZone : MonoBehaviour
    {
        [SerializeField] private bool once = true;
        [SerializeField] private string thought = "";
        [SerializeField, Min(0f)] private float thoughtSeconds = 3f;
        [SerializeField] private UnityEvent onPlayerEnter = new UnityEvent();

        private bool fired;

        public UnityEvent OnPlayerEnter => onPlayerEnter;

        public void Configure(bool fireOnce, string thoughtText)
        {
            once = fireOnce;
            thought = thoughtText;
        }

        private void Reset()
        {
            GetComponent<Collider>().isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (fired && once) return;
            if (other.GetComponentInParent<PlayerStealth>() == null) return;

            fired = true;
            if (!string.IsNullOrEmpty(thought))
                GameUI.Instance?.Hud.ShowThought(thought, thoughtSeconds);
            onPlayerEnter.Invoke();
        }
    }
}
