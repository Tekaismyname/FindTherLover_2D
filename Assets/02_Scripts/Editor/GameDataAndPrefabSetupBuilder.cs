using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using FindTheLover.Augments;
using FindTheLover.Augments.Data;
using FindTheLover.Combat.Data;
using FindTheLover.Core;
using FindTheLover.Core.Data;
using FindTheLover.Enemies.Data;
using FindTheLover.Equipment.Data;
using FindTheLover.Items;

namespace FindTheLover.EditorTools
{
    /// <summary>
    /// Editor Automation Tool that builds Master_GameCatalog.asset, wires GameDataManager in the active scene,
    /// cleans up duplicate player components, and converts essential Hierarchy managers, UI panels, and
    /// ProBuilder architecture monuments into reusable linked Prefabs.
    /// </summary>
    public static class GameDataAndPrefabSetupBuilder
    {
        private const string CoreDataDir = "Assets/01_Data/Core";
        private const string MasterCatalogPath = CoreDataDir + "/Master_GameCatalog.asset";
        private const string SystemsPrefabDir = "Assets/04_Prefabs/Systems";
        private const string UIPrefabDir = "Assets/04_Prefabs/UIs";
        private const string ArchPrefabDir = "Assets/04_Prefabs/Environment/Architecture";

        [MenuItem("Tools/Find The Lover/Setup Master GameData & Generate Hierarchy Prefabs")]
        public static void SetupGameDataAndHierarchyPrefabs()
        {
            Directory.CreateDirectory(Path.GetFullPath(CoreDataDir));
            Directory.CreateDirectory(Path.GetFullPath(SystemsPrefabDir));
            Directory.CreateDirectory(Path.GetFullPath(UIPrefabDir));
            Directory.CreateDirectory(Path.GetFullPath(ArchPrefabDir));

            // 1. Build & populate Master_GameCatalog.asset with all static ScriptableObject blueprints
            GameCatalogSO catalog = BuildMasterGameCatalog();

            // 2. Fix duplicate PlayerAugmentStats on Player root (keep single authoritative one on Quene_Player)
            CleanupDuplicatePlayerAugmentStats();

            // 3. Create or update GameDataManager in active scene
            GameDataManager dataManager = WireGameDataManager(catalog);

            // 4. Clean up generic root folder naming
            GameObject genericGroup = GameObject.Find("GameObject");
            if (genericGroup != null && genericGroup.transform.Find("Camfire_Sanctuary") != null)
            {
                genericGroup.name = "Zone1_Interactables";
                EditorUtility.SetDirty(genericGroup);
            }

            // 5. Convert all essential Hierarchy managers, UI canvases/panels, and monuments into connected Prefabs
            int createdCount = ConvertHierarchyObjectsToPrefabs();

            // Capture an initial clean GameData snapshot in Editor so Inspector shows populated fields immediately
            if (dataManager != null)
            {
                dataManager.CaptureSnapshotFromWorld();
                EditorUtility.SetDirty(dataManager);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();

            Debug.Log($"<color=#00FFCC><b>[GameDataAndPrefabSetupBuilder] Master_GameCatalog built ({catalog.items.Count} items, {catalog.equipmentItems.Count} equipment, {catalog.augments.Count} augments, {catalog.enemies.Count} enemies, {catalog.resources.Count} resources) & {createdCount} Hierarchy Prefabs created/linked!</b></color>");
        }

        private static GameCatalogSO BuildMasterGameCatalog()
        {
            GameCatalogSO catalog = AssetDatabase.LoadAssetAtPath<GameCatalogSO>(MasterCatalogPath);
            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<GameCatalogSO>();
                AssetDatabase.CreateAsset(catalog, MasterCatalogPath);
            }

            catalog.items = FindAllAssetsOfType<ItemDataSO>("Assets/01_Data");
            catalog.equipmentItems = FindAllAssetsOfType<EquipmentItemDataSO>("Assets/01_Data");
            catalog.augments = FindAllAssetsOfType<AugmentDataSO>("Assets/01_Data");
            catalog.enemies = FindAllAssetsOfType<EnemyDataSO>("Assets/01_Data");
            catalog.resources = FindAllAssetsOfType<ResourceDataSO>("Assets/01_Data");
            catalog.RebuildLookups();

            EditorUtility.SetDirty(catalog);
            return catalog;
        }

