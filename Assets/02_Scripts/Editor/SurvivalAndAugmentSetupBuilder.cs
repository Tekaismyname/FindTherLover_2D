using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using FindTheLover.Augments;
using FindTheLover.Augments.Data;
using FindTheLover.Enemies;
using FindTheLover.Enemies.Data;
using FindTheLover.UI;

namespace FindTheLover.EditorTools
{
    /// <summary>
    /// One-click Editor Builder that creates Data-Driven assets (NightRaid_Config, 18 TFT Augments, AugmentOrb prefab)
    /// and wires the Night Raid Director, Survival HUD, and 3-Card TFT Augment Selection Modal into the active scene.
    /// </summary>
    public static class SurvivalAndAugmentSetupBuilder
    {
        private const string NightRaidConfigPath = "Assets/01_Data/Enemies/NightRaid_Config.asset";
        private const string AugmentsFolder = "Assets/01_Data/Augments";
        private const string AugmentOrbMatPath = "Assets/03_Art/Materials/Items/AugmentOrb_Mat.mat";
        private const string AugmentOrbPrefabPath = "Assets/04_Prefabs/Items/AugmentOrb.prefab";

        [MenuItem("Tools/Find The Lover/Setup Survival Night Raid & TFT Augments")]
        public static void SetupSurvivalAndAugments()
        {
            EnsureFolders();

            // 1. Create or update NightRaid_Config.asset wired with all 5 enemy archetypes
            NightRaidConfigSO raidConfig = CreateOrUpdateNightRaidConfig();

            // 2. Create or update all 18 TFT Augment ScriptableObjects (6 Bronze, 6 Silver, 6 Gold)
            List<AugmentDataSO> catalog = CreateOrUpdateAll18Augments();

            // 3. Create or update AugmentOrb.prefab
            AugmentOrb orbPrefab = CreateOrUpdateAugmentOrbPrefab();

            // 4. Wire Managers & PlayerAugmentStats in active scene
            WireSceneManagers(raidConfig, catalog, orbPrefab);
            AttachPlayerAugmentStats();

            // 5. Build SurvivalHUD and 3-Card TFT Augment Selection Modal inside Canvas
            BuildSurvivalAndAugmentUI();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

            Debug.Log("<color=#00FFCC><b>[SurvivalAndAugmentSetupBuilder] Successfully set up NightRaidDirector, 18 TFT Augments, AugmentOrb prefab, SurvivalHUD, and Augment Selection Modal!</b></color>");
        }

        private static void EnsureFolders()
        {
            Directory.CreateDirectory(Path.GetFullPath(AugmentsFolder));
            Directory.CreateDirectory(Path.GetFullPath("Assets/03_Art/Materials/Items"));
            Directory.CreateDirectory(Path.GetFullPath("Assets/04_Prefabs/Items"));
        }

        private static NightRaidConfigSO CreateOrUpdateNightRaidConfig()
        {
            NightRaidConfigSO config = AssetDatabase.LoadAssetAtPath<NightRaidConfigSO>(NightRaidConfigPath);
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<NightRaidConfigSO>();
                AssetDatabase.CreateAsset(config, NightRaidConfigPath);
            }

            config.targetSurvivalDays = 20;
            config.calamityBossDays = new int[] { 5, 10, 15, 20 };
            config.baseNightBudget = 10;
            config.budgetPerDayMultiplier = 6;
            config.calamityBudgetMultiplier = 1.6f;
            config.dailyHealthScalePercent = 0.12f;
            config.dailyDamageScalePercent = 0.08f;
            config.eliteSpawnChance = 0.22f;
            config.spawnIntervalSeconds = 4.0f;
            config.maxSimultaneousRaiders = 14;
            config.minSpawnRadius = 25f;
            config.maxSpawnRadius = 35f;
            config.navMeshSnapDistance = 8f;

