using UnityEngine;
using UnityEngine.Events;

namespace GTP
{
    /// <summary>Cerradura (GDD §4.8): se fuerza con el minijuego de ganzúa para abrir el paso.</summary>
    public sealed class LockedGate : MonoBehaviour, IInteractable
    {
        [SerializeField] private GameObject barrier;
        [SerializeField] private UnityEvent onOpened = new UnityEvent();

        private bool open;

        public string Prompt => "Forzar cerradura";
        public bool CanInteract => !open;
        public UnityEvent OnOpened => onOpened;

        public void Configure(GameObject blockingObject) => barrier = blockingObject;

        public void Interact(PlayerInteractor interactor)
        {
            if (open) return;
            GameUI.Instance.Minigame.StartLockpick(success =>
            {
                if (!success)
                {
                    GameUI.Instance.Hud.ShowThought("Se resbaló la ganzúa... otra vez.", 2f);
                    return;
                }

                open = true;
                GameManager gm = GameManager.Instance;
                if (gm != null) gm.Session.AddScore(gm.Config.minigamePoints);
                if (barrier != null) barrier.SetActive(false);
                GameUI.Instance.Hud.ShowThought("¡Abierta! Al callejón.", 2f);
                onOpened.Invoke();
            });
        }
    }
}
