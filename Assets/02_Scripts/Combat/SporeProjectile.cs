using System.Collections;
using UnityEngine;

namespace FindTheLover.Combat
{
    /// <summary>
    /// Logic Layer: Acid spore projectile fired by Spore Shroom.
    /// Travels towards target, deals direct impact damage, and inflicts poison DoT (Damage Over Time).
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class SporeProjectile : MonoBehaviour
    {
        [Header("Projectile Settings")]
        [Tooltip("Travel speed in meters per second.")]
        [SerializeField] private float moveSpeed = 8.5f;

        [Tooltip("Maximum lifetime in seconds before automatic cleanup.")]
        [SerializeField] private float maxLifetime = 4.0f;

        [Tooltip("Direct damage dealt on impact.")]
        [SerializeField] private float impactDamage = 10f;

        [Tooltip("Damage dealt per second by poison DoT (GDD: 2 HP/s).")]
        [SerializeField] private float poisonDamagePerSecond = 2f;

        [Tooltip("Duration in seconds of the poison DoT status effect (GDD: 5s).")]
        [SerializeField] private float poisonDuration = 5f;

        [Header("Visual & Audio References")]
        [Tooltip("Child animator controlling fly and hit sprite states.")]
        [SerializeField] private Animator animator;

        [Tooltip("Optional hit audio clip.")]
        [SerializeField] private AudioClip hitSound;

        private Vector3 _moveDirection = Vector3.forward;
        private GameObject _owner;
        private bool _hasHit;
        private Collider _collider;

        private static readonly int HitHash = Animator.StringToHash("Hit");

        private void Awake()
        {
            _collider = GetComponent<Collider>();
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }
        }

        /// <summary>
        /// Initializes trajectory, damage payload, and ignores owner collision.
        /// </summary>
        public void Initialize(Vector3 direction, float damage, GameObject owner, float dps = 2f, float duration = 5f)
        {
            direction.y = 0f;
            _moveDirection = direction.sqrMagnitude > 0.001f ? direction.normalized : Vector3.forward;
            impactDamage = damage;
            _owner = owner;
            poisonDamagePerSecond = dps;
            poisonDuration = duration;
            _hasHit = false;

            Destroy(gameObject, maxLifetime);
        }

        private void Update()
        {
            if (_hasHit) return;

            transform.position += _moveDirection * (moveSpeed * Time.deltaTime);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_hasHit) return;

            // Ignore the caster and fellow enemies
            if (_owner != null && (other.gameObject == _owner || other.transform.IsChildOf(_owner.transform)))
            {
                return;
            }

            if (other.GetComponentInParent<Enemies.EnemyAI>() != null)
            {
                return;
            }

            // Ignore trigger hazard volumes like CampfireSanctuary or PoisonGasZone
            if (other.isTrigger && !other.CompareTag("Player"))
            {
                return;
            }

            // Hit target checking
            if (other.CompareTag("Player") || other.GetComponentInParent<PlayerLocomotion.PlayerLocomotion>() != null)
            {
                IDamageable damageable = other.GetComponentInChildren<IDamageable>() ??
                                         other.GetComponentInParent<IDamageable>();

                if (damageable != null)
                {
                    _hasHit = true;
                    Vector3 hitPoint = other.ClosestPoint(transform.position);
                    Vector3 hitNormal = -_moveDirection;

                    // 1. Direct impact damage
                    damageable.TakeDamage(impactDamage, hitPoint, hitNormal);

                    // 2. Inflict Poison Status Effect (2 HP/s for 5s)
                    GameObject targetRoot = other.GetComponentInParent<PlayerLocomotion.PlayerLocomotion>()?.gameObject ?? other.gameObject;
                    PoisonStatusEffect poisonEffect = targetRoot.GetComponent<PoisonStatusEffect>();
                    if (poisonEffect == null)
                    {
                        poisonEffect = targetRoot.AddComponent<PoisonStatusEffect>();
                    }
                    poisonEffect.ApplyPoison(poisonDamagePerSecond, poisonDuration);

                    PlayHitFeedbackAndDestroy();
                    return;
                }
            }

            // Impact with solid environment obstacles
            if (!other.isTrigger)
            {
                _hasHit = true;
                PlayHitFeedbackAndDestroy();
            }
        }

        private void PlayHitFeedbackAndDestroy()
        {
            if (_collider != null) _collider.enabled = false;

            if (hitSound != null)
            {
                AudioSource.PlayClipAtPoint(hitSound, transform.position);
            }

            if (animator != null)
            {
                animator.SetTrigger(HitHash);
            }

            // Wait for hit explosion animation to finish (~0.28s) before destroy
            Destroy(gameObject, 0.3f);
        }
    }
}
