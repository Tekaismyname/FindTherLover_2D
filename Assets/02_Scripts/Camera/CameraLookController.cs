using UnityEditor.SettingsManagement;
using UnityEngine;


namespace FindTheLover.CameraSystem
{
    /// <summary>
    /// Rotates player body horizontally (Yaw) and camera pivot vertically (Pitch).
    /// Driven purely by ILookInputProvider and CameraSettingSO.
    /// </summary>
    public class CameraLookController : MonoBehaviour
    {
        [Header("Data & Transforms")]
        [SerializeField] private CameraSettingSO settings;
        [SerializeField] private Transform pitchTransform;
        [SerializeField] private Transform yawTransform;

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

        private void Awake()
        {
            if (target == null)
            {
                var player = GameObject.Find("Quene_Player");
                if (player != null) target = player.transform;
            }
            _lookInputProvider = GetComponent<ILookInputProvider>();
            if (pitchTransform == null) pitchTransform = transform;

            _yaw = yawTransform != null ? yawTransform.eulerAngles.y :
            pitchTransform.eulerAngles.y;
            _pitch = pitchTransform.localEulerAngles.x;

            if (_pitch > 180f) _pitch -= 360f; // Normalize 0..360 t -180...180
            _targetPitch = _pitch;
            _targetYaw = _yaw;
        }

        private void LateUpdate()
        {
            if (settings == null || _lookInputProvider == null) return;

            Vector2 delta = _lookInputProvider.GetLookDelta();

            //  Calculate target angles with Reverse Y support
            _targetYaw += delta.x * settings.sensitivityX;
            float pitchDelta = (settings.inverY ? 1f : -1f) * delta.y * settings.sensitivityY;
            _targetPitch = Mathf.Clamp(_targetPitch + pitchDelta, settings.minPitch, settings.maxPitch);
            _yaw = Mathf.SmoothDampAngle(_yaw, _targetYaw, ref _yawVelocity, settings.smoothTime);
            _pitch = Mathf.Clamp(Mathf.SmoothDamp(_pitch, _targetPitch, ref _pitchVelocity, settings.smoothTime), settings.minPitch, settings.maxPitch);

            // Apply rorations 
            Quaternion rotation = Quaternion.Euler(_pitch, _yaw, 0f);
            if (target != null)
            {
                Vector3 pivotPosition = target.position + targetOffset;
                // Đẩy camera ra phía sau pivot theo khoảng cách distance
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
