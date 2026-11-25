using System.Collections.Generic;
using UnityEngine;
using FindTheLover.Augments.Data;
using FindTheLover.Combat.Data;
using FindTheLover.Enemies.Data;
using FindTheLover.Equipment.Data;
using FindTheLover.Items;

namespace FindTheLover.Core.Data
{
    /// <summary>
    /// Data Layer (ScriptableObject): Central Master Catalog indexing every static game blueprint
    /// (Items, Equipment, TFT Augments, Enemies, Resources) by stable string ID for O(1) Save/Load resolution.
    /// Follows Flyweight Pattern and protects Standalone builds from asset path fragility.
    /// </summary>
    [CreateAssetMenu(fileName = "Master_GameCatalog", menuName = "Find The Lover/Core/Master Game Catalog")]
    public class GameCatalogSO : ScriptableObject
    {
        [Header("Inventory Items")]
        public List<ItemDataSO> items = new List<ItemDataSO>();

        [Header("Equippable Gear & Tools")]
        public List<EquipmentItemDataSO> equipmentItems = new List<EquipmentItemDataSO>();

        [Header("TFT 3-Tier Augments")]
        public List<AugmentDataSO> augments = new List<AugmentDataSO>();

        [Header("Enemy Archetypes")]
        public List<EnemyDataSO> enemies = new List<EnemyDataSO>();

        [Header("Harvestable Resource Nodes")]
        public List<ResourceDataSO> resources = new List<ResourceDataSO>();

        // Lazily built O(1) runtime lookup maps (never serialized)
        private Dictionary<string, ItemDataSO> _itemLookup;
        private Dictionary<string, EquipmentItemDataSO> _equipmentLookup;
        private Dictionary<string, AugmentDataSO> _augmentLookup;

        /// <summary>
        /// Forces lookup dictionaries to rebuild (call after runtime catalog injection in tests).
        /// </summary>
        public void RebuildLookups()
        {
            _itemLookup = new Dictionary<string, ItemDataSO>();
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i] != null && !string.IsNullOrEmpty(items[i].itemId))
                {
                    _itemLookup[items[i].itemId] = items[i];
                }
            }

            _equipmentLookup = new Dictionary<string, EquipmentItemDataSO>();
            for (int i = 0; i < equipmentItems.Count; i++)
            {
                if (equipmentItems[i] != null && !string.IsNullOrEmpty(equipmentItems[i].itemId))
                {
                    _equipmentLookup[equipmentItems[i].itemId] = equipmentItems[i];
                }
            }

            _augmentLookup = new Dictionary<string, AugmentDataSO>();
            for (int i = 0; i < augments.Count; i++)
            {
                if (augments[i] != null && !string.IsNullOrEmpty(augments[i].augmentId))
                {
                    _augmentLookup[augments[i].augmentId] = augments[i];
                }
            }
        }

        public ItemDataSO GetItemById(string itemId)
        {
            if (string.IsNullOrEmpty(itemId)) return null;
            if (_itemLookup == null) RebuildLookups();
            return _itemLookup.TryGetValue(itemId, out var result) ? result : null;
        }

        public EquipmentItemDataSO GetEquipmentById(string equipmentId)
        {
            if (string.IsNullOrEmpty(equipmentId)) return null;
            if (_equipmentLookup == null) RebuildLookups();
            return _equipmentLookup.TryGetValue(equipmentId, out var result) ? result : null;
        }

        public AugmentDataSO GetAugmentById(string augmentId)
        {
            if (string.IsNullOrEmpty(augmentId)) return null;
            if (_augmentLookup == null) RebuildLookups();
            return _augmentLookup.TryGetValue(augmentId, out var result) ? result : null;
        }
    }
}