            config.enemyRoster = new List<NightEnemyRosterEntry>();
            TryAddRoster(config.enemyRoster, "Forest Goblin", "Assets/04_Prefabs/Characters/Enemy_Goblin.prefab", 2, 1, 5.0f);
            TryAddRoster(config.enemyRoster, "Shadow Bat", "Assets/04_Prefabs/Characters/Enemy_ShadowBat.prefab", 3, 1, 3.5f);
            TryAddRoster(config.enemyRoster, "Skeleton Goblin", "Assets/04_Prefabs/Characters/Enemy_Skeleton.prefab", 4, 2, 3.2f);
            TryAddRoster(config.enemyRoster, "Spore Shroom", "Assets/04_Prefabs/Characters/Enemy_SporeShroom.prefab", 5, 3, 2.5f);
            TryAddRoster(config.enemyRoster, "Magma Scarab", "Assets/04_Prefabs/Characters/Enemy_MagmaScarab.prefab", 5, 4, 2.2f);

            EditorUtility.SetDirty(config);
            return config;
        }

        private static void TryAddRoster(List<NightEnemyRosterEntry> list, string label, string prefabPath, int cost, int minDay, float weight)
        {
            GameObject prefabObj = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefabObj == null) return;
            EnemyAI ai = prefabObj.GetComponent<EnemyAI>();
            if (ai == null) return;

