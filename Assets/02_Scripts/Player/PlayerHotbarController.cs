using System;
using UnityEngine;
using UnityEngine.InputSystem;
using FindTheLover.Combat;
using FindTheLover.Equipment.Data;
using FindTheLover.Items;
using FindTherLover.Player;

namespace FindTheLover.Player
{
    /// <summary>
    /// Logic Layer: Manages the 5-slot Muck-like Survival Hotbar (Keys 1-5 & Mouse Scroll):
    /// Slot 0 (Key 1): Sword | Slot 1 (Key 2): Axe | Slot 2 (Key 3): Pickaxe | Slot 3 (Key 4): Torch | Slot 4 (Key 5): Heal Food.
    /// Drives PlayerEquipment Modular Hand Socket and syncs upgrades automatically when crafting higher-tier gear.
    /// </summary>
    public class PlayerHotbarController : MonoBehaviour
    {
        [Header("Hotbar Equipment Slots (Slots 1-4)")]
        [Tooltip("Index 0=Sword, 1=Axe, 2=Pickaxe, 3=Torch.")]
        [SerializeField] private EquipmentItemDataSO[] _equipmentSlots = new EquipmentItemDataSO[4];

        [Header("Consumable Heal Slot (Slot 5)")]
        [Tooltip("Primary edible item consumed when pressing Key 5 (e.g., Red Berries or Apple).")]
        [SerializeField] private ItemDataSO _consumableHealItem;

        [Tooltip("Health restored when consuming one heal item from Slot 5.")]
        [SerializeField] private float _consumableHealAmount = 25f;

        private PlayerEquipment _playerEquipment;
        private InventorySystem _inventory;
        private HealthSystem _healthSystem;
        private PlayerTorchController _torchController;

        private int _activeSlotIndex = 0;

        /// <summary>Fired whenever active selection or slot contents change: (activeSlotIndex).</summary>
        public event Action<int> OnHotbarChanged;

        public int ActiveSlotIndex => _activeSlotIndex;
        public int EquipmentSlotCount => _equipmentSlots != null ? _equipmentSlots.Length : 0;
        public ItemDataSO ConsumableHealItem => _consumableHealItem;

        private void Awake()
        {
            _playerEquipment = GetComponent<PlayerEquipment>();
            _inventory = GetComponent<InventorySystem>();
            _healthSystem = GetComponent<HealthSystem>();
            _torchController = GetComponent<PlayerTorchController>();
        }

        private void Start()
        {
            SelectSlot(0);
        }

        private void Update()
        {
            if (Keyboard.current == null) return;

            if (Keyboard.current.digit1Key.wasPressedThisFrame) SelectSlot(0);
            else if (Keyboard.current.digit2Key.wasPressedThisFrame) SelectSlot(1);
            else if (Keyboard.current.digit3Key.wasPressedThisFrame) SelectSlot(2);
            else if (Keyboard.current.digit4Key.wasPressedThisFrame) SelectSlot(3);
            else if (Keyboard.current.digit5Key.wasPressedThisFrame) UseConsumableSlot();

            // Mouse wheel quick cycling across equipment slots 1-4
            if (Mouse.current != null)
            {
                float scrollY = Mouse.current.scroll.ReadValue().y;
                if (scrollY > 0.1f) SelectSlot((_activeSlotIndex + 3) % 4);
                else if (scrollY < -0.1f) SelectSlot((_activeSlotIndex + 1) % 4);
            }
        }

        /// <summary>
        /// Returns the EquipmentItemDataSO assigned to a hotbar slot (0..3).
        /// </summary>
        public EquipmentItemDataSO GetEquipmentInSlot(int slotIndex)
        {
            if (_equipmentSlots == null || slotIndex < 0 || slotIndex >= _equipmentSlots.Length) return null;
            return _equipmentSlots[slotIndex];
        }

        /// <summary>
        /// Selects hotbar slot 0..3 and equips its weapon/tool into the Modular Hand Socket.
        /// </summary>
        public void SelectSlot(int slotIndex)
        {
            if (_equipmentSlots == null || slotIndex < 0 || slotIndex >= _equipmentSlots.Length) return;

            _activeSlotIndex = slotIndex;
            EquipmentItemDataSO targetEquip = _equipmentSlots[slotIndex];

            if (_playerEquipment != null)
            {
                _playerEquipment.EquipItem(targetEquip);
            }

            // Ignite torch automatically when switching to the Torch slot
            if (targetEquip != null && targetEquip.equipmentType == EquipmentType.Torch && _torchController != null && !_torchController.IsFlameActive)
            {
                _torchController.IgniteNormalFlame();
            }

            OnHotbarChanged?.Invoke(_activeSlotIndex);
        }

        /// <summary>
        /// Upgrades the matching equipment slot (Sword->0, Axe->1, Pickaxe->2, Torch->3) and equips it immediately.
        /// </summary>
        public int AssignAndEquipCraftedEquipment(EquipmentItemDataSO newEquipment)
        {
            if (newEquipment == null) return -1;

            int targetSlot = MapEquipmentTypeToSlot(newEquipment.equipmentType);
            if (_equipmentSlots == null || _equipmentSlots.Length < 4)
            {
                _equipmentSlots = new EquipmentItemDataSO[4];
            }

            _equipmentSlots[targetSlot] = newEquipment;
            SelectSlot(targetSlot);
            return targetSlot;
        }

        /// <summary>
        /// Consumes one healing food item from InventorySystem (Slot 5) to restore Health.
        /// </summary>
        public bool UseConsumableSlot()
        {
            if (_consumableHealItem == null || _inventory == null || _healthSystem == null) return false;
            if (_healthSystem.IsDead || _healthSystem.CurrentHealth >= _healthSystem.MaxHealth) return false;

            if (_inventory.RemoveItem(_consumableHealItem, 1))
            {
                _healthSystem.Heal(_consumableHealAmount);
                FindTheLover.UI.DamageNumberSpawner.Instance?.SpawnDamage(_consumableHealAmount, transform.position + Vector3.up * 1.4f, FindTheLover.UI.Data.DamagePopupStyle.Heal);
                OnHotbarChanged?.Invoke(_activeSlotIndex);
                return true;
            }

            Debug.LogWarning("[PlayerHotbarController] No healing food left in Inventory!");
            return false;
        }

        public int GetConsumableCount()
        {
            if (_inventory == null || _consumableHealItem == null) return 0;
            return _inventory.GetItemCount(_consumableHealItem);
        }

        public static int MapEquipmentTypeToSlot(EquipmentType type)
        {
            switch (type)
            {
                case EquipmentType.Sword:   return 0;
                case EquipmentType.Axe:     return 1;
                case EquipmentType.Pickaxe: return 2;
                case EquipmentType.Torch:   return 3;
                default:                    return 0;
            }
        }
    }
}
