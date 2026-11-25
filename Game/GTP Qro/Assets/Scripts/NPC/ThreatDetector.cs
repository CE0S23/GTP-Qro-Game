using System.Collections.Generic;
using UnityEngine;

namespace GTP
{
    /// <summary>
    /// Base de toda amenaza que detecta por exposición: acumula sospecha mientras ve al jugador
    /// y, al llegar al tiempo de detección (GDD: 2 s), le quita un intento.
    /// Las subclases solo definen CÓMO ven (cono, foco...).
    /// </summary>
    public abstract class ThreatDetector : MonoBehaviour
    {
        private static readonly List<ThreatDetector> active = new List<ThreatDetector>();

        /// <summary>Sospecha máxima (0..1) de todas las amenazas activas, para la UI.</summary>
        public static float MaxSuspicion01
        {
            get
            {
                float max = 0f;
                for (int i = 0; i < active.Count; i++)
                    max = Mathf.Max(max, active[i].Suspicion01);
                return max;
            }
        }

        [SerializeField] protected string caughtMessage = "Te detectaron.";
        [SerializeField, Min(0f)] private float suspicionDecayRate = 0.75f;
        [SerializeField] private GameObject alertIndicator;

        private float detectionTime = 2f;
        private float suspicion;

        public bool SeesPlayer { get; private set; }
        public float Suspicion01 => Mathf.Clamp01(suspicion / detectionTime);

        public void SetAlertIndicator(GameObject indicator) => alertIndicator = indicator;
        public void SetCaughtMessage(string message) => caughtMessage = message;

        protected virtual void OnEnable()
        {
            active.Add(this);
            GameManager.PlayerRespawned += ResetSuspicion;
        }

        protected virtual void OnDisable()
        {
            active.Remove(this);
            GameManager.PlayerRespawned -= ResetSuspicion;
            SeesPlayer = false;
            suspicion = 0f;
        }

        protected virtual void Start()
        {
            if (GameManager.Instance != null)
                detectionTime = GameManager.Instance.Config.detectionTime;
            if (alertIndicator != null) alertIndicator.SetActive(false);
        }

        protected virtual void Update()
        {
            PlayerStealth player = PlayerStealth.Current;
            GameManager gm = GameManager.Instance;
            bool playing = gm == null || gm.State == GameState.Exploracion;

            SeesPlayer = playing && player != null && player.IsDetectable && CanSee(player);

            if (SeesPlayer)
                suspicion += Time.deltaTime;
            else
                suspicion = Mathf.Max(0f, suspicion - suspicionDecayRate * Time.deltaTime);

            if (alertIndicator != null && alertIndicator.activeSelf != (suspicion > 0f))
                alertIndicator.SetActive(suspicion > 0f);

            if (suspicion >= detectionTime)
            {
                suspicion = 0f;
                gm?.ReportPlayerCaught(caughtMessage);
            }
        }

        /// <summary>Devuelve true si la amenaza percibe al jugador este frame.</summary>
        protected abstract bool CanSee(PlayerStealth player);

        protected virtual void ResetSuspicion()
        {
            suspicion = 0f;
            SeesPlayer = false;
        }

        /// <summary>Línea de visión sin obstáculos sólidos (ignora triggers).</summary>
        protected static bool HasLineOfSight(Vector3 from, PlayerStealth player)
        {
            Vector3 to = player.EyePoint;
            Vector3 dir = to - from;
            float dist = dir.magnitude;
            if (dist < 0.01f) return true;
            if (Physics.Raycast(from, dir / dist, out RaycastHit hit, dist, ~0, QueryTriggerInteraction.Ignore))
                return hit.collider.GetComponentInParent<PlayerStealth>() == player;
            return true;
        }
    }
}
