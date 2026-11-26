using System.Collections;
using NUnit.Framework;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace GTP.Tests
{
    /// <summary>
    /// Hito 03: NavMesh (AU 3.1), Cinemachine (DE 2.1), post-processing y partículas (DE 3.1),
    /// Blend Tree (DE 4.1) y corrección de la salida bloqueada.
    /// </summary>
    public sealed class Hito03PlayModeTests
    {
        private static readonly string[] Levels =
        {
            SceneNames.Cuarto, SceneNames.Barrio, SceneNames.Encrucijada,
            SceneNames.RutaBien, SceneNames.RutaMal, SceneNames.Spot
        };

        private static IEnumerator Load(string scene)
        {
            SceneManager.LoadScene(scene);
            yield return null;
            yield return new WaitForSecondsRealtime(0.3f);
        }

        private static void SkipDialogue()
        {
            GameUI.Instance.Dialogue.Hide();
            GameManager.Instance.SetState(GameState.Exploracion);
        }

        [TearDown]
        public void ResetDebug()
        {
            if (GameManager.Instance == null) return;
            GameManager.Instance.DebugInvulnerable = false;
            // La sesión persiste entre pruebas: devolver los intentos para no afectar a las siguientes.
            GameManager.Instance.Session.RestoreLevelStart();
        }

        [UnityTest]
        public IEnumerator EveryLevel_HasNavMesh_CinemachineAndPostProcessing([ValueSource(nameof(Levels))] string scene)
        {
            yield return Load(scene);
            Vector3 p = PlayerStealth.Current.transform.position;
            Assert.IsTrue(NavMesh.SamplePosition(p, out _, 2f, NavMesh.AllAreas), "Sin NavMesh bajo el jugador en " + scene);
            Assert.IsNotNull(Camera.main.GetComponent<CinemachineBrain>(), "La cámara no tiene CinemachineBrain");
            Assert.IsNotNull(Object.FindAnyObjectByType<CameraDirector>(), "Falta CameraDirector");
            Assert.IsNotNull(PostFXController.Current, "Falta el Volume con PostFXController");
            Assert.IsTrue(PostFXController.Current.GetComponent<Volume>().isGlobal);
        }

        [UnityTest]
        public IEnumerator Barrio_PathAroundCar_HasCorners()
        {
            yield return Load(SceneNames.Barrio);
            // Auto_01 está en (48, 4): una ruta de un lado al otro debe rodearlo.
            var path = new NavMeshPath();
            NavMesh.CalculatePath(Sample(new Vector3(45f, 0f, 4f)), Sample(new Vector3(51f, 0f, 4f)), NavMesh.AllAreas, path);
            Assert.AreEqual(NavMeshPathStatus.PathComplete, path.status);
            Assert.Greater(path.corners.Length, 2, "La ruta debería rodear el auto (más de 2 esquinas)");
        }

        [UnityTest]
        public IEnumerator RutaMal_Gate_IsDynamicObstacle_OpensNavMeshWhenForced()
        {
            yield return Load(SceneNames.RutaMal);
            SkipDialogue();
            GameObject gate = GameObject.Find("Porton");
            Assert.IsNotNull(gate);
            Vector3 from = new Vector3(58f, 0f, 0f), to = new Vector3(67f, 0f, 0f);
            var path = new NavMeshPath();

            NavMesh.CalculatePath(Sample(from), Sample(to), NavMesh.AllAreas, path);
            Assert.AreNotEqual(NavMeshPathStatus.PathComplete, path.status, "El portón cerrado debería bloquear el callejón");

            gate.SetActive(false); // lo mismo que hace LockedGate al ganar el minijuego
            yield return new WaitForSeconds(0.3f);
            NavMesh.CalculatePath(Sample(from), Sample(to), NavMesh.AllAreas, path);
            Assert.AreEqual(NavMeshPathStatus.PathComplete, path.status, "Al abrir el portón la ruta debe quedar libre");
        }

        private static Vector3 Sample(Vector3 p)
        {
            Assert.IsTrue(NavMesh.SamplePosition(p, out NavMeshHit hit, 3f, NavMesh.AllAreas), "Sin NavMesh en " + p);
            return hit.position;
        }

        [UnityTest]
        public IEnumerator Chaser_UsesNavMeshWhileChasing()
        {
            yield return Load(SceneNames.Barrio);
            SkipDialogue();
            ChaserNPC mugger = GameObject.Find("Asaltante").GetComponent<ChaserNPC>();
            GameManager.Instance.DebugInvulnerable = true;
            PlayerStealth.Current.Movement.Teleport(new Vector3(39f, 1f, 4f));
            mugger.Activate();
            yield return new WaitForSeconds(0.5f);
            Assert.IsTrue(mugger.IsChasing);
            Assert.IsTrue(mugger.IsUsingNavMesh, "El perseguidor debería seguir una ruta del NavMesh");
        }

        [UnityTest]
        public IEnumerator Dialogue_BlendsToCloseUpCamera()
        {
            yield return Load(SceneNames.Barrio); // la escena arranca con un diálogo
            Assert.AreEqual(GameState.Narrativa, GameManager.Instance.State);
            yield return null;
            var director = Object.FindAnyObjectByType<CameraDirector>();
            Assert.IsTrue(director.CloseUpActive, "En diálogo debe activarse el plano cercano");
            SkipDialogue();
            yield return null;
            yield return null;
            Assert.IsFalse(director.CloseUpActive, "En exploración vuelve la cámara de seguimiento");
        }

        [UnityTest]
        public IEnumerator PlayerCaught_TriggersShakeParticlesAndVignetteFlash()
        {
            yield return Load(SceneNames.Barrio);
            SkipDialogue();
            yield return new WaitForSeconds(GameManager.Instance.Config.invulnerabilityAfterRespawn + 0.2f);
            int before = FeedbackFX.Instance.CaughtBursts;
            float vignetteBefore = PostFXController.Current.CurrentVignetteIntensity;

            GameManager.Instance.ReportPlayerCaught("prueba");
            yield return null;
            yield return null;

            Assert.AreEqual(before + 1, FeedbackFX.Instance.CaughtBursts);
            Assert.Greater(PostFXController.Current.CurrentVignetteIntensity, vignetteBefore + 0.05f);
            ParticleSystem burst = FeedbackFX.Instance.transform.Find("FX_Detectado").GetComponent<ParticleSystem>();
            Assert.Greater(burst.particleCount, 0);
        }

        [UnityTest]
        public IEnumerator Player_HasLocomotionBlendTree_SpeedParamFollowsMovement()
        {
            yield return Load(SceneNames.Barrio);
            SkipDialogue();
            var loco = PlayerStealth.Current.GetComponentInChildren<LocomotionAnimator>();
            Assert.IsNotNull(loco);
            Animator animator = loco.GetComponent<Animator>();
            Assert.IsNotNull(animator.runtimeAnimatorController, "Falta Locomocion.controller");

            Rigidbody body = PlayerStealth.Current.GetComponent<Rigidbody>();
            for (int i = 0; i < 30; i++)
            {
                body.MovePosition(body.position + Vector3.right * 4f * Time.fixedDeltaTime);
                yield return new WaitForFixedUpdate();
            }
            Assert.Greater(animator.GetFloat(LocomotionAnimator.SpeedParam), 1f);
        }

        [UnityTest]
        public IEnumerator LockedExit_UnlockedWhilePlayerInside_CompletesLevel()
        {
            yield return Load(SceneNames.Cuarto);
            SkipDialogue();
            LevelExit exit = Object.FindAnyObjectByType<LevelExit>();
            Assert.IsTrue(exit.IsLocked);
            PlayerStealth.Current.Movement.Teleport(exit.transform.position);
            yield return new WaitForSeconds(0.3f);
            Assert.AreEqual(SceneNames.Cuarto, SceneManager.GetActiveScene().name, "Bloqueada: no debe salir");

            exit.Unlock();
            // El siguiente nivel abre con diálogo (timeScale = 0): esperar en tiempo real.
            yield return new WaitForSecondsRealtime(0.5f);
            Assert.AreEqual(SceneNames.Barrio, SceneManager.GetActiveScene().name,
                "Al desbloquear con el jugador dentro debe pasar al siguiente nivel");
        }
    }
}
