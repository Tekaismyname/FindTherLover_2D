using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;
using FindTheLover.Environment.Hazards;
using FindTheLover.Environment.Puzzles;

namespace FindTheLover.EditorTools
{
    /// <summary>
    /// Editor Tool: Procedurally generates stylized hand-painted textures, URP materials,
    /// and 100% native ProBuilder 3D models for Spike_Trap, Pressure_Plate, and Puzzle_Lever
    /// matching the approved concept art 1-to-1.
    /// </summary>
    public static class StylizedPropBuilder
    {
        private const string EnvMatDir = "Assets/03_Art/Materials/Environment";
        private const string TexDir = "Assets/03_Art/Materials/Environment/Textures";
        private const string PrefabDir = "Assets/04_Prefabs/Environment";

        public struct PropMaterials
        {
            public Material stoneCarved;
            public Material stoneMossy;
            public Material darkPit;
            public Material spikeBronzeIron;
            public Material forgedIron;
            public Material antiqueBronze;
            public Material polishedWood;
            public Material runeCyanGlow;
        }

        [MenuItem("Tools/Environment/🔨 Rebuild Stylized 3D Props (ProBuilder Native)")]
        public static void RebuildAllStylizedProps()
        {
            EnsureDirectories();
            CleanStrayProBuilderObjects();

            PropMaterials mats = GenerateProceduralMaterialsAndTextures();

            RebuildProBuilderSpikeTrap(mats);
            RebuildProBuilderPressurePlate(mats);
            RebuildProBuilderPuzzleLever(mats);

            UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("<color=cyan><b>[StylizedPropBuilder] Rebuilt 100% ProBuilder 3D models & Procedural Materials for Spike_Trap, Pressure_Plate, and Puzzle_Lever!</b></color>");
        }

        private static void EnsureDirectories()
        {
            if (!AssetDatabase.IsValidFolder("Assets/03_Art/Materials"))
                AssetDatabase.CreateFolder("Assets/03_Art", "Materials");
            if (!AssetDatabase.IsValidFolder(EnvMatDir))
                AssetDatabase.CreateFolder("Assets/03_Art/Materials", "Environment");
            if (!AssetDatabase.IsValidFolder(TexDir))
                AssetDatabase.CreateFolder(EnvMatDir, "Textures");
            if (!AssetDatabase.IsValidFolder("Assets/04_Prefabs"))
                AssetDatabase.CreateFolder("Assets", "04_Prefabs");
            if (!AssetDatabase.IsValidFolder(PrefabDir))
                AssetDatabase.CreateFolder("Assets/04_Prefabs", "Environment");
        }

        private static void CleanStrayProBuilderObjects()
        {
            string[] strayNames = { "Cube", "Cylinder", "Cone", "Pipe", "Sphere", "Arch", "Prism", "ProBuilder Mesh" };
            foreach (string sName in strayNames)
            {
                GameObject obj = GameObject.Find(sName);
                while (obj != null && obj.transform.parent == null)
                {
                    Object.DestroyImmediate(obj);
                    obj = GameObject.Find(sName);
                }
            }
        }

        /// <summary>
        /// Samples Terrain height at worldXZ and clears tall grass details in a small radius around the prop
        /// so the stone prop sits flush on the ground without grass poking through it.
        /// </summary>
        private static Vector3 SnapToTerrainAndClearGrass(Vector3 worldPos, float clearRadius = 2.2f)
        {
            Terrain terrain = Terrain.activeTerrain;
            if (terrain == null || terrain.terrainData == null) return worldPos;

            float groundY = terrain.SampleHeight(worldPos) + terrain.transform.position.y;
            worldPos.y = groundY;

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
                {
                    td.SetDetailLayer(startX, startY, layer, emptyPatch);
                }
            }

            return worldPos;
        }

