using UnityEngine;

namespace FindTheLover.Enemies
{
    /// <summary>
    /// Logic Layer: Concrete AI controller for the Zone 1 Forest Goblin archetype.
    /// Inherits core sensory detection, NavMesh pathfinding, Night Frenzy buffs,
    /// and melee engagement state machine from the abstract EnemyAI base class.
    /// </summary>
    public class GoblinEnemyAI : EnemyAI
    {
        // Forest Goblin uses the standard melee rusher behavior defined in EnemyAI.
        // Specific Goblin-only overrides (such as ambush leaps or loot drops) can be extended here.
    }
}
