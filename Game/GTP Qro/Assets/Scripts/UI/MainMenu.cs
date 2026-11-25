using UnityEngine;
using UnityEngine.UI;

namespace GTP
{
    /// <summary>Menú principal (Escena_Menu): jugar, ver controles, salir y récord.</summary>
    public sealed class MainMenu : MonoBehaviour
    {
        private GameObject controlsPanel;

        private void Start()
        {
            Canvas canvas = UIFactory.CreateCanvas(transform, "MenuCanvas", 5);
            Transform r = canvas.transform;
            UIFactory.Panel(r, "Fondo", new Color(0.04f, 0.05f, 0.12f, 0.6f), Vector2.zero, Vector2.one);
            UIFactory.Label(r, "Titulo", "GTP QroCITY stories", 96, TextAnchor.MiddleCenter, new Color(1f, 0.8f, 0.3f),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-900f, 240f), new Vector2(900f, 380f));
            UIFactory.Label(r, "Subtitulo", "Una noche. Una decisión. Dos finales.", 36, TextAnchor.MiddleCenter, Color.white,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-900f, 170f), new Vector2(900f, 240f));

            Button play = UIFactory.Button(r, "Jugar", "Jugar", new Vector2(0f, 40f), new Vector2(480f, 100f),
                () => GameManager.Instance.StartNewGame());
            UIFactory.Button(r, "Controles", "Controles", new Vector2(0f, -80f), new Vector2(480f, 100f),
                () => controlsPanel.SetActive(!controlsPanel.activeSelf));
            UIFactory.Button(r, "Salir", "Salir", new Vector2(0f, -200f), new Vector2(480f, 100f), Quit);

            int best = PlayerPrefs.GetInt(HighScore.Key, 0);
            UIFactory.Label(r, "Record", "Récord: " + best, 30, TextAnchor.MiddleCenter, new Color(0.6f, 1f, 0.6f),
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-400f, 90f), new Vector2(400f, 140f));
            UIFactory.Label(r, "Aviso", "Videojuego académico con mensaje contra el uso de drogas · UTEQ", 22, TextAnchor.MiddleCenter,
                new Color(1f, 1f, 1f, 0.55f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-800f, 30f), new Vector2(800f, 70f));

            Image controls = UIFactory.Panel(r, "PanelControles", new Color(0.02f, 0.02f, 0.05f, 0.96f),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-620f, -330f), new Vector2(620f, 330f));
            controlsPanel = controls.gameObject;
            UIFactory.Label(controls.transform, "Texto",
                "CONTROLES\n\n" +
                "WASD / Flechas: moverse\n" +
                "Shift: correr (te ven desde más lejos)\n" +
                "C: agacharse (te ven desde más cerca)\n" +
                "E: interactuar · leer grafitis · esconderse en contenedores\n" +
                "Espacio: acción en minijuegos\n" +
                "Esc: pausa\n\n" +
                "Tienes 3 intentos por nivel y un celular con poca batería.\n" +
                "Si un policía te ve 2 segundos o te alcanzan, pierdes un intento.",
                30, TextAnchor.MiddleCenter, Color.white, Vector2.zero, Vector2.one, new Vector2(30f, 110f), new Vector2(-30f, -30f));
            UIFactory.Button(controls.transform, "Cerrar", "Cerrar", new Vector2(0f, -260f), new Vector2(300f, 80f),
                () => controlsPanel.SetActive(false));
            controlsPanel.SetActive(false);

            play.Select();
        }

        private static void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