        // =========================================================================
        // PROCEDURAL TEXTURE & URP MATERIAL GENERATOR (HAND-PAINTED STYLIZED LOOK)
        // =========================================================================
        private static PropMaterials GenerateProceduralMaterialsAndTextures()
        {
            PropMaterials mats = new PropMaterials();

            // 1. Carved Ruin Stone (Smooth hand-painted warm sandstone/limestone with subtle edge highlight & grain)
            Texture2D stoneTex = GenerateTexture("Tex_StoneCarved", 256, 256, (u, v) =>
            {
                float n1 = Mathf.PerlinNoise(u * 4.5f + 1.3f, v * 4.5f + 2.1f);
                float n2 = Mathf.PerlinNoise(u * 14f + 5.7f, v * 14f + 8.4f) * 0.18f;
                float n3 = Mathf.PerlinNoise(u * 32f + 2.2f, v * 32f + 4.9f) * 0.08f;

                float edgeDist = Mathf.Min(Mathf.Min(u, 1f - u), Mathf.Min(v, 1f - v));
                float bevelHighlight = Mathf.SmoothStep(0.14f, 0.0f, edgeDist) * 0.14f;

                Color darkStone = new Color(0.48f, 0.45f, 0.41f);
                Color warmStone = new Color(0.68f, 0.64f, 0.58f);
                Color col = Color.Lerp(darkStone, warmStone, Mathf.Clamp01(n1 * 0.75f + n2 + n3));
                return col + new Color(bevelHighlight, bevelHighlight * 0.95f, bevelHighlight * 0.85f, 0f);
            });
            mats.stoneCarved = CreateOrUpdateMaterial("Prop_StoneCarved_Mat", stoneTex, Color.white, 0.18f, 0.0f);

            // 2. Mossy Crevice Stone (Olive-green moss blended into damp ruin stone)
            Texture2D mossTex = GenerateTexture("Tex_StoneMossy", 256, 256, (u, v) =>
            {
                float n1 = Mathf.PerlinNoise(u * 6f + 3.2f, v * 6f + 1.9f);
                float n2 = Mathf.PerlinNoise(u * 16f, v * 16f) * 0.25f;
                Color stone = new Color(0.38f, 0.37f, 0.33f);
                Color moss = new Color(0.35f, 0.45f, 0.24f);
                return Color.Lerp(stone, moss, Mathf.Clamp01(n1 * 0.8f + n2));
            });
            mats.stoneMossy = CreateOrUpdateMaterial("Prop_StoneMossy_Mat", mossTex, Color.white, 0.14f, 0.0f);

            // 3. Deep Dark Cavity Pit (Charcoal shadow interior for holes & moats)
            Texture2D pitTex = GenerateTexture("Tex_DarkPit", 128, 128, (u, v) =>
            {
                float n = Mathf.PerlinNoise(u * 8f, v * 8f) * 0.04f;
                return new Color(0.06f + n, 0.06f + n, 0.07f + n);
            });
            mats.darkPit = CreateOrUpdateMaterial("Prop_DarkPit_Mat", pitTex, Color.white, 0.05f, 0.0f);

            // 4. Spike Bronze-Iron Metallic (Warm golden-bronze & gunmetal iron matching Concept Image 1)
            Texture2D spikeTex = GenerateTexture("Tex_SpikeBronzeIron", 256, 256, (u, v) =>
            {
                float streak = Mathf.PerlinNoise(u * 20f, v * 3f);
                float wear = Mathf.PerlinNoise(u * 10f + 2f, v * 10f + 4f);
                Color gunmetal = new Color(0.38f, 0.39f, 0.43f);
                Color warmBronze = new Color(0.84f, 0.60f, 0.34f);
                Color goldHighlight = new Color(0.95f, 0.76f, 0.48f);
                Color blend = Color.Lerp(gunmetal, warmBronze, Mathf.Clamp01(v * 0.65f + streak * 0.5f));
                return Color.Lerp(blend, goldHighlight, wear * 0.35f);
            });
            mats.spikeBronzeIron = CreateOrUpdateMaterial("Prop_SpikeBronzeIron_Mat", spikeTex, Color.white, 0.62f, 0.72f);

            // 5. Forged Dark Iron (Hand-forged steel-iron with bright metallic edge sheen for lever shaft & barrel)
            Texture2D ironTex = GenerateTexture("Tex_ForgedIron", 256, 256, (u, v) =>
            {
                float brush = Mathf.PerlinNoise(u * 24f, v * 5f) * 0.16f;
                float hammer = Mathf.PerlinNoise(u * 10f, v * 10f) * 0.12f;
                return new Color(0.36f + brush + hammer, 0.40f + brush + hammer, 0.45f + brush + hammer);
            });
            mats.forgedIron = CreateOrUpdateMaterial("Prop_ForgedIron_Mat", ironTex, Color.white, 0.58f, 0.75f);

            // 6. Antique Bronze with Turquoise Patina (For hinge brackets & collars)
            Texture2D bronzeTex = GenerateTexture("Tex_AntiqueBronze", 256, 256, (u, v) =>
            {
                float n = Mathf.PerlinNoise(u * 8f + 4.1f, v * 8f + 2.7f);
                float detail = Mathf.PerlinNoise(u * 20f, v * 20f) * 0.18f;
                Color bronze = new Color(0.80f, 0.55f, 0.26f);
                Color patina = new Color(0.24f, 0.56f, 0.48f);
                return Color.Lerp(bronze, patina, n > 0.60f ? (n - 0.48f) * 1.3f : detail * 0.35f);
            });
            mats.antiqueBronze = CreateOrUpdateMaterial("Prop_AntiqueBronze_Mat", bronzeTex, Color.white, 0.56f, 0.72f);

            // 7. Polished Walnut Wood Grain (For pear-shaped lever handle knob)
            Texture2D woodTex = GenerateTexture("Tex_PolishedWood", 256, 256, (u, v) =>
            {
                float grain = Mathf.Sin((u * 14f + Mathf.PerlinNoise(u * 5f, v * 2.5f) * 2.5f) * Mathf.PI);
                float t = (grain + 1f) * 0.5f;
                Color darkWood = new Color(0.46f, 0.25f, 0.12f);
                Color warmWood = new Color(0.74f, 0.45f, 0.23f);
                return Color.Lerp(darkWood, warmWood, t);
            });
            mats.polishedWood = CreateOrUpdateMaterial("Prop_PolishedWood_Mat", woodTex, Color.white, 0.55f, 0.05f);

            // 8. Glowing Cyan Rune (Crisp Aqua-Cyan magic crystal emission without white blowout)
            Texture2D runeTex = GenerateTexture("Tex_RuneCyanGlow", 128, 128, (u, v) =>
            {
                return new Color(0.20f, 0.90f, 1.0f);
            });
            mats.runeCyanGlow = CreateOrUpdateMaterial("Prop_RuneCyanGlow_Mat", runeTex,
                new Color(0.20f, 0.90f, 1.0f), 0.45f, 0.0f, new Color(0.10f, 0.78f, 0.96f) * 1.65f);

            return mats;
        }

        private static Texture2D GenerateTexture(string name, int width, int height, System.Func<float, float, Color> pixelFunc)
        {
            string assetPath = $"{TexDir}/{name}.png";
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, true);
            Color[] pixels = new Color[width * height];

            for (int y = 0; y < height; y++)
            {
                float v = (float)y / (height - 1);
                for (int x = 0; x < width; x++)
                {
                    float u = (float)x / (width - 1);
                    pixels[y * width + x] = pixelFunc(u, v);
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();

            byte[] pngBytes = tex.EncodeToPNG();
            Object.DestroyImmediate(tex);

            string fullPath = Path.Combine(Directory.GetCurrentDirectory(), assetPath);
            File.WriteAllBytes(fullPath, pngBytes);
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);

            return AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
        }

