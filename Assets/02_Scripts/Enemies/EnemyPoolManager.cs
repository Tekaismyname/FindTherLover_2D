using System.Collections.Generic;
using UnityEngine;

namespace FindTheLover.Enemies
{
    /// <summary>
    /// High-performance Object Pool managing pre-allocated enemy instances.
    /// Eliminates garbage collection spikes caused by runtime Instantiate and Destroy.
    /// </summary>
    public class EnemyPoolManager : MonoBehaviour
    {
        public static EnemyPoolManager Instance { get; private set; }

        [Header("Pool configuration")]
        [Tooltip("Enemy prefab with enemy AI and healthsystem attached")]
        [SerializeField] private EnemyAI _enemyPrefab;

        [Tooltip("Number of instances pre-allocated on awake")]
        [Range(5, 50)]
        [SerializeField] private int _initialPoolSize = 15;

        private readonly Queue<EnemyAI> _poolQueue = new Queue<EnemyAI>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            PrewarmPool();
        }

        /// <summary>
        /// Instantiates pool objects upfront during initialization so runtime gameplay is zero-allocation.
        /// </summary>


        private void PrewarmPool()
        {
            if (_enemyPrefab == null)
            {
                Debug.LogError("[EnemyPoolManager] Missing Enemy Prefab reference!", this);
                return;
            }

            for (int i = 0; i < _initialPoolSize; i++)
            {
                EnemyAI enemy = Instantiate(_enemyPrefab, transform);
                enemy.gameObject.SetActive(false);
                _poolQueue.Enqueue(enemy);
            }

        }

        /// <summary>
        /// Retrieves an idle enemy from the pool, or instantiates an overflow instance if exhausted.
        /// </summary>
        public EnemyAI GetEnemy()
        {
            EnemyAI enemy;

            if (_poolQueue.Count > 0)
            {
                enemy = _poolQueue.Dequeue();
            }
            else
            {
                // Dynamic overflow safeguard if spawn demand exceeds prewarmed pool
                enemy = Instantiate(_enemyPrefab, transform);
            }
            return enemy;
        }


        /// <summary>
        /// Returns a defeated enemy back into the pool for future reuse.
        /// </summary>
        public void ReturnEnemy(EnemyAI enemy)
        {
            if (enemy == null) return;

            enemy.gameObject.SetActive(false);
            enemy.transform.SetParent(transform);

            if (!_poolQueue.Contains(enemy))
            {
                _poolQueue.Enqueue(enemy);
            }
        }


        
    }
}