            list.Add(new NightEnemyRosterEntry
            {
                label = label,
                prefab = ai,
                budgetCost = cost,
                minDayToAppear = minDay,
                spawnWeight = weight
            });
        }

        private static List<AugmentDataSO> CreateOrUpdateAll18Augments()
        {
            var definitions = new (string file, string id, string title, string desc, AugmentTier tier, AugmentCategory cat, AugmentEffectType effect, float v1, float v2, bool stack, int maxStack)[]
            {
                // --- 6 BRONZE AUGMENTS ---
                ("Aug_Bronze_01_WindSneakers", "aug_bronze_sneakers", "Wind Sneakers", "Increases movement speed by +20% for nimble kiting at night.", AugmentTier.Bronze, AugmentCategory.StatModifier, AugmentEffectType.MoveSpeedPercent, 0.20f, 0f, true, 3),
                ("Aug_Bronze_02_HeartyStew", "aug_bronze_stew", "Hearty Camp Stew", "Grants +40 Max Health and restores 40 HP immediately.", AugmentTier.Bronze, AugmentCategory.StatModifier, AugmentEffectType.MaxHealthFlat, 40f, 0f, true, 3),
                ("Aug_Bronze_03_VampiricFang", "aug_bronze_vamp", "Vampiric Fang", "Restores 5% of all melee damage dealt as Health.", AugmentTier.Bronze, AugmentCategory.OnHitEffect, AugmentEffectType.LifestealPercent, 0.05f, 0f, true, 3),
                ("Aug_Bronze_04_MinerInstinct", "aug_bronze_miner", "Miner's Instinct", "Doubles (+100%) wood and stone yield when harvesting resources.", AugmentTier.Bronze, AugmentCategory.StatModifier, AugmentEffectType.HarvestYieldMultiplier, 1.0f, 0f, false, 1),
                ("Aug_Bronze_05_EternalWick", "aug_bronze_wick", "Eternal Torch Wick", "Extends survival torch burn duration by +100% (up to 80s).", AugmentTier.Bronze, AugmentCategory.StatModifier, AugmentEffectType.TorchDurationPercent, 1.0f, 0f, false, 1),
                ("Aug_Bronze_06_ReinforcedGuard", "aug_bronze_guard", "Reinforced Bark Guard", "Reduces all incoming monster damage by 15%.", AugmentTier.Bronze, AugmentCategory.StatModifier, AugmentEffectType.DamageReductionPercent, 0.15f, 0f, true, 2),

                // --- 6 SILVER AUGMENTS ---
                ("Aug_Silver_01_ChainLightning", "aug_silver_lightning", "Chain Lightning", "Melee strikes arc lightning to 3 nearby enemies for 45% weapon damage.", AugmentTier.Silver, AugmentCategory.OnHitEffect, AugmentEffectType.ChainLightningOnHit, 0.45f, 3f, true, 2),
                ("Aug_Silver_02_BlazingDash", "aug_silver_dash", "Blazing Dash Trail", "Sprinting scorches nearby pursuing monsters for 14 damage every 0.5s.", AugmentTier.Silver, AugmentCategory.OnHitEffect, AugmentEffectType.BlazingDashTrail, 14f, 0.5f, false, 1),
                ("Aug_Silver_03_GuardianSoul", "aug_silver_shield", "Guardian Soul Shell", "Generates a 30 HP protective barrier that refreshes every 20 seconds.", AugmentTier.Silver, AugmentCategory.StatModifier, AugmentEffectType.RegeneratingShield, 30f, 20f, true, 2),
                ("Aug_Silver_04_ToxicRebound", "aug_silver_rebound", "Toxic Spore Rebound", "Taking damage unleashes a 25 DMG retaliation nova in a 4.5m radius.", AugmentTier.Silver, AugmentCategory.OnHitEffect, AugmentEffectType.ToxicReboundNova, 25f, 4.5f, false, 1),
                ("Aug_Silver_05_PartnerRadiance", "aug_silver_partner", "Partner's Radiance", "Empowers melee strikes by +25% attack damage through lover synergy.", AugmentTier.Silver, AugmentCategory.CompanionBond, AugmentEffectType.PartnerDamageAura, 0.25f, 4f, true, 2),
                ("Aug_Silver_06_WindSlash", "aug_silver_windslash", "Gale Blade Mastery", "Sharpens combo attacks for +30% bonus melee damage.", AugmentTier.Silver, AugmentCategory.OnHitEffect, AugmentEffectType.WindSlashFinisher, 0.30f, 6f, false, 1),

                // --- 6 GOLD AUGMENTS ---
                ("Aug_Gold_01_ImmortalBond", "aug_gold_immortal", "Immortal Lover Bond", "Defies death once: automatically revives with 60% HP and 3 seconds of invincibility.", AugmentTier.Gold, AugmentCategory.CompanionBond, AugmentEffectType.ImmortalLoverBond, 0.60f, 3f, false, 1),
                ("Aug_Gold_02_CelestialWrath", "aug_gold_celestial", "Celestial Thunderstorm", "Every 6 seconds, calls down a 120 DMG lightning bolt on the healthiest nearby foe.", AugmentTier.Gold, AugmentCategory.UltimatePassive, AugmentEffectType.CelestialThunderstorm, 120f, 6f, false, 1),
                ("Aug_Gold_03_GoldenTouch", "aug_gold_midas", "Golden Touch of Midas", "Doubles gold and rare drop yield (+100%) and boosts harvesting by +50%.", AugmentTier.Gold, AugmentCategory.UltimatePassive, AugmentEffectType.GoldenMidasTouch, 1.0f, 0f, false, 1),
                ("Aug_Gold_04_EternalRing", "aug_gold_ring", "Eternal Anniversary Ring", "Grants an unbreakable vow reducing all incoming Boss & Raider damage by 40%.", AugmentTier.Gold, AugmentCategory.CompanionBond, AugmentEffectType.EternalRingProtection, 0.40f, 0f, false, 1),
                ("Aug_Gold_05_BerserkerFrenzy", "aug_gold_berserker", "Crimson Moon Frenzy", "Surges power: +45% Melee Damage and +25% Movement Speed.", AugmentTier.Gold, AugmentCategory.UltimatePassive, AugmentEffectType.BerserkerFrenzy, 0.45f, 0.25f, false, 1),
                ("Aug_Gold_06_ArcaneOverdrive", "aug_gold_arcane", "Soul Flame Overdrive", "Infuses blade with Soul Flame: +35% Melee Damage and +10% Lifesteal.", AugmentTier.Gold, AugmentCategory.UltimatePassive, AugmentEffectType.ArcaneOverdrive, 0.35f, 0.10f, false, 1)
            };

            List<AugmentDataSO> catalog = new List<AugmentDataSO>();
            foreach (var d in definitions)
            {
                string path = $"{AugmentsFolder}/{d.file}.asset";
                AugmentDataSO asset = AssetDatabase.LoadAssetAtPath<AugmentDataSO>(path);
                if (asset == null)
                {
                    asset = ScriptableObject.CreateInstance<AugmentDataSO>();
                    AssetDatabase.CreateAsset(asset, path);
                }

                asset.augmentId = d.id;
                asset.displayName = d.title;
                asset.description = d.desc;
                asset.tier = d.tier;
                asset.category = d.cat;
                asset.effectType = d.effect;
                asset.primaryValue = d.v1;
                asset.secondaryValue = d.v2;
                asset.allowStacking = d.stack;
                asset.maxStacks = d.maxStack;

                EditorUtility.SetDirty(asset);
                catalog.Add(asset);
            }

            return catalog;
        }

        private static AugmentOrb CreateOrUpdateAugmentOrbPrefab()
        {
            Shader lit = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            Material orbMat = AssetDatabase.LoadAssetAtPath<Material>(AugmentOrbMatPath);
            if (orbMat == null)
            {
                orbMat = new Material(lit);
                AssetDatabase.CreateAsset(orbMat, AugmentOrbMatPath);
            }
            orbMat.SetColor("_BaseColor", new Color(1f, 0.84f, 0.2f, 1f));
            orbMat.EnableKeyword("_EMISSION");
            orbMat.SetColor("_EmissionColor", new Color(1f, 0.78f, 0.15f, 1f) * 2.0f);
            orbMat.SetFloat("_Smoothness", 0.85f);
            EditorUtility.SetDirty(orbMat);

            GameObject temp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            temp.name = "AugmentOrb";
            temp.transform.localScale = Vector3.one * 0.65f;

            Renderer rend = temp.GetComponent<Renderer>();
            if (rend != null) rend.sharedMaterial = orbMat;

            SphereCollider col = temp.GetComponent<SphereCollider>();
            col.isTrigger = true;
            col.radius = 1.6f;

            GameObject lightObj = new GameObject("OrbGlowLight");
            lightObj.transform.SetParent(temp.transform, false);
            Light light = lightObj.AddComponent<Light>();
            light.type = LightType.Point;
            light.range = 6.5f;
            light.intensity = 2.6f;
            light.color = new Color(1f, 0.84f, 0.2f, 1f);

            AugmentOrb orbScript = temp.AddComponent<AugmentOrb>();
            SerializedObject so = new SerializedObject(orbScript);
            so.FindProperty("_orbRenderer").objectReferenceValue = rend;
            so.FindProperty("_orbPointLight").objectReferenceValue = light;
            so.ApplyModifiedPropertiesWithoutUndo();

            GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(temp, AugmentOrbPrefabPath);
            Object.DestroyImmediate(temp);
            return savedPrefab.GetComponent<AugmentOrb>();
        }

        private static void WireSceneManagers(NightRaidConfigSO raidConfig, List<AugmentDataSO> catalog, AugmentOrb orbPrefab)
        {
            // A. NightRaidDirector
            NightRaidDirector director = Object.FindAnyObjectByType<NightRaidDirector>();
            if (director == null)
            {
                GameObject dirObj = new GameObject("NightRaidDirector");
                director = dirObj.AddComponent<NightRaidDirector>();
            }
            SerializedObject dirSO = new SerializedObject(director);
            dirSO.FindProperty("_config").objectReferenceValue = raidConfig;
            dirSO.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(director);

            // B. AugmentManager & AugmentDropController
            AugmentManager augManager = Object.FindAnyObjectByType<AugmentManager>();
            if (augManager == null)
            {
                GameObject augObj = new GameObject("AugmentSystemManager");
                augManager = augObj.AddComponent<AugmentManager>();
            }
            SerializedObject augSO = new SerializedObject(augManager);
            SerializedProperty catalogProp = augSO.FindProperty("_augmentCatalog");
            catalogProp.ClearArray();
            for (int i = 0; i < catalog.Count; i++)
            {
                catalogProp.InsertArrayElementAtIndex(i);
                catalogProp.GetArrayElementAtIndex(i).objectReferenceValue = catalog[i];
            }
            augSO.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(augManager);

            AugmentDropController dropCtrl = augManager.GetComponent<AugmentDropController>();
            if (dropCtrl == null) dropCtrl = augManager.gameObject.AddComponent<AugmentDropController>();
            SerializedObject dropSO = new SerializedObject(dropCtrl);
            dropSO.FindProperty("_augmentOrbPrefab").objectReferenceValue = orbPrefab;
            dropSO.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(dropCtrl);
        }

        private static void AttachPlayerAugmentStats()
        {
            GameObject scenePlayer = GameObject.FindGameObjectWithTag("Player");
            if (scenePlayer != null && scenePlayer.GetComponent<PlayerAugmentStats>() == null)
            {
                scenePlayer.AddComponent<PlayerAugmentStats>();
                EditorUtility.SetDirty(scenePlayer);
            }

            string playerPrefabPath = "Assets/04_Prefabs/Characters/Player.prefab";
            GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(playerPrefabPath);
            if (prefabAsset != null && prefabAsset.GetComponent<PlayerAugmentStats>() == null)
            {
                using (var scope = new PrefabUtility.EditPrefabContentsScope(playerPrefabPath))
                {
                    if (scope.prefabContentsRoot.GetComponent<PlayerAugmentStats>() == null)
                    {
                        scope.prefabContentsRoot.AddComponent<PlayerAugmentStats>();
                    }
                }
            }
        }

        private static void BuildSurvivalAndAugmentUI()
        {
            Canvas hudCanvas = FindOrCreateScreenCanvas();

            // --- 1. SURVIVAL HUD (Top-Center Day Counter, Countdown & Pulsing Raid Warning) ---
            Transform existingHud = hudCanvas.transform.Find("SurvivalHUD_Panel");
            if (existingHud != null) Object.DestroyImmediate(existingHud.gameObject);

            GameObject hudRoot = CreateUIObject("SurvivalHUD_Panel", hudCanvas.transform);
            RectTransform hudRect = hudRoot.GetComponent<RectTransform>();
            hudRect.anchorMin = new Vector2(0.5f, 1f);
            hudRect.anchorMax = new Vector2(0.5f, 1f);
            hudRect.pivot = new Vector2(0.5f, 1f);
            hudRect.anchoredPosition = new Vector2(0f, -14f);
            hudRect.sizeDelta = new Vector2(360f, 88f);

            Image hudBg = hudRoot.AddComponent<Image>();
            hudBg.color = new Color(0.08f, 0.10f, 0.16f, 0.78f);

            TextMeshProUGUI dayText = CreateTMP("DayCounterText", hudRoot.transform, "DAY 1 / 20", 26, FontStyles.Bold, new Color(1f, 0.92f, 0.45f, 1f));
            RectTransform dayRect = dayText.GetComponent<RectTransform>();
            dayRect.anchorMin = new Vector2(0f, 0.52f);
            dayRect.anchorMax = new Vector2(0.58f, 1f);
            dayRect.offsetMin = new Vector2(14f, 0f);
            dayRect.offsetMax = new Vector2(0f, -4f);
            dayText.alignment = TextAlignmentOptions.MidlineLeft;

            TextMeshProUGUI timerText = CreateTMP("PhaseCountdownText", hudRoot.transform, "NIGHT IN 01:00", 16, FontStyles.Bold, Color.white);
            RectTransform timerRect = timerText.GetComponent<RectTransform>();
            timerRect.anchorMin = new Vector2(0.55f, 0.52f);
            timerRect.anchorMax = new Vector2(1f, 1f);
            timerRect.offsetMin = Vector2.zero;
            timerRect.offsetMax = new Vector2(-14f, -4f);
            timerText.alignment = TextAlignmentOptions.MidlineRight;

            GameObject barBgObj = CreateUIObject("PhaseBarBG", hudRoot.transform);
            RectTransform barBgRect = barBgObj.GetComponent<RectTransform>();
            barBgRect.anchorMin = new Vector2(0f, 0.34f);
            barBgRect.anchorMax = new Vector2(1f, 0.48f);
            barBgRect.offsetMin = new Vector2(14f, 0f);
            barBgRect.offsetMax = new Vector2(-14f, 0f);
            barBgObj.AddComponent<Image>().color = new Color(0.18f, 0.20f, 0.28f, 0.9f);

            GameObject barFillObj = CreateUIObject("PhaseBarFill", barBgObj.transform);
            RectTransform barFillRect = barFillObj.GetComponent<RectTransform>();
            barFillRect.anchorMin = Vector2.zero;
            barFillRect.anchorMax = Vector2.one;
            barFillRect.offsetMin = Vector2.zero;
            barFillRect.offsetMax = Vector2.zero;
            Image barFillImg = barFillObj.AddComponent<Image>();
            barFillImg.color = new Color(1f, 0.82f, 0.25f, 1f);
            barFillImg.type = Image.Type.Filled;
            barFillImg.fillMethod = Image.FillMethod.Horizontal;

            TextMeshProUGUI augCountText = CreateTMP("AugmentSummaryText", hudRoot.transform, "AUGMENTS: 0  |  PRESS [K] TEST", 13, FontStyles.Normal, new Color(0.75f, 0.92f, 1f, 1f));
            RectTransform augCountRect = augCountText.GetComponent<RectTransform>();
            augCountRect.anchorMin = new Vector2(0f, 0f);
            augCountRect.anchorMax = new Vector2(1f, 0.32f);
            augCountRect.offsetMin = new Vector2(14f, 2f);
            augCountRect.offsetMax = new Vector2(-14f, 0f);
            augCountText.alignment = TextAlignmentOptions.MidlineLeft;

            // Pulsing Red Night Raid Warning Banner below Day Counter
            GameObject bannerObj = CreateUIObject("NightRaidWarningBanner", hudRoot.transform);
            RectTransform bannerRect = bannerObj.GetComponent<RectTransform>();
            bannerRect.anchorMin = new Vector2(0.5f, 0f);
            bannerRect.anchorMax = new Vector2(0.5f, 0f);
            bannerRect.pivot = new Vector2(0.5f, 1f);
            bannerRect.anchoredPosition = new Vector2(0f, -8f);
            bannerRect.sizeDelta = new Vector2(420f, 40f);
            Image bannerImg = bannerObj.AddComponent<Image>();
            bannerImg.color = new Color(0.78f, 0.14f, 0.14f, 0.88f);

            TextMeshProUGUI bannerText = CreateTMP("RaidWarningText", bannerObj.transform, "NIGHT RAID! DEFEND THE CAMP!", 18, FontStyles.Bold, Color.white);
            StretchFull(bannerText.GetComponent<RectTransform>());
            bannerText.alignment = TextAlignmentOptions.Center;
            bannerObj.SetActive(false);

            SurvivalHUDView hudView = hudRoot.AddComponent<SurvivalHUDView>();
            SerializedObject hudSO = new SerializedObject(hudView);
            hudSO.FindProperty("_dayCounterText").objectReferenceValue = dayText;
            hudSO.FindProperty("_phaseCountdownText").objectReferenceValue = timerText;
            hudSO.FindProperty("_phaseProgressBar").objectReferenceValue = barFillImg;
            hudSO.FindProperty("_raidWarningBanner").objectReferenceValue = bannerObj;
            hudSO.FindProperty("_raidWarningText").objectReferenceValue = bannerText;
            hudSO.FindProperty("_raidBannerBackground").objectReferenceValue = bannerImg;
            hudSO.FindProperty("_augmentSummaryText").objectReferenceValue = augCountText;
            hudSO.ApplyModifiedPropertiesWithoutUndo();

            // --- 2. TFT 3-CARD AUGMENT SELECTION MODAL ---
            Transform existingModal = hudCanvas.transform.Find("AugmentSelectionModal_Root");
            if (existingModal != null) Object.DestroyImmediate(existingModal.gameObject);

            GameObject modalControllerObj = CreateUIObject("AugmentSelectionModal_Root", hudCanvas.transform);
            StretchFull(modalControllerObj.GetComponent<RectTransform>());

            GameObject modalBackdrop = CreateUIObject("ModalBackdrop", modalControllerObj.transform);
            StretchFull(modalBackdrop.GetComponent<RectTransform>());
            modalBackdrop.AddComponent<Image>().color = new Color(0.03f, 0.04f, 0.08f, 0.82f);

            TextMeshProUGUI headerText = CreateTMP("ModalHeader", modalBackdrop.transform, "CHOOSE AN AUGMENT", 34, FontStyles.Bold, new Color(1f, 0.88f, 0.35f, 1f));
            RectTransform headerRect = headerText.GetComponent<RectTransform>();
            headerRect.anchorMin = new Vector2(0.5f, 0.82f);
            headerRect.anchorMax = new Vector2(0.5f, 0.94f);
            headerRect.sizeDelta = new Vector2(640f, 60f);
            headerText.alignment = TextAlignmentOptions.Center;

            GameObject cardsRow = CreateUIObject("CardsContainer", modalBackdrop.transform);
            RectTransform rowRect = cardsRow.GetComponent<RectTransform>();
            rowRect.anchorMin = new Vector2(0.5f, 0.46f);
            rowRect.anchorMax = new Vector2(0.5f, 0.46f);
            rowRect.sizeDelta = new Vector2(900f, 390f);

            HorizontalLayoutGroup hlg = cardsRow.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 26f;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;

            AugmentCardUI[] cardUIs = new AugmentCardUI[3];
            for (int i = 0; i < 3; i++)
            {
                cardUIs[i] = CreateAugmentCard(cardsRow.transform, i);
            }

            modalBackdrop.SetActive(false);

            AugmentSelectionModalUI modalUI = modalControllerObj.AddComponent<AugmentSelectionModalUI>();
            SerializedObject modalSO = new SerializedObject(modalUI);
            modalSO.FindProperty("_modalRootPanel").objectReferenceValue = modalBackdrop;
            modalSO.FindProperty("_cardsContainer").objectReferenceValue = rowRect;
            SerializedProperty slotsProp = modalSO.FindProperty("_cardSlots");
            slotsProp.ClearArray();
            for (int i = 0; i < 3; i++)
            {
                slotsProp.InsertArrayElementAtIndex(i);
                slotsProp.GetArrayElementAtIndex(i).objectReferenceValue = cardUIs[i];
            }
            modalSO.ApplyModifiedPropertiesWithoutUndo();
        }

        private static AugmentCardUI CreateAugmentCard(Transform parent, int index)
        {
            // Outer glowing border
            GameObject cardRoot = CreateUIObject($"AugmentCard_{index + 1}", parent);
            Image borderImg = cardRoot.AddComponent<Image>();
            borderImg.color = new Color(1f, 0.84f, 0.18f, 1f);
            Button btn = cardRoot.AddComponent<Button>();

            // Inner dark slate card body (6px glowing border frame)
            GameObject innerBody = CreateUIObject("CardBody", cardRoot.transform);
            RectTransform innerRect = innerBody.GetComponent<RectTransform>();
            innerRect.anchorMin = Vector2.zero;
            innerRect.anchorMax = Vector2.one;
            innerRect.offsetMin = new Vector2(6f, 6f);
            innerRect.offsetMax = new Vector2(-6f, -6f);
            innerBody.AddComponent<Image>().color = new Color(0.09f, 0.11f, 0.18f, 0.96f);

            // Tier Ribbon
            GameObject ribbonObj = CreateUIObject("TierRibbon", innerBody.transform);
            RectTransform ribbonRect = ribbonObj.GetComponent<RectTransform>();
            ribbonRect.anchorMin = new Vector2(0f, 0.86f);
            ribbonRect.anchorMax = new Vector2(1f, 1f);
            ribbonRect.offsetMin = Vector2.zero;
            ribbonRect.offsetMax = Vector2.zero;
            Image ribbonImg = ribbonObj.AddComponent<Image>();
            ribbonImg.color = new Color(0.6f, 0.45f, 0.1f, 0.95f);

            TextMeshProUGUI tierText = CreateTMP("TierLabel", ribbonObj.transform, "GOLD TIER", 15, FontStyles.Bold, Color.white);
            StretchFull(tierText.GetComponent<RectTransform>());
            tierText.alignment = TextAlignmentOptions.Center;

            // Emblem Icon Box
            GameObject iconObj = CreateUIObject("IconBadge", innerBody.transform);
            RectTransform iconRect = iconObj.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.5f, 0.56f);
            iconRect.anchorMax = new Vector2(0.5f, 0.80f);
            iconRect.sizeDelta = new Vector2(82f, 82f);
            Image iconImg = iconObj.AddComponent<Image>();
            iconImg.color = Color.white;

            // Title
            TextMeshProUGUI titleText = CreateTMP("TitleText", innerBody.transform, "Augment Title", 22, FontStyles.Bold, Color.white);
            RectTransform titleRect = titleText.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.06f, 0.40f);
            titleRect.anchorMax = new Vector2(0.94f, 0.55f);
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;
            titleText.alignment = TextAlignmentOptions.Center;

            // Description
            TextMeshProUGUI descText = CreateTMP("DescriptionText", innerBody.transform, "Augment effect description.", 16, FontStyles.Normal, new Color(0.86f, 0.90f, 0.96f, 1f));
            RectTransform descRect = descText.GetComponent<RectTransform>();
            descRect.anchorMin = new Vector2(0.08f, 0.14f);
            descRect.anchorMax = new Vector2(0.92f, 0.40f);
            descRect.offsetMin = Vector2.zero;
            descRect.offsetMax = Vector2.zero;
            descText.alignment = TextAlignmentOptions.Top;
            descText.textWrappingMode = TextWrappingModes.Normal;

            // Hotkey Footer
            TextMeshProUGUI hotkeyText = CreateTMP("HotkeyHint", innerBody.transform, $"[{index + 1}] SELECT", 13, FontStyles.Bold, new Color(0.75f, 0.82f, 0.92f, 0.9f));
            RectTransform keyRect = hotkeyText.GetComponent<RectTransform>();
            keyRect.anchorMin = new Vector2(0f, 0.02f);
            keyRect.anchorMax = new Vector2(1f, 0.12f);
            keyRect.offsetMin = Vector2.zero;
            keyRect.offsetMax = Vector2.zero;
            hotkeyText.alignment = TextAlignmentOptions.Center;

            AugmentCardUI cardUI = cardRoot.AddComponent<AugmentCardUI>();
            SerializedObject cardSO = new SerializedObject(cardUI);
            cardSO.FindProperty("_borderGlowImage").objectReferenceValue = borderImg;
            cardSO.FindProperty("_tierRibbonImage").objectReferenceValue = ribbonImg;
            cardSO.FindProperty("_iconImage").objectReferenceValue = iconImg;
            cardSO.FindProperty("_tierLabelText").objectReferenceValue = tierText;
            cardSO.FindProperty("_titleText").objectReferenceValue = titleText;
            cardSO.FindProperty("_descriptionText").objectReferenceValue = descText;
            cardSO.FindProperty("_hotkeyHintText").objectReferenceValue = hotkeyText;
            cardSO.FindProperty("_selectButton").objectReferenceValue = btn;
            cardSO.ApplyModifiedPropertiesWithoutUndo();

            return cardUI;
        }

        private static Canvas FindOrCreateScreenCanvas()
        {
            Canvas[] canvases = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            foreach (Canvas c in canvases)
            {
                if (c.renderMode == RenderMode.ScreenSpaceOverlay)
                {
                    return c;
                }
            }

            GameObject obj = new GameObject("SurvivalHUD_Canvas");
            Canvas canvas = obj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 20;
            CanvasScaler scaler = obj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            obj.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        private static GameObject CreateUIObject(string name, Transform parent)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go;
        }

        private static TextMeshProUGUI CreateTMP(string name, Transform parent, string text, float fontSize, FontStyles style, Color color)
        {
            GameObject go = CreateUIObject(name, parent);
            TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.fontStyle = style;
            tmp.color = color;
            tmp.raycastTarget = false;
            return tmp;
        }

        private static void StretchFull(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
