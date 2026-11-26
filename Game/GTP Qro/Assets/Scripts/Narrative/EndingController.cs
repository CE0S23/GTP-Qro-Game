using System.Collections;
using UnityEngine;

namespace GTP
{
    /// <summary>
    /// Resolución en el Spot (GDD §4.5, nivel 4). Muestra el final según la ruta guardada,
    /// aplica el cambio visual del amanecer, calcula el puntaje final y vuelve al menú.
    /// </summary>
    public sealed class EndingController : MonoBehaviour
    {
        [SerializeField] private Light sun;
        [SerializeField] private Color goodSunColor = new Color(1f, 0.85f, 0.6f);
        [SerializeField] private Color badCalmColor = new Color(1f, 0.25f, 0.85f);
        [SerializeField] private Color badDawnColor = new Color(0.55f, 0.55f, 0.55f);

        private bool started;

        public void Configure(Light directional) => sun = directional;

        /// <summary>Evento: el jugador llegó al Spot.</summary>
        public void Begin()
        {
            if (started) return;
            started = true;

            GameManager gm = GameManager.Instance;
            Route route = gm.Session.Route == Route.None ? Route.Bien : gm.Session.Route;
            if (route == Route.Bien) PlayGoodEnding();
            else PlayBadEnding();
        }

        private void PlayGoodEnding()
        {
            GameUI.Instance.Dialogue.Play(new[]
            {
                new DialogueLine("Novia", "Llegamos. Mira, ya está saliendo el sol."),
                new DialogueLine("Amigo", "Así se ve mejor la ciudad, ¿no? Sin andar huyendo."),
                new DialogueLine("Pensamiento", "No necesitaba escapar de mí. Necesitaba a alguien a mi lado."),
                new DialogueLine("", "FINAL DEL BIEN · La conexión humana fue la verdadera salida.")
            }, () => StartCoroutine(GoodDawn()));
        }

        private IEnumerator GoodDawn()
        {
            GameManager.Instance.SetState(GameState.Final);
            PostFXController.Current?.SetMood(PostFXController.Mood.CalidoAmanecer, 2f);
            yield return LerpLight(goodSunColor, 1.3f, 2f);
            Finish(Route.Bien);
        }

        private void PlayBadEnding()
        {
            GameUI.Instance.Dialogue.Play(new[]
            {
                new DialogueLine("Pensamiento", "Llegué. Solo. Como siempre."),
                new DialogueLine("Pensamiento", "Por lo menos esto me va a calmar...")
            }, () => GameUI.Instance.Minigame.StartMash("Encendedor",
                "Presiona ESPACIO rápido para raspar el encendedor y proteger la llama.", 12, OnLighter));
        }

        private void OnLighter(bool success)
        {
            if (PlayerStealth.Current != null)
                FeedbackFX.Instance?.PlayLighterFlame(PlayerStealth.Current.transform.position + Vector3.up * 0.6f);
            StartCoroutine(BadDawn());
        }

        private IEnumerator BadDawn()
        {
            GameManager.Instance.SetState(GameState.Final);
            PostFXController.Current?.SetMood(PostFXController.Mood.Saturado, 1.5f);
            yield return LerpLight(badCalmColor, 1.6f, 1.5f);
            yield return new WaitForSecondsRealtime(1.5f);
            PostFXController.Current?.SetMood(PostFXController.Mood.Desaturado, 2.5f);
            FeedbackFX.Instance?.StopLighterFlame();
            yield return LerpLight(badDawnColor, 0.6f, 2.5f);
            bool done = false;
            GameUI.Instance.Dialogue.Play(new[]
            {
                new DialogueLine("", "Sale el sol. El efecto se desvanece."),
                new DialogueLine("Pensamiento", "Golpeado, sucio y solo... Solo fue un espejismo."),
                new DialogueLine("", "FINAL DEL MAL · La droga no fue una salida: empeoró la realidad.")
            }, () => done = true, keepState: true);
            yield return new WaitUntil(() => done);
            Finish(Route.Mal);
        }

        private IEnumerator LerpLight(Color color, float intensity, float seconds)
        {
            if (sun == null) yield break;
            Color startColor = sun.color;
            float startIntensity = sun.intensity;
            Color startAmbient = RenderSettings.ambientLight;
            for (float t = 0f; t < seconds; t += Time.unscaledDeltaTime)
            {
                float k = t / seconds;
                sun.color = Color.Lerp(startColor, color, k);
                sun.intensity = Mathf.Lerp(startIntensity, intensity, k);
                RenderSettings.ambientLight = Color.Lerp(startAmbient, color * 0.45f, k);
                yield return null;
            }
            sun.color = color;
            sun.intensity = intensity;
        }

        private void Finish(Route route)
        {
            GameManager gm = GameManager.Instance;
            GameSession s = gm.Session;
            int batteryBonus = Mathf.RoundToInt(s.Battery) * gm.Config.endBonusPerBatteryPercent;
            int livesBonus = s.Lives * gm.Config.endBonusPerLife;
            s.AddScore(batteryBonus + livesBonus);

            int best = PlayerPrefs.GetInt(HighScore.Key, 0);
            bool record = s.Score > best;
            if (record)
            {
                PlayerPrefs.SetInt(HighScore.Key, s.Score);
                PlayerPrefs.Save();
            }

            string summary =
                (route == Route.Bien ? "FINAL DEL BIEN" : "FINAL DEL MAL") + "\n\n" +
                "Grafitis leídos: " + s.GraffitiRead + "\n" +
                "Baterías recogidas: " + s.PickupsCollected + "\n" +
                "Tramos de sigilo perfecto: " + s.PerfectStealthSections + "\n" +
                "Veces detectado: " + s.Detections + "\n" +
                "Bono batería (" + Mathf.RoundToInt(s.Battery) + "%): +" + batteryBonus + "\n" +
                "Bono intentos (" + s.Lives + "): +" + livesBonus + "\n\n" +
                "PUNTAJE FINAL: " + s.Score + (record ? "   ¡NUEVO RÉCORD!" : "   (Récord: " + best + ")");

            GameUI.Instance.Summary.Show(summary);
        }
    }

    public static class HighScore
    {
        public const string Key = "GTP_HighScore";
    }
}
