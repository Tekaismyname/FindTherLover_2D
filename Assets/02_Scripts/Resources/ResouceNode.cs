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

        //  IHarvestable
        public HarvestToolType RequiredTool => HarvestToolType.Axe;
        public float ToolEfficiencyMultiplier => 2.0f;

        public float CurrentHealth => _currentHealth;
        public bool IsDead => _isDead;

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
        /// Spawns physical loot items and bursts them outward.
        /// </summary>
        private void SpawnLoot()
        {
            if (resourceData == null || resourceData.dropPrefab == null) return;

            int count = Random.Range(resourceData.minDropCount, resourceData.maxDropCount + 1);
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

        public void Harvest(float baseDamage, HarvestToolType usedTool, Vector3 hitPoint)
        {
            float finalDamage = baseDamage;
            if (usedTool == RequiredTool)
            {
                finalDamage *= ToolEfficiencyMultiplier;
                Debug.Log($"<color=yellow>[Harvest]</color> Correct tool ({usedTool}) used! Damage x{ToolEfficiencyMultiplier} = {finalDamage}");
            }

            Vector3 hitNormal = (hitPoint - transform.position).normalized;
            TakeDamage(finalDamage, hitPoint, hitNormal);
        }
    }
}