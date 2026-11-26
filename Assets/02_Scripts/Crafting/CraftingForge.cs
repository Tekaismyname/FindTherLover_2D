using System;
using System.Collections.Generic;
using UnityEngine;
using FindTheLover.Core;
using FindTheLover.Crafting.Data;
using FindTheLover.Items;
using FindTheLover.Player;

namespace FindTheLover.Crafting
{
    /// <summary>
    /// Logic Layer: Muck-Style Crafting Forge evaluating ingredient availability in InventorySystem,
    /// deducting resources atomically, upgrading PlayerHotbarController modular weapons/tools,
    /// and synchronizing live state to GameDataManager.
    /// </summary>
    public class CraftingForge : MonoBehaviour
    {
        public static CraftingForge Instance { get; private set; }

        [Header("Data Layer Recipe Catalog")]
        [Tooltip("Ordered list of craftable 5-tier weapons, tools, torches, and healing consumables.")]
        [SerializeField] private List<CraftingRecipeSO> _recipes = new List<CraftingRecipeSO>();

        [Header("Audio Feedback")]
        [SerializeField] private AudioClip _anvilCraftSFX;

        /// <summary>Broadcast whenever a recipe is successfully crafted.</summary>
        public event Action<CraftingRecipeSO> OnItemCrafted;

        public IReadOnlyList<CraftingRecipeSO> Recipes => _recipes;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        /// <summary>
        /// Replaces the recipe list dynamically (used by Editor builders and automated unit tests).
        /// </summary>
        public void SetRecipes(List<CraftingRecipeSO> recipes)
        {
            _recipes = recipes ?? new List<CraftingRecipeSO>();
        }

        /// <summary>
        /// Returns true if the specified inventory holds enough of every required ingredient for this recipe.
        /// </summary>
        public bool CanCraft(CraftingRecipeSO recipe, InventorySystem inventory)
        {
            if (recipe == null || inventory == null) return false;
            if (recipe.ingredients == null || recipe.ingredients.Count == 0) return true;

            for (int i = 0; i < recipe.ingredients.Count; i++)
            {
                CraftingIngredient req = recipe.ingredients[i];
                if (req.item == null || req.requiredAmount <= 0) continue;
                if (inventory.GetItemCount(req.item) < req.requiredAmount)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Atomically deducts required ingredients and grants the crafted weapon upgrade or consumable item.
        /// </summary>
        public bool TryCraft(CraftingRecipeSO recipe, InventorySystem inventory, PlayerHotbarController hotbar = null)
        {
            if (!CanCraft(recipe, inventory))
            {
                Debug.LogWarning($"[CraftingForge] Missing ingredients to craft '{(recipe != null ? recipe.displayName : "null")}'.");
                return false;
            }

            // 1. Deduct all required ingredients
            for (int i = 0; i < recipe.ingredients.Count; i++)
            {
                CraftingIngredient req = recipe.ingredients[i];
                if (req.item != null && req.requiredAmount > 0)
                {
                    inventory.RemoveItem(req.item, req.requiredAmount);
                }
            }

            // 2. Grant crafted equipment directly into the matching Modular Hand Socket Hotbar slot
            if (recipe.resultEquipment != null)
            {
                PlayerHotbarController targetHotbar = hotbar ?? FindAnyObjectByType<PlayerHotbarController>();
                if (targetHotbar != null)
                {
                    targetHotbar.AssignAndEquipCraftedEquipment(recipe.resultEquipment);
                }
            }

            // 3. Grant crafted inventory item (e.g., Healing Berries / Apple / Torch refill)
            if (recipe.resultItem != null && recipe.resultItemQuantity > 0)
            {
                inventory.AddItem(recipe.resultItem, recipe.resultItemQuantity);
            }

            // 4. Sync with GameDataManager & play forge sound
            if (_anvilCraftSFX != null)
            {
                AudioSource.PlayClipAtPoint(_anvilCraftSFX, transform.position);
            }

            if (GameDataManager.Instance != null)
            {
                GameDataManager.Instance.CaptureSnapshotFromWorld();
            }

            Debug.Log($"<color=#FFD700>[CraftingForge]</color> Crafted [{recipe.tier}] {recipe.displayName}!");
            OnItemCrafted?.Invoke(recipe);
            return true;
        }
    }
}
