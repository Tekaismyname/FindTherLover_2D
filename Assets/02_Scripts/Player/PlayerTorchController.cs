using System;
using UnityEngine;

namespace FindTheLover.Player
{
    /// <summary>
    /// Represents the current flame state of the player's survival torch/lantern.
    /// </summary>
    public enum TorchFlameType
    {
        Unlit = 0,       // Torch is extinguished, offers no protection against poison gas
        NormalFlame = 1, // Warm orange flame, protects against poison gas
        SoulFlame = 2    // Mystical cyan flame, protects against poison gas AND burns Thorn Barriers
    }

    /// <summary>
    /// Logic & Presentation Layer: Manages the 40-second burn decay of the player's torch,
    /// flame type transitions (Normal vs Soul Flame), and dynamic light/color feedback.
    /// </summary>
    public class PlayerTorchController : MonoBehaviour
    {
        [Header("Torch Decay Configuration")]
        [Tooltip("Maximum burn duration in seconds when fully lit (GDD: 40 seconds)")]
        [SerializeField] private float maxBurnDuration = 40.0f;

        [Tooltip("Whether the torch starts lit when entering the scene")]
        [SerializeField] private bool startLit = true;

        [Header("Light Visual Feedback")]
        [Tooltip("Maximum intensity of the torch point light when fuel is full")]
        [SerializeField] private float maxLightIntensity = 2.2f;

        [Tooltip("Minimum intensity right before the torch extinguishes")]
        [SerializeField] private float minLightIntensity = 0.4f;

        [Tooltip("Warm orange color for Normal Flame")]
        [SerializeField] private Color normalFlameColor = new Color(1.0f, 0.65f, 0.2f, 1.0f);

        [Tooltip("Mystical cyan color for Soul Flame")]
        [SerializeField] private Color soulFlameColor = new Color(0.15f, 0.95f, 1.0f, 1.0f);

        // Runtime state
        private TorchFlameType _currentFlameType = TorchFlameType.Unlit;
        private float _remainingBurnTime = 0f;
        private float _baseMaxBurnDuration = 40.0f;
        private Light _torchLight;
        private SpriteRenderer _torchSpriteRenderer;

        private void Awake()
        {
            _baseMaxBurnDuration = maxBurnDuration;
        }

        /// <summary>
        /// Scales the torch maximum burn duration based on survival augments (e.g., Eternal Wick x2).
        /// </summary>
        public void SetBurnDurationMultiplier(float multiplier)
        {
            float clamped = Mathf.Clamp(multiplier, 0.5f, 4.0f);
            float ratio = NormalizedFuel;
            maxBurnDuration = _baseMaxBurnDuration * clamped;
            if (IsFlameActive)
            {
                _remainingBurnTime = maxBurnDuration * ratio;
                OnTorchFuelChanged?.Invoke(_remainingBurnTime, maxBurnDuration);
            }
        }

        // Events for UI HUD & Puzzle feedback
        public event Action<float, float> OnTorchFuelChanged;
        public event Action<TorchFlameType> OnFlameTypeChanged;
        public event Action OnTorchExtinguished;

        public TorchFlameType CurrentFlameType => _currentFlameType;
        public float RemainingBurnTime => _remainingBurnTime;
        public float MaxBurnDuration => maxBurnDuration;
        public float NormalizedFuel => maxBurnDuration > 0f ? Mathf.Clamp01(_remainingBurnTime / maxBurnDuration) : 0f;
        public bool IsFlameActive => _currentFlameType != TorchFlameType.Unlit && _remainingBurnTime > 0f;
        public bool HasSoulFlame => _currentFlameType == TorchFlameType.SoulFlame && _remainingBurnTime > 0f;

        private void Start()
        {
            RefreshSocketVisualReferences();

            if (startLit)
            {
                IgniteNormalFlame();
            }
            else
            {
                ExtinguishTorch();
            }
        }

        private void Update()
        {
            // Automatically refill torch when standing inside the main Campfire Sanctuary
            if (Environment.CampfireSanctuary.Instance != null &&
                Environment.CampfireSanctuary.Instance.IsPositionInSanctuary(transform.position))
            {
                if (_currentFlameType == TorchFlameType.Unlit)
                {
                    IgniteNormalFlame();
                }
                else if (_remainingBurnTime < maxBurnDuration)
                {
                    _remainingBurnTime = maxBurnDuration;
                    OnTorchFuelChanged?.Invoke(_remainingBurnTime, maxBurnDuration);
                }
                UpdateVisualFeedback();
                return;
            }

            if (!IsFlameActive) return;

            // Burn fuel down over real time
            _remainingBurnTime = Mathf.Max(0f, _remainingBurnTime - Time.deltaTime);
            OnTorchFuelChanged?.Invoke(_remainingBurnTime, maxBurnDuration);

            UpdateVisualFeedback();

            if (_remainingBurnTime <= 0f)
            {
                ExtinguishTorch();
            }
        }

