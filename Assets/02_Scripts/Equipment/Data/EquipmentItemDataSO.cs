using UnityEngine;

namespace FindTheLover.Equipment.Data
{
    public enum EquipmentType
    {
        None,
        Torch,
        Axe,
        Sword
    }

    /// <summary>
    /// Data Layer: Flyweight blueprint holding static parameters and visual prefab for an equipable tool/weapon.
    /// </summary>
    [CreateAssetMenu(fileName = "NewEquipmentItem", menuName = "FindTheLover/Equipment/Equipment Item")]
    public class EquipmentItemDataSO : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Unique technical identifier for the equipment")]
        public string itemId = "item_torch";


        [Tooltip("In-game display name")]
        public string displayName = "Handheld torch";

        [Tooltip("Functional  category of his equipment")]
        public EquipmentType equipmentType = EquipmentType.None;

        [Header("Presentation")]
        [Tooltip("Visual prefabs spawned into the Player's Hand_socker")]
        public GameObject heldPrefab;

        [Tooltip("Local position offset relative to Hand_socker")]
        public Vector3 localOffset = Vector3.zero;

        [Tooltip("Local euler rotation relative to Hand_Socket")]
        public Vector3 localRotation = Vector3.zero;
    }
    
    
}