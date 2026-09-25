using UnityEngine;
using UnityEngine.InputSystem;

namespace FindTheLover.CameraSystem
{
    /// <summary>
    /// Smooth 2.5D / HD-2D Follow Camera with collision-free elevation,
    /// dynamic zoom, and smooth damping for 3D terrain traversal.
    /// </summary>
    public class CameraFollow3D : MonoBehaviour
    {
        [Header("Target Tracking")]
        [Tooltip("The player transform to follow")]
        [SerializeField] private Transform target;
        [Tooltip("Target look-at height offset (chest / upper body)")]
        [SerializeField] private Vector3 lookAtOffset = new Vector3(0f, 1.2f, 0f);

        [Header("Camera Positioning & Angle")]
        [Tooltip("Default camera offset relative to target (X: horizontal, Y: height, Z: distance)")]
        [SerializeField] private Vector3 defaultOffset = new Vector3(0f, 9.5f, -12.5f);
        [Tooltip("Fixed camera pitch angle looking down at character (degrees)")]
        [Range(20f, 60f)]
        [SerializeField] private float pitchAngle = 36f;

        [Header("Smooth Damping")]
        [Tooltip("Time to reach target position smoothly (seconds)")]
        [Range(0.05f, 0.5f)]
        [SerializeField] private float smoothTime = 0.15f;

        [Header("Zoom Settings")]
        [Tooltip("Enable mouse scroll wheel zooming")]
        [SerializeField] private bool enableZoom = true;
        [SerializeField] private float zoomSpeed = 2.5f;
        [SerializeField] private float minZoom = 5f;
        [SerializeField] private float maxZoom = 25f;

        // Internal velocity for SmoothDamp
        private Vector3 _currentVelocity;
        private float _currentZoomDistance;

        private void Start()
        {
            // Auto-detect player if not assigned
            if (target == null)
            {
                var player = GameObject.Find("Quene_Player");
                if (player != null) target = player.transform;
            }

            _currentZoomDistance = defaultOffset.magnitude;

            // Snap camera immediately to target on start
            if (target != null)
            {
                transform.position = target.position + defaultOffset;
                transform.rotation = Quaternion.Euler(pitchAngle, 0f, 0f);
            }
        }

        private void LateUpdate()
        {
            if (target == null) return;

            HandleZoom();
            FollowTarget();
        }

        /// <summary>
        /// Reads scroll wheel input to zoom camera in and out.
        /// </summary>
        private void HandleZoom()
        {
            if (!enableZoom) return;

            float scroll = 0f;
            if (Mouse.current != null)
            {
                scroll = Mouse.current.scroll.ReadValue().y;
            }

            if (Mathf.Abs(scroll) > 0.01f)
            {
                _currentZoomDistance -= Mathf.Sign(scroll) * zoomSpeed;
                _currentZoomDistance = Mathf.Clamp(_currentZoomDistance, minZoom, maxZoom);
            }
        }

        /// <summary>
        /// Smoothly tracks player position with high-precision damping.
        /// </summary>
        private void FollowTarget()
        {
            // Direction from target to camera normalized
            Vector3 direction = defaultOffset.normalized;
            Vector3 desiredOffset = direction * _currentZoomDistance;

            Vector3 desiredPosition = target.position + desiredOffset;

            // Smoothly interpolate position
            transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref _currentVelocity, smoothTime);

            // Maintain stable camera pitch looking down at target lookAt position
            Vector3 lookTarget = target.position + lookAtOffset;
            Quaternion targetRotation = Quaternion.Euler(pitchAngle, 0f, 0f);
            transform.rotation = targetRotation;
        }

        /// <summary>
        /// Manually assign target at runtime (e.g. on respawn).
        /// </summary>
        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }
    }
}
