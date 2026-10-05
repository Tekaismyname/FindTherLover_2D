using UnityEngine;

namespace FindTheLover.Combat
{

    /// <summary>
    /// Universal contract for any entity capable of receiving damage (Trees, Rocks, Enemies, Breakables).
    /// Adheres strictly to SOLID Interface Segregation Principle (ISP) and Dependency Inversion Principle (DIP).
    /// </summary>
    public interface IDamageable
    {
        /// <summary>
        /// Applies damage and directional impact to the entity.
        /// </summary>
        /// <param name="damage">Amount of health deducted.</param>
        /// <param name="hitPoint">World position of the impact point.</param>
        /// <param name="hitNormal">Surface normal vector of the strike.</param>

        void TakeDamage(float damage, Vector3 hitPoint, Vector3 hitNormal);
        
        //current health point of the entity
        float CurrentHealth { get; }

        // Indicates whether the entit been destroyed or killed
        bool IsDead { get; }
    }
}