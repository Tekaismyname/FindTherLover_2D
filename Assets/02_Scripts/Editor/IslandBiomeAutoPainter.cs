using System.IO;
using UnityEditor;
using UnityEngine;

namespace FindTheLover.Environment.Editor
{
    /// <summary>
    /// Production-grade Editor Tool to procedurally paint 4 rich biomes (Sand Coastline,
    /// Lush Forest, Dirt Paths, Rocky Chasm & Toxic Marsh) onto Unity Terrain alphamaps.
    /// Supports automatic layer loading, smooth Perlin noise edge blending, and full Undo.
    /// </summary>
    public class IslandBiomeAutoPainter : EditorWindow
    {
        [Header("Target")]
        [SerializeField] private Terrain targetTerrain;

        [Header("Terrain Layers (Assets/01_Data/Terrain)")]
        [SerializeField] private TerrainLayer sandLayer;
        [SerializeField] private TerrainLayer grassLayer;
        [SerializeField] private TerrainLayer dirtPathLayer;
        [SerializeField] private TerrainLayer rockLayer;

        [Header("Biome Configuration")]
        [Range(0.1f, 0.5f)] [SerializeField] private float coastalSandCutoff = 0.28f;
        [Range(0.4f, 0.8f)] [SerializeField] private float easternChasmStart = 0.62f;
        [Range(0.6f, 0.9f)] [SerializeField] private float northernMarshStart = 0.72f;
        [Range(1f, 8f)] [SerializeField] private float pathWidth = 3.5f;
        [Range(10f, 45f)] [SerializeField] private float cliffSlopeThreshold = 28f;
        [Range(4f, 25f)] [SerializeField] private float naturalNoiseFrequency = 14f;

        private Vector2 scrollPos;

        [MenuItem("Tools/Environment/🎨 Island 4-Biomes Auto Painter")]
        public static void OpenWindow()
        {
            var window = GetWindow<IslandBiomeAutoPainter>("Biome Painter");
            window.minSize = new Vector2(420, 560);
            window.Show();
        }

        private void OnEnable()
        {
            if (targetTerrain == null)
            {
                targetTerrain = Terrain.activeTerrain;
            }

            // Auto-load existing project terrain layers
            if (sandLayer == null)
                sandLayer = AssetDatabase.LoadAssetAtPath<TerrainLayer>("Assets/01_Data/Terrain/Layer_Sand.terrainlayer");
            if (grassLayer == null)
                grassLayer = AssetDatabase.LoadAssetAtPath<TerrainLayer>("Assets/01_Data/Terrain/Layer_Grass.terrainlayer");
            if (dirtPathLayer == null)
                dirtPathLayer = AssetDatabase.LoadAssetAtPath<TerrainLayer>("Assets/01_Data/Terrain/Layer_DirtPath.terrainlayer");
            if (rockLayer == null)
                rockLayer = AssetDatabase.LoadAssetAtPath<TerrainLayer>("Assets/01_Data/Terrain/Layer_Rock.terrainlayer");
        }

