using System;
using System.Collections.Generic;
using UnityEngine;

namespace FindTheLover.Items
{

    /// <summary>
    /// Represents a single slot in the inventory holding an item blueprint and count.
    /// </summary>
    [Serializable]
    public class ItemStack
    {
        public ItemDataSO itemData;
        public int quantity;

        public ItemStack(ItemDataSO data, int amount)
        {
            itemData = data;
            quantity = amount;
        }

    }

    /// <summary>
    /// Controller for managing player inventory capacity, auto-stacking, and change events.
    /// Strictly decoupled from UI views via C# Action events.
    /// </summary>
    public class InventorySystem : MonoBehaviour
    {
        [Header("Inventory capacity")]
        [Tooltip("Maximun number of distinct item slots")]
        [SerializeField] private int maxSlots = 20;

        [Header("Current Items")]
        [SerializeField] private List<ItemStack> slots = new List<ItemStack>();

        /// <summary>
        /// Event fired whenever items are added, removed, or stacked.
        /// UI Views subscribe to this event to re-render without polling in Update().
        /// </summary>
        public event Action OnInventoryChanged;

        public IReadOnlyList<ItemStack> Slots => slots;

        /// <summary>
        /// Adds an item into inventory. Automatically stacks into existing slots first.
        /// </summary>
        /// <returns>True if added successfully, False if inventory is completely full.</returns>
        /// 
        public bool AddItem(ItemDataSO item, int amount = 1)
        {
            if (item == null || amount <= 0) return false;
            int remaining = amount;
            //  1.  Try stacking into  existing non-full slots of the same item
            foreach (var slot in slots)
            {
                if (slot.itemData == item && slot.quantity < item.maxStack)
                {
                    int spaceInSlot = item.maxStack - slot.quantity;
                    int addCount = Mathf.Min(spaceInSlot, remaining);

                    slot.quantity += addCount;
                    remaining -= addCount;

                    if (remaining <= 0) break;
                }
            }

            //  2. If leftover ramins, create bew Skits (up to maxSlots)
            while (remaining > 0 && slots.Count < maxSlots)
            {
                int addCount = Mathf.Min(item.maxStack, remaining);
                slots.Add(new ItemStack(item, addCount));
                remaining -= addCount;
            }

            // Notify UI views that inventory has changed
            OnInventoryChanged?.Invoke();

            bool success = (remaining == 0);

            if (!success)
            {
                Debug.LogWarning($"<color=orange>[Inventory]</color> Inventory full! Could not store {remaining}x {item.itemName}.");
            }

            return success;
        }

        /// <summary>
        /// Queries the total count of an item across all slots (useful for Crafting recipes).
        /// </summary>
        public int GetItemCount(ItemDataSO item)
        {
            if (item == null) return 0;
            int total = 0;
            foreach (var slot in slots)
            {
                if (slot.itemData == item) total += slot.quantity;
            }
            return total;
        }

        /// <summary>
        /// Queries the total count of an item by its unique itemId string.
        /// </summary>
        public int GetItemCount(string itemId)
        {
            int total = 0;
            foreach (var slot in slots)
            {
                if (slot.itemData != null && slot.itemData.itemId == itemId)
                {
                    total += slot.quantity;
                }
            }
            return total;
        }
    }
}