using UnityEngine;
using UnityEditor;

public class SpritePivotEditor : EditorWindow
{
    private Texture2D spriteSheet;
    private SpriteAlignment pivotAlignment = SpriteAlignment.Center;
    private Vector2 customPivot = new Vector2(0.5f, 0.5f);

    // Tạo menu trên thanh công cụ của Unity
    [MenuItem("Tools/Sprite Pivot Editor")]
    public static void ShowWindow()
    {
        GetWindow<SpritePivotEditor>("Sprite Pivot Editor");
    }

    private void OnGUI()
    {
        GUILayout.Label("Cập nhật Pivot cho Multiple Sprite", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // Ô kéo thả file Texture (Sprite Sheet)
        spriteSheet = (Texture2D)EditorGUILayout.ObjectField("Sprite Sheet", spriteSheet, typeof(Texture2D), false);

        EditorGUILayout.Space();

        // Lựa chọn vị trí Pivot
        pivotAlignment = (SpriteAlignment)EditorGUILayout.EnumPopup("Vị trí Pivot", pivotAlignment);

        // Nếu chọn Custom, hiển thị thêm ô nhập tọa độ X, Y (từ 0 đến 1)
        if (pivotAlignment == SpriteAlignment.Custom)
        {
            customPivot = EditorGUILayout.Vector2Field("Tọa độ Custom (0-1)", customPivot);
        }

        EditorGUILayout.Space();

        // Nút thực thi
        if (GUILayout.Button("Apply Pivot cho tất cả Sprites", GUILayout.Height(30)))
        {
            ApplyPivot();
        }
    }

    private void ApplyPivot()
    {
        if (spriteSheet == null)
        {
            EditorUtility.DisplayDialog("Lỗi", "Vui lòng chọn một file Sprite Sheet (Texture2D) vào ô trống.", "OK");
            return;
        }

        string path = AssetDatabase.GetAssetPath(spriteSheet);
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;

        if (importer == null)
        {
            EditorUtility.DisplayDialog("Lỗi", "Không thể đọc dữ liệu Texture.", "OK");
            return;
        }

        // Kiểm tra xem Texture đã được chỉnh sang chế độ Multiple chưa
        if (importer.spriteImportMode != SpriteImportMode.Multiple)
        {
            EditorUtility.DisplayDialog("Lỗi", "Ảnh được chọn chưa được set Sprite Mode thành 'Multiple'.", "OK");
            return;
        }

        // Lấy danh sách toàn bộ các sprite đã được slice (cắt)
        SpriteMetaData[] spritesheetMeta = importer.spritesheet;

        if (spritesheetMeta.Length == 0)
        {
            EditorUtility.DisplayDialog("Lỗi", "Chưa có sprite nào được cắt trong file này. Hãy vào Sprite Editor để Slice trước.", "OK");
            return;
        }

        // Lặp qua từng sprite và cập nhật Pivot
        for (int i = 0; i < spritesheetMeta.Length; i++)
        {
            spritesheetMeta[i].alignment = (int)pivotAlignment;
            
            // Nếu là custom thì gán tọa độ Vector2
            if (pivotAlignment == SpriteAlignment.Custom)
            {
                spritesheetMeta[i].pivot = customPivot;
            }
        }

        // Gán ngược dữ liệu lại cho Importer
        importer.spritesheet = spritesheetMeta;
        
        // Lưu lại thay đổi và Reimport để Unity cập nhật asset
        EditorUtility.SetDirty(importer);
        importer.SaveAndReimport();

        EditorUtility.DisplayDialog("Thành công", $"Đã cập nhật Pivot thành công cho {spritesheetMeta.Length} sprites!", "OK");
    }
}