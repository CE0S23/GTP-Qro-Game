using UnityEngine;
using UnityEngine.Events;

namespace GTP
{
    /// <summary>
    /// Objeto o personaje con el que se habla con E (celular, puerta del Punto, Orlando, novia...).
    /// Al terminar el diálogo dispara un UnityEvent (desbloquear salida, abrir decisión, seguir...).
    /// </summary>
    public sealed class NarrativeInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private string prompt = "Hablar";
        [SerializeField] private DialogueLine[] lines = new DialogueLine[0];
        [Tooltip("Diálogo corto si se vuelve a interactuar después de la primera vez.")]
        [SerializeField] private DialogueLine[] repeatLines = new DialogueLine[0];
        [SerializeField] private bool available = true;
        [SerializeField] private UnityEvent onFirstFinished = new UnityEvent();

        private bool done;

        public string Prompt => prompt;
        public bool CanInteract => available && (!done || repeatLines.Length > 0);
        public UnityEvent OnFirstFinished => onFirstFinished;

        public void Configure(string interactionPrompt, DialogueLine[] dialogue, DialogueLine[] repeat, bool startsAvailable)
        {
            prompt = interactionPrompt;
            lines = dialogue;
            repeatLines = repeat ?? new DialogueLine[0];
            available = startsAvailable;
        }

        /// <summary>Evento: habilita la interacción (p. ej., el amigo tras hablar con la novia).</summary>
        public void SetAvailable(bool value) => available = value;

        public void Interact(PlayerInteractor interactor)
        {
            if (done)
            {
                GameUI.Instance.Dialogue.Play(repeatLines, null);
                return;
            }

            GameUI.Instance.Dialogue.Play(lines, () =>
            {
                done = true;
                onFirstFinished.Invoke();
            });
        }
    }
}
