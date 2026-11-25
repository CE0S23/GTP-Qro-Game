using UnityEngine;
using UnityEngine.InputSystem;

namespace GTP
{
    /// <summary>Contrato de los objetos con los que el jugador interactúa con la tecla E.</summary>
    public interface IInteractable
    {
        string Prompt { get; }
        bool CanInteract { get; }
        Transform transform { get; }
        void Interact(PlayerInteractor interactor);
    }

    /// <summary>Busca el objeto interactuable más cercano, muestra el aviso y ejecuta la acción con E.</summary>
    [DisallowMultipleComponent]
    public sealed class PlayerInteractor : MonoBehaviour
    {
        [SerializeField, Min(0.5f)] private float radius = 1.8f;

        private readonly Collider[] hits = new Collider[16];
        private IInteractable current;

        public PlayerStealth Stealth { get; private set; }

        private void Awake()
        {
            Stealth = GetComponent<PlayerStealth>();
        }

        private void Update()
        {
            GameManager gm = GameManager.Instance;
            bool canAct = gm == null || gm.CanPlayerAct;

            current = canAct ? FindNearest() : null;
            GameUI.Instance?.Hud.SetPrompt(current != null ? "[E] " + current.Prompt : "");

            Keyboard keyboard = Keyboard.current;
            if (current != null && keyboard != null && keyboard.eKey.wasPressedThisFrame)
                current.Interact(this);
        }

        private IInteractable FindNearest()
        {
            // Escondido: la única interacción válida es salir del contenedor.
            if (Stealth != null && Stealth.IsHidden)
                return Stealth.CurrentHidingSpot;

            int count = Physics.OverlapSphereNonAlloc(transform.position, radius, hits, ~0, QueryTriggerInteraction.Collide);
            IInteractable best = null;
            float bestDistance = float.MaxValue;
            for (int i = 0; i < count; i++)
            {
                IInteractable candidate = hits[i].GetComponentInParent<IInteractable>();
                if (candidate == null || !candidate.CanInteract) continue;
                float distance = (hits[i].ClosestPoint(transform.position) - transform.position).sqrMagnitude;
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = candidate;
                }
            }
            return best;
        }

        private void OnDisable()
        {
            GameUI.Instance?.Hud.SetPrompt("");
        }
    }
}
