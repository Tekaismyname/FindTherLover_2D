using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;
using FindTheLover.Environment;
using FindTheLover.Environment.Hazards;
using FindTheLover.Environment.Puzzles;

namespace FindTheLover.EditorTools
{
    /// <summary>
    /// Production-grade Editor World Builder for Zone 2 (Toxic Marsh & Soul Flame Island)
    /// and Multi-Island Pathway Painter.
    /// - Extracts and builds custom 2.5D Swamp Environment Prefabs (Weeping Willow, Dead Rune Tree,
    ///   Giant Bioluminescent Mushrooms, Runic Obsidian Boulder, Crystal & Ore Rocks).
    /// - Repaints organic winding S-curve pathways & natural biomes on both Zone 1 and Zone 2 Terrains.
    /// - Constructs the Inter-Island Bridge, Ruin Archways, Colonnade, Puzzle Props, VFX, and NavMesh.
    /// </summary>
    public static class Zone2IslandWorldBuilder
    {
        private const string EnvMatDir = "Assets/03_Art/Materials/Environment";
        private const string PrefabDir = "Assets/04_Prefabs/Environment";
        private const string EnvPrefabDir = "Assets/04_Prefabs/Environments";
        private const string SwampSpriteDir = "Assets/03_Art/Sprites/Environment/Zone2_Swamp";
        private const string TerrainDataPath = "Assets/01_Data/Terrain/Zone2_ToxicIsland_TerrainData.asset";

        // Island 2 World Spatial Constants
        private static readonly Vector3 Zone2TerrainOrigin = new Vector3(250f, 0f, -200f);
        private static readonly Vector3 Zone2TerrainSize = new Vector3(400f, 80f, 400f);
        private static readonly Vector2 IslandCenterXZ = new Vector2(440f, 0f);
        private const float BaseGroundHeight = 8.0f;

        // Key Landmark Coordinates on Island 2
        private static readonly Vector3 BridgeStartZone1 = new Vector3(222f, BaseGroundHeight, 0f);
        private static readonly Vector3 BridgeEndZone2 = new Vector3(282f, BaseGroundHeight, 0f);
        private static readonly Vector3 PosEntranceArch = new Vector3(322f, BaseGroundHeight, 0f);
        private static readonly Vector3 PosBrazier01 = new Vector3(342f, BaseGroundHeight, 0f);
        private static readonly Vector3 PosCentralFork = new Vector3(435f, BaseGroundHeight, -6f);
        private static readonly Vector3 PosSwampLake = new Vector3(415f, 6.9f, 18f);
        private static readonly Vector3 PosSoulAltar = new Vector3(435f, BaseGroundHeight, -68f);
        private static readonly Vector3 PosBrazier02 = new Vector3(485f, BaseGroundHeight, 14f);
        private static readonly Vector3 PosThornGate = new Vector3(535f, BaseGroundHeight, 0f);

        [MenuItem("Tools/Environment/🏝️ Build Zone 2 Toxic Island & Bridge (Full World)")]
        public static void BuildZone2IslandWorld()
        {
            CleanupOldZone2Objects();

            // 1. Slice & Build Custom Zone 2 Swamp 2.5D Environment Prefabs
            Dictionary<string, GameObject> zone2EnvPrefabs = ExtractAndBuildZone2SwampPrefabs();

            // 2. Load ProBuilder Materials
            Material stoneCarved = LoadMat($"{EnvMatDir}/Prop_StoneCarved_Mat.mat");
            Material stoneMossy = LoadMat($"{EnvMatDir}/Prop_StoneMossy_Mat.mat");
            Material darkPit = LoadMat($"{EnvMatDir}/Prop_DarkPit_Mat.mat");
            Material antiqueBronze = LoadMat($"{EnvMatDir}/Prop_AntiqueBronze_Mat.mat");
            Material forgedIron = LoadMat($"{EnvMatDir}/Prop_ForgedIron_Mat.mat");
            Material spikeBronzeIron = LoadMat($"{EnvMatDir}/Prop_SpikeBronzeIron_Mat.mat");
            Material polishedWood = LoadMat($"{EnvMatDir}/Prop_PolishedWood_Mat.mat");
            Material darkBrambleWood = LoadMat($"{EnvMatDir}/Prop_DarkBrambleWood_Mat.mat");
            Material obsidianStone = LoadMat($"{EnvMatDir}/Prop_DarkObsidianStone_Mat.mat");
            Material runeCyanGlow = LoadMat($"{EnvMatDir}/Prop_RuneCyanGlow_Mat.mat");
            Material soulCoreGlow = LoadMat($"{EnvMatDir}/Prop_SoulCoreGlow_Mat.mat");
            Material emberOrangeGlow = LoadMat($"{EnvMatDir}/Prop_EmberOrangeGlow_Mat.mat");
            Material flameGoldenGlow = LoadMat($"{EnvMatDir}/Prop_FlameGoldenGlow_Mat.mat");

            // 3. Sculpt Zone 1 Winding Eastern Pass & Repaint Zone 1 Natural Biomes + Winding Pathways
            Terrain zone1Terrain = GameObject.Find("Terrain")?.GetComponent<Terrain>();
            if (zone1Terrain != null)
            {
                SculptAndPaintZone1BiomesAndPaths(zone1Terrain);
            }

            // 4. Configure Archipelago Ocean Water
            SetupArchipelagoOceanWater();

            // 5. Create & Sculpt Zone 2 Island Terrain
            Terrain zone2Terrain = CreateAndSculptZone2Terrain(zone1Terrain);

            // 6. Paint 4 Biomes & Winding Pathways on Zone 2 Terrain
            PaintZone2BiomesAndWindingPaths(zone2Terrain);

            // 7. Spawn Multi-Layer 2.5D Grass, Wildflowers, Ferns & Bushes on Zone 2 Terrain
            SpawnZone2DetailGrass(zone2Terrain, zone1Terrain);

            // 8. Create Master Hierarchy Root for Zone 2 Island
            GameObject levelRoot = new GameObject("Zone2_ToxicCavern_Level");
            levelRoot.transform.position = Vector3.zero;

            GameObject archRoot = new GameObject("Architecture_ProBuilder");
            archRoot.transform.SetParent(levelRoot.transform, false);

            GameObject propsRoot = new GameObject("Puzzle_Props");
            propsRoot.transform.SetParent(levelRoot.transform, false);

            GameObject vegRoot = new GameObject("Vegetation_25D_TreesAndRocks");
            vegRoot.transform.SetParent(levelRoot.transform, false);

            GameObject lightsRoot = new GameObject("Atmosphere_Lights_And_Water");
            lightsRoot.transform.SetParent(levelRoot.transform, false);

            // 9. Build Stylized ProBuilder Inter-Island Bridge (Zone 1 -> Zone 2)
            BuildInterIslandBridge(archRoot.transform, stoneCarved, stoneMossy, obsidianStone, polishedWood, forgedIron, antiqueBronze, runeCyanGlow);

            // 10. Build Entrance Ruin Archway, Southern Soul Sanctuary Colonnade & Gorge Buttresses
            BuildEntranceRuinArchway(archRoot.transform, PosEntranceArch, stoneCarved, stoneMossy, obsidianStone, runeCyanGlow);
            BuildSoulSanctuaryColonnade(archRoot.transform, PosSoulAltar, stoneCarved, stoneMossy, obsidianStone, runeCyanGlow);
            BuildGorgeSealButtresses(archRoot.transform, PosThornGate, stoneCarved, stoneMossy, obsidianStone, runeCyanGlow);

            // 11. Build Puzzle Landmarks (Brazier 01, Brazier 02, Soul Flame Altar, Thorn Barrier Gate)
            GameObject b1 = BuildTripodBrazier(
                "Brazier_Checkpoint_01",
                SnapToTerrainAndClearGrass(zone2Terrain, PosBrazier01, 2.8f),
                BrazierRole.NormalCheckpoint,
                true,
                stoneCarved, stoneMossy, forgedIron, antiqueBronze, polishedWood, darkPit, emberOrangeGlow, flameGoldenGlow);
            b1.transform.SetParent(propsRoot.transform, true);

            GameObject b2 = BuildTripodBrazier(
                "Brazier_Checkpoint_02",
                SnapToTerrainAndClearGrass(zone2Terrain, PosBrazier02, 2.8f),
                BrazierRole.NormalCheckpoint,
                false,
                stoneCarved, stoneMossy, forgedIron, antiqueBronze, polishedWood, darkPit, emberOrangeGlow, flameGoldenGlow);
            b2.transform.SetParent(propsRoot.transform, true);

            GameObject altar = BuildMonumentalSoulAltar(
                "Soul_Flame_Altar",
                SnapToTerrainAndClearGrass(zone2Terrain, PosSoulAltar, 3.5f),
                BrazierRole.SoulFlameAltar,
                true,
                obsidianStone, stoneCarved, stoneMossy, darkPit, runeCyanGlow, soulCoreGlow);
            altar.transform.SetParent(propsRoot.transform, true);

            GameObject gate = BuildTwinObeliskThornGate(
                "Thorn_Barrier_Gate",
                SnapToTerrainAndClearGrass(zone2Terrain, PosThornGate, 3.6f),
                stoneCarved, stoneMossy, obsidianStone, darkBrambleWood, spikeBronzeIron, forgedIron, antiqueBronze, runeCyanGlow);
            gate.transform.SetParent(propsRoot.transform, true);

            // 12. Setup Toxic Swamp Lake & Multi-Layer Poison Gas Zone VFX
            SetupSwampWaterAndPoisonMist(lightsRoot.transform, propsRoot.transform, zone2Terrain);

            // 13. Scatter Custom Zone 2 Swamp Trees, Giant Mushrooms, Runic Boulders & Crystal Rocks
            ScatterZone2CustomVegetation(vegRoot.transform, zone2Terrain, zone2EnvPrefabs);

            // 14. Position Zone 2 Enemies onto Island 2
            PositionZone2Enemies(zone2Terrain, levelRoot.transform);

            // 15. Rebuild NavMesh
            BakeWorldNavMesh(zone1Terrain, zone2Terrain);

            UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("<color=#00FFCC><b>[Zone2IslandWorldBuilder] Successfully built Zone 2 Custom Swamp Sprites, Winding Pathways on Both Islands & ProBuilder Monuments!</b></color>");
        }

