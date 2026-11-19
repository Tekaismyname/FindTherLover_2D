using System;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.InputSystem;
using FindTheLover.Environment.Data;

namespace FindTheLover.Environment
{
    /// <summary>
    /// Logic Layer: Muck-like Deterministic Seeded Terrain Generator managing both Early-Game
    /// (gentle coastal meadow) and End-Game (ridged calamity island + boss caldera) terrains.
    /// Uses System.Random(worldSeed) so Multiplayer Host & Client reproduce identical terrain from a single seed integer.
    /// </summary>
    public class SeededWorldTerrainManager : MonoBehaviour
    {
        public static SeededWorldTerrainManager Instance { get; private set; }

        [Header("World Seed Settings")]
        [Tooltip("Deterministic world seed shared between co-op players (identical seed = identical islands).")]
        [SerializeField] private int _worldSeed = 20261119;

        [Tooltip("If true, generates a fresh random seed automatically when entering Play Mode.")]
        [SerializeField] private bool _randomizeSeedOnStart = false;

        [Tooltip("If true, regenerates terrain heights & biomes on Start using the active worldSeed.")]
        [SerializeField] private bool _generateOnStart = false;

        [Tooltip("QA Testing hotkey: press R in Play Mode to roll a new random seed live.")]
        [SerializeField] private bool _enableRuntimeRandomizeHotkey = true;

        [Header("Early-Game Terrain (Island 1 - Starter Sanctuary)")]
        [SerializeField] private Terrain _earlyGameTerrain;
        [SerializeField] private SeededTerrainProfileSO _earlyGameProfile;

        [Header("End-Game Terrain (Island 2 - Toxic Marsh & Volcanic Boss Caldera)")]
        [SerializeField] private Terrain _endGameTerrain;
        [SerializeField] private SeededTerrainProfileSO _endGameProfile;

        [Header("Optional Post-Generation Synchronization")]
        [Tooltip("Parent transforms whose child trees/rocks should snap vertically onto the regenerated terrain.")]
        [SerializeField] private Transform[] _propParentsToReground;

        [Tooltip("Optional NavMeshSurface rebuilt automatically after terrain regeneration.")]
        [SerializeField] private NavMeshSurface _navMeshSurface;

        /// <summary>Fired whenever the archipelago finishes regenerating for a new world seed.</summary>
        public event Action<int> OnWorldSeedGenerated;

        public int WorldSeed => _worldSeed;
        public SeededTerrainProfileSO EarlyGameProfile => _earlyGameProfile;
        public SeededTerrainProfileSO EndGameProfile => _endGameProfile;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            if (_generateOnStart)
            {
                int seedToUse = _randomizeSeedOnStart
                    ? System.Environment.TickCount ^ UnityEngine.Random.Range(1000, 999999)
                    : _worldSeed;
                GenerateWorldWithSeed(seedToUse);
            }
        }

