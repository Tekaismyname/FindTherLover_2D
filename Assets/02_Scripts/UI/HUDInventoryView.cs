using TMPro;
using UnityEngine;
using UnityEngine.UI;
using FindTheLover.Items;

namespace FindTheLover.UI
{
    /// <summary>
    /// Presentation Layer View for displaying resource counters on the player HUD.
    /// Strictly Event-Driven: Only refreshes when OnInventoryChanged fires.
    /// </summary>
    public class HUDInventoryView : MonoBehaviour
    {
        [Header("Data Reference")]
        [Tooltip("Blueprint data of the wood item to count")]
        [SerializeField] private ItemDataSO woodItemData;

        [Header("UI Elements")]
        [Tooltip("TextMeshPro text component displaying the wood quantity")]
        [SerializeField] private TextMeshProUGUI woodCountText;

        [Tooltip("Image component displaying the wood icon")]
        [SerializeField] private Image woodiconImage;

        [Header("Source")]
        [Tooltip("Reference to the player's inventory system. If null, automatically finds local player.")]
        [SerializeField] private InventorySystem playerInventory;

        private void Start()
        {
            // if inventory not assigned in inspector, automatically  locate player's inventory'
            if (playerInventory == null)
            {
                var player = GameObject.FindWithTag("Player");
                if (player != null)
                {
                    if (playerInventory == null)
                    {
                        playerInventory = player.GetComponentInParent<InventorySystem>();
                    }
                }
            }

            // setup icon from itemDataSO if assigned
            if (woodiconImage != null && woodItemData != null && woodItemData.icon != null)
            {
                woodiconImage.sprite = woodItemData.icon;
            }


            // subcribe to inventory  changes
            SubscribeToEvents();

            // Initial UI  refresh
            RefreshDisplay();
        }


        private void OnEnable()
        {
            SubscribeToEvents();
        }

        private void OnDisable()
        {
            UnsubscribeFromEvents();
        }

        private void SubscribeToEvents()
        {
            if (playerInventory != null)
            {
                playerInventory.OnInventoryChanged -= RefreshDisplay;
                playerInventory.OnInventoryChanged += RefreshDisplay;

            }
        }

        private void UnsubscribeFromEvents()
        {
            if (playerInventory != null)
            {
                playerInventory.OnInventoryChanged -= RefreshDisplay;
            }
        }
        
         /// <summary>
        /// Recalculates total wood and updates text display.
        /// </summary
        public void RefreshDisplay()
        {
            if (woodCountText == null) return;

            int totalWood = 0;
            if (playerInventory != null && woodItemData != null)
            {
                totalWood = playerInventory.GetItemCount(woodItemData);
            }

            woodCountText.text = $"x {totalWood}";
        }
        
    }

}