        // =========================================================================
        // STEP 1: EXTRACT & BUILD CUSTOM ZONE 2 SWAMP 2.5D PREFABS
        // =========================================================================
        private static Dictionary<string, GameObject> ExtractAndBuildZone2SwampPrefabs()
        {
            var dict = new Dictionary<string, GameObject>();
            string sourceSheetPath = $"{SwampSpriteDir}/Zone2_Swamp_Source.jpg";
            AssetDatabase.ImportAsset(sourceSheetPath, ImportAssetOptions.ForceUpdate);

            TextureImporter srcImp = AssetImporter.GetAtPath(sourceSheetPath) as TextureImporter;
            if (srcImp != null)
            {
                srcImp.isReadable = true;
                srcImp.textureCompression = TextureImporterCompression.Uncompressed;
                srcImp.SaveAndReimport();
            }

            Texture2D sheet = AssetDatabase.LoadAssetAtPath<Texture2D>(sourceSheetPath);
            Mesh quadMesh = AssetDatabase.LoadAssetAtPath<Mesh>("Assets/03_Art/Meshes/Quad_BottomPivot.asset");

            if (sheet != null && quadMesh != null)
            {
                int halfW = sheet.width / 2;
                int halfH = sheet.height / 2;

                // 2x2 Grid:
                // Top-Left (x: 0, y: halfH): Zone2_WeepingWillow
                // Top-Right (x: halfW, y: halfH): Zone2_DeadRuneTree
                // Bottom-Left (x: 0, y: 0): Zone2_GiantSporeShrooms
                // Bottom-Right (x: halfW, y: 0): Zone2_RunicObsidianRock
                var quadrants = new[]
                {
                    new { Name = "Zone2_WeepingWillow", X = 0, Y = halfH, Scale = new Vector3(11.5f, 13.0f, 1f), IsTree = true },
                    new { Name = "Zone2_DeadRuneTree", X = halfW, Y = halfH, Scale = new Vector3(10.0f, 12.2f, 1f), IsTree = true },
                    new { Name = "Zone2_GiantSporeShrooms", X = 0, Y = 0, Scale = new Vector3(7.2f, 8.0f, 1f), IsTree = false },
                    new { Name = "Zone2_RunicObsidianRock", X = halfW, Y = 0, Scale = new Vector3(7.8f, 8.8f, 1f), IsTree = false }
                };

                foreach (var q in quadrants)
                {
                    Texture2D subTex = ExtractCleanQuadrantWithFloodFill(sheet, q.X, q.Y, halfW, halfH);
                    string pngPath = $"{SwampSpriteDir}/{q.Name}.png";
                    File.WriteAllBytes(pngPath, subTex.EncodeToPNG());
                    Object.DestroyImmediate(subTex);
                    AssetDatabase.ImportAsset(pngPath, ImportAssetOptions.ForceUpdate);

                    TextureImporter ti = AssetImporter.GetAtPath(pngPath) as TextureImporter;
                    if (ti != null)
                    {
                        ti.textureType = TextureImporterType.Default;
                        ti.alphaIsTransparency = true;
                        ti.wrapMode = TextureWrapMode.Clamp;
                        ti.filterMode = FilterMode.Point;
                        ti.SaveAndReimport();
                    }

                    Texture2D loadedTex = AssetDatabase.LoadAssetAtPath<Texture2D>(pngPath);
                    Material mat = CreateAlphaClippedLitMaterial($"{EnvMatDir}/{q.Name}_Mat.mat", q.Name + "_Mat", loadedTex);
                    GameObject pf = CreateBillboardQuadPrefab($"{EnvPrefabDir}/{q.Name}_Quad.prefab", q.Name + "_Quad", quadMesh, mat, q.Scale, q.IsTree);
                    dict[q.Name] = pf;
                }
            }

            // Also create prefabs for the unused Crystal Rock (rocks_2) & Ore Rock (rocks_1) & Autumn Birch (trees_12)
            Texture2D crystalTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/03_Art/Sprites/Environment/Rocks/Extracted/rocks_2.png");
            if (crystalTex != null && quadMesh != null)
            {
                Material cMat = CreateAlphaClippedLitMaterial($"{EnvMatDir}/Rock_Crystal_Mat.mat", "Rock_Crystal_Mat", crystalTex);
                dict["Rock_Crystal"] = CreateBillboardQuadPrefab($"{EnvPrefabDir}/Rock_Crystal_Quad.prefab", "Rock_Crystal_Quad", quadMesh, cMat, new Vector3(8.2f, 9.5f, 1f), false);
            }

            Texture2D oreTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/03_Art/Sprites/Environment/Rocks/Extracted/rocks_1.png");
            if (oreTex != null && quadMesh != null)
            {
                Material oMat = CreateAlphaClippedLitMaterial($"{EnvMatDir}/Rock_Ore_Mat.mat", "Rock_Ore_Mat", oreTex);
                dict["Rock_Ore"] = CreateBillboardQuadPrefab($"{EnvPrefabDir}/Rock_Ore_Quad.prefab", "Rock_Ore_Quad", quadMesh, oMat, new Vector3(8.0f, 9.2f, 1f), false);
            }

            return dict;
        }

        /// <summary>
        /// Extracts a sub-rectangle from a spritesheet, removes pure/near-white background via border flood-fill,
        /// and shifts the visible sprite down so its lowest opaque pixel rests cleanly at Y = 0 (Bottom Pivot).
        /// </summary>
        private static Texture2D ExtractCleanQuadrantWithFloodFill(Texture2D sheet, int startX, int startY, int w, int h)
        {
            Color[] raw = sheet.GetPixels(startX, startY, w, h);
            Color32[] pixels = new Color32[w * h];
            for (int i = 0; i < raw.Length; i++) pixels[i] = raw[i];

            bool IsBgWhite(Color32 c)
            {
                int maxC = Mathf.Max(c.r, Mathf.Max(c.g, c.b));
                int minC = Mathf.Min(c.r, Mathf.Min(c.g, c.b));
                if (minC >= 200 && (maxC - minC) <= 42) return true;
                // Pale mint-white outer glow around the mushroom cap (high G & B, washed-out R >= 168)
                if (c.r >= 168 && c.g >= 198 && c.b >= 188) return true;
                return false;
            }

            // Clear both outer background and enclosed white pockets between hanging willow vines / mushroom stems
            for (int i = 0; i < pixels.Length; i++)
            {
                if (IsBgWhite(pixels[i]))
                {
                    pixels[i] = new Color32(0, 0, 0, 0);
                }
            }

            // Clean 2-pass pale anti-aliasing fringe adjacent to transparent background
            for (int pass = 0; pass < 2; pass++)
            {
                Color32[] dehalod = (Color32[])pixels.Clone();
                for (int y = 1; y < h - 1; y++)
                {
                    for (int x = 1; x < w - 1; x++)
                    {
                        int idx = y * w + x;
                        Color32 c = pixels[idx];
                        if (c.a == 0) continue;
                        int minC = Mathf.Min(c.r, Mathf.Min(c.g, c.b));
                        int maxC = Mathf.Max(c.r, Mathf.Max(c.g, c.b));
                        bool isPaleFringe = (minC >= 155 && (maxC - minC) <= 55) || (c.r >= 145 && c.g >= 185 && c.b >= 175);
                        if (isPaleFringe)
                        {
                            if (pixels[idx - 1].a == 0 || pixels[idx + 1].a == 0 || pixels[idx - w].a == 0 || pixels[idx + w].a == 0)
                            {
                                dehalod[idx] = new Color32(0, 0, 0, 0);
                            }
                        }
                    }
                }
                pixels = dehalod;
            }

            // Find lowest visible pixel row (minY) and shift down to y=0 so Quad_BottomPivot stands flush on the ground!
            int minY = 0;
            for (int y = 0; y < h; y++)
            {
                bool hasOpaque = false;
                for (int x = 0; x < w; x++)
                {
                    if (pixels[y * w + x].a > 30) { hasOpaque = true; break; }
                }
                if (hasOpaque) { minY = y; break; }
            }

            Color32[] grounded = new Color32[w * h];
            for (int y = minY; y < h; y++)
            {
                int dstY = y - minY;
                for (int x = 0; x < w; x++)
                    grounded[dstY * w + x] = pixels[y * w + x];
            }

            Texture2D outTex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            outTex.SetPixels32(grounded);
            outTex.Apply();
            return outTex;
        }

        private static Material CreateAlphaClippedLitMaterial(string matPath, string matName, Texture2D tex)
        {
            Material m = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            Shader lit = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            if (m == null)
            {
                m = new Material(lit);
                m.name = matName;
                AssetDatabase.CreateAsset(m, matPath);
            }
            m.shader = lit;
            m.SetColor("_BaseColor", Color.white);
            m.SetTexture("_BaseMap", tex);
            m.mainTexture = tex;
            m.SetFloat("_AlphaClip", 1f);
            m.SetFloat("_Cutoff", 0.45f);
            m.SetFloat("_Smoothness", 0.05f);
            m.EnableKeyword("_ALPHATEST_ON");
            EditorUtility.SetDirty(m);
            return m;
        }

        private static GameObject CreateBillboardQuadPrefab(string prefabPath, string name, Mesh quadMesh, Material mat, Vector3 scale, bool isTree)
        {
            GameObject temp = new GameObject(name);
            temp.transform.localScale = scale;

            MeshFilter mf = temp.AddComponent<MeshFilter>();
            mf.sharedMesh = quadMesh;

            MeshRenderer mr = temp.AddComponent<MeshRenderer>();
            mr.sharedMaterial = mat;

            if (isTree)
            {
                CapsuleCollider cc = temp.AddComponent<CapsuleCollider>();
                cc.center = new Vector3(0f, 0.18f, 0f);
                cc.radius = 0.08f;
                cc.height = 0.36f;
            }
            else
            {
                BoxCollider bc = temp.AddComponent<BoxCollider>();
                bc.center = new Vector3(0f, 0.22f, 0f);
                bc.size = new Vector3(0.45f, 0.44f, 0.45f);
            }

            temp.AddComponent<DynamicBillboard>();

            GameObject saved = PrefabUtility.SaveAsPrefabAsset(temp, prefabPath);
            Object.DestroyImmediate(temp);
            return saved;
        }

        private static void CleanupOldZone2Objects()
        {
            string[] oldRoots = {
                "Zone2_ToxicCavern_Level",
                "Terrain_Zone2_ToxicIsland",
                "Brazier_Checkpoint_01",
                "Brazier_Checkpoint_02",
                "Soul_Flame_Altar",
                "Thorn_Barrier_Gate",
                "Poison_Gas_Zone"
            };
            foreach (string name in oldRoots)
            {
                GameObject obj = GameObject.Find(name);
                while (obj != null)
                {
                    Object.DestroyImmediate(obj);
                    obj = GameObject.Find(name);
                }
            }
        }

        private static Material LoadMat(string path)
        {
            Material m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (m == null)
            {
                Shader lit = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                m = new Material(lit);
            }
            return m;
        }

