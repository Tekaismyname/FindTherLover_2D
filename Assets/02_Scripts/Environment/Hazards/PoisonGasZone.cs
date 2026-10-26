using UnityEngine;
using FindTheLover.Combat;
using FindTheLover.Player;

namespace FindTheLover.Environment.Hazards
{
    /// <summary>
    /// Logic Layer: Environmental poison gas hazard zone (Zone 2).
    /// Deals periodic poison damage (5 HP/s) to the player if they are inside the gas
    /// without an active torch flame or nearby safe sanctuary light.
    /// </summary>

    [RequireComponent(typeof(BoxCollider))]
    public class PoisonGasZone : MonoBehaviour
    {
        [Header("Poison Hazard Settings")]
        [Tooltip("Damage dealt per tick when unprotected inside poison gas (GGD: 5hp/s)")]
        [SerializeField] private float damagePerTick = 5.0f;

        [Tooltip("Time interval in second between poison damage ticks")]
        [SerializeField] private float tickInterval = 1.0f;

        // Runtime Tracking
        private BoxCollider _zoneCollider;
        private HealthSystem _playerHealth;
        private PlayerTorchController _playerTorch;
        private Transform _playerTransform;
        private float _tickTimer = 0f;

        void Awake()
        {
            _zoneCollider = GetComponent<BoxCollider>();

            if (_zoneCollider != null)
            {
                _zoneCollider.isTrigger = true;
            }
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.isTrigger) return;

            var locomotion = other.GetComponentInParent<PlayerLocomotion.PlayerLocomotion>();
            if (other.CompareTag("Player") || locomotion != null)
            {
                Transform targetRoot = locomotion != null ? locomotion.transform : other.transform;
                _playerTransform = targetRoot;
                _playerHealth = targetRoot.GetComponentInChildren<HealthSystem>() ?? targetRoot.GetComponentInParent<HealthSystem>();
                _playerTorch = targetRoot.GetComponentInChildren<PlayerTorchController>() ?? targetRoot.GetComponentInParent<PlayerTorchController>();
                _tickTimer = 0f;

                Debug.Log("<color=green>[PoisonGasZone] Player entered Toxic Gas Zone!</color>");
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (other.isTrigger) return;

            var locomotion = other.GetComponentInParent<PlayerLocomotion.PlayerLocomotion>();
            if (other.CompareTag("Player") || locomotion != null)
            {
                _playerTransform = null;
                _playerHealth = null;
                _playerTorch = null;
                _tickTimer = 0f;
                Debug.Log("<color=green>[PoisonGasZone] Player exited Toxic Gas Zone safely.</color>");
            }
        }


        private void Update()
        {
            if (_playerHealth == null || _playerHealth.IsDead || _playerTransform == null)
                return;
            // check if player is protected by an active torch Or a safe sanctuary light
            if (IsPlayerProtectedFromGas(_playerTransform.position))
            {
                // Reset poison build-up while protected by fire
                _tickTimer = 0f;
                return;
            }

            // player is unprotected in the dark gas -> accumulate poison tick timer 
            _tickTimer += Time.deltaTime;
            if (_tickTimer >= tickInterval)
            {
                _tickTimer -= tickInterval;
                _playerHealth.TakeDamage(damagePerTick, _playerTransform.position, Vector3.up);
                Debug.Log($"<color=lime>[PoisonGasZone] Toxic gas dealt {damagePerTick} damage! Light your torch quickly!</color>");
            }
        }

        /// <summary>
        /// Evaluates whether the player is currently shielded from poison gas by firelight.
        /// </summary>
        public bool IsPlayerProtectedFromGas(Vector3 playerPosition)
        {
            //1. protected if holding a burn torch (Normal flame or soul flame)
            if (_playerTorch != null && _playerTorch.IsFlameActive)
            {
                return true;
            }

            //2. protected if standing inside the main Campfire Sanctuary
            if (CampfireSanctuary.Instance != null && CampfireSanctuary.Instance.IsPositionInSanctuary(playerPosition))
            {
                return false;
            }

            // 3. Protected if standing inside the safe radius of any lit Ancient Brazier
            if (Puzzles.AncientBrazier.IsPositionShelteredByAnyBrazier(playerPosition))
            {
                return true;
            }
            return false;
        }

        private void OnDrawGizmos()
        {
            BoxCollider col = GetComponent<BoxCollider>();
            if (col == null) return;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = new Color(0.2f, 0.9f, 0.2f, 0.18f);
            Gizmos.DrawCube(col.center, col.size);
            Gizmos.color = new Color(0.2f, 1.0f, 0.2f, 0.7f);
            Gizmos.DrawWireCube(col.center, col.size);
        }
    }
}