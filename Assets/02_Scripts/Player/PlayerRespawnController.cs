using UnityEngine;
using FindTheLover.Combat;
using System.Collections;

namespace FindTheLover.Player
{
    /// <summary>
    /// Logic Layer: Listens to player death event, handles respawn flow,
    /// and safely teleports the character back to the campfire sanctuary.
    /// </summary>
    public class PlayerRespawnController : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Player HealthSystem reference.")]
        [SerializeField] private HealthSystem healthSystem;

        [Tooltip("Spawn point anchor located beside the Campfire.")]
        [SerializeField] private Transform campfireSpawnPoint;

        private CharacterController _characterController;

        private void Awake()
        {
            if (healthSystem == null) healthSystem = GetComponent<HealthSystem>();
            _characterController = GetComponent<CharacterController>();
        }

        private void OnEnable()
        {
            if (healthSystem != null) healthSystem.OnDeath += HandlePlayerDeath;
        }

        private void OnDisable()
        {
            if (healthSystem != null) healthSystem.OnDeath -= HandlePlayerDeath;
        }

        private void HandlePlayerDeath()
        {
            Debug.Log("[PlayerRespawnController] Player perished! Teleporting to Campfire in 1.5s...");
            StartCoroutine(RespawnRoutine(1.5f));
        }

        private IEnumerator RespawnRoutine(float delay)
        {
            yield return new WaitForSeconds(delay);

            // Temporarily disable CharacterController to allow position displacement
            if (_characterController != null) _characterController.enabled = false;

            if (campfireSpawnPoint != null)
            {
                transform.position = campfireSpawnPoint.position + Vector3.up * 0.1f;
            }
            else
            {
                transform.position = Vector3.up * 0.5f;
            }

            if (_characterController != null) _characterController.enabled = true;

            // Revive with 100% full health
            if (healthSystem != null)
            {
                healthSystem.SetMaxHealth(healthSystem.MaxHealth, resetCurrentHealth: true);
            }

            Debug.Log("[PlayerRespawnController] Player safely revived at Campfire!");
        }
    }
}