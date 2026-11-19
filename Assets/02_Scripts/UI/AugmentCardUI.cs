using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using FindTheLover.Augments.Data;

namespace FindTheLover.UI
{
    /// <summary>
    /// Presentation Layer: Individual TFT-style Augment Card widget inside the 3-card selection modal.
    /// Renders tier-colored glowing border (Bronze / Silver / Gold), ribbon label, title, icon, and description.
    /// </summary>
    public class AugmentCardUI : MonoBehaviour
    {
        [Header("Card Visual References")]
        [SerializeField] private Image _borderGlowImage;
        [SerializeField] private Image _tierRibbonImage;
        [SerializeField] private Image _iconImage;
        [SerializeField] private TextMeshProUGUI _tierLabelText;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _descriptionText;
        [SerializeField] private TextMeshProUGUI _hotkeyHintText;
        [SerializeField] private Button _selectButton;

        private AugmentDataSO _boundAugment;
        private Action<AugmentDataSO> _onSelectedCallback;

        private void Awake()
        {
            if (_selectButton != null)
            {
                _selectButton.onClick.AddListener(HandleCardClicked);
            }
        }

        private void OnDestroy()
        {
            if (_selectButton != null)
            {
                _selectButton.onClick.RemoveListener(HandleCardClicked);
            }
        }

        /// <summary>
        /// Binds an AugmentDataSO blueprint to this card and updates all tier visual accents.
        /// </summary>
        public void Bind(AugmentDataSO augment, int slotIndex, Action<AugmentDataSO> onSelected)
        {
            _boundAugment = augment;
            _onSelectedCallback = onSelected;

            if (augment == null)
            {
                gameObject.SetActive(false);
                return;
            }

            gameObject.SetActive(true);
            Color tierColor = augment.GetTierAccentColor();

            if (_borderGlowImage != null) _borderGlowImage.color = tierColor;
            if (_tierRibbonImage != null) _tierRibbonImage.color = new Color(tierColor.r * 0.7f, tierColor.g * 0.7f, tierColor.b * 0.7f, 0.95f);
            if (_tierLabelText != null)
            {
                _tierLabelText.text = augment.GetTierLabel();
                _tierLabelText.color = Color.white;
            }

            if (_titleText != null)
            {
                _titleText.text = augment.displayName;
                _titleText.color = tierColor;
            }

            if (_descriptionText != null)
            {
                _descriptionText.text = augment.description;
            }

            if (_iconImage != null)
            {
                if (augment.icon != null)
                {
                    _iconImage.sprite = augment.icon;
                    _iconImage.color = Color.white;
                }
                else
                {
                    // Fallback tinted emblem badge when custom icon sprite is not yet assigned
                    _iconImage.sprite = null;
                    _iconImage.color = tierColor;
                }
            }

            if (_hotkeyHintText != null)
            {
                _hotkeyHintText.text = $"[{slotIndex + 1}] CLICK OR PRESS {slotIndex + 1}";
            }
        }

        private void HandleCardClicked()
        {
            if (_boundAugment != null)
            {
                _onSelectedCallback?.Invoke(_boundAugment);
            }
        }
    }
}
