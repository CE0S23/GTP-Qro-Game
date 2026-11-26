using UnityEngine;

namespace GTP
{
    /// <summary>
    /// Patrulla por puntos de ruta (en bucle o ida y vuelta) con espera en cada punto.
    /// Se puede detener temporalmente con <see cref="Hold"/> (p. ej., cuando el policía ve al jugador).
    /// Con <c>useNavMesh</c> camina entre puntos siguiendo el NavMesh y rodea obstáculos
    /// (incluidos los NavMeshObstacle que se mueven o desaparecen durante el juego).
    /// </summary>
    public sealed class PatrolRoute : MonoBehaviour
    {
        [SerializeField] private Vector3[] waypoints = new Vector3[0];
        [SerializeField, Min(0f)] private float speed = 2f;
        [SerializeField, Min(0f)] private float waitAtPoint = 1f;
        [SerializeField] private bool pingPong;
        [SerializeField] private bool rotateToMovement = true;
        [SerializeField] private bool useNavMesh;

        private readonly NavSteering nav = new NavSteering(0.5f);

        private Rigidbody body;
        private int index;
        private int direction = 1;
        private float waitTimer;
        private Vector3 startPosition;
        private Quaternion startRotation;

        public bool Hold { get; set; }

        public void Configure(Vector3[] points, float moveSpeed, float wait, bool usePingPong, bool rotate = true,
                              bool navMesh = false)
        {
            useNavMesh = navMesh;
            waypoints = points;
            speed = moveSpeed;
            waitAtPoint = wait;
            pingPong = usePingPong;
            rotateToMovement = rotate;
        }

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            startPosition = transform.position;
            startRotation = transform.rotation;
        }

        private void OnEnable() => GameManager.PlayerRespawned += ResetRoute;
        private void OnDisable() => GameManager.PlayerRespawned -= ResetRoute;

        private void ResetRoute()
        {
            index = 0;
            direction = 1;
            waitTimer = 0f;
            Hold = false;
            nav.Invalidate();
            MoveTo(startPosition);
            transform.rotation = startRotation;
        }

        private void FixedUpdate()
        {
            if (Hold || waypoints == null || waypoints.Length == 0) return;

            if (waitTimer > 0f)
            {
                waitTimer -= Time.fixedDeltaTime;
                return;
            }

            Vector3 target = waypoints[index];
            target.y = transform.position.y;
            Vector3 position = transform.position;
            Vector3 next = Vector3.MoveTowards(position, target, speed * Time.fixedDeltaTime);
            if (useNavMesh && (target - position).sqrMagnitude > 0.25f)
            {
                Vector3 dir = nav.DirectionTo(position, target);
                if (dir != Vector3.zero)
                    next = position + dir * Mathf.Min(speed * Time.fixedDeltaTime, (target - position).magnitude);
            }
            Vector3 delta = next - position;

            if (rotateToMovement && delta.sqrMagnitude > 0.000001f)
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(delta), 360f * Time.fixedDeltaTime);

            MoveTo(next);

            if ((next - target).sqrMagnitude < 0.0025f)
            {
                waitTimer = waitAtPoint;
                AdvanceIndex();
                nav.Invalidate();
            }
        }

        private void AdvanceIndex()
        {
            if (waypoints.Length < 2) return;
            if (!pingPong)
            {
                index = (index + 1) % waypoints.Length;
                return;
            }
            if (index + direction < 0 || index + direction >= waypoints.Length)
                direction = -direction;
            index += direction;
        }

        private void MoveTo(Vector3 position)
        {
            if (body != null && body.isKinematic)
                body.MovePosition(position);
            else
                transform.position = position;
        }

        private void OnDrawGizmosSelected()
        {
            if (waypoints == null) return;
            Gizmos.color = Color.cyan;
            for (int i = 0; i < waypoints.Length; i++)
            {
                Gizmos.DrawWireSphere(waypoints[i], 0.3f);
                if (i + 1 < waypoints.Length) Gizmos.DrawLine(waypoints[i], waypoints[i + 1]);
            }
        }
    }
}
