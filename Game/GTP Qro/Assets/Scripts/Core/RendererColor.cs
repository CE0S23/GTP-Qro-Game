using UnityEngine;

namespace GTP
{
    /// <summary>
    /// Cambia el color de un material URP Lit (usa _BaseColor, no _Color).
    /// Usa un MaterialPropertyBlock: no crea una copia del material por objeto (renderer.material),
    /// así no quedan materiales huérfanos en memoria al cambiar de escena.
    /// </summary>
    public static class RendererColor
    {
        private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");
        private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");
        private static MaterialPropertyBlock block;

        public static void Set(Renderer renderer, Color color)
        {
            if (renderer == null) return;
            block ??= new MaterialPropertyBlock();
            renderer.GetPropertyBlock(block);
            Material shared = renderer.sharedMaterial;
            block.SetColor(shared != null && shared.HasProperty(BaseColor) ? BaseColor : ColorId, color);
            if (shared != null && shared.HasProperty(EmissionColor)) block.SetColor(EmissionColor, color * 0.8f);
            renderer.SetPropertyBlock(block);
        }
    }
}
