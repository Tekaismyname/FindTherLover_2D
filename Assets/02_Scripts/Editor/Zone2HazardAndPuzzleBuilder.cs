using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;
using FindTheLover.Environment;
using FindTheLover.Environment.Hazards;
using FindTheLover.Environment.Puzzles;

namespace FindTheLover.EditorTools
{
    /// <summary>
    /// Editor Tool: Procedurally constructs high-detail 100% Native ProBuilder 3D models
    /// matching the approved concept art for Zone 2:
    /// 1. Brazier_Checkpoint_01 (Wrought-Iron Tripod Fire Basket with Cobblestone Base, Logs & 3D Campfire)
    /// 2. Soul_Flame_Altar (Dark Runestone Stepped Plinth, Beast Brackets & 3D Swirling Cyan Soul Flame)
    /// 3. Thorn_Barrier_Gate (Twin Pyramidion Obelisks, Gnarled Bramble Thorns, Chained Runic Seal)
    /// 4. Poison_Gas_Zone (Toxic Mist Hazard Volume)
    /// </summary>
    public static class Zone2HazardAndPuzzleBuilder
    {
        private const string EnvMatDir = "Assets/03_Art/Materials/Environment";
        private const string PrefabDir = "Assets/04_Prefabs/Environment";

        [MenuItem("Tools/Environment/🔥 Build Zone 2 Hazards & Puzzles (ProBuilder Native)")]
        public static void BuildAllZone2Features()
        {
            EnsureDirectories();
            CleanStrayProBuilderObjects();

            Material stoneCarved = GetOrCreateLitMaterial(
                $"{EnvMatDir}/Prop_StoneCarved_Mat.mat",
                "Prop_StoneCarved_Mat",
                new Color(0.56f, 0.53f, 0.48f, 1.0f),
                0.05f, 0.25f);

            Material stoneMossy = GetOrCreateLitMaterial(
                $"{EnvMatDir}/Prop_StoneMossy_Mat.mat",
                "Prop_StoneMossy_Mat",
                new Color(0.36f, 0.46f, 0.32f, 1.0f),
                0.05f, 0.22f);

            Material darkPit = GetOrCreateLitMaterial(
                $"{EnvMatDir}/Prop_DarkPit_Mat.mat",
                "Prop_DarkPit_Mat",
                new Color(0.10f, 0.09f, 0.09f, 1.0f),
                0.05f, 0.15f);

            Material antiqueBronze = GetOrCreateLitMaterial(
                $"{EnvMatDir}/Prop_AntiqueBronze_Mat.mat",
                "Prop_AntiqueBronze_Mat",
                new Color(0.72f, 0.50f, 0.24f, 1.0f),
                0.78f, 0.58f);

            Material forgedIron = GetOrCreateLitMaterial(
                $"{EnvMatDir}/Prop_ForgedIron_Mat.mat",
                "Prop_ForgedIron_Mat",
                new Color(0.28f, 0.31f, 0.35f, 1.0f),
                0.75f, 0.52f);

            Material spikeBronzeIron = GetOrCreateLitMaterial(
                $"{EnvMatDir}/Prop_SpikeBronzeIron_Mat.mat",
                "Prop_SpikeBronzeIron_Mat",
                new Color(0.54f, 0.41f, 0.28f, 1.0f),
                0.72f, 0.55f);

            Material polishedWood = GetOrCreateLitMaterial(
                $"{EnvMatDir}/Prop_PolishedWood_Mat.mat",
                "Prop_PolishedWood_Mat",
                new Color(0.46f, 0.28f, 0.15f, 1.0f),
                0.05f, 0.30f);

            Material darkBrambleWood = GetOrCreateLitMaterial(
                $"{EnvMatDir}/Prop_DarkBrambleWood_Mat.mat",
                "Prop_DarkBrambleWood_Mat",
                new Color(0.30f, 0.19f, 0.12f, 1.0f),
                0.08f, 0.28f);

            Material obsidianStone = GetOrCreateLitMaterial(
                $"{EnvMatDir}/Prop_DarkObsidianStone_Mat.mat",
                "Prop_DarkObsidianStone_Mat",
                new Color(0.22f, 0.27f, 0.33f, 1.0f),
                0.18f, 0.42f);

            Material runeCyanGlow = GetOrCreateEmissiveMaterial(
                $"{EnvMatDir}/Prop_RuneCyanGlow_Mat.mat",
                "Prop_RuneCyanGlow_Mat",
                new Color(0.12f, 0.92f, 1.0f, 1.0f),
                new Color(0.25f, 2.4f, 2.8f, 1.0f),
                0.65f);

            Material soulCoreGlow = GetOrCreateEmissiveMaterial(
                $"{EnvMatDir}/Prop_SoulCoreGlow_Mat.mat",
                "Prop_SoulCoreGlow_Mat",
                new Color(0.65f, 0.98f, 1.0f, 1.0f),
                new Color(1.1f, 3.4f, 3.8f, 1.0f),
                0.75f);

            Material emberOrangeGlow = GetOrCreateEmissiveMaterial(
                $"{EnvMatDir}/Prop_EmberOrangeGlow_Mat.mat",
                "Prop_EmberOrangeGlow_Mat",
                new Color(1.0f, 0.36f, 0.06f, 1.0f),
                new Color(2.8f, 0.75f, 0.10f, 1.0f),
                0.35f);

            Material flameGoldenGlow = GetOrCreateEmissiveMaterial(
                $"{EnvMatDir}/Prop_FlameGoldenGlow_Mat.mat",
                "Prop_FlameGoldenGlow_Mat",
                new Color(1.0f, 0.82f, 0.22f, 1.0f),
                new Color(3.4f, 2.2f, 0.35f, 1.0f),
                0.55f);

            // 1. Build & Place Tripod Wrought-Iron Brazier Checkpoint
            BuildTripodBrazierCheckpoint(
                "Brazier_Checkpoint_01",
                new Vector3(13.0f, 0f, 2.0f),
                BrazierRole.NormalCheckpoint,
                false,
                stoneCarved, stoneMossy, forgedIron, antiqueBronze, polishedWood, darkPit, emberOrangeGlow, flameGoldenGlow
            );

            // 2. Build & Place Monumental Dark Stone Soul Flame Altar
            BuildSoulFlameAltar(
                "Soul_Flame_Altar",
                new Vector3(22.0f, 0f, 3.5f),
                BrazierRole.SoulFlameAltar,
                true,
                obsidianStone, stoneCarved, stoneMossy, darkPit, runeCyanGlow, soulCoreGlow
            );

            // 3. Build & Place Twin-Obelisk Chained Thorn Barrier Gate
            BuildThornBarrierGate(
                "Thorn_Barrier_Gate",
                new Vector3(22.0f, 0f, -1.5f),
                stoneCarved, stoneMossy, obsidianStone, darkBrambleWood, spikeBronzeIron, forgedIron, antiqueBronze, runeCyanGlow
            );

            // 4. Build & Place Poison Gas Zone
            BuildPoisonGasZone(
                "Poison_Gas_Zone",
                new Vector3(17.0f, 0f, 1.0f),
                new Vector3(14.0f, 4.0f, 12.0f)
            );

            CleanStrayProBuilderObjects();
            UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("<color=cyan><b>[Zone2HazardAndPuzzleBuilder] Successfully built 100% Native 3D ProBuilder models matching the approved concept art!</b></color>");
        }

