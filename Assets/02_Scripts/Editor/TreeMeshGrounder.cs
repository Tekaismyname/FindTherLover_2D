using UnityEditor;
using UnityEngine;

namespace FindTheLover.Environment.Editor
{
    /// <summary>
    /// Utility to adjust Quad_BottomPivot mesh vertices so tree roots
    /// pierce deep into terrain ground and eliminate floating air gaps.
    /// </summary>
    public static class TreeMeshGrounder
    {
        [MenuItem("Tools/Environment/Fix Tree Floating (Cắm Rễ Xuống Đất)")]
        public static void FixTreeFloating()
        {
            string path = "Assets/03_Art/Meshes/Quad_BottomPivot.asset";
            var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);

            if (mesh == null)
            {
                mesh = new Mesh();
                mesh.name = "Quad_BottomPivot";
                AssetDatabase.CreateAsset(mesh, path);
            }

            // Lower bottom vertices by -0.15 (at Scale Y=12, this sinks roots by 1.8 meters into the dirt)
            float sinkOffset = -0.15f;

            mesh.vertices = new Vector3[]
            {
                new Vector3(-0.5f, sinkOffset, 0f),        // Bottom-Left
                new Vector3( 0.5f, sinkOffset, 0f),        // Bottom-Right
                new Vector3(-0.5f, 1.0f + sinkOffset, 0f), // Top-Left
                new Vector3( 0.5f, 1.0f + sinkOffset, 0f)  // Top-Right
            };

            mesh.uv = new Vector2[]
            {
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(1, 1)
            };

            mesh.triangles = new int[] { 0, 2, 1, 2, 3, 1 };
            mesh.normals = new Vector3[]
            {
                -Vector3.forward,
                -Vector3.forward,
                -Vector3.forward,
                -Vector3.forward
            };

            mesh.RecalculateBounds();
            EditorUtility.SetDirty(mesh);
            AssetDatabase.SaveAssets();

            // Refresh Terrain to re-render tree instances with new mesh vertices
            var terrain = Terrain.activeTerrain;
            if (terrain != null)
            {
                var td = terrain.terrainData;
                var trees = td.treeInstances;
                td.treeInstances = trees; // Force terrain tree batcher rebuild
                EditorUtility.SetDirty(td);
                EditorUtility.SetDirty(terrain);
            }

            EditorUtility.DisplayDialog(
                "Đã Cố Định Rễ Cây!",
                "Đã hạ các đỉnh của lưới Quad_BottomPivot cắm sâu vào đất -1.8m!\n\nToàn bộ cây trên Terrain đã được cắm ngập rễ xuống mặt đất và dính chặt vào bóng râm.",
                "Tuyệt vời"
            );
        }
    }
}
