using System;
using System.Collections.Generic;
using UnityEngine;
using FindTheLover.Equipment.Data;
using FindTheLover.Items;

namespace FindTheLover.Crafting.Data
{
    /// <summary>
    /// Single required ingredient entry inside a CraftingRecipeSO blueprint.
    /// </summary>
    [Serializable]
    public struct CraftingIngredient
    {
        public ItemDataSO item;
        [Min(1)] public int requiredAmount;
    }

    /// <summary>
    /// Data Layer (ScriptableObject): Blueprint defining a Muck-style crafting recipe for
    /// 5-tier modular weapons/tools (Wood -> Stone -> Iron -> Obsidian -> Adamantite) or consumables.
    /// </summary>
    [CreateAssetMenu(fileName = "Recipe_Data", menuName = "Find The Lover/Crafting/Crafting Recipe")]
    public class CraftingRecipeSO : ScriptableObject
    {
        [Header("Recipe Identity")]
        public string recipeId = "recipe_stone_sword";
        public string displayName = "Stone Broadsword";
        [TextArea(2, 3)]
        public string description = "Forged stone blade (+12 melee DMG). Upgrades Hotbar Slot 1.";
        public WeaponMaterialTier tier = WeaponMaterialTier.Stone;

        [Header("Required Ingredients")]
        public List<CraftingIngredient> ingredients = new List<CraftingIngredient>();

        [Header("Crafted Output (Equipment Upgrade OR Consumable Item)")]
        [Tooltip("If assigned, crafting upgrades/equips this modular weapon/tool into the Player Hotbar.")]
        public EquipmentItemDataSO resultEquipment;

        [Tooltip("If assigned, crafting adds this item stack into InventorySystem (e.g., Berries, Torch).")]
        public ItemDataSO resultItem;

        [Min(1)] public int resultItemQuantity = 1;

        /// <summary>
        /// Returns the primary display sprite for this recipe's output.
        /// </summary>
        public Sprite GetResultIcon()
        {
            if (resultEquipment != null && resultEquipment.weaponSprite != null) return resultEquipment.weaponSprite;
            if (resultItem != null && resultItem.icon != null) return resultItem.icon;
            return null;
        }

        /// <summary>
        /// Returns the material tint color applied to the recipe icon & UI badge.
        /// </summary>
        public Color GetResultTint()
        {
            if (resultEquipment != null) return resultEquipment.tierTintColor;
            return Color.white;
        }
    }
}