        private static Material CreateOrUpdateMaterial(string name, Texture2D albedoTex, Color baseColor, float smoothness, float metallic, Color? emissionColor = null)
        {
            string path = $"{EnvMatDir}/{name}.mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);

            Shader litShader = Shader.Find("Universal Render Pipeline/Lit");
            if (litShader == null) litShader = Shader.Find("Standard");

            if (mat == null)
            {
                mat = new Material(litShader);
                mat.name = name;
                AssetDatabase.CreateAsset(mat, path);
            }
            else
            {
                mat.shader = litShader;
            }

            mat.SetColor("_BaseColor", baseColor);
            mat.color = baseColor;
            if (albedoTex != null)
            {
                mat.SetTexture("_BaseMap", albedoTex);
                mat.mainTexture = albedoTex;
            }

            mat.SetFloat("_Smoothness", smoothness);
            mat.SetFloat("_Metallic", metallic);

            if (emissionColor.HasValue)
            {
                mat.EnableKeyword("_EMISSION");
                mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
                mat.SetColor("_EmissionColor", emissionColor.Value);
            }
            else
            {
                mat.DisableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", Color.black);
            }

            EditorUtility.SetDirty(mat);
            return mat;
        }

        // =========================================================================
        // PROBUILDER GEOMETRY HELPERS (TRUE BEVEL, PIPE, CONE & SAFE COMBINE)
        // =========================================================================
        private static ProBuilderMesh CreateBeveledBox(Vector3 worldPos, Vector3 size, float bevelAmount, Material mat, Quaternion? rotation = null)
        {
            ProBuilderMesh pb = ShapeGenerator.GenerateCube(PivotLocation.Center, size);
            pb.transform.position = worldPos;
            pb.transform.rotation = rotation ?? Quaternion.identity;
            pb.transform.localScale = Vector3.one;

            if (bevelAmount > 0.001f)
            {
                List<Edge> edges = new List<Edge>();
                foreach (Face f in pb.faces) edges.AddRange(f.edges);
                Bevel.BevelEdges(pb, edges, bevelAmount);
            }

            pb.SetMaterial(pb.faces, mat);
            pb.ToMesh();
            pb.Refresh();
            return pb;
        }

        private static ProBuilderMesh CreatePipeMesh(Vector3 worldPos, float radius, float height, float thickness, int subdivAxis, Material mat, Quaternion? rotation = null)
        {
            ProBuilderMesh pb = ShapeGenerator.GeneratePipe(PivotLocation.Center, radius, height, thickness, subdivAxis, 1);
            pb.transform.position = worldPos;
            pb.transform.rotation = rotation ?? Quaternion.identity;
            pb.transform.localScale = Vector3.one;
            pb.SetMaterial(pb.faces, mat);
            pb.ToMesh();
            pb.Refresh();
            return pb;
        }

        private static ProBuilderMesh CreateCylinderMesh(Vector3 worldPos, float radius, float height, int axisDivisions, Material mat, Quaternion? rotation = null)
        {
            ProBuilderMesh pb = ShapeGenerator.GenerateCylinder(PivotLocation.Center, axisDivisions, radius, height, 1, 1);
            pb.transform.position = worldPos;
            pb.transform.rotation = rotation ?? Quaternion.identity;
            pb.transform.localScale = Vector3.one;
            pb.SetMaterial(pb.faces, mat);
            pb.ToMesh();
            pb.Refresh();
            return pb;
        }

        private static ProBuilderMesh CreateConeMesh(Vector3 worldPos, float radius, float height, int subdivAxis, Material mat, Quaternion? rotation = null)
        {
            ProBuilderMesh pb = ShapeGenerator.GenerateCone(PivotLocation.Center, radius, height, subdivAxis);
            pb.transform.position = worldPos;
            pb.transform.rotation = rotation ?? Quaternion.identity;
            pb.transform.localScale = Vector3.one;
            pb.SetMaterial(pb.faces, mat);
            pb.ToMesh();
            pb.Refresh();
            return pb;
        }

        private static ProBuilderMesh CreateScaledSphereMesh(Vector3 worldPos, Vector3 scaleSize, Material mat, Quaternion? rotation = null)
        {
            ProBuilderMesh pb = ShapeGenerator.CreateShape(ShapeType.Sphere);
            pb.transform.position = worldPos;
            pb.transform.rotation = rotation ?? Quaternion.identity;
            pb.transform.localScale = scaleSize;
            MeshTransform.FreezeScaleTransform(pb);
            pb.SetMaterial(pb.faces, mat);
            pb.ToMesh();
            pb.Refresh();
            return pb;
        }

        private static GameObject CombineIntoUnifiedProBuilderMesh(List<ProBuilderMesh> parts, Transform parentTransform, string meshName)
        {
            ProBuilderMesh targetAnchor = parts[0];
            targetAnchor.transform.rotation = parentTransform.rotation;
            targetAnchor.transform.localScale = Vector3.one;
            MeshTransform.SetPivot(targetAnchor, parentTransform.position);

            IList<ProBuilderMesh> combinedList = CombineMeshes.Combine(parts, targetAnchor);
            ProBuilderMesh finalMesh = combinedList[0];

            foreach (ProBuilderMesh p in parts)
            {
                if (p != null && p != finalMesh)
                {
                    Object.DestroyImmediate(p.gameObject);
                }
            }

            finalMesh.ToMesh();
            finalMesh.Refresh();

            GameObject resultObj = finalMesh.gameObject;
            resultObj.name = meshName;
            resultObj.transform.SetParent(parentTransform, true);
            resultObj.transform.localPosition = Vector3.zero;
            resultObj.transform.localRotation = Quaternion.identity;
            resultObj.transform.localScale = Vector3.one;

            if (resultObj.GetComponent<Collider>() != null)
            {
                Object.DestroyImmediate(resultObj.GetComponent<Collider>());
            }

            return resultObj;
        }

