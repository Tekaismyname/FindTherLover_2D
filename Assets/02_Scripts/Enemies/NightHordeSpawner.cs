using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using FindTheLover.Environment;
using FindTheLover.Enemies.Data;

namespace FindTheLover.Enemies
{
    /// <summary>
    /// Spawns nocturnal monsters within a safe donut perimeter around the player during nighttime.
    /// Follows Event-Driven architecture by listening directly to DayNightCycleManager transitions.
    /// </summary>
    public class NightHordeSpawner : MonoBehaviour
    {
        [Header("Configuration")]
        [Tooltip("Blueprint containing spawn intervals, distance ranges, and simultaneous capacity.")]
        [SerializeField] private NightSpawnConfigSO _config;

        [Header("Player Reference")]
        [Tooltip("Transform of the player to anchor the donut spawn calculation around.")]
        [SerializeField] private Transform _playerTransform;

        private Coroutine _spawnCoroutine;

        // Tracks live enemies spawned by this controller to enforce max simultaneous limit
        private readonly List<EnemyAI> _activeNightEnemies = new List<EnemyAI>();

        private void Start()
        {
            LocatePlayer();

            // Subscribe to Day/Night lifecycle broadcast events
            if (DayNightCycleManager.Instance != null)
            {
                DayNightCycleManager.Instance.OnNightStarted += HandleNightStarted;
                DayNightCycleManager.Instance.OnDayStarted += HandleDayStarted;
            }
        }

        private void OnDestroy()
        {
            // Always unsubscribe from events on destruction to prevent memory leaks
            if (DayNightCycleManager.Instance != null)
            {
                DayNightCycleManager.Instance.OnNightStarted -= HandleNightStarted;
                DayNightCycleManager.Instance.OnDayStarted -= HandleDayStarted;
            }
        }

        /// <summary>
        /// Locates player in scene dynamically if not manually assigned in inspector.
        /// </summary>
        private void LocatePlayer()
        {
            if (_playerTransform != null) return;

            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                _playerTransform = player.transform;
            }
        }

        /// <summary>
        /// Callback triggered when nighttime begins: starts the recurring spawn routine.
        /// </summary>
        private void HandleNightStarted(int currentDay)
        {
            Debug.Log($"<color=red>[NightHordeSpawner]</color> Night has fallen! Starting nocturnal monster spawns.");
            if (_spawnCoroutine != null) StopCoroutine(_spawnCoroutine);
            _spawnCoroutine = StartCoroutine(SpawnRoutine());
        }

        /// <summary>
        /// Callback triggered when daytime arrives: halts spawns to allow resource gathering.
        /// </summary>
        private void HandleDayStarted(int currentDay)
        {
            Debug.Log($"<color=green>[NightHordeSpawner]</color> Dawn has broken! Stopping nocturnal monster spawns.");
            if (_spawnCoroutine != null)
            {
                StopCoroutine(_spawnCoroutine);
                _spawnCoroutine = null;
            }
        }

        /// <summary>
        /// Periodic spawn timer checking capacity limits before deploying monsters.
        /// </summary>
        private IEnumerator SpawnRoutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(_config.spawnInterval);

                // Purge defeated or disabled enemies from active list to accurately reflect live count
                _activeNightEnemies.RemoveAll(e => e == null || !e.gameObject.activeInHierarchy);

                // Skip spawn cycle if current monster count has reached the configured maximum
                if (_activeNightEnemies.Count >= _config.maxSimultaneousEnemies)
                {
                    continue;
                }

                TrySpawnEnemy();
            }
        }

        /// <summary>
        /// Computes donut polar coordinates and validates ground placement on the NavMesh.
        /// </summary>
        private void TrySpawnEnemy()
        {
            if (_playerTransform == null)
            {
                LocatePlayer();
                if (_playerTransform == null) return;
            }

            // 1. Generate random polar coordinates (angle between 0-360 deg, radius between min and max)
            float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float randomRadius = Random.Range(_config.minSpawnRadius, _config.maxSpawnRadius);

            // 2. Convert polar coordinates to Cartesian offset on horizontal plane (XZ)
            Vector3 offset = new Vector3(Mathf.Cos(randomAngle) * randomRadius, 0f, Mathf.Sin(randomAngle) * randomRadius);
            Vector3 rawTargetPosition = _playerTransform.position + offset;

            // 3. Raycast/Sample position against baked NavMesh geometry to guarantee solid ground
            if (NavMesh.SamplePosition(rawTargetPosition, out NavMeshHit hit, _config.navMeshSnapDistance, NavMesh.AllAreas))
            {
                // 4. Retrieve pre-allocated instance from object pool
                EnemyAI enemy = EnemyPoolManager.Instance.GetEnemy();
                if (enemy != null)
                {
                    enemy.SpawnAt(hit.position);
                    _activeNightEnemies.Add(enemy);
                    Debug.Log($"<color=orange>[NightHordeSpawner]</color> Deployed {enemy.name} into darkness at {hit.position}!");
                }
            }
        }
    }
}