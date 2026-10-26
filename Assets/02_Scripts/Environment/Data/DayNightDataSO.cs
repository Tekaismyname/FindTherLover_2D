using UnityEngine;

namespace FindTheLover.Environment.Data
{

    /// <summary>
    /// Blueprint configuring the Day/Night cycle timings, colors, and fog atmosphere.
    /// Follows Data-Driven Architecture for seamless designer balancing.
    /// </summary>
    [CreateAssetMenu(fileName = "DayNight_Config", menuName = "Game/Environtment/Day Night Config")]
    public class DayNightDataSO : ScriptableObject
    {
        [Header("Timing Settings")]
        [Tooltip("Full day duration in real-world seconds (e.g, 600s = 10 minutes)")]
        [Range(10f, 1800f)] public float dayDurationInSeconds = 120f;

        [Tooltip("Time value (0 to 1) when daytime officially starts")]
        [Range(0f, 1f)] public float dayStartNormalized = 0.2f;

        [Tooltip("time value (0 to 1) when nightime officially start")]
        [Range(0f, 1f)] public float nightStartNormalized = 0.7f;

        [Header("Lighting Gradients")]
        [Tooltip("Color fo the sun Directional light across the entire day")]
        public Gradient sunColorGradient;

        [Tooltip("Intensity multiplelier of the sun light across entire day")]
        public AnimationCurve sunIntensityCurve;

        [Header("Atmosphere & Fog")]
        [Tooltip("Fog colr transition across the cycle")]
        public Gradient fogColorGradient;

        [Tooltip("Fog density curve across the cycle (denser at midnight)")]
        public AnimationCurve fogDensityCurve;
    }
}