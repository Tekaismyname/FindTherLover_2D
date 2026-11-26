using UnityEngine;


namespace FindTheLover.CameraSystem
{
    /// <summary>
    /// Orbits the camera horizontally (Yaw) around the player target.
    /// Vertical pitch is locked by default for crisp 2.5D presentation without unwanted up/down tilt.
    /// Driven purely by ILookInputProvider and CameraSettingSO.
    /// </summary>
    public class CameraLookController : MonoBehaviour
    {
        [Header("Data & Transforms")]
        [SerializeField] private CameraSettingSO settings;
        [SerializeField] private Transform pitchTransform;
        [SerializeField] private Transform yawTransform;

        [Header("Horizontal-Only Lock (2.5D Mode)")]
        [Tooltip("When enabled, ignores vertical mouse movement (Y) so the camera only rotates horizontally around the character.")]
        [SerializeField] private bool lockVerticalPitch = true;
        [Tooltip("Fixed downward pitch angle (degrees) maintained while vertical pitch is locked.")]
        [SerializeField] private float fixedPitchAngle = 15f;

        [Header("Orbit Pivot Settings")]
        [Tooltip("Target player to orbit around")]
        [SerializeField] private Transform target;
        [Tooltip("Pivot offset at character's chest height")]
        [SerializeField] private Vector3 targetOffset = new Vector3(0f, 1.2f, 0f);
        [Tooltip("Distance from camera to player pivot")]
        [SerializeField] private float distance = 10.5f;
        private ILookInputProvider _lookInputProvider;
        private float _yaw;
        private float _pitch;
        private float _targetPitch;
        private float _targetYaw;
        private float _yawVelocity;
        private float _pitchVelocity;

        public bool LockVerticalPitch => lockVerticalPitch;
        public float CurrentYaw => _yaw;
        public float CurrentPitch => _pitch;

        /// <summary>
        /// Pure deterministic helper that computes the next target (yaw, pitch) given look delta.
        /// Keeps pitch strictly locked when lockVertical is true.
        /// </summary>
        public static Vector2 ComputeTargetAngles(
            float currentTargetYaw,
            float currentTargetPitch,
            Vector2 lookDelta,
            float sensitivityX,
            float sensitivityY,
            bool invertY,
            float minPitch,
            float maxPitch,
            bool lockVertical,
            float lockedPitch)
        {
            float nextYaw = currentTargetYaw + lookDelta.x * sensitivityX;
            if (lockVertical)
            {
                return new Vector2(nextYaw, lockedPitch);
            }

            float pitchDelta = (invertY ? 1f : -1f) * lookDelta.y * sensitivityY;
            float nextPitch = Mathf.Clamp(currentTargetPitch + pitchDelta, minPitch, maxPitch);
            return new Vector2(nextYaw, nextPitch);
        }

        private void Awake()
        {
            if (target == null)
            {
                var player = GameObject.Find("Quene_Player");
                if (player != null) target = player.transform;
            }
            _lookInputProvider = GetComponent<ILookInputProvider>();
            if (pitchTransform == null) pitchTransform = transform;

            _yaw = yawTransform != null ? yawTransform.eulerAngles.y : pitchTransform.eulerAngles.y;

            if (lockVerticalPitch)
            {
                _pitch = fixedPitchAngle;
                _targetPitch = fixedPitchAngle;
            }
            else
            {
                _pitch = pitchTransform.localEulerAngles.x;
                if (_pitch > 180f) _pitch -= 360f; // Normalize 0..360 to -180..180
                _targetPitch = _pitch;
            }
            _targetYaw = _yaw;
        }

        private void LateUpdate()
        {
            if (settings == null || _lookInputProvider == null) return;

            Vector2 delta = _lookInputProvider.GetLookDelta();

            // Compute target angles (Vertical Y delta is completely ignored when lockVerticalPitch is enabled)
            Vector2 nextTargets = ComputeTargetAngles(
                _targetYaw,
                _targetPitch,
                delta,
                settings.sensitivityX,
                settings.sensitivityY,
                settings.inverY,
                settings.minPitch,
                settings.maxPitch,
                lockVerticalPitch,
                fixedPitchAngle);

            _targetYaw = nextTargets.x;
            _targetPitch = nextTargets.y;

            _yaw = Mathf.SmoothDampAngle(_yaw, _targetYaw, ref _yawVelocity, settings.smoothTime);
            _pitch = lockVerticalPitch
                ? fixedPitchAngle
                : Mathf.Clamp(Mathf.SmoothDamp(_pitch, _targetPitch, ref _pitchVelocity, settings.smoothTime), settings.minPitch, settings.maxPitch);

            // Apply horizontal-only orbit rotation around target
            Quaternion rotation = Quaternion.Euler(_pitch, _yaw, 0f);
            if (target != null)
            {
                Vector3 pivotPosition = target.position + targetOffset;
                Vector3 desiredPosition = pivotPosition + rotation * new Vector3(0f, 0f, -distance);
                transform.position = desiredPosition;
                transform.rotation = rotation;
            }
            else
            {
                transform.rotation = rotation;
            }
        }
    }


}
