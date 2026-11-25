using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace GTP
{
    /// <summary>Caja de diálogo / pensamientos. Pausa la tensión mientras está abierta (estado Narrativa).</summary>
    public sealed class DialogueBox : MonoBehaviour
    {
        private GameObject root;
        private Text speakerText, bodyText;
        private DialogueLine[] lines;
        private int index;
        private Action onComplete;
        private bool keepState;
        private int openedFrame;

        public bool IsOpen => root.activeSelf;

        public static DialogueBox Build(Transform canvas)
        {
            var box = canvas.gameObject.AddComponent<DialogueBox>();
            Image panel = UIFactory.Panel(canvas, "Caja", new Color(0.03f, 0.03f, 0.07f, 0.93f),
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-760f, 30f), new Vector2(760f, 290f));
            box.root = panel.gameObject;
            box.speakerText = UIFactory.Label(panel.transform, "Hablante", "", 32, TextAnchor.UpperLeft, new Color(1f, 0.8f, 0.3f),
                Vector2.zero, Vector2.one, new Vector2(36f, 20f), new Vector2(-36f, -18f));
            box.speakerText.fontStyle = FontStyle.Bold;
            box.bodyText = UIFactory.Label(panel.transform, "Texto", "", 34, TextAnchor.UpperLeft, Color.white,
                Vector2.zero, Vector2.one, new Vector2(36f, 20f), new Vector2(-36f, -66f));
            UIFactory.Label(panel.transform, "Continuar", "E / Espacio / clic para continuar", 20, TextAnchor.LowerRight,
                new Color(1f, 1f, 1f, 0.5f), Vector2.zero, Vector2.one, new Vector2(36f, 14f), new Vector2(-36f, -14f));
            box.root.SetActive(false);
            return box;
        }

        /// <param name="keepState">true: no cambia el estado de juego (se usa durante el Final).</param>
        public void Play(DialogueLine[] dialogue, Action completed, bool keepState = false)
        {
            if (dialogue == null || dialogue.Length == 0)
            {
                completed?.Invoke();
                return;
            }

            lines = dialogue;
            index = 0;
            onComplete = completed;
            this.keepState = keepState;
            openedFrame = Time.frameCount;
            root.SetActive(true);
            ShowLine();
            if (!keepState) GameManager.Instance?.SetState(GameState.Narrativa);
        }

        public void Hide()
        {
            root.SetActive(false);
            onComplete = null;
        }

        private void ShowLine()
        {
            DialogueLine line = lines[index];
            speakerText.text = line.speaker;
            bodyText.text = line.text;
        }

        private void Update()
        {
            if (!root.activeSelf || Time.frameCount == openedFrame) return;

            Keyboard k = Keyboard.current;
            Mouse m = Mouse.current;
            bool advance = (k != null && (k.eKey.wasPressedThisFrame || k.spaceKey.wasPressedThisFrame || k.enterKey.wasPressedThisFrame))
                           || (m != null && m.leftButton.wasPressedThisFrame);
            if (!advance) return;

            index++;
            if (index < lines.Length)
            {
                ShowLine();
                return;
            }

            Action callback = onComplete;
            Hide();
            if (!keepState && GameManager.Instance != null && GameManager.Instance.State == GameState.Narrativa)
                GameManager.Instance.SetState(GameState.Exploracion);
            callback?.Invoke();
        }
    }
}