        private void Update()
        {
            if (_enableRuntimeRandomizeHotkey && Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
            {
                RegenerateWithRandomSeed();
            }
        }

        /// <summary>
        /// Generates a fresh random integer seed and immediately rebuilds all registered terrains.
        /// </summary>
        public int RegenerateWithRandomSeed()
        {
            int newSeed = unchecked(System.Environment.TickCount * 397 ^ UnityEngine.Random.Range(1, int.MaxValue));
            newSeed = Mathf.Abs(newSeed % 9999999) + 1;
            GenerateWorldWithSeed(newSeed);
            return newSeed;
        }

        /// <summary>
        /// Deterministically sculpts heights, paints biome splatmaps, clears grass on roads/cliffs,
        /// re-grounds environment props, and rebakes NavMesh for the given world seed.
        /// </summary>
        public void GenerateWorldWithSeed(int seed)
        {
            _worldSeed = seed;

            if (_earlyGameTerrain != null && _earlyGameProfile != null)
            {
                ApplySeedToTerrain(_earlyGameTerrain, _earlyGameProfile, seed);
            }

            if (_endGameTerrain != null && _endGameProfile != null)
            {
                // Derive deterministic sub-seed for End-Game island so both islands stay unique yet reproducible
                int endGameSeed = unchecked(seed * 73856093 ^ 19349663);
                ApplySeedToTerrain(_endGameTerrain, _endGameProfile, endGameSeed);
            }

            RegroundSceneProps();
            RebuildNavMeshIfPresent();

            Debug.Log($"<color=#00FFCC><b>[SeededWorldTerrainManager] Generated Early-Game & End-Game Terrains with Seed: {_worldSeed}</b></color>");
            OnWorldSeedGenerated?.Invoke(_worldSeed);
        }

        /// <summary>
        /// Derives deterministic Perlin noise offsets strictly from System.Random(seed) for Co-op determinism.
        /// </summary>
        public static Vector2 GenerateSeedOffset(int seed, int salt = 0)
        {
            System.Random prng = new System.Random(unchecked(seed + salt * 48611));
            float ox = (float)(prng.NextDouble() * 10000.0 - 5000.0);
            float oz = (float)(prng.NextDouble() * 10000.0 - 5000.0);
            return new Vector2(ox, oz);
        }

        /// <summary>
        /// Pure deterministic height evaluator for any world XZ coordinate given a profile and seed.
        /// Distinguishes smooth Early-Game Meadow FBM from sharp End-Game Ridged Multifractal peaks
        /// while guaranteeing 100% safe elevation on all protected anchors and corridors.
        /// </summary>
        public static float EvaluateWorldHeight(float worldX, float worldZ, SeededTerrainProfileSO profile, Vector2 primaryOffset, Vector2 coastOffset)
        {
            if (profile == null) return 8.0f;

            Vector2 pos = new Vector2(worldX, worldZ);
            Vector2 delta = pos - profile.islandCenterXZ;
            float dist = delta.magnitude;
            float angle = Mathf.Atan2(delta.y, delta.x);

            // 1. Seeded organic coastline wobble using polar angle sampling
            float coastNoise = Mathf.PerlinNoise(
                Mathf.Cos(angle) * 1.4f + coastOffset.x * 0.01f + 50f,
                Mathf.Sin(angle) * 1.4f + coastOffset.y * 0.01f + 50f
            );
            float effectiveRadius = profile.islandBaseRadius + (coastNoise - 0.5f) * 2f * profile.coastWobbleAmplitude;
            float normDist = dist / Mathf.Max(1f, effectiveRadius);

            // 2. Interior procedural relief: Smooth FBM (Early-Game) vs Ridged Multifractal (End-Game)
            float fbm = SampleFractalNoise(worldX, worldZ, profile.noiseFrequency, profile.noiseOctaves, primaryOffset,
                ridged: profile.archetype == TerrainZoneArchetype.EndGameCalamity,
                ridgeSharpness: profile.ridgeSharpness);

            float interiorOffset = (profile.archetype == TerrainZoneArchetype.EndGameCalamity)
                ? fbm * profile.interiorReliefAmplitude
                : (fbm - 0.45f) * 2f * profile.interiorReliefAmplitude;

            // 3. Base island shelf + perimeter mountain rim + coastal dropoff into ocean
            float height = profile.baseGroundHeight + interiorOffset;

            if (normDist > profile.rimStartNormalizedRadius && normDist < 0.94f)
            {
                // Bell-shaped perimeter mountain ring whose peaks vary with world seed
                float rimT = (normDist - profile.rimStartNormalizedRadius) / (0.94f - profile.rimStartNormalizedRadius);
                float rimBell = Mathf.Sin(Mathf.Clamp01(rimT) * Mathf.PI);
                float peakMod = 0.65f + 0.70f * fbm;
                height += rimBell * profile.perimeterRimHeight * peakMod;
            }
            else if (normDist >= 0.94f)
            {
                // Drop smoothly beneath ocean sea level to form natural island shoreline
                float seaT = Mathf.Clamp01((normDist - 0.94f) / 0.14f);
                height = Mathf.Lerp(profile.baseGroundHeight - 1.2f, 0.2f, Mathf.SmoothStep(0f, 1f, seaT));
            }

            // 4. Enforce protected circular anchors (Campfire, Intro Beach, Lakes, Soul Sanctuary, Boss Caldera)
            if (profile.protectedAnchors != null)
            {
                for (int i = 0; i < profile.protectedAnchors.Count; i++)
                {
                    TerrainAnchorZone anchor = profile.protectedAnchors[i];
                    float d = Vector2.Distance(pos, anchor.worldCenterXZ);
                    float maxInfluence = anchor.flatRadius + anchor.blendWidth;
                    if (d < maxInfluence)
                    {
                        float t = Mathf.Clamp01((d - anchor.flatRadius) / Mathf.Max(0.01f, anchor.blendWidth));
                        float weight = 1f - Mathf.SmoothStep(0f, 1f, t);
                        height = Mathf.Lerp(height, anchor.targetWorldHeight, weight);
                    }
                }
            }

            // 5. Enforce protected corridors (Eastern Bridge Pass, Swamp Trails, Thorn Gate Gorge)
            if (profile.protectedCorridors != null)
            {
                for (int i = 0; i < profile.protectedCorridors.Count; i++)
                {
                    TerrainCorridorAnchor corridor = profile.protectedCorridors[i];
                    float d = DistanceToCurvedCorridor(pos, corridor, out _);
                    float maxInfluence = corridor.halfWidth + corridor.blendWidth;
                    if (d < maxInfluence)
                    {
                        float t = Mathf.Clamp01((d - corridor.halfWidth) / Mathf.Max(0.01f, corridor.blendWidth));
                        float weight = 1f - Mathf.SmoothStep(0f, 1f, t);
                        height = Mathf.Lerp(height, corridor.targetWorldHeight, weight);
                    }
                }
            }

            return Mathf.Max(0f, height);
        }

        private static float SampleFractalNoise(float x, float z, float baseFreq, int octaves, Vector2 offset, bool ridged, float ridgeSharpness)
        {
            float sum = 0f;
            float amp = 1f;
            float freq = baseFreq;
            float maxAmp = 0f;

            for (int o = 0; o < octaves; o++)
            {
                float n = Mathf.PerlinNoise(x * freq + offset.x + o * 131.7f, z * freq + offset.y + o * 277.3f);
                if (ridged)
                {
                    float ridge = 1f - Mathf.Abs(n * 2f - 1f);
                    n = Mathf.Pow(Mathf.Clamp01(ridge), ridgeSharpness);
                }
                sum += n * amp;
                maxAmp += amp;
                amp *= 0.5f;
                freq *= 2.05f;
            }

            return maxAmp > 0f ? sum / maxAmp : 0f;
        }

        private static float DistanceToCurvedCorridor(Vector2 point, TerrainCorridorAnchor c, out float alongT)
        {
            Vector2 ab = c.endXZ - c.startXZ;
            float lenSq = ab.sqrMagnitude;
            if (lenSq < 0.001f)
            {
                alongT = 0f;
                return Vector2.Distance(point, c.startXZ);
            }

            alongT = Mathf.Clamp01(Vector2.Dot(point - c.startXZ, ab) / lenSq);
            Vector2 closest = c.startXZ + ab * alongT;

            if (Mathf.Abs(c.curveAmplitude) > 0.01f)
            {
                Vector2 perp = new Vector2(-ab.y, ab.x).normalized;
                float wave = Mathf.Sin(alongT * Mathf.PI * 2f) * c.curveAmplitude * Mathf.Sin(alongT * Mathf.PI);
                closest += perp * wave;
            }

            return Vector2.Distance(point, closest);
        }

        private void ApplySeedToTerrain(Terrain terrain, SeededTerrainProfileSO profile, int terrainSeed)
        {
            TerrainData td = terrain.terrainData;
            if (td == null) return;

            Vector3 tPos = terrain.transform.position;
            Vector3 tSize = td.size;

            Vector2 primaryOffset = GenerateSeedOffset(terrainSeed, 1);
            Vector2 coastOffset = GenerateSeedOffset(terrainSeed, 2);

            // 1. Sculpt Heightmap
            int hRes = td.heightmapResolution;
            float[,] heights = new float[hRes, hRes];
            for (int y = 0; y < hRes; y++)
            {
                float normZ = (float)y / (hRes - 1);
                float worldZ = tPos.z + normZ * tSize.z;

                for (int x = 0; x < hRes; x++)
                {
                    float normX = (float)x / (hRes - 1);
                    float worldX = tPos.x + normX * tSize.x;

                    float worldH = EvaluateWorldHeight(worldX, worldZ, profile, primaryOffset, coastOffset);
                    heights[y, x] = Mathf.Clamp01(worldH / tSize.y);
                }
            }
            td.SetHeights(0, 0, heights);

            // 2. Repaint 4-Channel Biome Splatmap ([0] Grass, [1] Rock, [2] DirtPath, [3] Sand)
            int aW = td.alphamapWidth;
            int aH = td.alphamapHeight;
            int layers = td.alphamapLayers;
            if (layers >= 4)
            {
                float[,,] alphas = new float[aH, aW, layers];
                for (int y = 0; y < aH; y++)
                {
                    float normZ = (float)y / (aH - 1);
                    float worldZ = tPos.z + normZ * tSize.z;

                    for (int x = 0; x < aW; x++)
                    {
                        float normX = (float)x / (aW - 1);
                        float worldX = tPos.x + normX * tSize.x;

                        float h = td.GetInterpolatedHeight(normX, normZ);
                        float steep = td.GetSteepness(normX, normZ);
                        float microNoise = Mathf.PerlinNoise(worldX * 0.06f + primaryOffset.x, worldZ * 0.06f + primaryOffset.y);

                        float wGrass = 0f;
                        float wRock = 0f;
                        float wPath = 0f;
                        float wSand = 0f;

                        // Coastline & shallow basins -> Sand (Channel 3)
                        if (h < profile.sandMaxHeight)
                        {
                            wSand = 1f;
                        }
                        // Steep slopes, ridges, or high elevation -> Rock (Channel 1)
                        else if (steep > profile.rockMinSlope || h > profile.rockStartHeight)
                        {
                            wRock = Mathf.Clamp01((steep - (profile.rockMinSlope - 4f)) / 12f + (h - (profile.rockStartHeight - 1.5f)) / 10f);
                            wGrass = 1f - wRock;
                        }
                        // Playable meadows / swamp floor -> Grass + subtle biome rock ratio
                        else
                        {
                            float rockBias = Mathf.Lerp(profile.flatGroundRockRatio * 0.5f, profile.flatGroundRockRatio * 1.4f, microNoise);
                            wRock = Mathf.Clamp01(rockBias);
                            wGrass = 1f - wRock;
                        }

                        // Winding Dirt Pathways along all protected corridors (Channel 2)
                        Vector2 pWobble = new Vector2(worldX + (microNoise - 0.5f) * 1.8f, worldZ + (microNoise - 0.5f) * 1.8f);
                        float distPath = GetMinDistanceToCorridors(pWobble, profile);
                        if (distPath < profile.pathwayHalfWidth && h >= profile.sandMaxHeight - 0.2f && h <= profile.rockStartHeight)
                        {
                            float pFactor = Mathf.SmoothStep(1f, 0f, distPath / profile.pathwayHalfWidth);
                            wPath = pFactor * 0.92f;
                            if (distPath > profile.pathwayHalfWidth * 0.62f)
                            {
                                wRock = Mathf.Max(wRock, 0.35f * pFactor);
                            }
                            wGrass *= (1f - pFactor * 0.88f);
                        }

                        // Volcanic Boss Caldera rocky stone ring accent on End-Game island
                        if (profile.archetype == TerrainZoneArchetype.EndGameCalamity && profile.protectedAnchors != null)
                        {
                            for (int i = 0; i < profile.protectedAnchors.Count; i++)
                            {
                                if (profile.protectedAnchors[i].label.Contains("Caldera"))
                                {
                                    float dCaldera = Vector2.Distance(pWobble, profile.protectedAnchors[i].worldCenterXZ);
                                    if (dCaldera < profile.protectedAnchors[i].flatRadius)
                                    {
                                        float ring = Mathf.SmoothStep(0.35f, 0.95f, dCaldera / profile.protectedAnchors[i].flatRadius);
                                        wRock = Mathf.Max(wRock, ring);
                                        wPath = Mathf.Max(wPath, (1f - ring) * 0.65f);
                                        wGrass *= 0.2f;
                                    }
                                }
                            }
                        }

                        float total = wGrass + wRock + wPath + wSand;
                        if (total < 0.001f) { wGrass = 1f; total = 1f; }

                        alphas[y, x, 0] = wGrass / total;
                        alphas[y, x, 1] = wRock / total;
                        alphas[y, x, 2] = wPath / total;
                        alphas[y, x, 3] = wSand / total;
                    }
                }
                td.SetAlphamaps(0, 0, alphas);
            }

            // 3. Clear 2.5D Grass Detail tufts on roads, steep mountain ridges, and underwater beds
            ClearDetailsOnRoadsAndCliffs(td, tPos, tSize, profile);
        }

        private static float GetMinDistanceToCorridors(Vector2 point, SeededTerrainProfileSO profile)
        {
            float minDist = float.MaxValue;
            if (profile.protectedCorridors != null)
            {
                for (int i = 0; i < profile.protectedCorridors.Count; i++)
                {
                    float d = DistanceToCurvedCorridor(point, profile.protectedCorridors[i], out _);
                    if (d < minDist) minDist = d;
                }
            }
            return minDist;
        }

        private static void ClearDetailsOnRoadsAndCliffs(TerrainData td, Vector3 tPos, Vector3 tSize, SeededTerrainProfileSO profile)
        {
            int dRes = td.detailResolution;
            int dCount = td.detailPrototypes != null ? td.detailPrototypes.Length : 0;
            if (dRes <= 0 || dCount <= 0) return;

            for (int layer = 0; layer < dCount; layer++)
            {
                int[,] map = td.GetDetailLayer(0, 0, dRes, dRes, layer);
                for (int dy = 0; dy < dRes; dy++)
                {
                    float normZ = (float)dy / (dRes - 1);
                    float worldZ = tPos.z + normZ * tSize.z;

                    for (int dx = 0; dx < dRes; dx++)
                    {
                        if (map[dy, dx] == 0) continue;
                        float normX = (float)dx / (dRes - 1);
                        float worldX = tPos.x + normX * tSize.x;

                        float h = td.GetInterpolatedHeight(normX, normZ);
                        float steep = td.GetSteepness(normX, normZ);
                        if (steep > profile.rockMinSlope + 4f || h < profile.sandMaxHeight || h > profile.rockStartHeight ||
                            GetMinDistanceToCorridors(new Vector2(worldX, worldZ), profile) < profile.pathwayHalfWidth + 0.5f)
                        {
                            map[dy, dx] = 0;
                        }
                    }
                }
                td.SetDetailLayer(0, 0, layer, map);
            }
        }

        private void RegroundSceneProps()
        {
            if (_propParentsToReground == null || _propParentsToReground.Length == 0) return;

            for (int p = 0; p < _propParentsToReground.Length; p++)
            {
                Transform parent = _propParentsToReground[p];
                if (parent == null) continue;

                for (int i = 0; i < parent.childCount; i++)
                {
                    Transform child = parent.GetChild(i);
                    Vector3 pos = child.position;
                    Terrain owner = FindTerrainUnderPoint(pos);
                    if (owner != null)
                    {
                        pos.y = owner.SampleHeight(pos) + owner.transform.position.y;
                        child.position = pos;
                    }
                }
            }
        }

        private Terrain FindTerrainUnderPoint(Vector3 worldPos)
        {
            if (IsPointInsideTerrain(_earlyGameTerrain, worldPos)) return _earlyGameTerrain;
            if (IsPointInsideTerrain(_endGameTerrain, worldPos)) return _endGameTerrain;
            return null;
        }

        private static bool IsPointInsideTerrain(Terrain t, Vector3 pos)
        {
            if (t == null || t.terrainData == null) return false;
            Vector3 tp = t.transform.position;
            Vector3 ts = t.terrainData.size;
            return pos.x >= tp.x && pos.x <= tp.x + ts.x && pos.z >= tp.z && pos.z <= tp.z + ts.z;
        }

        private void RebuildNavMeshIfPresent()
        {
            if (_navMeshSurface == null)
            {
                _navMeshSurface = FindAnyObjectByType<NavMeshSurface>();
            }
            if (_navMeshSurface != null)
            {
                _navMeshSurface.BuildNavMesh();
            }
        }
    }
}
