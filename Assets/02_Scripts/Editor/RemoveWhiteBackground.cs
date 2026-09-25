using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class RemoveWhiteBackground : Editor
{
    [MenuItem("Assets/Tools/Xóa Nền Trắng Thông Minh (Flood Fill)", false, 1)]
    public static void CleanWhiteBackground()
    {
        Object[] selectedObjects = Selection.objects;
        if (selectedObjects.Length == 0)
        {
            EditorUtility.DisplayDialog("Thông báo", "Vui lòng chọn các file ảnh cần xóa nền trước!", "OK");
            return;
        }

        int count = 0;
        foreach (Object obj in selectedObjects)
        {
            string path = AssetDatabase.GetAssetPath(obj);
            if (string.IsNullOrEmpty(path)) continue;

            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) continue;

            // Bật quyền đọc ghi Texture
            bool wasReadable = importer.isReadable;
            importer.isReadable = true;
            importer.alphaSource = TextureImporterAlphaSource.FromInput;
            importer.alphaIsTransparency = true;
            importer.SaveAndReimport();

            Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (tex == null) continue;

            int width = tex.width;
            int height = tex.height;
            Color32[] pixels = tex.GetPixels32();
            bool[,] visited = new bool[width, height];
            Queue<Vector2Int> queue = new Queue<Vector2Int>();

            // Hàm kiểm tra xem pixel có phải màu trắng phông nền không
            bool IsWhite(Color32 c) => c.r >= 245 && c.g >= 245 && c.b >= 245;

            // Thêm tất cả các pixel ở 4 mép viền ngoài vào hàng đợi
            for (int x = 0; x < width; x++)
            {
                if (IsWhite(pixels[0 * width + x])) { queue.Enqueue(new Vector2Int(x, 0)); visited[x, 0] = true; }
                if (IsWhite(pixels[(height - 1) * width + x])) { queue.Enqueue(new Vector2Int(x, height - 1)); visited[x, height - 1] = true; }
            }
            for (int y = 0; y < height; y++)
            {
                if (IsWhite(pixels[y * width + 0])) { queue.Enqueue(new Vector2Int(0, y)); visited[0, y] = true; }
                if (IsWhite(pixels[y * width + (width - 1)])) { queue.Enqueue(new Vector2Int(width - 1, y)); visited[width - 1, y] = true; }
            }

            // Thuật toán Flood Fill loang từ ngoài vào trong, gặp nét đen thì dừng lại
            Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
            while (queue.Count > 0)
            {
                Vector2Int curr = queue.Dequeue();
                pixels[curr.y * width + curr.x].a = 0; // Biến thành trong suốt

                foreach (var dir in dirs)
                {
                    int nx = curr.x + dir.x;
                    int ny = curr.y + dir.y;

                    if (nx >= 0 && nx < width && ny >= 0 && ny < height && !visited[nx, ny])
                    {
                        visited[nx, ny] = true;
                        if (IsWhite(pixels[ny * width + nx]))
                        {
                            queue.Enqueue(new Vector2Int(nx, ny));
                        }
                    }
                }
            }

            // Ghi đè file PNG mới với nền trong suốt
            Texture2D newTex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            newTex.SetPixels32(pixels);
            newTex.Apply();

            byte[] bytes = newTex.EncodeToPNG();
            File.WriteAllBytes(path, bytes);

            importer.isReadable = wasReadable;
            importer.SaveAndReimport();
            count++;
        }

        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Thành công", $"Đã xóa nền trong suốt an toàn cho {count} ảnh!", "Tuyệt vời");
    }
}