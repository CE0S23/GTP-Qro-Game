using UnityEngine;

namespace GTP
{
    /// <summary>Cambia el color de un material URP Lit (usa _BaseColor, no _Color).</summary>
    public static class RendererColor
    {
        private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
        private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

        public static void Set(Renderer renderer, Color color)
        {
            if (renderer == null) return;
            Material material = renderer.material;
            if (material.HasProperty(BaseColor)) material.SetColor(BaseColor, color);
            else material.color = color;
            if (material.HasProperty(EmissionColor)) material.SetColor(EmissionColor, color * 0.8f);
        }
    }
}
