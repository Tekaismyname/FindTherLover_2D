using UnityEngine;

namespace FindTheLover.Combat.Data
{
    public enum ResourceType
    {
        Wood,
        Stone,
        Iron,
        Bush,
        Crystal,
        Obsidian
    }
    /// <summary>
    /// Data Layer Blueprint for harvestable resource nodes (Trees, Stone Boulders, Iron Veins, Amber Crystals).
    /// Encapsulates durability, required harvest tool, hit juice feedback, and loot drop specifications.
    /// </summary>

    [CreateAssetMenu(fileName = "NewResourceData", menuName = "FindTheLover/Resources/ResourceDataSO")]
    public class ResourceDataSO : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Descriptive name of the harvestable node.")]
        public string resourceName = "Oak Tree";
        public ResourceType resourceType = ResourceType.Wood;

        [Header("Harvest Tool Requirements")]
        [Tooltip("Optimal tool required to harvest or mine this node efficiently (Axe for Wood, Pickaxe for Stone/Ore/Crystal).")]
        public HarvestToolType requiredTool = HarvestToolType.Axe;

        [Tooltip("Damage multiplier when struck with the matching optimal tool.")]
        [Range(1.0f, 5.0f)] public float toolEfficiencyMultiplier = 2.0f;

        [Tooltip("Damage multiplier when struck with a non-matching tool (if requireExactTool is false).")]
        [Range(0.0f, 1.0f)] public float wrongToolDamageMultiplier = 0.25f;

        [Tooltip("When enabled, hard veins (Iron Ore, Amber Crystal, Obsidian) require the exact matching tool to deal damage.")]
        public bool requireExactTool = false;

        [Header("Durability")]
        [Tooltip("Maximum hit points before the node breaks.")]
        public float maxHealth = 60f;

        [Header("hit Juice & Feedback")]
        [Range(0.05f, 0.4f)] public float hitShakeIntensity = 0.15f;

        [Tooltip("Duration of the hit bounce animation in seconnds.")]
        [Range(0.05f, 0.4f)] public float hitShakeDuration = 0.18f;

        [Tooltip("Audio  clip played when struck.")]
        public AudioClip hitSound;

        [Tooltip("Audio clip played when broken.")]
        public AudioClip breakSound;

        [Header("Loot Drops")]
        [Tooltip("Physical item prefab spawned unpon destruction (e.g Item_Wood,Stone...)")]
        public GameObject dropPrefab;

        [Tooltip("Minimum number of items dropped.")]
        [Range(1, 10)] public int minDropCount = 2;

        [Tooltip("Maximun n umber of items dropped.")]
        [Range(1, 10)] public int maxDropCount = 4;

        [Tooltip("Explosive outward ejection force applied to loot items.")]
        public float dropEjectionForce = 4.0f;
    }
}