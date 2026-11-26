using UnityEngine;

namespace FindTheLover.Equipment.Data
{
    public enum EquipmentType
    {
        None = 0,
        Torch = 1,
        Axe = 2,
        Sword = 3,
        Pickaxe = 4
    }

    /// <summary>
    /// Muck-like 5-tier material progression used for Modular Hand Socket tinting and stat scaling.
    /// </summary>
    public enum WeaponMaterialTier
    {
        Wood = 0,
        Stone = 1,
        Iron = 2,
        Obsidian = 3,
        Adamantite = 4
    }

    /// <summary>
    /// Data Layer: Flyweight blueprint holding static stats, material tier tint, procedural swing angles,
    /// and sprite/prefab presentation for an equippable tool or weapon.
    /// </summary>
    [CreateAssetMenu(fileName = "NewEquipmentItem", menuName = "FindTheLover/Equipment/Equipment Item")]
    public class EquipmentItemDataSO : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Unique technical identifier for GameData persistence and crafting.")]
        public string itemId = "item_torch";

        [Tooltip("In-game display name shown on Hotbar and Crafting UI.")]
        public string displayName = "Handheld Torch";

        [Tooltip("Functional category of this equipment.")]
        public EquipmentType equipmentType = EquipmentType.None;

        [Tooltip("Material progression tier (Wood -> Stone -> Iron -> Obsidian -> Adamantite).")]
        public WeaponMaterialTier materialTier = WeaponMaterialTier.Wood;

        [Header("Combat & Harvest Stats")]
        [Tooltip("Flat bonus damage added to PlayerCombat melee strikes while equipped.")]
        public float weaponDamageBonus = 0f;

        [Tooltip("Damage multiplier when harvesting matching resource nodes (Trees for Axe, Rocks for Pickaxe).")]
        [Range(0.5f, 5f)] public float harvestPowerMultiplier = 1f;

        [Header("Modular Hand Socket Presentation")]
        [Tooltip("Standalone 2D sprite rendered inside the Modular Hand Socket (no baked character spritesheet required).")]
        public Sprite weaponSprite;

        [Tooltip("Color tint applied to the modular weapon sprite to visually distinguish the 5 material tiers.")]
        public Color tierTintColor = Color.white;

        [Tooltip("Uniform local scale applied to the modular weapon sprite in hand.")]
        [Range(0.2f, 2.5f)] public float visualScale = 0.72f;

        [Tooltip("Optional custom prefab spawned instead of the modular sprite (e.g., animated Lantern or Torch).")]
        public GameObject heldPrefab;

        [Tooltip("Local position offset relative to Hand/Belt Socket.")]
        public Vector3 localOffset = Vector3.zero;

        [Tooltip("Local euler rotation relative to Hand/Belt Socket when idle.")]
        public Vector3 localRotation = Vector3.zero;

        [Header("Procedural Swing Arc Settings")]
        [Tooltip("Starting Z rotation angle (degrees) at the windup of a swing.")]
        public float swingStartAngle = 55f;

        [Tooltip("Ending Z rotation angle (degrees) at the follow-through of a swing.")]
        public float swingEndAngle = -105f;

        [Tooltip("Duration in seconds of one procedural swing animation.")]
        [Range(0.08f, 0.6f)] public float swingDuration = 0.22f;

        /// <summary>
        /// Returns the canonical visual accent color for a given material tier.
        /// </summary>
        public static Color GetCanonicalTierColor(WeaponMaterialTier tier)
        {
            switch (tier)
            {
                case WeaponMaterialTier.Wood:       return new Color(0.82f, 0.60f, 0.38f, 1f); // Warm Oak Wood
                case WeaponMaterialTier.Stone:      return new Color(0.74f, 0.78f, 0.82f, 1f); // Rugged Slate Stone
                case WeaponMaterialTier.Iron:       return new Color(0.95f, 0.97f, 1.00f, 1f); // Bright Forged Steel
                case WeaponMaterialTier.Obsidian:   return new Color(0.66f, 0.36f, 0.96f, 1f); // Runic Purple Obsidian
                case WeaponMaterialTier.Adamantite: return new Color(0.28f, 0.95f, 0.98f, 1f); // Radiant Cyan Adamantite
                default:                            return Color.white;
            }
        }
    }
}
