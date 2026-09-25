using UnityEngine;

namespace FindTheLover.CameraSystem
{
    /// <summary>
    /// Data Layer Blueprint for camera sensitivity, clamping, and inversion settings.
    /// </summary>


    [CreateAssetMenu(fileName = "CameraSettings", menuName = "FindTheLover/Camera/Settings")]
    public class CameraSettingSO : ScriptableObject
    {
        [Header("Sensitivity  & Inversion")]
        [Tooltip("Horizontal Sensitivity multipler.")]
        public float sensitivityX = 0.15f;

        [Tooltip("Vertical sensitivity multipler.")]
        public float sensitivityY = 0.15f;

        [Tooltip("Invert vertical look axis (Reverse Y).")]
        public bool inverY = false;

        [Header("Pitch limits (Verical  Clamp)")]
        [Range(-89f, 0f)] public float minPitch = -75f;
        [Range(0f, 89f)] public float maxPitch = 75f;

        [Header("Smoothing")]
        [Range(0f, 0.1f)] public float smoothTime = 0.02f;
    }
}