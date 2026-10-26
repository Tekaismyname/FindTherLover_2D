using UnityEngine;
using FindTheLover.Combat;

namespace FindTheLover.Environment
{
    /// <summary>
    /// Logic Layer: Safe sanctuary zone providing continuous health regeneration to players
    /// and establishing a defensive perimeter against nighttime monsters.
    /// </summary>
    [RequireComponent(typeof(SphereCollider))]
    public class CampfireSanctuary : MonoBehaviour
    {
        public static CampfireSanctuary Instance { get; private set; }
        // =========================================================================================
        // KHỐI 1: CẤU HÌNH BÁN KÍNH AN TOÀN & HỒI MÁU
        // =========================================================================================
        [Header("Sanctuary Configuration")]
        [Tooltip("Safe perimeter radius in meters.")]
        [SerializeField] private float sanctuaryRadius = 5.0f;

        [Tooltip("Health points regenerated per second while the player rests inside.")]
        [SerializeField] private float healPerSecond = 4.0f;

        [Header("Atmosphere")]
        [Tooltip("Point Light simulating the flickering warm flame.")]
        [SerializeField] private Light fireLight;

        private HealthSystem _playerHealth;
        private SphereCollider _triggerCollider;

        public float SanctuaryRadius => sanctuaryRadius;

        // =========================================================================================
        // KHỐI 2: KHỞI TẠO TRIGGER COLLIDER
        // =========================================================================================
        private void Awake()
        {
             if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;


            _triggerCollider = GetComponent<SphereCollider>();
            _triggerCollider.isTrigger = true;
            _triggerCollider.radius = sanctuaryRadius;
        }

        // =========================================================================================
        // KHỐI 3: QUÉT VÀO / RA VÙNG LỬA TRẠI (TRIGGER EVENTS)
        // =========================================================================================
        private void OnTriggerEnter(Collider other)
        {
            // Detect Quene either on root or nested character hierarchies
            if (other.CompareTag("Player") || other.GetComponentInParent<PlayerLocomotion.PlayerLocomotion>() != null)
            {
                _playerHealth = other.GetComponentInChildren<HealthSystem>() ?? other.GetComponentInParent<HealthSystem>();
                Debug.Log("[CampfireSanctuary] Player entered sanctuary! Healing activated.");
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player") || other.GetComponentInParent<PlayerLocomotion.PlayerLocomotion>() != null)
            {
                _playerHealth = null;
                Debug.Log("[CampfireSanctuary] Player left sanctuary. Healing paused.");
            }
        }

        // =========================================================================================
        // KHỐI 4: VÒNG LẶP HỒI MÁU & ÁNH SÁNG BẬP BÙNG
        // =========================================================================================
        private void Update()
        {
            // 1. Continuous health regeneration until max health reached
            if (_playerHealth != null && _playerHealth.CurrentHealth < _playerHealth.MaxHealth)
            {
                _playerHealth.Heal(healPerSecond * Time.deltaTime);
            }

            // 2. Realistic flame flickering effect
            if (fireLight != null)
            {
                fireLight.intensity = Mathf.PingPong(Time.time * 2.5f, 0.6f) + 2.5f;
            }
        }

        // =========================================================================================
        // KHỐI 5: VẼ GIZMOS TRỰC QUAN TRÊN SCENE
        // =========================================================================================
        
           /// <summary>
        /// Checks if a given world position is sheltered within the safe sanctuary perimeter.
        /// </summary>
        public bool IsPositionInSanctuary(Vector3 targetPosition)
        {
            return Vector3.Distance(transform.position, targetPosition) <= sanctuaryRadius;
        }

        private void OnDrawGizmos()
        {
            // Green wireframe sphere visualizing the safe zone border
            Gizmos.color = new Color(0.2f, 1f, 0.4f, 0.35f);
            Gizmos.DrawWireSphere(transform.position, sanctuaryRadius);
        }
    }
}