        // =========================================================================
        // STEP 3: SCULPT & REPAINT ZONE 1 BIOMES + ORGANIC WINDING PATHWAYS
        // =========================================================================
        private static void SculptAndPaintZone1BiomesAndPaths(Terrain zone1)
        {
            TerrainData td = zone1.terrainData;
            int hRes = td.heightmapResolution;
            float[,] heights = td.GetHeights(0, 0, hRes, hRes);
            Vector3 tPos = zone1.transform.position;
            Vector3 tSize = td.size;

            // 1. Sculpt Organic Winding Eastern Bridge Pass on Zone 1
            for (int y = 0; y < hRes; y++)
            {
                float normZ = (float)y / (hRes - 1);
                float worldZ = tPos.z + normZ * tSize.z;

                for (int x = 0; x < hRes; x++)
                {
                    float normX = (float)x / (hRes - 1);
                    float worldX = tPos.x + normX * tSize.x;

                    // Follow the natural S-curve path centerZ(worldX) along X = [65, 250]
                    if (worldX > 65f && Mathf.Abs(worldZ) < 55f)
                    {
                        float curveZ = GetZone1EastRoadCurveZ(worldX);
                        float distZ = Mathf.Abs(worldZ - curveZ);
                        float passBlend = Mathf.SmoothStep(1f, 0f, Mathf.Clamp01((distZ - 10f) / 30f));

                        float targetWorldH = BaseGroundHeight;
                        if (worldX > 222f)
                        {
                            float seaT = Mathf.Clamp01((worldX - 222f) / 18f);
                            targetWorldH = Mathf.Lerp(BaseGroundHeight, 0.5f, Mathf.SmoothStep(0f, 1f, seaT));
                        }

                        float currentWorldH = heights[y, x] * tSize.y;
                        float blendedWorldH = Mathf.Lerp(currentWorldH, targetWorldH, passBlend);
                        heights[y, x] = blendedWorldH / tSize.y;
                    }
                    else if (worldX > 230f)
                    {
                        float edgeT = Mathf.Clamp01((worldX - 230f) / 18f);
                        float currentWorldH = heights[y, x] * tSize.y;
                        heights[y, x] = Mathf.Lerp(currentWorldH, 0.0f, edgeT) / tSize.y;
                    }
                }
            }
            td.SetHeights(0, 0, heights);

            // 2. Repaint Zone 1 Biomes & Winding Pathways with CORRECT Layer Order:
            // [0] Layer_Grass, [1] Layer_Rock, [2] Layer_DirtPath, [3] Layer_Sand
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
                        float noise = Mathf.PerlinNoise(worldX * 0.05f + 2.3f, worldZ * 0.05f + 7.1f);

                        float wGrass = 0f;
                        float wRock = 0f;
                        float wPath = 0f;
                        float wSand = 0f;

                        // A. Low Coastline, Lake Bed & Southern Beach -> Sand (Channel 3)
                        if (h < 6.8f || (worldZ < -68f && h < 9.2f && Mathf.Abs(worldX) < 85f))
                        {
                            float sandBlend = (h < 6.8f) ? 1f : Mathf.SmoothStep(0f, 1f, (-68f - worldZ) / 22f);
                            wSand = sandBlend;
                            wGrass = 1f - sandBlend;
                        }
                        // B. Steep Mountain Rim & Canyon Slopes -> Rock (Channel 1)
                        else if (steep > 18f || h > 13.5f)
                        {
                            wRock = Mathf.Clamp01((steep - 14f) / 14f + (h - 12f) / 12f);
                            wGrass = 1f - wRock;
                        }
                        // C. Valley Meadow -> Lush Grass (Channel 0) with subtle mossy rock patches
                        else
                        {
                            wGrass = Mathf.Lerp(0.78f, 0.98f, noise);
                            wRock = 1f - wGrass;
                        }

                        // D. Organic Winding S-Curve Pathways on Zone 1 (Channel 2)
                        Vector2 pWobble = new Vector2(worldX + (noise - 0.5f) * 1.8f, worldZ + (noise - 0.5f) * 1.8f);
                        float distToZ1Path = DistanceToZone1Pathways(pWobble);
                        if (distToZ1Path < 4.2f && h >= 6.6f && h <= 11.5f)
                        {
                            float pFactor = Mathf.SmoothStep(1f, 0f, distToZ1Path / 4.2f);
                            wPath = pFactor * 0.94f;
                            // Subtle stone border at road edges
                            if (distToZ1Path > 2.6f)
                                wRock = Mathf.Max(wRock, 0.35f * pFactor);
                            wGrass *= (1f - pFactor * 0.90f);
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

            // 3. Clear 2.5D Grass Details along the newly painted S-Curve Pathways & Steep Mountain Rim on Zone 1
            int dRes = td.detailResolution;
            int dCount = td.detailPrototypes != null ? td.detailPrototypes.Length : 0;
            if (dRes > 0 && dCount > 0)
            {
                for (int layer = 0; layer < dCount; layer++)
                {
                    int[,] detailMap = td.GetDetailLayer(0, 0, dRes, dRes, layer);
                    for (int dy = 0; dy < dRes; dy++)
                    {
                        float normZ = (float)dy / (dRes - 1);
                        float worldZ = tPos.z + normZ * tSize.z;
                        for (int dx = 0; dx < dRes; dx++)
                        {
                            if (detailMap[dy, dx] == 0) continue;
                            float normX = (float)dx / (dRes - 1);
                            float worldX = tPos.x + normX * tSize.x;
                            float h = td.GetInterpolatedHeight(normX, normZ);
                            float steep = td.GetSteepness(normX, normZ);
                            if (steep > 22f || h < 6.8f || h > 13.5f || DistanceToZone1Pathways(new Vector2(worldX, worldZ)) < 4.5f)
                            {
                                detailMap[dy, dx] = 0;
                            }
                        }
                    }
                    td.SetDetailLayer(0, 0, layer, detailMap);
                }
            }

            EditorUtility.SetDirty(td);
        }

        private static float GetZone1EastRoadCurveZ(float worldX)
        {
            if (worldX <= 1.5f) return 11.5f;
            if (worldX >= 210f) return 0f;
            float t = (worldX - 1.5f) / (210f - 1.5f);
            float baseZ = Mathf.Lerp(11.5f, 0f, Mathf.SmoothStep(0f, 1f, t));
            float wave = Mathf.Sin(t * Mathf.PI * 2.0f) * 8.5f * (1f - t * 0.6f);
            return baseZ + wave;
        }

        private static float DistanceToZone1Pathways(Vector2 p)
        {
            Vector2 pSouthBeach = new Vector2(0f, -92f);
            Vector2 pSouthMid1 = new Vector2(-9f, -52f);
            Vector2 pSouthMid2 = new Vector2(6f, -18f);
            Vector2 pCampfire = new Vector2(1.5f, 11.5f);
            Vector2 pLakeMid = new Vector2(-8f, 38f);
            Vector2 pLakeShore = new Vector2(2f, 62f);
            Vector2 pWestStream = new Vector2(-42f, 22f);

            // Winding South-to-North Main Trail
            float d1 = DistPointToSegment(p, pSouthBeach, pSouthMid1);
            float d2 = DistPointToSegment(p, pSouthMid1, pSouthMid2);
            float d3 = DistPointToSegment(p, pSouthMid2, pCampfire);
            float d4 = DistPointToSegment(p, pCampfire, pLakeMid);
            float d5 = DistPointToSegment(p, pLakeMid, pLakeShore);
            float d6 = DistPointToSegment(p, pCampfire, pWestStream);

            // Winding Eastern Pass Road from Campfire (1.5, 11.5) to Bridgehead (222, 0)
            float dEast = float.MaxValue;
            if (p.x >= -4f && p.x <= 226f)
            {
                float targetZ = GetZone1EastRoadCurveZ(Mathf.Clamp(p.x, 1.5f, 222f));
                dEast = Mathf.Abs(p.y - targetZ);
            }

            // Campfire Sanctuary Circular Plaza (Radius 7.5m)
            float dCampPlaza = Mathf.Max(0f, Vector2.Distance(p, pCampfire) - 7.5f);

            float minSeg = Mathf.Min(Mathf.Min(d1, d2), Mathf.Min(Mathf.Min(d3, d4), Mathf.Min(d5, d6)));
            return Mathf.Min(minSeg, Mathf.Min(dEast, dCampPlaza));
        }

        // =========================================================================
        // STEP 4: SETUP ARCHIPELAGO OCEAN WATER
        // =========================================================================
        private static void SetupArchipelagoOceanWater()
        {
            GameObject ocean = null;
            foreach (var root in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
            {
                if (root.name == "Ocean_Water") { ocean = root; break; }
            }
            if (ocean != null)
            {
                ocean.SetActive(true);
                ocean.transform.position = new Vector3(200f, 3.2f, 0f);
                ocean.transform.localScale = new Vector3(220f, 1f, 220f);
                Material waterMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/03_Art/Materials/Water/RealisticWater_Mat.mat");
                if (waterMat != null && ocean.GetComponent<MeshRenderer>() != null)
                {
                    ocean.GetComponent<MeshRenderer>().sharedMaterial = waterMat;
                }
            }
        }

        // =========================================================================
        // STEP 5: CREATE & SCULPT ZONE 2 ISLAND TERRAIN
        // =========================================================================
        private static Terrain CreateAndSculptZone2Terrain(Terrain zone1Terrain)
        {
            TerrainData td = AssetDatabase.LoadAssetAtPath<TerrainData>(TerrainDataPath);
            if (td == null)
            {
                td = new TerrainData();
                td.heightmapResolution = 513;
                td.alphamapResolution = 512;
                td.baseMapResolution = 512;
                td.SetDetailResolution(1024, 16);
                td.size = Zone2TerrainSize;
                AssetDatabase.CreateAsset(td, TerrainDataPath);
            }
            else
            {
                td.heightmapResolution = 513;
                td.alphamapResolution = 512;
                td.SetDetailResolution(1024, 16);
                td.size = Zone2TerrainSize;
            }

            if (zone1Terrain != null && zone1Terrain.terrainData.terrainLayers != null && zone1Terrain.terrainData.terrainLayers.Length >= 4)
            {
                td.terrainLayers = zone1Terrain.terrainData.terrainLayers;
            }

            int hRes = td.heightmapResolution;
            float[,] heights = new float[hRes, hRes];

            for (int y = 0; y < hRes; y++)
            {
                float normZ = (float)y / (hRes - 1);
                float worldZ = Zone2TerrainOrigin.z + normZ * Zone2TerrainSize.z;

                for (int x = 0; x < hRes; x++)
                {
                    float normX = (float)x / (hRes - 1);
                    float worldX = Zone2TerrainOrigin.x + normX * Zone2TerrainSize.x;

                    Vector2 worldXZ = new Vector2(worldX, worldZ);

                    float n1 = Mathf.PerlinNoise(worldX * 0.015f + 11.3f, worldZ * 0.015f + 7.9f);
                    float n2 = Mathf.PerlinNoise(worldX * 0.045f + 33.1f, worldZ * 0.045f + 19.4f);
                    float coastWarp = (n1 - 0.5f) * 26f + (n2 - 0.5f) * 10f;

                    float dx = (worldX - IslandCenterXZ.x) / 1.15f;
                    float dz = (worldZ - IslandCenterXZ.y) / 0.95f;
                    float distFromCenter = Mathf.Sqrt(dx * dx + dz * dz) + coastWarp;

                    float h = BaseGroundHeight;

                    // 1. Outer Ocean Dropoff
                    if (distFromCenter > 135f)
                    {
                        float seaT = Mathf.Clamp01((distFromCenter - 135f) / 28f);
                        h = Mathf.Lerp(BaseGroundHeight, 0.4f, Mathf.SmoothStep(0f, 1f, seaT));
                    }
                    // 2. Protective Island Mountain Rim
                    else if (distFromCenter > 92f)
                    {
                        float rimT = Mathf.Sin(Mathf.Clamp01((distFromCenter - 92f) / 43f) * Mathf.PI);
                        float peakH = 22f + n1 * 18f + n2 * 6f;
                        h = BaseGroundHeight + rimT * peakH;
                    }
                    else
                    {
                        h = BaseGroundHeight + (n2 - 0.5f) * 0.65f;
                    }

                    // 3. Sculpt Organic Curved Interior Hills Dividing Central Marsh & Southern Soul Sanctuary
                    if (worldX > 360f && worldX < 505f && worldZ > -56f && worldZ < -18f)
                    {
                        float distToAltarPass = Mathf.Abs(worldX - PosSoulAltar.x);
                        if (distToAltarPass > 8.5f)
                        {
                            float passFactor = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((distToAltarPass - 8.5f) / 18f));
                            float outerXFactor = Mathf.Sin(Mathf.Clamp01((worldX - 360f) / 145f) * Mathf.PI);
                            float ridgeZWidth = 34f + (n1 - 0.5f) * 10f;
                            float zDistFromRidgeCenter = Mathf.Abs(worldZ - (-37f + (n2 - 0.5f) * 6f));
                            float ridgeZFactor = Mathf.SmoothStep(1f, 0f, Mathf.Clamp01(zDistFromRidgeCenter / (ridgeZWidth * 0.5f)));
                            float ridgeH = (13f + n1 * 11f) * passFactor * outerXFactor * ridgeZFactor;
                            h = Mathf.Max(h, BaseGroundHeight + ridgeH);
                        }
                    }

                    // 4. Sculpt Sunken Toxic Swamp Lake Basin at PosSwampLake
                    float distToSwamp = Vector2.Distance(worldXZ, new Vector2(PosSwampLake.x, PosSwampLake.z));
                    if (distToSwamp < 23f)
                    {
                        float swampT = Mathf.SmoothStep(1f, 0f, distToSwamp / 23f);
                        h = Mathf.Lerp(h, 5.4f, swampT);
                    }

                    // 5. Carve Flat Walkable Corridors & Plazas along the Winding Pathways
                    float roadDist = DistanceToIsland2Walkways(worldXZ);
                    if (roadDist < 15f)
                    {
                        float flatFactor = Mathf.SmoothStep(1f, 0f, Mathf.Clamp01((roadDist - 6.0f) / 9.0f));
                        h = Mathf.Lerp(h, BaseGroundHeight, flatFactor);
                    }

                    // 6. Western Ocean Channel
                    if (worldX < 278f)
                    {
                        float bridgeZDist = Mathf.Abs(worldZ);
                        if (worldX < 268f || bridgeZDist > 22f)
                        {
                            float channelT = Mathf.Clamp01((278f - worldX) / 12f);
                            h = Mathf.Lerp(h, 0.4f, channelT);
                        }
                    }

                    // 7. Sculpt Dramatic Canyon Backdrop Behind (+Z) the Thorn Barrier Gate at (535, 0)
                    if (worldX > 518f && worldX < 552f)
                    {
                        float gateZ = Mathf.Abs(worldZ);
                        if (gateZ > 3.5f && gateZ < 32f)
                        {
                            float gorgeWallT = Mathf.Sin(Mathf.Clamp01((gateZ - 3.5f) / 28.5f) * Mathf.PI);
                            float gorgeX = Mathf.Sin(Mathf.Clamp01((worldX - 518f) / 34f) * Mathf.PI);
                            float maxWallH = (worldZ > 0f) ? (18f + n2 * 6f) : 3.2f;
                            h = Mathf.Max(h, BaseGroundHeight + gorgeWallT * gorgeX * maxWallH);
                        }
                    }

                    heights[y, x] = Mathf.Clamp01(h / Zone2TerrainSize.y);
                }
            }

            td.SetHeights(0, 0, heights);

            GameObject terrainGO = Terrain.CreateTerrainGameObject(td);
            terrainGO.name = "Terrain_Zone2_ToxicIsland";
            terrainGO.transform.position = Zone2TerrainOrigin;
            Terrain terrainComp = terrainGO.GetComponent<Terrain>();
            terrainComp.detailObjectDistance = 350f;
            terrainComp.detailObjectDensity = 1.0f;

            EditorUtility.SetDirty(td);
            return terrainComp;
        }

        /// <summary>
        /// Computes distance to the multi-branching organic S-curve trail network on Island 2:
        /// 1. Western Bridge Entry -> Entrance Archway -> Brazier 01 Plaza
        /// 2. Southern Winding Marsh Trail: Brazier 01 -> South of Swamp Lake -> Central Crossroads
        /// 3. Northern Lakeside Scenic Loop: Brazier 01 -> North Shore of Swamp Lake -> Brazier 02
        /// 4. Sanctuary Canyon Trail: Central Crossroads -> Soul Flame Altar Plaza
        /// 5. Eastern Seal Gorge Trail: Central Crossroads -> Brazier 02 Plaza -> Thorn Barrier Gate -> East Shore
        /// </summary>
        private static float DistanceToIsland2Walkways(Vector2 p)
        {
            Vector2 pLanding = new Vector2(278f, 0f);
            Vector2 pBrazier1 = new Vector2(PosBrazier01.x, PosBrazier01.z);

            // Southern Marsh Winding S-Curve
            Vector2 pSouth1 = new Vector2(374f, -15f);
            Vector2 pSouth2 = new Vector2(406f, -17f);
            Vector2 pFork = new Vector2(PosCentralFork.x, PosCentralFork.z);

            // Northern Swamp Lake Scenic Loop
            Vector2 pNorth1 = new Vector2(372f, 28f);
            Vector2 pNorth2 = new Vector2(415f, 47f);
            Vector2 pNorth3 = new Vector2(456f, 34f);

            // Southern Sanctuary Canyon S-Curve
            Vector2 pAltarMid = new Vector2(430f, -38f);
            Vector2 pAltar = new Vector2(PosSoulAltar.x, PosSoulAltar.z);

            // Eastern Gorge Trail
            Vector2 pEastMid1 = new Vector2(460f, 5f);
            Vector2 pBrazier2 = new Vector2(PosBrazier02.x, PosBrazier02.z);
            Vector2 pEastMid2 = new Vector2(510f, 5f);
            Vector2 pGate = new Vector2(PosThornGate.x + 18f, PosThornGate.z);

            float dEntry = DistPointToSegment(p, pLanding, pBrazier1);

            float dSouthA = DistPointToSegment(p, pBrazier1, pSouth1);
            float dSouthB = DistPointToSegment(p, pSouth1, pSouth2);
            float dSouthC = DistPointToSegment(p, pSouth2, pFork);
            float minSouth = Mathf.Min(dSouthA, Mathf.Min(dSouthB, dSouthC));

            float dNorthA = DistPointToSegment(p, pBrazier1, pNorth1);
            float dNorthB = DistPointToSegment(p, pNorth1, pNorth2);
            float dNorthC = DistPointToSegment(p, pNorth2, pNorth3);
            float dNorthD = DistPointToSegment(p, pNorth3, pBrazier2);
            float minNorth = Mathf.Min(Mathf.Min(dNorthA, dNorthB), Mathf.Min(dNorthC, dNorthD));

            float dSanctA = DistPointToSegment(p, pFork, pAltarMid);
            float dSanctB = DistPointToSegment(p, pAltarMid, pAltar);
            float minSanct = Mathf.Min(dSanctA, dSanctB);

            float dEastA = DistPointToSegment(p, pFork, pEastMid1);
            float dEastB = DistPointToSegment(p, pEastMid1, pBrazier2);
            float dEastC = DistPointToSegment(p, pBrazier2, pEastMid2);
            float dEastD = DistPointToSegment(p, pEastMid2, pGate);
            float minEast = Mathf.Min(Mathf.Min(dEastA, dEastB), Mathf.Min(dEastC, dEastD));

            // Circular Plazas at Landmarks
            float dAltarPlaza = Mathf.Max(0f, Vector2.Distance(p, pAltar) - 10.5f);
            float dBrazier1Plaza = Mathf.Max(0f, Vector2.Distance(p, pBrazier1) - 6.8f);
            float dBrazier2Plaza = Mathf.Max(0f, Vector2.Distance(p, pBrazier2) - 6.5f);
            float dForkPlaza = Mathf.Max(0f, Vector2.Distance(p, pFork) - 5.5f);

            float minRoads = Mathf.Min(dEntry, Mathf.Min(Mathf.Min(minSouth, minNorth), Mathf.Min(minSanct, minEast)));
            float minPlazas = Mathf.Min(Mathf.Min(dAltarPlaza, dForkPlaza), Mathf.Min(dBrazier1Plaza, dBrazier2Plaza));
            return Mathf.Min(minRoads, minPlazas);
        }

        private static float DistPointToSegment(Vector2 p, Vector2 a, Vector2 b)
        {
            Vector2 ab = b - a;
            float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / Mathf.Max(0.001f, Vector2.Dot(ab, ab)));
            return Vector2.Distance(p, a + t * ab);
        }

