using UnityEngine;

namespace GTP
{
    /// <summary>Objetivo mostrado en el celular (texto + flecha de mapa hacia el destino).</summary>
    public sealed class ObjectiveStep : MonoBehaviour
    {
        [SerializeField] private string text = "Ve al Punto";
        [SerializeField] private Transform target;
        [SerializeField] private bool activateOnStart;

        public void Configure(string objective, Transform destination, bool onStart)
        {
            text = objective;
            target = destination;
            activateOnStart = onStart;
        }

        private void Start()
        {
            if (activateOnStart) Activate();
        }

        /// <summary>Evento: este objetivo pasa a ser el actual.</summary>
        public void Activate()
        {
            GameUI.Instance?.Hud.SetObjective(text, target);
        }
    }
}
