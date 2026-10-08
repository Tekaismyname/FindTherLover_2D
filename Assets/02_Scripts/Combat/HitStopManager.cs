using System.Collections;
using UnityEngine;

namespace FindTheLover.Combat
{
    /// <summary>
    /// Logic Layer singleton that manages frame freezes (Hit Stop) during impacts.
    /// Freezes Time.timeScale briefly while allowing real-time effects like CameraShake to continue.
    /// </summary>

    public class HitStopManager : MonoBehaviour
    {
        public static HitStopManager Instance { get; private set; }

        private Coroutine _hitStopCoroutine;

        private void Awake() {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }


        void OnDestroy()
        {
            if (Instance == this)
            {
                Time.timeScale = 1f;
                Instance = null;
            }
        }


        /// <summary>
        /// Triggers a brief game freeze using real-world seconds.
        /// </summary>
        /// <param name="duration">Duration in unscaled real-time seconds (e.g. 0.05f to 0.08f).</param>

        public void TriggerHitStop(float duration)
        {
            if (duration <= 0f) return;

            // Cancel any pending unfreeze to smoothly extend or refresh hit stop
            if (_hitStopCoroutine != null)
            {
                StopCoroutine(_hitStopCoroutine);
            }

            _hitStopCoroutine = StartCoroutine(HitStopRoutine(duration));
        }
        
        private IEnumerator HitStopRoutine(float duration)
        {
            Time.timeScale = 0f;

            // WaitForSecondRealtime continues ticking even when Time.timeScale is 0
            yield return new WaitForSecondsRealtime(duration);

            Time.timeScale = 1f;
            _hitStopCoroutine = null;
        }
    }
}