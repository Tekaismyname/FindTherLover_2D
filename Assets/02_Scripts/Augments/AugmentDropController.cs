using UnityEngine;
using FindTheLover.Augments.Data;
using FindTheLover.Enemies;
using FindTheLover.Environment;

namespace FindTheLover.Augments
{
    /// <summary>
    /// Logic Layer: Event-Driven Augment Orb spawner subscribing to EnemyAI.OnAnyEnemyDefeated.
    /// Drops guaranteed Silver/Gold orbs from Elite raiders and Calamity Bosses, and occasional Bronze orbs from regular foes.
    /// </summary>
    public class AugmentDropController : MonoBehaviour
    {
        [Header("Drop Prefab")]
        [Tooltip("Collectible AugmentOrb prefab spawned at defeated enemy positions.")]
        [SerializeField] private AugmentOrb _augmentOrbPrefab;

        [Header("Drop Probabilities")]
        [Tooltip("Drop chance (0..1) when defeating a standard enemy.")]
        [Range(0f, 1f)] [SerializeField] private float _standardEnemyDropChance = 0.15f;

        [Tooltip("Drop chance (0..1) when defeating an Elite night raider (default: 100%).")]
        [Range(0f, 1f)] [SerializeField] private float _eliteEnemyDropChance = 1.0f;

        [Tooltip("Vertical spawn elevation above the defeated enemy's ground point.")]
        [SerializeField] private float _dropHeightOffset = 0.9f;

        private void OnEnable()
        {
            EnemyAI.OnAnyEnemyDefeated += HandleEnemyDefeated;
        }

        private void OnDisable()
        {
            EnemyAI.OnAnyEnemyDefeated -= HandleEnemyDefeated;
        }

        /// <summary>
        /// Determines the guaranteed minimum tier dropped based on enemy rank and current survival day.
        /// </summary>
        public static AugmentTier DetermineDropTier(bool isElite, int currentDay, bool isCalamityNight)
        {
            if (isElite && (isCalamityNight || currentDay >= 10))
            {
                return AugmentTier.Gold;
            }

            if (isElite || currentDay >= 5)
            {
                return AugmentTier.Silver;
            }

            return AugmentTier.Bronze;
        }

        private void HandleEnemyDefeated(EnemyAI enemy, Vector3 deathPosition)
        {
            if (_augmentOrbPrefab == null || enemy == null) return;

            bool isElite = enemy.IsElite;
            float chance = isElite ? _eliteEnemyDropChance : _standardEnemyDropChance;
            if (Random.value > chance) return;

            int currentDay = DayNightCycleManager.Instance != null ? DayNightCycleManager.Instance.CurrentDay : 1;
            bool isCalamity = NightRaidDirector.Instance != null && NightRaidDirector.Instance.IsCalamityNight;

            AugmentTier dropTier = DetermineDropTier(isElite, currentDay, isCalamity);
            Vector3 spawnPos = deathPosition + Vector3.up * _dropHeightOffset;

            AugmentOrb orb = Instantiate(_augmentOrbPrefab, spawnPos, Quaternion.identity);
            orb.InitializeTier(dropTier);

            Debug.Log($"<color=#FFD700>[AugmentDropController]</color> Dropped [{dropTier}] Augment Orb from {enemy.name}!");
        }
    }
}
