using UnityEngine;
using UnityEngine.UI;

namespace GTP
{
    /// <summary>
    /// Interfaz del celular (GDD §4.6): batería, mapa con flecha al objetivo, puntaje, intentos,
    /// pensamientos del personaje, avisos y barra de detección.
    /// </summary>
    public sealed class PhoneUIController : MonoBehaviour
    {
        private GameObject root;
        private Text batteryText, scoreText, livesText, objectiveText, distanceText, promptText, thoughtText, toastText, alertText;
        private Image batteryFill, alertFill;
        private RectTransform arrow;
        private GameSession session;
        private Transform objectiveTarget;
        private float thoughtUntil, toastUntil;

        public string CurrentObjective => objectiveText != null ? objectiveText.text : "";

        public static PhoneUIController Build(Transform canvas)
        {
            var hud = canvas.gameObject.AddComponent<PhoneUIController>();
            hud.root = UIFactory.Rect(canvas, "Raiz", Vector2.zero, Vector2.one).gameObject;
            Transform r = hud.root.transform;

            // Celular (esquina superior izquierda)
            Image phone = UIFactory.Panel(r, "Celular", new Color(0.05f, 0.05f, 0.08f, 0.88f),
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(24f, -444f), new Vector2(324f, -24f));
            Transform p = phone.transform;
            hud.batteryText = UIFactory.Label(p, "Bateria", "Batería 12%", 26, TextAnchor.MiddleLeft, Color.white,
                new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(16f, -48f), new Vector2(-16f, -8f));
            Image batteryBg = UIFactory.Panel(p, "BateriaFondo", new Color(0.25f, 0.25f, 0.25f), new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(16f, -66f), new Vector2(-16f, -52f));
            hud.batteryFill = UIFactory.Panel(batteryBg.transform, "BateriaNivel", new Color(0.3f, 0.9f, 0.4f), Vector2.zero, Vector2.one);
            hud.scoreText = UIFactory.Label(p, "Puntaje", "Puntos: 0", 26, TextAnchor.MiddleLeft, Color.white,
                new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(16f, -110f), new Vector2(-16f, -72f));
            hud.livesText = UIFactory.Label(p, "Intentos", "Intentos: 3", 26, TextAnchor.MiddleLeft, new Color(1f, 0.55f, 0.6f),
                new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(16f, -148f), new Vector2(-16f, -110f));

            // Mapa del celular: círculo con flecha hacia el objetivo
            Image map = UIFactory.Panel(p, "Mapa", new Color(0.1f, 0.18f, 0.3f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f));
            map.rectTransform.sizeDelta = new Vector2(200f, 200f);
            map.rectTransform.anchoredPosition = new Vector2(0f, 150f);
            UIFactory.Label(map.transform, "Titulo", "MAPA", 18, TextAnchor.UpperCenter, new Color(0.6f, 0.75f, 1f),
                Vector2.zero, Vector2.one, new Vector2(0f, 0f), new Vector2(0f, -6f));
            hud.arrow = UIFactory.Rect(map.transform, "Flecha", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            hud.arrow.sizeDelta = new Vector2(20f, 150f);
            Image shaft = UIFactory.Panel(hud.arrow, "Asta", new Color(1f, 0.85f, 0.2f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 1f),
                new Vector2(-4f, 0f), new Vector2(4f, -20f));
            shaft.raycastTarget = false;
            Image head = UIFactory.Panel(hud.arrow, "Punta", new Color(1f, 0.85f, 0.2f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
            head.rectTransform.sizeDelta = new Vector2(26f, 26f);
            head.rectTransform.anchoredPosition = new Vector2(0f, -16f);
            head.rectTransform.localRotation = Quaternion.Euler(0f, 0f, 45f);
            hud.distanceText = UIFactory.Label(map.transform, "Distancia", "", 20, TextAnchor.LowerCenter, Color.white,
                Vector2.zero, Vector2.one, new Vector2(0f, 6f), Vector2.zero);
            hud.objectiveText = UIFactory.Label(p, "Objetivo", "", 22, TextAnchor.UpperLeft, new Color(1f, 0.9f, 0.5f),
                new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(16f, 4f), new Vector2(-16f, 46f));

            // Detección
            hud.alertText = UIFactory.Label(r, "Alerta", "¡TE ESTÁN VIENDO!", 34, TextAnchor.MiddleCenter, new Color(1f, 0.3f, 0.25f),
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(-300f, -80f), new Vector2(300f, -30f));
            Image alertBg = UIFactory.Panel(hud.alertText.transform, "Fondo", new Color(0.2f, 0.05f, 0.05f, 0.8f), new Vector2(0f, 0f), new Vector2(1f, 0f),
                new Vector2(50f, -16f), new Vector2(-50f, -6f));
            hud.alertFill = UIFactory.Panel(alertBg.transform, "Nivel", new Color(1f, 0.25f, 0.2f), Vector2.zero, Vector2.one);

            // Aviso de interacción, pensamientos y toasts
            hud.promptText = UIFactory.Label(r, "Aviso", "", 30, TextAnchor.MiddleCenter, Color.white,
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-400f, 230f), new Vector2(400f, 280f));
            hud.thoughtText = UIFactory.Label(r, "Pensamiento", "", 30, TextAnchor.MiddleCenter, new Color(0.8f, 0.9f, 1f),
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-700f, 140f), new Vector2(700f, 220f));
            hud.thoughtText.fontStyle = FontStyle.Italic;
            hud.toastText = UIFactory.Label(r, "Toast", "", 30, TextAnchor.UpperRight, new Color(0.6f, 1f, 0.6f),
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-620f, -140f), new Vector2(-30f, -30f));
            UIFactory.Label(r, "Controles", "WASD/Flechas mover · Shift correr · C agacharse · E interactuar · Esc pausa", 20,
                TextAnchor.LowerRight, new Color(1f, 1f, 1f, 0.55f), new Vector2(1f, 0f), new Vector2(1f, 0f),
                new Vector2(-1000f, 10f), new Vector2(-20f, 40f));

            hud.SetFill(hud.alertFill, 0f);
            hud.root.SetActive(false);
            return hud;
        }

