using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using FindTheLover.Augments;
using FindTheLover.CameraSystem;
using FindTheLover.Combat;
using FindTheLover.Combat.Data;
using FindTheLover.Core.Data;
using FindTheLover.Environment;
using FindTheLover.Items;
using FindTherLover.Player;

namespace FindTheLover.EditorTools
{
    /// <summary>
    /// Editor Setup Builder configuring:
    /// 1. Horizontal-only CameraLookController (locks vertical pitch on Main Camera).
    /// 2. 4-Way Modular Hand Socket offsets & duplicate PlayerAugmentStats cleanup on Quene_Player.
    /// 3. Full Stone / Iron Ore / Amber Crystal / Obsidian Mining System (Loot Prefabs, ResourceDataSO, all 4 Rock Prefabs & all Scene rocks).
    /// </summary>
    public static class MiningAndCameraPolishBuilder
    {
        private const string ResourceDataDir = "Assets/01_Data/Resources";
        private const string ItemPrefabDir = "Assets/04_Prefabs/Items/PickupItems";

        [MenuItem("Tools/Find The Lover/Setup Mining System + Horizontal Camera + Socket Fix")]
        public static void ApplyAllPolishAndMiningSetup()
        {
            // 1. Lock Main Camera Vertical Pitch (Horizontal Yaw Only)
            ConfigureHorizontalOnlyCamera();

            // 2. Clean duplicate PlayerAugmentStats & sync 4-way Modular Hand Socket offsets on Quene_Player + Player.prefab
            ConfigurePlayerSocketAndCleanup();

            // 3. Build / update world pickup prefabs (Item_Stone, Item_IronOre, Item_AmberCrystal)
            GameObject stoneLoot = EnsureLootPrefab(
                $"{ItemPrefabDir}/Item_Stone.prefab",
                "Assets/01_Data/Items/PickupItems/Item_Stone_Data.asset",
                "Assets/03_Art/Sprites/Environment/Pickup_Items/Extracted/Item_Stone.png");

            GameObject ironLoot = EnsureLootPrefab(
                $"{ItemPrefabDir}/Item_IronOre.prefab",
                "Assets/01_Data/Items/PickupItems/Item_IronOre_Data.asset",
                "Assets/03_Art/Sprites/Environment/Pickup_Items/Extracted/Item_IronOre.png");

            GameObject crystalLoot = EnsureLootPrefab(
                $"{ItemPrefabDir}/Item_AmberCrystal.prefab",
                "Assets/01_Data/Items/PickupItems/Item_AmberCrystal_Data.asset",
                "Assets/03_Art/Sprites/Environment/Pickup_Items/Extracted/Item_AmberCrystal.png");

            // 4. Configure ResourceDataSO blueprints (Wood = Axe, Stone/Iron/Crystal/Obsidian = Pickaxe)
            UpdateTreeBlueprint();

            ResourceDataSO stoneData = UpsertResourceData(
                $"{ResourceDataDir}/bigStone_Data.asset",
                "Stone Boulder", ResourceType.Stone, HarvestToolType.Pickaxe,
                maxHealth: 80f, efficiency: 2.0f, wrongToolMult: 0.25f, requireExact: false,
                dropPrefab: stoneLoot, minDrop: 2, maxDrop: 4);

            ResourceDataSO ironData = UpsertResourceData(
                $"{ResourceDataDir}/Rock_IronOre_Data.asset",
                "Iron Ore Vein", ResourceType.Iron, HarvestToolType.Pickaxe,
                maxHealth: 130f, efficiency: 2.2f, wrongToolMult: 0f, requireExact: true,
                dropPrefab: ironLoot, minDrop: 2, maxDrop: 4);

            ResourceDataSO crystalData = UpsertResourceData(
                $"{ResourceDataDir}/Rock_AmberCrystal_Data.asset",
                "Amber Crystal Cluster", ResourceType.Crystal, HarvestToolType.Pickaxe,
                maxHealth: 180f, efficiency: 2.4f, wrongToolMult: 0f, requireExact: true,
                dropPrefab: crystalLoot, minDrop: 2, maxDrop: 3);

            ResourceDataSO obsidianData = UpsertResourceData(
                $"{ResourceDataDir}/Rock_Obsidian_Data.asset",
                "Runic Obsidian Rock", ResourceType.Obsidian, HarvestToolType.Pickaxe,
                maxHealth: 160f, efficiency: 2.2f, wrongToolMult: 0f, requireExact: true,
                dropPrefab: ironLoot, minDrop: 3, maxDrop: 5);

            // 5. Wire ResourceNode onto all 4 Rock Prefabs
            WireRockPrefab("Assets/04_Prefabs/Environments/Rock_01_Quad.prefab", stoneData);
            WireRockPrefab("Assets/04_Prefabs/Environments/Rock_Ore_Quad.prefab", ironData);
            WireRockPrefab("Assets/04_Prefabs/Environments/Rock_Crystal_Quad.prefab", crystalData);
            WireRockPrefab("Assets/04_Prefabs/Environments/Zone2_RunicObsidianRock_Quad.prefab", obsidianData);

            // 6. Wire every rock instance already in the open scene so 100% of rocks are mineable!
            int wiredSceneRocks = WireAllSceneRocks(stoneData, ironData, crystalData, obsidianData);

            // 7. Place 3 accessible Zone 1 starter mining nodes (Stone, Iron Ore, Amber Crystal) near Campfire/Anvil
            PlaceStarterMiningNodesNearCampfire();

            // 8. Refresh Master_GameCatalog resources list
            RefreshCatalogResources(stoneData, ironData, crystalData, obsidianData);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorSceneManager.SaveOpenScenes();

            Debug.Log($"<color=#00FFAA><b>[MiningAndCameraPolishBuilder] Horizontal Camera Locked, 4-Way Hand Socket Synced, and {wiredSceneRocks} Scene Rocks Upgraded for Pickaxe Mining!</b></color>");
        }

