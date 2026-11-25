using UnityEngine;

namespace GTP
{
    /// <summary>Contenedor de basura (GDD §4.8): con E el jugador se esconde y las amenazas no lo ven.</summary>
    public sealed class HidingSpot : MonoBehaviour, IInteractable
    {
        private PlayerStealth occupant;

        public string Prompt => occupant != null ? "Salir del contenedor" : "Esconderse";
        public bool CanInteract => true;

        public void Interact(PlayerInteractor interactor)
        {
            PlayerStealth stealth = interactor.Stealth;
            if (stealth == null) return;

            if (occupant != null)
            {
                occupant.ExitHiding();
                occupant = null;
                return;
            }

            occupant = stealth;
            stealth.EnterHiding(this);
            GameUI.Instance?.Hud.ShowThought("Aquí no me ven... (E para salir)", 2f);
        }

        private void OnEnable() => GameManager.PlayerRespawned += ClearOccupant;
        private void OnDisable() => GameManager.PlayerRespawned -= ClearOccupant;
        private void ClearOccupant() => occupant = null;
    }
}