        // =========================================================================
        // STEP 6: PAINT 4 BIOMES & WINDING PATHS ON ZONE 2 ISLAND TERRAIN
        // =========================================================================
        private static void PaintZone2BiomesAndWindingPaths(Terrain terrain)
        {
            TerrainData td = terrain.terrainData;
            int aW = td.alphamapWidth;
            int aH = td.alphamapHeight;
            int numLayers = td.alphamapLayers;
            if (numLayers < 4) return;

            float[,,] splat = new float[aH, aW, numLayers];

            for (int y = 0; y < aH; y++)
            {
                float normZ = (float)y / (aH - 1);
                float worldZ = Zone2TerrainOrigin.z + normZ * Zone2TerrainSize.z;

                for (int x = 0; x < aW; x++)
                {
                    float normX = (float)x / (aW - 1);
                    float worldX = Zone2TerrainOrigin.x + normX * Zone2TerrainSize.x;

                    float height = td.GetInterpolatedHeight(normX, normZ);
                    float steepness = td.GetSteepness(normX, normZ);
                    float noise = Mathf.PerlinNoise(worldX * 0.06f + 5.1f, worldZ * 0.06f + 9.3f);

                    // Layers: [0] Grass, [1] Rock, [2] DirtPath, [3] Sand
                    float wGrass = 0f;
                    float wRock = 0f;
                    float wPath = 0f;
                    float wSand = 0f;

                    if (height < 6.4f)
                    {
                        wSand = 1.0f;
                    }
                    else if (steepness > 20f || height > 12.5f)
                    {
                        wRock = Mathf.Clamp01((steepness - 14f) / 14f + (height - 11f) / 10f);
                        wGrass = 1f - wRock;
                    }
                    else
                    {
                        wGrass = Mathf.Lerp(0.58f, 0.96f, noise);
                        wRock = 1f - wGrass;
                    }

                    // Winding Dirt Roads with Natural Cobblestone/Rock Borders
                    Vector2 wobbleP = new Vector2(worldX + (noise - 0.5f) * 2.0f, worldZ + (noise - 0.5f) * 2.0f);
                    float roadDist = DistanceToIsland2Walkways(wobbleP);
                    if (roadDist < 4.4f && height >= 6.5f && height <= 10.5f)
                    {
                        float pFactor = Mathf.SmoothStep(1f, 0f, roadDist / 4.4f);
                        wPath = pFactor * 0.94f;
                        if (roadDist > 2.7f)
                        {
                            wRock = Mathf.Max(wRock, 0.40f * pFactor);
                        }
                        wGrass *= (1f - pFactor * 0.90f);
                    }

                    float sum = wGrass + wRock + wPath + wSand;
                    if (sum < 0.001f) { wGrass = 1f; sum = 1f; }

                    splat[y, x, 0] = wGrass / sum;
                    splat[y, x, 1] = wRock / sum;
                    splat[y, x, 2] = wPath / sum;
                    splat[y, x, 3] = wSand / sum;
                }
            }

            td.SetAlphamaps(0, 0, splat);
            EditorUtility.SetDirty(td);
        }

        // =========================================================================
        // STEP 7: SPAWN MULTI-LAYER 2.5D SPRITE GRASS, FLOWERS & BUSHES
        // =========================================================================
        private static void SpawnZone2DetailGrass(Terrain zone2, Terrain zone1)
        {
            TerrainData td = zone2.terrainData;
            if (zone1 != null && zone1.terrainData.detailPrototypes != null && zone1.terrainData.detailPrototypes.Length > 0)
            {
                td.detailPrototypes = zone1.terrainData.detailPrototypes;
            }

            int numPrototypes = td.detailPrototypes.Length;
            if (numPrototypes == 0) return;

            int dRes = td.detailResolution;
            int aW = td.alphamapWidth;
            int aH = td.alphamapHeight;
            float[,,] alphas = td.GetAlphamaps(0, 0, aW, aH);

            int[][,] detailMaps = new int[numPrototypes][,];
            for (int i = 0; i < numPrototypes; i++)
                detailMaps[i] = new int[dRes, dRes];

            int[] baseGrassLayers = { 0, 5, 8, 12, 16, 17, 19 };
            int[] flowerLayers = { 2, 6, 10, 14, 18 };
            int[] reedLayers = { 4, 7, 11, 15 };
            int[] bushLayers = { 1, 3, 9, 13 };
            var rand = new System.Random(1337);

            for (int y = 0; y < dRes; y++)
            {
                float normZ = (float)y / (dRes - 1);
                int ay = Mathf.Clamp((int)(normZ * aH), 0, aH - 1);

                for (int x = 0; x < dRes; x++)
                {
                    float normX = (float)x / (dRes - 1);
                    int ax = Mathf.Clamp((int)(normX * aW), 0, aW - 1);

                    float h = td.GetInterpolatedHeight(normX, normZ);
                    if (h < 7.1f || h > 36f) continue;

                    float steep = td.GetSteepness(normX, normZ);
                    if (steep > 28f) continue;

                    float wGrass = alphas[ay, ax, 0];
                    float wRock = alphas[ay, ax, 1];
                    float wPath = alphas[ay, ax, 2];
                    if (wPath > 0.25f) continue;

                    float macroNoise = Mathf.PerlinNoise(normX * 14f + 3.1f, normZ * 14f + 8.2f);
                    float flowerCluster = Mathf.PerlinNoise(normX * 26f + 19.3f, normZ * 26f + 41.7f);
                    float bushCluster = Mathf.PerlinNoise(normX * 18f + 61.1f, normZ * 18f + 23.4f);

                    if (wGrass > 0.20f)
                    {
                        int baseProto = baseGrassLayers[((x / 3) + (y / 3)) % baseGrassLayers.Length] % numPrototypes;
                        int density = (int)(Mathf.Lerp(130f, 255f, macroNoise) * wGrass);
                        detailMaps[baseProto][y, x] = Mathf.Clamp(density, 0, 255);

                        if (macroNoise > 0.45f)
                        {
                            int reedProto = reedLayers[((x / 4) + (y / 4)) % reedLayers.Length] % numPrototypes;
                            detailMaps[reedProto][y, x] = Mathf.Clamp((int)(macroNoise * 190f), 0, 255);
                        }
                    }

                    if (wGrass > 0.32f && flowerCluster > 0.50f)
                    {
                        int fProto = flowerLayers[((int)(normX * 18f) + (int)(normZ * 18f)) % flowerLayers.Length] % numPrototypes;
                        detailMaps[fProto][y, x] = Mathf.Clamp((int)((flowerCluster - 0.50f) * 420f), 0, 255);
                    }

                    if (bushCluster > 0.60f && (wRock > 0.12f || h > 8.5f))
                    {
                        int bProto = bushLayers[rand.Next(bushLayers.Length)] % numPrototypes;
                        detailMaps[bProto][y, x] = Mathf.Clamp((int)((bushCluster - 0.60f) * 320f), 0, 220);
                    }
                }
            }

            for (int i = 0; i < numPrototypes; i++)
                td.SetDetailLayer(0, 0, i, detailMaps[i]);

            EditorUtility.SetDirty(td);
        }

