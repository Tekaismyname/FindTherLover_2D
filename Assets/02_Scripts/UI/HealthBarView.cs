using UnityEngine;
using UnityEngine.UI;

namespace FindTheLover.Combat.UI
{
    /// <summary>
    /// Presentation View for displaying a floating World-Space health bar over entities.
    /// Bilboards towards the main camera and subscribes to HealthSystem events.
    /// </summary>

    public class HealthBarView : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("The healthSystem component to observe")]
        [SerializeField] private HealthSystem healthSystem;

        [Tooltip("The UI Image configured as filled to display remianing health")]
        [SerializeField] private Image healthFillImage;

        private Camera _mainCamera;

        private void Awake()
        {
            _mainCamera = Camera.main;

            if (healthSystem == null)
            {
                healthSystem = GetComponentInParent<HealthSystem>();
            }
        }

        private void OnEnable()
        {
            if (healthSystem != null)
            {
                healthSystem.OnHealthChanged += UpdateHealthBar;
            }
        }

        void OnDisable()
        {
            if (healthSystem != null)
            {
                healthSystem.OnHealthChanged -= UpdateHealthBar;
            }
        }

        private void LateUpdate()
        {
            // Billboard: keep the health bar flat facecing the camera view
            if (_mainCamera != null)
            {
                transform.rotation = _mainCamera.transform.rotation;
            }
        }
        
        /// <summary>
        /// Recalculates fillAmount normalized between 0.0 and 1.0.
        /// </summary>
        public void UpdateHealthBar(float currentHealth, float maxHealth)
        {
            if (healthFillImage == null || maxHealth <= 0f) return;

            healthFillImage.fillAmount = currentHealth / maxHealth;            
        }
    }
}