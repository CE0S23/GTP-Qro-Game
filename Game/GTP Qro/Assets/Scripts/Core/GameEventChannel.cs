using System;
using UnityEngine;

namespace GTP
{
    /// <summary>
    /// Canal de eventos como ScriptableObject (rúbrica AU 2.1). Quien emite y quien escucha solo
    /// conocen el asset, no se conocen entre sí ni dependen de objetos de la escena.
    /// El parámetro es la posición en el mundo donde ocurrió el evento (para partículas, sonido, etc.).
    /// </summary>
    [CreateAssetMenu(menuName = "GTP/Canal de evento", fileName = "Evento")]
    public sealed class GameEventChannel : ScriptableObject
    {
        [TextArea] [SerializeField] private string description = "";

        public event Action<Vector3> Raised;

        public string Description => description;

        public void Raise(Vector3 worldPosition) => Raised?.Invoke(worldPosition);

        public void Raise() => Raise(Vector3.zero);

        /// <summary>Crea un canal en memoria (si el asset falta o en pruebas).</summary>
        public static GameEventChannel CreateRuntime(string channelName)
        {
            var channel = CreateInstance<GameEventChannel>();
            channel.name = channelName;
            return channel;
        }
    }

    /// <summary>
    /// Acceso a los canales de juego guardados en Assets/Resources/Eventos/*.asset.
    /// </summary>
    public static class GameEvents
    {
        public const string Folder = "Eventos";

        public const string PlayerCaughtName = "Evento_JugadorDetectado";
        public const string PickupName = "Evento_BateriaRecogida";
        public const string GraffitiName = "Evento_GrafitiLeido";
        public const string CheckpointName = "Evento_PuntoControl";
        public const string StealthPerfectName = "Evento_SigiloPerfecto";
        public const string MinigameWonName = "Evento_MinijuegoGanado";
        public const string LevelCompletedName = "Evento_NivelCompletado";

        public static readonly string[] All =
        {
            PlayerCaughtName, PickupName, GraffitiName, CheckpointName, StealthPerfectName, MinigameWonName, LevelCompletedName
        };

        private static GameEventChannel playerCaught, pickup, graffiti, checkpoint, stealthPerfect, minigameWon, levelCompleted;

        /// <summary>El jugador perdió un intento (detectado o alcanzado).</summary>
        public static GameEventChannel PlayerCaught => Get(ref playerCaught, PlayerCaughtName);
        public static GameEventChannel PickupCollected => Get(ref pickup, PickupName);
        public static GameEventChannel GraffitiRead => Get(ref graffiti, GraffitiName);
        public static GameEventChannel CheckpointReached => Get(ref checkpoint, CheckpointName);
        public static GameEventChannel StealthPerfect => Get(ref stealthPerfect, StealthPerfectName);
        public static GameEventChannel MinigameWon => Get(ref minigameWon, MinigameWonName);
        public static GameEventChannel LevelCompleted => Get(ref levelCompleted, LevelCompletedName);

        private static GameEventChannel Get(ref GameEventChannel cache, string assetName)
        {
            if (cache != null) return cache;
            cache = Resources.Load<GameEventChannel>(Folder + "/" + assetName);
            if (cache == null) cache = GameEventChannel.CreateRuntime(assetName);
            return cache;
        }
    }
}