        // =========================================================================
        // STEP 9: PROBUILDER INTER-ISLAND BRIDGE (ZONE 1 -> ZONE 2)
        // =========================================================================
        private static void BuildInterIslandBridge(
            Transform parent,
            Material stoneCarved,
            Material stoneMossy,
            Material obsidian,
            Material woodMat,
            Material ironMat,
            Material bronzeMat,
            Material cyanGlow)
        {
            GameObject bridgeRoot = new GameObject("Bridge_Zone1_To_Zone2");
            bridgeRoot.transform.SetParent(parent, false);
            bridgeRoot.transform.position = new Vector3(252f, BaseGroundHeight, 0f);

            List<ProBuilderMesh> parts = new List<ProBuilderMesh>();
            float startX = BridgeStartZone1.x;
            float endX = BridgeEndZone2.x;
            float length = endX - startX;
            float centerX = (startX + endX) * 0.5f;
            float deckY = BaseGroundHeight;
            float bridgeWidth = 6.4f;

            float[] pierXs = { 224f, 238f, 252f, 266f, 280f };
            foreach (float px in pierXs)
            {
                for (int side = -1; side <= 1; side += 2)
                {
                    float pz = side * (bridgeWidth * 0.5f + 0.35f);
                    AddPart(parts, ShapeGenerator.GenerateCube(PivotLocation.Center, new Vector3(1.8f, 8.6f, 1.4f)), new Vector3(px, deckY - 3.8f, pz), Quaternion.identity, stoneMossy);
                    AddPart(parts, ShapeGenerator.GenerateCube(PivotLocation.Center, new Vector3(1.1f, 2.2f, 1.1f)), new Vector3(px, deckY + 1.1f, pz), Quaternion.identity, stoneCarved);
                    AddPart(parts, ShapeGenerator.GenerateCube(PivotLocation.Center, new Vector3(1.25f, 0.24f, 1.25f)), new Vector3(px, deckY + 2.25f, pz), Quaternion.identity, obsidian);
                    AddPart(parts, ShapeGenerator.GenerateCylinder(PivotLocation.Center, 8, 0.28f, 0.55f, 1, 1), new Vector3(px, deckY + 2.65f, pz), Quaternion.identity, cyanGlow);
                    AddPart(parts, ShapeGenerator.GenerateCone(PivotLocation.Center, 0.45f, 0.45f, 4), new Vector3(px, deckY + 3.10f, pz), Quaternion.Euler(0f, 45f, 0f), stoneCarved);
                }
            }

            for (int side = -1; side <= 1; side += 2)
            {
                float curbZ = side * (bridgeWidth * 0.5f);
                AddPart(parts, ShapeGenerator.GenerateCube(PivotLocation.Center, new Vector3(length, 0.65f, 0.55f)), new Vector3(centerX, deckY - 0.1f, curbZ), Quaternion.identity, stoneCarved);
                AddPart(parts, ShapeGenerator.GenerateCube(PivotLocation.Center, new Vector3(length, 0.14f, 0.14f)), new Vector3(centerX, deckY + 1.15f, curbZ), Quaternion.identity, ironMat);
                AddPart(parts, ShapeGenerator.GenerateCube(PivotLocation.Center, new Vector3(length, 0.10f, 0.10f)), new Vector3(centerX, deckY + 0.65f, curbZ), Quaternion.identity, bronzeMat);
            }

            int plankCount = 48;
            float plankStep = length / plankCount;
            for (int i = 0; i < plankCount; i++)
            {
                float px = startX + (i + 0.5f) * plankStep;
                AddPart(parts, ShapeGenerator.GenerateCube(PivotLocation.Center, new Vector3(plankStep * 0.88f, 0.22f, bridgeWidth - 0.4f)),
                    new Vector3(px, deckY - 0.06f, 0f), Quaternion.identity, (i % 5 == 0) ? stoneMossy : woodMat);
            }

            CombineAllParts(parts, bridgeRoot, "Bridge_ProBuilder_Mesh");

            BoxCollider floorCol = bridgeRoot.AddComponent<BoxCollider>();
            floorCol.center = new Vector3(0f, -0.25f, 0f);
            floorCol.size = new Vector3(length + 4f, 0.5f, bridgeWidth);

            for (int side = -1; side <= 1; side += 2)
            {
                GameObject railColObj = new GameObject($"Bridge_Guardrail_{(side < 0 ? "South" : "North")}");
                railColObj.transform.SetParent(bridgeRoot.transform, false);
                BoxCollider rCol = railColObj.AddComponent<BoxCollider>();
                rCol.center = new Vector3(0f, 1.0f, side * (bridgeWidth * 0.5f + 0.2f));
                rCol.size = new Vector3(length, 2.2f, 0.5f);
            }
        }

        // =========================================================================
        // STEP 10: ORNATE RUIN ARCHWAYS & SOUL SANCTUARY COLONNADE
        // =========================================================================
        private static void BuildEntranceRuinArchway(
            Transform parent,
            Vector3 centerPos,
            Material stoneCarved,
            Material stoneMossy,
            Material obsidian,
            Material cyanGlow)
        {
            GameObject archRoot = new GameObject("Zone2_Entrance_RuinArchway");
            archRoot.transform.SetParent(parent, false);
            archRoot.transform.position = centerPos;

            List<ProBuilderMesh> parts = new List<ProBuilderMesh>();
            float halfSpan = 3.8f;

            for (int side = -1; side <= 1; side += 2)
            {
                Vector3 colBase = centerPos + new Vector3(0f, 0f, side * halfSpan);
                AddPart(parts, ShapeGenerator.GenerateCube(PivotLocation.Center, new Vector3(1.8f, 0.5f, 1.8f)), colBase + new Vector3(0f, 0.25f, 0f), Quaternion.identity, stoneMossy);
                AddPart(parts, ShapeGenerator.GenerateCube(PivotLocation.Center, new Vector3(1.4f, 0.4f, 1.4f)), colBase + new Vector3(0f, 0.70f, 0f), Quaternion.identity, stoneCarved);
                AddPart(parts, ShapeGenerator.GenerateCube(PivotLocation.Center, new Vector3(1.1f, 3.8f, 1.1f)), colBase + new Vector3(0f, 2.80f, 0f), Quaternion.identity, stoneCarved);
                AddPart(parts, ShapeGenerator.GenerateCube(PivotLocation.Center, new Vector3(1.5f, 0.45f, 1.5f)), colBase + new Vector3(0f, 4.90f, 0f), Quaternion.identity, obsidian);
                AddPart(parts, ShapeGenerator.GenerateCube(PivotLocation.Center, new Vector3(1.14f, 2.6f, 0.14f)), colBase + new Vector3(0f, 2.80f, 0f), Quaternion.identity, cyanGlow);
            }

            AddPart(parts, ShapeGenerator.GenerateCube(PivotLocation.Center, new Vector3(1.3f, 0.85f, halfSpan * 2f + 2.4f)), centerPos + new Vector3(0f, 5.50f, 0f), Quaternion.identity, stoneMossy);
            AddPart(parts, ShapeGenerator.GenerateCube(PivotLocation.Center, new Vector3(1.4f, 0.22f, halfSpan * 2f + 1.2f)), centerPos + new Vector3(0f, 5.50f, 0f), Quaternion.identity, cyanGlow);

            CombineAllParts(parts, archRoot, "Entrance_Archway_Mesh");
        }

        private static void BuildSoulSanctuaryColonnade(
            Transform parent,
            Vector3 altarCenter,
            Material stoneCarved,
            Material stoneMossy,
            Material obsidian,
            Material cyanGlow)
        {
            GameObject sanctRoot = new GameObject("SoulSanctuary_AncientColonnade");
            sanctRoot.transform.SetParent(parent, false);
            sanctRoot.transform.position = altarCenter;

            List<ProBuilderMesh> parts = new List<ProBuilderMesh>();

            AddPart(parts, ShapeGenerator.GenerateCylinder(PivotLocation.Center, 16, 6.2f, 0.12f, 1, 1), altarCenter + new Vector3(0f, 0.04f, 0f), Quaternion.identity, stoneMossy);
            AddPart(parts, ShapeGenerator.GeneratePipe(PivotLocation.Center, 5.8f, 0.16f, 0.45f, 16, 1), altarCenter + new Vector3(0f, 0.08f, 0f), Quaternion.identity, obsidian);
            AddPart(parts, ShapeGenerator.GeneratePipe(PivotLocation.Center, 5.2f, 0.18f, 0.12f, 16, 1), altarCenter + new Vector3(0f, 0.09f, 0f), Quaternion.identity, cyanGlow);

            // 6 Ancient Ruin Columns flanking East & West of the Altar (leaving both North +Z entry and South -Z 2.5D Camera sightline open!)
            float[] angles = { 65f, 100f, 135f, 225f, 260f, 295f };
            float radius = 8.2f;
            for (int i = 0; i < angles.Length; i++)
            {
                float rad = angles[i] * Mathf.Deg2Rad;
                Vector3 cPos = altarCenter + new Vector3(Mathf.Sin(rad) * radius, 0f, Mathf.Cos(rad) * radius);
                Quaternion cRot = Quaternion.Euler(0f, angles[i], 0f);
                float h = (i % 2 == 0) ? 3.8f : 2.9f;

                AddPart(parts, ShapeGenerator.GenerateCylinder(PivotLocation.Center, 8, 0.85f, 0.45f, 1, 1), cPos + new Vector3(0f, 0.22f, 0f), cRot, stoneMossy);
                AddPart(parts, ShapeGenerator.GenerateCylinder(PivotLocation.Center, 8, 0.62f, h, 1, 1), cPos + new Vector3(0f, 0.45f + h * 0.5f, 0f), cRot, stoneCarved);
                AddPart(parts, ShapeGenerator.GeneratePipe(PivotLocation.Center, 0.66f, 0.18f, 0.08f, 8, 1), cPos + new Vector3(0f, 1.5f, 0f), cRot, cyanGlow);
                AddPart(parts, ShapeGenerator.GenerateCylinder(PivotLocation.Center, 8, 0.78f, 0.35f, 1, 1), cPos + new Vector3(0f, 0.45f + h + 0.15f, 0f), cRot, obsidian);

                GameObject colObj = new GameObject($"Sanctuary_Pillar_Col_{i}");
                colObj.transform.SetParent(sanctRoot.transform, false);
                colObj.transform.position = cPos;
                BoxCollider bCol = colObj.AddComponent<BoxCollider>();
                bCol.center = new Vector3(0f, 1.8f, 0f);
                bCol.size = new Vector3(1.5f, 3.6f, 1.5f);
            }

            CombineAllParts(parts, sanctRoot, "Sanctuary_Colonnade_Mesh");
        }

