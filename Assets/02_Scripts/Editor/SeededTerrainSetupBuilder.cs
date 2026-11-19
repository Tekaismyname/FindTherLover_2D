using System.Collections.Generic;
using System.IO;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using FindTheLover.Environment;
using FindTheLover.Environment.Data;

namespace FindTheLover.EditorTools
{
    /// <summary>
    /// One-click Editor Setup Tool creating Early-Game and End-Game SeededTerrainProfileSO assets,
    /// wiring SeededWorldTerrainManager into the active scene, and regenerating seeded islands on demand.
    /// </summary>
    public static class SeededTerrainSetupBuilder
    {
        private const string TerrainDataFolder = "Assets/01_Data/Terrain";
        private const string EarlyProfilePath = TerrainDataFolder + "/EarlyGame_TerrainProfile.asset";
        private const string EndGameProfilePath = TerrainDataFolder + "/EndGame_TerrainProfile.asset";

        [MenuItem("Tools/Find The Lover/Setup Seeded Terrain System (Early & End Game)")]
        public static void SetupSeededTerrainSystem()
        {
            Directory.CreateDirectory(Path.GetFullPath(TerrainDataFolder));

            SeededTerrainProfileSO earlyProfile = CreateOrUpdateEarlyGameProfile();
            SeededTerrainProfileSO endGameProfile = CreateOrUpdateEndGameProfile();

            SeededWorldTerrainManager manager = WireManagerInScene(earlyProfile, endGameProfile);
            if (manager != null)
            {
                manager.GenerateWorldWithSeed(manager.WorldSeed);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

            Debug.Log("<color=#00FFCC><b>[SeededTerrainSetupBuilder] Configured Early-Game & End-Game Seeded Terrain System successfully!</b></color>");
        }

        [MenuItem("Tools/Find The Lover/Regenerate Terrain With Random Seed")]
        public static void RegenerateTerrainWithRandomSeed()
        {
            SeededWorldTerrainManager manager = Object.FindAnyObjectByType<SeededWorldTerrainManager>();
            if (manager == null)
            {
                SetupSeededTerrainSystem();
                manager = Object.FindAnyObjectByType<SeededWorldTerrainManager>();
            }

            if (manager != null)
            {
                int newSeed = manager.RegenerateWithRandomSeed();
                EditorUtility.SetDirty(manager);
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                Debug.Log($"<color=#FFD700>[SeededTerrainSetupBuilder] Regenerated archipelago with new random seed: {newSeed}</color>");
            }
        }

        private static SeededTerrainProfileSO CreateOrUpdateEarlyGameProfile()
        {
            SeededTerrainProfileSO p = AssetDatabase.LoadAssetAtPath<SeededTerrainProfileSO>(EarlyProfilePath);
            if (p == null)
            {
                p = ScriptableObject.CreateInstance<SeededTerrainProfileSO>();
                AssetDatabase.CreateAsset(p, EarlyProfilePath);
            }

            p.zoneName = "Zone 1 — Early-Game Coastal Sanctuary";
            p.archetype = TerrainZoneArchetype.EarlyGameMeadow;
            p.islandCenterXZ = Vector2.zero;
            p.baseGroundHeight = 8.0f;
            p.seaLevelHeight = 5.5f;
            p.islandBaseRadius = 205f;
            p.coastWobbleAmplitude = 24f;
            p.perimeterRimHeight = 26f;
            p.rimStartNormalizedRadius = 0.68f;
            p.noiseFrequency = 0.014f;
            p.interiorReliefAmplitude = 2.8f; // Gentle walkable rolling meadow hills
            p.noiseOctaves = 3;
            p.ridgeSharpness = 1.2f;
            p.sandMaxHeight = 6.9f;
            p.rockMinSlope = 19f;
            p.rockStartHeight = 13.8f;
            p.flatGroundRockRatio = 0.06f; // Lush green grass dominance
            p.pathwayHalfWidth = 4.2f;

            p.protectedAnchors = new List<TerrainAnchorZone>
            {
                new TerrainAnchorZone { label = "Campfire Sanctuary", worldCenterXZ = new Vector2(0f, 0f), flatRadius = 26f, blendWidth = 18f, targetWorldHeight = 8.0f },
                new TerrainAnchorZone { label = "Intro South Beach", worldCenterXZ = new Vector2(0f, -90f), flatRadius = 22f, blendWidth = 20f, targetWorldHeight = 6.3f },
                new TerrainAnchorZone { label = "Emerald Lake Basin", worldCenterXZ = new Vector2(0f, 74f), flatRadius = 17f, blendWidth = 14f, targetWorldHeight = 4.6f },
                new TerrainAnchorZone { label = "Eastern Bridgehead", worldCenterXZ = new Vector2(215f, 0f), flatRadius = 18f, blendWidth = 14f, targetWorldHeight = 8.0f }
            };

            p.protectedCorridors = new List<TerrainCorridorAnchor>
            {
                new TerrainCorridorAnchor { label = "South Beach to Campfire", startXZ = new Vector2(0f, -88f), endXZ = new Vector2(0f, 0f), halfWidth = 6f, blendWidth = 12f, targetWorldHeight = 8.0f, curveAmplitude = 6.5f },
                new TerrainCorridorAnchor { label = "Campfire to Emerald Lake", startXZ = new Vector2(0f, 0f), endXZ = new Vector2(0f, 58f), halfWidth = 5f, blendWidth = 10f, targetWorldHeight = 8.0f, curveAmplitude = 5.0f },
                new TerrainCorridorAnchor { label = "Campfire to Eastern Bridge", startXZ = new Vector2(0f, 0f), endXZ = new Vector2(222f, 0f), halfWidth = 10f, blendWidth = 18f, targetWorldHeight = 8.0f, curveAmplitude = 8.5f }
            };

            EditorUtility.SetDirty(p);
            return p;
        }

        private static SeededTerrainProfileSO CreateOrUpdateEndGameProfile()
        {
            SeededTerrainProfileSO p = AssetDatabase.LoadAssetAtPath<SeededTerrainProfileSO>(EndGameProfilePath);
            if (p == null)
            {
                p = ScriptableObject.CreateInstance<SeededTerrainProfileSO>();
                AssetDatabase.CreateAsset(p, EndGameProfilePath);
            }

            p.zoneName = "Zone 2+ — End-Game Toxic Ridge & Boss Caldera";
            p.archetype = TerrainZoneArchetype.EndGameCalamity;
            p.islandCenterXZ = new Vector2(455f, 0f);
            p.baseGroundHeight = 8.0f;
            p.seaLevelHeight = 5.5f;
            p.islandBaseRadius = 178f;
            p.coastWobbleAmplitude = 20f;
            p.perimeterRimHeight = 46f; // Towering jagged end-game cliffs
            p.rimStartNormalizedRadius = 0.72f;
            p.noiseFrequency = 0.020f;
            p.interiorReliefAmplitude = 6.5f; // Rugged dark rock ridges between trails
            p.noiseOctaves = 4;
            p.ridgeSharpness = 2.3f;
            p.sandMaxHeight = 6.7f;
            p.rockMinSlope = 14f; // Rocky volcanic aesthetic dominates faster
            p.rockStartHeight = 11.2f;
            p.flatGroundRockRatio = 0.34f; // Darker stony swamp / caldera floor
            p.pathwayHalfWidth = 4.0f;

            p.protectedAnchors = new List<TerrainAnchorZone>
            {
                new TerrainAnchorZone { label = "West Bridge Landing & Archway", worldCenterXZ = new Vector2(315f, 0f), flatRadius = 30f, blendWidth = 16f, targetWorldHeight = 8.0f },
                new TerrainAnchorZone { label = "Brazier Checkpoint 01", worldCenterXZ = new Vector2(342f, 0f), flatRadius = 14f, blendWidth = 10f, targetWorldHeight = 8.0f },
                new TerrainAnchorZone { label = "Toxic Swamp Lake", worldCenterXZ = new Vector2(415f, 18f), flatRadius = 14f, blendWidth = 12f, targetWorldHeight = 4.7f },
                new TerrainAnchorZone { label = "Soul Flame Sanctuary", worldCenterXZ = new Vector2(435f, -68f), flatRadius = 22f, blendWidth = 14f, targetWorldHeight = 8.0f },
                new TerrainAnchorZone { label = "Brazier Checkpoint 02", worldCenterXZ = new Vector2(485f, 14f), flatRadius = 16f, blendWidth = 12f, targetWorldHeight = 8.0f },
                new TerrainAnchorZone { label = "Thorn Barrier Gorge", worldCenterXZ = new Vector2(535f, 0f), flatRadius = 14f, blendWidth = 10f, targetWorldHeight = 8.0f },
                new TerrainAnchorZone { label = "EndGame Boss Caldera Arena", worldCenterXZ = new Vector2(582f, 0f), flatRadius = 32f, blendWidth = 14f, targetWorldHeight = 8.5f }
            };

            p.protectedCorridors = new List<TerrainCorridorAnchor>
            {
                new TerrainCorridorAnchor { label = "Bridgehead to Brazier 01", startXZ = new Vector2(280f, 0f), endXZ = new Vector2(345f, 0f), halfWidth = 8f, blendWidth = 14f, targetWorldHeight = 8.0f, curveAmplitude = 0f },
                new TerrainCorridorAnchor { label = "Northern Marsh Loop", startXZ = new Vector2(342f, 0f), endXZ = new Vector2(485f, 14f), halfWidth = 7f, blendWidth = 12f, targetWorldHeight = 8.0f, curveAmplitude = 18f },
                new TerrainCorridorAnchor { label = "Southern Sanctuary Trail", startXZ = new Vector2(342f, 0f), endXZ = new Vector2(435f, -68f), halfWidth = 7f, blendWidth = 12f, targetWorldHeight = 8.0f, curveAmplitude = -10f },
                new TerrainCorridorAnchor { label = "Sanctuary to Brazier 02", startXZ = new Vector2(435f, -68f), endXZ = new Vector2(485f, 14f), halfWidth = 7f, blendWidth = 12f, targetWorldHeight = 8.0f, curveAmplitude = 8f },
                new TerrainCorridorAnchor { label = "Brazier 02 to Thorn Gate & Boss Caldera", startXZ = new Vector2(485f, 14f), endXZ = new Vector2(582f, 0f), halfWidth = 8f, blendWidth = 14f, targetWorldHeight = 8.2f, curveAmplitude = 0f }
            };

            EditorUtility.SetDirty(p);
            return p;
        }

        private static SeededWorldTerrainManager WireManagerInScene(SeededTerrainProfileSO earlyProfile, SeededTerrainProfileSO endGameProfile)
        {
            SeededWorldTerrainManager manager = Object.FindAnyObjectByType<SeededWorldTerrainManager>();
            if (manager == null)
            {
                GameObject obj = new GameObject("SeededWorldTerrainManager");
                manager = obj.AddComponent<SeededWorldTerrainManager>();
            }

            Terrain earlyTerrain = null;
            Terrain endGameTerrain = null;
            foreach (Terrain t in Object.FindObjectsByType<Terrain>(FindObjectsSortMode.None))
            {
                if (t.name == "Terrain_Zone2_ToxicIsland") endGameTerrain = t;
                else if (t.name == "Terrain") earlyTerrain = t;
            }

            List<Transform> propParents = new List<Transform>();
            string[] candidateParents = { "Zone2_SwampFlora_Props", "Environment_Trees", "Trees", "Rocks", "WorldProps" };
            foreach (string pName in candidateParents)
            {
                GameObject found = GameObject.Find(pName);
                if (found != null) propParents.Add(found.transform);
            }

            NavMeshSurface navSurface = Object.FindAnyObjectByType<NavMeshSurface>();

            SerializedObject so = new SerializedObject(manager);
            so.FindProperty("_earlyGameTerrain").objectReferenceValue = earlyTerrain;
            so.FindProperty("_earlyGameProfile").objectReferenceValue = earlyProfile;
            so.FindProperty("_endGameTerrain").objectReferenceValue = endGameTerrain;
            so.FindProperty("_endGameProfile").objectReferenceValue = endGameProfile;
            so.FindProperty("_navMeshSurface").objectReferenceValue = navSurface;

            SerializedProperty propsArr = so.FindProperty("_propParentsToReground");
            propsArr.ClearArray();
            for (int i = 0; i < propParents.Count; i++)
            {
                propsArr.InsertArrayElementAtIndex(i);
                propsArr.GetArrayElementAtIndex(i).objectReferenceValue = propParents[i];
            }

            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(manager);
            return manager;
        }
    }
}
