using UnityEngine;
using UnityEngine.UI;

namespace GTP
{
    /// <summary>Menú de pausa (Esc): continuar, reiniciar nivel o volver al menú.</summary>
    public sealed class PausePanel : MonoBehaviour
    {
        private GameObject root;

        public static PausePanel Build(Transform canvas)
        {
            var panel = canvas.gameObject.AddComponent<PausePanel>();
            Image bg = UIFactory.Panel(canvas, "Fondo", new Color(0f, 0f, 0f, 0.7f), Vector2.zero, Vector2.one);
            panel.root = bg.gameObject;
            UIFactory.Label(bg.transform, "Titulo", "PAUSA", 80, TextAnchor.MiddleCenter, Color.white,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-400f, 160f), new Vector2(400f, 280f));
            UIFactory.Button(bg.transform, "Continuar", "Continuar (Esc)", new Vector2(0f, 40f), new Vector2(480f, 90f),
                () => GameManager.Instance.Resume());
            UIFactory.Button(bg.transform, "Reiniciar", "Reiniciar nivel", new Vector2(0f, -70f), new Vector2(480f, 90f),
                () => GameManager.Instance.RetryLevel());
            UIFactory.Button(bg.transform, "Menu", "Menú principal", new Vector2(0f, -180f), new Vector2(480f, 90f),
                () => GameManager.Instance.GoToMenu());
            panel.root.SetActive(false);
            return panel;
        }

        public void Show() => root.SetActive(true);
        public void Hide() => root.SetActive(false);
    }
}
