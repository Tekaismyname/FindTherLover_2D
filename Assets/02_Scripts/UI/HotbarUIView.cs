using TMPro;
using UnityEngine;
using UnityEngine.UI;
using FindTheLover.Equipment.Data;
using FindTheLover.Items;
using FindTheLover.Player;

namespace FindTheLover.UI
{
    /// <summary>
    /// Presentation Layer: Bottom-Center 5-Slot Survival Hotbar displaying modular weapons (1=Sword, 2=Axe,
    /// 3=Pickaxe, 4=Torch) and consumable heal food (5=Berries) with active glowing selection borders.
    /// </summary>
    public class HotbarUIView : MonoBehaviour
    {
        [Header("Slot Visual Arrays (Length = 5)")]
        [SerializeField] private Image[] _slotBorderImages;
        [SerializeField] private Image[] _slotIconImages;
        [SerializeField] private TextMeshProUGUI[] _slotLabelTexts;
        [SerializeField] private Button[] _slotButtons;

        [Header("Visual Highlight Colors")]
        [SerializeField] private Color _selectedBorderColor = new Color(1.0f, 0.85f, 0.22f, 1f);
        [SerializeField] private Color _inactiveBorderColor = new Color(0.28f, 0.34f, 0.44f, 0.9f);

        private PlayerHotbarController _hotbar;
        private InventorySystem _inventory;

        private void Start()
        {
            _hotbar = FindAnyObjectByType<PlayerHotbarController>();
            _inventory = _hotbar != null ? _hotbar.GetComponent<InventorySystem>() : FindAnyObjectByType<InventorySystem>();

            if (_slotButtons != null)
            {
                for (int i = 0; i < _slotButtons.Length; i++)
                {
                    int capturedIndex = i;
                    if (_slotButtons[i] != null)
                    {
                        _slotButtons[i].onClick.AddListener(() => OnSlotClicked(capturedIndex));
                    }
                }
            }

            if (_hotbar != null) _hotbar.OnHotbarChanged += HandleHotbarChanged;
            if (_inventory != null) _inventory.OnInventoryChanged += RefreshAllSlots;

            RefreshAllSlots();
        }

        private void OnDestroy()
        {
            if (_hotbar != null) _hotbar.OnHotbarChanged -= HandleHotbarChanged;
            if (_inventory != null) _inventory.OnInventoryChanged -= RefreshAllSlots;
        }

        private void OnSlotClicked(int slotIndex)
        {
            if (_hotbar == null) return;
            if (slotIndex < 4) _hotbar.SelectSlot(slotIndex);
            else _hotbar.UseConsumableSlot();
        }

        private void HandleHotbarChanged(int activeSlot)
        {
            RefreshAllSlots();
        }

        /// <summary>
        /// Refreshes all 5 Hotbar slot icons, material tints, active selection highlight, and food count.
        /// </summary>
        public void RefreshAllSlots()
        {
            if (_hotbar == null) _hotbar = FindAnyObjectByType<PlayerHotbarController>();
            if (_hotbar == null) return;

            for (int i = 0; i < 4; i++)
            {
                EquipmentItemDataSO eq = _hotbar.GetEquipmentInSlot(i);
                bool isSelected = (_hotbar.ActiveSlotIndex == i);

                if (_slotBorderImages != null && i < _slotBorderImages.Length && _slotBorderImages[i] != null)
                {
                    _slotBorderImages[i].color = isSelected ? _selectedBorderColor : _inactiveBorderColor;
                }

                if (_slotIconImages != null && i < _slotIconImages.Length && _slotIconImages[i] != null)
                {
                    if (eq != null && eq.weaponSprite != null)
                    {
                        _slotIconImages[i].enabled = true;
                        _slotIconImages[i].sprite = eq.weaponSprite;
                        _slotIconImages[i].color = eq.tierTintColor;
                    }
                    else
                    {
                        _slotIconImages[i].enabled = false;
                    }
                }

                if (_slotLabelTexts != null && i < _slotLabelTexts.Length && _slotLabelTexts[i] != null)
                {
                    _slotLabelTexts[i].text = eq != null ? $"[{i + 1}] {eq.materialTier}" : $"[{i + 1}]";
                }
            }

            // Slot 5 (index 4): Healing Consumable
            if (_slotBorderImages != null && _slotBorderImages.Length > 4 && _slotBorderImages[4] != null)
            {
                _slotBorderImages[4].color = new Color(0.75f, 0.28f, 0.36f, 0.92f);
            }
            if (_slotIconImages != null && _slotIconImages.Length > 4 && _slotIconImages[4] != null)
            {
                ItemDataSO food = _hotbar.ConsumableHealItem;
                _slotIconImages[4].enabled = food != null && food.icon != null;
                if (food != null) _slotIconImages[4].sprite = food.icon;
                _slotIconImages[4].color = Color.white;
            }
            if (_slotLabelTexts != null && _slotLabelTexts.Length > 4 && _slotLabelTexts[4] != null)
            {
                _slotLabelTexts[4].text = $"[5] x{_hotbar.GetConsumableCount()}";
            }
        }
    }
}
