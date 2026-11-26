using System;
using TMPro;
using UnityEngine;
using FindTheLover.UI.Data;

namespace FindTheLover.UI
{
    /// <summary>
    /// Presentation Layer: Single pooled 2.5D World-Space floating damage number popup.
    /// Billboards toward Camera.main, plays an Ease-Out Back punch scale + decelerating upward arc,
    /// fades out cleanly, and returns itself to DamageNumberSpawner with zero GC allocations.
    /// </summary>
    [RequireComponent(typeof(TextMeshPro))]
    public class DamageNumberPopup : MonoBehaviour
    {
        private TextMeshPro _tmp;
        private Camera _mainCamera;
        private Action<DamageNumberPopup> _onCompleted;

        private Vector3 _startWorldPos;
        private Vector3 _driftVector;
        private Color _baseColor;
        private float _lifetime = 0.85f;
        private float _riseHeight = 1.85f;
        private float _fadeStart = 0.55f;
        private float _peakScale = 1.25f;
        private float _elapsed;
        private bool _isPlaying;

        public bool IsPlaying => _isPlaying;

        private void Awake()
        {
            _tmp = GetComponent<TextMeshPro>();
            _mainCamera = Camera.main;
            ConfigureSortingOrder();
        }

        /// <summary>
        /// Initializes and starts playback of this pooled popup at a world impact position.
        /// </summary>
        public void Play(
            string text,
            Vector3 worldPos,
            Color color,
            float fontSize,
            float peakScale,
            float lifetime,
            float riseHeight,
            float fadeStartNormalized,
            Vector3 lateralDrift,
            Action<DamageNumberPopup> onCompleted)
        {
            if (_tmp == null) _tmp = GetComponent<TextMeshPro>();
            if (_mainCamera == null) _mainCamera = Camera.main;

            _startWorldPos = worldPos;
            _driftVector = lateralDrift;
            _baseColor = color;
            _lifetime = Mathf.Max(0.15f, lifetime);
            _riseHeight = riseHeight;
            _fadeStart = Mathf.Clamp(fadeStartNormalized, 0.1f, 0.95f);
            _peakScale = Mathf.Max(0.5f, peakScale);
            _onCompleted = onCompleted;
            _elapsed = 0f;
            _isPlaying = true;

            _tmp.text = text;
            _tmp.fontSize = fontSize;
            _tmp.color = color;
            ConfigureSortingOrder();

            transform.position = worldPos;
            transform.localScale = Vector3.one * 0.4f;
            AlignToCamera();
            gameObject.SetActive(true);
        }

        private void LateUpdate()
        {
            if (!_isPlaying) return;

            _elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(_elapsed / _lifetime);

            EvaluateAnimation(t, _riseHeight, _peakScale, _fadeStart, out float yOffset, out float scale, out float alpha);

            // Smooth upward arc + gentle horizontal drift
            transform.position = _startWorldPos + new Vector3(_driftVector.x * t, yOffset, _driftVector.z * t);
            transform.localScale = Vector3.one * scale;

            Color c = _baseColor;
            c.a = alpha;
            _tmp.color = c;

            AlignToCamera();

            if (_elapsed >= _lifetime)
            {
                _isPlaying = false;
                gameObject.SetActive(false);
                _onCompleted?.Invoke(this);
            }
        }

        /// <summary>
        /// Pure deterministic animation evaluator computing (yOffset, scale, alpha) at normalized progress t (0..1).
        /// Enables strict Unit Testing without requiring Play Mode frame loops.
        /// </summary>
        public static void EvaluateAnimation(
            float normalizedTime,
            float riseHeight,
            float peakScale,
            float fadeStartNormalized,
            out float yOffset,
            out float scale,
            out float alpha)
        {
            float t = Mathf.Clamp01(normalizedTime);

            // Ease-Out Cubic vertical lift (fast pop upward, smooth hover settle)
            float easedLift = 1f - Mathf.Pow(1f - t, 3f);
            yOffset = easedLift * riseHeight;

            // Elastic punch-in during the first 22% of lifetime, then settle smoothly to 0.92x
            if (t < 0.22f)
            {
                float popT = t / 0.22f;
                scale = Mathf.Lerp(0.45f, peakScale, 1f - Mathf.Pow(1f - popT, 2f));
            }
            else
            {
                float settleT = (t - 0.22f) / 0.78f;
                scale = Mathf.Lerp(peakScale, 0.92f, settleT);
            }

            // Crisp visibility until fadeStartNormalized, then linear fade to 0
            if (t <= fadeStartNormalized)
            {
                alpha = 1f;
            }
            else
            {
                float fadeT = (t - fadeStartNormalized) / Mathf.Max(0.01f, 1f - fadeStartNormalized);
                alpha = Mathf.Clamp01(1f - fadeT);
            }
        }

        private void AlignToCamera()
        {
            if (_mainCamera == null) _mainCamera = Camera.main;
            if (_mainCamera != null)
            {
                transform.rotation = _mainCamera.transform.rotation;
            }
        }

        private void ConfigureSortingOrder()
        {
            if (_tmp != null && _tmp.renderer != null)
            {
                _tmp.renderer.sortingOrder = 200;
            }
        }
    }
}
