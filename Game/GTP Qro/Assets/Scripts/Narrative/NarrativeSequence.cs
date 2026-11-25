using UnityEngine;
using UnityEngine.Events;

namespace GTP
{
    /// <summary>
    /// Escena narrativa disparada por evento (una zona, el inicio del nivel o el final de otra acción).
    /// Ejemplos: inicio del Cuarto, redada, revelación en casa de la novia.
    /// </summary>
    public sealed class NarrativeSequence : MonoBehaviour
    {
        [SerializeField] private DialogueLine[] lines = new DialogueLine[0];
        [SerializeField] private bool playOnStart;
        [SerializeField] private UnityEvent onFinished = new UnityEvent();

        private bool played;

        public UnityEvent OnFinished => onFinished;

        public void Configure(DialogueLine[] dialogue, bool onStart)
        {
            lines = dialogue;
            playOnStart = onStart;
        }

        private void Start()
        {
            if (playOnStart) Play();
        }

        /// <summary>Evento: reproduce la secuencia una sola vez.</summary>
        public void Play()
        {
            if (played) return;
            played = true;
            GameUI.Instance.Dialogue.Play(lines, onFinished.Invoke);
        }
    }
}