        // =========================================================================
        // 1. SPIKE TRAP (Concept Art 1: Beveled Stone Blocks, 9 Deep Wells, 9 Bronze-Iron Spikes)
        // =========================================================================
        private static void RebuildProBuilderSpikeTrap(PropMaterials mats)
        {
            GameObject existing = GameObject.Find("Spike_Trap");
            Vector3 worldPos = new Vector3(4.5f, 7.56f, 0.0f);
            if (existing != null)
            {
                worldPos = existing.transform.position;
                Object.DestroyImmediate(existing);
            }
            worldPos = SnapToTerrainAndClearGrass(worldPos, 2.4f);

            GameObject trap = new GameObject("Spike_Trap");
            trap.transform.position = worldPos;

            BoxCollider col = trap.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.center = new Vector3(0f, 0.45f, 0f);
            col.size = new Vector3(2.0f, 0.9f, 2.0f);

            trap.AddComponent<AudioSource>();

            // --- A. Base_Plate (Unified Multi-Material ProBuilder Mesh) ---
            List<ProBuilderMesh> baseParts = new List<ProBuilderMesh>();

            // 1. Mossy recessed foundation floor tray
            baseParts.Add(CreateBeveledBox(worldPos + new Vector3(0f, 0.065f, 0f), new Vector3(1.92f, 0.13f, 1.92f), 0.02f, mats.stoneMossy));

            // 2. 4 Corner Carved Stone Blocks (Snugly fitted with crisp beveled V-joints)
            float cornerOffset = 0.77f;
            Vector3 cornerSize = new Vector3(0.48f, 0.22f, 0.48f);
            baseParts.Add(CreateBeveledBox(worldPos + new Vector3(-cornerOffset, 0.11f, -cornerOffset), cornerSize, 0.035f, mats.stoneCarved));
            baseParts.Add(CreateBeveledBox(worldPos + new Vector3(cornerOffset, 0.11f, -cornerOffset), cornerSize, 0.035f, mats.stoneCarved));
            baseParts.Add(CreateBeveledBox(worldPos + new Vector3(-cornerOffset, 0.11f, cornerOffset), cornerSize, 0.035f, mats.stoneCarved));
            baseParts.Add(CreateBeveledBox(worldPos + new Vector3(cornerOffset, 0.11f, cornerOffset), cornerSize, 0.035f, mats.stoneCarved));

            // 3. 8 Perimeter Edge Stone Blocks (Snugly meeting the corner blocks)
            float[] sidePairs = { -0.265f, 0.265f };
            foreach (float s in sidePairs)
            {
                baseParts.Add(CreateBeveledBox(worldPos + new Vector3(s, 0.108f, 0.79f), new Vector3(0.54f, 0.215f, 0.40f), 0.032f, mats.stoneCarved));
                baseParts.Add(CreateBeveledBox(worldPos + new Vector3(s, 0.108f, -0.79f), new Vector3(0.54f, 0.215f, 0.40f), 0.032f, mats.stoneCarved));
                baseParts.Add(CreateBeveledBox(worldPos + new Vector3(0.79f, 0.108f, s), new Vector3(0.40f, 0.215f, 0.54f), 0.032f, mats.stoneCarved));
                baseParts.Add(CreateBeveledBox(worldPos + new Vector3(-0.79f, 0.108f, s), new Vector3(0.40f, 0.215f, 0.54f), 0.032f, mats.stoneCarved));
            }

            // 4. Inner Carved Grid Frame Dividers (3x3 cell grid lines + inner perimeter frame)
            float[] gridDivs = { -0.245f, 0.245f };
            foreach (float g in gridDivs)
            {
                baseParts.Add(CreateBeveledBox(worldPos + new Vector3(g, 0.13f, 0f), new Vector3(0.06f, 0.05f, 1.46f), 0.008f, mats.stoneCarved));
                baseParts.Add(CreateBeveledBox(worldPos + new Vector3(0f, 0.13f, g), new Vector3(1.46f, 0.05f, 0.06f), 0.008f, mats.stoneCarved));
            }

            // 5. 9 Deep Circular Spike Wells (Hollow Stone Pipe Rims + Deep Charcoal Pit Interiors)
            float[] wellOffsets = { -0.49f, 0f, 0.49f };
            for (int r = 0; r < 3; r++)
            {
                for (int c = 0; c < 3; c++)
                {
                    Vector3 wellCenter = worldPos + new Vector3(wellOffsets[r], 0f, wellOffsets[c]);
                    baseParts.Add(CreatePipeMesh(wellCenter + new Vector3(0f, 0.145f, 0f), 0.205f, 0.08f, 0.044f, 16, mats.stoneCarved));
                    baseParts.Add(CreateCylinderMesh(wellCenter + new Vector3(0f, 0.13f, 0f), 0.165f, 0.02f, 16, mats.darkPit));
                }
            }

            CombineIntoUnifiedProBuilderMesh(baseParts, trap.transform, "Base_Plate");

            // --- B. Spikes_Root (9 Sharp Faceted Bronze-Iron Spikes) ---
            GameObject spikesRoot = new GameObject("Spikes_Root");
            spikesRoot.transform.SetParent(trap.transform, false);
            spikesRoot.transform.localPosition = new Vector3(0f, 0.0f, 0f);

            List<ProBuilderMesh> spikeParts = new List<ProBuilderMesh>();
            for (int r = 0; r < 3; r++)
            {
                for (int c = 0; c < 3; c++)
                {
                    Vector3 spikePos = spikesRoot.transform.position + new Vector3(wellOffsets[r], 0f, wellOffsets[c]);
                    spikeParts.Add(CreateConeMesh(spikePos + new Vector3(0f, 0.48f, 0f), 0.125f, 0.80f, 6, mats.spikeBronzeIron));
                    spikeParts.Add(CreateConeMesh(spikePos + new Vector3(0f, 0.24f, 0f), 0.134f, 0.30f, 6, mats.forgedIron));
                }
            }

            CombineIntoUnifiedProBuilderMesh(spikeParts, spikesRoot.transform, "Spikes_9x_Cones");

            // --- C. Configure SpikeTrap Script ---
            SpikeTrap spikeComp = trap.AddComponent<SpikeTrap>();
            SerializedObject so = new SerializedObject(spikeComp);
            so.FindProperty("spikesRoot").objectReferenceValue = spikesRoot.transform;
            so.FindProperty("retractedPosY").floatValue = -0.68f;
            so.FindProperty("warningPosY").floatValue = -0.32f;
            so.FindProperty("extendedPosY").floatValue = 0.0f;
            so.FindProperty("spikeDamage").floatValue = 25f;
            so.FindProperty("inactiveDuration").floatValue = 2.0f;
            so.FindProperty("warningDuration").floatValue = 0.5f;
            so.FindProperty("extendedDuration").floatValue = 1.0f;
            so.ApplyModifiedProperties();

            PrefabUtility.SaveAsPrefabAssetAndConnect(trap, PrefabDir + "/Spike_Trap.prefab", InteractionMode.UserAction);
        }