        private static void BuildGorgeSealButtresses(
            Transform parent,
            Vector3 gateCenter,
            Material stoneCarved,
            Material stoneMossy,
            Material obsidian,
            Material cyanGlow)
        {
            GameObject buttressRoot = new GameObject("ThornGate_Gorge_Buttresses");
            buttressRoot.transform.SetParent(parent, false);
            buttressRoot.transform.position = gateCenter;

            List<ProBuilderMesh> parts = new List<ProBuilderMesh>();

            for (int side = -1; side <= 1; side += 2)
            {
                for (int step = 0; step < 3; step++)
                {
                    float zPos = side * (3.4f + step * 2.2f);
                    float height = 4.2f + step * 1.4f;
                    Vector3 bPos = gateCenter + new Vector3(0f, height * 0.5f, zPos);

                    AddPart(parts,
                        ShapeGenerator.GenerateCube(PivotLocation.Center, new Vector3(2.4f - step * 0.2f, height, 2.4f)),
                        bPos,
                        Quaternion.Euler(0f, step * 5f * side, 0f),
                        (step % 2 == 0) ? stoneMossy : obsidian);
                }

                GameObject blockCol = new GameObject($"Gate_WingCollider_{(side < 0 ? "South" : "North")}");
                blockCol.transform.SetParent(buttressRoot.transform, false);
                BoxCollider bc = blockCol.AddComponent<BoxCollider>();
                bc.center = new Vector3(0f, 3.0f, side * 6.5f);
                bc.size = new Vector3(2.5f, 6.0f, 8.5f);
            }

            CombineAllParts(parts, buttressRoot, "Gorge_Buttresses_Mesh");
        }

        // =========================================================================
        // STEP 11: PROBUILDER PUZZLE PROPS (BRAZIER, SOUL ALTAR, THORN GATE)
        // =========================================================================
        private static GameObject BuildTripodBrazier(
            string objectName,
            Vector3 worldPos,
            BrazierRole role,
            bool startLit,
            Material stoneCarved,
            Material stoneMossy,
            Material forgedIron,
            Material antiqueBronze,
            Material woodMat,
            Material darkPit,
            Material emberGlow,
            Material flameGolden)
        {
            GameObject brazierRoot = new GameObject(objectName);
            brazierRoot.transform.position = worldPos;

            List<ProBuilderMesh> parts = new List<ProBuilderMesh>();
            AddPart(parts, ShapeGenerator.GenerateCylinder(PivotLocation.Center, 12, 0.85f, 0.10f, 1, 1), worldPos + new Vector3(0f, 0.05f, 0f), Quaternion.identity, stoneMossy);

            for (int i = 0; i < 8; i++)
            {
                float deg = i * 45f;
                float rad = deg * Mathf.Deg2Rad;
                AddPart(parts, ShapeGenerator.GenerateCube(PivotLocation.Center, new Vector3(0.40f, 0.10f, 0.36f)),
                    worldPos + new Vector3(Mathf.Sin(rad) * 0.60f, 0.06f, Mathf.Cos(rad) * 0.60f),
                    Quaternion.Euler(0f, deg, 0f), (i % 2 == 0) ? stoneCarved : stoneMossy);
            }

            AddPart(parts, ShapeGenerator.GeneratePipe(PivotLocation.Center, 0.48f, 0.06f, 0.06f, 16, 1), worldPos + new Vector3(0f, 0.44f, 0f), Quaternion.identity, forgedIron);
            AddPart(parts, ShapeGenerator.GeneratePipe(PivotLocation.Center, 0.42f, 0.06f, 0.06f, 16, 1), worldPos + new Vector3(0f, 0.70f, 0f), Quaternion.identity, forgedIron);

            for (int leg = 0; leg < 3; leg++)
            {
                float legAngle = leg * 120f + 30f;
                Quaternion legRot = Quaternion.Euler(0f, legAngle, 0f);
                Vector3 radialDir = legRot * Vector3.forward;

                AddPart(parts, ShapeGenerator.GenerateCube(PivotLocation.Center, new Vector3(0.09f, 0.42f, 0.10f)), worldPos + radialDir * 0.52f + new Vector3(0f, 0.35f, 0f), legRot * Quaternion.Euler(-30f, 0f, 0f), forgedIron);
                AddPart(parts, ShapeGenerator.GenerateCube(PivotLocation.Center, new Vector3(0.09f, 0.48f, 0.10f)), worldPos + radialDir * 0.50f + new Vector3(0f, 0.92f, 0f), legRot * Quaternion.Euler(24f, 0f, 0f), forgedIron);
                AddPart(parts, ShapeGenerator.GenerateCube(PivotLocation.Center, new Vector3(0.12f, 0.08f, 0.13f)), worldPos + radialDir * 0.42f + new Vector3(0f, 0.70f, 0f), legRot, antiqueBronze);
            }

            AddPart(parts, ShapeGenerator.GenerateCylinder(PivotLocation.Center, 12, 0.42f, 0.08f, 1, 1), worldPos + new Vector3(0f, 0.75f, 0f), Quaternion.identity, forgedIron);
            for (int r = 0; r < 12; r++)
            {
                float ribAngle = r * 30f;
                Quaternion ribRot = Quaternion.Euler(0f, ribAngle, 0f);
                Vector3 ribDir = ribRot * Vector3.forward;
                AddPart(parts, ShapeGenerator.GenerateCube(PivotLocation.Center, new Vector3(0.05f, 0.45f, 0.05f)), worldPos + ribDir * 0.48f + new Vector3(0f, 0.95f, 0f), ribRot * Quaternion.Euler(22f, 0f, 0f), forgedIron);
            }

            AddPart(parts, ShapeGenerator.GeneratePipe(PivotLocation.Center, 0.60f, 0.10f, 0.07f, 16, 1), worldPos + new Vector3(0f, 1.16f, 0f), Quaternion.identity, antiqueBronze);
            AddPart(parts, ShapeGenerator.GenerateCylinder(PivotLocation.Center, 10, 0.42f, 0.16f, 1, 1), worldPos + new Vector3(0f, 0.85f, 0f), Quaternion.identity, darkPit);

            for (int l = 0; l < 4; l++)
            {
                float yaw = l * 85f + 15f;
                Quaternion logRot = Quaternion.Euler(58f, yaw, 0f);
                Vector3 logOffset = Quaternion.Euler(0f, yaw, 0f) * new Vector3(0f, 1.08f, 0.06f);
                AddPart(parts, ShapeGenerator.GenerateCylinder(PivotLocation.Center, 8, 0.075f, 0.58f, 1, 1), worldPos + logOffset, logRot, woodMat);
            }

            CombineAllParts(parts, brazierRoot, "Brazier_Tripod_Structure");

            BoxCollider rootCol = brazierRoot.AddComponent<BoxCollider>();
            rootCol.center = new Vector3(0f, 0.65f, 0f);
            rootCol.size = new Vector3(1.4f, 1.3f, 1.4f);

            GameObject flameObj = new GameObject("Flame_Visual");
            flameObj.transform.SetParent(brazierRoot.transform, false);
            List<ProBuilderMesh> flameParts = new List<ProBuilderMesh>();
            AddPart(flameParts, ShapeGenerator.GenerateCone(PivotLocation.Center, 0.22f, 0.72f, 6), worldPos + new Vector3(0f, 1.45f, 0f), Quaternion.identity, flameGolden);
            for (int f = 0; f < 5; f++)
            {
                float fRad = (f * 72f) * Mathf.Deg2Rad;
                AddPart(flameParts, ShapeGenerator.GenerateCone(PivotLocation.Center, 0.15f, 0.52f, 5),
                    worldPos + new Vector3(Mathf.Sin(fRad) * 0.15f, 1.36f, Mathf.Cos(fRad) * 0.15f),
                    Quaternion.Euler(0f, f * 72f, 0f) * Quaternion.Euler(-14f, 0f, 12f),
                    (f % 2 == 0) ? emberGlow : flameGolden);
            }
            CombineAllParts(flameParts, flameObj, "Campfire_3D_Flame");

            GameObject lightObj = new GameObject("Brazier_Light");
            lightObj.transform.SetParent(brazierRoot.transform, false);
            lightObj.transform.localPosition = new Vector3(0f, 1.65f, 0f);
            Light pLight = lightObj.AddComponent<Light>();
            pLight.type = LightType.Point;
            pLight.color = new Color(1.0f, 0.68f, 0.22f);
            pLight.intensity = 3.5f;
            pLight.range = 8.0f;

            AncientBrazier brazierComp = brazierRoot.AddComponent<AncientBrazier>();
            SerializedObject so = new SerializedObject(brazierComp);
            so.FindProperty("brazierRole").enumValueIndex = (int)role;
            so.FindProperty("startLit").boolValue = startLit;
            so.FindProperty("interactRadius").floatValue = 2.6f;
            so.FindProperty("safeRadius").floatValue = 4.0f;
            so.FindProperty("flameVisual").objectReferenceValue = flameObj;
            so.FindProperty("brazierLight").objectReferenceValue = pLight;
            so.ApplyModifiedProperties();

            flameObj.SetActive(startLit);
            pLight.enabled = startLit;
            return brazierRoot;
        }

