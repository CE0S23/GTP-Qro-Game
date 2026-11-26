using System.Collections;
using System.Globalization;
using System.IO;
using System.Text;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.SceneManagement;

namespace GTP
{
    /// <summary>
    /// Sonda de rendimiento para el "Análisis de Optimización" (rúbrica AU 4.1).
    /// Solo se activa si el ejecutable se lanza con el argumento <c>-gtpPerf</c>; en una partida normal no existe.
    ///
    /// Recorrido automático (≈ 2.5 min): Barrio → Ruta del Mal (redada activa) → prueba de estrés con
    /// 20 policías extra → se repite la carga de escenas para comprobar que la memoria no crece (fugas).
    /// Cada 0.5 s escribe en <c>perf_gtp.csv</c>: FPS, tiempo de CPU del hilo principal, basura generada (GC),
    /// memoria total usada, batches, SetPass calls y triángulos (contadores de ProfilerRecorder).
    /// Además guarda un archivo <c>perf_gtp.raw</c> del Profiler (abrir en Window → Analysis → Profiler → Load)
    /// y capturas de pantalla de cada fase.
    /// Uso: <c>"GTP QroCITY stories.exe" -gtpPerf -gtpPerfOut C:\carpeta [-gtpPerfNoLog] [-gtpPerfCycles 6]</c>
    /// </summary>
    public sealed class PerfProbe : MonoBehaviour
    {
        public static bool Active { get; private set; }

        private ProfilerRecorder mainThread, gcAlloc, totalUsed, gcReserved, gcUsed, gfxUsed, batches, drawCalls, setPass, triangles;
        private bool binaryLog = true;
        private int reloadCycles = 3;
        private StringBuilder csv;
        private string outDir;
        private string phase = "inicio";
        private float sampleTimer, frameAccum, startTime;
        private int frameCount;
        private float worstFrameMs;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Boot()
        {
            string[] args = System.Environment.GetCommandLineArgs();
            if (System.Array.IndexOf(args, "-gtpPerf") < 0) return;
            Active = true;
            var go = new GameObject("PerfProbe");
            DontDestroyOnLoad(go);
            var probe = go.AddComponent<PerfProbe>();
            int outIndex = System.Array.IndexOf(args, "-gtpPerfOut");
            probe.outDir = outIndex >= 0 && outIndex + 1 < args.Length ? args[outIndex + 1] : Application.persistentDataPath;
            probe.binaryLog = System.Array.IndexOf(args, "-gtpPerfNoLog") < 0;
            int cyclesIndex = System.Array.IndexOf(args, "-gtpPerfCycles");
            if (cyclesIndex >= 0 && cyclesIndex + 1 < args.Length && int.TryParse(args[cyclesIndex + 1], out int cycles))
                probe.reloadCycles = Mathf.Max(1, cycles);
        }

        private void Start()
        {
            Directory.CreateDirectory(outDir);
            Application.runInBackground = true; // si la ventana pierde el foco, la medición no se pausa
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = -1; // sin límite: se mide el costo real

            mainThread = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "Main Thread", 15);
            gcAlloc = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Allocated In Frame");
            totalUsed = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Total Used Memory");
            gcReserved = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Reserved Memory");
            gcUsed = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Used Memory");
            gfxUsed = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Gfx Used Memory");
            drawCalls = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Draw Calls Count");
            batches = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Batches Count");
            setPass = ProfilerRecorder.StartNew(ProfilerCategory.Render, "SetPass Calls Count");
            triangles = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Triangles Count");

            csv = new StringBuilder("t_s,fase,escena,fps,frame_ms_peor,cpu_main_ms,gc_alloc_kb_frame,total_used_mb,gc_reserved_mb,gc_used_mb,gfx_used_mb,batches,draw_calls,setpass,triangulos\n");
            startTime = Time.realtimeSinceStartup;

            if (binaryLog)
            {
                // Archivo .raw para abrirlo en el Profiler del editor (ocupa ~5 MB/s; desactivar con -gtpPerfNoLog).
                Profiler.logFile = Path.Combine(outDir, "perf_gtp");
                Profiler.enableBinaryLog = true;
                Profiler.enabled = true;
            }

            StartCoroutine(Run());
        }

        private IEnumerator Run()
        {
            yield return Play(SceneNames.Barrio, "barrio", 20f, new[] { V(3, 0), V(30, -2), V(60, 2), V(90, -2), V(112, 1) });
            yield return Play(SceneNames.RutaMal, "redada", 30f, new[] { V(17, 0), V(24, -2), V(34, 3), V(44, -1), V(54, 3), V(60, 0) },
                activateRaid: true);
            yield return Play(SceneNames.RutaMal, "estres_20_policias", 25f, new[] { V(20, 0), V(30, 2), V(40, -2), V(50, 2), V(58, 0) },
                activateRaid: true, extraPolice: 20);
            for (int i = 0; i < reloadCycles; i++)
            {
                yield return Play(SceneNames.Barrio, "recarga_" + (i + 1), 6f, new[] { V(3, 0), V(40, 0) });
                yield return Play(SceneNames.RutaMal, "recarga_" + (i + 1), 6f, new[] { V(17, 0), V(40, 0) }, activateRaid: true);
            }

            Profiler.enabled = false;
            Profiler.enableBinaryLog = false;
            File.WriteAllText(Path.Combine(outDir, "perf_gtp.csv"), csv.ToString());
            Debug.Log("[GTP] PerfProbe terminado: " + outDir);
            Application.Quit();
        }

