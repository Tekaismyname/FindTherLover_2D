using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace FindTheLover.Environment.Editor
{
    public enum GrassDensityPreset
    {
        Performance,
        LushRecommended,
        ExtremeDense
    }

    /// <summary>
    /// Production-grade Editor Window for converting 2D Sprite Sheets into Terrain Detail Prototypes
    /// and procedurally scattering millions of natural, multi-layered plants across massive landscapes.
    /// </summary>
    public class TerrainGrassAutoSpawner : EditorWindow
    {
        [Header("References")]
        public Terrain targetTerrain;
        public Texture2D sourceSpriteSheet;
        public string extractedFolder = "Assets/03_Art/Sprites/Environment/Plants/Extracted";

        [Header("Density & Presets")]
        public GrassDensityPreset densityPreset = GrassDensityPreset.LushRecommended;
        [Range(100f, 600f)] public float renderDistance = 350f;

        [Header("Ecological Constraints")]
        [Range(15f, 45f)] public float maxSlopeAngle = 32f;
        public float minAltitude = 7.4f; // Water level cutoff
        public float maxAltitude = 58.0f; // Mountain greenline cutoff
        [Range(0.1f, 0.9f)] public float roadExclusionThreshold = 0.35f;

        [Header("Clustering Noise")]
        [Range(5f, 30f)] public float noiseScale = 12f;

        private Vector2 _scrollPos;

        [MenuItem("Tools/Environment/Natural Grass Spawner")]
        public static void OpenWindow()
        {
            var window = GetWindow<TerrainGrassAutoSpawner>("Grass Spawner");
            window.minSize = new Vector2(400, 560);
            window.Show();
        }

        private void OnEnable()
        {
            if (targetTerrain == null)
            {
                targetTerrain = Terrain.activeTerrain;
            }
        }

        private void OnGUI()
        {
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            EditorGUILayout.LabelField("🌿 Natural Grass Spawner (Multi-Layer Engine)", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Tạo thảm thực vật đa tầng (Cỏ nền, Hoa dại, Dương xỉ, Bụi rậm) với quy mô hàng triệu thực thể cho bản đồ lớn (500m+).",
                MessageType.Info);

            EditorGUILayout.Space(8);
            targetTerrain = (Terrain)EditorGUILayout.ObjectField("Target Terrain", targetTerrain, typeof(Terrain), true);
            sourceSpriteSheet = (Texture2D)EditorGUILayout.ObjectField("Source Sprite Sheet", sourceSpriteSheet, typeof(Texture2D), false);
            extractedFolder = EditorGUILayout.TextField("Extracted Folder", extractedFolder);

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("⚡ Density Preset & View Distance", EditorStyles.boldLabel);
            densityPreset = (GrassDensityPreset)EditorGUILayout.EnumPopup("Density Preset", densityPreset);
            renderDistance = EditorGUILayout.Slider("Render Distance (m)", renderDistance, 100f, 500f);

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("📐 Ecological Constraints", EditorStyles.boldLabel);
            maxSlopeAngle = EditorGUILayout.Slider("Max Slope Angle", maxSlopeAngle, 15f, 45f);
            minAltitude = EditorGUILayout.FloatField("Min Altitude (Water Cutoff)", minAltitude);
            maxAltitude = EditorGUILayout.FloatField("Max Altitude (Mountain Cutoff)", maxAltitude);
            roadExclusionThreshold = EditorGUILayout.Slider("Road Exclusion Threshold", roadExclusionThreshold, 0.1f, 0.9f);

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("🎲 Clustering", EditorStyles.boldLabel);
            noiseScale = EditorGUILayout.Slider("Cluster Noise Scale", noiseScale, 5f, 30f);

            EditorGUILayout.Space(12);

            // Button 1: Extract Sprites
            if (GUILayout.Button("✂️ 1. Extract Sprites From Sheet", GUILayout.Height(30)))
            {
                ExtractSprites();
            }

            // Button 2: Register Prototypes
            if (GUILayout.Button("📋 2. Register Detail Prototypes (Upscaled for 500m)", GUILayout.Height(30)))
            {
                RegisterDetailPrototypes();
            }

            EditorGUILayout.Space(6);
            GUI.backgroundColor = new Color(0.35f, 0.95f, 0.45f);
            // Button 3: Spawn Multi-layer Ecosystem
            if (GUILayout.Button("🌿 3. Spawn Multi-Layer Ecosystem (4M+ Grass)", GUILayout.Height(45)))
            {
                SpawnNaturalGrass();
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.Space(8);
            GUI.backgroundColor = new Color(1f, 0.4f, 0.4f);
            if (GUILayout.Button("🗑️ Clear All Grass From Terrain", GUILayout.Height(25)))
            {
                ClearAllGrass();
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.EndScrollView();
        }

        private void ExtractSprites()
        {
            if (sourceSpriteSheet == null)
            {
                EditorUtility.DisplayDialog("Error", "Please assign a Source Sprite Sheet texture!", "OK");
                return;
            }

            string assetPath = AssetDatabase.GetAssetPath(sourceSpriteSheet);
            var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null) return;

            if (!importer.isReadable)
            {
                importer.isReadable = true;
                importer.SaveAndReimport();
            }

            var sprites = AssetDatabase.LoadAllAssetsAtPath(assetPath).OfType<Sprite>().OrderBy(s => s.name).ToArray();
            if (sprites.Length == 0)
            {
                EditorUtility.DisplayDialog("Error", "No sliced sprites found. Make sure Sprite Mode is Multiple!", "OK");
                return;
            }

            if (!Directory.Exists(extractedFolder)) Directory.CreateDirectory(extractedFolder);

            int count = 0;
            foreach (var s in sprites)
            {
                int rx = Mathf.Clamp((int)s.rect.x, 0, sourceSpriteSheet.width - 1);
                int ry = Mathf.Clamp((int)s.rect.y, 0, sourceSpriteSheet.height - 1);
                int rw = Mathf.Clamp((int)s.rect.width, 1, sourceSpriteSheet.width - rx);
                int rh = Mathf.Clamp((int)s.rect.height, 1, sourceSpriteSheet.height - ry);

                var pixels = sourceSpriteSheet.GetPixels(rx, ry, rw, rh);
                var subTex = new Texture2D(rw, rh, TextureFormat.RGBA32, false);
                subTex.SetPixels(pixels);
                subTex.Apply();

                string filePath = $"{extractedFolder}/{s.name}.png";
                File.WriteAllBytes(filePath, subTex.EncodeToPNG());
                DestroyImmediate(subTex);
                count++;
            }

            AssetDatabase.Refresh();

            foreach (var s in sprites)
            {
                string filePath = $"{extractedFolder}/{s.name}.png";
                var subImp = AssetImporter.GetAtPath(filePath) as TextureImporter;
                if (subImp != null)
                {
                    subImp.textureType = TextureImporterType.Default;
                    subImp.alphaIsTransparency = true;
                    subImp.wrapMode = TextureWrapMode.Clamp;
                    subImp.filterMode = FilterMode.Bilinear;
                    subImp.SaveAndReimport();
                }
            }

            EditorUtility.DisplayDialog("Success", $"Extracted {count} sprites into {extractedFolder}!", "OK");
        }

        public void RegisterDetailPrototypes()
        {
            if (targetTerrain == null)
            {
                EditorUtility.DisplayDialog("Error", "Please assign a Target Terrain!", "OK");
                return;
            }

            if (!Directory.Exists(extractedFolder))
            {
                EditorUtility.DisplayDialog("Error", $"Folder {extractedFolder} does not exist. Run step 1 first!", "OK");
                return;
            }

            var pngFiles = Directory.GetFiles(extractedFolder, "*.png").OrderBy(f => f).ToArray();
            if (pngFiles.Length == 0)
            {
                EditorUtility.DisplayDialog("Error", "No PNG files found in extracted folder!", "OK");
                return;
            }

            var dps = new List<DetailPrototype>();
            for (int i = 0; i < pngFiles.Length; i++)
            {
                string relativePath = pngFiles[i].Replace('\\', '/');
                if (relativePath.StartsWith(Application.dataPath))
                {
                    relativePath = "Assets" + relativePath.Substring(Application.dataPath.Length);
                }

                var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(relativePath);
                if (tex == null) continue;

                var dp = new DetailPrototype();
                dp.prototypeTexture = tex;
                dp.renderMode = DetailRenderMode.GrassBillboard;

                // Scale tuned for a massive 500m landscape
                if (i == 1 || i == 3 || i == 9 || i == 13) // Bushes & Shrubs
                {
                    dp.minWidth = 2.2f; dp.maxWidth = 3.6f;
                    dp.minHeight = 1.8f; dp.maxHeight = 3.0f;
                    dp.bendFactor = 0.25f;
                }
                else if (i == 4 || i == 7 || i == 11 || i == 15) // Reeds & Ferns
                {
                    dp.minWidth = 1.4f; dp.maxWidth = 2.2f;
                    dp.minHeight = 1.4f; dp.maxHeight = 2.4f;
                    dp.bendFactor = 0.5f;
                }
                else if (i == 2 || i == 6 || i == 10 || i == 14 || i == 18) // Wildflowers
                {
                    dp.minWidth = 1.2f; dp.maxWidth = 1.8f;
                    dp.minHeight = 1.1f; dp.maxHeight = 1.7f;
                    dp.bendFactor = 0.35f;
                }
                else // Base Meadow Grass
                {
                    dp.minWidth = 1.2f; dp.maxWidth = 2.0f;
                    dp.minHeight = 1.1f; dp.maxHeight = 1.8f;
                    dp.bendFactor = 0.5f;
                }

                dp.healthyColor = Color.white;
                dp.dryColor = new Color(0.92f, 0.98f, 0.88f);
                dp.noiseSpread = 0.15f;
                dps.Add(dp);
            }

            targetTerrain.terrainData.detailPrototypes = dps.ToArray();
            targetTerrain.detailObjectDistance = renderDistance;
            targetTerrain.detailObjectDensity = 1.0f;
            EditorUtility.SetDirty(targetTerrain.terrainData);
            EditorUtility.SetDirty(targetTerrain);
            AssetDatabase.SaveAssets();

            EditorUtility.DisplayDialog("Success", $"Registered {dps.Count} upscaled DetailPrototypes on Terrain!", "OK");
        }

        public void SpawnNaturalGrass()
        {
            if (targetTerrain == null)
            {
                EditorUtility.DisplayDialog("Error", "Please assign a Target Terrain!", "OK");
                return;
            }

            var td = targetTerrain.terrainData;
            int numPrototypes = td.detailPrototypes.Length;
            if (numPrototypes == 0)
            {
                RegisterDetailPrototypes();
                numPrototypes = td.detailPrototypes.Length;
                if (numPrototypes == 0) return;
            }

            int dRes = td.detailResolution;
            int alphaW = td.alphamapWidth;
            int alphaH = td.alphamapHeight;
            float[,,] alphas = td.GetAlphamaps(0, 0, alphaW, alphaH);
            int pathLayerIdx = Mathf.Min(2, td.terrainLayers.Length - 1);

            int[][,] detailMaps = new int[numPrototypes][,];
            for (int i = 0; i < numPrototypes; i++)
            {
                detailMaps[i] = new int[dRes, dRes];
            }

            float densityMult = (densityPreset == GrassDensityPreset.Performance) ? 0.6f :
                                (densityPreset == GrassDensityPreset.LushRecommended) ? 1.0f : 1.5f;

            // Categorized prototype indices
            int[] baseGrassLayers = new int[] { 0, 5, 8, 12, 16, 17, 19 };
            int[] flowerLayers = new int[] { 2, 6, 10, 14, 18 };
            int[] reedLayers = new int[] { 4, 7, 11, 15 };
            int[] bushLayers = new int[] { 1, 3, 9, 13 };

            // Remap if fewer prototypes exist
            System.Func<int[], int> SafePick = (arr) => arr[Random.Range(0, arr.Length)] % numPrototypes;

            var rand = new System.Random(42);

            for (int y = 0; y < dRes; y++)
            {
                float normZ = (float)y / (dRes - 1);
                int ay = Mathf.Clamp((int)(normZ * alphaH), 0, alphaH - 1);

                for (int x = 0; x < dRes; x++)
                {
                    float normX = (float)x / (dRes - 1);
                    int ax = Mathf.Clamp((int)(normX * alphaW), 0, alphaW - 1);

                    // 1. Altitude Masking
                    float currentH = td.GetInterpolatedHeight(normX, normZ);
                    if (currentH < minAltitude || currentH > maxAltitude) continue;

                    // 2. Slope Masking
                    float steepness = td.GetSteepness(normX, normZ);
                    if (steepness > maxSlopeAngle) continue;

                    // 3. Road / Path Masking
                    float pathWeight = alphas[ay, ax, pathLayerIdx];
                    if (pathWeight > roadExclusionThreshold) continue;

                    float grassWeight = alphas[ay, ax, 0];
                    float rockWeight = alphas[ay, ax, Mathf.Min(1, td.terrainLayers.Length - 1)];

                    // Multi-frequency noise
                    float macroNoise = Mathf.PerlinNoise(normX * noiseScale + 1.2f, normZ * noiseScale + 4.5f);
                    float flowerCluster = Mathf.PerlinNoise(normX * (noiseScale * 2.2f) + 15.3f, normZ * (noiseScale * 2.2f) + 29.8f);
                    float bushCluster = Mathf.PerlinNoise(normX * (noiseScale * 1.4f) + 55.1f, normZ * (noiseScale * 1.4f) + 72.4f);

                    // TẦNG 1: THẢM CỎ NỀN (Phủ xanh toàn bộ thung lũng)
                    if (grassWeight > 0.18f)
                    {
                        int baseProto = baseGrassLayers[((x / 4) + (y / 4)) % baseGrassLayers.Length] % numPrototypes;
                        int baseDensity = (int)(Mathf.Lerp(8f, 15f, macroNoise) * grassWeight * densityMult);
                        if (pathWeight > 0.15f) baseDensity = (int)(baseDensity * (1f - (pathWeight - 0.15f) * 4f));
                        detailMaps[baseProto][y, x] = Mathf.Clamp(baseDensity, 0, 16);

                        if (macroNoise > 0.45f)
                        {
                            int secProto = baseGrassLayers[(((x / 4) + (y / 4)) + 2) % baseGrassLayers.Length] % numPrototypes;
                            detailMaps[secProto][y, x] = Mathf.Clamp((int)(macroNoise * 10f * densityMult), 0, 14);
                        }
                    }

                    // TẦNG 2: CÁNH ĐỒNG HOA DẠI (Nở rộ trên thảm cỏ)
                    if (grassWeight > 0.35f && flowerCluster > 0.52f)
                    {
                        int fProto = flowerLayers[((int)(normX * 12f) + (int)(normZ * 12f)) % flowerLayers.Length] % numPrototypes;
                        int fDensity = (int)((flowerCluster - 0.52f) * 24f * densityMult);
                        detailMaps[fProto][y, x] = Mathf.Clamp(fDensity, 0, 14);
                    }

                    // TẦNG 3: BỤI RẬM VÀ CÂY BỤI LỚN (Chân núi & gò đồi)
                    if (bushCluster > 0.65f && rockWeight > 0.10f && currentH > 10f)
                    {
                        int bProto = bushLayers[rand.Next(bushLayers.Length)] % numPrototypes;
                        int bDensity = (int)((bushCluster - 0.65f) * 18f * densityMult);
                        detailMaps[bProto][y, x] = Mathf.Clamp(bDensity, 0, 10);
                    }
                }
            }

            for (int i = 0; i < numPrototypes; i++)
            {
                td.SetDetailLayer(0, 0, i, detailMaps[i]);
            }

            targetTerrain.detailObjectDistance = renderDistance;
            targetTerrain.detailObjectDensity = 1.0f;
            EditorUtility.SetDirty(td);
            EditorUtility.SetDirty(targetTerrain);

            EditorUtility.DisplayDialog("Complete", $"Natural grass ecosystem spawned with {densityPreset} preset! Render distance set to {renderDistance}m.", "OK");
        }

        private void ClearAllGrass()
        {
            if (targetTerrain == null) return;
            var td = targetTerrain.terrainData;
            int dRes = td.detailResolution;
            int[,] emptyLayer = new int[dRes, dRes];
            for (int i = 0; i < td.detailPrototypes.Length; i++)
            {
                td.SetDetailLayer(0, 0, i, emptyLayer);
            }
            EditorUtility.SetDirty(td);
            EditorUtility.DisplayDialog("Cleared", "All grass removed from terrain.", "OK");
        }
    }
}
