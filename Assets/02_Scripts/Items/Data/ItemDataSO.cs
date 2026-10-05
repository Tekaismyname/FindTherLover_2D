using UnityEngine;

namespace FindTheLover.Items
{
    public enum ItemType
    {
        Resource,
        Tool,
        Weapon,
        Consumanle,
        Armor
    }
    /// <summary>
    /// Data Layer Blueprint for inventory items and world drops.
    /// Follows Flyweight Pattern: A single shared asset instance in memory.
    /// </summary>
    [CreateAssetMenu(fileName = "NewItemData", menuName = "FindTheLover/Items/ItemDataSO")]
    public class ItemDataSO : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Unique string identifier for saving and networking")]
        public string itemId = "wood_log";

        [Tooltip("Display name shown on UI")]
        public string itemName = "Wood Log";

        [Tooltip("Classification type of the item")]
        public ItemType itemType = ItemType.Resource;

        [Header("Display")]
        [Tooltip("2d Sprite icon used in Inventory and HUD")]
        public Sprite icon;

        [TextArea(2, 4)]
        [Tooltip("Flavour text and description of the item")]
        public string description = "A sturdy piece of wood gathered from felled trees";

        [Header("Stracking &  Capacity")]
        [Tooltip("maximun quantity allowed per inventory slot.")]
        [Range(1, 999)] public int maxStack = 99;

        [Tooltip("World drop prefab instantiated when dropped on ground")]
        public GameObject worldPrefab;
    }
}