        // =========================================================================
        // 2. PRESSURE PLATE (Concept Art 2: 2-Tier Beveled Frame, Dark Moat, Beveled Button & Crisp Cyan 'R' Rune)
        // =========================================================================
        private static void RebuildProBuilderPressurePlate(PropMaterials mats)
        {
            GameObject existing = GameObject.Find("Pressure_Plate");
            Vector3 worldPos = new Vector3(-3f, 7.56f, 0f);
            if (existing != null)
            {
                worldPos = existing.transform.position;
                Object.DestroyImmediate(existing);
            }
            worldPos = SnapToTerrainAndClearGrass(worldPos, 2.4f);

            GameObject plateObj = new GameObject("Pressure_Plate");
            plateObj.transform.position = worldPos;

            BoxCollider plateCol = plateObj.AddComponent<BoxCollider>();
            plateCol.isTrigger = true;
            plateCol.center = new Vector3(0f, 0.6f, 0f);
            plateCol.size = new Vector3(1.6f, 1.2f, 1.6f);

            Rigidbody plateRb = plateObj.AddComponent<Rigidbody>();
            plateRb.isKinematic = true;
            plateRb.useGravity = false;

            plateObj.AddComponent<AudioSource>();

            // --- A. Base_Plinth (2-Tier Beveled Stone Plinth + Hollow Recessed Moat) ---
            List<ProBuilderMesh> plinthParts = new List<ProBuilderMesh>();

            // 1. Tier 1: Wide bottom stone plinth with heavy beveled edges & corners
            plinthParts.Add(CreateBeveledBox(worldPos + new Vector3(0f, 0.055f, 0f), new Vector3(1.86f, 0.11f, 1.86f), 0.038f, mats.stoneCarved));

            // 2. Mossy stepped transition layer
            plinthParts.Add(CreateBeveledBox(worldPos + new Vector3(0f, 0.115f, 0f), new Vector3(1.68f, 0.04f, 1.68f), 0.015f, mats.stoneMossy));

            // 3. Deep dark moat floor inside the upper hollow frame
            plinthParts.Add(CreateBeveledBox(worldPos + new Vector3(0f, 0.135f, 0f), new Vector3(1.20f, 0.02f, 1.20f), 0.005f, mats.darkPit));

            // 4. Tier 2: Continuous Hollow Raised Stone Outer Wall (4 Beveled Walls + 4 Corner Pillars)
            float wallOffset = 0.67f;
            plinthParts.Add(CreateBeveledBox(worldPos + new Vector3(0f, 0.18f, wallOffset), new Vector3(1.16f, 0.16f, 0.26f), 0.028f, mats.stoneCarved));
            plinthParts.Add(CreateBeveledBox(worldPos + new Vector3(0f, 0.18f, -wallOffset), new Vector3(1.16f, 0.16f, 0.26f), 0.028f, mats.stoneCarved));
            plinthParts.Add(CreateBeveledBox(worldPos + new Vector3(wallOffset, 0.18f, 0f), new Vector3(0.26f, 0.16f, 1.16f), 0.028f, mats.stoneCarved));
            plinthParts.Add(CreateBeveledBox(worldPos + new Vector3(-wallOffset, 0.18f, 0f), new Vector3(0.26f, 0.16f, 1.16f), 0.028f, mats.stoneCarved));

            Vector3 cPillarSize = new Vector3(0.28f, 0.165f, 0.28f);
            plinthParts.Add(CreateBeveledBox(worldPos + new Vector3(-wallOffset, 0.182f, -wallOffset), cPillarSize, 0.032f, mats.stoneCarved));
            plinthParts.Add(CreateBeveledBox(worldPos + new Vector3(wallOffset, 0.182f, -wallOffset), cPillarSize, 0.032f, mats.stoneCarved));
            plinthParts.Add(CreateBeveledBox(worldPos + new Vector3(-wallOffset, 0.182f, wallOffset), cPillarSize, 0.032f, mats.stoneCarved));
            plinthParts.Add(CreateBeveledBox(worldPos + new Vector3(wallOffset, 0.182f, wallOffset), cPillarSize, 0.032f, mats.stoneCarved));

            // 5. Inner Decorative Carved Border Trim (Framing the sunken moat)
            float trimOffset = 0.52f;
            plinthParts.Add(CreateBeveledBox(worldPos + new Vector3(0f, 0.22f, trimOffset), new Vector3(1.08f, 0.06f, 0.05f), 0.01f, mats.stoneMossy));
            plinthParts.Add(CreateBeveledBox(worldPos + new Vector3(0f, 0.22f, -trimOffset), new Vector3(1.08f, 0.06f, 0.05f), 0.01f, mats.stoneMossy));
            plinthParts.Add(CreateBeveledBox(worldPos + new Vector3(trimOffset, 0.22f, 0f), new Vector3(0.05f, 0.06f, 1.08f), 0.01f, mats.stoneMossy));
            plinthParts.Add(CreateBeveledBox(worldPos + new Vector3(-trimOffset, 0.22f, 0f), new Vector3(0.05f, 0.06f, 1.08f), 0.01f, mats.stoneMossy));

            CombineIntoUnifiedProBuilderMesh(plinthParts, plateObj.transform, "Base_Plinth");

            // --- B. Plate_Visual (Beveled Stone Button + Carved Dark Groove + Glowing Cyan 'R' Rune) ---
            GameObject plateVisual = new GameObject("Plate_Visual");
            plateVisual.transform.SetParent(plateObj.transform, false);
            plateVisual.transform.localPosition = new Vector3(0f, 0.15f, 0f);

            Vector3 pvPos = plateVisual.transform.position;
            List<ProBuilderMesh> buttonParts = new List<ProBuilderMesh>();

            // 1. Thick Beveled Central Stone Button Slab
            buttonParts.Add(CreateBeveledBox(pvPos + new Vector3(0f, 0.08f, 0f), new Vector3(0.90f, 0.16f, 0.90f), 0.042f, mats.stoneCarved));

            // 2. Carved Dark Groove Border + Glowing Cyan Rune ('R'-Shaped Nordic/Elven Rune matching Concept Image 2)
            float grooveY = 0.158f;
            float runeY = 0.163f;

            // Dark carved channel outlines for high contrast around the glowing cyan rune
            buttonParts.Add(CreateBeveledBox(pvPos + new Vector3(-0.08f, grooveY, 0.0f), new Vector3(0.072f, 0.012f, 0.39f), 0.002f, mats.darkPit, Quaternion.Euler(0f, -12f, 0f)));
            buttonParts.Add(CreateBeveledBox(pvPos + new Vector3(0.01f, grooveY, 0.11f), new Vector3(0.068f, 0.012f, 0.25f), 0.002f, mats.darkPit, Quaternion.Euler(0f, 52f, 0f)));
            buttonParts.Add(CreateBeveledBox(pvPos + new Vector3(0.02f, grooveY, 0.01f), new Vector3(0.068f, 0.012f, 0.23f), 0.002f, mats.darkPit, Quaternion.Euler(0f, -48f, 0f)));
            buttonParts.Add(CreateBeveledBox(pvPos + new Vector3(0.09f, grooveY, -0.07f), new Vector3(0.068f, 0.012f, 0.25f), 0.002f, mats.darkPit, Quaternion.Euler(0f, 28f, 0f)));
            buttonParts.Add(CreateBeveledBox(pvPos + new Vector3(0.15f, grooveY, 0.03f), new Vector3(0.064f, 0.012f, 0.15f), 0.002f, mats.darkPit, Quaternion.Euler(0f, -10f, 0f)));

            // Glowing Cyan inner rune strokes
            buttonParts.Add(CreateBeveledBox(pvPos + new Vector3(-0.08f, runeY, 0.0f), new Vector3(0.044f, 0.014f, 0.35f), 0.004f, mats.runeCyanGlow, Quaternion.Euler(0f, -12f, 0f)));
            buttonParts.Add(CreateBeveledBox(pvPos + new Vector3(0.01f, runeY, 0.11f), new Vector3(0.042f, 0.014f, 0.21f), 0.004f, mats.runeCyanGlow, Quaternion.Euler(0f, 52f, 0f)));
            buttonParts.Add(CreateBeveledBox(pvPos + new Vector3(0.02f, runeY, 0.01f), new Vector3(0.042f, 0.014f, 0.19f), 0.004f, mats.runeCyanGlow, Quaternion.Euler(0f, -48f, 0f)));
            buttonParts.Add(CreateBeveledBox(pvPos + new Vector3(0.09f, runeY, -0.07f), new Vector3(0.042f, 0.014f, 0.21f), 0.004f, mats.runeCyanGlow, Quaternion.Euler(0f, 28f, 0f)));
            buttonParts.Add(CreateBeveledBox(pvPos + new Vector3(0.15f, runeY, 0.03f), new Vector3(0.038f, 0.014f, 0.11f), 0.004f, mats.runeCyanGlow, Quaternion.Euler(0f, -10f, 0f)));

            CombineIntoUnifiedProBuilderMesh(buttonParts, plateVisual.transform, "Stone_Button_With_Rune");

            // Soft Cyan Point Light (gentle intensity so it doesn't wash out the rune shape)
            GameObject glowLightObj = new GameObject("Rune_Glow_Light");
            glowLightObj.transform.SetParent(plateVisual.transform, false);
            glowLightObj.transform.localPosition = new Vector3(0f, 0.28f, 0f);
            Light ptLight = glowLightObj.AddComponent<Light>();
            ptLight.type = LightType.Point;
            ptLight.color = new Color(0.15f, 0.88f, 1.0f);
            ptLight.intensity = 0.22f;
            ptLight.range = 1.4f;

            // --- C. Configure PressurePlate Script ---
            PressurePlate plateComp = plateObj.AddComponent<PressurePlate>();
            SerializedObject so = new SerializedObject(plateComp);
            so.FindProperty("plateVisual").objectReferenceValue = plateVisual.transform;
            so.FindProperty("depressDistance").floatValue = 0.07f;
            so.FindProperty("springSpeed").floatValue = 12.0f;
            so.ApplyModifiedProperties();

            PrefabUtility.SaveAsPrefabAssetAndConnect(plateObj, PrefabDir + "/Pressure_Plate.prefab", InteractionMode.UserAction);
        }

