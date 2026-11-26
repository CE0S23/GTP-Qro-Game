using UnityEngine;
using UnityEngine.AI;

namespace GTP
{
    /// <summary>
    /// Navegación con el paquete AI Navigation (rúbrica AU 3.1). Calcula una ruta sobre el NavMesh
    /// horneado de la escena y devuelve la siguiente esquina hacia la que hay que avanzar, de modo que
    /// los NPC rodean autos, edificios y contenedores en lugar de atorarse.
    /// La ruta se recalcula cada <see cref="repathInterval"/> segundos (el objetivo se mueve y los
    /// obstáculos con NavMeshObstacle pueden abrirse o cerrarse), no cada frame, para ahorrar CPU.
    /// Si la escena no tiene NavMesh, se comporta como antes: línea recta al objetivo.
    /// Es una clase C# simple para que la usen el Rigidbody de cada NPC sin un NavMeshAgent
    /// (así la física y los triggers siguen funcionando igual que en el beta).
    /// </summary>
    public sealed class NavSteering
    {
        private const float SampleDistance = 2f;
        private const float CornerReachedDistance = 0.35f;

        private NavMeshPath path; // se crea al primer uso: Unity no permite crearlo en inicializadores de MonoBehaviour
        private readonly float repathInterval;
        private readonly Vector3[] corners = new Vector3[32];
        private int cornerCount;
        private int cornerIndex;
        private float nextRepathTime;
        private Vector3 lastTarget;

        public NavSteering(float repathSeconds = 0.25f)
        {
            repathInterval = repathSeconds;
        }

        /// <summary>True si la última ruta se calculó sobre el NavMesh (no en línea recta).</summary>
        public bool HasNavPath { get; private set; }

        public int CornerCount => cornerCount;

        public void Invalidate()
        {
            nextRepathTime = 0f;
            cornerCount = 0;
            HasNavPath = false;
        }

        /// <summary>Dirección horizontal normalizada desde <paramref name="from"/> hacia <paramref name="target"/>.</summary>
        public Vector3 DirectionTo(Vector3 from, Vector3 target)
        {
            if (Time.time >= nextRepathTime || (target - lastTarget).sqrMagnitude > 4f)
                Repath(from, target);

            Vector3 next = target;
            if (HasNavPath)
            {
                while (cornerIndex < cornerCount - 1 && Flat(corners[cornerIndex] - from).sqrMagnitude < CornerReachedDistance * CornerReachedDistance)
                    cornerIndex++;
                next = corners[cornerIndex];
            }

            Vector3 dir = Flat(next - from);
            return dir.sqrMagnitude > 0.0001f ? dir.normalized : Vector3.zero;
        }

        private void Repath(Vector3 from, Vector3 target)
        {
            nextRepathTime = Time.time + repathInterval;
            lastTarget = target;
            HasNavPath = false;
            cornerCount = 0;
            cornerIndex = 0;

            if (path == null) path = new NavMeshPath();
            if (!NavMesh.SamplePosition(from, out NavMeshHit start, SampleDistance, NavMesh.AllAreas)) return;
            if (!NavMesh.SamplePosition(target, out NavMeshHit end, SampleDistance, NavMesh.AllAreas)) return;
            if (!NavMesh.CalculatePath(start.position, end.position, NavMesh.AllAreas, path)) return;
            if (path.status == NavMeshPathStatus.PathInvalid) return;

            cornerCount = path.GetCornersNonAlloc(corners);
            if (cornerCount < 2) return;
            HasNavPath = true;
            cornerIndex = 1; // la esquina 0 es la posición actual
        }

        private static Vector3 Flat(Vector3 v) => new Vector3(v.x, 0f, v.z);
    }
}