        private static void EnsureDirectories()
        {
            if (!AssetDatabase.IsValidFolder("Assets/04_Prefabs"))
                AssetDatabase.CreateFolder("Assets", "04_Prefabs");
            if (!AssetDatabase.IsValidFolder(PrefabDir))
                AssetDatabase.CreateFolder("Assets/04_Prefabs", "Environment");
        }

        private static Material GetOrCreateLitMaterial(string path, string matName, Color albedo, float metallic, float smoothness)
        {
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                Shader lit = Shader.Find("Universal Render Pipeline/Lit");
                if (lit == null) lit = Shader.Find("Standard");
                mat = new Material(lit);
                mat.name = matName;
                AssetDatabase.CreateAsset(mat, path);
            }
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", albedo);
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", albedo);
            if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", metallic);
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
            EditorUtility.SetDirty(mat);
            return mat;
        }

        private static Material GetOrCreateEmissiveMaterial(string path, string matName, Color baseColor, Color emissionColor, float smoothness)
        {
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                Shader lit = Shader.Find("Universal Render Pipeline/Lit");
                if (lit == null) lit = Shader.Find("Standard");
                mat = new Material(lit);
                mat.name = matName;
                AssetDatabase.CreateAsset(mat, path);
            }
            mat.EnableKeyword("_EMISSION");
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", baseColor);
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", baseColor);
            if (mat.HasProperty("_EmissionColor")) mat.SetColor("_EmissionColor", emissionColor);
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
            EditorUtility.SetDirty(mat);
            return mat;
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

        private static Vector3 SnapToTerrainAndClearGrass(Vector3 worldPos, float clearRadius = 2.2f)
        {
            Terrain terrain = Terrain.activeTerrain;
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
                {
                    td.SetDetailLayer(startX, startY, layer, emptyPatch);
                }
            }

