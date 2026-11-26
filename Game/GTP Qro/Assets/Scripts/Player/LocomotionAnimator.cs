using UnityEngine;

namespace GTP
{
    /// <summary>
    /// Alimenta el Blend Tree de locomoción (rúbrica DE 4.1) con la velocidad horizontal real del personaje.
    /// El Animator Controller (Assets/Animations/Locomocion.controller, generado por el constructor) tiene un
    /// Blend Tree 1D por el parámetro "Velocidad" que mezcla Reposo (0) → Caminar (4) → Correr (6.4).
    /// Funciona para el jugador y para los NPC (policías, perseguidores, compañeros) porque mide el
    /// desplazamiento del objeto raíz, sin importar si lo mueve un Rigidbody o un MovePosition cinemático.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public sealed class LocomotionAnimator : MonoBehaviour
    {
        public static readonly int SpeedParam = Animator.StringToHash("Velocidad");

        [Tooltip("Objeto que se desplaza (normalmente el padre con el Rigidbody).")]
        [SerializeField] private Transform mover;
        [SerializeField, Min(0f)] private float dampTime = 0.12f;

        private Animator animator;
        private Vector3 lastPosition;

        public float MeasuredSpeed { get; private set; }

        public void Configure(Transform movingRoot) => mover = movingRoot;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            if (mover == null) mover = transform.parent != null ? transform.parent : transform;
            lastPosition = mover.position;
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            if (dt <= 0f) return; // juego en pausa / diálogo: el Animator también se congela

            Vector3 delta = mover.position - lastPosition;
            delta.y = 0f;
            lastPosition = mover.position;

            // Teletransporte (reaparición, compañero que alcanza al jugador): no cuenta como velocidad.
            float speed = delta.magnitude / dt;
            if (speed > 20f) speed = 0f;

            MeasuredSpeed = speed;
            animator.SetFloat(SpeedParam, speed, dampTime, dt);
        }
    }
}
