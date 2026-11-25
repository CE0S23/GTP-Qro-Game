using UnityEngine;

namespace GTP
{
    /// <summary>Cámara cenital inclinada que sigue al jugador con suavizado (sin rotar).</summary>
    public sealed class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(0f, 14f, -10f);
        [SerializeField, Min(0.01f)] private float smoothTime = 0.15f;

        private Vector3 velocity;

        public void Configure(Transform followTarget, Vector3 cameraOffset)
        {
            target = followTarget;
            offset = cameraOffset;
        }

        private void Start()
        {
            if (target == null) return;
            transform.position = target.position + offset;
            transform.rotation = Quaternion.LookRotation(-offset, Vector3.up);
        }

        private void LateUpdate()
        {
            if (target == null) return;
            transform.position = Vector3.SmoothDamp(transform.position, target.position + offset, ref velocity, smoothTime,
                Mathf.Infinity, Time.unscaledDeltaTime);
        }
    }
}
