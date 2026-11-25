using System;
using System.Collections.Generic;

namespace GTP
{
    /// <summary>
    /// Estado de una partida: puntaje, intentos (vidas), batería, ruta y objetos ya recogidos.
    /// Clase C# pura (sin MonoBehaviour) para poder probarla sin escena.
    /// Los cambios se publican con eventos; la UI y el GameManager se suscriben.
    /// </summary>
    public sealed class GameSession
    {
        public event Action<int, int> ScoreChanged;      // (total, delta)
        public event Action<int> LivesChanged;
        public event Action<float> BatteryChanged;
        public event Action BatteryDepleted;
        public event Action<Route> RouteChosen;

        public int Score { get; private set; }
        public int Lives { get; private set; }
        public int MaxLives { get; }
        public float Battery { get; private set; }
        public Route Route { get; private set; }
        public int Detections { get; private set; }
        public int GraffitiRead { get; private set; }
        public int PickupsCollected { get; private set; }
        public int PerfectStealthSections { get; private set; }

        private readonly HashSet<string> collected = new HashSet<string>();
        private Snapshot levelStart;

        public GameSession(int maxLives, float startBattery)
        {
            if (maxLives < 1) throw new ArgumentOutOfRangeException(nameof(maxLives));
            MaxLives = maxLives;
            Lives = maxLives;
            Battery = Clamp(startBattery);
            MarkLevelStart();
        }

        // ---------- Puntaje ----------
        public void AddScore(int delta)
        {
            if (delta == 0) return;
            int previous = Score;
            Score = Math.Max(0, Score + delta);
            if (Score != previous)
                ScoreChanged?.Invoke(Score, Score - previous);
        }

        public void RegisterGraffiti() => GraffitiRead++;
        public void RegisterPickup() => PickupsCollected++;
        public void RegisterPerfectStealth() => PerfectStealthSections++;

        // ---------- Intentos ----------
        /// <summary>Resta un intento. Devuelve true si aún quedan intentos.</summary>
        public bool LoseLife()
        {
            Detections++;
            if (Lives > 0)
            {
                Lives--;
                LivesChanged?.Invoke(Lives);
            }
            return Lives > 0;
        }

        // ---------- Batería ----------
        public void DrainBattery(float amount)
        {
            if (amount <= 0f || Battery <= 0f) return;
            Battery = Clamp(Battery - amount);
            BatteryChanged?.Invoke(Battery);
            if (Battery <= 0f)
                BatteryDepleted?.Invoke();
        }

        public void AddBattery(float amount)
        {
            if (amount <= 0f) return;
            Battery = Clamp(Battery + amount);
            BatteryChanged?.Invoke(Battery);
        }

        // ---------- Ruta ----------
        /// <summary>La ruta se guarda una sola vez (GDD: "La ruta se guarda una sola vez").</summary>
        public bool TryChooseRoute(Route route)
        {
            if (Route != Route.None || route == Route.None) return false;
            Route = route;
            RouteChosen?.Invoke(route);
            return true;
        }

        // ---------- Objetos únicos (grafitis, cargadores) ----------
        public bool IsCollected(string id) => collected.Contains(id);
        public bool TryCollect(string id) => !string.IsNullOrEmpty(id) && collected.Add(id);

        // ---------- Reintento de nivel tras Game Over ----------
        public void MarkLevelStart()
        {
            levelStart = new Snapshot
            {
                score = Score,
                battery = Battery,
                detections = Detections,
                graffiti = GraffitiRead,
                pickups = PickupsCollected,
                perfect = PerfectStealthSections,
                collected = new List<string>(collected)
            };
        }

        /// <summary>Restaura el estado del inicio del nivel con intentos completos.</summary>
        public void RestoreLevelStart()
        {
            Score = levelStart.score;
            Battery = Math.Max(levelStart.battery, 1f);
            Detections = levelStart.detections;
            GraffitiRead = levelStart.graffiti;
            PickupsCollected = levelStart.pickups;
            PerfectStealthSections = levelStart.perfect;
            collected.Clear();
            collected.UnionWith(levelStart.collected);
            Lives = MaxLives;
            ScoreChanged?.Invoke(Score, 0);
            LivesChanged?.Invoke(Lives);
            BatteryChanged?.Invoke(Battery);
        }

        private static float Clamp(float value) => value < 0f ? 0f : (value > 100f ? 100f : value);

        private struct Snapshot
        {
            public int score, detections, graffiti, pickups, perfect;
            public float battery;
            public List<string> collected;
        }
    }
}
