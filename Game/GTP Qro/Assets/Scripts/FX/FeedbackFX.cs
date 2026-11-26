using Unity.Cinemachine;
using UnityEngine;

namespace GTP
{
    /// <summary>
    /// Retroalimentación visual de las acciones (rúbrica DE 2.1 y DE 3.1):
    /// partículas al recoger baterías, leer grafitis, pasar puntos de control, sigilo perfecto,
    /// minijuego ganado y al ser detectado, polvo al correr, llama del encendedor y
    /// sacudida de cámara (Cinemachine Impulse) cuando el jugador pierde un intento.
    ///
    /// Escucha los canales <see cref="GameEvents"/> (ScriptableObjects): no conoce a quién emite.
    /// Optimización: cada efecto es UN ParticleSystem persistente que se reutiliza con Emit()
    /// en la posición del evento, así no se crean ni destruyen objetos durante el juego (sin basura para el GC).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class FeedbackFX : MonoBehaviour
    {
        public static FeedbackFX Instance { get; private set; }

        /// <summary>Canal de impulso que escuchan las cámaras de Cinemachine del juego.</summary>
        public const int ImpulseChannel = 1;

        [SerializeField, Min(0f)] private float caughtShakeForce = 1.4f;
        [SerializeField, Min(0.02f)] private float dustInterval = 0.07f;

        private ParticleSystem burstPickup, burstGraffiti, burstCheckpoint, burstCaught, burstStealth, burstMinigame, dust, flame;
        private CinemachineImpulseSource impulse;
        private Material particleMaterial;
        private float dustTimer;

        public CinemachineImpulseSource Impulse => impulse;
        public int CaughtBursts { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;

            particleMaterial = CreateParticleMaterial();
            burstPickup = CreateBurst("FX_Bateria", new Color(0.3f, 0.75f, 1f), 3.5f, 0.6f, 0.18f, -0.4f);
            burstGraffiti = CreateBurst("FX_Grafiti", new Color(1f, 0.85f, 0.25f), 2.5f, 0.9f, 0.16f, -0.2f);
            burstCheckpoint = CreateBurst("FX_PuntoControl", new Color(0.3f, 1f, 0.45f), 4f, 0.8f, 0.2f, -0.6f);
            burstCaught = CreateBurst("FX_Detectado", new Color(1f, 0.18f, 0.12f), 6f, 0.5f, 0.22f, 0.5f);
            burstStealth = CreateBurst("FX_SigiloPerfecto", new Color(0.85f, 0.9f, 1f), 3f, 1.1f, 0.14f, -0.5f);
            burstMinigame = CreateBurst("FX_Minijuego", new Color(1f, 0.6f, 0.15f), 5f, 0.7f, 0.18f, 0.2f);
            dust = CreateBurst("FX_PolvoCorrer", new Color(0.75f, 0.7f, 0.62f, 0.55f), 0.8f, 0.45f, 0.25f, 0f);
            flame = CreateFlame();

            impulse = gameObject.AddComponent<CinemachineImpulseSource>();
            impulse.ImpulseDefinition.ImpulseChannel = ImpulseChannel;
            impulse.ImpulseDefinition.ImpulseType = CinemachineImpulseDefinition.ImpulseTypes.Uniform;
            impulse.ImpulseDefinition.ImpulseShape = CinemachineImpulseDefinition.ImpulseShapes.Bump;
            impulse.ImpulseDefinition.ImpulseDuration = 0.35f;
            impulse.DefaultVelocity = new Vector3(0.35f, -0.5f, 0f);
        }

        private void OnEnable()
        {
            GameEvents.PlayerCaught.Raised += OnCaught;
            GameEvents.PickupCollected.Raised += OnPickup;
            GameEvents.GraffitiRead.Raised += OnGraffiti;
            GameEvents.CheckpointReached.Raised += OnCheckpoint;
            GameEvents.StealthPerfect.Raised += OnStealth;
            GameEvents.MinigameWon.Raised += OnMinigame;
        }

        private void OnDisable()
        {
            GameEvents.PlayerCaught.Raised -= OnCaught;
            GameEvents.PickupCollected.Raised -= OnPickup;
            GameEvents.GraffitiRead.Raised -= OnGraffiti;
            GameEvents.CheckpointReached.Raised -= OnCheckpoint;
            GameEvents.StealthPerfect.Raised -= OnStealth;
            GameEvents.MinigameWon.Raised -= OnMinigame;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void Update()
        {
            PlayerStealth player = PlayerStealth.Current;
            if (player == null || !player.IsDetectable || !player.Movement.IsRunning) return;
            dustTimer -= Time.deltaTime;
            if (dustTimer > 0f) return;
            dustTimer = dustInterval;
            Emit(dust, player.transform.position + Vector3.down * 0.85f, 2);
        }

        // ---------------- Eventos ----------------
        private void OnCaught(Vector3 position)
        {
            CaughtBursts++;
            Emit(burstCaught, position, 40);
            impulse.GenerateImpulseWithForce(caughtShakeForce);
        }

        private void OnPickup(Vector3 position) => Emit(burstPickup, position, 28);
        private void OnGraffiti(Vector3 position) => Emit(burstGraffiti, position + Vector3.up * 0.8f, 24);
        private void OnCheckpoint(Vector3 position) => Emit(burstCheckpoint, position, 36);
        private void OnStealth(Vector3 position) => Emit(burstStealth, position + Vector3.up, 30);

        private void OnMinigame(Vector3 position)
        {
            Emit(burstMinigame, position + Vector3.up, 40);
            impulse.GenerateImpulseWithForce(caughtShakeForce * 0.4f);
        }

        public void PlayLighterFlame(Vector3 position)
        {
            flame.transform.position = position;
            flame.Play();
        }

        public void StopLighterFlame() => flame.Stop(true, ParticleSystemStopBehavior.StopEmitting);

        // ---------------- Construcción de sistemas ----------------
        private static void Emit(ParticleSystem system, Vector3 position, int count)
        {
            var emit = new ParticleSystem.EmitParams { position = position, applyShapeToPosition = true };
            system.Emit(emit, count);
        }

        private ParticleSystem CreateBurst(string systemName, Color color, float speed, float lifetime, float size, float gravity)
        {
            var go = new GameObject(systemName);
            go.transform.SetParent(transform, false);
            var ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var main = ps.main;
            main.loop = false;
            main.playOnAwake = false;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startLifetime = new ParticleSystem.MinMaxCurve(lifetime * 0.6f, lifetime);
            main.startSpeed = new ParticleSystem.MinMaxCurve(speed * 0.4f, speed);
            main.startSize = new ParticleSystem.MinMaxCurve(size * 0.5f, size);
            main.startColor = color;
            main.gravityModifier = gravity;
            main.maxParticles = 300;
            main.useUnscaledTime = false;

            var emission = ps.emission;
            emission.enabled = false;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.25f;

            var fade = ps.colorOverLifetime;
            fade.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0.8f, 0.5f), new GradientAlphaKey(0f, 1f) });
            fade.color = gradient;

