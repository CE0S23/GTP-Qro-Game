using Unity.Cinemachine;
using UnityEngine;

namespace GTP
{
    /// <summary>
    /// Cámaras inteligentes con Cinemachine (rúbrica DE 2.1).
    /// - <see cref="followCamera"/>: CinemachineCamera cenital con Follow al jugador (CinemachineFollow con amortiguación).
    /// - <see cref="closeUpCamera"/>: plano más cercano que toma el control en diálogos, decisión y minijuegos;
    ///   el CinemachineBrain hace la transición (blend EaseInOut) entre ambas.
    /// Ambas tienen CinemachineImpulseListener: la sacudida (Camera Shake) se dispara desde
    /// <see cref="FeedbackFX"/> cuando el jugador es detectado o alcanzado.
    /// </summary>
    public sealed class CameraDirector : MonoBehaviour
    {
        private const int ActivePriority = 20;
        private const int IdlePriority = 0;

        [SerializeField] private CinemachineCamera followCamera;
        [SerializeField] private CinemachineCamera closeUpCamera;

        public bool CloseUpActive { get; private set; }
        public CinemachineCamera FollowCamera => followCamera;
        public CinemachineCamera CloseUpCamera => closeUpCamera;

        public void Configure(CinemachineCamera follow, CinemachineCamera closeUp)
        {
            followCamera = follow;
            closeUpCamera = closeUp;
        }

        private void Start() => Apply(false);

        private void OnEnable() => GameManager.PlayerRespawned += SnapToPlayer;
        private void OnDisable() => GameManager.PlayerRespawned -= SnapToPlayer;

        /// <summary>Al reaparecer en el punto de control la cámara corta directo (sin barrer todo el nivel).</summary>
        private void SnapToPlayer()
        {
            if (followCamera != null) followCamera.PreviousStateIsValid = false;
            if (closeUpCamera != null) closeUpCamera.PreviousStateIsValid = false;
        }

        private void Update()
        {
            GameManager gm = GameManager.Instance;
            bool wantCloseUp = gm != null &&
                               (gm.State == GameState.Narrativa || gm.State == GameState.Decision || gm.State == GameState.Minijuego);
            if (wantCloseUp != CloseUpActive) Apply(wantCloseUp);
        }

        private void Apply(bool closeUp)
        {
            CloseUpActive = closeUp;
            if (closeUpCamera != null) closeUpCamera.Priority = closeUp ? ActivePriority : IdlePriority;
            if (followCamera != null) followCamera.Priority = closeUp ? IdlePriority : ActivePriority - 10;
        }
    }
}
