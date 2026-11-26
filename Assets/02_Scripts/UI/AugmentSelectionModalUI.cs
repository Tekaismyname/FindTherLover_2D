using UnityEngine;
using UnityEngine.InputSystem;
using FindTheLover.Augments;
using FindTheLover.Augments.Data;

namespace FindTheLover.UI
{
    /// <summary>
    /// Presentation Layer: Modal controller managing the 3-card TFT Augment selection screen.
    /// Supports both mouse clicks and quick keyboard hotkeys (1, 2, 3) with smooth unscaled pop-in animation.
    /// </summary>
    public class AugmentSelectionModalUI : MonoBehaviour
    {
        [Header("Modal References")]
        [Tooltip("Root panel toggled visible when an augment selection offer is active.")]
        [SerializeField] private GameObject _modalRootPanel;

        [Tooltip("Container transform animated smoothly when the modal opens.")]
        [SerializeField] private RectTransform _cardsContainer;

        [Tooltip("Exactly 3 AugmentCardUI slots for Pick-1-of-3 TFT selection.")]
        [SerializeField] private AugmentCardUI[] _cardSlots;

        [Header("Co-op & Cursor Behavior")]
        [Tooltip("Optional singleplayer time slowdown while picking cards (keep 1.0 in online co-op).")]
        [Range(0f, 1f)] [SerializeField] private float _selectionTimeScale = 0.25f;

        [SerializeField] private float _popInSpeed = 9f;

        private bool _isOpen;
        private float _popProgress;
        private CursorLockMode _previousCursorLockMode;
        private bool _previousCursorVisible;

        public bool IsOpen => _isOpen;

        private void Start()
        {
            if (_modalRootPanel != null)
            {
                _modalRootPanel.SetActive(false);
            }

            if (AugmentManager.Instance != null)
            {
                AugmentManager.Instance.OnAugmentChoiceOffered += HandleChoicesOffered;
                AugmentManager.Instance.OnAugmentSelected += HandleAugmentSelected;
            }
        }

        private void OnDestroy()
        {
            if (AugmentManager.Instance != null)
            {
                AugmentManager.Instance.OnAugmentChoiceOffered -= HandleChoicesOffered;
                AugmentManager.Instance.OnAugmentSelected -= HandleAugmentSelected;
            }

            if (_isOpen)
            {
                Time.timeScale = 1f;
            }
        }

        private void Update()
        {
            if (!_isOpen) return;

            // Smooth unscaled pop-in animation so UI stays snappy even when timeScale is slowed
            if (_cardsContainer != null && _popProgress < 1f)
            {
                _popProgress = Mathf.Min(1f, _popProgress + Time.unscaledDeltaTime * _popInSpeed);
                float easedScale = Mathf.SmoothStep(0.82f, 1f, _popProgress);
                _cardsContainer.localScale = Vector3.one * easedScale;
            }

            // Quick keyboard selection hotkeys: [1], [2], [3]
            if (Keyboard.current != null && AugmentManager.Instance != null)
            {
                if (Keyboard.current.digit1Key.wasPressedThisFrame) AugmentManager.Instance.SelectOfferedChoiceByIndex(0);
                else if (Keyboard.current.digit2Key.wasPressedThisFrame) AugmentManager.Instance.SelectOfferedChoiceByIndex(1);
                else if (Keyboard.current.digit3Key.wasPressedThisFrame) AugmentManager.Instance.SelectOfferedChoiceByIndex(2);
            }
        }

        private void HandleChoicesOffered(AugmentDataSO[] choices)
        {
            if (_modalRootPanel == null || _cardSlots == null) return;

            _isOpen = true;
            _popProgress = 0f;
            _modalRootPanel.SetActive(true);

            if (_cardsContainer != null)
            {
                _cardsContainer.localScale = Vector3.one * 0.82f;
            }

            for (int i = 0; i < _cardSlots.Length; i++)
            {
                if (_cardSlots[i] == null) continue;
                AugmentDataSO data = (choices != null && i < choices.Length) ? choices[i] : null;
                int ownedLevel = (data != null && AugmentManager.Instance != null)
                    ? AugmentManager.Instance.GetStackCount(data.augmentId)
                    : 0;
                _cardSlots[i].Bind(data, i, OnCardClicked, ownedLevel);
            }

            _previousCursorLockMode = Cursor.lockState;
            _previousCursorVisible = Cursor.visible;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (_selectionTimeScale < 1f)
            {
                Time.timeScale = _selectionTimeScale;
            }
        }

        private void OnCardClicked(AugmentDataSO chosen)
        {
            if (AugmentManager.Instance != null)
            {
                AugmentManager.Instance.SelectAugment(chosen);
            }
        }

        private void HandleAugmentSelected(AugmentDataSO chosen, int newStack)
        {
            CloseModal();
        }

        private void CloseModal()
        {
            _isOpen = false;
            if (_modalRootPanel != null)
            {
                _modalRootPanel.SetActive(false);
            }

            Time.timeScale = 1f;
            Cursor.lockState = _previousCursorLockMode;
            Cursor.visible = _previousCursorVisible;
        }
    }
}
