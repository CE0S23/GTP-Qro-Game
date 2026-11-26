using UnityEngine;

namespace GTP
{
    /// <summary>
    /// Compañero que sigue al jugador (Orlando en la Ruta del Mal; novia y amigo en la Ruta del Bien).
    /// No colisiona con el jugador para no empujarlo; se teletransporta si se queda muy atrás.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public sealed class CompanionFollower : MonoBehaviour
    {
        [SerializeField] private bool followOnStart;
        [SerializeField, Min(0.5f)] private float followDistance = 2f;
        [SerializeField, Min(0f)] private float speed = 5.5f;
        [SerializeField, Min(2f)] private float teleportDistance = 14f;
        [SerializeField] private Vector3 sideOffset = new Vector3(-1f, 0f, -1f);

        private readonly NavSteering nav = new NavSteering(0.4f);
        private Rigidbody body;
        private bool following;

        public void Configure(bool startFollowing, Vector3 offset)
        {
            followOnStart = startFollowing;
            sideOffset = offset;
        }

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
        }

        private void Start()
        {
            following = followOnStart;
            IgnorePlayerCollision();
        }

        private void IgnorePlayerCollision()
        {
            PlayerStealth player = PlayerStealth.Current;
            if (player == null) return;
            Collider mine = GetComponent<Collider>();
            Collider theirs = player.GetComponent<Collider>();
            if (mine != null && theirs != null)
                Physics.IgnoreCollision(mine, theirs);
        }

        /// <summary>Evento: empieza a seguir al jugador.</summary>
        public void StartFollowing()
        {
            following = true;
            IgnorePlayerCollision();
        }

        public void StopFollowing() => following = false;

        private void FixedUpdate()
        {
            PlayerStealth player = PlayerStealth.Current;
            if (!following || player == null)
            {
                body.linearVelocity = Vector3.zero;
                return;
            }

            Vector3 target = player.transform.position + sideOffset;
            Vector3 delta = target - transform.position;
            delta.y = 0f;
            float distance = delta.magnitude;

            if (distance > teleportDistance)
            {
                body.position = new Vector3(target.x, transform.position.y, target.z);
                body.linearVelocity = Vector3.zero;
                return;
            }

            if (distance <= followDistance * 0.5f)
            {
                body.linearVelocity = Vector3.zero;
                return;
            }

            float factor = Mathf.Clamp01((distance - followDistance * 0.5f) / followDistance);
            // Rodea obstáculos con el NavMesh (si existe); si no, línea recta.
            Vector3 dir = nav.DirectionTo(transform.position, target);
            if (dir == Vector3.zero) dir = delta.normalized;
            body.linearVelocity = dir * speed * Mathf.Max(0.4f, factor);
            transform.rotation = Quaternion.LookRotation(dir);
        }
    }
}
