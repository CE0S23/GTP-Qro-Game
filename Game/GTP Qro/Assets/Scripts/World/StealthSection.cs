using UnityEngine;

namespace GTP
{
    /// <summary>
    /// Tramo de sigilo: si el jugador lo cruza sin perder intentos, recibe bonificación de puntaje.
    /// Begin() y Complete() se llaman desde TriggerZones (eventos).
    /// </summary>
    public sealed class StealthSection : MonoBehaviour
    {
        private int detectionsAtStart = -1;
        private bool completed;

        public void Begin()
        {
            if (completed || detectionsAtStart >= 0) return;
            if (GameManager.Instance == null) return;
            detectionsAtStart = GameManager.Instance.Session.Detections;
        }

        public void Complete()
        {
            GameManager gm = GameManager.Instance;
            if (completed || gm == null || detectionsAtStart < 0) return;
            completed = true;
            if (gm.Session.Detections == detectionsAtStart)
            {
                gm.Session.RegisterPerfectStealth();
                gm.Session.AddScore(gm.Config.stealthBonusPoints);
                GameUI.Instance?.Hud.ShowToast("¡Sigilo perfecto! +" + gm.Config.stealthBonusPoints);
            }
        }
    }
}
