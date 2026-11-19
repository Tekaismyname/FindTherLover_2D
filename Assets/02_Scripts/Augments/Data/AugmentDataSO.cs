using UnityEngine;

namespace FindTheLover.Augments.Data
{
    /// <summary>
    /// Data Layer (ScriptableObject): Flyweight blueprint defining a TFT-style 3-tier augment card.
    /// Zero-code extensibility: Designers can author new Bronze, Silver, or Gold augments directly in Inspector.
    /// </summary>
    [CreateAssetMenu(fileName = "Augment_Data", menuName = "Find The Lover/Augments/Augment Data")]
    public class AugmentDataSO : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Unique identifier preventing duplicate rolls within the same selection modal.")]
        public string augmentId = "aug_bronze_01";

        [Tooltip("Card title displayed on the TFT selection modal.")]
        public string displayName = "Wind Sneakers";

        [Tooltip("Concise gameplay description shown beneath the card icon.")]
        [TextArea(2, 4)]
        public string description = "Increases movement speed by 20%.";

        [Tooltip("Icon rendered inside the augment card UI.")]
        public Sprite icon;

        [Header("Classification")]
        public AugmentTier tier = AugmentTier.Bronze;
        public AugmentCategory category = AugmentCategory.StatModifier;
        public AugmentEffectType effectType = AugmentEffectType.MoveSpeedPercent;

        [Header("Numerical Parameters")]
        [Tooltip("Primary effect magnitude (e.g., 0.20 for +20% speed, 40 for +40 HP, 120 for lightning damage).")]
        public float primaryValue = 0.20f;

        [Tooltip("Secondary effect parameter (e.g., chain target count, cooldown seconds, AoE radius).")]
        public float secondaryValue = 0f;

        [Header("Stacking Rules")]
        [Tooltip("Whether the same augment can be selected more than once across a single run.")]
        public bool allowStacking = false;

        [Tooltip("Maximum stack limit when allowStacking is enabled.")]
        [Min(1)] public int maxStacks = 1;

        /// <summary>
        /// Returns the canonical TFT-inspired glowing border color for this augment's tier.
        /// </summary>
        public Color GetTierAccentColor()
        {
            switch (tier)
            {
                case AugmentTier.Gold:
                    return new Color(1.0f, 0.84f, 0.18f, 1.0f); // Radiant Gold
                case AugmentTier.Silver:
                    return new Color(0.75f, 0.88f, 1.0f, 1.0f); // Gleaming Prismatic Silver
                default:
                    return new Color(0.84f, 0.52f, 0.24f, 1.0f); // Burnished Bronze
            }
        }

        /// <summary>
        /// Returns an uppercase label representing the tier ribbon text.
        /// </summary>
        public string GetTierLabel()
        {
            switch (tier)
            {
                case AugmentTier.Gold: return "GOLD TIER";
                case AugmentTier.Silver: return "SILVER TIER";
                default: return "BRONZE TIER";
            }
        }
    }
}
