using UnityEngine;

namespace FindTheLover.Enemies.Data
{
    /// <summary>
    /// Data Layer blueprint for enemy archetypes.
    /// Encapsulates survival stats, locomotion parameters, sensory ranges, and combat attributes.
    /// </summary>
    [CreateAssetMenu(fileName = "NewEnemyData", menuName = "FindTheLover/Combat/EnemyDataSO")]
    public class EnemyDataSO : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Descriptive name of the enemy archetype.")]
        public string enemyName = "Forest Goblin";

        [Header("Survival")]
        [Tooltip("Maximum health pool of the enemy.")]
        public float maxHealth = 40f;

        [Header("Locomotion & NavMesh")]
        [Tooltip("Movement speed in units per second.")]
        public float moveSpeed = 3.5f;

        [Tooltip("Turning rotation speed in degrees per second.")]
        public float angularSpeed = 480f;

        [Tooltip("Acceleration factor when changing velocity.")]
        public float acceleration = 12f;

        [Tooltip("Minimum stopping distance before reaching destination.")]
        public float stoppingDistance = 1.0f;

        [Header("Combat & Sensory")]
        [Tooltip("Spherical detection radius for spotting the player.")]
        public float detectRadius = 10f;

        [Tooltip("Melee engagement attack range in units.")]
        public float attackRange = 1.3f;

        [Tooltip("Damage dealt per attack hit.")]
        public float attackDamage = 8f;

        [Tooltip("Cooldown duration in seconds between successive strikes.")]
        public float attackCooldown = 1.5f;

        [Header("Audio SFX References")]
        [Tooltip("Audio clip played during attack execution.")]
        public AudioClip attackSound;

        [Tooltip("Audio clip played when taking damage.")]
        public AudioClip hurtSound;

        [Tooltip("Audio clip played upon entity death.")]
        public AudioClip deathSound;
    }
}