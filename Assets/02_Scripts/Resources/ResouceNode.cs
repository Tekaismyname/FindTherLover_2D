using System.Collections;
using FindTheLover.Combat;
using FindTheLover.Combat.Data;
using UnityEngine;

namespace FindTheLover.Environment
{
    /// <summary>
    /// Controller for destructible trees, rocks, and foliage.
    /// Implements IDamageable to receive strikes, plays elastic hit-bounce shake, and spawns loot.
    /// </summary>
    public class ResourceNode : MonoBehaviour, IDamageable,  IHarvestable
    {
        [Header("Data Configuration")]
        [Tooltip("Resource blueprint holding health, sounds, and drop parameters.")]
        [SerializeField] private ResourceDataSO resourceData;

        private float _currentHealth;
        private Vector3 _originalScale;
        private Coroutine _shakeCoroutine;
        private bool _isDead = false;

        /// <summary>
        /// Broadcast whenever any resource node is struck (finalDamage, hitPoint, wasBlockedByWrongTool, requiredTool).
        /// Observed by DamageNumberSpawner to pop floating harvest numbers or "NEED PICKAXE!" warnings.
        /// </summary>
        public static event System.Action<float, Vector3, bool, HarvestToolType> OnAnyResourceHarvested;

        // IHarvestable (Data-Driven from ResourceDataSO!)
        public HarvestToolType RequiredTool => resourceData != null ? resourceData.requiredTool : HarvestToolType.Axe;
        public float ToolEfficiencyMultiplier => resourceData != null ? resourceData.toolEfficiencyMultiplier : 2.0f;
        public ResourceDataSO ResourceData => resourceData;

        public float CurrentHealth => _currentHealth;
        public bool IsDead => _isDead;

        /// <summary>
        /// Assigns or overrides the blueprint data at runtime or in automated tests.
        /// </summary>
        public void ConfigureData(ResourceDataSO data)
        {
            resourceData = data;
            if (resourceData != null)
            {
                _currentHealth = resourceData.maxHealth;
                _isDead = false;
            }
        }

        private void Awake()
        {
            if (resourceData != null)
            {
                _currentHealth = resourceData.maxHealth;
            }
            _originalScale = transform.localScale;
        }

        /// <summary>
        /// Receives damage, triggers elastic squashing feedback, and destroys node if health depleted.
        /// </summary>
        public void TakeDamage(float damage, Vector3 hitPoint, Vector3 hitNormal)
        {
            if (_isDead) return;

            _currentHealth -= damage;

            // Trigger elastic hit-shake bounce (Juice)
            if (_shakeCoroutine != null) StopCoroutine(_shakeCoroutine);
            _shakeCoroutine = StartCoroutine(HitBounceRoutine(hitNormal));

            // Play hit audio
            if (resourceData != null && resourceData.hitSound != null)
            {
                AudioSource.PlayClipAtPoint(resourceData.hitSound, transform.position);
            }

            if (_currentHealth <= 0f)
            {
                Die();
            }
        }

        /// <summary>
        /// Elastic squash & stretch animation providing tactile combat feel (Juice).
        /// </summary>
        private IEnumerator HitBounceRoutine(Vector3 hitNormal)
        {
            float duration = resourceData != null ? resourceData.hitShakeDuration : 0.18f;
            float intensity = resourceData != null ? resourceData.hitShakeIntensity : 0.15f;
            float elapsed = 0f;

            // Squash scale: expands horizontally, compresses vertically
            Vector3 squashedScale = new Vector3(
                _originalScale.x * (1f + intensity * 0.5f),
                _originalScale.y * (1f - intensity),
                _originalScale.z * (1f + intensity * 0.5f)
            );

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                // Decaying sine wave for springy rubber bounce
                float curve = Mathf.Sin(t * Mathf.PI * 3f) * (1f - t);
                transform.localScale = Vector3.LerpUnclamped(_originalScale, squashedScale, curve);
                yield return null;
            }

