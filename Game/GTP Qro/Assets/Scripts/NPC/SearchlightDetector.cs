using UnityEngine;

namespace GTP
{
    /// <summary>
    /// Foco del helicóptero (GDD §4.8): círculo de luz que se desplaza con PatrolRoute.
    /// Detecta al jugador dentro del círculo si no está escondido.
    /// </summary>
    public sealed class SearchlightDetector : ThreatDetector
    {
        [SerializeField, Min(0.5f)] private float radius = 3f;
        [SerializeField] private Renderer discRenderer;
        [SerializeField] private Color calmColor = new Color(1f, 1f, 0.75f, 0.3f);
        [SerializeField] private Color alertColor = new Color(1f, 0.2f, 0.1f, 0.5f);

        private MaterialPropertyBlock block;
        private static readonly int ColorId = Shader.PropertyToID("_Color");

        public void Configure(float discRadius, Renderer disc)
        {
            radius = discRadius;
            discRenderer = disc;
        }

        protected override bool CanSee(PlayerStealth player)
        {
            Vector3 delta = player.transform.position - transform.position;
            delta.y = 0f;
            float r = radius * Mathf.Min(1f, player.DetectionRangeMultiplier + 0.2f);
            return delta.sqrMagnitude <= r * r;
        }

        protected override void Update()
        {
            base.Update();
            if (discRenderer == null) return;
            block ??= new MaterialPropertyBlock();
            block.SetColor(ColorId, Color.Lerp(calmColor, alertColor, Suspicion01));
            discRenderer.SetPropertyBlock(block);
        }
    }
}
