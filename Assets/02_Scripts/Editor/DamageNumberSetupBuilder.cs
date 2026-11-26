using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using FindTheLover.Combat;
using FindTheLover.UI;
using FindTheLover.UI.Data;

namespace FindTheLover.EditorTools
{
    /// <summary>
    /// One-click Editor Setup Builder creating the Data-Driven DamageNumber_Style asset,
    /// World-Space DamageNumber_Popup.prefab, and wiring DamageNumber_Spawner.prefab in the open scene.
    /// </summary>
    public static class DamageNumberSetupBuilder
    {
        private const string StyleDir = "Assets/01_Data/UI";
        private const string StyleAssetPath = "Assets/01_Data/UI/DamageNumber_Style.asset";
        private const string PopupPrefabPath = "Assets/04_Prefabs/UIs/DamageNumber_Popup.prefab";
        private const string SpawnerPrefabPath = "Assets/04_Prefabs/Systems/DamageNumber_Spawner.prefab";

        [MenuItem("Tools/Find The Lover/Setup Floating Combat Damage Numbers")]
        public static void SetupFloatingDamageNumbers()
        {
            Directory.CreateDirectory(Path.GetFullPath(StyleDir));
            Directory.CreateDirectory(Path.GetFullPath("Assets/04_Prefabs/UIs"));
            Directory.CreateDirectory(Path.GetFullPath("Assets/04_Prefabs/Systems"));

            // 1. Create or load DamageNumber_Style.asset
            DamageNumberStyleSO style = AssetDatabase.LoadAssetAtPath<DamageNumberStyleSO>(StyleAssetPath);
            if (style == null)
            {
                style = ScriptableObject.CreateInstance<DamageNumberStyleSO>();
                AssetDatabase.CreateAsset(style, StyleAssetPath);
            }
            style.lifetime = 0.85f;
            style.riseHeight = 1.85f;
            style.verticalSpawnOffset = 1.0f;
            style.horizontalScatter = 0.65f;
            style.fadeStartNormalized = 0.55f;
            style.baseFontSize = 7.0f;
            style.normalPeakScale = 1.25f;
            style.criticalPeakScale = 1.65f;
            style.prewarmPoolSize = 24;
            EditorUtility.SetDirty(style);

            // 2. Build clean World-Space TMP DamageNumber_Popup.prefab
            GameObject tempPopup = new GameObject("DamageNumber_Popup");
            TextMeshPro tmp = tempPopup.AddComponent<TextMeshPro>();
            tmp.text = "25";
            tmp.fontSize = 7.0f;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.textWrappingMode = TextWrappingModes.NoWrap;
            tmp.color = Color.white;
            tmp.outlineWidth = 0.24f;
            tmp.outlineColor = new Color32(14, 10, 20, 255);
            RectTransform rt = tempPopup.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(6f, 2f);
            if (tmp.renderer != null) tmp.renderer.sortingOrder = 200;

            tempPopup.AddComponent<DamageNumberPopup>();
            GameObject savedPopupPrefab = PrefabUtility.SaveAsPrefabAsset(tempPopup, PopupPrefabPath);
            Object.DestroyImmediate(tempPopup);

            // 3. Create or update DamageNumber_Spawner in Scene & save as linked Prefab
            GameObject spawnerObj = GameObject.Find("DamageNumber_Spawner");
            if (spawnerObj == null)
            {
                spawnerObj = new GameObject("DamageNumber_Spawner");
            }

            DamageNumberSpawner spawner = spawnerObj.GetComponent<DamageNumberSpawner>();
            if (spawner == null) spawner = spawnerObj.AddComponent<DamageNumberSpawner>();

            PlayerCombat pc = Object.FindAnyObjectByType<PlayerCombat>();
            SerializedObject so = new SerializedObject(spawner);
            so.FindProperty("styleConfig").objectReferenceValue = style;
            so.FindProperty("popupPrefab").objectReferenceValue = savedPopupPrefab != null ? savedPopupPrefab.GetComponent<DamageNumberPopup>() : null;
            so.FindProperty("playerCombat").objectReferenceValue = pc;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(spawnerObj);

            if (!PrefabUtility.IsPartOfPrefabInstance(spawnerObj))
            {
                PrefabUtility.SaveAsPrefabAssetAndConnect(spawnerObj, SpawnerPrefabPath, InteractionMode.AutomatedAction);
            }
            else
            {
                PrefabUtility.ApplyPrefabInstance(spawnerObj, InteractionMode.AutomatedAction);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorSceneManager.SaveOpenScenes();

            Debug.Log("<color=#FFCC33><b>[DamageNumberSetupBuilder] Floating Combat Damage Numbers (Pool=24, Crit/Normal/Harvest/Heal) installed & wired!</b></color>");
        }
    }
}
