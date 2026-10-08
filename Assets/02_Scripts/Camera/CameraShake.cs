using UnityEngine;

namespace FindTheLover.CameraSystem
{
    /// <summary>
    /// Presentation Layer component that applies dynamic, trauma-decay screen shake.
    /// Can be triggered globally via CameraShake.Instance.Shake().
    /// </summary>
    [DefaultExecutionOrder(100)] // Ensures shake is applied after camera follow scripts update
    public class CameraShake : MonoBehaviour
    {
        public static CameraShake Instance { get; private set; }

        [Header("Settings")]
        [Tooltip("Multiplies overall shake strength globally")]
        [SerializeField] private float globalShakeMultiplier = 1.0f;

        [Tooltip("If true, automatically offsets this Transform's position in LateUpdate")]
        [SerializeField] private bool applyDirectlyToTransform = true;

        // Current shake state
        private float _currentIntensity = 0f;
        private float _totalDuration = 0f;
        private float _elapsedTime = 0f;
        private Vector3 _currentShakeOffset = Vector3.zero;

        /// <summary>
        /// Public accessor if other scripts (e.g. CameraFollow3D) wish to read the raw offset.
        /// </summary>
        public Vector3 CurrentOffset => _currentShakeOffset;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        /// <summary>
        /// Triggers a screen shake with quadratic falloff.
        /// If a stronger shake is already active, it keeps the strongest intensity.
        /// </summary>
        /// <param name="intensity">Displacement radius in world units (e.g. 0.1f - 0.5f).</param>
        /// <param name="duration">Duration of the shake in seconds (e.g. 0.1f - 0.25f).</param>
        public void Shake(float intensity, float duration)
        {
            if (intensity <= 0f || duration <= 0f) return;

            // Retain the higher intensity if already shaking
            _currentIntensity = Mathf.Max(_currentIntensity, intensity * globalShakeMultiplier);
            _totalDuration = Mathf.Max(_totalDuration - _elapsedTime, duration);
            _elapsedTime = 0f;
        }

        private void LateUpdate()
        {
            if (_totalDuration <= 0f || _elapsedTime >= _totalDuration)
            {
                _currentShakeOffset = Vector3.zero;
                _currentIntensity = 0f;
                _totalDuration = 0f;
                return;
            }

            _elapsedTime += Time.unscaledDeltaTime;

            // Calculate normalized remaining strength (1 -> 0) with quadratic decay
            float remainingRatio = 1f - Mathf.Clamp01(_elapsedTime / _totalDuration);
            float currentDecay = remainingRatio * remainingRatio; // Quadratic curve

            // Generate random shake displacement on X and Y axes
            Vector2 randomPoint = Random.insideUnitCircle * (_currentIntensity * currentDecay);
            _currentShakeOffset = new Vector3(randomPoint.x, randomPoint.y, 0f);

            // Directly offset the camera transform after camera follow calculation
            if (applyDirectlyToTransform)
            {
                transform.position += _currentShakeOffset;
            }
        }
    }
}