            var sizeOverLife = ps.sizeOverLifetime;
            sizeOverLife.enabled = true;
            sizeOverLife.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0.2f));

            ConfigureRenderer(go);
            return ps;
        }

        private ParticleSystem CreateFlame()
        {
            var go = new GameObject("FX_LlamaEncendedor");
            go.transform.SetParent(transform, false);
            var ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var main = ps.main;
            main.loop = true;
            main.playOnAwake = false;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.35f, 0.6f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.6f, 1.2f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.12f, 0.3f);
            main.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 0.85f, 0.3f), new Color(1f, 0.35f, 0.05f));
            main.gravityModifier = -0.3f;
            main.useUnscaledTime = true; // el final corre con el juego "congelado" en diálogos

            var emission = ps.emission;
            emission.rateOverTime = 45f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 8f;
            shape.radius = 0.05f;
            go.transform.rotation = Quaternion.Euler(-90f, 0f, 0f);

            var sizeOverLife = ps.sizeOverLifetime;
            sizeOverLife.enabled = true;
            sizeOverLife.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0f));

            ConfigureRenderer(go);
            return ps;
        }

        private void ConfigureRenderer(GameObject go)
        {
            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = particleMaterial;
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
        }

        /// <summary>
        /// Material de partículas: Assets/Resources/FX/Particula.mat (URP Particles/Unlit, aditivo),
        /// creado por el constructor de escenas. Se le asigna una textura circular suave generada en memoria.
        /// </summary>
        private static Material CreateParticleMaterial()
        {
            Material source = Resources.Load<Material>("FX/Particula");
            Material material;
            if (source != null)
            {
                material = new Material(source);
            }
            else
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
                if (shader == null) shader = Shader.Find("Sprites/Default");
                material = new Material(shader);
            }

            Texture2D soft = SoftCircle(32);
            if (material.HasProperty("_BaseMap")) material.SetTexture("_BaseMap", soft);
            if (material.HasProperty("_MainTex")) material.SetTexture("_MainTex", soft);
            return material;
        }

        private static Texture2D SoftCircle(int size)
        {
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false) { name = "CirculoSuave", wrapMode = TextureWrapMode.Clamp };
            var pixels = new Color32[size * size];
            float half = (size - 1) * 0.5f;
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float d = Mathf.Sqrt((x - half) * (x - half) + (y - half) * (y - half)) / half;
                byte a = (byte)(Mathf.Clamp01(1f - d) * Mathf.Clamp01(1f - d) * 255f);
                pixels[y * size + x] = new Color32(255, 255, 255, a);
            }
            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            return texture;
        }
    }
}
