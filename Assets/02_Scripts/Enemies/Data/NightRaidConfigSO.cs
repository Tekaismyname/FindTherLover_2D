using System;
using System.Collections.Generic;
using UnityEngine;

namespace FindTheLover.Enemies.Data
{
    /// <summary>
    /// Serializable roster entry mapping an enemy prefab to its spawn budget cost and progression threshold.
    /// </summary>
    [Serializable]
    public struct NightEnemyRosterEntry
    {
        [Tooltip("Display label for designer clarity in the Inspector.")]
        public string label;

        [Tooltip("Enemy prefab implementing EnemyAI (e.g., Goblin, Skeleton, Scarab, SporeShroom, ShadowBat).")]
        public EnemyAI prefab;

        [Tooltip("Budget points consumed when deploying one instance of this enemy.")]
        [Min(1)] public int budgetCost;

        [Tooltip("Earliest survival day when this enemy archetype can join night raids.")]
        [Min(1)] public int minDayToAppear;

        [Tooltip("Relative selection weight during weighted random roster rolls.")]
        [Range(0.1f, 10f)] public float spawnWeight;
    }

    /// <summary>
    /// Data Layer (ScriptableObject): Configures Muck-like escalating night raids, 20-day survival goal,
    /// annular spawn ring bounds, calamity boss nights (Days 5, 10, 15, 20), and multi-archetype roster.
    /// </summary>
    [CreateAssetMenu(fileName = "NightRaid_Config", menuName = "Find The Lover/Enemies/Night Raid Config")]
    public class NightRaidConfigSO : ScriptableObject
    {
        [Header("Survival Marathon Settings")]
        [Tooltip("Total days players must survive to complete the survival marathon goal.")]
        [Range(5, 50)] public int targetSurvivalDays = 20;

        [Tooltip("Specific milestone nights when Calamity Boss raids trigger.")]
        public int[] calamityBossDays = new int[] { 5, 10, 15, 20 };

        [Header("Escalating Night Budget Formula")]
        [Tooltip("Base spawn budget on Night 1 (Formula: Base + (Day - 1) * Multiplier).")]
        [Min(1)] public int baseNightBudget = 10;

        [Tooltip("Additional spawn budget granted for each subsequent survival day.")]
        [Min(1)] public int budgetPerDayMultiplier = 6;

        [Tooltip("Extra budget multiplier applied on Calamity Boss Nights (Days 5, 10, 15, 20).")]
        [Range(1f, 3f)] public float calamityBudgetMultiplier = 1.6f;

        [Header("Daily Enemy Stat Scaling")]
        [Tooltip("Health percentage increase per day after Day 1 (e.g., 0.12 = +12% HP per day).")]
        [Range(0f, 0.5f)] public float dailyHealthScalePercent = 0.12f;

        [Tooltip("Damage percentage increase per day after Day 1 (e.g., 0.08 = +8% DMG per day).")]
        [Range(0f, 0.5f)] public float dailyDamageScalePercent = 0.08f;

        [Tooltip("Probability (0..1) that a spawned night raider is upgraded to an Elite raider.")]
        [Range(0f, 1f)] public float eliteSpawnChance = 0.2f;

        [Header("Wave Timing & Capacity")]
        [Tooltip("Seconds between consecutive wave spawn ticks during nighttime.")]
        [Range(1f, 15f)] public float spawnIntervalSeconds = 4.0f;

        [Tooltip("Maximum active night raiders allowed simultaneously to maintain smooth 60 FPS.")]
        [Range(2, 30)] public int maxSimultaneousRaiders = 14;

        [Header("Annular Spawn Ring (Outside Camera View)")]
        [Tooltip("Inner radius from player/camp anchor (prevents monsters popping directly on screen).")]
        [Range(10f, 40f)] public float minSpawnRadius = 25f;

        [Tooltip("Outer radius from player/camp anchor (keeps raiders within active island bounds).")]
        [Range(20f, 60f)] public float maxSpawnRadius = 35f;

        [Tooltip("Maximum search radius when snapping calculated coordinates onto baked NavMesh.")]
        [Range(2f, 15f)] public float navMeshSnapDistance = 8f;

        [Header("Enemy Archetype Roster")]
        [Tooltip("Multi-archetype monster roster unlocked progressively across survival days.")]
        public List<NightEnemyRosterEntry> enemyRoster = new List<NightEnemyRosterEntry>();

        [Header("Atmospheric Audio Cues")]
        [Tooltip("Warning bell sound played when dusk falls and a Night Raid begins.")]
        public AudioClip duskAlarmClip;

        [Tooltip("Intense horn/gong sound played on Calamity Boss Nights (Days 5, 10, 15, 20).")]
        public AudioClip calamityWarningClip;

        [Tooltip("Relief chime played when dawn breaks and players survive the night.")]
        public AudioClip dawnReliefClip;
    }
}
