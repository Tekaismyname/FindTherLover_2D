using FindTheLover.Equipment.Data;
using FindTheLover.PlayerLocomotion;
using UnityEngine;

namespace FindTherLover.Player
{
    /// <summary>
    /// Logic Layer: Manages attaching/detaching holdable equipment into Player's Hand_Socket,
    /// dynamic SortingOrder layering (in front of / behind body), and horizontal direction flipping.
    /// </summary>

    public class PlayerEquipment : MonoBehaviour
    {
        [Header("Socket Configuration")]
        [Tooltip("Anchor transform representing Player's Position")]
        [SerializeField] private Transform beltSocket;

        [Header("Starting Equipment ")]
        [Tooltip("Item automatically equipped upon scene initialization")]
        [SerializeField] private EquipmentItemDataSO initialEquipment;

        // Component reference
        private SpriteRenderer _playerSpriteRenderer;
        private PlayerLocomotion _playerLocomotion;
        private SpriteRenderer _itemSpriteRenderer;

        // Runtime state
        private EquipmentItemDataSO currentData;
        private GameObject currentInstance;
        private Vector3 initialSocketScale;
        private Vector3 initialSocketPos;

        /// <summary>Event broadcast whenever the player equips or unequips gear (consumed by GameDataManager).</summary>
        public event System.Action<EquipmentItemDataSO> OnEquipmentChanged;

        public EquipmentItemDataSO CurrentEquippedData => currentData;
        public EquipmentType CurrentEquipmentType => currentData != null ? currentData.equipmentType : EquipmentType.None;

        private void Awake()
        {
            _playerSpriteRenderer = GetComponent<SpriteRenderer>();
            _playerLocomotion = GetComponent<PlayerLocomotion>();

            if (beltSocket != null)
            {
                initialSocketScale = beltSocket.localScale;
                initialSocketPos = beltSocket.localPosition;
            }
            else
            {
                Debug.LogWarning("[PlayerEquipment] handSocket is missing! Spawning fallback socket.");
                GameObject fallback = new GameObject("Hand_Socket");
                fallback.transform.SetParent(transform);
                fallback.transform.localPosition = new Vector3(0.25f, 0.8f, 0f);
                beltSocket = fallback.transform;
                initialSocketScale = beltSocket.localScale;
            }
        }

        private void Start()
        {
            if (initialEquipment != null)
            {
                EquipItem(initialEquipment);
            }

        }

        private void LateUpdate()
        {
            UpdateSortingAndFlipping();
        }

        /// <summary>
        /// Equips a new item blueprint into the hand socket, replacing any currently held item.
        /// </summary>
        public void EquipItem(EquipmentItemDataSO newItem)
        {
            if (newItem == null)
            {
                UnequipCurrentItem();
                return;
            }

            UnequipCurrentItem();
            currentData = newItem;

            if (newItem.heldPrefab != null && beltSocket != null)
            {
                currentInstance = Instantiate(newItem.heldPrefab, beltSocket);
                currentInstance.transform.localPosition = newItem.localOffset;
                currentInstance.transform.localRotation = Quaternion.Euler(newItem.localRotation);
                _itemSpriteRenderer = currentInstance.GetComponentInChildren<SpriteRenderer>();
            }

            Debug.Log($"[PlayerEquipment] Equipped: {newItem.displayName} ({newItem.equipmentType})");
            OnEquipmentChanged?.Invoke(currentData);
        }

        /// <summary>
        /// Removes and destroys currently equipped item in hand socket.
        /// </summary>
        public void UnequipCurrentItem()
        {
            bool hadItem = currentData != null || currentInstance != null;
            if (currentInstance != null)
            {
                Destroy(currentInstance);
                currentInstance = null;
                _itemSpriteRenderer = null;
            }

            currentData = null;
            if (hadItem) OnEquipmentChanged?.Invoke(null);
        }
        
    
        /// <summary>
        /// Dynamically positions the Belt_Socket at the character's hip,
        /// mirrors horizontally based on facing direction, and adjusts SortingOrder.
        /// </summary>
        public void UpdateSortingAndFlipping()
        {
            // Exit early if essential references or socket are missing
            if (_playerLocomotion == null || _playerSpriteRenderer == null 
                || _itemSpriteRenderer == null || beltSocket == null) return;

            // 1. Dynamic Sorting Order: draw item behind body when moving North (Up), in front otherwise
            Vector2 facing = _playerLocomotion.LastFacingDirection;
            if (facing.y > 0.1f)
            {
                // Moving Up (North): item renders behind character sprite
                _itemSpriteRenderer.sortingOrder = _playerSpriteRenderer.sortingOrder - 1;
            }
            else
            {
                // Moving Down/Left/Right: item renders in front of character sprite
                _itemSpriteRenderer.sortingOrder = _playerSpriteRenderer.sortingOrder + 1;
            }

            // 2. Horizontal Hip Alignment based on player sprite flipping
            bool isFacingLeft = _playerSpriteRenderer.flipX;
            
            // Base hip offset coordinates (relative to Quene's center)
            float hipOffsetX = 0.2f;
            float hipOffsetY = 0.45f;

            if (!isFacingLeft)
            {
                // ---> [FACING RIGHT]: Position at right hip with gentle forward tilt
                beltSocket.localPosition = new Vector3(hipOffsetX, hipOffsetY, 0f);
                beltSocket.localRotation = Quaternion.Euler(0f, 0f, -15f);
                beltSocket.localScale = new Vector3(Mathf.Abs(initialSocketScale.x), initialSocketScale.y, initialSocketScale.z);
            }
            else
            {
                // <--- [FACING LEFT]: Position at left hip with mirrored forward tilt
                beltSocket.localPosition = new Vector3(-hipOffsetX, hipOffsetY, 0f);
                beltSocket.localRotation = Quaternion.Euler(0f, 0f, 15f);
                beltSocket.localScale = new Vector3(-Mathf.Abs(initialSocketScale.x), initialSocketScale.y, initialSocketScale.z);
            }
        }
    }
}