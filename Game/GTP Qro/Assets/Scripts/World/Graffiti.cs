using UnityEngine;

namespace GTP
{
    /// <summary>
    /// Grafiti reflexivo (GDD §4.8): mensaje explícito contra el uso de drogas.
    /// Leerlo por primera vez suma puntaje (es la exploración que el juego premia).
    /// </summary>
    public sealed class Graffiti : MonoBehaviour, IInteractable
    {
        [SerializeField] private string message = "NO ES UNA SALIDA";
        [SerializeField, TextArea] private string reflection = "Hay otras formas de sentirse mejor.";

        public string Prompt => "Leer grafiti";
        public bool CanInteract => true;

        private string Id => gameObject.scene.name + "/" + name;

        public void Configure(string graffitiText, string thought)
        {
            message = graffitiText;
            reflection = thought;
        }

        public void Interact(PlayerInteractor interactor)
        {
            GameManager gm = GameManager.Instance;
            if (gm != null && gm.Session.TryCollect(Id))
            {
                gm.Session.RegisterGraffiti();
                gm.Session.AddScore(gm.Config.graffitiPoints);
                GameEvents.GraffitiRead.Raise(transform.position);
            }

            GameUI.Instance.Dialogue.Play(new[]
            {
                new DialogueLine("Grafiti", "«" + message + "»"),
                new DialogueLine("Pensamiento", reflection)
            }, null);
        }
    }
}