        private static GameObject BuildMonumentalSoulAltar(
            string objectName,
            Vector3 worldPos,
            BrazierRole role,
            bool startLit,
            Material obsidianStone,
            Material stoneCarved,
            Material stoneMossy,
            Material darkPit,
            Material runeCyanGlow,
            Material soulCoreGlow)
        {
            GameObject altarRoot = new GameObject(objectName);
            altarRoot.transform.position = worldPos;

            List<ProBuilderMesh> parts = new List<ProBuilderMesh>();
            AddPart(parts, ShapeGenerator.GenerateCylinder(PivotLocation.Center, 8, 1.15f, 0.18f, 1, 1), worldPos + new Vector3(0f, 0.09f, 0f), Quaternion.identity, obsidianStone);
            AddPart(parts, ShapeGenerator.GeneratePipe(PivotLocation.Center, 0.98f, 0.03f, 0.06f, 16, 1), worldPos + new Vector3(0f, 0.19f, 0f), Quaternion.identity, runeCyanGlow);
            AddPart(parts, ShapeGenerator.GenerateCylinder(PivotLocation.Center, 8, 0.85f, 0.20f, 1, 1), worldPos + new Vector3(0f, 0.28f, 0f), Quaternion.identity, obsidianStone);
            AddPart(parts, ShapeGenerator.GeneratePipe(PivotLocation.Center, 0.72f, 0.03f, 0.05f, 16, 1), worldPos + new Vector3(0f, 0.39f, 0f), Quaternion.identity, runeCyanGlow);
            AddPart(parts, ShapeGenerator.GenerateCylinder(PivotLocation.Center, 8, 0.60f, 0.16f, 1, 1), worldPos + new Vector3(0f, 0.46f, 0f), Quaternion.identity, stoneCarved);
            AddPart(parts, ShapeGenerator.GenerateCylinder(PivotLocation.Center, 8, 0.42f, 0.60f, 1, 1), worldPos + new Vector3(0f, 0.82f, 0f), Quaternion.identity, obsidianStone);

            for (int i = 0; i < 8; i++)
            {
                float deg = i * 45f + 22.5f;
                float rad = deg * Mathf.Deg2Rad;
                AddPart(parts, ShapeGenerator.GenerateCube(PivotLocation.Center, new Vector3(0.055f, 0.50f, 0.04f)),
                    worldPos + new Vector3(Mathf.Sin(rad) * 0.40f, 0.82f, Mathf.Cos(rad) * 0.40f),
                    Quaternion.Euler(0f, deg, 0f), runeCyanGlow);
            }

            AddPart(parts, ShapeGenerator.GeneratePipe(PivotLocation.Center, 0.82f, 0.28f, 0.16f, 8, 1), worldPos + new Vector3(0f, 1.28f, 0f), Quaternion.identity, obsidianStone);
            AddPart(parts, ShapeGenerator.GeneratePipe(PivotLocation.Center, 0.76f, 0.04f, 0.06f, 16, 1), worldPos + new Vector3(0f, 1.43f, 0f), Quaternion.identity, runeCyanGlow);
            AddPart(parts, ShapeGenerator.GenerateCylinder(PivotLocation.Center, 8, 0.64f, 0.12f, 1, 1), worldPos + new Vector3(0f, 1.28f, 0f), Quaternion.identity, darkPit);

            CombineAllParts(parts, altarRoot, "Altar_Monument_Structure");

            BoxCollider rootCol = altarRoot.AddComponent<BoxCollider>();
            rootCol.center = new Vector3(0f, 0.75f, 0f);
            rootCol.size = new Vector3(1.8f, 1.5f, 1.8f);

            GameObject flameObj = new GameObject("Flame_Visual");
            flameObj.transform.SetParent(altarRoot.transform, false);
            List<ProBuilderMesh> flameParts = new List<ProBuilderMesh>();
            AddPart(flameParts, ShapeGenerator.GenerateCone(PivotLocation.Center, 0.26f, 0.90f, 6), worldPos + new Vector3(0f, 1.75f, 0f), Quaternion.identity, soulCoreGlow);
            for (int t = 0; t < 6; t++)
            {
                float tRad = (t * 60f) * Mathf.Deg2Rad;
                AddPart(flameParts, ShapeGenerator.GenerateCone(PivotLocation.Center, 0.17f, 0.65f, 5),
                    worldPos + new Vector3(Mathf.Sin(tRad) * 0.19f, 1.62f, Mathf.Cos(tRad) * 0.19f),
                    Quaternion.Euler(0f, t * 60f, 0f) * Quaternion.Euler(-16f, 0f, 18f),
                    (t % 2 == 0) ? runeCyanGlow : soulCoreGlow);
            }
            CombineAllParts(flameParts, flameObj, "SoulFlame_3D_Swirl");

            GameObject lightObj = new GameObject("Brazier_Light");
            lightObj.transform.SetParent(altarRoot.transform, false);
            lightObj.transform.localPosition = new Vector3(0f, 1.95f, 0f);
            Light pLight = lightObj.AddComponent<Light>();
            pLight.type = LightType.Point;
            pLight.color = new Color(0.15f, 0.95f, 1.0f);
            pLight.intensity = 4.2f;
            pLight.range = 12.0f;

            AncientBrazier brazierComp = altarRoot.AddComponent<AncientBrazier>();
            SerializedObject so = new SerializedObject(brazierComp);
            so.FindProperty("brazierRole").enumValueIndex = (int)role;
            so.FindProperty("startLit").boolValue = startLit;
            so.FindProperty("interactRadius").floatValue = 2.8f;
            so.FindProperty("safeRadius").floatValue = 4.5f;
            so.FindProperty("flameVisual").objectReferenceValue = flameObj;
            so.FindProperty("brazierLight").objectReferenceValue = pLight;
            so.ApplyModifiedProperties();

            flameObj.SetActive(startLit);
            pLight.enabled = startLit;
            return altarRoot;
        }

        private static GameObject BuildTwinObeliskThornGate(
            string objectName,
            Vector3 worldPos,
            Material stoneCarved,
            Material stoneMossy,
            Material obsidianStone,
            Material darkWood,
            Material spikeBronzeIron,
            Material forgedIron,
            Material antiqueBronze,
            Material runeCyanGlow)
        {
            GameObject gateRoot = new GameObject(objectName);
            gateRoot.transform.position = worldPos;

            List<ProBuilderMesh> parts = new List<ProBuilderMesh>();
            float[] zSides = { 2.25f, -2.25f };
            foreach (float zCenter in zSides)
            {
                Vector3 pBase = worldPos + new Vector3(0f, 0f, zCenter);
                AddPart(parts, ShapeGenerator.GenerateCube(PivotLocation.Center, new Vector3(1.15f, 0.28f, 1.15f)), pBase + new Vector3(0f, 0.14f, 0f), Quaternion.identity, stoneMossy);
                AddPart(parts, ShapeGenerator.GenerateCube(PivotLocation.Center, new Vector3(0.95f, 0.24f, 0.95f)), pBase + new Vector3(0f, 0.40f, 0f), Quaternion.identity, stoneCarved);
                AddPart(parts, ShapeGenerator.GenerateCube(PivotLocation.Center, new Vector3(0.78f, 1.35f, 0.78f)), pBase + new Vector3(0f, 1.18f, 0f), Quaternion.identity, stoneCarved);
                AddPart(parts, ShapeGenerator.GenerateCube(PivotLocation.Center, new Vector3(0.88f, 0.16f, 0.88f)), pBase + new Vector3(0f, 1.92f, 0f), Quaternion.identity, obsidianStone);
                AddPart(parts, ShapeGenerator.GenerateCube(PivotLocation.Center, new Vector3(0.70f, 1.25f, 0.70f)), pBase + new Vector3(0f, 2.62f, 0f), Quaternion.identity, stoneCarved);
                AddPart(parts, ShapeGenerator.GenerateCone(PivotLocation.Center, 0.56f, 0.70f, 4), pBase + new Vector3(0f, 3.60f, 0f), Quaternion.Euler(0f, 45f, 0f), stoneCarved);
                AddPart(parts, ShapeGenerator.GenerateCube(PivotLocation.Center, new Vector3(0.04f, 1.8f, 0.08f)), pBase + new Vector3(-0.40f, 1.90f, 0f), Quaternion.identity, runeCyanGlow);
            }

            float[] vineY = { 0.38f, 0.75f, 1.12f, 1.50f, 1.88f, 2.25f, 2.62f };
            for (int v = 0; v < vineY.Length; v++)
            {
                AddPart(parts, ShapeGenerator.GenerateCube(PivotLocation.Center, new Vector3(0.24f, 0.24f, 4.0f)),
                    worldPos + new Vector3(0f, vineY[v], 0f),
                    Quaternion.Euler((v % 2 == 0) ? -9f : 9f, 0f, 0f), darkWood);
            }

            float[] spikeZ = { -1.6f, -1.2f, -0.8f, 0f, 0.8f, 1.2f, 1.6f };
            for (int s = 0; s < spikeZ.Length; s++)
            {
                AddPart(parts, ShapeGenerator.GenerateCone(PivotLocation.Center, 0.12f, 0.68f, 6),
                    worldPos + new Vector3(-0.36f, 0.8f + (s % 2) * 0.9f, spikeZ[s]),
                    Quaternion.Euler(0f, 0f, 95f), spikeBronzeIron);
                AddPart(parts, ShapeGenerator.GenerateCone(PivotLocation.Center, 0.12f, 0.68f, 6),
                    worldPos + new Vector3(0f, 2.95f, spikeZ[s]),
                    Quaternion.identity, forgedIron);
            }

            Vector3 sealCenter = worldPos + new Vector3(0f, 1.60f, 0f);
            Quaternion discRot = Quaternion.Euler(0f, 0f, 90f);
            AddPart(parts, ShapeGenerator.GeneratePipe(PivotLocation.Center, 0.78f, 0.46f, 0.13f, 16, 1), sealCenter, discRot, antiqueBronze);
            AddPart(parts, ShapeGenerator.GenerateCylinder(PivotLocation.Center, 16, 0.66f, 0.42f, 1, 1), sealCenter, discRot, obsidianStone);
            AddPart(parts, ShapeGenerator.GeneratePipe(PivotLocation.Center, 0.58f, 0.48f, 0.05f, 16, 1), sealCenter, discRot, runeCyanGlow);
            AddPart(parts, ShapeGenerator.GenerateCylinder(PivotLocation.Center, 8, 0.16f, 0.50f, 1, 1), sealCenter, discRot, runeCyanGlow);

            CombineAllParts(parts, gateRoot, "Thorn_Gate_Structure");

            BoxCollider gateCol = gateRoot.AddComponent<BoxCollider>();
            gateCol.center = new Vector3(0f, 1.80f, 0f);
            gateCol.size = new Vector3(1.1f, 3.60f, 5.40f);

            ThornBarrierGate gateComp = gateRoot.AddComponent<ThornBarrierGate>();
            SerializedObject so = new SerializedObject(gateComp);
            so.FindProperty("interactRadius").floatValue = 3.2f;
            so.FindProperty("burnDuration").floatValue = 1.5f;
            so.ApplyModifiedProperties();

            return gateRoot;
        }

        // =========================================================================
        // STEP 12: TOXIC SWAMP WATER & MULTI-LAYER POISON GAS VFX
        // =========================================================================
        private static void SetupSwampWaterAndPoisonMist(Transform lightsParent, Transform propsParent, Terrain zone2)
        {
            GameObject swampWater = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            swampWater.name = "Zone2_ToxicSwamp_Water";
            swampWater.transform.SetParent(lightsParent, false);
            swampWater.transform.position = PosSwampLake;
            swampWater.transform.localScale = new Vector3(44f, 0.08f, 44f);
            if (swampWater.GetComponent<Collider>() != null)
                Object.DestroyImmediate(swampWater.GetComponent<Collider>());

            Material waterMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/03_Art/Materials/Water/RealisticWater_Mat.mat");
            if (waterMat != null)
                swampWater.GetComponent<MeshRenderer>().sharedMaterial = waterMat;

            GameObject gasPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/04_Prefabs/Environment/Poison_Gas_Zone.prefab");
            GameObject gasZone;
            Vector3 gasCenter = new Vector3(440f, BaseGroundHeight + 0.05f, -15f);
            Vector3 gasSize = new Vector3(185f, 7.5f, 155f);

            if (gasPrefab != null)
            {
                gasZone = (GameObject)PrefabUtility.InstantiatePrefab(gasPrefab, propsParent);
                gasZone.name = "Poison_Gas_Zone";
                gasZone.transform.position = gasCenter;

                BoxCollider bc = gasZone.GetComponent<BoxCollider>();
                if (bc != null)
                {
                    bc.center = new Vector3(0f, gasSize.y * 0.5f, 0f);
                    bc.size = gasSize;
                }

                Transform mistPlane = gasZone.transform.Find("Toxic_Mist_GroundPlane");
                if (mistPlane != null)
                {
                    mistPlane.gameObject.SetActive(false);
                }

                foreach (ParticleSystem ps in gasZone.GetComponentsInChildren<ParticleSystem>(true))
                {
                    var shape = ps.shape;
                    shape.scale = new Vector3(135f, 115f, 2.5f);
                    ps.Simulate(2.5f, true, true);
                    ps.Play(true);
                }
            }
            else
            {
                gasZone = new GameObject("Poison_Gas_Zone");
                gasZone.transform.SetParent(propsParent, false);
                gasZone.transform.position = gasCenter;
                BoxCollider bc = gasZone.AddComponent<BoxCollider>();
                bc.isTrigger = true;
                bc.center = new Vector3(0f, gasSize.y * 0.5f, 0f);
                bc.size = gasSize;
                gasZone.AddComponent<PoisonGasZone>();
            }

            CreateAtmosLight(lightsParent, "Light_BridgeArrival", new Vector3(282f, 11.5f, 0f), new Color(0.3f, 0.85f, 1.0f), 3.0f, 18f);
            CreateAtmosLight(lightsParent, "Light_ToxicSwampMist", new Vector3(415f, 11.0f, 18f), new Color(0.55f, 0.22f, 0.90f), 3.8f, 28f);
            CreateAtmosLight(lightsParent, "Light_SoulSanctuaryGlow", PosSoulAltar + new Vector3(0f, 4.5f, 0f), new Color(0.15f, 0.92f, 1.0f), 4.5f, 26f);
            CreateAtmosLight(lightsParent, "Light_ThornGateGorge", PosThornGate + new Vector3(-3f, 3.5f, 0f), new Color(1.0f, 0.52f, 0.18f), 3.5f, 18f);
        }

