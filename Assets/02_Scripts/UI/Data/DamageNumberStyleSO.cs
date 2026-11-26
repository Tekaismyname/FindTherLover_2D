using UnityEngine;

namespace FindTheLover.UI.Data
{
    public enum DamagePopupStyle
    {
        Normal,
        Critical,
        Harvest,
        Heal,
        Blocked,
        PlayerHurt
    }

    /// <summary>
    /// Data Layer (ScriptableObject): Blueprint configuring floating combat damage number appearance,
    /// animation motion, colors per hit category, and object pool capacity.
    /// Zero hardcoded combat feedback constants in controller scripts.
    /// </summary>
    [CreateAssetMenu(fileName = "DamageNumber_Style", menuName = "Find The Lover/UI/Damage Number Style")]
    public class DamageNumberStyleSO : ScriptableObject
    {
        [Header("Motion & Lifetime")]
        [Tooltip("Total duration (seconds) a damage popup remains visible before returning to the pool.")]
        [Range(0.25f, 2.0f)] public float lifetime = 0.85f;

        [Tooltip("Upward float height (meters) reached over the popup's lifetime.")]
        [Range(0.5f, 4.0f)] public float riseHeight = 1.85f;

        [Tooltip("Initial vertical spawn offset above the exact impact point.")]
        public float verticalSpawnOffset = 0.9f;

        [Tooltip("Random horizontal scatter range (meters) so rapid multi-hits never overlap identically.")]
        [Range(0f, 1.5f)] public float horizontalScatter = 0.55f;

        [Tooltip("Normalized time (0..1) when alpha fade-out begins.")]
        [Range(0.2f, 0.9f)] public float fadeStartNormalized = 0.55f;

        [Header("Typography & Punch Scale")]
        [Tooltip("Base world-space TextMeshPro font size for standard damage numbers.")]
        public float baseFontSize = 6.5f;

        [Tooltip("Peak pop-in scale multiplier for normal hits.")]
        public float normalPeakScale = 1.25f;

        [Tooltip("Peak pop-in scale multiplier for Combo Finisher / Critical hits.")]
        public float criticalPeakScale = 1.65f;

        [Header("Category Colors")]
        public Color normalColor = new Color(1.00f, 0.97f, 0.88f, 1f);
        public Color criticalColor = new Color(1.00f, 0.72f, 0.14f, 1f);
        public Color harvestColor = new Color(0.95f, 0.84f, 0.42f, 1f);
        public Color healColor = new Color(0.28f, 0.95f, 0.46f, 1f);
        public Color blockedColor = new Color(1.00f, 0.42f, 0.25f, 1f);
        public Color playerHurtColor = new Color(1.00f, 0.22f, 0.26f, 1f);

        [Header("Zero-Allocation Object Pool")]
        [Tooltip("Number of popups pre-instantiated at scene startup to guarantee 0 GC Alloc in combat.")]
        [Range(8, 64)] public int prewarmPoolSize = 24;

        /// <summary>
        /// Resolves the canonical color associated with a floating popup category.
        /// </summary>
        public Color GetColorForStyle(DamagePopupStyle style)
        {
            return style switch
            {
                DamagePopupStyle.Critical => criticalColor,
                DamagePopupStyle.Harvest => harvestColor,
                DamagePopupStyle.Heal => healColor,
                DamagePopupStyle.Blocked => blockedColor,
                DamagePopupStyle.PlayerHurt => playerHurtColor,
                _ => normalColor
            };
        }

        /// <summary>
        /// Resolves the peak punch scale associated with a floating popup category.
        /// </summary>
        public float GetPeakScaleForStyle(DamagePopupStyle style)
        {
            return style switch
            {
                DamagePopupStyle.Critical => criticalPeakScale,
                DamagePopupStyle.Blocked => 1.05f,
                _ => normalPeakScale
            };
        }
    }
}
