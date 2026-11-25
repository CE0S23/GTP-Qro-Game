namespace GTP
{
    /// <summary>Ruta narrativa elegida en la Encrucijada (GDD §4.6, Branching Path).</summary>
    public enum Route
    {
        None = 0,
        Bien = 1,
        Mal = 2
    }

    /// <summary>Estados de juego del GDD §8.2. Sigilo/Persecución se maneja por amenaza (ThreatDetector).</summary>
    public enum GameState
    {
        Menu,
        Exploracion,
        Narrativa,
        Decision,
        Minijuego,
        Pausa,
        GameOver,
        Final
    }

    /// <summary>Nombres de las escenas del GDD §8.1. Deben coincidir con Build Settings.</summary>
    public static class SceneNames
    {
        public const string Menu = "Escena_Menu";
        public const string Cuarto = "Escena_Cuarto";
        public const string Barrio = "Escena_Barrio";
        public const string Encrucijada = "Escena_Encrucijada";
        public const string RutaBien = "Escena_RutaBien";
        public const string RutaMal = "Escena_RutaMal";
        public const string Spot = "Escena_Spot";

        public static readonly string[] BuildOrder =
        {
            Menu, Cuarto, Barrio, Encrucijada, RutaBien, RutaMal, Spot
        };
    }

    [System.Serializable]
    public struct DialogueLine
    {
        public string speaker;
        [UnityEngine.TextArea(2, 4)] public string text;

        public DialogueLine(string speaker, string text)
        {
            this.speaker = speaker;
            this.text = text;
        }
    }
}
