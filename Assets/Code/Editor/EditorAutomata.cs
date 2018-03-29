using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class EditorAutomata
{
    [MenuItem("Custom/Automate/Set all materials to default")]
    public static void SetToDefaultMaterial()
    {
        var mat = AssetDatabase.GetBuiltinExtraResource<Material>("Sprites-Default.mat");

        GameObject[] gos = Selection.gameObjects;
        for (int i = 0; i < gos.Length; i++)
        {
            SpriteRenderer[] spriteRenderers = gos[i].GetComponentsInChildren<SpriteRenderer>();
            for (int s = 0; s < spriteRenderers.Length; s++)
            {
                spriteRenderers[s].material = mat;
            }
        }
    }

    [MenuItem("Custom/Automate/Make renderers concise")]
    public static void ConciseRenderers()
    {
        string[] paths = AssetDatabase.FindAssets("t:GameObject");
        for (int i = 0; i < paths.Length; i++)
        {
            GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(paths[i]));

            SpriteRenderer[] spriteRenderers = go.GetComponentsInChildren<SpriteRenderer>();
            HelperExt.FixSpriteRenderers(spriteRenderers);
        }
    }

    [MenuItem("Custom/Automate/Make shadows concise")]
    public static void ConciseShadows()
    {
        var mat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Materials/Shadow.mat");
        string[] paths = AssetDatabase.FindAssets("t:GameObject");
        for (int i = 0; i < paths.Length;i++)
        {
            GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(paths[i]));
            
            SpriteRenderer[] spriteRenderers = go.GetComponentsInChildren<SpriteRenderer>();
            for (int s = 0; s < spriteRenderers.Length; s++)
            {
                if(spriteRenderers[s].color.r == 0f && spriteRenderers[s].color.g == 0f && spriteRenderers[s].color.b == 0f || spriteRenderers[s].name == "Shadow")
                {
                    spriteRenderers[s].color = new Color(0f, 0f, 0f, 0.65f);
                    spriteRenderers[s].name = "Shadow";
                    spriteRenderers[s].material = mat;
                    spriteRenderers[s].sortingOrder = -1;
                    spriteRenderers[s].gameObject.SetLayer("Shadow");
                }
            }
        }
    }

    [MenuItem("Custom/Automate/Assign default layer")]
    public static void DefaultLayer()
    {
        string[] paths = AssetDatabase.FindAssets("t:GameObject");
        for (int i = 0; i < paths.Length; i++)
        {
            GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(paths[i]));

            SpriteRenderer[] spriteRenderers = go.GetComponentsInChildren<SpriteRenderer>();
            for (int s = 0; s < spriteRenderers.Length; s++)
            {
                spriteRenderers[s].sortingLayerName = "Default";
            }
        }
    }

    [MenuItem("Custom/Automate/Set projectiles sorting order")]
    public static void ProjectileLayers()
    {
        string[] paths = AssetDatabase.FindAssets("t:GameObject");
        for (int i = 0; i < paths.Length; i++)
        {
            GameObject proj = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(paths[i]));
            if(proj.GetComponent<GunProjectile>())
            {
                SpriteRenderer[] spriteRenderers = proj.GetComponentsInChildren<SpriteRenderer>();
                for (int s = 0; s < spriteRenderers.Length; s++)
                {
                    if (spriteRenderers[s].color.r != 0f && spriteRenderers[s].color.g != 0f && spriteRenderers[s].color.b != 0f)
                    {
                        spriteRenderers[s].name = "Projectile";
                        spriteRenderers[s].sortingOrder = 4;
                    }
                }
            }
        }
    }
}
