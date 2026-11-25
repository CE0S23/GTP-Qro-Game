using UnityEngine;
using UnityEngine.UI;

namespace GTP
{
    /// <summary>Resumen final con desglose de puntaje y récord. Vuelve al menú.</summary>
    public sealed class SummaryPanel : MonoBehaviour
    {
        private GameObject root;
        private Text body;

        public static SummaryPanel Build(Transform canvas)
        {
            var panel = canvas.gameObject.AddComponent<SummaryPanel>();
            Image bg = UIFactory.Panel(canvas, "Fondo", new Color(0f, 0f, 0f, 0.82f), Vector2.zero, Vector2.one);
            panel.root = bg.gameObject;
            panel.body = UIFactory.Label(bg.transform, "Texto", "", 36, TextAnchor.MiddleCenter, Color.white,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-800f, -120f), new Vector2(800f, 420f));
            UIFactory.Label(bg.transform, "Creditos",
                "GTP QroCITY stories · UTEQ · César Osvaldo Ramírez Pérez · Orlando Rubio Cabrera · Mauricio Gabriel Vázquez",
                22, TextAnchor.MiddleCenter, new Color(1f, 1f, 1f, 0.6f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(-900f, 30f), new Vector2(900f, 80f));
            UIFactory.Button(bg.transform, "Menu", "Volver al menú", new Vector2(0f, -250f), new Vector2(480f, 100f),
                () => GameManager.Instance.GoToMenu());
            panel.root.SetActive(false);
            return panel;
        }

        public void Show(string text)
        {
            body.text = text;
            root.SetActive(true);
        }

        public void Hide() => root.SetActive(false);
    }
}
