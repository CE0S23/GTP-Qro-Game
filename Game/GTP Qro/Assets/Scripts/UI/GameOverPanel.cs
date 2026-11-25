using UnityEngine;
using UnityEngine.UI;

namespace GTP
{
    /// <summary>Game Over (sin intentos o batería en 0 %): reintentar el nivel o salir al menú.</summary>
    public sealed class GameOverPanel : MonoBehaviour
    {
        private GameObject root;
        private Text reasonText;

        public static GameOverPanel Build(Transform canvas)
        {
            var panel = canvas.gameObject.AddComponent<GameOverPanel>();
            Image bg = UIFactory.Panel(canvas, "Fondo", new Color(0.15f, 0f, 0f, 0.88f), Vector2.zero, Vector2.one);
            panel.root = bg.gameObject;
            UIFactory.Label(bg.transform, "Titulo", "GAME OVER", 96, TextAnchor.MiddleCenter, new Color(1f, 0.3f, 0.25f),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-600f, 200f), new Vector2(600f, 330f));
            panel.reasonText = UIFactory.Label(bg.transform, "Motivo", "", 34, TextAnchor.MiddleCenter, Color.white,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-750f, 40f), new Vector2(750f, 190f));
            UIFactory.Button(bg.transform, "Reintentar", "Reintentar nivel", new Vector2(-270f, -120f), new Vector2(480f, 100f),
                () => GameManager.Instance.RetryLevel());
            UIFactory.Button(bg.transform, "Menu", "Menú principal", new Vector2(270f, -120f), new Vector2(480f, 100f),
                () => GameManager.Instance.GoToMenu());
            panel.root.SetActive(false);
            return panel;
        }

        public void Show(string reason, int score)
        {
            reasonText.text = reason + "\n\nPuntaje: " + score;
            root.SetActive(true);
        }

        public void Hide() => root.SetActive(false);
    }
}