        public void Bind(GameSession newSession)
        {
            if (session != null)
            {
                session.ScoreChanged -= OnScore;
                session.LivesChanged -= OnLives;
                session.BatteryChanged -= OnBattery;
            }
            session = newSession;
            session.ScoreChanged += OnScore;
            session.LivesChanged += OnLives;
            session.BatteryChanged += OnBattery;
            OnScore(session.Score, 0);
            OnLives(session.Lives);
            OnBattery(session.Battery);
        }

        public void SetVisible(bool visible) => root.SetActive(visible);

        public void SetObjective(string text, Transform target)
        {
            objectiveText.text = string.IsNullOrEmpty(text) ? "" : "Objetivo: " + text;
            objectiveTarget = target;
            if (!string.IsNullOrEmpty(text)) ShowToast("Nuevo objetivo");
        }

        public void SetPrompt(string text)
        {
            if (promptText != null && promptText.text != text) promptText.text = text;
        }

        public void ShowThought(string text, float seconds)
        {
            thoughtText.text = text;
            thoughtUntil = Time.unscaledTime + seconds;
        }

        public void ShowToast(string text)
        {
            toastText.text = Time.unscaledTime < toastUntil && !string.IsNullOrEmpty(toastText.text)
                ? toastText.text + "\n" + text
                : text;
            toastUntil = Time.unscaledTime + 2.2f;
        }

        private void OnScore(int total, int delta)
        {
            scoreText.text = "Puntos: " + total;
            if (delta > 0) ShowToast("+" + delta + " puntos");
            else if (delta < 0) ShowToast(delta + " puntos");
        }

        private void OnLives(int lives) => livesText.text = "Intentos: " + lives;

        private void OnBattery(float battery)
        {
            batteryText.text = "Batería " + Mathf.CeilToInt(battery) + "%";
            SetFill(batteryFill, Mathf.Clamp01(battery / 15f));
            batteryFill.color = battery <= 4f ? new Color(1f, 0.25f, 0.2f) : (battery <= 8f ? new Color(1f, 0.8f, 0.2f) : new Color(0.3f, 0.9f, 0.4f));
        }

        private void SetFill(Image image, float value)
        {
            // Sin sprite, Image.fillAmount no recorta: se escala el ancho mediante anclas dentro del fondo.
            image.rectTransform.anchorMax = new Vector2(Mathf.Clamp01(value), 1f);
        }

        private void Update()
        {
            if (!root.activeSelf) return;

            if (thoughtText.text.Length > 0 && Time.unscaledTime > thoughtUntil) thoughtText.text = "";
            if (toastText.text.Length > 0 && Time.unscaledTime > toastUntil) toastText.text = "";

            float suspicion = ThreatDetector.MaxSuspicion01;
            alertText.gameObject.SetActive(suspicion > 0.01f);
            SetFill(alertFill, suspicion);

            PlayerStealth player = PlayerStealth.Current;
            bool hasTarget = objectiveTarget != null && player != null;
            arrow.gameObject.SetActive(hasTarget);
            distanceText.text = "";
            if (hasTarget)
            {
                Vector3 delta = objectiveTarget.position - player.transform.position;
                delta.y = 0f;
                float angle = Mathf.Atan2(delta.x, delta.z) * Mathf.Rad2Deg;
                arrow.localRotation = Quaternion.Euler(0f, 0f, -angle);
                distanceText.text = Mathf.RoundToInt(delta.magnitude) + " m";
            }
        }
    }
}
