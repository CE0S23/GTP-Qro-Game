using UnityEngine;

namespace GTP
{
    /// <summary>
    /// Qué tan visible es el jugador para las amenazas: agachado reduce el radio de detección,
    /// correr lo aumenta y estar escondido en un contenedor lo vuelve indetectable.
    /// </summary>
    [RequireComponent(typeof(PlayerMovement))]
    [DisallowMultipleComponent]
    public sealed class PlayerStealth : MonoBehaviour
    {
        public static PlayerStealth Current { get; private set; }

        [SerializeField] private GameObject visualRoot;

        private PlayerMovement movement;
        private float crouchMultiplier = 0.6f;
        private float runMultiplier = 1.25f;

        public bool IsHidden { get; private set; }
        public HidingSpot CurrentHidingSpot { get; private set; }
        public PlayerMovement Movement => movement;

        /// <summary>Punto que usan los rayos de línea de visión.</summary>
        public Vector3 EyePoint => transform.position + Vector3.up * 0.2f;

        public float DetectionRangeMultiplier
        {
            get
            {
                if (movement.IsCrouching) return crouchMultiplier;
                if (movement.IsRunning) return runMultiplier;
                return 1f;
            }
        }

        public bool IsDetectable => !IsHidden && isActiveAndEnabled;

        public void Configure(GameObject visual) => visualRoot = visual;

        private void Awake()
        {
            movement = GetComponent<PlayerMovement>();
            Current = this;
        }

        private void Start()
        {
            if (GameManager.Instance == null) return;
            crouchMultiplier = GameManager.Instance.Config.crouchDetectionMultiplier;
            runMultiplier = GameManager.Instance.Config.runDetectionMultiplier;
        }

        private void OnDestroy()
        {
            if (Current == this) Current = null;
        }

        public void EnterHiding(HidingSpot spot)
        {
            IsHidden = true;
            CurrentHidingSpot = spot;
            movement.enabled = false;
            if (visualRoot != null) visualRoot.SetActive(false);
        }

        public void ExitHiding()
        {
            IsHidden = false;
            CurrentHidingSpot = null;
            movement.enabled = true;
            if (visualRoot != null) visualRoot.SetActive(true);
        }
    }
}
