using UnityEngine;

namespace GTP
{
    /// <summary>
    /// Cono de visión de un policía (GDD §4.9). Detecta al jugador por distancia, ángulo y
    /// línea de visión (los obstáculos lo cubren). Dibuja el cono recortado por obstáculos.
    /// Mientras ve al jugador, detiene la patrulla y gira hacia él.
    /// </summary>
    public sealed class PoliceVisionCone : ThreatDetector
    {
        [SerializeField, Min(1f)] private float viewDistance = 7f;
        [SerializeField, Range(10f, 180f)] private float viewAngle = 70f;
        [SerializeField] private float eyeHeight = 0.2f;
        [SerializeField] private MeshFilter coneMeshFilter;
        [SerializeField, Range(6, 60)] private int coneSegments = 24;
        [SerializeField] private Color calmColor = new Color(1f, 0.9f, 0.2f, 0.28f);
        [SerializeField] private Color alertColor = new Color(1f, 0.15f, 0.1f, 0.45f);

        private PatrolRoute patrol;
        private Mesh coneMesh;
        private Vector3[] vertices;
        private Color[] colors;

        public void Configure(float distance, float angle, MeshFilter meshFilter)
        {
            viewDistance = distance;
            viewAngle = angle;
            coneMeshFilter = meshFilter;
        }

        private void Awake()
        {
            patrol = GetComponent<PatrolRoute>();
            if (coneMeshFilter == null) return;

            coneMesh = new Mesh { name = "ConoVision" };
            coneMesh.MarkDynamic();
            vertices = new Vector3[coneSegments + 2];
            colors = new Color[vertices.Length];
            var triangles = new int[coneSegments * 3];
            for (int i = 0; i < coneSegments; i++)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = i + 2;
            }
            coneMesh.vertices = vertices;
            coneMesh.triangles = triangles;
            coneMeshFilter.sharedMesh = coneMesh;
        }

        protected override bool CanSee(PlayerStealth player)
        {
            Vector3 eye = transform.position + Vector3.up * eyeHeight;
            Vector3 toPlayer = player.transform.position - transform.position;
            toPlayer.y = 0f;

            float range = viewDistance * player.DetectionRangeMultiplier;
            if (toPlayer.sqrMagnitude > range * range) return false;
            if (Vector3.Angle(transform.forward, toPlayer) > viewAngle * 0.5f) return false;
            return HasLineOfSight(eye, player);
        }

        protected override void Update()
        {
            base.Update();

            if (patrol != null)
                patrol.Hold = SeesPlayer || Suspicion01 > 0.25f;

            PlayerStealth player = PlayerStealth.Current;
            if (SeesPlayer && player != null)
            {
                Vector3 look = player.transform.position - transform.position;
                look.y = 0f;
                if (look.sqrMagnitude > 0.01f)
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(look), 240f * Time.deltaTime);
            }
        }

        private void LateUpdate()
        {
            if (coneMesh == null) return;

            // Vértices en espacio local del objeto del cono (hijo sin rotación propia).
            Transform coneTransform = coneMeshFilter.transform;
            Vector3 origin = transform.position + Vector3.up * eyeHeight;
            Color color = Color.Lerp(calmColor, alertColor, Suspicion01);

            vertices[0] = coneTransform.InverseTransformPoint(transform.position + Vector3.up * 0.05f);
            colors[0] = color;
            float half = viewAngle * 0.5f;
            for (int i = 0; i <= coneSegments; i++)
            {
                float angle = -half + viewAngle * i / coneSegments;
                Vector3 dir = Quaternion.AngleAxis(angle, Vector3.up) * transform.forward;
                float dist = viewDistance;
                if (Physics.Raycast(origin, dir, out RaycastHit hit, viewDistance, ~0, QueryTriggerInteraction.Ignore)
                    && hit.collider.GetComponentInParent<PlayerStealth>() == null)
                    dist = hit.distance;
                Vector3 world = transform.position + dir * dist + Vector3.up * 0.05f;
                vertices[i + 1] = coneTransform.InverseTransformPoint(world);
                colors[i + 1] = new Color(color.r, color.g, color.b, color.a * 0.35f);
            }

            coneMesh.vertices = vertices;
            coneMesh.colors = colors;
            coneMesh.RecalculateBounds();
        }

        private void OnDestroy()
        {
            if (coneMesh != null) Destroy(coneMesh);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Vector3 left = Quaternion.AngleAxis(-viewAngle * 0.5f, Vector3.up) * transform.forward;
            Vector3 right = Quaternion.AngleAxis(viewAngle * 0.5f, Vector3.up) * transform.forward;
            Gizmos.DrawLine(transform.position, transform.position + left * viewDistance);
            Gizmos.DrawLine(transform.position, transform.position + right * viewDistance);
        }
    }
}