            return worldPos;
        }

        /// <summary>
        /// Centers the ProBuilder mesh pivot at its exact geometric bounding center,
        /// then places and rotates it in world space for millimeter-accurate assembly.
        /// </summary>
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

            if (finalMesh.GetComponent<Collider>() != null)
            {
                Object.DestroyImmediate(finalMesh.GetComponent<Collider>());
            }

            return finalMesh;
        }

        // =========================================================================
        // 1. WROUGHT-IRON TRIPOD FIRE BASKET BRAZIER (Brazier_Checkpoint_01)
        // Matches: ancient_brazier_tripod_concept_1792729060484.jpg
        // =========================================================================
        private static GameObject BuildTripodBrazierCheckpoint(
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
            GameObject existing = GameObject.Find(objectName);
            if (existing != null) Object.DestroyImmediate(existing);

            worldPos = SnapToTerrainAndClearGrass(worldPos, 2.5f);

            GameObject brazierRoot = new GameObject(objectName);
            brazierRoot.transform.position = worldPos;
            brazierRoot.transform.rotation = Quaternion.identity;

            List<ProBuilderMesh> parts = new List<ProBuilderMesh>();

            // A. Cracked Cobblestone Circular Base Pad
            AddPart(parts,
                ShapeGenerator.GenerateCylinder(PivotLocation.Center, 12, 0.78f, 0.08f, 1, 1),
                worldPos + new Vector3(0f, 0.04f, 0f),
                Quaternion.identity,
                stoneMossy);

            // 8 Rustic Flagstone Pavers arranged around perimeter
            for (int i = 0; i < 8; i++)
            {
                float angleDeg = i * 45f;
                float rad = angleDeg * Mathf.Deg2Rad;
                Vector3 offset = new Vector3(Mathf.Sin(rad) * 0.56f, 0.06f, Mathf.Cos(rad) * 0.56f);
                AddPart(parts,
                    ShapeGenerator.GenerateCube(PivotLocation.Center, new Vector3(0.38f, 0.09f, 0.34f)),
                    worldPos + offset,
                    Quaternion.Euler(0f, angleDeg, 0f),
                    (i % 2 == 0) ? stoneCarved : stoneMossy);
            }

            // Center Ash Mound & Fallen Glowing Embers on the Cobblestone Base
            AddPart(parts,
                ShapeGenerator.GenerateCylinder(PivotLocation.Center, 10, 0.28f, 0.05f, 1, 1),
                worldPos + new Vector3(0f, 0.09f, 0f),
                Quaternion.identity,
                darkPit);

            for (int e = 0; e < 4; e++)
            {
                float eRad = (e * 90f + 25f) * Mathf.Deg2Rad;
                AddPart(parts,
                    ShapeGenerator.GenerateCube(PivotLocation.Center, new Vector3(0.07f, 0.04f, 0.07f)),
                    worldPos + new Vector3(Mathf.Sin(eRad) * 0.14f, 0.115f, Mathf.Cos(eRad) * 0.14f),
                    Quaternion.Euler(15f * e, e * 40f, 10f),
                    emberGlow);
            }

            // B. Two Horizontal Wrought-Iron Bracing Rings (Lower & Waist)
            AddPart(parts,
                ShapeGenerator.GeneratePipe(PivotLocation.Center, 0.47f, 0.055f, 0.055f, 16, 1),
                worldPos + new Vector3(0f, 0.42f, 0f),
                Quaternion.identity,
                forgedIron);

            AddPart(parts,
                ShapeGenerator.GeneratePipe(PivotLocation.Center, 0.40f, 0.06f, 0.055f, 16, 1),
                worldPos + new Vector3(0f, 0.68f, 0f),
                Quaternion.identity,
                forgedIron);

            // C. Three Hand-Forged Wrought-Iron Tripod Legs with Curled Scroll Feet (30, 150, 270 deg)
            for (int leg = 0; leg < 3; leg++)
            {
                float legAngle = leg * 120f + 30f;
                Quaternion legRot = Quaternion.Euler(0f, legAngle, 0f);
                Vector3 radialDir = legRot * Vector3.forward;

                // 1. Bottom Curled Scroll Foot
                AddPart(parts,
                    ShapeGenerator.GenerateCylinder(PivotLocation.Center, 12, 0.115f, 0.095f, 1, 1),
                    worldPos + radialDir * 0.66f + new Vector3(0f, 0.17f, 0f),
                    legRot * Quaternion.Euler(0f, 0f, 90f),
                    forgedIron);

                AddPart(parts,
                    ShapeGenerator.GenerateCylinder(PivotLocation.Center, 8, 0.05f, 0.11f, 1, 1),
                    worldPos + radialDir * 0.66f + new Vector3(0f, 0.17f, 0f),
                    legRot * Quaternion.Euler(0f, 0f, 90f),
                    antiqueBronze);

                // 2. Lower Swept Iron Leg Segment
                AddPart(parts,
                    ShapeGenerator.GenerateCube(PivotLocation.Center, new Vector3(0.08f, 0.38f, 0.095f)),
                    worldPos + radialDir * 0.52f + new Vector3(0f, 0.34f, 0f),
                    legRot * Quaternion.Euler(-32f, 0f, 0f),
                    forgedIron);

                // 3. Mid-Waist Iron Strut + Forged Bronze Clasp
                AddPart(parts,
                    ShapeGenerator.GenerateCube(PivotLocation.Center, new Vector3(0.08f, 0.30f, 0.095f)),
                    worldPos + radialDir * 0.41f + new Vector3(0f, 0.59f, 0f),
                    legRot * Quaternion.Euler(6f, 0f, 0f),
                    forgedIron);

                AddPart(parts,
                    ShapeGenerator.GenerateCube(PivotLocation.Center, new Vector3(0.11f, 0.07f, 0.12f)),
                    worldPos + radialDir * 0.41f + new Vector3(0f, 0.68f, 0f),
                    legRot,
                    antiqueBronze);

                // 4. Upper Flared Basket Arm
                AddPart(parts,
                    ShapeGenerator.GenerateCube(PivotLocation.Center, new Vector3(0.085f, 0.46f, 0.095f)),
                    worldPos + radialDir * 0.50f + new Vector3(0f, 0.93f, 0f),
                    legRot * Quaternion.Euler(24f, 0f, 0f),
                    forgedIron);

                // 5. Upper Outer Scroll Curl & Bronze Pin
                AddPart(parts,
                    ShapeGenerator.GenerateCylinder(PivotLocation.Center, 10, 0.095f, 0.09f, 1, 1),
                    worldPos + radialDir * 0.62f + new Vector3(0f, 1.07f, 0f),
                    legRot * Quaternion.Euler(0f, 0f, 90f),
                    forgedIron);

                AddPart(parts,
                    ShapeGenerator.GenerateCylinder(PivotLocation.Center, 8, 0.045f, 0.105f, 1, 1),
                    worldPos + radialDir * 0.62f + new Vector3(0f, 1.07f, 0f),
                    legRot * Quaternion.Euler(0f, 0f, 90f),
                    antiqueBronze);
            }

            // D. Ribbed Iron Fire Basket (Bottom Bowl + 16 Slatted Ribs + Riveted Bronze Rim)
            AddPart(parts,
                ShapeGenerator.GenerateCylinder(PivotLocation.Center, 12, 0.40f, 0.07f, 1, 1),
                worldPos + new Vector3(0f, 0.74f, 0f),
                Quaternion.identity,
                forgedIron);

            for (int r = 0; r < 16; r++)
            {
                float ribAngle = r * 22.5f;
                Quaternion ribRot = Quaternion.Euler(0f, ribAngle, 0f);
                Vector3 ribDir = ribRot * Vector3.forward;

                AddPart(parts,
                    ShapeGenerator.GenerateCube(PivotLocation.Center, new Vector3(0.042f, 0.44f, 0.042f)),
                    worldPos + ribDir * 0.47f + new Vector3(0f, 0.94f, 0f),
                    ribRot * Quaternion.Euler(23f, 0f, 0f),
                    forgedIron);
            }

            // Riveted Antique Bronze Upper Collar Rim
            AddPart(parts,
                ShapeGenerator.GeneratePipe(PivotLocation.Center, 0.59f, 0.095f, 0.065f, 16, 1),
                worldPos + new Vector3(0f, 1.15f, 0f),
                Quaternion.identity,
                antiqueBronze);

            // 12 Forged Iron Rivet Studs around the Bronze Rim
            for (int rv = 0; rv < 12; rv++)
            {
                float rvAngle = rv * 30f;
                Quaternion rvRot = Quaternion.Euler(0f, rvAngle, 0f);
                Vector3 rvDir = rvRot * Vector3.forward;
                AddPart(parts,
                    ShapeGenerator.GenerateCube(PivotLocation.Center, new Vector3(0.05f, 0.05f, 0.04f)),
                    worldPos + rvDir * 0.595f + new Vector3(0f, 1.15f, 0f),
                    rvRot,
                    forgedIron);
            }

            // E. Charcoal Bed, Glowing Red Coals & Criss-Crossed Firewood Logs inside Basket
            AddPart(parts,
                ShapeGenerator.GenerateCylinder(PivotLocation.Center, 10, 0.40f, 0.16f, 1, 1),
                worldPos + new Vector3(0f, 0.84f, 0f),
                Quaternion.identity,
                darkPit);

            for (int c = 0; c < 8; c++)
            {
                float cAngle = c * 45f + 11f;
                float cRad = cAngle * Mathf.Deg2Rad;
                float dist = (c % 2 == 0) ? 0.34f : 0.22f;
                float yH = (c % 2 == 0) ? 0.88f : 0.96f;
                AddPart(parts,
                    ShapeGenerator.GenerateCube(PivotLocation.Center, new Vector3(0.14f, 0.11f, 0.14f)),
                    worldPos + new Vector3(Mathf.Sin(cRad) * dist, yH, Mathf.Cos(cRad) * dist),
                    Quaternion.Euler(20f * c, cAngle, 15f * c),
                    emberGlow);
            }

            // 5 Criss-Crossed Firewood Logs stacked in the basket
            float[] logYaw = { 15f, 85f, 155f, 230f, 305f };
            float[] logPitch = { 56f, 62f, 52f, 64f, 48f };
            for (int l = 0; l < 5; l++)
            {
                Quaternion logRot = Quaternion.Euler(logPitch[l], logYaw[l], 0f);
                Vector3 logOffset = Quaternion.Euler(0f, logYaw[l], 0f) * new Vector3(0f, 1.08f, 0.07f);
                AddPart(parts,
                    ShapeGenerator.GenerateCylinder(PivotLocation.Center, 8, 0.072f, 0.58f, 1, 1),
                    worldPos + logOffset,
                    logRot,
                    woodMat);

                AddPart(parts,
                    ShapeGenerator.GenerateCylinder(PivotLocation.Center, 8, 0.076f, 0.08f, 1, 1),
                    worldPos + logOffset - new Vector3(0f, 0.05f, 0f),
                    logRot,
                    emberGlow);
            }

            CombineAllParts(parts, brazierRoot, "Brazier_Tripod_Structure");

            // Add Root Physical NavMesh Obstacle Collider
            BoxCollider rootCol = brazierRoot.AddComponent<BoxCollider>();
            rootCol.center = new Vector3(0f, 0.65f, 0f);
            rootCol.size = new Vector3(1.4f, 1.3f, 1.4f);
            rootCol.isTrigger = false;

            // F. 100% Native 3D Stylized Campfire Flame Sculpture (Inside Flame_Visual)
            GameObject flameObj = new GameObject("Flame_Visual");
            flameObj.transform.SetParent(brazierRoot.transform, false);
            flameObj.transform.localPosition = Vector3.zero;
            flameObj.transform.localRotation = Quaternion.identity;

            List<ProBuilderMesh> flameParts = new List<ProBuilderMesh>();
            // Central Golden-Yellow Core Flame
            AddPart(flameParts,
                ShapeGenerator.GenerateCone(PivotLocation.Center, 0.19f, 0.65f, 6),
                worldPos + new Vector3(0f, 1.42f, 0f),
                Quaternion.Euler(4f, 15f, -5f),
                flameGolden);

            // 5 Outer Orange-Red Flickering Flame Tongues
            for (int f = 0; f < 5; f++)
            {
                float fDeg = f * 72f;
                float fRad = fDeg * Mathf.Deg2Rad;
                Vector3 fPos = worldPos + new Vector3(Mathf.Sin(fRad) * 0.14f, 1.34f + (f % 2) * 0.05f, Mathf.Cos(fRad) * 0.14f);
                Quaternion fRot = Quaternion.Euler(0f, fDeg, 0f) * Quaternion.Euler(-14f, 0f, (f % 2 == 0) ? 12f : -12f);
                AddPart(flameParts,
                    ShapeGenerator.GenerateCone(PivotLocation.Center, 0.14f, 0.50f, 5),
                    fPos,
                    fRot,
                    (f % 2 == 0) ? emberGlow : flameGolden);
            }

            // 6 Floating Ember Sparks
            Vector3[] sparkOffsets =
            {
                new Vector3(0.16f, 1.72f, 0.10f),
                new Vector3(-0.18f, 1.82f, -0.08f),
                new Vector3(0.08f, 1.94f, -0.14f),
                new Vector3(-0.12f, 1.65f, 0.16f),
                new Vector3(0.22f, 1.58f, -0.12f),
                new Vector3(0.02f, 2.04f, 0.04f)
            };
            for (int sp = 0; sp < sparkOffsets.Length; sp++)
            {
                AddPart(flameParts,
                    ShapeGenerator.GenerateCube(PivotLocation.Center, new Vector3(0.045f, 0.07f, 0.045f)),
                    worldPos + sparkOffsets[sp],
                    Quaternion.Euler(35f, sp * 55f, 35f),
                    (sp % 2 == 0) ? flameGolden : emberGlow);
            }

            CombineAllParts(flameParts, flameObj, "Campfire_3D_Flame");

            // Flame Point Light
            GameObject lightObj = new GameObject("Brazier_Light");
            lightObj.transform.SetParent(brazierRoot.transform, false);
            lightObj.transform.localPosition = new Vector3(0f, 1.55f, 0f);

            Light pLight = lightObj.AddComponent<Light>();
            pLight.type = LightType.Point;
            pLight.color = new Color(1.0f, 0.65f, 0.2f);
            pLight.intensity = 2.4f;
            pLight.range = 5.0f;
            pLight.shadows = LightShadows.Soft;

            // Configure AncientBrazier Component
            AncientBrazier brazierComp = brazierRoot.AddComponent<AncientBrazier>();
            SerializedObject soBrazier = new SerializedObject(brazierComp);
            soBrazier.FindProperty("brazierRole").enumValueIndex = (int)role;
            soBrazier.FindProperty("startLit").boolValue = startLit;
            soBrazier.FindProperty("interactRadius").floatValue = 2.4f;
            soBrazier.FindProperty("safeRadius").floatValue = 3.5f;
            soBrazier.FindProperty("flameVisual").objectReferenceValue = flameObj;
            soBrazier.FindProperty("brazierLight").objectReferenceValue = pLight;
            soBrazier.ApplyModifiedProperties();

            flameObj.SetActive(startLit);
            pLight.enabled = startLit;

            PrefabUtility.SaveAsPrefabAssetAndConnect(brazierRoot, $"{PrefabDir}/Ancient_Brazier.prefab", InteractionMode.UserAction);
            return brazierRoot;
        }

        // =========================================================================
        // 2. MONUMENTAL DARK STONE SOUL FLAME ALTAR (Soul_Flame_Altar)
        // Matches: soul_flame_altar_concept_1792728023178.jpg
        // =========================================================================
        private static GameObject BuildSoulFlameAltar(
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
            GameObject existing = GameObject.Find(objectName);
            if (existing != null) Object.DestroyImmediate(existing);

            worldPos = SnapToTerrainAndClearGrass(worldPos, 2.8f);

            GameObject altarRoot = new GameObject(objectName);
            altarRoot.transform.position = worldPos;
            altarRoot.transform.rotation = Quaternion.identity;

            List<ProBuilderMesh> parts = new List<ProBuilderMesh>();

            // A. 3-Tiered Octagonal Dark Runestone Stepped Plinth
            // Tier 1 (Bottom Wide Octagon)
            AddPart(parts,
                ShapeGenerator.GenerateCylinder(PivotLocation.Center, 8, 0.92f, 0.16f, 1, 1),
                worldPos + new Vector3(0f, 0.08f, 0f),
                Quaternion.identity,
                obsidianStone);

            // Glowing Cyan Concentric Runic Circle 1 inlaid on top of Tier 1
            AddPart(parts,
                ShapeGenerator.GeneratePipe(PivotLocation.Center, 0.80f, 0.024f, 0.045f, 16, 1),
                worldPos + new Vector3(0f, 0.165f, 0f),
                Quaternion.identity,
                runeCyanGlow);

            // 8 Radial Cyan Rune Glyphs around Tier 1 Step & 4 Mossy Ruin Corner Stones
            for (int i = 0; i < 8; i++)
            {
                float deg = i * 45f + 22.5f;
                float a = deg * Mathf.Deg2Rad;
                Quaternion rot = Quaternion.Euler(0f, deg, 0f);
                AddPart(parts,
                    ShapeGenerator.GenerateCube(PivotLocation.Center, new Vector3(0.08f, 0.03f, 0.11f)),
                    worldPos + new Vector3(Mathf.Sin(a) * 0.77f, 0.165f, Mathf.Cos(a) * 0.77f),
                    rot,
                    runeCyanGlow);

                if (i % 2 == 0)
                {
                    AddPart(parts,
                        ShapeGenerator.GenerateCube(PivotLocation.Center, new Vector3(0.28f, 0.12f, 0.22f)),
                        worldPos + new Vector3(Mathf.Sin(a) * 0.88f, 0.06f, Mathf.Cos(a) * 0.88f),
                        rot,
                        stoneMossy);
                }
            }

            // Tier 2 (Middle Octagonal Step)
            AddPart(parts,
                ShapeGenerator.GenerateCylinder(PivotLocation.Center, 8, 0.72f, 0.18f, 1, 1),
                worldPos + new Vector3(0f, 0.25f, 0f),
                Quaternion.identity,
                obsidianStone);

            // 8 Glowing Cyan Rune Tablets inset on Tier 2 Vertical Faces
            for (int i = 0; i < 8; i++)
            {
                float deg = i * 45f + 22.5f;
                float rad = deg * Mathf.Deg2Rad;
                AddPart(parts,
                    ShapeGenerator.GenerateCube(PivotLocation.Center, new Vector3(0.22f, 0.08f, 0.03f)),
                    worldPos + new Vector3(Mathf.Sin(rad) * 0.67f, 0.25f, Mathf.Cos(rad) * 0.67f),
                    Quaternion.Euler(0f, deg, 0f),
                    runeCyanGlow);
            }

            // Glowing Cyan Concentric Runic Circle 2 inlaid on top of Tier 2
            AddPart(parts,
                ShapeGenerator.GeneratePipe(PivotLocation.Center, 0.62f, 0.024f, 0.04f, 16, 1),
                worldPos + new Vector3(0f, 0.345f, 0f),
                Quaternion.identity,
                runeCyanGlow);

            // Tier 3 (Upper Plinth Collar)
            AddPart(parts,
                ShapeGenerator.GenerateCylinder(PivotLocation.Center, 8, 0.52f, 0.14f, 1, 1),
                worldPos + new Vector3(0f, 0.41f, 0f),
                Quaternion.identity,
                stoneCarved);

            // B. Carved Runestone Pillar Shaft with 8 Glowing Vertical Cyan Runic Channels
            AddPart(parts,
                ShapeGenerator.GenerateCylinder(PivotLocation.Center, 8, 0.36f, 0.54f, 1, 1),
                worldPos + new Vector3(0f, 0.74f, 0f),
                Quaternion.identity,
                obsidianStone);

            for (int i = 0; i < 8; i++)
            {
                float deg = i * 45f + 22.5f;
                float rad = deg * Mathf.Deg2Rad;
                AddPart(parts,
                    ShapeGenerator.GenerateCube(PivotLocation.Center, new Vector3(0.045f, 0.44f, 0.03f)),
                    worldPos + new Vector3(Mathf.Sin(rad) * 0.34f, 0.74f, Mathf.Cos(rad) * 0.34f),
                    Quaternion.Euler(0f, deg, 0f),
                    runeCyanGlow);
            }

            // C. 4 Sculpted Stone Beast-Head Brackets supporting the Basin (45, 135, 225, 315 deg)
            for (int b = 0; b < 4; b++)
            {
                float bDeg = b * 90f + 45f;
                Quaternion bRot = Quaternion.Euler(0f, bDeg, 0f);
                Vector3 bDir = bRot * Vector3.forward;

                AddPart(parts,
                    ShapeGenerator.GenerateCube(PivotLocation.Center, new Vector3(0.18f, 0.24f, 0.26f)),
                    worldPos + bDir * 0.44f + new Vector3(0f, 0.92f, 0f),
                    bRot * Quaternion.Euler(28f, 0f, 0f),
                    stoneCarved);

                AddPart(parts,
                    ShapeGenerator.GenerateCube(PivotLocation.Center, new Vector3(0.22f, 0.16f, 0.24f)),
                    worldPos + bDir * 0.56f + new Vector3(0f, 1.04f, 0f),
                    bRot * Quaternion.Euler(-10f, 0f, 0f),
                    obsidianStone);

                AddPart(parts,
                    ShapeGenerator.GenerateCube(PivotLocation.Center, new Vector3(0.235f, 0.045f, 0.055f)),
                    worldPos + bDir * 0.60f + new Vector3(0f, 1.06f, 0f),
                    bRot,
                    runeCyanGlow);
            }

            // D. Ornate Deep Carved Stone Basin with Glowing Cyan Runes
            AddPart(parts,
                ShapeGenerator.GenerateCylinder(PivotLocation.Center, 8, 0.56f, 0.10f, 1, 1),
                worldPos + new Vector3(0f, 1.05f, 0f),
                Quaternion.identity,
                stoneCarved);

            AddPart(parts,
                ShapeGenerator.GeneratePipe(PivotLocation.Center, 0.74f, 0.26f, 0.14f, 8, 1),
                worldPos + new Vector3(0f, 1.22f, 0f),
                Quaternion.identity,
                obsidianStone);

            for (int i = 0; i < 8; i++)
            {
                float deg = i * 45f + 22.5f;
                float rad = deg * Mathf.Deg2Rad;
                Quaternion rRot = Quaternion.Euler(0f, deg, 0f);

                AddPart(parts,
                    ShapeGenerator.GenerateCube(PivotLocation.Center, new Vector3(0.24f, 0.16f, 0.05f)),
                    worldPos + new Vector3(Mathf.Sin(rad) * 0.69f, 1.22f, Mathf.Cos(rad) * 0.69f),
                    rRot,
                    stoneCarved);

                AddPart(parts,
                    ShapeGenerator.GenerateCube(PivotLocation.Center, new Vector3(0.14f, 0.07f, 0.06f)),
                    worldPos + new Vector3(Mathf.Sin(rad) * 0.70f, 1.22f, Mathf.Cos(rad) * 0.70f),
                    rRot,
                    runeCyanGlow);
            }

            // Glowing Cyan Upper Rim Ring
            AddPart(parts,
                ShapeGenerator.GeneratePipe(PivotLocation.Center, 0.69f, 0.025f, 0.045f, 16, 1),
                worldPos + new Vector3(0f, 1.355f, 0f),
                Quaternion.identity,
                runeCyanGlow);

            // Dark Pit Interior & Cyan Soul-Coal Crystals
            AddPart(parts,
                ShapeGenerator.GenerateCylinder(PivotLocation.Center, 8, 0.58f, 0.12f, 1, 1),
                worldPos + new Vector3(0f, 1.24f, 0f),
                Quaternion.identity,
                darkPit);

            for (int c = 0; c < 6; c++)
            {
                float cRad = (c * 60f) * Mathf.Deg2Rad;
                AddPart(parts,
                    ShapeGenerator.GenerateCube(PivotLocation.Center, new Vector3(0.14f, 0.09f, 0.14f)),
                    worldPos + new Vector3(Mathf.Sin(cRad) * 0.28f, 1.31f, Mathf.Cos(cRad) * 0.28f),
                    Quaternion.Euler(18f, c * 60f, 22f),
                    runeCyanGlow);
            }

            CombineAllParts(parts, altarRoot, "Altar_Monument_Structure");

            // Add Root Physical NavMesh Obstacle Collider
            BoxCollider rootCol = altarRoot.AddComponent<BoxCollider>();
            rootCol.center = new Vector3(0f, 0.70f, 0f);
            rootCol.size = new Vector3(1.65f, 1.40f, 1.65f);
            rootCol.isTrigger = false;

            // E. 100% Native 3D Swirling Cyan Soul Flame & Floating Wisps (Inside Flame_Visual)
            GameObject flameObj = new GameObject("Flame_Visual");
            flameObj.transform.SetParent(altarRoot.transform, false);
            flameObj.transform.localPosition = Vector3.zero;
            flameObj.transform.localRotation = Quaternion.identity;

            List<ProBuilderMesh> flame3DParts = new List<ProBuilderMesh>();
            // Central Brilliant Soul Core Spire
            AddPart(flame3DParts,
                ShapeGenerator.GenerateCone(PivotLocation.Center, 0.22f, 0.78f, 6),
                worldPos + new Vector3(0f, 1.66f, 0f),
                Quaternion.Euler(5f, 0f, -6f),
                soulCoreGlow);

            // 6 Swirling Turquoise-Cyan Outer Flame Tendrils
            for (int t = 0; t < 6; t++)
            {
                float tDeg = t * 60f;
                float tRad = tDeg * Mathf.Deg2Rad;
                Vector3 tPos = worldPos + new Vector3(Mathf.Sin(tRad) * 0.17f, 1.54f + (t % 2) * 0.06f, Mathf.Cos(tRad) * 0.17f);
                Quaternion tRot = Quaternion.Euler(0f, tDeg, 0f) * Quaternion.Euler(-16f, 0f, 18f);
                AddPart(flame3DParts,
                    ShapeGenerator.GenerateCone(PivotLocation.Center, 0.15f, 0.58f, 5),
                    tPos,
                    tRot,
                    (t % 2 == 0) ? runeCyanGlow : soulCoreGlow);
            }

            // 6 Floating Cyan Soul Wisps spiraling upward
            Vector3[] wispOffsets =
            {
                new Vector3(0.24f, 1.82f, 0.14f),
                new Vector3(-0.22f, 1.94f, -0.12f),
                new Vector3(0.14f, 2.10f, -0.16f),
                new Vector3(-0.16f, 1.72f, 0.22f),
                new Vector3(0.26f, 1.66f, -0.14f),
                new Vector3(-0.04f, 2.22f, 0.06f)
            };
            for (int w = 0; w < wispOffsets.Length; w++)
            {
                AddPart(flame3DParts,
                    ShapeGenerator.GenerateCube(PivotLocation.Center, new Vector3(0.06f, 0.10f, 0.06f)),
                    worldPos + wispOffsets[w],
                    Quaternion.Euler(45f, w * 45f, 45f),
                    (w % 2 == 0) ? soulCoreGlow : runeCyanGlow);
            }

            CombineAllParts(flame3DParts, flameObj, "SoulFlame_3D_Swirl");

            // Flame Point Light
            GameObject lightObj = new GameObject("Brazier_Light");
            lightObj.transform.SetParent(altarRoot.transform, false);
            lightObj.transform.localPosition = new Vector3(0f, 1.70f, 0f);

            Light pLight = lightObj.AddComponent<Light>();
            pLight.type = LightType.Point;
            pLight.color = new Color(0.15f, 0.95f, 1.0f);
            pLight.intensity = 2.8f;
            pLight.range = 6.0f;
            pLight.shadows = LightShadows.Soft;

            // Configure AncientBrazier Component
            AncientBrazier brazierComp = altarRoot.AddComponent<AncientBrazier>();
            SerializedObject soBrazier = new SerializedObject(brazierComp);
            soBrazier.FindProperty("brazierRole").enumValueIndex = (int)role;
            soBrazier.FindProperty("startLit").boolValue = startLit;
            soBrazier.FindProperty("interactRadius").floatValue = 2.4f;
            soBrazier.FindProperty("safeRadius").floatValue = 3.5f;
            soBrazier.FindProperty("flameVisual").objectReferenceValue = flameObj;
            soBrazier.FindProperty("brazierLight").objectReferenceValue = pLight;
            soBrazier.ApplyModifiedProperties();

            flameObj.SetActive(startLit);
            pLight.enabled = startLit;

            PrefabUtility.SaveAsPrefabAssetAndConnect(altarRoot, $"{PrefabDir}/Soul_Flame_Altar.prefab", InteractionMode.UserAction);
            return altarRoot;
        }

        // =========================================================================
        // 3. TWIN-OBELISK CHAINED THORN BARRIER GATE (Thorn_Barrier_Gate)
        // Matches: thorn_barrier_gate_concept_1792728044297.jpg
        // =========================================================================
        private static GameObject BuildThornBarrierGate(
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
            GameObject existing = GameObject.Find(objectName);
            if (existing != null) Object.DestroyImmediate(existing);

            worldPos = SnapToTerrainAndClearGrass(worldPos, 3.4f);

            GameObject gateRoot = new GameObject(objectName);
            gateRoot.transform.position = worldPos;
            gateRoot.transform.rotation = Quaternion.identity;

            List<ProBuilderMesh> parts = new List<ProBuilderMesh>();

            // A. Twin Ancient Carved Stone Obelisk Pillars (at Z = +2.10 and Z = -2.10)
            float[] zSides = { 2.10f, -2.10f };
            foreach (float zCenter in zSides)
            {
                Vector3 pBase = worldPos + new Vector3(0f, 0f, zCenter);

                // 1. Stepped Plinth Base Tier 1 & Tier 2
                AddPart(parts,
                    ShapeGenerator.GenerateCube(PivotLocation.Center, new Vector3(1.06f, 0.26f, 1.06f)),
                    pBase + new Vector3(0f, 0.13f, 0f),
                    Quaternion.identity,
                    stoneMossy);

                AddPart(parts,
                    ShapeGenerator.GenerateCube(PivotLocation.Center, new Vector3(0.88f, 0.22f, 0.88f)),
                    pBase + new Vector3(0f, 0.37f, 0f),
                    Quaternion.identity,
                    stoneCarved);

                // 2. Lower Obelisk Shaft
                AddPart(parts,
                    ShapeGenerator.GenerateCube(PivotLocation.Center, new Vector3(0.72f, 1.22f, 0.72f)),
                    pBase + new Vector3(0f, 1.09f, 0f),
                    Quaternion.identity,
                    stoneCarved);

                // 3. Mid Decorative Carved Collar Band
                AddPart(parts,
                    ShapeGenerator.GenerateCube(PivotLocation.Center, new Vector3(0.80f, 0.14f, 0.80f)),
                    pBase + new Vector3(0f, 1.77f, 0f),
                    Quaternion.identity,
                    obsidianStone);

                // 4. Upper Tapered Obelisk Shaft
                AddPart(parts,
                    ShapeGenerator.GenerateCube(PivotLocation.Center, new Vector3(0.64f, 1.16f, 0.64f)),
                    pBase + new Vector3(0f, 2.42f, 0f),
                    Quaternion.identity,
                    stoneCarved);

                // 5. Upper Cornice Molding
                AddPart(parts,
                    ShapeGenerator.GenerateCube(PivotLocation.Center, new Vector3(0.76f, 0.14f, 0.76f)),
                    pBase + new Vector3(0f, 3.07f, 0f),
                    Quaternion.identity,
                    stoneMossy);

                // 6. 4-Sided Pyramidion Cap (Rotated 45 deg so flat faces align with cube shaft)
                AddPart(parts,
                    ShapeGenerator.GenerateCone(PivotLocation.Center, 0.52f, 0.64f, 4),
                    pBase + new Vector3(0f, 3.46f, 0f),
                    Quaternion.Euler(0f, 45f, 0f),
                    stoneCarved);

                // 7. Glowing Cyan Carved Runes on Front (-X) & Back (+X) of each Obelisk
                float[] xFaces = { -0.365f, 0.365f };
                foreach (float xFace in xFaces)
                {
                    AddPart(parts,
                        ShapeGenerator.GenerateCube(PivotLocation.Center, new Vector3(0.025f, 0.68f, 0.055f)),
                        pBase + new Vector3(xFace, 1.12f, 0f),
                        Quaternion.identity,
                        runeCyanGlow);
                    AddPart(parts,
                        ShapeGenerator.GenerateCube(PivotLocation.Center, new Vector3(0.025f, 0.24f, 0.045f)),
                        pBase + new Vector3(xFace, 1.24f, 0.08f),
                        Quaternion.Euler(35f, 0f, 0f),
                        runeCyanGlow);
                    AddPart(parts,
                        ShapeGenerator.GenerateCube(PivotLocation.Center, new Vector3(0.025f, 0.24f, 0.045f)),
                        pBase + new Vector3(xFace, 1.00f, -0.08f),
                        Quaternion.Euler(-35f, 0f, 0f),
                        runeCyanGlow);

                    float xUpper = (xFace < 0f) ? -0.325f : 0.325f;
                    AddPart(parts,
                        ShapeGenerator.GenerateCube(PivotLocation.Center, new Vector3(0.025f, 0.62f, 0.05f)),
                        pBase + new Vector3(xUpper, 2.42f, 0f),
                        Quaternion.identity,
                        runeCyanGlow);
                    AddPart(parts,
                        ShapeGenerator.GenerateCube(PivotLocation.Center, new Vector3(0.025f, 0.22f, 0.045f)),
                        pBase + new Vector3(xUpper, 2.54f, -0.07f),
                        Quaternion.Euler(-35f, 0f, 0f),
                        runeCyanGlow);
                }

                // 8. Gnarled Dark Wood Roots gripping the Obelisk Base
                for (int r = 0; r < 4; r++)
                {
                    float rDeg = r * 90f + 35f;
                    Quaternion rRot = Quaternion.Euler(0f, rDeg, 0f);
                    Vector3 rDir = rRot * Vector3.forward;
                    AddPart(parts,
                        ShapeGenerator.GenerateCube(PivotLocation.Center, new Vector3(0.11f, 0.55f, 0.11f)),
                        pBase + rDir * 0.48f + new Vector3(0f, 0.26f, 0f),
                        rRot * Quaternion.Euler(32f, 0f, 12f),
                        darkWood);
                }
            }

            // B. Dense Intertwined Gnarled Dark Bramble Vines Spanning Z = -1.80 to +1.80 (Centered along X=0 so Seal sits proudly in front)
            float[] vineY = { 0.35f, 0.68f, 1.02f, 1.38f, 1.72f, 2.08f, 2.42f, 2.68f };
            float[] vinePitch = { -11f, 13f, -8f, 10f, -13f, 9f, -6f, 4f };
            float[] vineYaw = { 3f, -4f, 4f, -3f, 4f, -4f, 2f, -2f };
            float[] vineX = { -0.04f, 0.05f, -0.03f, 0.03f, -0.04f, 0.04f, -0.02f, 0.02f };

            for (int v = 0; v < vineY.Length; v++)
            {
                AddPart(parts,
                    ShapeGenerator.GenerateCube(PivotLocation.Center, new Vector3(0.20f, 0.22f, 3.65f)),
                    worldPos + new Vector3(vineX[v], vineY[v], 0f),
                    Quaternion.Euler(vinePitch[v], vineYaw[v], 0f),
                    darkWood);
            }

            // 6 Outer Diagonal Interwoven Bramble Trunks (Framing the central seal without blocking it)
            float[] diagZ = { -1.35f, -0.95f, 0.95f, 1.35f, -1.10f, 1.10f };
            float[] diagY = { 1.15f, 1.55f, 1.55f, 1.15f, 2.15f, 2.15f };
            float[] diagTilt = { 38f, -36f, 36f, -38f, 52f, -52f };
            for (int d = 0; d < diagZ.Length; d++)
            {
                AddPart(parts,
                    ShapeGenerator.GenerateCube(PivotLocation.Center, new Vector3(0.18f, 1.60f, 0.18f)),
                    worldPos + new Vector3((d % 2 == 0) ? -0.05f : 0.05f, diagY[d], diagZ[d]),
                    Quaternion.Euler(diagTilt[d], 0f, (d % 2 == 0) ? 6f : -6f),
                    darkWood);
            }

            // C. 42 Sharp Forged Iron & Bronze Spikes Jutting Outward (Front, Back & Top Crest)
            float[] spikeZ = { -1.55f, -1.30f, -1.05f, -0.82f, -0.58f, 0f, 0.58f, 0.82f, 1.05f, 1.30f, 1.55f };
            float[] spikeYRow1 = { 0.55f, 0.85f, 0.65f, 0.95f, 0.52f, 0.48f, 0.52f, 0.92f, 0.68f, 0.88f, 0.58f };
            float[] spikeYRow2 = { 1.65f, 1.95f, 2.25f, 1.85f, 2.38f, 2.52f, 2.38f, 1.88f, 2.22f, 1.92f, 1.68f };

            for (int s = 0; s < spikeZ.Length; s++)
            {
                Material sMat1 = (s % 2 == 0) ? spikeBronzeIron : forgedIron;
                Material sMat2 = (s % 2 == 0) ? forgedIron : spikeBronzeIron;

                AddPart(parts,
                    ShapeGenerator.GenerateCone(PivotLocation.Center, 0.11f, 0.62f, 6),
                    worldPos + new Vector3(-0.34f, spikeYRow1[s], spikeZ[s]),
                    Quaternion.Euler(0f, (s % 3 - 1) * 12f, 98f),
                    sMat1);

                AddPart(parts,
                    ShapeGenerator.GenerateCone(PivotLocation.Center, 0.105f, 0.58f, 6),
                    worldPos + new Vector3(0.34f, spikeYRow1[s] + 0.10f, -spikeZ[s]),
                    Quaternion.Euler(0f, (s % 3 - 1) * 12f, -98f),
                    sMat2);

                AddPart(parts,
                    ShapeGenerator.GenerateCone(PivotLocation.Center, 0.115f, 0.66f, 6),
                    worldPos + new Vector3(-0.34f, spikeYRow2[s], spikeZ[s]),
                    Quaternion.Euler(0f, (s % 3 - 1) * 10f, 82f),
                    sMat2);

                AddPart(parts,
                    ShapeGenerator.GenerateCone(PivotLocation.Center, 0.11f, 0.60f, 6),
                    worldPos + new Vector3(0.34f, spikeYRow2[s] - 0.10f, -spikeZ[s]),
                    Quaternion.Euler(0f, (s % 3 - 1) * 10f, -82f),
                    sMat1);

                // Top Crest Upward-Pointing Spikes
                AddPart(parts,
                    ShapeGenerator.GenerateCone(PivotLocation.Center, 0.10f, 0.58f, 6),
                    worldPos + new Vector3(0f, 2.86f, spikeZ[s]),
                    Quaternion.Euler((s - 5) * 4f, 0f, (s % 2 == 0) ? 14f : -14f),
                    spikeBronzeIron);
            }

            // D. Central Circular Cursed Runic Talisman Seal Plate at (0, 1.52, 0) (Thick & Unobstructed on both Front -X & Back +X)
            Vector3 sealCenter = worldPos + new Vector3(0f, 1.52f, 0f);
            Quaternion discRot = Quaternion.Euler(0f, 0f, 90f);

            // 1. Outer Forged Bronze Shield Rim & Dark Runestone Core Disc (Thickness = 0.44m so it sits cleanly outside the bramble vines)
            AddPart(parts,
                ShapeGenerator.GeneratePipe(PivotLocation.Center, 0.74f, 0.44f, 0.12f, 16, 1),
                sealCenter,
                discRot,
                antiqueBronze);

            AddPart(parts,
                ShapeGenerator.GenerateCylinder(PivotLocation.Center, 16, 0.64f, 0.40f, 1, 1),
                sealCenter,
                discRot,
                obsidianStone);

            // 12 Forged Iron Boss Rivets around the Outer Shield Rim
            for (int r = 0; r < 12; r++)
            {
                float rRad = (r * 30f) * Mathf.Deg2Rad;
                Vector3 rimPos = sealCenter + new Vector3(0f, Mathf.Cos(rRad) * 0.68f, Mathf.Sin(rRad) * 0.68f);
                AddPart(parts,
                    ShapeGenerator.GenerateCube(PivotLocation.Center, new Vector3(0.48f, 0.065f, 0.065f)),
                    rimPos,
                    Quaternion.Euler(r * 30f, 0f, 0f),
                    forgedIron);
            }

            // 2. Concentric Glowing Cyan Runic Rings & Central Vortex Sigil (Protruding 0.46m so 100% visible on Front & Back)
            AddPart(parts,
                ShapeGenerator.GeneratePipe(PivotLocation.Center, 0.56f, 0.45f, 0.045f, 16, 1),
                sealCenter,
                discRot,
                runeCyanGlow);

            AddPart(parts,
                ShapeGenerator.GeneratePipe(PivotLocation.Center, 0.30f, 0.46f, 0.045f, 12, 1),
                sealCenter,
                discRot,
                runeCyanGlow);

            AddPart(parts,
                ShapeGenerator.GenerateCylinder(PivotLocation.Center, 8, 0.13f, 0.47f, 1, 1),
                sealCenter,
                discRot,
                runeCyanGlow);

            // 8 Radial Glowing Cyan Runes between Inner & Outer Rings
            for (int g = 0; g < 8; g++)
            {
                float gDeg = g * 45f;
                float gRad = gDeg * Mathf.Deg2Rad;
                Vector3 gPos = sealCenter + new Vector3(0f, Mathf.Cos(gRad) * 0.43f, Mathf.Sin(gRad) * 0.43f);
                AddPart(parts,
                    ShapeGenerator.GenerateCube(PivotLocation.Center, new Vector3(0.455f, 0.14f, 0.045f)),
                    gPos,
                    Quaternion.Euler(gDeg + 20f, 0f, 0f),
                    runeCyanGlow);
            }

            // E. Heavy Forged Iron Chains Anchoring Central Seal to Twin Obelisks (Visible on both Front -X & Back +X)
            float[] chainXOffsets = { -0.22f, 0.22f };
            foreach (float xChain in chainXOffsets)
            {
                Vector3[] chainStarts =
                {
                    sealCenter + new Vector3(xChain, 0.46f, 0.54f),
                    sealCenter + new Vector3(xChain, -0.46f, 0.54f),
                    sealCenter + new Vector3(xChain, 0.46f, -0.54f),
                    sealCenter + new Vector3(xChain, -0.46f, -0.54f)
                };
                Vector3[] chainEnds =
                {
                    worldPos + new Vector3(xChain, 2.48f, 1.74f),
                    worldPos + new Vector3(xChain, 0.68f, 1.74f),
                    worldPos + new Vector3(xChain, 2.48f, -1.74f),
                    worldPos + new Vector3(xChain, 0.68f, -1.74f)
                };

                for (int c = 0; c < 4; c++)
                {
                    Vector3 startP = chainStarts[c];
                    Vector3 endP = chainEnds[c];
                    Vector3 dir = (endP - startP).normalized;
                    Quaternion chainLook = Quaternion.LookRotation(dir, Vector3.right);

                    AddPart(parts,
                        ShapeGenerator.GenerateCube(PivotLocation.Center, new Vector3(0.16f, 0.18f, 0.14f)),
                        endP,
                        Quaternion.identity,
                        forgedIron);

                    int linkCount = 9;
                    for (int l = 0; l < linkCount; l++)
                    {
                        float t = (l + 0.5f) / linkCount;
                        Vector3 linkPos = Vector3.Lerp(startP, endP, t);
                        Quaternion linkRot = chainLook * Quaternion.Euler(0f, 0f, (l % 2 == 0) ? 0f : 90f);

                        AddPart(parts,
                            ShapeGenerator.GenerateCube(PivotLocation.Center, new Vector3(0.11f, 0.06f, 0.22f)),
                            linkPos,
                            linkRot,
                            forgedIron);
                    }
                }
            }

            CombineAllParts(parts, gateRoot, "Thorn_Gate_Structure");

            // Add Physical Blocking BoxCollider
            BoxCollider gateCol = gateRoot.AddComponent<BoxCollider>();
            gateCol.center = new Vector3(0f, 1.65f, 0f);
            gateCol.size = new Vector3(0.95f, 3.30f, 5.10f);
            gateCol.isTrigger = false;

            // Add ThornBarrierGate Component
            ThornBarrierGate gateComp = gateRoot.AddComponent<ThornBarrierGate>();
            SerializedObject soGate = new SerializedObject(gateComp);
            soGate.FindProperty("interactRadius").floatValue = 2.8f;
            soGate.FindProperty("burnDuration").floatValue = 1.5f;
            soGate.ApplyModifiedProperties();

            PrefabUtility.SaveAsPrefabAssetAndConnect(gateRoot, $"{PrefabDir}/Thorn_Barrier_Gate.prefab", InteractionMode.UserAction);
            return gateRoot;
        }

        // =========================================================================
        // 4. POISON GAS ZONE BUILDER
        // =========================================================================
        private static GameObject BuildPoisonGasZone(string objectName, Vector3 worldPos, Vector3 size)
        {
            GameObject existing = GameObject.Find(objectName);
            if (existing != null) Object.DestroyImmediate(existing);

            worldPos = SnapToTerrainAndClearGrass(worldPos, 0f);
            worldPos.y += 0.05f;

            GameObject gasRoot = new GameObject(objectName);
            gasRoot.transform.position = worldPos;

            BoxCollider col = gasRoot.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.center = new Vector3(0f, size.y * 0.5f, 0f);
            col.size = size;

            PoisonGasZone gasComp = gasRoot.AddComponent<PoisonGasZone>();
            SerializedObject soGas = new SerializedObject(gasComp);
            soGas.FindProperty("damagePerTick").floatValue = 5.0f;
            soGas.FindProperty("tickInterval").floatValue = 1.0f;
            soGas.ApplyModifiedProperties();

            GameObject mistQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            mistQuad.name = "Toxic_Mist_GroundPlane";
            mistQuad.transform.SetParent(gasRoot.transform);
            mistQuad.transform.localPosition = new Vector3(0f, 0.08f, 0f);
            mistQuad.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            mistQuad.transform.localScale = new Vector3(size.x, size.z, 1f);

            if (mistQuad.GetComponent<Collider>() != null)
            {
                Object.DestroyImmediate(mistQuad.GetComponent<Collider>());
            }

            string mistMatPath = "Assets/03_Art/Materials/Environment/Toxic_Mist_Mat.mat";
            Material mistMat = AssetDatabase.LoadAssetAtPath<Material>(mistMatPath);
            if (mistMat == null)
            {
                Shader unlitShader = Shader.Find("Universal Render Pipeline/Unlit");
                if (unlitShader == null) unlitShader = Shader.Find("Sprites/Default");
                mistMat = new Material(unlitShader);
                mistMat.name = "Toxic_Mist_Mat";
                mistMat.color = new Color(0.2f, 0.85f, 0.25f, 0.35f);
                AssetDatabase.CreateAsset(mistMat, mistMatPath);
            }
            mistQuad.GetComponent<MeshRenderer>().sharedMaterial = mistMat;

            return gasRoot;
        }
    }
}
