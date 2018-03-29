using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.Linq;
using UnityEngine;
using System.IO;

public class EditorOutlineGenerator : EditorWindow
{
    [MenuItem("Custom/Outliner")]
    public static void ItemsOpen()
    {
        GetWindow(typeof(EditorOutlineGenerator), false, "Outliner");
    }
    
    Texture2D textureAsset;
    Color outlineColor = Color.white;
    int outlineWidth = 1;

    void OnGUI()
    {
        textureAsset = (Texture2D)EditorGUILayout.ObjectField("Asset", textureAsset, typeof(Texture2D), false);
        outlineColor = EditorGUILayout.ColorField("Outline Color", outlineColor);
        outlineWidth = EditorGUILayout.IntSlider("Outline Width", outlineWidth, 1, 6);

        if (GUILayout.Button("Outline it!"))
        {
            CreateOutline();
        }
    }
    
    void CreateOutline()
    {
        string path = AssetDatabase.GetAssetPath(textureAsset);
        TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
        if (File.Exists(path.Replace(Path.GetExtension(path), "_Outline.png")))
        {
            File.Delete(path.Replace(Path.GetExtension(path), "_Outline.png"));
        }
        byte[] data = File.ReadAllBytes(path);
        Texture2D texture = new Texture2D(textureAsset.width, textureAsset.height, textureAsset.format, false);
        for (int x = 0; x < texture.width; x++)
        {
            for (int y = 0; y < texture.height; y++)
            {
                texture.SetPixel(x, y, Color.clear);
            }
        }

        for (int x = 0; x < textureAsset.width; x++)
        {
            for (int y = 0; y < textureAsset.height; y++)
            {
                if (textureAsset.GetPixel(x, y).a != 0)
                {
                    for (int x2 = -1; x2 <= 1; x2++)
                    {
                        for (int y2 = -1; y2 <= 1; y2++)
                        {
                            if (textureAsset.GetPixel(x + x2, y + y2).a == 0)
                            {
                                texture.SetPixel(x + x2, y + y2, Color.white);
                            }
                        }
                    }
                }
            }
        }

        texture.filterMode = FilterMode.Point;
        texture.Apply();
        path = path.Replace(Path.GetExtension(path), "_Outline.png");
        data = texture.EncodeToPNG();
        File.WriteAllBytes(path, data);

        AssetDatabase.Refresh();

        TextureImporter outlineImporter = AssetImporter.GetAtPath(path) as TextureImporter;

        outlineImporter.spriteBorder = textureImporter.spriteBorder;
        outlineImporter.spriteImportMode = textureImporter.spriteImportMode;
        outlineImporter.spritePackingTag = textureImporter.spritePackingTag;
        outlineImporter.spritePivot = textureImporter.spritePivot;
        outlineImporter.spritePixelsPerUnit = textureImporter.spritePixelsPerUnit;
        outlineImporter.textureCompression = textureImporter.textureCompression;
        outlineImporter.crunchedCompression = textureImporter.crunchedCompression;
        outlineImporter.compressionQuality = textureImporter.compressionQuality;
        outlineImporter.spritesheet = textureImporter.spritesheet;

        for (int i = 0; i < outlineImporter.spritesheet.Length; i++)
        {
            SpriteMetaData metaData = textureImporter.spritesheet[i];
            metaData.name += "_Outline";
            outlineImporter.spritesheet[i] = metaData;
        }

        AssetDatabase.Refresh();
    }
}
