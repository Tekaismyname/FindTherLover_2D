using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.UI;
using FindTheLover.Crafting;
using FindTheLover.Crafting.Data;
using FindTheLover.Equipment.Data;
using FindTheLover.Items;
using FindTheLover.Player;
using FindTheLover.UI;
using FindTherLover.Player;

namespace FindTheLover.EditorTools
{
    /// <summary>
    /// One-click Editor Setup Tool for Modular Hand Socket Weapons, 5-Slot Hotbar UI,
    /// 8 Survival Pickup Items, and the Muck-Style 5-Tier Blacksmith Crafting Forge.
    /// </summary>
    public static class Day20WeaponSocketAndCraftingBuilder
    {
        private const string WeaponsSheetPath = "Assets/03_Art/Sprites/Weapon/Weapons.png";
        private const string WeaponsOutDir = "Assets/03_Art/Sprites/Weapon/Extracted";
        private const string PickupSheetPath = "Assets/03_Art/Sprites/Environment/Pickup_Items/Pickup_Items.png";
        private const string PickupOutDir = "Assets/03_Art/Sprites/Environment/Pickup_Items/Extracted";
        private const string EquipDataDir = "Assets/01_Data/Items/EquipmentItems";
        private const string PickupDataDir = "Assets/01_Data/Items/PickupItems";
        private const string CraftingDataDir = "Assets/01_Data/Crafting";

        [MenuItem("Tools/Find The Lover/Setup Day 20 (Modular Hand Socket + Hotbar + Crafting Forge)")]
        public static void SetupModularWeaponsHotbarAndCrafting()
        {
            Directory.CreateDirectory(Path.GetFullPath(WeaponsOutDir));
            Directory.CreateDirectory(Path.GetFullPath(PickupOutDir));
            Directory.CreateDirectory(Path.GetFullPath(EquipDataDir));
            Directory.CreateDirectory(Path.GetFullPath(PickupDataDir));
            Directory.CreateDirectory(Path.GetFullPath(CraftingDataDir));

            // 1. Extract 4 Weapons (Axe, Pickaxe, Sword, Torch) with Bottom-Left grip pivot (0.18, 0.18)
            var weaponSprites = ExtractHorizontalStripSprites(
                WeaponsSheetPath, WeaponsOutDir,
                new[] { "Weapon_Axe", "Weapon_Pickaxe", "Weapon_Sword", "Weapon_Torch" },
                pivot: new Vector2(0.18f, 0.18f), ppu: 245f, isolateMainColumns: true);

            // 2. Extract all 8 Pickup Items (Wood, Branch, Stone, IronOre, AmberCrystal, RedBerries, Fiber, Apple)
            var itemSprites = ExtractHorizontalStripSprites(
                PickupSheetPath, PickupOutDir,
                new[] { "Item_Wood", "Item_Branch", "Item_Stone", "Item_IronOre", "Item_AmberCrystal", "Item_RedBerries", "Item_FiberBundle", "Item_Apple" },
                pivot: new Vector2(0.5f, 0.5f), ppu: 128f);

            // 3. Create / update all 8 ItemDataSO blueprints
            var items = BuildAll8ItemAssets(itemSprites);

            // 4. Create 15 Modular Tiered EquipmentItemDataSO assets (5 tiers x Sword/Axe/Pickaxe) + update Torch
            var equipment = BuildAllTieredModularEquipment(weaponSprites);

            // 5. Create 14 CraftingRecipeSO blueprints
            var recipes = BuildAllCraftingRecipes(items, equipment);

            // 6. Wire CraftingForge system & build 3D ProBuilder Anvil beside Campfire Sanctuary
            WireCraftingForgeAndAnvil(recipes);

            // 7. Wire PlayerHotbarController onto Quene_Player (both Scene & Player.prefab)
            WirePlayerHotbarAndStarterGear(equipment, items["berries"]);

            // 8. Build Bottom-Center 5-Slot Hotbar UI & [C] Crafting Modal UI inside HUD_Canvas
            BuildHotbarAndCraftingUI();

            // 9. Refresh Master_GameCatalog & Linked Prefabs
            GameDataAndPrefabSetupBuilder.SetupGameDataAndHierarchyPrefabs();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorSceneManager.SaveOpenScenes();

            Debug.Log("<color=#00FFCC><b>[Day20Builder] Successfully built Modular Hand Socket (15 Tiered Weapons), 8 Items, 14 Crafting Recipes, Hotbar 1-5, Crafting Modal [C], and ProBuilder Blacksmith Anvil!</b></color>");
        }

