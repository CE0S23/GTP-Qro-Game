using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace GTP
{
    /// <summary>Interfaz de decisión con dos opciones (mouse o teclas 1/2).</summary>
    public sealed class DecisionPanel : MonoBehaviour
    {
        private GameObject root;
        private Text questionText, optionAText, optionBText;
        private Action<int> onChosen;
        private int openedFrame;

        public static DecisionPanel Build(Transform canvas)
        {
            var panel = canvas.gameObject.AddComponent<DecisionPanel>();
            Image bg = UIFactory.Panel(canvas, "Fondo", new Color(0f, 0f, 0f, 0.75f), Vector2.zero, Vector2.one);
            panel.root = bg.gameObject;
            panel.questionText = UIFactory.Label(bg.transform, "Pregunta", "", 48, TextAnchor.MiddleCenter, Color.white,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-800f, 140f), new Vector2(800f, 260f));
            Button a = UIFactory.Button(bg.transform, "OpcionA", "", new Vector2(-380f, -40f), new Vector2(680f, 120f), () => panel.Choose(0));
            Button b = UIFactory.Button(bg.transform, "OpcionB", "", new Vector2(380f, -40f), new Vector2(680f, 120f), () => panel.Choose(1));
            panel.optionAText = a.GetComponentInChildren<Text>();
            panel.optionBText = b.GetComponentInChildren<Text>();
            UIFactory.Label(bg.transform, "Ayuda", "Haz clic en una opción o presiona 1 / 2. La decisión no se puede cambiar.", 24,
                TextAnchor.MiddleCenter, new Color(1f, 1f, 1f, 0.6f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(-800f, -220f), new Vector2(800f, -160f));
            panel.root.SetActive(false);
            return panel;
        }

        public void Show(string question, string optionA, string optionB, Action<int> chosen)
        {
            questionText.text = question;
            optionAText.text = "1. " + optionA;
            optionBText.text = "2. " + optionB;
            onChosen = chosen;
            openedFrame = Time.frameCount;
            root.SetActive(true);
            GameManager.Instance?.SetState(GameState.Decision);
        }

        public void Hide()
        {
            root.SetActive(false);
            onChosen = null;
        }

        private void Choose(int option)
        {
            if (!root.activeSelf || Time.frameCount == openedFrame) return;
            Action<int> callback = onChosen;
            Hide();
            callback?.Invoke(option);
        }

        private void Update()
        {
            if (!root.activeSelf) return;
            Keyboard k = Keyboard.current;
            if (k == null) return;
            if (k.digit1Key.wasPressedThisFrame || k.numpad1Key.wasPressedThisFrame) Choose(0);
            else if (k.digit2Key.wasPressedThisFrame || k.numpad2Key.wasPressedThisFrame) Choose(1);
        }
    }
}