        private static void ConfigureHorizontalOnlyCamera()
        {
            Camera cam = Camera.main;
            if (cam == null) return;

            CameraLookController clc = cam.GetComponent<CameraLookController>();
            if (clc != null)
            {
                SerializedObject so = new SerializedObject(clc);
                so.FindProperty("lockVerticalPitch").boolValue = true;
                so.FindProperty("fixedPitchAngle").floatValue = 15f;
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(clc);
            }
        }

        private static void ConfigurePlayerSocketAndCleanup()
        {
            void Apply(GameObject queneObj)
            {
                if (queneObj == null) return;

                // Clean any duplicate PlayerAugmentStats components (keep exactly one)
                PlayerAugmentStats[] stats = queneObj.GetComponents<PlayerAugmentStats>();
                for (int i = 1; i < stats.Length; i++)
                {
                    Object.DestroyImmediate(stats[i]);
                }

                PlayerEquipment pe = queneObj.GetComponent<PlayerEquipment>();
                if (pe != null)
                {
                    SerializedObject peso = new SerializedObject(pe);
                    peso.FindProperty("handOffsetRight").vector2Value = new Vector2(0.74f, -0.78f);
                    peso.FindProperty("handOffsetSide").vector2Value = new Vector2(0.46f, -0.76f);
                    peso.FindProperty("handOffsetBack").vector2Value = new Vector2(0.64f, -0.68f);
                    peso.FindProperty("depthLayerZOffset").floatValue = 0.08f;
                    peso.ApplyModifiedPropertiesWithoutUndo();
                    EditorUtility.SetDirty(pe);
                }

                // Ensure Quene's sprite starts with flipX = false
                SpriteRenderer sr = queneObj.GetComponent<SpriteRenderer>();
                if (sr != null) sr.flipX = false;

                EditorUtility.SetDirty(queneObj);
            }

            string playerPrefabPath = "Assets/04_Prefabs/Characters/Player.prefab";
            using (var scope = new PrefabUtility.EditPrefabContentsScope(playerPrefabPath))
            {
                Transform q = scope.prefabContentsRoot.transform.Find("Quene_Player");
                if (q != null) Apply(q.gameObject);
            }

            GameObject scenePlayer = GameObject.Find("Player");
            if (scenePlayer != null)
            {
                Transform q = scenePlayer.transform.Find("Quene_Player");
                if (q != null) Apply(q.gameObject);
            }
        }

