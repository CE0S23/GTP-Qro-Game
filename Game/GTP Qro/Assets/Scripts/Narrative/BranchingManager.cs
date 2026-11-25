using UnityEngine;

namespace GTP
{
    /// <summary>
    /// Sistema de decisiones (GDD §4.6). Abre la interfaz con dos opciones, guarda la ruta
    /// una sola vez en la sesión y carga la escena de la ruta elegida.
    /// </summary>
    public sealed class BranchingManager : MonoBehaviour
    {
        [SerializeField] private string question = "¿Qué vas a hacer?";
        [SerializeField] private string badOption = "Seguir buscando";
        [SerializeField] private string goodOption = "Ir con mi novia y mi amigo";

        /// <summary>Evento: muestra la decisión (se llama al terminar el diálogo con Orlando).</summary>
        public void OpenDecision()
        {
            GameUI.Instance.Decision.Show(question, badOption, goodOption, OnChosen);
        }

        private void OnChosen(int option)
        {
            Route route = option == 0 ? Route.Mal : Route.Bien;
            GameManager gm = GameManager.Instance;
            gm.Session.TryChooseRoute(route);
            gm.CompleteLevel(gm.Session.Route == Route.Mal ? SceneNames.RutaMal : SceneNames.RutaBien);
        }
    }
}
