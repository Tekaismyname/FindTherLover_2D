using UnityEngine;
using System.Collections;
using System;

namespace FindTheLover.Combat
{
    /// <summary>
    /// Presentation juice effect: Flashes the SpriteRenderer in red upon receiving damage.
    /// Subscribes to HealthSystem.OnDamaged.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class DamageFlash : MonoBehaviour
    {
        [Header("Flash Comnfiguration")]
        [Tooltip("Color to flash when hit")]
        [SerializeField] private Color flashColor = Color.red;

        [Tooltip("Duration of the flash effect in seconds")]
        [Range(0.05f, 0.4f)][SerializeField] private float flashDuration = 0.15f;

        private SpriteRenderer _spriteRenderer;
        private HealthSystem _healthSystem;
        private Color _originalColor = Color.white;
        private Coroutine _flashCoroutine;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _healthSystem = GetComponent<HealthSystem>();

            if (_healthSystem == null)
            {
                _healthSystem = GetComponentInParent<HealthSystem>();
            }

            if (_spriteRenderer != null)
            {
                _originalColor = _spriteRenderer.color;
            }
        }

        private void OnEnable()
        {
            if (_healthSystem != null)
            {
                _healthSystem.OnDamaged += TriggerFlash;
            }
        }

        private void OnDisable()
        {
            if (_healthSystem != null)
            {
                _healthSystem.OnDamaged -= TriggerFlash;
            }
        }

        private void TriggerFlash(Vector3 hitNormal)
        {
            if (_flashCoroutine != null) StopCoroutine(_flashCoroutine);
            _flashCoroutine = StartCoroutine(FlashRoutine());
        }
        
        private IEnumerator FlashRoutine()
        {
            float elapsed = 0f;

            while (elapsed < flashDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / flashDuration;

                // Smoothly iterpolate from flashColor back to original color
                _spriteRenderer.color = Color.Lerp(flashColor, _originalColor, t);
                yield return null;
            }

            _spriteRenderer.color = _originalColor;
        }
    }
}