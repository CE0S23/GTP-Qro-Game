using UnityEngine;

namespace GTP
{
    /// <summary>Punto de control (GDD §4.8): guarda la posición de reaparición al pasar el jugador.</summary>
    [RequireComponent(typeof(Collider))]
    public sealed class Checkpoint : MonoBehaviour
    {
        [SerializeField] private Renderer indicator;
        [SerializeField] private Color activeColor = new Color(0.2f, 1f, 0.4f);

        private bool reached;

        public void Configure(Renderer marker) => indicator = marker;

        private void OnTriggerEnter(Collider other)
        {
            PlayerRespawn respawn = other.GetComponentInParent<PlayerRespawn>();
            if (respawn == null || reached) return;

            reached = true;
            respawn.SetCheckpoint(transform.position);
            RendererColor.Set(indicator, activeColor);
            GameUI.Instance?.Hud.ShowToast("Punto de control");
        }
    }
}
