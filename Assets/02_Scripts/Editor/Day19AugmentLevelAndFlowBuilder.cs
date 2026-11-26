using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using FindTheLover.Augments.Data;
using FindTheLover.UI;

namespace FindTheLover.EditorTools
{
    /// <summary>
    /// One-click Editor Setup Builder for Day 19:
    /// 1. Upgrades all 18 TFT AugmentDataSO blueprints to 3-level scaling (Lv.1 -> Lv.2 -> Lv.3).
    /// 2. Builds Singleplayer v1.0 Pause Menu [ESC] & Game Over / Campfire Respawn Modal UI inside HUD_Canvas.
    /// </summary>
    public static class Day19AugmentLevelAndFlowBuilder
    {
        private const string FlowPrefabPath = "Assets/04_Prefabs/UIs/SurvivalFlowUI_Root.prefab";

        [MenuItem("Tools/Find The Lover/Setup Day 19 (TFT Lv.1-3 Augments + Pause & Game Over UI)")]
        public static void SetupAugmentLevelsAndSurvivalFlow()
        {
            int upgradedAugments = UpgradeAll18AugmentAssetsTo3Levels();
            BuildSurvivalFlowModalUI();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorSceneManager.SaveOpenScenes();

            Debug.Log($"<color=#FFD700><b>[Day19Builder] Upgraded {upgradedAugments} TFT Augments to Lv.1-Lv.3 and built Pause [ESC] + Game Over Respawn UI!</b></color>");
        }

        private static int UpgradeAll18AugmentAssetsTo3Levels()
        {
            string[] guids = AssetDatabase.FindAssets("t:AugmentDataSO", new[] { "Assets/01_Data" });
            int count = 0;

            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                AugmentDataSO aug = AssetDatabase.LoadAssetAtPath<AugmentDataSO>(path);
                if (aug == null) continue;

                aug.allowStacking = true;
                aug.maxStacks = aug.effectType == AugmentEffectType.ImmortalLoverBond ? 2 : 3;
                aug.levelPrimaryMultipliers = new float[] { 1.0f, 1.75f, 2.75f };
                EditorUtility.SetDirty(aug);
                count++;
            }

            return count;
        }

