using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace GTP.Tests
{
    /// <summary>Prueba de humo: cada escena carga y corre unos segundos sin errores en consola.</summary>
    public sealed class SceneSmokeTests
    {
        private static readonly string[] Levels =
        {
            SceneNames.Cuarto, SceneNames.Barrio, SceneNames.Encrucijada,
            SceneNames.RutaBien, SceneNames.RutaMal, SceneNames.Spot
        };

        [UnityTest]
        public IEnumerator Menu_LoadsWithoutErrors()
        {
            SceneManager.LoadScene(SceneNames.Menu);
            yield return null;
            yield return new WaitForSecondsRealtime(0.5f);
            Assert.IsNotNull(GameManager.Instance);
            Assert.AreEqual(GameState.Menu, GameManager.Instance.State);
        }

        [UnityTest]
        public IEnumerator Level_LoadsAndRunsWithoutErrors([ValueSource(nameof(Levels))] string scene)
        {
            SceneManager.LoadScene(scene);
            yield return null;
            yield return new WaitForSecondsRealtime(2f);
            Assert.IsNotNull(GameManager.Instance, "GameManager no se creó");
            Assert.IsNotNull(PlayerStealth.Current, "No hay jugador en " + scene);
            Assert.IsNotNull(Camera.main, "No hay cámara principal en " + scene);
            Assert.AreNotEqual(GameState.GameOver, GameManager.Instance.State);
        }

        [UnityTest]
        public IEnumerator ThreeCatches_LeadToGameOver_AndRetryRestoresLives()
        {
            SceneManager.LoadScene(SceneNames.Barrio);
            yield return null;
            // El diálogo de inicio pone timeScale en 0: se espera en tiempo real y se cierra.
            yield return new WaitForSecondsRealtime(0.3f);
            GameManager gm = GameManager.Instance;
            gm.Session.RestoreLevelStart(); // intentos completos aunque otra prueba haya usado algunos
            GameUI.Instance.Dialogue.Hide();
            gm.SetState(GameState.Exploracion);

            for (int i = 0; i < 3; i++)
            {
                // Espera a que termine la invulnerabilidad tras cargar / reaparecer.
                yield return new WaitForSeconds(gm.Config.invulnerabilityAfterRespawn + 0.2f);
                gm.ReportPlayerCaught("prueba");
            }

            Assert.AreEqual(GameState.GameOver, gm.State);
            Assert.AreEqual(0, gm.Session.Lives);

            gm.RetryLevel();
            yield return null;
            yield return new WaitForSecondsRealtime(0.5f);
            Assert.AreEqual(gm.Session.MaxLives, gm.Session.Lives);
            Assert.AreEqual(SceneNames.Barrio, SceneManager.GetActiveScene().name);
        }
    }
}
