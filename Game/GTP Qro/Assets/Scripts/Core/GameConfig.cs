using UnityEngine;

namespace GTP
{
    /// <summary>
    /// Contenedor de datos de balance (ScriptableObject). Ningún valor de balance vive en la escena:
    /// se edita en Assets/Resources/GameConfig.asset sin tocar código.
    /// </summary>
    [CreateAssetMenu(menuName = "GTP/Game Config", fileName = "GameConfig")]
    public sealed class GameConfig : ScriptableObject
    {
        [Header("Intentos (vidas) y batería")]
        [Min(1)] public int startLives = 3;
        [Range(1f, 100f)] public float startBattery = 12f;
        [Tooltip("Porcentaje de batería que se consume por minuto en exploración.")]
        [Min(0f)] public float batteryDrainPerMinute = 1.2f;
        [Min(0f)] public float batteryPickupAmount = 3f;

        [Header("Jugador")]
        [Min(0f)] public float walkSpeed = 4f;
        [Min(1f)] public float runMultiplier = 1.6f;
        [Range(0.1f, 1f)] public float crouchMultiplier = 0.5f;
        [Range(0.1f, 1f)] public float crouchDetectionMultiplier = 0.6f;
        [Min(1f)] public float runDetectionMultiplier = 1.25f;

        [Header("Sigilo")]
        [Tooltip("Segundos de exposición continua para ser detectado (GDD: 2 s).")]
        [Min(0.1f)] public float detectionTime = 2f;
        [Min(0f)] public float invulnerabilityAfterRespawn = 2f;

        [Header("Puntaje")]
        public int graffitiPoints = 100;
        public int pickupPoints = 50;
        public int stealthBonusPoints = 200;
        public int levelCompletePoints = 100;
        public int minigamePoints = 150;
        public int detectionPenalty = 50;
        public int endBonusPerBatteryPercent = 20;
        public int endBonusPerLife = 100;

        public static GameConfig LoadOrDefault()
        {
            GameConfig config = Resources.Load<GameConfig>("GameConfig");
            return config != null ? config : CreateInstance<GameConfig>();
        }
    }
}
