using UnityEngine;
using UnityEngine.InputSystem;

namespace FindTheLover.Combat
{
    /// <summary>
    /// Debug testing utility to simulate damage and healing using keyboard keys.
    /// Press K to take 20 damage, Press H to heal 20 health.
    /// </summary>
    public class HealthTestInput : MonoBehaviour
    {
        private HealthSystem _healthSystem;

        private void Awake()
        {
            _healthSystem = GetComponent<HealthSystem>();
            if (_healthSystem == null)
            {
                _healthSystem = GetComponentInChildren<HealthSystem>();
            }
        }

        private void Update()
        {
            if (_healthSystem == null) return;

            // Press K: Simulate taking 20 damage
            if (Keyboard.current != null && Keyboard.current.kKey.wasPressedThisFrame)
            {
                _healthSystem.TakeDamage(20f, transform.position, Vector3.back);
            }

            // Press H: Simulate healing 20 health
            if (Keyboard.current != null && Keyboard.current.hKey.wasPressedThisFrame)
            {
                _healthSystem.Heal(20f);
            }
        }
    }
}