            transform.localScale = _originalScale;
        }

        private void Die()
        {
            _isDead = true;

            // Play break sound
            if (resourceData != null && resourceData.breakSound != null)
            {
                AudioSource.PlayClipAtPoint(resourceData.breakSound, transform.position);
            }

            // Scatter loot
            SpawnLoot();

            // Destroy the tree/rock entity
            Destroy(gameObject);
        }

        /// <summary>
        /// Spawns physical loot items and bursts them outward, scaled by any active Harvest Yield Augments.
        /// </summary>
        private void SpawnLoot()
        {
            if (resourceData == null || resourceData.dropPrefab == null) return;

            int baseCount = Random.Range(resourceData.minDropCount, resourceData.maxDropCount + 1);
            float yieldMultiplier = 1f;
            var augmentStats = Object.FindAnyObjectByType<FindTheLover.Augments.PlayerAugmentStats>();
            if (augmentStats != null)
            {
                yieldMultiplier = augmentStats.HarvestMultiplier;
            }

            int count = Mathf.Max(1, Mathf.RoundToInt(baseCount * yieldMultiplier));
            Vector3 spawnOrigin = transform.position + Vector3.up * 2f;

            for (int i = 0; i < count; i++)
            {
                Vector3 burstDir = (Random.insideUnitSphere + Vector3.up * 1.2f).normalized;
                GameObject loot = Instantiate(resourceData.dropPrefab, spawnOrigin, Random.rotation);

                if (loot.TryGetComponent<Rigidbody>(out var rb))
                {
                    rb.AddForce(burstDir * resourceData.dropEjectionForce, ForceMode.Impulse);
                }
            }
        }

        /// <summary>
        /// Evaluates tool compatibility (Pickaxe for Stone/Ore/Crystal, Axe for Wood) and applies mining/chopping damage.
        /// </summary>
        public void Harvest(float baseDamage, HarvestToolType usedTool, Vector3 hitPoint)
        {
            if (_isDead) return;

            Vector3 hitNormal = (hitPoint - transform.position).normalized;
            float finalDamage = ComputeHarvestDamage(baseDamage, usedTool, resourceData);

            if (finalDamage <= 0f)
            {
                // Play elastic recoil bounce as tactile feedback that a stronger/matching tool is needed
                if (_shakeCoroutine != null) StopCoroutine(_shakeCoroutine);
                _shakeCoroutine = StartCoroutine(HitBounceRoutine(hitNormal));
                OnAnyResourceHarvested?.Invoke(0f, hitPoint, true, RequiredTool);
                Debug.Log($"<color=#FF9944>[Mining]</color> {resourceData?.resourceName ?? name} requires <b>{RequiredTool}</b>! Press [3] Pickaxe or [2] Axe.");
                return;
            }

            OnAnyResourceHarvested?.Invoke(finalDamage, hitPoint, false, RequiredTool);

            if (usedTool == RequiredTool)
            {
                Debug.Log($"<color=yellow>[Harvest/Mine]</color> Optimal tool ({usedTool}) hit {resourceData?.resourceName ?? name} for {finalDamage:F1} DMG!");
            }

            TakeDamage(finalDamage, hitPoint, hitNormal);
        }

        /// <summary>
        /// Pure deterministic formula calculating harvest/mining damage based on tool match and blueprint rules.
        /// </summary>
        public static float ComputeHarvestDamage(float baseDamage, HarvestToolType usedTool, ResourceDataSO data)
        {
            if (baseDamage <= 0f) return 0f;
            if (data == null) return baseDamage;

            if (usedTool == data.requiredTool)
            {
                return baseDamage * Mathf.Max(1f, data.toolEfficiencyMultiplier);
            }

            if (data.requireExactTool)
            {
                return 0f;
            }

            return baseDamage * Mathf.Clamp01(data.wrongToolDamageMultiplier);
        }
    }
}