        private static void CreateAtmosLight(Transform parent, string name, Vector3 pos, Color col, float intensity, float range)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            Light l = go.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = col;
            l.intensity = intensity;
            l.range = range;
        }

        // =========================================================================
        // STEP 13: SCATTER CUSTOM ZONE 2 SWAMP TREES, GIANT SHROOMS & RUNE ROCKS
        // =========================================================================
        private static void ScatterZone2CustomVegetation(Transform vegParent, Terrain zone2, Dictionary<string, GameObject> customPrefabs)
        {
            GameObject weepingWillow = customPrefabs.ContainsKey("Zone2_WeepingWillow") ? customPrefabs["Zone2_WeepingWillow"] : null;
            GameObject deadRuneTree = customPrefabs.ContainsKey("Zone2_DeadRuneTree") ? customPrefabs["Zone2_DeadRuneTree"] : null;
            GameObject giantShrooms = customPrefabs.ContainsKey("Zone2_GiantSporeShrooms") ? customPrefabs["Zone2_GiantSporeShrooms"] : null;
            GameObject runicRock = customPrefabs.ContainsKey("Zone2_RunicObsidianRock") ? customPrefabs["Zone2_RunicObsidianRock"] : null;
            GameObject crystalRock = customPrefabs.ContainsKey("Rock_Crystal") ? customPrefabs["Rock_Crystal"] : null;
            GameObject oreRock = customPrefabs.ContainsKey("Rock_Ore") ? customPrefabs["Rock_Ore"] : null;

            GameObject pinePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/04_Prefabs/Environments/Tree_01_Quad.prefab");
            GameObject rockPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/04_Prefabs/Environments/Rock_01_Quad.prefab");

            var rand = new System.Random(2026);
            TerrainData td = zone2.terrainData;

            int spawnedTrees = 0;
            int spawnedProps = 0;

            for (int attempt = 0; attempt < 1100 && (spawnedTrees < 165 || spawnedProps < 75); attempt++)
            {
                float wx = Zone2TerrainOrigin.x + 35f + (float)rand.NextDouble() * 330f;
                float wz = Zone2TerrainOrigin.z + 35f + (float)rand.NextDouble() * 330f;
                Vector2 pXZ = new Vector2(wx, wz);

                float normX = (wx - Zone2TerrainOrigin.x) / Zone2TerrainSize.x;
                float normZ = (wz - Zone2TerrainOrigin.z) / Zone2TerrainSize.z;
                float h = td.GetInterpolatedHeight(normX, normZ);
                float steep = td.GetSteepness(normX, normZ);

                if (h < 7.3f || h > 28f || steep > 30f) continue;

                float roadDist = DistanceToIsland2Walkways(pXZ);
                if (roadDist < 6.8f) continue;

                float swampDist = Vector2.Distance(pXZ, new Vector2(PosSwampLake.x, PosSwampLake.z));
                if (swampDist < 19.5f) continue;

                bool isPropOrRock = (steep > 14f || roadDist < 10.5f || swampDist < 28f || rand.NextDouble() < 0.35);
                if (isPropOrRock && spawnedProps < 75)
                {
                    double rRoll = rand.NextDouble();
                    GameObject chosenPf = null;
                    Vector3 baseScale = new Vector3(8f, 9f, 1f);

                    if (rRoll < 0.38 && giantShrooms != null)
                    {
                        chosenPf = giantShrooms;
                        baseScale = new Vector3(7.2f, 8.0f, 1f);
                    }
                    else if (rRoll < 0.68 && runicRock != null)
                    {
                        chosenPf = runicRock;
                        baseScale = new Vector3(7.8f, 8.8f, 1f);
                    }
                    else if (rRoll < 0.85 && crystalRock != null)
                    {
                        chosenPf = crystalRock;
                        baseScale = new Vector3(7.5f, 8.6f, 1f);
                    }
                    else if (oreRock != null)
                    {
                        chosenPf = oreRock;
                        baseScale = new Vector3(7.5f, 8.5f, 1f);
                    }
                    else
                    {
                        chosenPf = rockPrefab;
                    }

                    if (chosenPf != null)
                    {
                        GameObject inst = (GameObject)PrefabUtility.InstantiatePrefab(chosenPf, vegParent);
                        inst.transform.position = new Vector3(wx, h, wz);
                        float s = 0.80f + (float)rand.NextDouble() * 0.40f;
                        inst.transform.localScale = new Vector3(baseScale.x * s, baseScale.y * s, 1f);
                        spawnedProps++;
                    }
                }
                else if (!isPropOrRock && spawnedTrees < 165)
                {
                    double tRoll = rand.NextDouble();
                    GameObject chosenTree = null;
                    Vector3 baseScale = new Vector3(11.0f, 12.8f, 1f);

                    if (tRoll < 0.48 && weepingWillow != null)
                    {
                        chosenTree = weepingWillow;
                        baseScale = new Vector3(11.8f, 13.5f, 1f);
                    }
                    else if (tRoll < 0.82 && deadRuneTree != null)
                    {
                        chosenTree = deadRuneTree;
                        baseScale = new Vector3(10.5f, 12.5f, 1f);
                    }
                    else
                    {
                        chosenTree = pinePrefab;
                        baseScale = new Vector3(10.0f, 12.0f, 1f);
                    }

                    if (chosenTree != null)
                    {
                        GameObject inst = (GameObject)PrefabUtility.InstantiatePrefab(chosenTree, vegParent);
                        inst.transform.position = new Vector3(wx, h, wz);
                        float s = 0.85f + (float)rand.NextDouble() * 0.35f;
                        inst.transform.localScale = new Vector3(baseScale.x * s, baseScale.y * s, 1f);
                        spawnedTrees++;
                    }
                }
            }
        }

        // =========================================================================
        // STEP 14: POSITION ZONE 2 ENEMIES ONTO ISLAND 2
        // =========================================================================
        private static void PositionZone2Enemies(Terrain zone2, Transform levelRoot)
        {
            PlaceOrInstantiateEnemy("Enemy_SporeShroom", "Assets/04_Prefabs/Characters/Enemy_SporeShroom.prefab",
                SnapToTerrainAndClearGrass(zone2, new Vector3(388f, BaseGroundHeight, -14f), 0f), levelRoot);

            PlaceOrInstantiateEnemy("Enemy_Skeleton", "Assets/04_Prefabs/Characters/Enemy_Skeleton.prefab",
                SnapToTerrainAndClearGrass(zone2, new Vector3(435f, BaseGroundHeight, -6f), 0f), levelRoot);

            PlaceOrInstantiateEnemy("Enemy_MagmaScarab", "Assets/04_Prefabs/Characters/Enemy_MagmaScarab.prefab",
                SnapToTerrainAndClearGrass(zone2, new Vector3(432f, BaseGroundHeight, -46f), 0f), levelRoot);

            Vector3 batGround = SnapToTerrainAndClearGrass(zone2, new Vector3(512f, BaseGroundHeight, 3f), 0f);
            PlaceOrInstantiateEnemy("Enemy_ShadowBat", "Assets/04_Prefabs/Characters/Enemy_ShadowBat.prefab",
                batGround + new Vector3(0f, 3.2f, 0f), levelRoot);
        }

        private static void PlaceOrInstantiateEnemy(string name, string prefabPath, Vector3 pos, Transform parent)
        {
            GameObject existing = GameObject.Find(name);
            if (existing == null)
            {
                GameObject pf = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                if (pf != null)
                {
                    existing = (GameObject)PrefabUtility.InstantiatePrefab(pf);
                    existing.name = name;
                }
            }
            if (existing != null)
            {
                existing.transform.SetParent(parent, true);
                NavMeshAgent agent = existing.GetComponent<NavMeshAgent>();
                if (agent != null) agent.enabled = false;
                existing.transform.position = pos;
                if (agent != null) agent.enabled = true;
            }
        }

        private static void BakeWorldNavMesh(Terrain zone1, Terrain zone2)
        {
            if (zone1 != null)
            {
                NavMeshSurface s1 = zone1.GetComponent<NavMeshSurface>();
                if (s1 != null) s1.BuildNavMesh();
            }
        }

        private static Vector3 SnapToTerrainAndClearGrass(Terrain terrain, Vector3 worldPos, float clearRadius)
        {
            if (terrain == null || terrain.terrainData == null) return worldPos;
            float groundY = terrain.SampleHeight(worldPos) + terrain.transform.position.y;
            worldPos.y = groundY;

            if (clearRadius <= 0.01f) return worldPos;

            TerrainData td = terrain.terrainData;
            Vector3 relPos = worldPos - terrain.transform.position;
            float normX = Mathf.Clamp01(relPos.x / td.size.x);
            float normZ = Mathf.Clamp01(relPos.z / td.size.z);

            int detailW = td.detailWidth;
            int detailH = td.detailHeight;
            int centerX = Mathf.RoundToInt(normX * (detailW - 1));
            int centerY = Mathf.RoundToInt(normZ * (detailH - 1));
            int radX = Mathf.Max(2, Mathf.RoundToInt((clearRadius / td.size.x) * detailW));
            int radY = Mathf.Max(2, Mathf.RoundToInt((clearRadius / td.size.z) * detailH));

            int startX = Mathf.Clamp(centerX - radX, 0, detailW - 1);
            int startY = Mathf.Clamp(centerY - radY, 0, detailH - 1);
            int width = Mathf.Clamp(centerX + radX, 0, detailW - 1) - startX + 1;
            int height = Mathf.Clamp(centerY + radY, 0, detailH - 1) - startY + 1;

            if (width > 0 && height > 0)
            {
                int[,] emptyPatch = new int[height, width];
                for (int layer = 0; layer < td.detailPrototypes.Length; layer++)
                    td.SetDetailLayer(startX, startY, layer, emptyPatch);
            }
            return worldPos;
        }

        private static void AddPart(List<ProBuilderMesh> parts, ProBuilderMesh mesh, Vector3 worldCenter, Quaternion rot, Material mat)
        {
            mesh.transform.position = Vector3.zero;
            mesh.transform.rotation = Quaternion.identity;
            mesh.SetMaterial(mesh.faces, mat);
            mesh.ToMesh();
            mesh.Refresh();

            MeshTransform.CenterPivot(mesh, null);
            mesh.transform.position = worldCenter;
            mesh.transform.rotation = rot;
            parts.Add(mesh);
        }

        private static ProBuilderMesh CombineAllParts(List<ProBuilderMesh> parts, GameObject parentObj, string meshName)
        {
            ProBuilderMesh targetAnchor = parts[0];
            targetAnchor.transform.rotation = parentObj.transform.rotation;
            MeshTransform.SetPivot(targetAnchor, parentObj.transform.position);

            IList<ProBuilderMesh> combinedList = CombineMeshes.Combine(parts, targetAnchor);
            ProBuilderMesh finalMesh = combinedList[0];

            foreach (var p in parts)
            {
                if (p != null && p != finalMesh) Object.DestroyImmediate(p.gameObject);
            }

            finalMesh.ToMesh();
            finalMesh.Refresh();
            finalMesh.gameObject.name = meshName;
            finalMesh.transform.SetParent(parentObj.transform, true);
            finalMesh.transform.localPosition = Vector3.zero;
            finalMesh.transform.localRotation = Quaternion.identity;
            finalMesh.transform.localScale = Vector3.one;
            finalMesh.gameObject.isStatic = true;

            if (finalMesh.GetComponent<Collider>() != null)
                Object.DestroyImmediate(finalMesh.GetComponent<Collider>());

            return finalMesh;
        }
    }
}
