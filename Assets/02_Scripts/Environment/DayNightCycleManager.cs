using System;
using FindTheLover.Environment.Data;
using UnityEngine;

namespace FindTheLover.Environment
{
    /// <summary>
    /// Logic Controller driving real-time 360-degree sun rotation and atmospheric transitions.
    /// </summary>
    public class DayNightCycleManager : MonoBehaviour
    {
        public static DayNightCycleManager Instance { get; private set; }

        [Header("Data Configuration")]
        [SerializeField] private DayNightDataSO _config;

        [Header("Scene References")]
        [SerializeField] private Light _directionalSunLight;

         [Header("Sun Orientation")]
        [Tooltip("Fixed Y-axis yaw angle to cast pleasant diagonal shadows across the 2.5D island.")]
        [SerializeField] private float _sunYawAngle = 170f;

        [Header("Runtime state (read only)")]
        [SerializeField] private int _currentDay = 1;
        [Range(0f, 1f)]
        [SerializeField] private float _normalizedTime = 0.25f;
        [SerializeField] private bool _isNight = false;

        // Notification transfer day night even
        public event Action<int> OnDayStarted;
        public event Action<int> OnNightStarted;

        public int CurrentDay => _currentDay;
        public float NormalizedTime => _normalizedTime;
        public bool IsNight => _isNight;
        public float DayDurationInSeconds => _config != null ? _config.dayDurationInSeconds : 120f;
        public float DayStartNormalized => _config != null ? _config.dayStartNormalized : 0.2f;
        public float NightStartNormalized => _config != null ? _config.nightStartNormalized : 0.7f;

        /// <summary>
        /// Calculates the remaining real-time seconds until the next phase transition (Dusk or Dawn).
        /// </summary>
        public float GetSecondsUntilNextPhase()
        {
            if (_config == null || _config.dayDurationInSeconds <= 0f) return 0f;

            float deltaNormalized;
            if (_isNight)
            {
                // Time remaining until daytime begins (wrapping around midnight 1.0 -> 0.0)
                deltaNormalized = _normalizedTime >= _config.nightStartNormalized
                    ? (1f - _normalizedTime) + _config.dayStartNormalized
                    : _config.dayStartNormalized - _normalizedTime;
            }
            else
            {
                // Time remaining until nighttime begins
                deltaNormalized = Mathf.Max(0f, _config.nightStartNormalized - _normalizedTime);
            }

            return Mathf.Max(0f, deltaNormalized * _config.dayDurationInSeconds);
        }

        private void Awake()
        {
            if (Instance != null && Instance != deathInstanceCheck(this))
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private bool deathInstanceCheck(DayNightCycleManager target) => Instance != target;

        private void Start()
        {
            // Enable Unity global redering
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;

            UpdateAtmosphere();
        }

        private void Update()
        {
            if (_config == null || _config.dayDurationInSeconds <= 0f) return;

            // Advance normalized time propotionally to delta time
            float timeStep = Time.deltaTime / _config.dayDurationInSeconds;
            _normalizedTime += timeStep;

            // wrap around after a full 24-hour cycle and increment day counter
            if (_normalizedTime >= 1f)
            {
                _normalizedTime -= 1f;
                _currentDay++;
                Debug.Log($"<color=yellow>[DayNightCycle]</color> A new day has dawned! Day {_currentDay} begins.");
            }

            CheckDayNightTransition();
            UpdateAtmosphere();
        }

        /// <summary>
        /// Calculates directional sun pitch rotation (360 degrees) and samples color/fog gradients.
        /// </summary>
        private void UpdateAtmosphere()
        {
            if (_config == null) return;

            //Rotate sun pitch: 0 = sunrise, 90 = noon, 180 = sunset, 270 = midnight

            float sunPitchAngle = _normalizedTime * 360f;

            if (_directionalSunLight != null)
            {
                _directionalSunLight.transform.rotation = Quaternion.Euler(sunPitchAngle, _sunYawAngle, 0f);
                _directionalSunLight.color = _config.sunColorGradient.Evaluate(_normalizedTime);
                _directionalSunLight.intensity = _config.sunIntensityCurve.Evaluate(_normalizedTime);
            }

            // Interpolate global corlor and density smoothly
            RenderSettings.fogColor = _config.fogColorGradient.Evaluate(_normalizedTime);
            RenderSettings.fogDensity = _config.fogDensityCurve.Evaluate(_normalizedTime);
        }
        
        /// <summary>
        /// Detects daytime/nighttime thresholds and dispatches broadcast events.
        /// </summary>
        private void CheckDayNightTransition()
        {
            bool wasNight = _isNight;
            _isNight = _normalizedTime < _config.dayStartNormalized || _normalizedTime >= _config.nightStartNormalized;

            //Dispatch notification event when state toggle
            if(wasNight != _isNight)
            {
                if (_isNight)
                {
                    Debug.Log($"<color=cyan>[DayNightCycle]</color> Night has fallen! Night {_currentDay} begins.");
                    OnNightStarted?.Invoke(_currentDay);
                }
                else
                {
                    Debug.Log($"<color=green>[DayNightCycle]</color> The sun rises! Day {_currentDay} begins.");
                    OnDayStarted?.Invoke(_currentDay);
                }
            }
        }
    }
}