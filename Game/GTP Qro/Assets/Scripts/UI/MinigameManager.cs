using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace GTP
{
    /// <summary>
    /// Minijuegos a pantalla completa (GDD §4.6):
    /// - Ganzúa: detener la aguja dentro de la zona verde 3 veces (2 fallos = reintentar).
    /// - Encendedor: presionar Espacio rápido para llenar la barra antes de que se apague.
    /// </summary>
    public sealed class MinigameManager : MonoBehaviour
    {
        private enum Mode { None, Lockpick, Mash }

        private GameObject root;
        private Text titleText, instructionsText, statusText;
        private RectTransform track, zone, needle, fill;
        private Mode mode;
        private Action<bool> onFinished;
        private int openedFrame;

        // Ganzúa
        private int hits, misses;
        private float needlePos, needleSpeed, zoneCenter, zoneWidth;
        private const int HitsNeeded = 3, MissesAllowed = 2;

        // Encendedor
        private float mashProgress;
        private int mashTarget;

        public bool IsOpen => root.activeSelf;

        public static MinigameManager Build(Transform canvas)
        {
            var mg = canvas.gameObject.AddComponent<MinigameManager>();
            Image bg = UIFactory.Panel(canvas, "Fondo", new Color(0.02f, 0.02f, 0.05f, 0.95f), Vector2.zero, Vector2.one);
            mg.root = bg.gameObject;
            Transform r = bg.transform;
            mg.titleText = UIFactory.Label(r, "Titulo", "", 60, TextAnchor.MiddleCenter, new Color(1f, 0.85f, 0.3f),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-800f, 260f), new Vector2(800f, 360f));
            mg.instructionsText = UIFactory.Label(r, "Instrucciones", "", 30, TextAnchor.MiddleCenter, Color.white,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-800f, 140f), new Vector2(800f, 250f));

            Image trackImg = UIFactory.Panel(r, "Pista", new Color(0.25f, 0.25f, 0.3f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            mg.track = trackImg.rectTransform;
            mg.track.sizeDelta = new Vector2(900f, 60f);
            Image zoneImg = UIFactory.Panel(mg.track, "ZonaVerde", new Color(0.2f, 0.85f, 0.35f), new Vector2(0f, 0f), new Vector2(0f, 1f));
            mg.zone = zoneImg.rectTransform;
            Image fillImg = UIFactory.Panel(mg.track, "Relleno", new Color(1f, 0.55f, 0.15f), Vector2.zero, new Vector2(0f, 1f));
            mg.fill = fillImg.rectTransform;
            Image needleImg = UIFactory.Panel(mg.track, "Aguja", Color.white, new Vector2(0f, 0f), new Vector2(0f, 1f),
                new Vector2(-4f, -20f), new Vector2(4f, 20f));
            mg.needle = needleImg.rectTransform;

            mg.statusText = UIFactory.Label(r, "Estado", "", 32, TextAnchor.MiddleCenter, Color.white,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-800f, -160f), new Vector2(800f, -80f));
            UIFactory.Label(r, "Salir", "Esc: abandonar", 22, TextAnchor.MiddleCenter, new Color(1f, 1f, 1f, 0.5f),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-400f, -260f), new Vector2(400f, -210f));
            mg.root.SetActive(false);
            return mg;
        }

        public void StartLockpick(Action<bool> finished)
        {
            Open(Mode.Lockpick, "Forzar cerradura",
                "Presiona ESPACIO cuando la aguja esté sobre la zona verde. Necesitas " + HitsNeeded + " aciertos.", finished);
            hits = 0;
            misses = 0;
            needlePos = 0f;
            needleSpeed = 0.9f;
            NewZone();
        }

        public void StartMash(string title, string instructions, int presses, Action<bool> finished)
        {
            Open(Mode.Mash, title, instructions, finished);
            mashProgress = 0f;
            mashTarget = Mathf.Max(1, presses);
        }

        public void Hide()
        {
            root.SetActive(false);
            mode = Mode.None;
            onFinished = null;
        }

        private void Open(Mode newMode, string title, string instructions, Action<bool> finished)
        {
            mode = newMode;
            onFinished = finished;
            openedFrame = Time.frameCount;
            titleText.text = title;
            instructionsText.text = instructions;
            statusText.text = "";
            zone.gameObject.SetActive(newMode == Mode.Lockpick);
            needle.gameObject.SetActive(newMode == Mode.Lockpick);
            fill.gameObject.SetActive(newMode == Mode.Mash);
            root.SetActive(true);
            GameManager gm = GameManager.Instance;
            if (gm != null && gm.State != GameState.Final)
                gm.SetState(GameState.Minijuego);
        }

        private void NewZone()
        {
            zoneWidth = Mathf.Max(0.1f, 0.2f - hits * 0.03f);
            zoneCenter = UnityEngine.Random.Range(0.2f, 0.8f);
            zone.anchorMin = new Vector2(zoneCenter - zoneWidth * 0.5f, 0f);
            zone.anchorMax = new Vector2(zoneCenter + zoneWidth * 0.5f, 1f);
        }

        private void Update()
        {
            if (mode == Mode.None || Time.frameCount == openedFrame) return;
            Keyboard k = Keyboard.current;
            if (k == null) return;

            if (k.escapeKey.wasPressedThisFrame)
            {
                Finish(false);
                return;
            }

            if (mode == Mode.Lockpick) UpdateLockpick(k);
            else UpdateMash(k);
        }

        private void UpdateLockpick(Keyboard k)
        {
            needlePos += needleSpeed * Time.unscaledDeltaTime;
            if (needlePos > 1f || needlePos < 0f)
            {
                needleSpeed = -needleSpeed;
                needlePos = Mathf.Clamp01(needlePos);
            }
            needle.anchorMin = new Vector2(needlePos, 0f);
            needle.anchorMax = new Vector2(needlePos, 1f);

            if (!k.spaceKey.wasPressedThisFrame) return;

            if (Mathf.Abs(needlePos - zoneCenter) <= zoneWidth * 0.5f)
            {
                hits++;
                needleSpeed = Mathf.Sign(needleSpeed) * (Mathf.Abs(needleSpeed) + 0.25f);
                statusText.text = "¡Clic! " + hits + "/" + HitsNeeded;
                if (hits >= HitsNeeded) { Finish(true); return; }
                NewZone();
            }
            else
            {
                misses++;
                statusText.text = "Fallo " + misses + "/" + MissesAllowed;
                if (misses >= MissesAllowed) Finish(false);
            }
        }

        private void UpdateMash(Keyboard k)
        {
            mashProgress = Mathf.Max(0f, mashProgress - 0.18f * Time.unscaledDeltaTime);
            if (k.spaceKey.wasPressedThisFrame)
                mashProgress += 1f / mashTarget;
            fill.anchorMax = new Vector2(Mathf.Clamp01(mashProgress), 1f);
            statusText.text = mashProgress < 0.5f ? "La llama se apaga..." : "Casi...";
            if (mashProgress >= 1f) Finish(true);
        }

        private void Finish(bool success)
        {
            Action<bool> callback = onFinished;
            bool wasFinal = GameManager.Instance != null && GameManager.Instance.State == GameState.Final;
            Hide();
            if (!wasFinal && GameManager.Instance != null)
                GameManager.Instance.SetState(GameState.Exploracion);
            callback?.Invoke(success);
        }
    }
}
