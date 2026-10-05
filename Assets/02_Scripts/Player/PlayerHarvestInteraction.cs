using UnityEngine;
using UnityEngine.InputSystem;
using FindTheLover.Combat;
using Unity.VisualScripting;

namespace FindTheLover.Player
{
     /// <summary>
    /// Handles player interaction with harvestable nodes and world loot drops.
    /// Supports automatic proximity sensing and interaction inputs (PC key E / Android touch button).
    /// </summary>
     
    public class PlayerHarvestInteraction : MonoBehaviour
    {
        [Header("Detection Settings")]
        [Tooltip("Radius around the player to scan for harvestable nodes or dropped items")]
        [SerializeField] private float scanRadius = 2.5f;

        [Tooltip("Layers representing harvestable node and dropped items")]
        [SerializeField] private LayerMask interactableLayers;

        [Header("Current Equipped Tool")]
        [Tooltip("The tool  currently held in the player's hands")]
        [SerializeField] private HarvestToolType currentEquippedTool = HarvestToolType.None;

        [Header("Input (New Input System)")]
        [Tooltip("Optional interact action reference (Key E)")]
        [SerializeField] private InputActionReference interactAction;

        private void OnEnable()
        {
            if (interactAction != null && interactAction.action != null)
            {
                interactAction.action.Enable();
            }
        }

        private void OnDisable()
        {
            if (interactAction != null && interactAction.action != null)
            {
                interactAction.action.Disable();
            }
        }

        private void Update()
        {
            // Trigger interaction when button/Tourch pressed
            if (interactAction != null && interactAction.action != null
                && interactAction.action.WasPressedThisFrame())
            {
                TryInteractNearby();
            }
        }

        /// <summary>
        /// Scans for the nearest interactable object and performs appropriate interaction.
        /// </summary>
        public void TryInteractNearby()
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, scanRadius, interactableLayers);
            if (hits.Length == 0) return;

            Collider nearest = null;

            float minDistance = float.MaxValue;

            foreach (var hit in hits)
            {
                if (hit.transform.root == transform.root) continue;

                float dist = Vector3.Distance(transform.position, hit.transform.position);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    nearest = hit;
                }
            }

            if (nearest != null)
            {
                // Check if target is a harvestabl resource node
                if (nearest.TryGetComponent<IHarvestable>(out var harvestable))
                {
                    Vector3 hitPoint = nearest.ClosestPoint(transform.position);
                    harvestable.Harvest(15f, currentEquippedTool, hitPoint);
                }
            }

        }


        /// <summary>
        /// Equips a new tool into the player's active hand.
        /// </summary>
        public void SetEquippedTool(HarvestToolType newTool)
        {
            currentEquippedTool = newTool;
            Debug.Log($"[PlayerHarvestInteraction]  switched tool to: {newTool}");
        }
        
        private void OnDrawGizmosSelected() {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, scanRadius);
        }
    }

}