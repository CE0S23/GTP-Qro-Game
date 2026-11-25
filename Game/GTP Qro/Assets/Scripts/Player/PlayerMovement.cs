using UnityEngine;
using UnityEngine.InputSystem;

namespace GTP
{
    /// <summary>
    /// Movimiento cardinal sobre X/Z para la cámara cenital. Acciones: caminar (WASD/flechas),
    /// correr (Shift) y agacharse (C, alterna). Solo maneja física y desplazamiento.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [DisallowMultipleComponent]
    public sealed class PlayerMovement : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float moveSpeed = 4f;
        [Tooltip("Hijo visual que gira hacia la dirección de movimiento (el Rigidbody no rota).")]
        [SerializeField] private Transform facingPivot;
        [SerializeField] private Transform crouchVisual;

        private Rigidbody body;
        private Vector2 moveInput;
        private float runMultiplier = 1.6f;
        private float crouchMultiplier = 0.5f;

        public bool IsRunning { get; private set; }
        public bool IsCrouching { get; private set; }
        public bool IsMoving => moveInput != Vector2.zero;
        public Vector3 Facing { get; private set; } = Vector3.forward;

        public void Configure(Transform facing, Transform crouchTarget)
        {
            facingPivot = facing;
            crouchVisual = crouchTarget;
        }

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
        }

        private void Start()
        {
            GameManager gm = GameManager.Instance;
            if (gm == null) return;
            moveSpeed = gm.Config.walkSpeed;
            runMultiplier = gm.Config.runMultiplier;
            crouchMultiplier = gm.Config.crouchMultiplier;
        }

        private void Update()
        {
            moveInput = Vector2.zero;
            IsRunning = false;

            Keyboard keyboard = Keyboard.current;
            if (keyboard == null || (GameManager.Instance != null && !GameManager.Instance.CanPlayerAct))
                return;

            float horizontal = (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed ? 1f : 0f)
                             - (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed ? 1f : 0f);
            float vertical = (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed ? 1f : 0f)
                           - (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed ? 1f : 0f);

            // Cuatro direcciones: horizontal tiene prioridad si hay dos ejes pulsados.
            moveInput = horizontal != 0f ? new Vector2(horizontal, 0f) : new Vector2(0f, vertical);

            if (keyboard.cKey.wasPressedThisFrame)
                SetCrouching(!IsCrouching);

            IsRunning = moveInput != Vector2.zero && keyboard.shiftKey.isPressed;
            if (IsRunning && IsCrouching)
                SetCrouching(false);

            if (moveInput != Vector2.zero)
            {
                Facing = new Vector3(moveInput.x, 0f, moveInput.y);
                if (facingPivot != null)
                    facingPivot.rotation = Quaternion.LookRotation(Facing, Vector3.up);
            }
        }

        private void FixedUpdate()
        {
            float speed = moveSpeed * (IsRunning ? runMultiplier : 1f) * (IsCrouching ? crouchMultiplier : 1f);
            // Velocidad en unidades/segundo; el motor físico ya aplica el paso de tiempo.
            body.linearVelocity = new Vector3(moveInput.x, 0f, moveInput.y) * speed;
        }

        public void SetCrouching(bool crouching)
        {
            IsCrouching = crouching;
            if (crouchVisual != null)
                crouchVisual.localScale = new Vector3(1f, crouching ? 0.6f : 1f, 1f);
        }

        /// <summary>Coloca al jugador en un punto (reaparición en checkpoint).</summary>
        public void Teleport(Vector3 position)
        {
            moveInput = Vector2.zero;
            body.linearVelocity = Vector3.zero;
            body.position = position;
            transform.position = position;
            SetCrouching(false);
        }

        private void OnDisable()
        {
            moveInput = Vector2.zero;
            if (body != null)
                body.linearVelocity = Vector3.zero;
        }
    }
}