        private static Dictionary<string, Sprite> ExtractHorizontalStripSprites(string sourcePath, string outDir, string[] names, Vector2 pivot, float ppu, bool isolateMainColumns = false)
        {
            var dict = new Dictionary<string, Sprite>();
            TextureImporter srcImp = AssetImporter.GetAtPath(sourcePath) as TextureImporter;
            if (srcImp != null && !srcImp.isReadable)
            {
                srcImp.isReadable = true;
                srcImp.textureCompression = TextureImporterCompression.Uncompressed;
                srcImp.SaveAndReimport();
            }

            Texture2D sheet = AssetDatabase.LoadAssetAtPath<Texture2D>(sourcePath);
            if (sheet == null) return dict;

            int count = names.Length;
            int cellW = sheet.width / count;
            int cellH = sheet.height;

            for (int i = 0; i < count; i++)
            {
                int startX = i * cellW;
                Color[] raw = sheet.GetPixels(startX, 0, cellW, cellH);
                Color32[] pixels = new Color32[cellW * cellH];

                bool[] colHasPixel = new bool[cellW];
                for (int y = 0; y < cellH; y++)
                {
                    for (int x = 0; x < cellW; x++)
                    {
                        Color32 c = raw[y * cellW + x];
                        bool isWhiteBg = c.a < 20 || (c.r >= 242 && c.g >= 242 && c.b >= 242);
                        if (isWhiteBg)
                        {
                            pixels[y * cellW + x] = new Color32(0, 0, 0, 0);
                        }
                        else
                        {
                            pixels[y * cellW + x] = c;
                            colHasPixel[x] = true;
                        }
                    }
                }

                // If isolateMainColumns is enabled, keep only the largest contiguous horizontal X-span
                // so tiny overlapping edge tips from neighboring sprites in the strip are cleanly removed.
                int allowedMinX = 0;
                int allowedMaxX = cellW - 1;
                if (isolateMainColumns)
                {
                    int bestStart = 0, bestLen = 0, curStart = -1;
                    for (int x = 0; x < cellW; x++)
                    {
                        if (colHasPixel[x])
                        {
                            if (curStart < 0) curStart = x;
                        }
                        else if (curStart >= 0)
                        {
                            int len = x - curStart;
                            if (len > bestLen) { bestLen = len; bestStart = curStart; }
                            curStart = -1;
                        }
                    }
                    if (curStart >= 0)
                    {
                        int len = cellW - curStart;
                        if (len > bestLen) { bestLen = len; bestStart = curStart; }
                    }
                    if (bestLen > 0)
                    {
                        allowedMinX = bestStart;
                        allowedMaxX = bestStart + bestLen - 1;
                    }
                }

                int minX = cellW, maxX = 0, minY = cellH, maxY = 0;
                for (int y = 0; y < cellH; y++)
                {
                    for (int x = 0; x < cellW; x++)
                    {
                        if (x < allowedMinX || x > allowedMaxX)
                        {
                            pixels[y * cellW + x] = new Color32(0, 0, 0, 0);
                            continue;
                        }
                        if (pixels[y * cellW + x].a > 0)
                        {
                            if (x < minX) minX = x;
                            if (x > maxX) maxX = x;
                            if (y < minY) minY = y;
                            if (y > maxY) maxY = y;
                        }
                    }
                }

                // Crop tightly with 8px transparent padding for crisp centered/grip alignment
                int pad = 8;
                int cropW = maxX >= minX ? (maxX - minX + 1) + pad * 2 : cellW;
                int cropH = maxY >= minY ? (maxY - minY + 1) + pad * 2 : cellH;
                int squareSize = Mathf.Max(cropW, cropH);

                Color32[] squarePixels = new Color32[squareSize * squareSize];
                if (maxX >= minX)
                {
                    int contentW = maxX - minX + 1;
                    int contentH = maxY - minY + 1;
                    int offX = (squareSize - contentW) / 2;
                    int offY = (squareSize - contentH) / 2;

                    for (int y = 0; y < contentH; y++)
                    {
                        for (int x = 0; x < contentW; x++)
                        {
                            squarePixels[(y + offY) * squareSize + (x + offX)] = pixels[(minY + y) * cellW + (minX + x)];
                        }
                    }
                }

                Texture2D outTex = new Texture2D(squareSize, squareSize, TextureFormat.RGBA32, false);
                outTex.SetPixels32(squarePixels);
                outTex.Apply();

                string outPath = $"{outDir}/{names[i]}.png";
                File.WriteAllBytes(Path.GetFullPath(outPath), outTex.EncodeToPNG());
                Object.DestroyImmediate(outTex);
                AssetDatabase.ImportAsset(outPath, ImportAssetOptions.ForceUpdate);

                TextureImporter ti = AssetImporter.GetAtPath(outPath) as TextureImporter;
                if (ti != null)
                {
                    ti.textureType = TextureImporterType.Sprite;
                    ti.spriteImportMode = SpriteImportMode.Single;
                    ti.spritePixelsPerUnit = ppu;
                    ti.filterMode = FilterMode.Point;
                    ti.alphaIsTransparency = true;
                    ti.textureCompression = TextureImporterCompression.Uncompressed;

                    var settings = new TextureImporterSettings();
                    ti.ReadTextureSettings(settings);
                    settings.spriteAlignment = (int)SpriteAlignment.Custom;
                    settings.spritePivot = pivot;
                    ti.SetTextureSettings(settings);
                    ti.SaveAndReimport();
                }

                Sprite sp = AssetDatabase.LoadAssetAtPath<Sprite>(outPath);
                if (sp != null) dict[names[i]] = sp;
            }

            return dict;
        }

