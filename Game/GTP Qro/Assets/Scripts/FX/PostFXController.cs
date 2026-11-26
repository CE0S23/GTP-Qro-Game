using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace GTP
{
    /// <summary>
    /// Post-processing de cada escena (rúbrica DE 3.1). Va en el Volume global que crea el constructor:
    /// el perfil base (Bloom, Vignette, Color Adjustments, Tonemapping) se define por escena y este
    /// componente lo anima en tiempo real como retroalimentación:
    /// - La viñeta se tiñe de rojo y se cierra mientras una amenaza te está viendo (sospecha 0..1).
    /// - Destello al perder un intento (canal <see cref="GameEvents.PlayerCaught"/>).
    /// - Cambios de "ánimo" en los finales: amanecer cálido (Bien) o saturado → gris (Mal).
    /// Trabaja sobre una copia del perfil (volume.profile), así nunca modifica el asset en disco.
    /// </summary>
    [RequireComponent(typeof(Volume))]
    public sealed class PostFXController : MonoBehaviour
    {
        public enum Mood { Normal, CalidoAmanecer, Saturado, Desaturado }

        public static PostFXController Current { get; private set; }

        [SerializeField] private Color dangerColor = new Color(0.75f, 0.05f, 0.05f);
        [SerializeField, Range(0f, 1f)] private float dangerVignette = 0.55f;
        [SerializeField, Min(0.05f)] private float flashSeconds = 0.6f;

        private VolumeProfile runtimeProfile;
        private Vignette vignette;
        private ColorAdjustments color;
        private ChromaticAberration chromatic;
        private Color baseVignetteColor;
        private float baseVignetteIntensity, baseSaturation, baseExposure, baseChromatic;
        private Color baseFilter;
        private float flash;
        private float smoothedSuspicion;

        // Transición de ánimo
        private float moodT = 1f, moodDuration = 1f;
        private float targetSaturation, targetExposure, targetChromatic, fromSaturation, fromExposure, fromChromatic;
        private Color targetFilter, fromFilter;

        public float CurrentVignetteIntensity => vignette != null ? vignette.intensity.value : 0f;

        private void Awake()
        {
            Current = this;
            VolumeProfile profile = GetComponent<Volume>().profile; // instancia en tiempo de ejecución
            if (profile == null) return;
            runtimeProfile = profile;
            profile.TryGet(out vignette);
            profile.TryGet(out color);
            profile.TryGet(out chromatic);

            if (vignette != null)
            {
                vignette.color.overrideState = true;
                vignette.intensity.overrideState = true;
                baseVignetteColor = vignette.color.value;
                baseVignetteIntensity = vignette.intensity.value;
            }
            if (color != null)
            {
                color.saturation.overrideState = true;
                color.postExposure.overrideState = true;
                color.colorFilter.overrideState = true;
                baseSaturation = color.saturation.value;
                baseExposure = color.postExposure.value;
                baseFilter = color.colorFilter.value;
            }
            if (chromatic != null)
            {
                chromatic.intensity.overrideState = true;
                baseChromatic = chromatic.intensity.value;
            }
            targetSaturation = fromSaturation = baseSaturation;
            targetExposure = fromExposure = baseExposure;
            targetChromatic = fromChromatic = baseChromatic;
            targetFilter = fromFilter = baseFilter;
        }

        private void OnEnable() => GameEvents.PlayerCaught.Raised += OnCaught;
        private void OnDisable() => GameEvents.PlayerCaught.Raised -= OnCaught;

        private void OnDestroy()
        {
            if (Current == this) Current = null;
            // volume.profile creó una copia en tiempo de ejecución: se libera junto con la escena.
            if (runtimeProfile != null) Destroy(runtimeProfile);
        }

        private void OnCaught(Vector3 _) => flash = 1f;

        /// <summary>Cambia el ánimo visual de la escena en <paramref name="seconds"/> (tiempo real).</summary>
        public void SetMood(Mood mood, float seconds)
        {
            if (color == null) return;
            fromSaturation = color.saturation.value;
            fromExposure = color.postExposure.value;
            fromFilter = color.colorFilter.value;
            fromChromatic = chromatic != null ? chromatic.intensity.value : 0f;
            moodDuration = Mathf.Max(0.01f, seconds);
            moodT = 0f;

            switch (mood)
            {
                case Mood.CalidoAmanecer:
                    targetSaturation = 15f; targetExposure = 0.35f; targetFilter = new Color(1f, 0.92f, 0.8f); targetChromatic = 0f;
                    break;
                case Mood.Saturado:
                    targetSaturation = 80f; targetExposure = 0.4f; targetFilter = new Color(1f, 0.75f, 1f); targetChromatic = 0.8f;
                    break;
                case Mood.Desaturado:
                    targetSaturation = -85f; targetExposure = -0.4f; targetFilter = new Color(0.85f, 0.85f, 0.9f); targetChromatic = 0.1f;
                    break;
                default:
                    targetSaturation = baseSaturation; targetExposure = baseExposure; targetFilter = baseFilter; targetChromatic = baseChromatic;
                    break;
            }
        }

        private void Update()
        {
            float dt = Time.unscaledDeltaTime;
            smoothedSuspicion = Mathf.MoveTowards(smoothedSuspicion, ThreatDetector.MaxSuspicion01, dt * 3f);
            flash = Mathf.Max(0f, flash - dt / flashSeconds);
            float danger = Mathf.Max(smoothedSuspicion, flash);

            if (vignette != null)
            {
                float pulse = smoothedSuspicion > 0.01f ? 0.06f * Mathf.Sin(Time.unscaledTime * 10f) * smoothedSuspicion : 0f;
                vignette.intensity.value = Mathf.Lerp(baseVignetteIntensity, dangerVignette, danger) + pulse;
                vignette.color.value = Color.Lerp(baseVignetteColor, dangerColor, danger);
            }

            if (moodT < 1f && color != null)
            {
                moodT = Mathf.Min(1f, moodT + dt / moodDuration);
                float k = Mathf.SmoothStep(0f, 1f, moodT);
                color.saturation.value = Mathf.Lerp(fromSaturation, targetSaturation, k);
                color.postExposure.value = Mathf.Lerp(fromExposure, targetExposure, k);
                color.colorFilter.value = Color.Lerp(fromFilter, targetFilter, k);
                if (chromatic != null) chromatic.intensity.value = Mathf.Lerp(fromChromatic, targetChromatic, k);
            }
        }
    }
}
