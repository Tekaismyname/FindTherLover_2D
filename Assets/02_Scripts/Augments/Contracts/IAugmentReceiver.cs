using FindTheLover.Augments.Data;

namespace FindTheLover.Augments.Contracts
{
    /// <summary>
    /// Dependency Inversion contract implemented by any character/entity capable of receiving TFT Augments.
    /// Keeps AugmentManager decoupled from concrete Player controllers.
    /// </summary>
    public interface IAugmentReceiver
    {
        /// <summary>
        /// Applies the effect of a newly chosen augment and updates character stats/passives.
        /// </summary>
        /// <param name="augment">The chosen ScriptableObject augment blueprint.</param>
        /// <param name="newStackCount">Total stacks owned for this augment after selection.</param>
        void ApplyAugment(AugmentDataSO augment, int newStackCount);
    }
}
