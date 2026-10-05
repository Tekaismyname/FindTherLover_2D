using System;
using UnityEngine;

namespace FindTheLover.Combat
{
    /// <summary>
    /// Core health and damage controller for players, enemies, and destructible entities.
    /// Implements IDamageable, enforces I-Frames, and notifies listeners via Action events.
    /// </summary>
    public class HealthSystem :  MonoBehaviour, IDamageable
    {
        [Header("Health Settings")]
        [Tooltip("Maximun  health pool")]
        [SerializeField] private float maxHealth = 100f;

        [Tooltip("Invincibility duration in second  after taking damage (I-Frames)")]
        [SerializeField] private float IFrameDuration = 0.25f;

        [Header("Audio Feedback")]
        [Tooltip("Audio clip played upon taking damage")]
        [SerializeField] private AudioClip hurtSound;

        [Tooltip("Audio clip played upon death")]
        [SerializeField] private AudioClip deathSound;

        private float _currentHealth;
        private float _lastDamageTime = -999f;
        private bool _isDead = false;

        // c# event for decoupled presentation (UI & VFX)
        public event Action<float, float> OnHealthChanged;
        public event Action<Vector3> OnDamaged;
        public event Action OnDeath;

        public float CurrentHealth => _currentHealth;
        public float MaxHealth => maxHealth;
        public bool IsDead => _isDead;

        private void Awake()
        {
            _currentHealth = maxHealth;
        }

        private void Start()
        {
            // Initial  notification so UI can set fillAmount to  1.0
            OnHealthChanged?.Invoke(_currentHealth, maxHealth);
        }

        /// <summary>
        /// Dynamically overrides the maximum health pool (e.g., injected from EnemyDataSO or Equipment).
        /// </summary>
        /// <param name="newMaxHealth">New maximum health value.</param>
        /// <param name="resetCurrentHealth">If true, restores current health to the new maximum.</param>
        public void SetMaxHealth(float newMaxHealth, bool resetCurrentHealth = true)
        {
            maxHealth = Mathf.Max(1f, newMaxHealth);

            if (resetCurrentHealth)
            {
                _currentHealth = maxHealth;
                _isDead = false;
                // Notify UI to update fill amount immediately
                OnHealthChanged?.Invoke(_currentHealth, maxHealth);
            }
        }

        /// <summary>
        /// IDamageable implementation: deducts health with I-Frame checks and triggers events.
        /// </summary>
        public void TakeDamage(float damage, Vector3 hitPoint, Vector3 hitNormal)
        {
            if (_isDead) return;
            // 1. check  I-Frames  (invincibility cooldown)
            if (Time.time - _lastDamageTime < IFrameDuration)
            {
                return;
            }

            _lastDamageTime = Time.time;

            // 2.  Deduct health with Mathf.Clamp
            _currentHealth = Mathf.Clamp(_currentHealth - damage, 0f, maxHealth);

            // 3. Play hurt audio
            if (hurtSound != null)
            {
                AudioSource.PlayClipAtPoint(hurtSound, transform.position);
            }

            // 4.  Notify listeners
            OnHealthChanged?.Invoke(_currentHealth, maxHealth);
            OnDamaged?.Invoke(hitNormal);

            Debug.Log($"<color=red>[HealthSystem]</color> {gameObject.name} took {damage} damage! Remaining: {_currentHealth}/{maxHealth}");

            // 5. Check death condition
            if (_currentHealth <= 0f)
            {
                Die();
            }
        }


        /// <summary>
        /// Restores health, clamped to maxHealth.
        /// </summary>
        public void Heal(float amount)
        {
            if (_isDead || amount <= 0f) return;

            _currentHealth = Mathf.Clamp(_currentHealth + amount, 0f, maxHealth);
            OnHealthChanged?.Invoke(_currentHealth, maxHealth);

            Debug.Log($"<color=green>[HealthSystem]</color> {gameObject.name} healed +{amount}! Current: {_currentHealth}/{maxHealth}");
        } 
        
        
        public void Die()
        {
            _isDead = true;

            if (deathSound != null)
            {
                AudioSource.PlayClipAtPoint(deathSound, transform.position);
            }

            OnDeath?.Invoke();
            Debug.Log($"<color=black><b>[HealthSystem] {gameObject.name} has died!</b></color>");
        }
    }
}