        private IEnumerator Play(string scene, string phaseName, float seconds, Vector3[] path, bool activateRaid = false, int extraPolice = 0)
        {
            phase = phaseName;
            SceneManager.LoadScene(scene);
            yield return null;
            yield return null;
            PrepareForAutoplay(activateRaid, extraPolice);

            PlayerStealth player = PlayerStealth.Current;
            Rigidbody body = player != null ? player.GetComponent<Rigidbody>() : null;
            float t = 0f;
            bool captured = false;
            while (t < seconds)
            {
                t += Time.unscaledDeltaTime;
                ForceExploration();
                if (body != null && path.Length > 1)
                {
                    float k = Mathf.Clamp01(t / seconds) * (path.Length - 1);
                    int i = Mathf.Min(path.Length - 2, Mathf.FloorToInt(k));
                    Vector3 p = Vector3.Lerp(path[i], path[i + 1], k - i);
                    body.MovePosition(new Vector3(p.x, body.position.y, p.z));
                }
                if (!captured && t > seconds * 0.5f)
                {
                    captured = true;
                    ScreenCapture.CaptureScreenshot(Path.Combine(outDir, "captura_" + phaseName + ".png"));
                }
                yield return null;
            }
        }

        private static void PrepareForAutoplay(bool activateRaid, int extraPolice)
        {
            GameManager gm = GameManager.Instance;
            gm.DebugInvulnerable = true; // la sonda mide rendimiento, no debe perder intentos
            GameUI.Instance.Dialogue.Hide();
            gm.SetState(GameState.Exploracion);

            if (!activateRaid) return;
            GameObject raid = null;
            foreach (GameObject root in SceneManager.GetActiveScene().GetRootGameObjects())
                if (root.name == "Redada") raid = root;
            if (raid == null) return;
            raid.SetActive(true);

            PatrolRoute template = raid.GetComponentInChildren<PatrolRoute>();
            for (int i = 0; i < extraPolice && template != null; i++)
            {
                GameObject clone = Instantiate(template.gameObject, raid.transform);
                clone.name = "Policia_Estres_" + i;
                Vector3 offset = new Vector3(Random.Range(-10f, 25f), 0f, Random.Range(-6f, 6f));
                clone.transform.position = template.transform.position + offset;
            }
        }

        private static void ForceExploration()
        {
            GameManager gm = GameManager.Instance;
            if (gm.State == GameState.Narrativa || gm.State == GameState.Minijuego || gm.State == GameState.GameOver)
            {
                GameUI.Instance.HideAllOverlays();
                gm.SetState(GameState.Exploracion);
            }
        }

        private void Update()
        {
            if (csv == null) return;
            float dt = Time.unscaledDeltaTime;
            frameAccum += dt;
            frameCount++;
            worstFrameMs = Mathf.Max(worstFrameMs, dt * 1000f);
            sampleTimer += dt;
            if (sampleTimer < 0.5f) return;

            float fps = frameCount / Mathf.Max(0.0001f, frameAccum);
            var ci = CultureInfo.InvariantCulture;
            csv.AppendFormat(ci, "{0:F2},{1},{2},{3:F1},{4:F2},{5:F2},{6:F2},{7:F1},{8:F1},{9:F1},{10:F1},{11},{12},{13},{14}\n",
                Time.realtimeSinceStartup - startTime, phase, SceneManager.GetActiveScene().name, fps, worstFrameMs,
                Average(mainThread) * 1e-6, gcAlloc.LastValue / 1024.0, totalUsed.LastValue / (1024.0 * 1024.0),
                gcReserved.LastValue / (1024.0 * 1024.0), gcUsed.LastValue / (1024.0 * 1024.0), gfxUsed.LastValue / (1024.0 * 1024.0),
                batches.LastValue, drawCalls.LastValue, setPass.LastValue, triangles.LastValue);

            sampleTimer = 0f;
            frameAccum = 0f;
            frameCount = 0;
            worstFrameMs = 0f;
        }

        private static double Average(ProfilerRecorder recorder)
        {
            int count = recorder.Capacity > 0 ? Mathf.Min(recorder.Count, recorder.Capacity) : 0;
            if (count == 0) return 0;
            double sum = 0;
            for (int i = 0; i < count; i++) sum += recorder.GetSample(i).Value;
            return sum / count;
        }

        private void OnDestroy()
        {
            mainThread.Dispose();
            gcAlloc.Dispose();
            totalUsed.Dispose();
            gcReserved.Dispose();
            gcUsed.Dispose();
            gfxUsed.Dispose();
            drawCalls.Dispose();
            batches.Dispose();
            setPass.Dispose();
            triangles.Dispose();
        }

        private static Vector3 V(float x, float z) => new Vector3(x, 0f, z);
    }
}