        private static GameObject EnsureLootPrefab(string prefabPath, string itemDataPath, string spritePath)
        {
            ItemDataSO itemData = AssetDatabase.LoadAssetAtPath<ItemDataSO>(itemDataPath);
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);

            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (existing == null)
            {
                string stoneTemplate = $"{ItemPrefabDir}/Item_Stone.prefab";
                AssetDatabase.CopyAsset(stoneTemplate, prefabPath);
                existing = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            }

            if (existing != null)
            {
                using (var scope = new PrefabUtility.EditPrefabContentsScope(prefabPath))
                {
                    GameObject root = scope.prefabContentsRoot;
                    SpriteRenderer sr = root.GetComponentInChildren<SpriteRenderer>();
                    if (sr != null && sprite != null)
                    {
                        sr.sprite = sprite;
                        sr.color = Color.white;
                    }

                    LootItem loot = root.GetComponent<LootItem>();
                    if (loot != null && itemData != null)
                    {
                        SerializedObject lso = new SerializedObject(loot);
                        lso.FindProperty("itemData").objectReferenceValue = itemData;
                        lso.ApplyModifiedPropertiesWithoutUndo();
                    }
                }
            }

            return AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        }

        private static void UpdateTreeBlueprint()
        {
            ResourceDataSO tree = AssetDatabase.LoadAssetAtPath<ResourceDataSO>($"{ResourceDataDir}/Tree_Oak_Data.asset");
            if (tree != null)
            {
                tree.resourceType = ResourceType.Wood;
                tree.requiredTool = HarvestToolType.Axe;
                tree.toolEfficiencyMultiplier = 2.0f;
                tree.wrongToolDamageMultiplier = 0.35f;
                tree.requireExactTool = false;
                EditorUtility.SetDirty(tree);
            }
        }

        private static ResourceDataSO UpsertResourceData(
            string path, string displayName, ResourceType resType, HarvestToolType tool,
            float maxHealth, float efficiency, float wrongToolMult, bool requireExact,
            GameObject dropPrefab, int minDrop, int maxDrop)
        {
            ResourceDataSO asset = AssetDatabase.LoadAssetAtPath<ResourceDataSO>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<ResourceDataSO>();
                AssetDatabase.CreateAsset(asset, path);
            }

            asset.resourceName = displayName;
            asset.resourceType = resType;
            asset.requiredTool = tool;
            asset.toolEfficiencyMultiplier = efficiency;
            asset.wrongToolDamageMultiplier = wrongToolMult;
            asset.requireExactTool = requireExact;
            asset.maxHealth = maxHealth;
            asset.hitShakeIntensity = 0.14f;
            asset.hitShakeDuration = 0.16f;
            asset.dropPrefab = dropPrefab;
            asset.minDropCount = minDrop;
            asset.maxDropCount = maxDrop;
            asset.dropEjectionForce = 4.2f;

