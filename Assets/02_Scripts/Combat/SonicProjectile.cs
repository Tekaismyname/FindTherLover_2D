using UnityEngine;
using FindTheLover.Combat;

namespace FindTheLover.Combat
{
    /// <summary>
    /// Presentation & Logic Layer: Sonic Shockwave Projectile fired by the Shadow Bat.
    /// Traverses horizontally in 3D space, damaging and staggering players upon contact.
    /// </summary>
    [RequireComponent(typeof(SphereCollider))]
    public class SonicProjectile : MonoBehaviour
    {
        [Header("Movement & Ballistics")]
        [Tooltip("Forward velocity in world units per second.")]
        [SerializeField] private float moveSpeed = 10.0f;

        [Tooltip("Maximum lifetime before automatic expiration.")]
        [SerializeField] private float maxLifetime = 3.5f;

        [Header("Damage Configuration")]
        [Tooltip("Direct kinetic damage delivered upon player impact.")]
        [SerializeField] private float impactDamage = 8.0f;

        [Tooltip("Knockback force imparted to the player upon contact.")]
        [SerializeField] private float knockbackForce = 6.0f;

        [Header("Visual Effects")]
        [Tooltip("Optional impact explosion prefab.")]
        [SerializeField] private GameObject impactVfxPrefab;

        private float _spawnTime;
        private bool _hasImpacted = false;

        private void Start()
        {
            _spawnTime = Time.time;
        }

        private void Update()
        {
            if (_hasImpacted) return;

            // Translate forward in world space
            transform.position += transform.forward * (moveSpeed * Time.deltaTime);

            if (Time.time >= _spawnTime + maxLifetime)
            {
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_hasImpacted) return;
            if (other.isTrigger) return;
            if (other.CompareTag("Enemy")) return; // Do not hit fellow enemies

            // Search for IDamageable on target or its hierarchy
            var damageable = other.GetComponentInChildren<IDamageable>() ?? other.GetComponentInParent<IDamageable>();
            if (damageable != null)
            {
                Vector3 hitPoint = other.ClosestPoint(transform.position);
                Vector3 hitNormal = transform.forward;
                damageable.TakeDamage(impactDamage, hitPoint, hitNormal);
                TriggerImpact();
            }
            else if (other.gameObject.layer == LayerMask.NameToLayer("Default") || other.CompareTag("Obstacle"))
            {
                TriggerImpact();
            }
        }

        private void TriggerImpact()
        {
            _hasImpacted = true;

            if (impactVfxPrefab != null)
            {
                Instantiate(impactVfxPrefab, transform.position, Quaternion.identity);
            }

            Destroy(gameObject);
        }
    }
}
