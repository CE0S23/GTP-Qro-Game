using System.Collections.Generic;
using Unity.AI.Navigation;
using Unity.Cinemachine;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace GTP.EditorTools
{
    /// <summary>
    /// Hito 03 — piezas que el constructor agrega a cada escena:
    /// cámaras Cinemachine (DE 2.1), Volume de post-processing (DE 3.1), Animator con Blend Tree (DE 4.1),
    /// canales de eventos ScriptableObject (AU 2.1) y NavMesh horneado (AU 3.1).
    /// </summary>
    public static partial class GreyboxSceneBuilder
    {
        private const string PostDir = "Assets/Settings/PostFX";
        private const string NavDir = "Assets/_Scenes/NavMesh";
        private const string AnimDir = "Assets/Animations";
        private const string EventsDir = "Assets/Resources/" + GameEvents.Folder;
        private const string FxDir = "Assets/Resources/FX";

        private static readonly Vector3 CloseUpOffset = new Vector3(0f, 7.5f, -6.5f);
        private static AnimatorController locomotion;

        private enum Mood { Noche, Cuarto, Parque, Peligro, Spot }

        // =====================================================================
        // Preparación de assets compartidos (se llama una vez en BuildAll)
        // =====================================================================
        private static void PrepareHito03Assets()
        {
            EnsureFolder(PostDir);
            EnsureFolder(NavDir);
            EnsureFolder(AnimDir);
            EnsureFolder(EventsDir);
            EnsureFolder(FxDir);
            EnsureEventChannels();
            EnsureParticleMaterial();
            locomotion = EnsureLocomotionController();
        }

        /// <summary>Se ejecuta justo antes de guardar cada escena.</summary>
        private static void FinishScene(string sceneName)
        {
            AddPostVolume(MoodFor(sceneName));
            if (Object.FindAnyObjectByType<PlayerStealth>() != null)
                BakeNavMesh(sceneName);
        }

        private static Mood MoodFor(string sceneName)
        {
            switch (sceneName)
            {
                case SceneNames.Cuarto: return Mood.Cuarto;
                case SceneNames.RutaBien: return Mood.Parque;
                case SceneNames.RutaMal: return Mood.Peligro;
                case SceneNames.Spot: return Mood.Spot;
                default: return Mood.Noche;
            }
        }

        // =====================================================================
        // AU 2.1 · Canales de eventos (ScriptableObject)
        // =====================================================================
        private static void EnsureEventChannels()
        {
            var descriptions = new Dictionary<string, string>
            {
                { GameEvents.PlayerCaughtName, "El jugador perdió un intento (cono de visión, foco o perseguidor). Lo emite GameManager; lo escuchan FeedbackFX (sacudida + partículas) y PostFXController (destello rojo)." },
                { GameEvents.PickupName, "Se recogió una batería portátil. Emite BatteryPickup." },
                { GameEvents.GraffitiName, "Se leyó un grafiti por primera vez. Emite Graffiti." },
                { GameEvents.CheckpointName, "Se activó un punto de control. Emite Checkpoint." },
                { GameEvents.StealthPerfectName, "Tramo de sigilo completado sin ser detectado. Emite StealthSection." },
                { GameEvents.MinigameWonName, "Se forzó la cerradura del portón. Emite LockedGate." },
                { GameEvents.LevelCompletedName, "El jugador cruzó una salida desbloqueada. Emite LevelExit." }
            };
            foreach (string channelName in GameEvents.All)
            {
                string path = EventsDir + "/" + channelName + ".asset";
                var channel = AssetDatabase.LoadAssetAtPath<GameEventChannel>(path);
                if (channel == null)
                {
                    channel = ScriptableObject.CreateInstance<GameEventChannel>();
                    AssetDatabase.CreateAsset(channel, path);
                }
                var so = new SerializedObject(channel);
                so.FindProperty("description").stringValue = descriptions[channelName];
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        // =====================================================================
        // DE 3.1 · Material de partículas y Volume de post-processing
        // =====================================================================
        private static void EnsureParticleMaterial()
        {
            const string path = FxDir + "/Particula.mat";
            Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (m == null)
            {
                m = new Material(shader);
                AssetDatabase.CreateAsset(m, path);
            }
            m.shader = shader;
            // Transparente aditivo: las partículas "brillan" y el Bloom las realza.
            m.SetFloat("_Surface", 1f);
            m.SetFloat("_Blend", 2f);
            m.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            m.SetFloat("_DstBlend", (float)BlendMode.One);
            m.SetFloat("_ZWrite", 0f);
            m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            m.renderQueue = (int)RenderQueue.Transparent;
            m.SetColor("_BaseColor", Color.white);
            EditorUtility.SetDirty(m);
        }

        private static void AddPostVolume(Mood mood)
        {
            var go = new GameObject("Volumen_PostProcesado");
            var volume = go.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = 1f;
            volume.sharedProfile = EnsureProfile(mood);
            go.AddComponent<PostFXController>();

            foreach (Camera cam in Object.FindObjectsByType<Camera>())
                cam.GetUniversalAdditionalCameraData().renderPostProcessing = true;
        }

        private static VolumeProfile EnsureProfile(Mood mood)
        {
            string path = PostDir + "/PostFX_" + mood + ".asset";
            var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(path);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<VolumeProfile>();
                AssetDatabase.CreateAsset(profile, path);
            }
            // Reconstruye los overrides para que el script sea la única fuente de verdad.
            foreach (VolumeComponent c in profile.components.ToArray())
            {
                profile.components.Remove(c);
                Object.DestroyImmediate(c, true);
            }

            float bloom = 0.9f, vignette = 0.32f, saturation = 5f, contrast = 12f, exposure = 0.15f, chromatic = 0f, grain = 0f;
            Color filter = Color.white;
            switch (mood)
            {
                case Mood.Cuarto: bloom = 0.7f; vignette = 0.28f; saturation = 10f; filter = new Color(1f, 0.93f, 0.85f); exposure = 0.2f; break;
                case Mood.Parque: bloom = 0.8f; vignette = 0.25f; saturation = 12f; filter = new Color(0.95f, 1f, 0.97f); exposure = 0.25f; break;
                case Mood.Peligro: bloom = 1.2f; vignette = 0.4f; saturation = -5f; contrast = 22f; chromatic = 0.15f; grain = 0.35f; filter = new Color(1f, 0.92f, 0.95f); break;
                case Mood.Spot: bloom = 0.9f; vignette = 0.25f; saturation = 8f; filter = new Color(1f, 0.95f, 0.9f); exposure = 0.2f; break;
            }

            var b = Add<Bloom>(profile);
            b.intensity.Override(bloom);
            b.threshold.Override(0.85f);
            b.scatter.Override(0.65f);

            var v = Add<Vignette>(profile);
            v.intensity.Override(vignette);
            v.smoothness.Override(0.45f);
            v.color.Override(Color.black);

            var ca = Add<ColorAdjustments>(profile);
            ca.saturation.Override(saturation);
            ca.contrast.Override(contrast);
            ca.postExposure.Override(exposure);
            ca.colorFilter.Override(filter);

            var tm = Add<Tonemapping>(profile);
            tm.mode.Override(TonemappingMode.Neutral);

            var chrom = Add<ChromaticAberration>(profile);
            chrom.intensity.Override(chromatic);

            if (grain > 0f)
            {
                var fg = Add<FilmGrain>(profile);
                fg.intensity.Override(grain);
                fg.type.Override(FilmGrainLookup.Medium1);
            }

            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssets();
            return profile;
        }

        private static T Add<T>(VolumeProfile profile) where T : VolumeComponent
        {
            T component = profile.Add<T>(true);
            component.name = typeof(T).Name;
            AssetDatabase.AddObjectToAsset(component, profile);
            return component;
        }

        // =====================================================================
        // DE 2.1 · Cinemachine (Follow + transición a plano cercano + Impulse)
        // =====================================================================
        private static void AddCinemachineRig(Camera mainCamera, Transform target)
        {
            var brain = mainCamera.gameObject.AddComponent<CinemachineBrain>();
            brain.IgnoreTimeScale = true; // los diálogos ponen timeScale en 0 y la transición debe verse
            brain.DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.EaseInOut, 0.9f);

            var rig = new GameObject("Camaras_Cinemachine");
            CinemachineCamera follow = VirtualCamera("CM_Seguimiento", rig.transform, target, CamOffset, 55f, 0.25f);
            CinemachineCamera closeUp = VirtualCamera("CM_PlanoCercano", rig.transform, target, CloseUpOffset, 45f, 0.6f);
            rig.AddComponent<CameraDirector>().Configure(follow, closeUp);
        }

        private static CinemachineCamera VirtualCamera(string name, Transform parent, Transform target, Vector3 offset, float fov, float damping)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = target.position + offset;
            go.transform.rotation = Quaternion.LookRotation(-offset, Vector3.up);

            var cam = go.AddComponent<CinemachineCamera>();
            cam.Follow = target;
            var lens = cam.Lens;
            lens.FieldOfView = fov;
            cam.Lens = lens;

            var body = go.AddComponent<CinemachineFollow>();
            body.FollowOffset = offset;
            var tracker = body.TrackerSettings;
            tracker.BindingMode = Unity.Cinemachine.TargetTracking.BindingMode.WorldSpace;
            tracker.PositionDamping = new Vector3(damping, damping, damping);
            body.TrackerSettings = tracker;

            var listener = go.AddComponent<CinemachineImpulseListener>();
            listener.ChannelMask = FeedbackFX.ImpulseChannel;
            listener.Gain = 1f;
            listener.Use2DDistance = true;
            return cam;
        }

        // =====================================================================
        // DE 4.1 · Animator Controller con Blend Tree de locomoción
        // =====================================================================
        private static AnimatorController EnsureLocomotionController()
        {
            AnimationClip idle = Clip("Reposo", 1.6f, 0f, 0.035f);
            AnimationClip walk = Clip("Caminar", 0.5f, 0.12f, 0.03f);
            AnimationClip run = Clip("Correr", 0.32f, 0.22f, 0.07f);

            string path = AnimDir + "/Locomocion.controller";
            AssetDatabase.DeleteAsset(path);
            AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(path);
            controller.AddParameter("Velocidad", AnimatorControllerParameterType.Float);
            controller.CreateBlendTreeInController("Locomocion", out BlendTree tree, 0);
            tree.name = "Locomocion_BlendTree";
            tree.blendType = BlendTreeType.Simple1D;
            tree.blendParameter = "Velocidad";
            tree.useAutomaticThresholds = false;
            tree.AddChild(idle, 0f);
            tree.AddChild(walk, 4f);
            tree.AddChild(run, 6.4f);
            EditorUtility.SetDirty(tree);
            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();
            return controller;
        }

        /// <summary>
        /// Clip procedural sobre el hijo "Rig": rebote vertical (bob) y estiramiento/aplastamiento (escala) para Reposo, Caminar y Correr.
        /// </summary>
        private static AnimationClip Clip(string clipName, float period, float bob, float squash)
        {
            string path = AnimDir + "/" + clipName + ".anim";
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if (clip == null)
            {
                clip = new AnimationClip();
                AssetDatabase.CreateAsset(clip, path);
            }
            clip.ClearCurves();

            var posY = new AnimationCurve();
            var scaleY = new AnimationCurve();
            var scaleXZ = new AnimationCurve();
            const int steps = 8;
            for (int i = 0; i <= steps; i++)
            {
                float t = period * i / steps;
                float phase = (float)i / steps * Mathf.PI * 2f;
                // Bob: dos rebotes por ciclo (un paso con cada pie); respiración: un ciclo.
                float y = bob > 0f ? Mathf.Abs(Mathf.Sin(phase)) * bob : 0f;
                float s = bob > 0f ? Mathf.Cos(phase * 2f) * squash : Mathf.Sin(phase) * squash;
                posY.AddKey(t, y);
                scaleY.AddKey(t, 1f + s);
                scaleXZ.AddKey(t, 1f - s * 0.5f);
            }
            clip.SetCurve("Rig", typeof(Transform), "m_LocalPosition.y", posY);
            clip.SetCurve("Rig", typeof(Transform), "m_LocalScale.y", scaleY);
            clip.SetCurve("Rig", typeof(Transform), "m_LocalScale.x", scaleXZ);
            clip.SetCurve("Rig", typeof(Transform), "m_LocalScale.z", scaleXZ);
            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = true;
            AnimationUtility.SetAnimationClipSettings(clip, settings);
            EditorUtility.SetDirty(clip);
            return clip;
        }

        /// <summary>Crea Visual (Animator) → Rig (animado). Devuelve el Rig, donde van las piezas del cuerpo.</summary>
        private static Transform AnimatedVisual(Transform root, out GameObject visual)
        {
            visual = new GameObject("Visual");
            visual.transform.SetParent(root, false);
            var animator = visual.AddComponent<Animator>();
            animator.runtimeAnimatorController = locomotion;
            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
            visual.AddComponent<LocomotionAnimator>().Configure(root);

            var rig = new GameObject("Rig");
            rig.transform.SetParent(visual.transform, false);
            return rig.transform;
        }

        // =====================================================================
        // AU 3.1 · NavMesh
        // =====================================================================
        /// <summary>
        /// Hornea el NavMesh con los colliders físicos de la escena. Antes se apagan los personajes
        /// (Rigidbody) y las zonas trigger para que no se horneen como obstáculos.
        /// </summary>
        private static void BakeNavMesh(string sceneName)
        {
            var go = new GameObject("NavMesh");
            var surface = go.AddComponent<NavMeshSurface>();
            surface.collectObjects = CollectObjects.All;
            surface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
            surface.layerMask = ~0;

            var hidden = new List<GameObject>();
            foreach (Rigidbody rb in Object.FindObjectsByType<Rigidbody>())
                if (rb.gameObject.activeSelf) { rb.gameObject.SetActive(false); hidden.Add(rb.gameObject); }
            var triggers = new List<Collider>();
            foreach (Collider c in Object.FindObjectsByType<Collider>())
                if (c.isTrigger && c.enabled) { c.enabled = false; triggers.Add(c); }

            surface.BuildNavMesh();

            foreach (GameObject g in hidden) g.SetActive(true);
            foreach (Collider c in triggers) c.enabled = true;

            string path = NavDir + "/" + sceneName + "_NavMesh.asset";
            AssetDatabase.DeleteAsset(path);
            AssetDatabase.CreateAsset(surface.navMeshData, path);
        }

        /// <summary>Obstáculo dinámico: no se hornea, "talla" el NavMesh mientras está activo (p. ej., el portón cerrado).</summary>
        private static void DynamicObstacle(GameObject g)
        {
            var modifier = g.AddComponent<NavMeshModifier>();
            modifier.ignoreFromBuild = true;
            var obstacle = g.AddComponent<NavMeshObstacle>();
            obstacle.shape = NavMeshObstacleShape.Box;
            obstacle.size = Vector3.one; // en espacio local: coincide con el cubo escalado
            obstacle.carving = true;
            obstacle.carveOnlyStationary = false;
        }
    }
}
