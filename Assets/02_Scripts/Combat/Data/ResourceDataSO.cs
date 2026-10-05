using UnityEngine;

namespace FindTheLover.Combat.Data
{
    public enum ResourceType
    {
        Wood,
        Stone,
        Iron,
        Bush
    }
    /// <summary>
    /// Data Layer Blueprint for harvestable resource nodes.
    /// Encapsulates durability, hit juice feedback, and loot drop specifications.
    /// </summary>

    [CreateAssetMenu(fileName = "NewResourceData", menuName = "FindTheLover/Resources/ResourceDataSO")]
    public class ResourceDataSO : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Descriptive name of the harverstable node.")]
        public string resourceName = "Oak Tree";
        public ResourceType resourceType = ResourceType.Wood;

        [Header("Durability")]
        [Tooltip("Maximun hit points before the node collapses.")]
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