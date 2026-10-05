using UnityEngine;
using System.Collections;
using UnityEditor.EditorTools;
using Unity.VisualScripting;

namespace FindTheLover.Items
{
    /// <summary>
    /// Controller for physical loot drops in the 3D world.
    /// Handles physical ejection bounce, 2.5D billboard alignment, and magnetic player attraction.
    /// </summary>

    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(SphereCollider))]
    public class LootItem : MonoBehaviour
    {
        [Header("Item Blueprint")]
        [Tooltip("Reference to the Item data scriptableObject")]
        [SerializeField] private ItemDataSO itemData;

        [Tooltip("Stack  quantity  contained withing this dropped item.")]
        [SerializeField] private int quantity = 1;

        [Header("Magnetic Vacuum Settings")]
        [Tooltip("Distance at which the item starts flying towards the player")]
        [SerializeField] private float magnegRadius = 4.5f;

        [Tooltip("Initial flight speed towards the player.")]
        [SerializeField] private float flySpeed = 8.0f;

        [Tooltip("Acceleration factor while flying towards the player")]
        [SerializeField] private float acceleration = 22.0f;

        [Tooltip("Cooldown before magnetism activated (ensure initial burst bounce plays out)")]
        [SerializeField] private float pickupDelay = 0.45f;

        [Header("Audio")]
        [Tooltip("Sound played when absorbed by player")]
        [SerializeField] private AudioClip pickupSound;

        private Rigidbody _rb;
        private Transform _playerTransform;
        private bool _canBePickedUp = false;
        private bool _isFlyingToPlayer = false;
        private Collider _collider;
        private Camera _mainCamera;

        public ItemDataSO ItemData => itemData;
        public int Quantity => quantity;

        public void Initialize(ItemDataSO data, int count = 1)
        {
            itemData = data;
            quantity = count;
        }

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _collider = GetComponent<Collider>();
            _mainCamera = Camera.main;

            // Chống rơi xuyên địa hình (Continuous Collision Detection)
            if (_rb != null)
            {
                _rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            }
        }

        private void Start()
        {
            StartCoroutine(EnablePickupAfterDelayRoutine());
        }

        private void LateUpdate()
        {
            // 2,5D billboard:  Ensure item sprite always faces camera orientation
            if (_mainCamera != null)
            {
                transform.rotation = _mainCamera.transform.rotation;
            }
        }

        private void Update()
        {
            if (!_canBePickedUp) return;

            if (!_isFlyingToPlayer)
            {
                DetectNearbyPlayer();
            }
            else
            {
                FlyTowardsPlayer();
            }
        }

        private IEnumerator EnablePickupAfterDelayRoutine()
        {
            yield return new WaitForSeconds(pickupDelay);
            _canBePickedUp = true;
        }

        private void DetectNearbyPlayer()
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, magnegRadius);
            foreach (Collider hit in hits)
            {
                if (hit.CompareTag("Player") || hit.transform.root.CompareTag("Player"))
                {
                    _playerTransform = hit.transform; 
                    _isFlyingToPlayer = true;
                    _rb.isKinematic = true;
                    
                    if (_collider != null)
                    {
                        _collider.isTrigger = true;
                    }
                    
                    break;
                }
            }
        }

        public void FlyTowardsPlayer()
        {
            if (_playerTransform == null) return;

            //Target chest height of player
            Vector3 targetPos = _playerTransform.position + Vector3.up * 1.0f;

            //Accelerate smoothly
            flySpeed += acceleration * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, targetPos, flySpeed * Time.deltaTime);

            // Collect once touched
            if (Vector3.Distance(transform.position, targetPos) < 0.4f)
            {
                Collect();
            }
        }

        private void Collect()
        {
            if (pickupSound != null)
            {
                AudioSource.PlayClipAtPoint(pickupSound, transform.position);
            }


            if(_playerTransform != null)
            {
                var inventory = _playerTransform.GetComponentInChildren<InventorySystem>();
                if (inventory == null)
                {
                    inventory = _playerTransform.GetComponentInParent<InventorySystem>();
                }

                if (inventory != null && itemData != null)
                {
                    inventory.AddItem(itemData, quantity);
                }
            }
            Debug.Log($"<color=green>[LootItem]</color> Collected {quantity}x {itemData?.itemName ?? "Item"}!");

            //Day 8  will  ad this item directly to InventorySytem
            Destroy(gameObject);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(transform.position, magnegRadius);
        }
    }
}