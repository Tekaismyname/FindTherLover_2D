namespace FindTheLover.Augments.Data
{
    /// <summary>
    /// Rarity and power tier classification inspired by TFT Augments (Bronze / Silver / Gold).
    /// </summary>
    public enum AugmentTier
    {
        Bronze = 0, // Core stat enhancements (+Move Speed, +Max HP, Lifesteal, Harvest Yield)
        Silver = 1, // Tactical combat & movement modifiers (Chain Lightning, Blazing Dash, Shield)
        Gold = 2    // Game-changing legendary passives (Immortal Bond, Celestial Thunderstorm, Golden Touch)
    }

    /// <summary>
    /// Functional gameplay category used for HUD filtering and synergy grouping.
    /// </summary>
    public enum AugmentCategory
    {
        StatModifier = 0,
        OnHitEffect = 1,
        CompanionBond = 2,
        UltimatePassive = 3
    }

    /// <summary>
    /// Deterministic runtime behavior identifier executed by <see cref="FindTheLover.Augments.PlayerAugmentStats"/>.
    /// </summary>
    public enum AugmentEffectType
    {
        // Bronze Tier Effects (6)
        MoveSpeedPercent = 0,
        MaxHealthFlat = 1,
        LifestealPercent = 2,
        HarvestYieldMultiplier = 3,
        TorchDurationPercent = 4,
        DamageReductionPercent = 5,

        // Silver Tier Effects (6)
        ChainLightningOnHit = 10,
        BlazingDashTrail = 11,
        RegeneratingShield = 12,
        ToxicReboundNova = 13,
        PartnerDamageAura = 14,
        WindSlashFinisher = 15,

        // Gold Tier Effects (6)
        ImmortalLoverBond = 20,
        CelestialThunderstorm = 21,
        GoldenMidasTouch = 22,
        EternalRingProtection = 23,
        BerserkerFrenzy = 24,
        ArcaneOverdrive = 25
    }
}
