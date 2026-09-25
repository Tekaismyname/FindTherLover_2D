using UnityEditor;
using UnityEngine;

namespace FindTheLover.Environment.Editor
{
    /// <summary>
    /// Production-grade Editor Tool to convert Terrain Trees into interactive GameObjects,
    /// correctly preserving the Prefab's base scale multiplied by random brush variance.
    /// Also includes a utility to fix scale on already-converted trees.
    /// </summary>
    public static class TerrainTreesToGameObjects
    {
        private const string ParentContainerName = "_Environment_Trees";

        [MenuItem("Tools/Environment/Convert Terrain Trees to GameObjects (Chuyển Cây Giữ Nguyên Scale)")]
        public static void ConvertTrees()
        {
            var terrain = Terrain.activeTerrain;
            if (terrain == null || terrain.terrainData == null)
            {
                EditorUtility.DisplayDialog("Lỗi", "Không tìm thấy Terrain nào trong Scene!", "OK");
                return;
            }

            var tData = terrain.terrainData;
            var treeInstances = tData.treeInstances;

            if (treeInstances == null || treeInstances.Length == 0)
            {
                EditorUtility.DisplayDialog(
                    "Thông báo", 
                    "Terrain không có cây nào trong bộ nhớ!\nNếu cây đã chuyển sang GameObject rồi, hãy dùng tính năng 'Fix Scale' bên dưới.", 
                    "OK"
                );
                return;
            }

            // Find or create parent container in the Scene
            var parentObj = GameObject.Find(ParentContainerName);
            if (parentObj == null)
            {
                parentObj = new GameObject(ParentContainerName);
                Undo.RegisterCreatedObjectUndo(parentObj, "Create Tree Container");
            }

            int convertedCount = 0;
            Vector3 terrainPos = terrain.transform.position;
            Vector3 terrainSize = tData.size;

            for (int i = 0; i < treeInstances.Length; i++)
            {
                var tree = treeInstances[i];
                if (tree.prototypeIndex < 0 || tree.prototypeIndex >= tData.treePrototypes.Length) continue;

                var prototype = tData.treePrototypes[tree.prototypeIndex];
                if (prototype.prefab == null) continue;

                // Calculate exact world position from normalized terrain coordinates (0..1)
                Vector3 worldPos = new Vector3(
                    tree.position.x * terrainSize.x + terrainPos.x,
                    tree.position.y * terrainSize.y + terrainPos.y,
                    tree.position.z * terrainSize.z + terrainPos.z
                );

                // Instantiate as linked Prefab
                var instance = (GameObject)PrefabUtility.InstantiatePrefab(prototype.prefab, parentObj.transform);
                if (instance != null)
                {
                    instance.transform.position = worldPos;
                    instance.transform.rotation = Quaternion.Euler(0f, tree.rotation * Mathf.Rad2Deg, 0f);

                    // PRESERVE PREFAB BASE SCALE!
                    Vector3 baseScale = prototype.prefab.transform.localScale;
                    instance.transform.localScale = new Vector3(
                        baseScale.x * tree.widthScale,
                        baseScale.y * tree.heightScale,
                        baseScale.z * tree.widthScale
                    );

                    Undo.RegisterCreatedObjectUndo(instance, "Convert Tree Instance");
                    convertedCount++;
                }
            }

            // Clear trees from the Terrain engine to prevent double rendering
            tData.SetTreeInstances(new TreeInstance[0], false);
            terrain.Flush();

            EditorUtility.SetDirty(tData);
            EditorUtility.SetDirty(terrain);

            EditorUtility.DisplayDialog(
                "Thành công!", 
                $"Đã chuyển đổi thành công {convertedCount} cây sang GameObject với SCALE GỐC CHUẨN XÁC 100%!", 
                "Tuyệt vời"
            );
        }

        [MenuItem("Tools/Environment/Fix Scale of Converted Trees (Sửa Scale Cây Đã Chuyển Về Chuẩn Prefab)")]
        public static void FixExistingTreeScales()
        {
            var container = GameObject.Find(ParentContainerName);
            if (container == null || container.transform.childCount == 0)
            {
                EditorUtility.DisplayDialog("Thông báo", $"Không tìm thấy thư mục '{ParentContainerName}' có chứa cây trong Scene!", "OK");
                return;
            }

            int fixedCount = 0;
            for (int i = 0; i < container.transform.childCount; i++)
            {
                var child = container.transform.GetChild(i);
                var prefabSource = PrefabUtility.GetCorrespondingObjectFromSource(child.gameObject);
                if (prefabSource != null)
                {
                    Vector3 baseScale = prefabSource.transform.localScale;
                    // If current scale is miniature (< 3), restore to prefab base scale
                    if (child.localScale.x < 3.0f || child.localScale.y < 3.0f)
                    {
                        Undo.RecordObject(child, "Fix Tree Scale");
                        float currentVariance = Mathf.Max(child.localScale.x, 0.8f);
                        child.localScale = new Vector3(
                            baseScale.x * currentVariance,
                            baseScale.y * currentVariance,
                            baseScale.z * currentVariance
                        );
                        fixedCount++;
                    }
                }
            }

            EditorUtility.DisplayDialog("Hoàn tất", $"Đã phóng to và sửa Scale thành công cho {fixedCount} cây về chuẩn Prefab gốc!", "OK");
        }
    }
}