        private static Dictionary<string, ItemDataSO> BuildAll8ItemAssets(Dictionary<string, Sprite> sprites)
        {
            var dict = new Dictionary<string, ItemDataSO>();

            dict["wood"] = UpsertItem($"{PickupDataDir}/Item_Wood_Data.asset", "wood_log", "Wood Log", ItemType.Resource, sprites.GetValueOrDefault("Item_Wood"), "Sturdy timber harvested from island trees.");
            dict["branch"] = UpsertItem($"{PickupDataDir}/Item_Branch_Data.asset", "wood_branch", "Oak Branch", ItemType.Resource, sprites.GetValueOrDefault("Item_Branch"), "Flexible branch used for weapon handles and torches.");
            dict["stone"] = UpsertItem($"{PickupDataDir}/Item_Stone_Data.asset", "stone_chunk", "Stone", ItemType.Resource, sprites.GetValueOrDefault("Item_Stone"), "Solid stone mined from boulders.");
            dict["iron"] = UpsertItem($"{PickupDataDir}/Item_IronOre_Data.asset", "iron_ore", "Iron Ore", ItemType.Resource, sprites.GetValueOrDefault("Item_IronOre"), "Raw iron ore forged into sharp steel gear.");
            dict["crystal"] = UpsertItem($"{PickupDataDir}/Item_AmberCrystal_Data.asset", "amber_crystal", "Adamantite Crystal", ItemType.Resource, sprites.GetValueOrDefault("Item_AmberCrystal"), "Radiant end-game crystal pulsing with ancient energy.");
            dict["berries"] = UpsertItem($"{PickupDataDir}/Item_Berries_Data.asset", "red_berries", "Red Berries", ItemType.Consumanle, sprites.GetValueOrDefault("Item_RedBerries"), "Sweet island berries restoring +25 HP (Press [5]).");
            dict["fiber"] = UpsertItem($"{PickupDataDir}/Item_Fiber_Data.asset", "plant_fiber", "Plant Fiber", ItemType.Resource, sprites.GetValueOrDefault("Item_FiberBundle"), "Tough woven plant fibers for binding tools and armor.");
            dict["apple"] = UpsertItem($"{PickupDataDir}/Item_Apple_Data.asset", "fresh_apple", "Crimson Apple", ItemType.Consumanle, sprites.GetValueOrDefault("Item_Apple"), "Crisp apple restoring vital health during night raids.");

            return dict;
        }

        private static ItemDataSO UpsertItem(string path, string id, string displayName, ItemType type, Sprite icon, string desc)
        {
            ItemDataSO asset = AssetDatabase.LoadAssetAtPath<ItemDataSO>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<ItemDataSO>();
                AssetDatabase.CreateAsset(asset, path);
            }
            asset.itemId = id;
            asset.itemName = displayName;
            asset.itemType = type;
            if (icon != null) asset.icon = icon;
            asset.description = desc;
            asset.maxStack = 99;
            EditorUtility.SetDirty(asset);
            return asset;
        }

        private static Dictionary<string, EquipmentItemDataSO> BuildAllTieredModularEquipment(Dictionary<string, Sprite> sprites)
        {
            var dict = new Dictionary<string, EquipmentItemDataSO>();
            var tiers = new (WeaponMaterialTier tier, string prefix, float swordDmg, float axeDmg, float pickDmg, float harvestMult)[]
            {
                (WeaponMaterialTier.Wood,       "Wood",       6f,  4f,  4f, 1.0f),
                (WeaponMaterialTier.Stone,      "Stone",     14f, 10f, 10f, 1.4f),
                (WeaponMaterialTier.Iron,       "Iron",      24f, 16f, 16f, 1.9f),
                (WeaponMaterialTier.Obsidian,   "Obsidian",  38f, 24f, 24f, 2.5f),
                (WeaponMaterialTier.Adamantite, "Adamantite",56f, 34f, 34f, 3.2f)
            };

            Sprite swordSp = sprites.GetValueOrDefault("Weapon_Sword");
            Sprite axeSp = sprites.GetValueOrDefault("Weapon_Axe");
            Sprite pickSp = sprites.GetValueOrDefault("Weapon_Pickaxe");
            Sprite torchSp = sprites.GetValueOrDefault("Weapon_Torch");

            foreach (var t in tiers)
            {
                Color tint = EquipmentItemDataSO.GetCanonicalTierColor(t.tier);
                string keyLower = t.prefix.ToLower();

                dict[$"sword_{keyLower}"] = UpsertModularEquip(
                    $"{EquipDataDir}/Sword_{t.prefix}.asset", $"sword_{keyLower}", $"{t.prefix} Sword",
                    EquipmentType.Sword, t.tier, swordSp, tint, t.swordDmg, 1.0f, 0.85f, 65f, -110f, 0.20f);

                dict[$"axe_{keyLower}"] = UpsertModularEquip(
                    $"{EquipDataDir}/Axe_{t.prefix}.asset", $"axe_{keyLower}", $"{t.prefix} Axe",
                    EquipmentType.Axe, t.tier, axeSp, tint, t.axeDmg, t.harvestMult, 0.82f, 80f, -95f, 0.24f);

                dict[$"pickaxe_{keyLower}"] = UpsertModularEquip(
                    $"{EquipDataDir}/Pickaxe_{t.prefix}.asset", $"pickaxe_{keyLower}", $"{t.prefix} Pickaxe",
                    EquipmentType.Pickaxe, t.tier, pickSp, tint, t.pickDmg, t.harvestMult, 0.82f, 80f, -95f, 0.24f);
            }

            // Update existing Item_Torch.asset so it displays the crisp Weapon_Torch icon on Hotbar Slot 4
            EquipmentItemDataSO torchAsset = AssetDatabase.LoadAssetAtPath<EquipmentItemDataSO>($"{EquipDataDir}/Item_Torch.asset");
            if (torchAsset != null)
            {
                torchAsset.equipmentType = EquipmentType.Torch;
                torchAsset.materialTier = WeaponMaterialTier.Wood;
                if (torchSp != null) torchAsset.weaponSprite = torchSp;
                torchAsset.tierTintColor = Color.white;
                torchAsset.visualScale = 0.78f;
                EditorUtility.SetDirty(torchAsset);
                dict["torch"] = torchAsset;
            }

            return dict;
        }