        private static void BuildSurvivalFlowModalUI()
        {
            GameObject hudCanvas = GameObject.Find("HUD_Canvas");
            if (hudCanvas == null)
            {
                Debug.LogWarning("[Day19Builder] HUD_Canvas not found in open scene!");
                return;
            }

            Transform existing = hudCanvas.transform.Find("SurvivalFlowUI_Root");
            if (existing != null) Object.DestroyImmediate(existing.gameObject);

            GameObject root = CreateUI("SurvivalFlowUI_Root", hudCanvas.transform);
            Stretch(root.GetComponent<RectTransform>());

            // --- 1. PAUSE MENU [ESC] ---
            GameObject pauseBackdrop = CreateUI("PauseMenu_Backdrop", root.transform);
            Stretch(pauseBackdrop.GetComponent<RectTransform>());
            pauseBackdrop.AddComponent<Image>().color = new Color(0.03f, 0.04f, 0.08f, 0.80f);

            GameObject pauseWin = CreateUI("PauseWindow", pauseBackdrop.transform);
            RectTransform pwr = pauseWin.GetComponent<RectTransform>();
            pwr.anchorMin = new Vector2(0.5f, 0.5f); pwr.anchorMax = new Vector2(0.5f, 0.5f);
            pwr.sizeDelta = new Vector2(460f, 420f);
            pauseWin.AddComponent<Image>().color = new Color(0.08f, 0.10f, 0.16f, 0.97f);

            TextMeshProUGUI pTitle = CreateTMP("Title", pauseWin.transform, "PAUSED", 30f, FontStyles.Bold, new Color(1f, 0.86f, 0.30f, 1f));
            RectTransform ptr = pTitle.GetComponent<RectTransform>();
            ptr.anchorMin = new Vector2(0f, 0.82f); ptr.anchorMax = new Vector2(1f, 0.96f);
            ptr.offsetMin = Vector2.zero; ptr.offsetMax = Vector2.zero;
            pTitle.alignment = TextAlignmentOptions.Center;

            TextMeshProUGUI pStats = CreateTMP("StatsText", pauseWin.transform, "DAY 1 / 20  |  SEED: 20261119  |  AUGMENTS: 0", 13f, FontStyles.Bold, new Color(0.76f, 0.86f, 0.98f, 1f));
            RectTransform psr = pStats.GetComponent<RectTransform>();
            psr.anchorMin = new Vector2(0.05f, 0.72f); psr.anchorMax = new Vector2(0.95f, 0.82f);
            psr.offsetMin = Vector2.zero; psr.offsetMax = Vector2.zero;
            pStats.alignment = TextAlignmentOptions.Center;

            Button resumeBtn = CreateMenuButton("Btn_Resume", pauseWin.transform, "RESUME  [ESC]", new Vector2(0f, 55f), new Color(0.18f, 0.62f, 0.36f, 1f));
            Button saveBtn = CreateMenuButton("Btn_Save", pauseWin.transform, "SAVE GAME  (JSON)", new Vector2(0f, -5f), new Color(0.20f, 0.45f, 0.72f, 1f));
            Button loadBtn = CreateMenuButton("Btn_Load", pauseWin.transform, "LOAD LAST SAVE", new Vector2(0f, -65f), new Color(0.32f, 0.36f, 0.52f, 1f));
            Button quitBtn = CreateMenuButton("Btn_Quit", pauseWin.transform, "QUIT TO DESKTOP", new Vector2(0f, -125f), new Color(0.66f, 0.20f, 0.24f, 1f));

            TextMeshProUGUI pFeedback = CreateTMP("FeedbackText", pauseWin.transform, "", 14f, FontStyles.Bold, Color.white);
            RectTransform pfr = pFeedback.GetComponent<RectTransform>();
            pfr.anchorMin = new Vector2(0f, 0.02f); pfr.anchorMax = new Vector2(1f, 0.11f);
            pfr.offsetMin = Vector2.zero; pfr.offsetMax = Vector2.zero;
            pFeedback.alignment = TextAlignmentOptions.Center;

            pauseBackdrop.SetActive(false);

            // --- 2. GAME OVER / CAMPFIRE RESPAWN MODAL ---
            GameObject goBackdrop = CreateUI("GameOver_Backdrop", root.transform);
            Stretch(goBackdrop.GetComponent<RectTransform>());
            goBackdrop.AddComponent<Image>().color = new Color(0.12f, 0.02f, 0.04f, 0.86f);

            GameObject goWin = CreateUI("GameOverWindow", goBackdrop.transform);
            RectTransform gwr = goWin.GetComponent<RectTransform>();
            gwr.anchorMin = new Vector2(0.5f, 0.5f); gwr.anchorMax = new Vector2(0.5f, 0.5f);
            gwr.sizeDelta = new Vector2(520f, 380f);
            goWin.AddComponent<Image>().color = new Color(0.11f, 0.05f, 0.08f, 0.97f);

            TextMeshProUGUI gTitle = CreateTMP("Title", goWin.transform, "FALLEN IN THE WILDS", 30f, FontStyles.Bold, new Color(1f, 0.32f, 0.34f, 1f));
            RectTransform gtr = gTitle.GetComponent<RectTransform>();
            gtr.anchorMin = new Vector2(0f, 0.78f); gtr.anchorMax = new Vector2(1f, 0.94f);
            gtr.offsetMin = Vector2.zero; gtr.offsetMax = Vector2.zero;
            gTitle.alignment = TextAlignmentOptions.Center;

            TextMeshProUGUI gSub = CreateTMP("Subtitle", goWin.transform, "The campfire's warmth still waits for Quene...", 15f, FontStyles.Italic, new Color(0.95f, 0.82f, 0.75f, 0.92f));
            RectTransform gsubr = gSub.GetComponent<RectTransform>();
            gsubr.anchorMin = new Vector2(0.05f, 0.66f); gsubr.anchorMax = new Vector2(0.95f, 0.78f);
            gsubr.offsetMin = Vector2.zero; gsubr.offsetMax = Vector2.zero;
            gSub.alignment = TextAlignmentOptions.Center;

            TextMeshProUGUI gStats = CreateTMP("StatsText", goWin.transform, "DAY 1 / 20  |  SEED: 20261119  |  AUGMENTS: 0", 13f, FontStyles.Bold, new Color(1f, 0.85f, 0.45f, 1f));
            RectTransform gsr = gStats.GetComponent<RectTransform>();
            gsr.anchorMin = new Vector2(0.05f, 0.55f); gsr.anchorMax = new Vector2(0.95f, 0.65f);
            gsr.offsetMin = Vector2.zero; gsr.offsetMax = Vector2.zero;
            gStats.alignment = TextAlignmentOptions.Center;

            Button respawnBtn = CreateMenuButton("Btn_RespawnCampfire", goWin.transform, "RESPAWN AT CAMPFIRE  (100% HP)", new Vector2(0f, -10f), new Color(0.86f, 0.46f, 0.14f, 1f), 360f);
            Button goLoadBtn = CreateMenuButton("Btn_LoadSave", goWin.transform, "LOAD LAST SAVE", new Vector2(0f, -72f), new Color(0.24f, 0.42f, 0.68f, 1f), 360f);
            Button goQuitBtn = CreateMenuButton("Btn_QuitDesktop", goWin.transform, "QUIT TO DESKTOP", new Vector2(0f, -132f), new Color(0.55f, 0.18f, 0.22f, 1f), 360f);

            goBackdrop.SetActive(false);

            // Wire SurvivalFlowModalUI serialized references
            SurvivalFlowModalUI flowUI = root.AddComponent<SurvivalFlowModalUI>();
            SerializedObject fso = new SerializedObject(flowUI);
            fso.FindProperty("_pauseBackdrop").objectReferenceValue = pauseBackdrop;
            fso.FindProperty("_pauseStatsText").objectReferenceValue = pStats;
            fso.FindProperty("_pauseFeedbackText").objectReferenceValue = pFeedback;
            fso.FindProperty("_resumeButton").objectReferenceValue = resumeBtn;
            fso.FindProperty("_saveGameButton").objectReferenceValue = saveBtn;
            fso.FindProperty("_loadGameButton").objectReferenceValue = loadBtn;
            fso.FindProperty("_pauseQuitButton").objectReferenceValue = quitBtn;

            fso.FindProperty("_gameOverBackdrop").objectReferenceValue = goBackdrop;
            fso.FindProperty("_gameOverStatsText").objectReferenceValue = gStats;
            fso.FindProperty("_respawnCampfireButton").objectReferenceValue = respawnBtn;
            fso.FindProperty("_gameOverLoadButton").objectReferenceValue = goLoadBtn;
            fso.FindProperty("_gameOverQuitButton").objectReferenceValue = goQuitBtn;
            fso.ApplyModifiedPropertiesWithoutUndo();

            if (!PrefabUtility.IsPartOfPrefabInstance(root))
            {
                PrefabUtility.SaveAsPrefabAssetAndConnect(root, FlowPrefabPath, InteractionMode.AutomatedAction);
            }
        }

        private static Button CreateMenuButton(string name, Transform parent, string label, Vector2 anchoredPos, Color bgColor, float width = 320f)
        {
            GameObject btnObj = CreateUI(name, parent);
            RectTransform rt = btnObj.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f); rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = new Vector2(width, 46f);

            btnObj.AddComponent<Image>().color = bgColor;
            Button btn = btnObj.AddComponent<Button>();

            TextMeshProUGUI txt = CreateTMP("Label", btnObj.transform, label, 16f, FontStyles.Bold, Color.white);
            Stretch(txt.GetComponent<RectTransform>());
            txt.alignment = TextAlignmentOptions.Center;
            return btn;
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
