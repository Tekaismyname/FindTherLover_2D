using System;
using System.Collections.Generic;
using UnityEngine;

namespace FindTheLover.Environment.Data
{
    /// <summary>
    /// High-level geomorphology archetype differentiating gentle early-game zones from harsh end-game zones.
    /// </summary>
    public enum TerrainZoneArchetype
    {
        EarlyGameMeadow = 0, // Soft multi-octave FBM hills, wide sandy beaches, lush meadows
        EndGameCalamity = 1  // Sharp Ridged Multifractal peaks, dark rocky ravines, volcanic boss caldera
    }

    /// <summary>
    /// Protected circular landmark zone guaranteed to stay flat at a target world elevation regardless of world seed.
    /// Prevents procedural mountains from burying Campfires, Bridges, Shrines, or Boss Arenas.
    /// </summary>
    [Serializable]
    public struct TerrainAnchorZone
    {
        [Tooltip("Human-readable label for this protected landmark (e.g., Campfire Sanctuary, Boss Caldera).")]
        public string label;

        [Tooltip("World-space XZ center coordinates of the protected zone.")]
        public Vector2 worldCenterXZ;

        [Tooltip("Inner radius guaranteed 100% flat at targetWorldHeight.")]
        [Min(1f)] public float flatRadius;

        [Tooltip("Outer blend band smoothly transitioning from flatRadius into procedural terrain.")]
        [Min(1f)] public float blendWidth;

        [Tooltip("Target world Y elevation inside the flat radius (default playable ground = 8.0m).")]
        public float targetWorldHeight;
    }

    /// <summary>
    /// Protected corridor connecting two world-space XZ points (e.g., bridge passes, gorge walkways).
    /// </summary>
    [Serializable]
    public struct TerrainCorridorAnchor
    {
        public string label;
        public Vector2 startXZ;
        public Vector2 endXZ;
        [Min(2f)] public float halfWidth;
        [Min(2f)] public float blendWidth;
        public float targetWorldHeight;
        [Tooltip("Optional sinusoidal wave amplitude to keep seeded roads organic.")]
        public float curveAmplitude;
    }

    /// <summary>
    /// Data Layer (ScriptableObject): Blueprint configuring seeded procedural terrain sculpting
    /// and biome splatmap painting for either Early-Game or End-Game islands.
    /// </summary>
    [CreateAssetMenu(fileName = "SeededTerrain_Profile", menuName = "Find The Lover/Environment/Seeded Terrain Profile")]
    public class SeededTerrainProfileSO : ScriptableObject
    {
        [Header("Zone Identity")]
        public string zoneName = "Early-Game Coastal Sanctuary";
        public TerrainZoneArchetype archetype = TerrainZoneArchetype.EarlyGameMeadow;

        [Header("Island Silhouette & Elevation")]
        [Tooltip("World-space XZ center of this island.")]
        public Vector2 islandCenterXZ = Vector2.zero;

        [Tooltip("Nominal walkable ground elevation in meters.")]
        public float baseGroundHeight = 8.0f;

        [Tooltip("Water sea level elevation in meters.")]
        public float seaLevelHeight = 5.5f;

        [Tooltip("Nominal island radius in meters before seeded coastal wobble.")]
        [Range(80f, 240f)] public float islandBaseRadius = 200f;

        [Tooltip("Organic coastline radius variation driven by world seed.")]
        [Range(0f, 40f)] public float coastWobbleAmplitude = 22f;

        [Tooltip("Outer perimeter mountain rim height above base ground.")]
        [Range(10f, 55f)] public float perimeterRimHeight = 26f;

        [Tooltip("Normalized radial start (0..1) where the outer perimeter rim rises.")]
        [Range(0.45f, 0.85f)] public float rimStartNormalizedRadius = 0.68f;

        [Header("Seeded Procedural Noise (FBM / Ridged)")]
        [Tooltip("Base frequency of interior procedural hills/ridges.")]
        [Range(0.005f, 0.05f)] public float noiseFrequency = 0.014f;

        [Tooltip("Interior hill or ridge elevation amplitude in meters.")]
        [Range(0f, 18f)] public float interiorReliefAmplitude = 4.5f;

        [Tooltip("Number of fractal noise octaves layered together.")]
        [Range(1, 5)] public int noiseOctaves = 3;

        [Tooltip("Ridge exponent applied when archetype is EndGameCalamity (higher = sharper volcanic peaks).")]
        [Range(1f, 3.5f)] public float ridgeSharpness = 2.0f;

        [Header("Biome Splatmap Thresholds (Channels: 0=Grass, 1=Rock, 2=DirtPath, 3=Sand)")]
        [Tooltip("World elevation below which beach sand dominates (Channel 3).")]
        public float sandMaxHeight = 6.9f;

        [Tooltip("Slope angle in degrees above which rock texture dominates (Channel 1).")]
        [Range(8f, 35f)] public float rockMinSlope = 18f;

        [Tooltip("Elevation above which rock texture begins blending in (Channel 1).")]
        public float rockStartHeight = 13.5f;

        [Tooltip("Base rock-to-grass tint mix on flat ground (0 = lush early meadow, 0.45 = harsh rocky end-game).")]
        [Range(0f, 0.65f)] public float flatGroundRockRatio = 0.08f;

        [Tooltip("Half-width of painted winding dirt pathways connecting corridors (Channel 2).")]
        [Range(2f, 8f)] public float pathwayHalfWidth = 4.0f;

        [Header("Protected Anchors (Never Buried By Seeded Noise)")]
        public List<TerrainAnchorZone> protectedAnchors = new List<TerrainAnchorZone>();
        public List<TerrainCorridorAnchor> protectedCorridors = new List<TerrainCorridorAnchor>();
    }
}
