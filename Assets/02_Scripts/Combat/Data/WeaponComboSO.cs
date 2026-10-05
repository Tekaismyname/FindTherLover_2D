using UnityEngine;
using System.Collections.Generic;

namespace FindTheLover.Combat.Data
{

    /// <summary>
    /// ScriptableObject holding a sequence of attacks forming a complete weapon combo.
    /// Can represent Unarmed, Wooden Stick, Iron Sword, etc.
    /// </summary>
    [CreateAssetMenu(fileName = "New_WeaponCombo", menuName = "FindTheLover/Combat/Weapon Combo")]
    public class WeaponComboSO : ScriptableObject
    {
        [Header("Weapon  Identity")]
        public string weaponName = "Unarmed  Hands";

        [Header("Combo Chain")]
        [Tooltip("List of attacks executed in sequential order")]
        public List<AttackDataSO> attackChain = new List<AttackDataSO>();

        /// <summary>
        /// Returns the attack data for the given step index (0-indexed).
        /// </summary>
        public AttackDataSO GetAttack(int index)
        {
            if (attackChain == null || attackChain.Count == 0) return null;
            if (index < 0 || index >= attackChain.Count) return null;
            return attackChain[index];
        }

        public int TotalAttacks => attackChain != null ? attackChain.Count : 0;
    }
}