            EditorUtility.SetDirty(asset);
            return asset;
        }

        private static void WireRockPrefab(string prefabPath, ResourceDataSO data)
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) == null) return;

            using (var scope = new PrefabUtility.EditPrefabContentsScope(prefabPath))
            {
                GameObject root = scope.prefabContentsRoot;
                ResourceNode rn = root.GetComponent<ResourceNode>();
                if (rn == null) rn = root.AddComponent<ResourceNode>();

                SerializedObject so = new SerializedObject(rn);
                so.FindProperty("resourceData").objectReferenceValue = data;
                so.ApplyModifiedPropertiesWithoutUndo();

                BoxCollider bc = root.GetComponent<BoxCollider>();
                if (bc == null)
                {
                    bc = root.AddComponent<BoxCollider>();
                    bc.size = new Vector3(1.8f, 1.6f, 1.0f);
                    bc.center = new Vector3(0f, 0.8f, 0f);
                }
            }
        }

        private static int WireAllSceneRocks(ResourceDataSO stone, ResourceDataSO iron, ResourceDataSO crystal, ResourceDataSO obsidian)
        {
            int count = 0;
            GameObject[] allObjects = Object.FindObjectsByType<GameObject>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

            for (int i = 0; i < allObjects.Length; i++)
            {
                GameObject go = allObjects[i];
                if (go == null) continue;
                string n = go.name;
                if (n.IndexOf("Rock", System.StringComparison.OrdinalIgnoreCase) < 0) continue;
                // Skip container group folders
                if (go.GetComponent<Renderer>() == null && go.GetComponent<Collider>() == null) continue;

                ResourceDataSO chosen = stone;
                if (n.IndexOf("Crystal", System.StringComparison.OrdinalIgnoreCase) >= 0) chosen = crystal;
                else if (n.IndexOf("Ore", System.StringComparison.OrdinalIgnoreCase) >= 0) chosen = iron;
                else if (n.IndexOf("Obsidian", System.StringComparison.OrdinalIgnoreCase) >= 0) chosen = obsidian;

                ResourceNode rn = go.GetComponent<ResourceNode>();
                if (rn == null) rn = go.AddComponent<ResourceNode>();

                SerializedObject so = new SerializedObject(rn);
                so.FindProperty("resourceData").objectReferenceValue = chosen;
                so.ApplyModifiedPropertiesWithoutUndo();

                if (go.GetComponent<Collider>() == null)
                {
                    BoxCollider bc = go.AddComponent<BoxCollider>();
                    bc.size = new Vector3(1.8f, 1.6f, 1.0f);
                }

                EditorUtility.SetDirty(go);
                count++;
            }

            return count;
        }

        private static void PlaceStarterMiningNodesNearCampfire()
        {
            GameObject parent = GameObject.Find("Zone1_MiningNodes");
            if (parent == null)
            {
                parent = new GameObject("Zone1_MiningNodes");
            }

            void SpawnStarterIfMissing(string nodeName, string prefabPath, Vector3 offsetFromAnvil)
            {
                if (parent.transform.Find(nodeName) != null) return;
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                if (prefab == null) return;

                GameObject anvil = GameObject.Find("Crafting_Anvil_Forge");
                Vector3 anchor = anvil != null ? anvil.transform.position : new Vector3(3.4f, 8.0f, -2.6f);
                Vector3 pos = anchor + offsetFromAnvil;

                Terrain t = Terrain.activeTerrain;
                if (t != null) pos.y = t.SampleHeight(pos) + t.transform.position.y + 0.9f;

                GameObject inst = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent.transform);
                inst.name = nodeName;
                inst.transform.position = pos;
                EditorUtility.SetDirty(inst);
            }

            SpawnStarterIfMissing("Starter_Mine_StoneBoulder", "Assets/04_Prefabs/Environments/Rock_01_Quad.prefab", new Vector3(4.5f, 0f, 1.5f));
            SpawnStarterIfMissing("Starter_Mine_IronOreVein", "Assets/04_Prefabs/Environments/Rock_Ore_Quad.prefab", new Vector3(7.2f, 0f, -1.2f));
            SpawnStarterIfMissing("Starter_Mine_AmberCrystal", "Assets/04_Prefabs/Environments/Rock_Crystal_Quad.prefab", new Vector3(9.8f, 0f, 1.8f));
        }

        private static void RefreshCatalogResources(params ResourceDataSO[] extraResources)
        {
            GameCatalogSO catalog = AssetDatabase.LoadAssetAtPath<GameCatalogSO>("Assets/01_Data/Core/Master_GameCatalog.asset");
            if (catalog == null) return;

            foreach (var r in extraResources)
            {
                if (r != null && !catalog.resources.Contains(r))
                {
                    catalog.resources.Add(r);
                }
            }
            EditorUtility.SetDirty(catalog);
        }
    }
}
