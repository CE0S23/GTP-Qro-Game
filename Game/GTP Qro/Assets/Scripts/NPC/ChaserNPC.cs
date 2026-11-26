using UnityEngine;

namespace GTP
{
    /// <summary>
    /// Perseguidor (asaltante del Barrio o policía del altercado). Se activa por cercanía o por evento,
    /// persigue al jugador y le quita un intento si lo alcanza. Se rinde por distancia o con GiveUp().
    /// Sigue una ruta del NavMesh (<see cref="NavSteering"/>) para rodear autos y esquinas.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public sealed class ChaserNPC : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float speed = 5f;
        [SerializeField, Min(0f)] private float activationRadius = 6f;
        [Tooltip("Si el jugador se aleja más que esto del punto de inicio, el perseguidor se rinde.")]
        [SerializeField, Min(1f)] private float leashRadius = 22f;
        [SerializeField, Min(0.3f)] private float catchDistance = 1.1f;
        [SerializeField] private string caughtMessage = "Te alcanzaron.";
        [SerializeField] private string activationThought = "";
        [SerializeField] private bool activateOnlyByEvent;
        [SerializeField] private bool reactivateAfterRespawn;

        private readonly NavSteering nav = new NavSteering();
        private Rigidbody body;
        private Vector3 home;
        private bool chasing;
        private bool defeated;
        private bool wasActivated;

        public bool IsChasing => chasing;
        public bool IsUsingNavMesh => nav.HasNavPath;

        public void Configure(float moveSpeed, float activation, float leash, string caught, string thought,
                              bool onlyByEvent, bool reactivate)
        {
            speed = moveSpeed;
            activationRadius = activation;
            leashRadius = leash;
            caughtMessage = caught;
            activationThought = thought;
            activateOnlyByEvent = onlyByEvent;
            reactivateAfterRespawn = reactivate;
        }

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            home = transform.position;
        }

        private void OnEnable() => GameManager.PlayerRespawned += ResetChaser;
        private void OnDisable() => GameManager.PlayerRespawned -= ResetChaser;

        /// <summary>Evento: empieza la persecución (p. ej., desde un TriggerZone).</summary>
        public void Activate()
        {
            if (defeated || chasing) return;
            chasing = true;
            wasActivated = true;
            if (!string.IsNullOrEmpty(activationThought))
                GameUI.Instance?.Hud.ShowThought(activationThought, 2.5f);
        }

        /// <summary>Evento: el jugador llegó a zona segura; el perseguidor abandona.</summary>
        public void GiveUp()
        {
            defeated = true;
            chasing = false;
        }

        private void ResetChaser()
        {
            body.linearVelocity = Vector3.zero;
            body.position = home;
            transform.position = home;
            nav.Invalidate();
            chasing = !defeated && reactivateAfterRespawn && wasActivated;
        }

        private void FixedUpdate()
        {
            PlayerStealth player = PlayerStealth.Current;
            GameManager gm = GameManager.Instance;
            bool playing = gm == null || gm.State == GameState.Exploracion;

            if (!playing || player == null || defeated)
            {
                body.linearVelocity = Vector3.zero;
                if (defeated) ReturnHome();
                return;
            }

            Vector3 toPlayer = player.transform.position - transform.position;
            toPlayer.y = 0f;

            if (!chasing && !activateOnlyByEvent && player.IsDetectable && toPlayer.magnitude <= activationRadius)
                Activate();

            if (!chasing)
            {
                body.linearVelocity = Vector3.zero;
                return;
            }

            Vector3 playerFromHome = player.transform.position - home;
            playerFromHome.y = 0f;
            if (playerFromHome.magnitude > leashRadius || !player.IsDetectable)
            {
                // Pierde el rastro: si el jugador está escondido o lejos, vuelve a casa y se puede reactivar.
                chasing = false;
                body.linearVelocity = Vector3.zero;
                return;
            }

            if (toPlayer.magnitude <= catchDistance)
            {
                body.linearVelocity = Vector3.zero;
                gm?.ReportPlayerCaught(caughtMessage);
                return;
            }

            Vector3 dir = nav.DirectionTo(transform.position, player.transform.position);
            if (dir == Vector3.zero) dir = toPlayer.normalized;
            body.linearVelocity = dir * speed;
            transform.rotation = Quaternion.LookRotation(dir);
        }

        private void ReturnHome()
        {
            Vector3 toHome = home - transform.position;
            toHome.y = 0f;
            if (toHome.magnitude <= 0.2f)
            {
                body.linearVelocity = Vector3.zero;
                return;
            }
            Vector3 dir = nav.DirectionTo(transform.position, home);
            if (dir == Vector3.zero) dir = toHome.normalized;
            body.linearVelocity = dir * speed * 0.5f;
        }
    }
}
