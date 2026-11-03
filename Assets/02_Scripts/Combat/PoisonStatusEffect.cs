using System.Collections;
using UnityEngine;

namespace FindTheLover.Combat
{
    /// <summary>
    /// Logic Layer: Temporary status effect inflicted upon entities struck by toxic attacks.
    /// Deducts health periodically (Damage Over Time / DoT) and refreshes duration upon reapplication.
    /// </summary>
    public class PoisonStatusEffect : MonoBehaviour
    {
        [Header("Status Feedback")]
        [Tooltip("Color tint applied to the entity's SpriteRenderer while poisoned.")]
        [SerializeField] private Color poisonTint = new Color(0.4f, 1f, 0.4f, 1f);

        private HealthSystem _healthSystem;
        private SpriteRenderer _spriteRenderer;
        private Color _originalColor = Color.white;
        private float _remainingDuration;
        private float _damagePerSecond = 2f;
        private Coroutine _dotCoroutine;

        private void Awake()
        {
            _healthSystem = GetComponentInChildren<HealthSystem>() ?? GetComponentInParent<HealthSystem>();
            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();

            if (_spriteRenderer != null)
            {
                _originalColor = _spriteRenderer.color;
            }
        }

        /// <summary>
        /// Applies or stacks poison DoT, refreshing duration if already active.
        /// </summary>
        /// <param name="dps">Damage dealt per tick.</param>
        /// <param name="duration">Duration in seconds.</param>
        public void ApplyPoison(float dps, float duration)
        {
            _damagePerSecond = dps;
            _remainingDuration = Mathf.Max(_remainingDuration, duration);

            if (_dotCoroutine == null)
            {
                _dotCoroutine = StartCoroutine(PoisonTickRoutine());
            }
        }

        private IEnumerator PoisonTickRoutine()
        {
            if (_spriteRenderer != null)
            {
                _spriteRenderer.color = poisonTint;
            }

            while (_remainingDuration > 0f)
            {
                yield return new WaitForSeconds(1.0f);
                _remainingDuration -= 1.0f;

                if (_healthSystem != null && !_healthSystem.IsDead)
                {
                    _healthSystem.TakeDamage(_damagePerSecond, transform.position, Vector3.up);
                    Debug.Log($"<color=lime>[PoisonStatusEffect]</color> {gameObject.name} took {_damagePerSecond} poison tick damage! (Remaining: {_remainingDuration:F1}s)");
                }
                else
                {
                    break;
                }
            }

            if (_spriteRenderer != null)
            {
                _spriteRenderer.color = _originalColor;
            }

            _dotCoroutine = null;
        }

        private void OnDisable()
        {
            if (_spriteRenderer != null)
            {
                _spriteRenderer.color = _originalColor;
            }
        }
    }
}
