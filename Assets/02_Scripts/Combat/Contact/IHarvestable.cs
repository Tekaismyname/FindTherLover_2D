using UnityEngine;

namespace FindTheLover.Combat
{
    public enum HarvestToolType
    {
        None,
        Axe,
        Pickaxe,
        Sickle
    }

    /// <summary>
    /// Contract for environmental resources that require specific harvest tools
    /// and reward players with raw crafting materials upon destruction.
    /// Strictly adheres to SOLID Interface Segregation Principle (ISP).
    /// </summary>
    public interface IHarvestable : IDamageable
    {
        // The optimal tool type required to efficiently harvest this node.
        HarvestToolType RequiredTool { get; }

        // Multiplier apllied to damage when struch by the correct tool.
        float ToolEfficiencyMultiplier { get; }

        //  Triggers resource harvesting with tool verification.
        void Harvest(float baseDamage, HarvestToolType usedTool, Vector3 hitPoint);
    }
    
}