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

        private ILookInputProvider _lookInputProvider;
        private float _yaw;
        private float _pitch;
        private float _yawVelocity;
        private float _pitchVelocity;

        private void Awake()
        {
            _lookInputProvider = GetComponent<ILookInputProvider>();
            if (pitchTransform == null) pitchTransform = transform;

            _yaw = yawTransform != null ? yawTransform.eulerAngles.y :
            pitchTransform.eulerAngles.y;
            _pitch = pitchTransform.localEulerAngles.x;
            if (_pitch > 180f) _pitch -= 360f; // Normalize 0..360 t -180...180
        }
        
        private void LateUpdate() {
            if (settings == null || _lookInputProvider == null) return;

            Vector2 delta = _lookInputProvider.GetLookDelta();

            //  Calculate target angles with Reverse Y support
            float targetYaw = _yaw + delta.x * settings.sensitivityX;
            float targetPitch = Mathf.Clamp(
                _pitch + (settings.inverY ? 1f : -1f) * delta.y * settings.sensitivityY,
                settings.minPitch,
                settings.maxPitch
            );

            // Aplly smooth dampening to remove jitter
            _yaw = Mathf.SmoothDampAngle(_yaw, targetYaw, ref _yawVelocity, settings.smoothTime);
            _pitch = Mathf.SmoothDampAngle(_pitch, targetPitch, ref _pitchVelocity, settings.smoothTime);


            // Apply rorations 
            if (yawTransform != null)
            {
                yawTransform.rotation = Quaternion.Euler(0f, _yaw, 0f);
                pitchTransform.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
            }
            else
            {
                pitchTransform.rotation = Quaternion.Euler(_pitch, _yaw, 0f);
            }    
        }
    }   


}