        private void OnGUI()
        {
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("🏝️ TỰ ĐỘNG PHÂN CHIA 4 VÙNG ĐẤT ĐẢO", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Công cụ này sẽ tự động đọc tọa độ địa hình và 'sơn' toàn bộ hòn đảo thành 4 vùng sinh thái:\n" +
                "1. Bãi Biển Cát Vàng (Phía Nam & Viền bờ biển)\n" +
                "2. Rừng Cây Rêu Phong (Trung tâm quanh Lửa Trại & Phía Tây)\n" +
                "3. Con Đường Mòn Đất (Nối Bờ Biển -> Lửa Trại -> Hẻm Vực)\n" +
                "4. Hẻm Vực Đá & Vách Dốc (Phía Đông & Rìa Hang Phía Bắc)",
                MessageType.Info
            );

            EditorGUILayout.Space(8);
            targetTerrain = (Terrain)EditorGUILayout.ObjectField("Target Terrain", targetTerrain, typeof(Terrain), true);

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("1. Cấu Hình 4 Lớp Đất (Terrain Layers)", EditorStyles.boldLabel);
            sandLayer = (TerrainLayer)EditorGUILayout.ObjectField("Layer 1: Sand (Cát)", sandLayer, typeof(TerrainLayer), false);
            grassLayer = (TerrainLayer)EditorGUILayout.ObjectField("Layer 2: Grass (Cỏ Xanh)", grassLayer, typeof(TerrainLayer), false);
            dirtPathLayer = (TerrainLayer)EditorGUILayout.ObjectField("Layer 3: Dirt Path (Đường Đất)", dirtPathLayer, typeof(TerrainLayer), false);
            rockLayer = (TerrainLayer)EditorGUILayout.ObjectField("Layer 4: Rock (Vách Đá)", rockLayer, typeof(TerrainLayer), false);

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("2. Tinh Chỉnh Phân Vùng Tự Nhiên", EditorStyles.boldLabel);
            coastalSandCutoff = EditorGUILayout.Slider("Vành Đai Cát Biển (Sand Area)", coastalSandCutoff, 0.1f, 0.5f);
            easternChasmStart = EditorGUILayout.Slider("Ranh Giới Hẻm Vực Đông (Chasm X)", easternChasmStart, 0.4f, 0.8f);
            northernMarshStart = EditorGUILayout.Slider("Ranh Giới Rìa Hang Bắc (Marsh Z)", northernMarshStart, 0.6f, 0.9f);
            pathWidth = EditorGUILayout.Slider("Bề Rộng Đường Mòn (Path Width m)", pathWidth, 1f, 8f);
            cliffSlopeThreshold = EditorGUILayout.Slider("Độ Dốc Hóa Đá (Cliff Slope Angle)", cliffSlopeThreshold, 10f, 45f);
            naturalNoiseFrequency = EditorGUILayout.Slider("Độ Uốn Lượn Tự Nhiên (Noise Scale)", naturalNoiseFrequency, 4f, 25f);

            EditorGUILayout.Space(16);
            GUI.backgroundColor = new Color(0.2f, 0.8f, 0.4f);
            if (GUILayout.Button("🎨 TỰ ĐỘNG SƠN 4 VÙNG ĐẤT NGAY (GENERATE BIOMES)", GUILayout.Height(42)))
            {
                PaintAllBiomes();
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.EndScrollView();
        }

        /// <summary>
        /// Computes and applies normalized multi-layered splatmap data across the entire terrain.
        /// </summary>
        private void PaintAllBiomes()
        {
            if (targetTerrain == null || targetTerrain.terrainData == null)
            {
                EditorUtility.DisplayDialog("Lỗi", "Vui lòng chọn Terrain trong Scene!", "OK");
                return;
            }

            var tData = targetTerrain.terrainData;
            Undo.RegisterCompleteObjectUndo(tData, "Auto Paint 4 Island Biomes");

            // Ensure Terrain has all 4 required layers assigned
            SetupTerrainLayers(tData);

            int alphaWidth = tData.alphamapWidth;
            int alphaHeight = tData.alphamapHeight;
            int numLayers = tData.alphamapLayers;

            if (numLayers < 4)
            {
                EditorUtility.DisplayDialog("Lỗi", "Terrain cần có ít nhất 4 Terrain Layers để sơn!", "OK");
                return;
            }

            float[,,] splatmapData = new float[alphaHeight, alphaWidth, numLayers];
            Vector3 terrainSize = tData.size;
            Vector3 terrainPos = targetTerrain.transform.position;

            EditorUtility.DisplayProgressBar("Sơn Địa Hình Đa Vùng Đất", "Đang tính toán phân bố Biome tự nhiên...", 0.3f);

            try
            {
                for (int y = 0; y < alphaHeight; y++)
                {
                    float normZ = (float)y / (alphaHeight - 1); // 0 (South) to 1 (North)

                    for (int x = 0; x < alphaWidth; x++)
                    {
                        float normX = (float)x / (alphaWidth - 1); // 0 (West) to 1 (East)

                        // World position coordinates
                        float worldX = terrainPos.x + normX * terrainSize.x;
                        float worldZ = terrainPos.z + normZ * terrainSize.z;

                        // Terrain physical metrics
                        float steepness = tData.GetSteepness(normX, normZ);
                        float height = tData.GetInterpolatedHeight(normX, normZ);

                        // Organic Perlin noise variation to eliminate harsh computer-generated grid lines
                        float noise = Mathf.PerlinNoise(normX * naturalNoiseFrequency, normZ * naturalNoiseFrequency);
                        float boundaryJitter = (noise - 0.5f) * 0.12f;

                        // Layer weights: [0] Sand, [1] Grass, [2] DirtPath, [3] Rock
                        float sandWeight = 0f;
                        float grassWeight = 0f;
                        float pathWeight = 0f;
                        float rockWeight = 0f;

                        // 1. CLIFF & ROCK DETECTION: Steep slopes automatically turn to rocky stone
                        if (steepness > cliffSlopeThreshold)
                        {
                            rockWeight = Mathf.Clamp01((steepness - cliffSlopeThreshold) / 10f);
                        }

                        // 2. BIOME DISTRIBUTION
                        // A. Southern Beach Coastline: High sand near south or low altitudes near water
                        if (normZ < (coastalSandCutoff + boundaryJitter) || height < 3.0f)
                        {
                            sandWeight = 1.0f;
                        }
                        // B. Eastern Chasm & Rocky Ravine: Exposed stone
                        else if (normX > (easternChasmStart + boundaryJitter))
                        {
                            rockWeight = Mathf.Max(rockWeight, 0.85f);
                            grassWeight = 0.15f;
                        }
                        // C. Northern Marsh: Toxic darkened soil & exposed rock
                        else if (normZ > (northernMarshStart + boundaryJitter))
                        {
                            rockWeight = Mathf.Max(rockWeight, 0.65f);
                            dirtPathWeight(ref pathWeight, worldX, worldZ, terrainSize, terrainPos);
                            grassWeight = 0.25f;
                        }
                        // D. Central & Western Lush Forest: Verdant grass sanctuary
                        else
                        {
                            grassWeight = 1.0f;
                        }

                        // 3. DIRT ROAD PATHWAY: Connecting South Beach -> Central Campfire -> East Chasm & North Marsh
                        float distToPaths = CalculateDistanceToPathways(normX, normZ);
                        float pathRadiusNorm = (pathWidth / terrainSize.x);
                        if (distToPaths < pathRadiusNorm)
                        {
                            float factor = 1.0f - (distToPaths / pathRadiusNorm);
                            pathWeight = Mathf.Max(pathWeight, factor * 0.95f);
                        }

                        // 4. COMBINE & NORMALIZE WEIGHTS (Must strictly sum to 1.0)
                        float total = sandWeight + grassWeight + pathWeight + rockWeight;
                        if (total < 0.001f)
                        {
                            grassWeight = 1.0f;
                            total = 1.0f;
                        }

                        splatmapData[y, x, 0] = sandWeight / total;
                        splatmapData[y, x, 1] = grassWeight / total;
                        splatmapData[y, x, 2] = pathWeight / total;
                        splatmapData[y, x, 3] = rockWeight / total;
                    }
                }

                // Apply updated splatmap matrix to terrain data
                tData.SetAlphamaps(0, 0, splatmapData);
                Debug.Log("<color=#44FF44><b>[IslandBiomeAutoPainter]</b></color> Successfully painted 4 biomes across the island terrain!");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        private void dirtPathWeight(ref float currentPathWeight, float wx, float wz, Vector3 size, Vector3 pos)
        {
            // Helper for toxic marsh path blends
            currentPathWeight = Mathf.Max(currentPathWeight, 0.2f);
        }

        /// <summary>
        /// Calculates the minimum 2D distance to winding island path segments.
        /// Segment 1: South Beach (0.50, 0.12) -> Center Campfire (0.50, 0.50)
        /// Segment 2: Center Campfire (0.50, 0.50) -> East Chasm Bridgehead (0.75, 0.52)
        /// Segment 3: Center Campfire (0.50, 0.50) -> North Marsh Entrance (0.50, 0.85)
        /// </summary>
        private float CalculateDistanceToPathways(float nx, float nz)
        {
            Vector2 p = new Vector2(nx, nz);

            // Add subtle organic curve wobble
            float wobble = (Mathf.Sin(nz * 20f) * 0.015f);
            p.x += wobble;

            Vector2 pBeach = new Vector2(0.50f, 0.12f);
            Vector2 pCampfire = new Vector2(0.50f, 0.50f);
            Vector2 pChasm = new Vector2(0.75f, 0.52f);
            Vector2 pNorth = new Vector2(0.50f, 0.85f);

            float d1 = DistToSegment(p, pBeach, pCampfire);
            float d2 = DistToSegment(p, pCampfire, pChasm);
            float d3 = DistToSegment(p, pCampfire, pNorth);

            return Mathf.Min(d1, Mathf.Min(d2, d3));
        }

        private float DistToSegment(Vector2 p, Vector2 a, Vector2 b)
        {
            Vector2 ab = b - a;
            float t = Vector2.Dot(p - a, ab) / Vector2.Dot(ab, ab);
            t = Mathf.Clamp01(t);
            return Vector2.Distance(p, a + t * ab);
        }

        private void SetupTerrainLayers(TerrainData tData)
        {
            TerrainLayer[] required = new TerrainLayer[] { sandLayer, grassLayer, dirtPathLayer, rockLayer };
            tData.terrainLayers = required;
        }
    }
}