        /// <summary>
        /// Ignites or refills the torch with a standard warm survival flame (40s).
        /// </summary>
        public void IgniteNormalFlame()
        {
            RefreshSocketVisualReferences();
            _remainingBurnTime = maxBurnDuration;

            if (_currentFlameType != TorchFlameType.SoulFlame)
            {
                _currentFlameType = TorchFlameType.NormalFlame;
                OnFlameTypeChanged?.Invoke(_currentFlameType);
            }

            UpdateVisualFeedback();
            OnTorchFuelChanged?.Invoke(_remainingBurnTime, maxBurnDuration);
            Debug.Log($"[PlayerTorchController] Torch ignited with {_currentFlameType}! Fuel: {_remainingBurnTime}s");
        }

        /// <summary>
        /// Imbues the player's torch with the mystical cyan Soul Flame and refills duration to 40s.
        /// </summary>
        public void IgniteSoulFlame()
        {
            RefreshSocketVisualReferences();
            _remainingBurnTime = maxBurnDuration;
            _currentFlameType = TorchFlameType.SoulFlame;

            UpdateVisualFeedback();
            OnFlameTypeChanged?.Invoke(_currentFlameType);
            OnTorchFuelChanged?.Invoke(_remainingBurnTime, maxBurnDuration);
            Debug.Log("<color=cyan>[PlayerTorchController] SOUL FLAME ACQUIRED! Ready to burn the Thorn Barrier!</color>");
        }

        /// <summary>
        /// Restores exact torch flame type and remaining burn duration from GameData.
        /// </summary>
        public void RestoreTorchState(TorchFlameType flameType, float remainingFuel)
        {
            RefreshSocketVisualReferences();
            _currentFlameType = flameType;
            _remainingBurnTime = Mathf.Clamp(remainingFuel, 0f, maxBurnDuration);
            if (_remainingBurnTime <= 0f)
            {
                _currentFlameType = TorchFlameType.Unlit;
            }

            UpdateVisualFeedback();
            OnFlameTypeChanged?.Invoke(_currentFlameType);
            OnTorchFuelChanged?.Invoke(_remainingBurnTime, maxBurnDuration);
        }

        /// <summary>
        /// Completely extinguishes the torch when fuel runs out or doused.
        /// </summary>
        public void ExtinguishTorch()
        {
            RefreshSocketVisualReferences();
            _remainingBurnTime = 0f;
            _currentFlameType = TorchFlameType.Unlit;

            UpdateVisualFeedback();
            OnFlameTypeChanged?.Invoke(_currentFlameType);
            OnTorchExtinguished?.Invoke();
            Debug.Log("<color=orange>[PlayerTorchController] Torch has burned out! Beware of poison gas!</color>");
        }

        /// <summary>
        /// Locates the Point Light and SpriteRenderer inside the Belt_Socket equipment instance.
        /// </summary>
        private void RefreshSocketVisualReferences()
        {
            if (_torchLight != null && _torchSpriteRenderer != null) return;

            Transform belt = transform.Find("Belt_Socket");
            if (belt != null)
            {
                _torchLight = belt.GetComponentInChildren<Light>(true);
                _torchSpriteRenderer = belt.GetComponentInChildren<SpriteRenderer>(true);
            }
            else
            {
                _torchLight = GetComponentInChildren<Light>(true);
            }
        }

        private void UpdateVisualFeedback()
        {
            RefreshSocketVisualReferences();

            bool lit = IsFlameActive;
            Color targetColor = (_currentFlameType == TorchFlameType.SoulFlame) ? soulFlameColor : normalFlameColor;

            if (_torchLight != null)
            {
                _torchLight.enabled = lit;
                if (lit)
                {
                    _torchLight.color = targetColor;
                    float baseIntensity = Mathf.Lerp(minLightIntensity, maxLightIntensity, NormalizedFuel);
                    float flicker = Mathf.PingPong(Time.time * 3.0f, 0.25f);
                    _torchLight.intensity = baseIntensity + flicker;
                }
            }

            if (_torchSpriteRenderer != null)
            {
                _torchSpriteRenderer.color = lit ? targetColor : new Color(0.35f, 0.35f, 0.35f, 1.0f);
            }
        }
    }
}
