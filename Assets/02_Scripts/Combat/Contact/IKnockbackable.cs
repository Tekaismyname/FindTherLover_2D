using UnityEngine;

namespace FindTheLover.Combat
{
    /// <summary>
    /// Contract for any entity capable of receiving physical knockback displacement.
    /// Follows the Interface Segregation Principle (ISP).
    /// </summary>
    public interface Iknockbackable
    {
        /// <summary>
        /// Applies an impulse displacement force in a specified direction.
        /// </summary>
        /// <param name="direction">Normalized world direction vector of the knockback.</param>
        /// <param name="force">Force magnitude in meters per second.</param>

        void ApplyKnockBack(Vector3 direction, float force);
    }
}