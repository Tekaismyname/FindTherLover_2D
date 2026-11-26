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

        [Header("Stacking & 3-Level Upgrade Rules (Lv.1 -> Lv.2 -> Lv.3)")]
        [Tooltip("Whether the same augment can be upgraded across multiple levels (Lv.1 -> Lv.2 -> Lv.3).")]
        public bool allowStacking = true;

        [Tooltip("Maximum upgrade level when allowStacking is enabled (default 3 for Lv.1/Lv.2/Lv.3).")]
        [Min(1)] public int maxStacks = 3;

        [Tooltip("Primary effect multiplier table for Lv.1, Lv.2, and Lv.3.")]
        public float[] levelPrimaryMultipliers = new float[] { 1.0f, 1.75f, 2.75f };

        /// <summary>Maximum achievable level for this augment.</summary>
        public int MaxLevel => allowStacking ? Mathf.Max(1, maxStacks) : 1;

        /// <summary>
        /// Computes the scaled primary value for a target augment level (1-based: Lv.1, Lv.2, Lv.3).
        /// </summary>
        public float GetPrimaryValueForLevel(int level)
        {
            int clamped = Mathf.Clamp(level, 1, MaxLevel);
            if (levelPrimaryMultipliers != null && levelPrimaryMultipliers.Length > 0)
            {
                int idx = Mathf.Clamp(clamped - 1, 0, levelPrimaryMultipliers.Length - 1);
                return primaryValue * levelPrimaryMultipliers[idx];
            }
            return primaryValue * clamped;
        }

        /// <summary>
        /// Computes the scaled secondary parameter for a target augment level (e.g., extra Chain Lightning targets).
        /// </summary>
        public float GetSecondaryValueForLevel(int level)
        {
            int clamped = Mathf.Clamp(level, 1, MaxLevel);
            if (secondaryValue <= 0f) return 0f;

            switch (effectType)
            {
                case AugmentEffectType.ChainLightningOnHit:
                    return secondaryValue + (clamped - 1); // +1 bounce target per level
                case AugmentEffectType.RegeneratingShield:
                case AugmentEffectType.CelestialThunderstorm:
                    return Mathf.Max(2.5f, secondaryValue * Mathf.Pow(0.85f, clamped - 1)); // Faster cooldown per level
                default:
                    return secondaryValue * (1f + 0.5f * (clamped - 1));
            }
        }

        /// <summary>
        /// Formats the TFT upgrade badge text shown on cards ("NEW (LV.1)", "LV.1 -> LV.2", "MAX LV.2 -> LV.3").
        /// </summary>
        public static string FormatUpgradeBadge(int currentLevel, int maxLevel)
        {
            int clampedMax = Mathf.Max(1, maxLevel);
            if (currentLevel <= 0) return "NEW  (LV.1)";
            if (currentLevel >= clampedMax) return $"MAXED  (LV.{clampedMax})";
            int nextLevel = currentLevel + 1;
            return nextLevel >= clampedMax
                ? $"MAX UPGRADE  LV.{currentLevel} -> LV.{nextLevel}"
                : $"UPGRADE  LV.{currentLevel} -> LV.{nextLevel}";
        }

        /// <summary>
        /// Builds a level-aware description highlighting the exact Lv.1/Lv.2/Lv.3 power jump on the card UI.
        /// </summary>
        public string GetLevelAwareDescription(int currentOwnedLevel)
        {
            int nextLevel = Mathf.Clamp(currentOwnedLevel + 1, 1, MaxLevel);
            if (nextLevel <= 1)
            {
                return description;
            }

            float prevMult = levelPrimaryMultipliers != null && levelPrimaryMultipliers.Length > 0
                ? levelPrimaryMultipliers[Mathf.Clamp(nextLevel - 2, 0, levelPrimaryMultipliers.Length - 1)]
                : (nextLevel - 1);
            float nextMult = levelPrimaryMultipliers != null && levelPrimaryMultipliers.Length > 0
                ? levelPrimaryMultipliers[Mathf.Clamp(nextLevel - 1, 0, levelPrimaryMultipliers.Length - 1)]
                : nextLevel;

            return $"{description}\n<color=#FFE066><b>[Lv.{nextLevel} Power: {prevMult:0.##}x -> {nextMult:0.##}x]</b></color>";
        }

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