        private static EquipmentItemDataSO UpsertModularEquip(string path, string id, string displayName, EquipmentType eqType, WeaponMaterialTier tier, Sprite sprite, Color tint, float dmgBonus, float harvestMult, float scale, float startAng, float endAng, float duration)
        {
            EquipmentItemDataSO asset = AssetDatabase.LoadAssetAtPath<EquipmentItemDataSO>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<EquipmentItemDataSO>();
                AssetDatabase.CreateAsset(asset, path);
            }

            asset.itemId = id;
            asset.displayName = displayName;
            asset.equipmentType = eqType;
            asset.materialTier = tier;
            asset.weaponSprite = sprite;
            asset.tierTintColor = tint;
            asset.heldPrefab = null; // Pure Modular Hand Socket mode!
            asset.weaponDamageBonus = dmgBonus;
            asset.harvestPowerMultiplier = harvestMult;
            asset.visualScale = scale;
            asset.swingStartAngle = startAng;
            asset.swingEndAngle = endAng;
            asset.swingDuration = duration;
            asset.localOffset = Vector3.zero;
            asset.localRotation = Vector3.zero;

            EditorUtility.SetDirty(asset);
            return asset;
        }

        private static List<CraftingRecipeSO> BuildAllCraftingRecipes(Dictionary<string, ItemDataSO> items, Dictionary<string, EquipmentItemDataSO> equip)
        {
            var list = new List<CraftingRecipeSO>();

            // Stone Tier
            list.Add(UpsertRecipe("Recipe_01_StoneSword", "Stone Sword", WeaponMaterialTier.Stone, equip["sword_stone"], null, 1, (items["wood"], 4), (items["stone"], 6)));
            list.Add(UpsertRecipe("Recipe_02_StoneAxe", "Stone Axe", WeaponMaterialTier.Stone, equip["axe_stone"], null, 1, (items["wood"], 4), (items["stone"], 5)));
            list.Add(UpsertRecipe("Recipe_03_StonePickaxe", "Stone Pickaxe", WeaponMaterialTier.Stone, equip["pickaxe_stone"], null, 1, (items["wood"], 4), (items["stone"], 6)));

            // Iron Tier
            list.Add(UpsertRecipe("Recipe_04_IronSword", "Iron Sword", WeaponMaterialTier.Iron, equip["sword_iron"], null, 1, (items["wood"], 5), (items["iron"], 6)));
            list.Add(UpsertRecipe("Recipe_05_IronAxe", "Iron Axe", WeaponMaterialTier.Iron, equip["axe_iron"], null, 1, (items["wood"], 5), (items["iron"], 5)));
            list.Add(UpsertRecipe("Recipe_06_IronPickaxe", "Iron Pickaxe", WeaponMaterialTier.Iron, equip["pickaxe_iron"], null, 1, (items["wood"], 5), (items["iron"], 6)));

            // Obsidian Tier
            list.Add(UpsertRecipe("Recipe_07_ObsidianSword", "Obsidian Sword", WeaponMaterialTier.Obsidian, equip["sword_obsidian"], null, 1, (items["iron"], 6), (items["stone"], 10)));
            list.Add(UpsertRecipe("Recipe_08_ObsidianAxe", "Obsidian Axe", WeaponMaterialTier.Obsidian, equip["axe_obsidian"], null, 1, (items["iron"], 5), (items["stone"], 8)));
            list.Add(UpsertRecipe("Recipe_09_ObsidianPickaxe", "Obsidian Pickaxe", WeaponMaterialTier.Obsidian, equip["pickaxe_obsidian"], null, 1, (items["iron"], 6), (items["stone"], 10)));

            // Adamantite End-Game Tier
            list.Add(UpsertRecipe("Recipe_10_AdamantiteSword", "Adamantite Blade", WeaponMaterialTier.Adamantite, equip["sword_adamantite"], null, 1, (items["iron"], 8), (items["crystal"], 6)));
            list.Add(UpsertRecipe("Recipe_11_AdamantiteAxe", "Adamantite War-Axe", WeaponMaterialTier.Adamantite, equip["axe_adamantite"], null, 1, (items["iron"], 6), (items["crystal"], 5)));
            list.Add(UpsertRecipe("Recipe_12_AdamantitePickaxe", "Adamantite Pickaxe", WeaponMaterialTier.Adamantite, equip["pickaxe_adamantite"], null, 1, (items["iron"], 6), (items["crystal"], 6)));

            // Consumables & Survival Gear
            list.Add(UpsertRecipe("Recipe_13_BerryRation", "Berry Heal Ration (x3)", WeaponMaterialTier.Wood, null, items["berries"], 3, (items["wood"], 2)));
            list.Add(UpsertRecipe("Recipe_14_SurvivalTorch", "Survival Torch", WeaponMaterialTier.Wood, equip.GetValueOrDefault("torch"), null, 1, (items["wood"], 3)));

            return list;
        }

        private static CraftingRecipeSO UpsertRecipe(string fileName, string title, WeaponMaterialTier tier, EquipmentItemDataSO resEquip, ItemDataSO resItem, int resQty, params (ItemDataSO item, int qty)[] costs)
        {
            string path = $"{CraftingDataDir}/{fileName}.asset";
            CraftingRecipeSO recipe = AssetDatabase.LoadAssetAtPath<CraftingRecipeSO>(path);
            if (recipe == null)
            {
                recipe = ScriptableObject.CreateInstance<CraftingRecipeSO>();
                AssetDatabase.CreateAsset(recipe, path);
            }

            recipe.recipeId = fileName.ToLower();
            recipe.displayName = title;
            recipe.tier = tier;
            recipe.resultEquipment = resEquip;
            recipe.resultItem = resItem;
            recipe.resultItemQuantity = resQty;
            recipe.ingredients = new List<CraftingIngredient>();
            foreach (var c in costs)
            {
                if (c.item != null)
                {
                    recipe.ingredients.Add(new CraftingIngredient { item = c.item, requiredAmount = c.qty });
                }
            }

            EditorUtility.SetDirty(recipe);
            return recipe;
        }

        private static void WireCraftingForgeAndAnvil(List<CraftingRecipeSO> recipes)
        {
            CraftingForge forge = Object.FindAnyObjectByType<CraftingForge>();
            if (forge == null)
            {
                GameObject forgeObj = new GameObject("CraftingForge");
                forge = forgeObj.AddComponent<CraftingForge>();
            }
            SerializedObject so = new SerializedObject(forge);
            SerializedProperty prop = so.FindProperty("_recipes");
            prop.ClearArray();
            for (int i = 0; i < recipes.Count; i++)
            {
                prop.InsertArrayElementAtIndex(i);
                prop.GetArrayElementAtIndex(i).objectReferenceValue = recipes[i];
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(forge);

            PrefabUtility.SaveAsPrefabAssetAndConnect(forge.gameObject, "Assets/04_Prefabs/Systems/CraftingForge.prefab", InteractionMode.AutomatedAction);

            // Build stylized 3D ProBuilder Blacksmith Anvil beside Campfire Sanctuary
            GameObject existingAnvil = GameObject.Find("Crafting_Anvil_Forge");
            if (existingAnvil != null) Object.DestroyImmediate(existingAnvil);

            GameObject anvilRoot = new GameObject("Crafting_Anvil_Forge");
            GameObject campfire = GameObject.Find("Camfire_Sanctuary");
            Vector3 anchorPos = campfire != null ? campfire.transform.position + new Vector3(3.6f, 0f, -0.5f) : new Vector3(3.6f, 8.0f, -0.5f);
            Terrain t1 = GameObject.Find("Terrain")?.GetComponent<Terrain>();
            if (t1 != null) anchorPos.y = t1.SampleHeight(anchorPos) + t1.transform.position.y;
            anvilRoot.transform.position = anchorPos;

            Shader lit = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            Material ironMat = new Material(lit) { color = new Color(0.22f, 0.24f, 0.29f, 1f) };
            ironMat.SetFloat("_Metallic", 0.72f);
            ironMat.SetFloat("_Smoothness", 0.45f);
            string anvilMatPath = "Assets/03_Art/Materials/Items/Anvil_Iron_Mat.mat";
            if (AssetDatabase.LoadAssetAtPath<Material>(anvilMatPath) == null) AssetDatabase.CreateAsset(ironMat, anvilMatPath);
            else ironMat = AssetDatabase.LoadAssetAtPath<Material>(anvilMatPath);

            var baseBlock = ShapeGenerator.CreateShape(ShapeType.Cube, PivotLocation.Center);
            baseBlock.name = "Anvil_Base";
            baseBlock.transform.SetParent(anvilRoot.transform, false);
            baseBlock.transform.localScale = new Vector3(1.1f, 0.35f, 0.7f);
            baseBlock.transform.localPosition = new Vector3(0f, 0.175f, 0f);
            baseBlock.GetComponent<MeshRenderer>().sharedMaterial = ironMat;

            var waist = ShapeGenerator.CreateShape(ShapeType.Cube, PivotLocation.Center);
            waist.name = "Anvil_Waist";
            waist.transform.SetParent(anvilRoot.transform, false);
            waist.transform.localScale = new Vector3(0.65f, 0.45f, 0.45f);
            waist.transform.localPosition = new Vector3(0f, 0.52f, 0f);
            waist.GetComponent<MeshRenderer>().sharedMaterial = ironMat;

            var topDeck = ShapeGenerator.CreateShape(ShapeType.Cube, PivotLocation.Center);
            topDeck.name = "Anvil_TopDeck";
            topDeck.transform.SetParent(anvilRoot.transform, false);
            topDeck.transform.localScale = new Vector3(1.55f, 0.34f, 0.62f);
            topDeck.transform.localPosition = new Vector3(0.12f, 0.88f, 0f);
            topDeck.GetComponent<MeshRenderer>().sharedMaterial = ironMat;

            BoxCollider col = anvilRoot.AddComponent<BoxCollider>();
            col.center = new Vector3(0f, 0.55f, 0f);
            col.size = new Vector3(1.6f, 1.1f, 0.8f);

            GameObject zone1Parent = GameObject.Find("Zone1_Interactables");
            if (zone1Parent != null) anvilRoot.transform.SetParent(zone1Parent.transform, true);

            PrefabUtility.SaveAsPrefabAssetAndConnect(anvilRoot, "Assets/04_Prefabs/Environment/Crafting_Anvil_Forge.prefab", InteractionMode.AutomatedAction);
        }

        private static void WirePlayerHotbarAndStarterGear(Dictionary<string, EquipmentItemDataSO> equip, ItemDataSO berries)
        {
            void ApplyOnQuene(GameObject queneObj)
            {
                if (queneObj == null) return;
                PlayerHotbarController hotbar = queneObj.GetComponent<PlayerHotbarController>();
                if (hotbar == null) hotbar = queneObj.AddComponent<PlayerHotbarController>();

                SerializedObject hso = new SerializedObject(hotbar);
                SerializedProperty arr = hso.FindProperty("_equipmentSlots");
                arr.ClearArray();
                EquipmentItemDataSO[] starters = { equip["sword_wood"], equip["axe_wood"], equip["pickaxe_wood"], equip.GetValueOrDefault("torch") };
                for (int i = 0; i < 4; i++)
                {
                    arr.InsertArrayElementAtIndex(i);
                    arr.GetArrayElementAtIndex(i).objectReferenceValue = starters[i];
                }
                hso.FindProperty("_consumableHealItem").objectReferenceValue = berries;
                hso.ApplyModifiedPropertiesWithoutUndo();

                PlayerEquipment pe = queneObj.GetComponent<PlayerEquipment>();
                if (pe != null)
                {
                    SerializedObject peso = new SerializedObject(pe);
                    peso.FindProperty("initialEquipment").objectReferenceValue = equip["sword_wood"];
                    peso.FindProperty("handOffsetRight").vector2Value = new Vector2(0.74f, -0.78f);
                    peso.FindProperty("swingForwardReach").floatValue = 0.65f;
                    peso.ApplyModifiedPropertiesWithoutUndo();
                }

                EditorUtility.SetDirty(queneObj);
            }

            string playerPrefabPath = "Assets/04_Prefabs/Characters/Player.prefab";
            using (var scope = new PrefabUtility.EditPrefabContentsScope(playerPrefabPath))
            {
                Transform quene = scope.prefabContentsRoot.transform.Find("Quene_Player");
                if (quene != null) ApplyOnQuene(quene.gameObject);
            }

            GameObject scenePlayer = GameObject.Find("Player");
            if (scenePlayer != null)
            {
                Transform quene = scenePlayer.transform.Find("Quene_Player");
                if (quene != null) ApplyOnQuene(quene.gameObject);
            }
        }

        private static void BuildHotbarAndCraftingUI()
        {
            GameObject hudCanvas = GameObject.Find("HUD_Canvas");
            if (hudCanvas == null) return;

            // --- 1. BOTTOM-CENTER 5-SLOT SURVIVAL HOTBAR ---
            Transform existingHotbar = hudCanvas.transform.Find("Hotbar_Panel");
            if (existingHotbar != null) Object.DestroyImmediate(existingHotbar.gameObject);

            GameObject hotbarRoot = CreateUI("Hotbar_Panel", hudCanvas.transform);
            RectTransform hbRect = hotbarRoot.GetComponent<RectTransform>();
            hbRect.anchorMin = new Vector2(0.5f, 0f);
            hbRect.anchorMax = new Vector2(0.5f, 0f);
            hbRect.pivot = new Vector2(0.5f, 0f);
            hbRect.anchoredPosition = new Vector2(0f, 16f);
            hbRect.sizeDelta = new Vector2(470f, 92f);

            hotbarRoot.AddComponent<Image>().color = new Color(0.06f, 0.08f, 0.13f, 0.80f);

            HorizontalLayoutGroup hlg = hotbarRoot.AddComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(10, 10, 8, 8);
            hlg.spacing = 10f;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;

            Image[] borders = new Image[5];
            Image[] icons = new Image[5];
            TextMeshProUGUI[] labels = new TextMeshProUGUI[5];
            Button[] buttons = new Button[5];

            for (int i = 0; i < 5; i++)
            {
                GameObject slot = CreateUI($"HotbarSlot_{i + 1}", hotbarRoot.transform);
                borders[i] = slot.AddComponent<Image>();
                borders[i].color = i == 0 ? new Color(1f, 0.85f, 0.22f, 1f) : new Color(0.28f, 0.34f, 0.44f, 0.9f);
                buttons[i] = slot.AddComponent<Button>();

                GameObject inner = CreateUI("InnerBG", slot.transform);
                RectTransform ir = inner.GetComponent<RectTransform>();
                ir.anchorMin = Vector2.zero; ir.anchorMax = Vector2.one;
                ir.offsetMin = new Vector2(3f, 3f); ir.offsetMax = new Vector2(-3f, -3f);
                inner.AddComponent<Image>().color = new Color(0.11f, 0.13f, 0.20f, 0.96f);

                GameObject iconObj = CreateUI("Icon", inner.transform);
                RectTransform icr = iconObj.GetComponent<RectTransform>();
                icr.anchorMin = new Vector2(0.5f, 0.58f); icr.anchorMax = new Vector2(0.5f, 0.58f);
                icr.sizeDelta = new Vector2(46f, 46f);
                icons[i] = iconObj.AddComponent<Image>();
                icons[i].preserveAspect = true;
                icons[i].raycastTarget = false;

                labels[i] = CreateTMP("Label", inner.transform, $"[{i + 1}]", 11f, FontStyles.Bold, Color.white);
                RectTransform lr = labels[i].GetComponent<RectTransform>();
                lr.anchorMin = new Vector2(0f, 0f); lr.anchorMax = new Vector2(1f, 0.28f);
                lr.offsetMin = Vector2.zero; lr.offsetMax = Vector2.zero;
                labels[i].alignment = TextAlignmentOptions.Center;
            }

            // Tiny [C] Forge hint badge directly above Hotbar
            TextMeshProUGUI forgeHint = CreateTMP("ForgeHotkeyHint", hotbarRoot.transform, "PRESS [C] CRAFTING FORGE  |  [1-4] WEAPONS  |  [5] EAT", 12f, FontStyles.Bold, new Color(0.85f, 0.92f, 1f, 0.92f));
            forgeHint.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
            RectTransform fhRect = forgeHint.GetComponent<RectTransform>();
            fhRect.anchorMin = new Vector2(0.5f, 1f); fhRect.anchorMax = new Vector2(0.5f, 1f);
            fhRect.pivot = new Vector2(0.5f, 0f);
            fhRect.anchoredPosition = new Vector2(0f, 4f);
            fhRect.sizeDelta = new Vector2(480f, 20f);
            forgeHint.alignment = TextAlignmentOptions.Center;

            HotbarUIView hotbarView = hotbarRoot.AddComponent<HotbarUIView>();
            SerializedObject hbSO = new SerializedObject(hotbarView);
            SetObjArray(hbSO.FindProperty("_slotBorderImages"), borders);
            SetObjArray(hbSO.FindProperty("_slotIconImages"), icons);
            SetObjArray(hbSO.FindProperty("_slotLabelTexts"), labels);
            SetObjArray(hbSO.FindProperty("_slotButtons"), buttons);
            hbSO.ApplyModifiedPropertiesWithoutUndo();

            if (!PrefabUtility.IsPartOfPrefabInstance(hotbarRoot))
            {
                PrefabUtility.SaveAsPrefabAssetAndConnect(hotbarRoot, "Assets/04_Prefabs/UIs/Hotbar_Panel.prefab", InteractionMode.AutomatedAction);
            }

            // --- 2. MUCK-STYLE [C] CRAFTING FORGE MODAL ---
            Transform existingCraft = hudCanvas.transform.Find("CraftingModal_Root");
            if (existingCraft != null) Object.DestroyImmediate(existingCraft.gameObject);

            GameObject craftRoot = CreateUI("CraftingModal_Root", hudCanvas.transform);
            Stretch(craftRoot.GetComponent<RectTransform>());

            GameObject backdrop = CreateUI("CraftingBackdrop", craftRoot.transform);
            Stretch(backdrop.GetComponent<RectTransform>());
            backdrop.AddComponent<Image>().color = new Color(0.03f, 0.04f, 0.08f, 0.78f);

            GameObject panel = CreateUI("ForgeWindow", backdrop.transform);
            RectTransform pr = panel.GetComponent<RectTransform>();
            pr.anchorMin = new Vector2(0.5f, 0.5f); pr.anchorMax = new Vector2(0.5f, 0.5f);
            pr.sizeDelta = new Vector2(680f, 520f);
            panel.AddComponent<Image>().color = new Color(0.08f, 0.10f, 0.16f, 0.97f);

            TextMeshProUGUI title = CreateTMP("HeaderTitle", panel.transform, "BLACKSMITH FORGE  [C]", 24f, FontStyles.Bold, new Color(1f, 0.86f, 0.32f, 1f));
            RectTransform tr = title.GetComponent<RectTransform>();
            tr.anchorMin = new Vector2(0f, 0.89f); tr.anchorMax = new Vector2(0.85f, 1f);
            tr.offsetMin = new Vector2(20f, 0f); tr.offsetMax = Vector2.zero;
            title.alignment = TextAlignmentOptions.MidlineLeft;

            GameObject closeObj = CreateUI("CloseButton", panel.transform);
            RectTransform cr = closeObj.GetComponent<RectTransform>();
            cr.anchorMin = new Vector2(1f, 1f); cr.anchorMax = new Vector2(1f, 1f);
            cr.pivot = new Vector2(1f, 1f);
            cr.anchoredPosition = new Vector2(-12f, -10f);
            cr.sizeDelta = new Vector2(38f, 38f);
            closeObj.AddComponent<Image>().color = new Color(0.75f, 0.20f, 0.22f, 1f);
            Button closeBtn = closeObj.AddComponent<Button>();
            TextMeshProUGUI closeTxt = CreateTMP("X", closeObj.transform, "X", 18f, FontStyles.Bold, Color.white);
            Stretch(closeTxt.GetComponent<RectTransform>());
            closeTxt.alignment = TextAlignmentOptions.Center;

            // ScrollRect for Recipes
            GameObject scrollObj = CreateUI("RecipeScrollView", panel.transform);
            RectTransform srRect = scrollObj.GetComponent<RectTransform>();
            srRect.anchorMin = new Vector2(0f, 0f); srRect.anchorMax = new Vector2(1f, 0.88f);
            srRect.offsetMin = new Vector2(14f, 14f); srRect.offsetMax = new Vector2(-14f, -4f);
            scrollObj.AddComponent<Image>().color = new Color(0.05f, 0.06f, 0.10f, 0.85f);
            scrollObj.AddComponent<RectMask2D>();
            ScrollRect sr = scrollObj.AddComponent<ScrollRect>();
            sr.horizontal = false;
            sr.vertical = true;
            sr.scrollSensitivity = 35f;

            GameObject content = CreateUI("Content", scrollObj.transform);
            RectTransform contentRect = content.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0f, 1f); contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.anchoredPosition = Vector2.zero;
            contentRect.sizeDelta = new Vector2(0f, 0f);

            VerticalLayoutGroup vlg = content.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(8, 8, 8, 8);
            vlg.spacing = 8f;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            ContentSizeFitter csf = content.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            sr.viewport = srRect;
            sr.content = contentRect;
            backdrop.SetActive(false);

            CraftingModalUI modalUI = craftRoot.AddComponent<CraftingModalUI>();
            SerializedObject cmSO = new SerializedObject(modalUI);
            cmSO.FindProperty("_modalRootPanel").objectReferenceValue = backdrop;
            cmSO.FindProperty("_recipeListContainer").objectReferenceValue = content.transform;
            cmSO.FindProperty("_closeButton").objectReferenceValue = closeBtn;
            cmSO.ApplyModifiedPropertiesWithoutUndo();

            if (!PrefabUtility.IsPartOfPrefabInstance(craftRoot))
            {
                PrefabUtility.SaveAsPrefabAssetAndConnect(craftRoot, "Assets/04_Prefabs/UIs/CraftingModal_Root.prefab", InteractionMode.AutomatedAction);
            }
        }

        private static void SetObjArray<T>(SerializedProperty prop, T[] values) where T : Object
        {
            prop.ClearArray();
            for (int i = 0; i < values.Length; i++)
            {
                prop.InsertArrayElementAtIndex(i);
                prop.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            }
        }

        private static GameObject CreateUI(string name, Transform parent)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go;
        }

        private static TextMeshProUGUI CreateTMP(string name, Transform parent, string text, float size, FontStyles style, Color col)
        {
            GameObject go = CreateUI(name, parent);
            TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.fontStyle = style;
            tmp.color = col;
            tmp.raycastTarget = false;
            return tmp;
        }

        private static void Stretch(RectTransform r)
        {
            r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one;
            r.offsetMin = Vector2.zero; r.offsetMax = Vector2.zero;
        }
    }
}
