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

        [Header("Singleplayer Modal Integration")]
        [Tooltip("When false and SurvivalFlowModalUI exists in scene, waits for the player to click Respawn on the Game Over screen.")]
        [SerializeField] private bool autoRespawnWithoutModal = false;

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
            bool hasGameOverModal = Object.FindAnyObjectByType<FindTheLover.UI.SurvivalFlowModalUI>(FindObjectsInactive.Include) != null;
            if (!autoRespawnWithoutModal && hasGameOverModal)
            {
                Debug.Log("[PlayerRespawnController] Player perished! Handing off to SurvivalFlowModalUI Game Over screen.");
                return;
            }

            Debug.Log("[PlayerRespawnController] Player perished! Teleporting to Campfire in 1.5s...");
            StartCoroutine(RespawnRoutine(1.5f));
        }

        /// <summary>
        /// Immediately revives the player at full health beside the Campfire Sanctuary.
        /// Invoked by SurvivalFlowModalUI when clicking "RESPAWN AT CAMPFIRE".
        /// </summary>
        public void RespawnNowAtCampfire()
        {
            if (_characterController != null) _characterController.enabled = false;

            if (campfireSpawnPoint != null)
            {
                transform.position = campfireSpawnPoint.position + Vector3.up * 0.1f;
            }
            else
            {
                transform.position = new Vector3(0f, 10.2f, 0f);
            }

            if (_characterController != null) _characterController.enabled = true;

            if (healthSystem != null)
            {
                healthSystem.SetMaxHealth(healthSystem.MaxHealth, resetCurrentHealth: true);
            }

            Debug.Log("[PlayerRespawnController] Player safely revived at Campfire Sanctuary!");
        }

        private IEnumerator RespawnRoutine(float delay)
        {
            yield return new WaitForSeconds(delay);

            // Abort campfire respawn if an augment (e.g., Immortal Lover Bond) already revived the player
            if (healthSystem != null && !healthSystem.IsDead)
            {
                yield break;
            }

            RespawnNowAtCampfire();
        }
    }
}