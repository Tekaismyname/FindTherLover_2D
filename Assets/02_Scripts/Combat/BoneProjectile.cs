using UnityEngine;

namespace FindTheLover.Combat
{
    /// <summary>
    /// Logic Layer: Linear projectile launched by the Skeleton Goblin.
    /// Flies along a horizontal direction vector, deals damage to the Player upon trigger contact,
    /// and self-destructs after its maximum lifetime expires.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class BoneProjectile : MonoBehaviour
    {
        [Header("Projectile Settings")]
        [Tooltip("Travel speed in meters per second.")]
        [SerializeField] private float moveSpeed = 9f;

        [Tooltip("Maximum lifetime in seconds before automatic cleanup.")]
        [SerializeField] private float maxLifetime = 3.5f;

        [Tooltip("Degrees per second to spin the visual child sprite.")]
        [SerializeField] private float spinSpeed = 720f;

        [Tooltip("Visual child transform to rotate while flying.")]
        [SerializeField] private Transform visualTransform;

        private Vector3 _moveDirection = Vector3.forward;
        private float _damage = 10f;
        private GameObject _owner;
        private bool _hasHit;

        private void Awake()
        {
            if (visualTransform == null && transform.childCount > 0)
            {
                visualTransform = transform.GetChild(0);
            }
        }

        /// <summary>
        /// Initializes projectile trajectory, damage payload, and owner reference to prevent self-collision.
        /// </summary>
        /// <param name="direction">World direction vector toward the target.</param>
        /// <param name="damage">Damage dealt on contact.</param>
        /// <param name="owner">The enemy GameObject that spawned this projectile.</param>
        public void Initialize(Vector3 direction, float damage, GameObject owner)
        {
            direction.y = 0f;
            _moveDirection = direction.sqrMagnitude > 0.001f ? direction.normalized : Vector3.forward;
            _damage = damage;
            _owner = owner;
            _hasHit = false;

            Destroy(gameObject, maxLifetime);
        }

        private void Update()
        {
            if (_hasHit) return;

            // 1. Displace projectile linearly across the horizontal plane
            transform.position += _moveDirection * (moveSpeed * Time.deltaTime);

            // 2. Spin visual sprite for dynamic throwing bone feedback
            if (visualTransform != null)
            {
                visualTransform.Rotate(0f, 0f, -spinSpeed * Time.deltaTime, Space.Self);
            }
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

            // Ignore non-solid trigger volumes (such as CampfireSanctuary or detection zones)
            if (other.isTrigger && !other.CompareTag("Player"))
            {
                return;
            }

            // Check if the hit object belongs to the Player or an IDamageable target
            if (other.CompareTag("Player") || other.GetComponentInParent<PlayerLocomotion.PlayerLocomotion>() != null)
            {
                IDamageable damageable = other.GetComponentInChildren<IDamageable>() ??
                                         other.GetComponentInParent<IDamageable>();

                if (damageable != null)
                {
                    _hasHit = true;
                    Vector3 hitPoint = other.ClosestPoint(transform.position);
                    Vector3 hitNormal = -_moveDirection;
                    damageable.TakeDamage(_damage, hitPoint, hitNormal);
                    Destroy(gameObject);
                    return;
                }
            }

            // Destroy projectile if it hits solid environment obstacles (walls, trees, rocks)
            if (!other.isTrigger)
            {
                _hasHit = true;
                Destroy(gameObject);
            }
        }
    }
}
