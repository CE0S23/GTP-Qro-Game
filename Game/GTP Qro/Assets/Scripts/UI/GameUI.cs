using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

namespace GTP
{
    /// <summary>
    /// Raíz persistente de la interfaz. Crea y expone cada panel; cada panel es un componente
    /// independiente con su propia responsabilidad.
    /// </summary>
    public sealed class GameUI : MonoBehaviour
    {
        public static GameUI Instance { get; private set; }

        public PhoneUIController Hud { get; private set; }
        public DialogueBox Dialogue { get; private set; }
        public DecisionPanel Decision { get; private set; }
        public MinigameManager Minigame { get; private set; }
        public PausePanel Pause { get; private set; }
        public GameOverPanel GameOver { get; private set; }
        public SummaryPanel Summary { get; private set; }

        public static GameUI Create(Transform parent, GameSession session)
        {
            var root = new GameObject("GameUI");
            root.transform.SetParent(parent, false);
            var ui = root.AddComponent<GameUI>();
            Instance = ui;

            var eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            eventSystem.transform.SetParent(root.transform, false);
            eventSystem.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();

            ui.Hud = PhoneUIController.Build(UIFactory.CreateCanvas(root.transform, "HUD", 10).transform);
            ui.Dialogue = DialogueBox.Build(UIFactory.CreateCanvas(root.transform, "Dialogo", 20).transform);
            ui.Decision = DecisionPanel.Build(UIFactory.CreateCanvas(root.transform, "Decision", 30).transform);
            ui.Minigame = MinigameManager.Build(UIFactory.CreateCanvas(root.transform, "Minijuego", 40).transform);
            ui.Pause = PausePanel.Build(UIFactory.CreateCanvas(root.transform, "Pausa", 50).transform);
            ui.GameOver = GameOverPanel.Build(UIFactory.CreateCanvas(root.transform, "GameOver", 60).transform);
            ui.Summary = SummaryPanel.Build(UIFactory.CreateCanvas(root.transform, "Resumen", 70).transform);
            ui.BindSession(session);
            return ui;
        }

        public void BindSession(GameSession session) => Hud.Bind(session);

        public void HideAllOverlays()
        {
            Dialogue.Hide();
            Decision.Hide();
            Minigame.Hide();
            Pause.Hide();
            GameOver.Hide();
            Summary.Hide();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
