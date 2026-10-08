using UnityEngine;

namespace FindTheLover.Environment
{
    public enum BillboardMode
    {
        FaceCamera, // Aligns sprite plane with the Camera (Standard 2.5D presentation)
        FacePlayer  // Rotates to track player position when approaching
    }

    /// <summary>
    /// Presentation Layer component that automatically orients 2D sprites towards the Camera or Player.
    /// Eliminates paper-thin silhouette distortion when the camera orbits in 3D space.
    /// Supports instant alignment for characters and smooth damping for environment props.
    /// </summary>
    [ExecuteAlways]
    public class DynamicBillboard : MonoBehaviour
    {
        [Header("Rotation Mode")]
        [Tooltip("Choose whether to align with Camera orientation or look towards the Player")]
        [SerializeField] private BillboardMode mode = BillboardMode.FaceCamera;

        [Header("Optimization & Responsiveness")]
        [Tooltip("Instantly aligns without damping. Recommended for Player and Enemies to eliminate rotation lag")]
        [SerializeField] private bool instantRotation = true;

        [Tooltip("Damping speed when instantRotation is disabled (higher values rotate faster)")]
        [SerializeField] private float smoothRotationSpeed = 15f;

        [Tooltip("Maximum distance from camera to update rotation (saves CPU for distant environment objects)")]
        [SerializeField] private float activeDistance = 50f;

        [Header("Axis Constraints")]
        [Tooltip("Locks the vertical Y axis so upright sprites do not tilt into the sky")]
        [SerializeField] private bool lockYAxis = true;

        private Camera _mainCam;
        private Transform _playerTransform;

        private void Start()
        {
            _mainCam = Camera.main;
            FindPlayer();
        }

        private void LateUpdate()
        {
            if (_mainCam == null) _mainCam = Camera.main;
            if (_mainCam == null) return;

            // 1. Distance check: Skip calculation if beyond active rendering threshold
            float distToCam = Vector3.Distance(transform.position, _mainCam.transform.position);
            if (distToCam > activeDistance) return;

            Vector3 targetDirection = Vector3.zero;

            // 2. Evaluate target direction
            if (mode == BillboardMode.FaceCamera)
            {
                targetDirection = _mainCam.transform.forward;
            }
            else if (mode == BillboardMode.FacePlayer)
            {
                if (_playerTransform == null) FindPlayer();
                if (_playerTransform != null)
                {
                    targetDirection = (_playerTransform.position - transform.position).normalized;
                }
            }

            // 3. Lock vertical tilt to prevent standing sprites from laying flat on the ground
            if (lockYAxis)
            {
                targetDirection.y = 0f;
            }

            if (targetDirection.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(targetDirection);

                // 4. Apply instant snap or smooth damped interpolation
                if (instantRotation || !Application.isPlaying)
                {
                    transform.rotation = targetRotation;
                }
                else
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, smoothRotationSpeed * Time.deltaTime);
                }
            }
        }

        private void FindPlayer()
        {
            var player = GameObject.FindWithTag("Player");
            if (player == null) player = GameObject.Find("Quene_Player");
            if (player != null) _playerTransform = player.transform;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, activeDistance);
        }
    }
}