        // =========================================================================
        // 3. PUZZLE LEVER (Concept Art 3: Chunky Stone Pedestal, Circular Trench, Bronze U-Hinge, Bent Iron Rod & Pear Wood Knob)
        // =========================================================================
        private static void RebuildProBuilderPuzzleLever(PropMaterials mats)
        {
            GameObject existing = GameObject.Find("Puzzle_Lever");
            Vector3 worldPos = new Vector3(0f, 7.56f, 3f);
            if (existing != null)
            {
                worldPos = existing.transform.position;
                Object.DestroyImmediate(existing);
            }
            worldPos = SnapToTerrainAndClearGrass(worldPos, 2.4f);

            GameObject leverObj = new GameObject("Puzzle_Lever");
            leverObj.transform.position = worldPos;
            leverObj.AddComponent<AudioSource>();

            // --- A. Lever_Pedestal_Base (Carved Stone Block + Circular Guide Trench + Antique Bronze Double Hinge) ---
            List<ProBuilderMesh> baseParts = new List<ProBuilderMesh>();

            // 1. Main lower carved stone pedestal block with heavy bevels
            baseParts.Add(CreateBeveledBox(worldPos + new Vector3(0f, 0.16f, 0f), new Vector3(0.96f, 0.32f, 0.96f), 0.045f, mats.stoneCarved));

            // 2. 4 Rugged Beveled Corner Stone Bolsters
            float cOff = 0.34f;
            Vector3 bolsterSize = new Vector3(0.32f, 0.36f, 0.32f);
            baseParts.Add(CreateBeveledBox(worldPos + new Vector3(-cOff, 0.18f, -cOff), bolsterSize, 0.035f, mats.stoneCarved));
            baseParts.Add(CreateBeveledBox(worldPos + new Vector3(cOff, 0.18f, -cOff), bolsterSize, 0.035f, mats.stoneCarved));
            baseParts.Add(CreateBeveledBox(worldPos + new Vector3(-cOff, 0.18f, cOff), bolsterSize, 0.035f, mats.stoneCarved));
            baseParts.Add(CreateBeveledBox(worldPos + new Vector3(cOff, 0.18f, cOff), bolsterSize, 0.035f, mats.stoneCarved));

            // 3. Upper Stone Deck with Front Rectangular Channel Cutout
            baseParts.Add(CreateBeveledBox(worldPos + new Vector3(-0.30f, 0.34f, 0f), new Vector3(0.34f, 0.08f, 0.90f), 0.025f, mats.stoneCarved));
            baseParts.Add(CreateBeveledBox(worldPos + new Vector3(0.30f, 0.34f, 0f), new Vector3(0.34f, 0.08f, 0.90f), 0.025f, mats.stoneCarved));
            baseParts.Add(CreateBeveledBox(worldPos + new Vector3(0f, 0.34f, 0.28f), new Vector3(0.34f, 0.08f, 0.34f), 0.025f, mats.stoneCarved));

            // Recessed front notch floor (offset slightly above 0.32f to avoid z-fighting)
            baseParts.Add(CreateBeveledBox(worldPos + new Vector3(0f, 0.312f, -0.32f), new Vector3(0.26f, 0.035f, 0.30f), 0.008f, mats.stoneMossy));

            // 4. Sunken Circular Guide Slot on Top Surface (Pipe Outer Ring + Dark Pit Trench + Central Stone Island)
            baseParts.Add(CreatePipeMesh(worldPos + new Vector3(0f, 0.37f, 0.02f), 0.34f, 0.05f, 0.065f, 20, mats.stoneCarved));
            baseParts.Add(CreateCylinderMesh(worldPos + new Vector3(0f, 0.352f, 0.02f), 0.28f, 0.02f, 20, mats.darkPit));
            baseParts.Add(CreateCylinderMesh(worldPos + new Vector3(0f, 0.37f, 0.02f), 0.185f, 0.045f, 16, mats.stoneCarved));

            // 5. Antique Bronze Double-Bracket Pivot Hinge (U-Clevis Mount matching Concept Image 3)
            Vector3 hingeCenter = worldPos + new Vector3(0f, 0.42f, 0.0f);

            baseParts.Add(CreateBeveledBox(worldPos + new Vector3(0f, 0.39f, -0.02f), new Vector3(0.24f, 0.035f, 0.28f), 0.008f, mats.antiqueBronze));
            baseParts.Add(CreateBeveledBox(worldPos + new Vector3(-0.075f, 0.36f, -0.15f), new Vector3(0.045f, 0.08f, 0.06f), 0.008f, mats.antiqueBronze));
            baseParts.Add(CreateBeveledBox(worldPos + new Vector3(0.075f, 0.36f, -0.15f), new Vector3(0.045f, 0.08f, 0.06f), 0.008f, mats.antiqueBronze));

            float cheekX = 0.072f;
            baseParts.Add(CreateBeveledBox(hingeCenter + new Vector3(-cheekX, -0.01f, 0f), new Vector3(0.038f, 0.09f, 0.14f), 0.008f, mats.antiqueBronze));
            baseParts.Add(CreateBeveledBox(hingeCenter + new Vector3(cheekX, -0.01f, 0f), new Vector3(0.038f, 0.09f, 0.14f), 0.008f, mats.antiqueBronze));

            Quaternion rotZ90 = Quaternion.Euler(0f, 0f, 90f);
            baseParts.Add(CreateCylinderMesh(hingeCenter + new Vector3(-cheekX, 0.02f, 0f), 0.068f, 0.038f, 16, mats.antiqueBronze, rotZ90));
            baseParts.Add(CreateCylinderMesh(hingeCenter + new Vector3(cheekX, 0.02f, 0f), 0.068f, 0.038f, 16, mats.antiqueBronze, rotZ90));
            baseParts.Add(CreateCylinderMesh(hingeCenter + new Vector3(0f, 0.02f, 0f), 0.058f, 0.11f, 16, mats.forgedIron, rotZ90));
            baseParts.Add(CreateCylinderMesh(hingeCenter + new Vector3(0f, 0.02f, 0f), 0.028f, 0.195f, 12, mats.antiqueBronze, rotZ90));

            CombineIntoUnifiedProBuilderMesh(baseParts, leverObj.transform, "Lever_Pedestal_Base");

            // --- B. Lever_Handle (Smoothly Bent Forged Iron Arm + Double Bronze Collars + Pear-Shaped Wood Knob) ---
            GameObject leverHandle = new GameObject("Lever_Handle");
            leverHandle.transform.SetParent(leverObj.transform, false);
            leverHandle.transform.localPosition = new Vector3(0f, 0.44f, 0f);
            leverHandle.transform.localRotation = Quaternion.identity;

            Vector3 pivotOrigin = leverHandle.transform.position;
            List<ProBuilderMesh> handleParts = new List<ProBuilderMesh>();

            // 1. Anchor hub inside pivot barrel
            handleParts.Add(CreateCylinderMesh(pivotOrigin, 0.045f, 0.08f, 12, mats.forgedIron, rotZ90));

            // 2. Lower Forged Iron Blade Segment
            Quaternion lowerBend = Quaternion.Euler(-4f, 0f, 0f);
            Vector3 lowerDir = lowerBend * Vector3.up;
            handleParts.Add(CreateBeveledBox(pivotOrigin + lowerDir * 0.105f, new Vector3(0.052f, 0.21f, 0.064f), 0.010f, mats.forgedIron, lowerBend));

            // Seamless Forged Elbow Transition Joint
            Vector3 elbowPos = pivotOrigin + lowerDir * 0.20f;
            handleParts.Add(CreateBeveledBox(elbowPos, new Vector3(0.050f, 0.06f, 0.060f), 0.010f, mats.forgedIron, Quaternion.Euler(-11f, 0f, 0f)));

            // 3. Upper Bent Forged Iron Blade Segment
            Quaternion upperBend = Quaternion.Euler(-18f, 0f, 0f);
            Vector3 upperDir = upperBend * Vector3.up;
            handleParts.Add(CreateBeveledBox(elbowPos + upperDir * 0.10f, new Vector3(0.046f, 0.20f, 0.054f), 0.009f, mats.forgedIron, upperBend));

            // 4. Double Antique Bronze Ferrule Collars
            Vector3 collarBase = elbowPos + upperDir * 0.20f;
            handleParts.Add(CreateCylinderMesh(collarBase, 0.048f, 0.026f, 16, mats.antiqueBronze, upperBend));
            handleParts.Add(CreateCylinderMesh(collarBase + upperDir * 0.022f, 0.042f, 0.022f, 16, mats.forgedIron, upperBend));
            handleParts.Add(CreateCylinderMesh(collarBase + upperDir * 0.042f, 0.052f, 0.026f, 16, mats.antiqueBronze, upperBend));

            // 5. Ergonomic Pear-Shaped Polished Wood Grip Knob
            Vector3 gripStart = collarBase + upperDir * 0.055f;
            handleParts.Add(CreateCylinderMesh(gripStart + upperDir * 0.04f, 0.048f, 0.08f, 16, mats.polishedWood, upperBend));
            handleParts.Add(CreateScaledSphereMesh(gripStart + upperDir * 0.11f, new Vector3(0.125f, 0.18f, 0.125f), mats.polishedWood, upperBend));
            handleParts.Add(CreateScaledSphereMesh(gripStart + upperDir * 0.165f, new Vector3(0.142f, 0.15f, 0.142f), mats.polishedWood, upperBend));

            // 6. Top Antique Bronze Finial Button Cap
            handleParts.Add(CreateScaledSphereMesh(gripStart + upperDir * 0.235f, new Vector3(0.075f, 0.035f, 0.075f), mats.antiqueBronze, upperBend));

            CombineIntoUnifiedProBuilderMesh(handleParts, leverHandle.transform, "Handle_Stick_And_Knob");

            // Tilt handle to initial resting angle (+22 degrees) so the bent curve matches Concept Image 3!
            leverHandle.transform.localRotation = Quaternion.Euler(22f, 0f, 0f);

            // --- C. Configure PuzzleLever Script ---
            PuzzleLever leverComp = leverObj.AddComponent<PuzzleLever>();
            SerializedObject so = new SerializedObject(leverComp);
            so.FindProperty("leverHandle").objectReferenceValue = leverHandle.transform;
            so.FindProperty("interactionRadius").floatValue = 2.0f;
            so.FindProperty("isReusable").boolValue = true;
            so.FindProperty("offAngle").floatValue = -35.0f;
            so.FindProperty("onAngle").floatValue = 35.0f;
            so.FindProperty("flipSpeed").floatValue = 10.0f;
            so.ApplyModifiedProperties();

            PrefabUtility.SaveAsPrefabAssetAndConnect(leverObj, PrefabDir + "/Puzzle_Lever.prefab", InteractionMode.UserAction);
        }
    }
}