        private static List<T> FindAllAssetsOfType<T>(string searchFolder) where T : Object
        {
            var list = new List<T>();
            string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}", new[] { searchFolder });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                T asset = AssetDatabase.LoadAssetAtPath<T>(path);
                if (asset != null && !list.Contains(asset))
                {
                    list.Add(asset);
                }
            }
            return list;
        }

        private static void CleanupDuplicatePlayerAugmentStats()
        {
            string playerPrefabPath = "Assets/04_Prefabs/Characters/Player.prefab";
            if (File.Exists(Path.GetFullPath(playerPrefabPath)))
            {
                using (var scope = new PrefabUtility.EditPrefabContentsScope(playerPrefabPath))
                {
                    var rootStats = scope.prefabContentsRoot.GetComponent<PlayerAugmentStats>();
                    Transform queneChild = scope.prefabContentsRoot.transform.Find("Quene_Player");
                    if (queneChild != null)
                    {
                        if (queneChild.GetComponent<PlayerAugmentStats>() == null)
                        {
                            queneChild.gameObject.AddComponent<PlayerAugmentStats>();
                        }
                        if (rootStats != null)
                        {
                            Object.DestroyImmediate(rootStats);
                        }
                    }
                }
            }

            GameObject scenePlayer = GameObject.Find("Player");
            if (scenePlayer != null)
            {
                Transform queneChild = scenePlayer.transform.Find("Quene_Player");
                var rootStats = scenePlayer.GetComponent<PlayerAugmentStats>();
                if (queneChild != null && queneChild.GetComponent<PlayerAugmentStats>() != null && rootStats != null)
                {
                    Object.DestroyImmediate(rootStats);
                    EditorUtility.SetDirty(scenePlayer);
                }
            }
        }

        private static GameDataManager WireGameDataManager(GameCatalogSO catalog)
        {
            GameDataManager manager = Object.FindAnyObjectByType<GameDataManager>();
            if (manager == null)
            {
                GameObject obj = new GameObject("GameDataManager");
                manager = obj.AddComponent<GameDataManager>();
            }

            SerializedObject so = new SerializedObject(manager);
            so.FindProperty("_masterCatalog").objectReferenceValue = catalog;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(manager);
            return manager;
        }

        private static int ConvertHierarchyObjectsToPrefabs()
        {
            int count = 0;

            // A. Core System Manager Prefabs (Assets/04_Prefabs/Systems/)
            var systemObjects = new (string sceneName, string prefabFile)[]
            {
                ("GameDataManager", "GameDataManager.prefab"),
                ("DayNight_Manager", "DayNight_Manager.prefab"),
                ("NightRaidDirector", "NightRaidDirector.prefab"),
                ("AugmentSystemManager", "AugmentSystemManager.prefab"),
                ("SeededWorldTerrainManager", "SeededWorldTerrainManager.prefab"),
                ("HitStopManager", "HitStopManager.prefab"),
                ("Nigh_Spawner", "EnemyPool_Manager.prefab"),
                ("EventSystem", "EventSystem.prefab")
            };

            foreach (var entry in systemObjects)
            {
                GameObject obj = GameObject.Find(entry.sceneName);
                if (SaveConnectedPrefab(obj, $"{SystemsPrefabDir}/{entry.prefabFile}"))
                {
                    count++;
                }
            }

            // B. Modular UI Prefabs (save child prefabs first so HUD_Canvas becomes a clean Nested Prefab)
            GameObject hudCanvas = GameObject.Find("HUD_Canvas");
            if (hudCanvas != null)
            {
                Transform modalRoot = hudCanvas.transform.Find("AugmentSelectionModal_Root");
                if (modalRoot != null)
                {
                    Transform card1 = modalRoot.Find("ModalBackdrop/CardsContainer/AugmentCard_1");
                    if (card1 != null && SavePrefabTemplateOnly(card1.gameObject, $"{UIPrefabDir}/AugmentCard_UI.prefab"))
                    {
                        count++;
                    }
                    if (SaveConnectedPrefab(modalRoot.gameObject, $"{UIPrefabDir}/AugmentSelectionModal_Root.prefab"))
                    {
                        count++;
                    }
                }

                Transform survivalHud = hudCanvas.transform.Find("SurvivalHUD_Panel");
                if (survivalHud != null && SaveConnectedPrefab(survivalHud.gameObject, $"{UIPrefabDir}/SurvivalHUD_Panel.prefab"))
                {
                    count++;
                }

                if (SaveConnectedPrefab(hudCanvas, $"{UIPrefabDir}/HUD_Canvas.prefab"))
                {
                    count++;
                }
            }

            // C. Reusable Environment & ProBuilder Architecture Prefabs (Assets/04_Prefabs/Environment/Architecture/)
            var archObjects = new (string scenePath, string prefabFile)[]
            {
                ("Zone2_ToxicCavern_Level/Architecture_ProBuilder/Bridge_Zone1_To_Zone2", "Bridge_Zone1_To_Zone2.prefab"),
                ("Zone2_ToxicCavern_Level/Architecture_ProBuilder/Zone2_Entrance_RuinArchway", "Zone2_Entrance_RuinArchway.prefab"),
                ("Zone2_ToxicCavern_Level/Architecture_ProBuilder/SoulSanctuary_AncientColonnade", "SoulSanctuary_AncientColonnade.prefab"),
                ("Zone2_ToxicCavern_Level/Architecture_ProBuilder/ThornGate_Gorge_Buttresses", "ThornGate_Gorge_Buttresses.prefab"),
                ("Ocean_Water", "Ocean_Water.prefab"),
                ("Lake_Water", "Lake_Water.prefab")
            };

            foreach (var entry in archObjects)
            {
                GameObject obj = GameObject.Find(entry.scenePath);
                if (SaveConnectedPrefab(obj, $"{ArchPrefabDir}/{entry.prefabFile}"))
                {
                    count++;
                }
            }

            return count;
        }

        private static bool SaveConnectedPrefab(GameObject sceneObj, string prefabPath)
        {
            if (sceneObj == null) return false;

            if (PrefabUtility.IsPartOfPrefabInstance(sceneObj))
            {
                // Already a prefab instance: apply overrides cleanly if linked to this path
                string existingPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(sceneObj);
                if (existingPath == prefabPath && PrefabUtility.IsOutermostPrefabInstanceRoot(sceneObj))
                {
                    PrefabUtility.ApplyPrefabInstance(sceneObj, InteractionMode.AutomatedAction);
                    return true;
                }

                // Child inside another prefab instance: cannot re-connect child directly
                return AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null;
            }

            GameObject saved = PrefabUtility.SaveAsPrefabAssetAndConnect(sceneObj, prefabPath, InteractionMode.AutomatedAction, out bool success);
            return success && saved != null;
        }

        private static bool SavePrefabTemplateOnly(GameObject sourceObj, string prefabPath)
        {
            if (sourceObj == null) return false;
            if (PrefabUtility.IsPartOfPrefabInstance(sourceObj))
            {
                return AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null;
            }
            GameObject saved = PrefabUtility.SaveAsPrefabAsset(sourceObj, prefabPath, out bool success);
            return success && saved != null;
        }
    }
}
