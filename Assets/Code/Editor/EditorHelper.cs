using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System;
using System.Linq;
using System.IO;
using System.Diagnostics;
using UnityEditor.Callbacks;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using Data;

using Object = UnityEngine.Object;
using Debug = UnityEngine.Debug;

public class EditorAssetHelper : AssetPostprocessor
{
    static string[] importedAssets;
    static string[] deletedAssets;
    static string[] movedAssets;
    static string[] movedFromAssetPaths;

    static bool Contains(string word)
    {
        return string.Join("\n", importedAssets).Contains(word) || string.Join("\n", deletedAssets).Contains(word) || string.Join("\n", movedAssets).Contains(word) || string.Join("\n", movedFromAssetPaths).Contains(word);
    }

    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        EditorAssetHelper.importedAssets = importedAssets;
        EditorAssetHelper.deletedAssets = deletedAssets;
        EditorAssetHelper.movedAssets = movedAssets;
        EditorAssetHelper.movedFromAssetPaths = movedFromAssetPaths;
        
        if(Contains("Assets/Art/Audio"))
        {
            EditorHelper.LoadAllAudio();
        }
        if (Contains("Assets/Art/Sprites"))
        {
            EditorHelper.LoadAllSprites();
        }
        if (Contains("Assets/Items") && !Contains("Assets/Items/Generator Presets") && !Contains("Assets/Items/Achievements"))
        {
            EditorHelper.LoadItems();
            EditorHelper.LoadTips();
        }
        if(Contains("Assets/Prefabs/Projectiles"))
        {
            EditorHelper.LoadProjectiles();
        }
        if(Contains(".mat"))
        {
            EditorHelper.LoadAllMaterials();
        }
        if(Contains("Assets/Prefabs/Generator/Tiles") || Contains("Assets/Prefabs/Generator/TilesAlt"))
        {
            EditorHelper.LoadTiles();
        }
        if(Contains("Assets/Art/Sprites/Tiles"))
        {
            EditorHelper.LoadTileSets();
        }
        if(Contains("Assets/Items/Generator Presets"))
        {
            EditorHelper.LoadGeneratorPresets();
        }
    }
}

[InitializeOnLoad]
public static class EditorSceneSaver
{
    static EditorSceneSaver()
    {
        #if UNITY_2017_2_OR_NEWER
        EditorApplication.pauseStateChanged += StateChange;
        #else
        EditorApplication.playmodeStateChanged += StateChange;
        #endif
    }
    #if UNITY_2017_2_OR_NEWER
    private static void StateChange(PauseState obj)
    {
        GenericStateChange();
    }
    #else
    private static void StateChange()
    {
        GenericStateChange();
    }
    #endif
    static void GenericStateChange()
    {
        if (!EditorApplication.isPlaying && EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorSceneManager.SaveOpenScenes();
            AssetDatabase.SaveAssets();
        }
    }
}

public class EditorHelper
{
    public static MonoScript[] RoomTypes
    {
        get
        {
            var allTypes = Types;
            List<MonoScript> roomTypes = new List<MonoScript>();
            for (int i = 0; i < allTypes.Length; i++)
            {
                if (allTypes[i].name.StartsWith("ObjectRoom") && allTypes[i].name != "ObjectRoomChecker")
                {
                    roomTypes.Add(allTypes[i]);
                }
            }

            return roomTypes.ToArray();
        }
    }

    public static MonoScript[] Types
    {
        get
        {
            MonoScript[] scripts = (MonoScript[])Resources.FindObjectsOfTypeAll(typeof(MonoScript));

            List<MonoScript> result = new List<MonoScript>();

            foreach (MonoScript m in scripts)
            {
                if (m.GetClass() != null)
                {
                    result.Add(m);
                }
            }
            return result.ToArray();
        }
    }

    [DidReloadScripts]
    private static void OnScriptsReloaded()
    {
        if (EditorSceneManager.GetActiveScene().name != "Game")
        {
            EditorSceneManager.OpenScene("Assets/Scenes/Game.unity");
            return;
        }

        MonoBehaviour[] objects = GameObject.FindObjectsOfType<MonoBehaviour>();
        for(int i = 0; i < objects.Length;i++)
        {
            if(objects[i] is IScriptReloadable)
            {
                (objects[i] as IScriptReloadable).ScriptReload();
            }
        }

        Quality.possibleQualities.Clear();

        string[] files = Directory.GetFiles(Application.dataPath + "/Code", "*.cs", SearchOption.AllDirectories);
        for(int i = 0; i < files.Length;i++)
        {
            string content = File.ReadAllText(files[i]);
            if ((content.Contains(".GetMultiplier(") || content.Contains(".HasQuality(") || content.Contains(".GetState(")) && !files[i].EndsWith("EditorHelper.cs"))
            {
                string[] lines = content.Split('\n');
                for (int l = 0; l < lines.Length; l++)
                {
                    string line = lines[l];
                    if (line.Contains(".GetMultiplier(") || line.Contains(".HasQuality(") || line.Contains(".GetState("))
                    {
                        string[] data = line.Split('"');
                        string option = data[data.Length - 2];

                        if(!Quality.possibleQualities.Contains(option))
                        {
                            Quality.possibleQualities.Add(option);
                        }
                    }
                }
            }
        }

        Quality.possibleQualities.Sort();
        
        EditorUI window = EditorWindow.GetWindow<EditorUI>("Helper");
        window.Show();
        window.Focus();
    }

    public static void RefreshGameWindow()
    {
        RenderTexture rt = (RenderTexture)AssetDatabase.LoadAssetAtPath("Assets\\Art\\GameView.renderTexture", typeof(RenderTexture));
        GameManager gm = GameObject.FindObjectOfType<GameManager>();

        Camera[] allCameras = GameObject.FindObjectsOfType<Camera>();
        for (int i = 0; i < allCameras.Length; i++)
        {
            if (allCameras[i].name == "GameCamera")
            {
                allCameras[i].targetTexture = null;
            }
            allCameras[i].orthographicSize = gm.gameHeight / 2;
        }

        RectTransform[] allRects = GameObject.FindObjectsOfType<RectTransform>();
        for (int i = 0; i < allRects.Length; i++)
        {
            if (allRects[i].sizeDelta == new Vector2(rt.width, rt.height))
            {
                allRects[i].sizeDelta = new Vector2(gm.gameWidth, gm.gameHeight);
            }
        }

        FilterMode fm = rt.filterMode;
        int aa = rt.antiAliasing;
        RenderTextureFormat rtf = rt.format;

        rt = new RenderTexture(gm.gameWidth, gm.gameHeight, 0, rtf);
        rt.Create();
        rt.wrapMode = TextureWrapMode.Clamp;
        rt.filterMode = fm;
        rt.antiAliasing = aa;

        AssetDatabase.CreateAsset(rt, "Assets\\Art\\GameView.renderTexture");

        AssetDatabase.Refresh();

        rt = (RenderTexture)AssetDatabase.LoadAssetAtPath("Assets\\Art\\GameView.renderTexture", typeof(RenderTexture));
        RenderTexture uiRt = (RenderTexture)AssetDatabase.LoadAssetAtPath("Assets\\Art\\UIView.renderTexture", typeof(RenderTexture));

        GameObject.Find("GameCamera").GetComponent<Camera>().targetTexture = rt;
        GameObject.Find("UICamera").GetComponent<Camera>().targetTexture = uiRt;

        PlayerSettings.defaultScreenWidth = gm.gameWidth;
        PlayerSettings.defaultScreenHeight = gm.gameHeight;
    }

    public static void LoadProjectiles()
    {
        PoolManager pm = GameObject.FindObjectOfType<PoolManager>();
        if (pm)
        {
            for(int i = 0; i < pm.poolItems.Count;i++)
            {
                if(!pm.poolItems[i].gameObject || pm.poolItems[i].gameObject.name.StartsWith("Projectile_"))
                {
                    pm.poolItems.RemoveAt(i);
                }
            }

            string[] projectileGuids = AssetDatabase.FindAssets("t:GameObject", new string[] { "Assets/Prefabs/Projectiles", "Assets/Prefabs/Projectile Shells" });
            for (int i = 0; i < projectileGuids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(projectileGuids[i]);
                GameObject poolObject = (GameObject)AssetDatabase.LoadAssetAtPath(path, typeof(GameObject));

                PoolManager.PoolItem newItem = new PoolManager.PoolItem(poolObject, pm.defaultMax);
                if (!pm.poolItems.Contains(newItem))
                {
                    pm.poolItems.Add(newItem);
                }
            }

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }
    }

    public static void LoadAllMaterials()
    {
        ObjectManager sm = GameObject.FindObjectOfType<ObjectManager>();
        if (sm)
        {
            string[] guids = AssetDatabase.FindAssets("t:Material");
            List<Material> newMaterials = new List<Material>();

            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                Material[] asset = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Material>().ToArray();

                newMaterials.AddRange(asset);
            }

            if (sm.materials.Count != guids.Length)
            {
                sm.materials.Clear();
                sm.materials = newMaterials;
                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            }
        }
    }

    public static void LoadAllSprites()
    {
        ObjectManager sm = GameObject.FindObjectOfType<ObjectManager>();
        if (sm)
        {
            string[] guids = AssetDatabase.FindAssets("t:Sprite");
            List<Sprite> newSprites = new List<Sprite>();
            List<Sprite> newSpriteOutlines = new List<Sprite>();

            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                Sprite[] asset = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToArray();

                for (int s = 0; s < asset.Length; s++)
                {
                    if (asset[s].texture.name.EndsWith("_Outline"))
                    {
                        if(!newSpriteOutlines.Contains(asset[s]))
                        {
                            newSpriteOutlines.Add(asset[s]);
                        }
                    }
                    else
                    {
                        if (!newSprites.Contains(asset[s]))
                        {
                            newSprites.Add(asset[s]);
                        }
                    }
                }
            }

            if (sm.sprites.Count != newSprites.Count)
            {
                sm.sprites.Clear();
                sm.sprites = newSprites;

                if(!Application.isPlaying)
                {
                    EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
                }
            }

            if (sm.spriteOutlines.Count != newSpriteOutlines.Count)
            {
                sm.spriteOutlines.Clear();
                sm.spriteOutlines = newSpriteOutlines;

                if (!Application.isPlaying)
                {
                    EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
                }
            }
        }
    }

    public static void LoadAllAudio()
    {
        ObjectManager sm = GameObject.FindObjectOfType<ObjectManager>();
        if (sm)
        {
            string[] guids = AssetDatabase.FindAssets("t:AudioClip");
            List<AudioClip> newSprites = new List<AudioClip>();

            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                AudioClip[] asset = AssetDatabase.LoadAllAssetsAtPath(path).OfType<AudioClip>().ToArray();

                newSprites.AddRange(asset);
            }

            if (sm.sprites.Count != guids.Length)
            {
                sm.clips.Clear();
                sm.clips = newSprites;

                if (Application.isPlaying) return;
                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            }
        }
    }

    public static void LoadTips()
    {
        GameManager gm = GameObject.FindObjectOfType<GameManager>();
        if (gm)
        {
            gm.tips.Clear();

            string[] guids = AssetDatabase.FindAssets("t:Tip");
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                Tip asset = AssetDatabase.LoadAssetAtPath<Tip>(path);

                gm.tips.Add(asset);
            }

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }
    }
    
    public static void LoadItems()
    {
        ItemManager im = GameObject.FindObjectOfType<ItemManager>();
        if (im)
        {
            im.items.Clear();
            im.consumables.Clear();
            im.armor.Clear();
            im.builds.Clear();
            im.guns.Clear();
            im.ammo.Clear();

            string[] guids = AssetDatabase.FindAssets("t:Item");
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                Item asset = AssetDatabase.LoadAssetAtPath<Item>(path);
                im.items.Add(asset);
            }
            guids = AssetDatabase.FindAssets("t:Consumable");
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                Consumable asset = AssetDatabase.LoadAssetAtPath<Consumable>(path);
                im.consumables.Add(asset);
            }
            guids = AssetDatabase.FindAssets("t:AmmoType");
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                AmmoType asset = AssetDatabase.LoadAssetAtPath<AmmoType>(path);
                im.ammo.Add(asset);
            }
            guids = AssetDatabase.FindAssets("t:Armor");
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                Armor asset = AssetDatabase.LoadAssetAtPath<Armor>(path);
                im.armor.Add(asset);
            }
            guids = AssetDatabase.FindAssets("t:PlayerBuild");
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                PlayerBuild asset = AssetDatabase.LoadAssetAtPath<PlayerBuild>(path);
                im.builds.Add(asset);
            }
            guids = AssetDatabase.FindAssets("t:Gun");
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                Gun asset = AssetDatabase.LoadAssetAtPath<Gun>(path);
                im.guns.Add(asset);
            }

            im.items = im.items.OrderBy(o => o.ID).ToList();
            im.consumables = im.consumables.OrderBy(o => o.ID).ToList();
            im.armor = im.armor.OrderBy(o => o.ID).ToList();
            im.guns = im.guns.OrderBy(o => o.ID).ToList();

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }
    }

    public static void LoadTiles()
    {
        GeneratorManager gm = GameObject.FindObjectOfType<GeneratorManager>();
        if (gm)
        {
            gm.art.Clear();
            gm.artFloorAlt.Clear();

            string[] tileGuids = AssetDatabase.FindAssets("t:GameObject", new string[] { "Assets/Prefabs/Generator/Tiles" });
            string[] tileAltGuids = AssetDatabase.FindAssets("t:GameObject", new string[] { "Assets/Prefabs/Generator/TilesAlt" });

            for(int i = 0; i < tileAltGuids.Length;i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(tileAltGuids[i]);
                GameObject tile = (GameObject)AssetDatabase.LoadAssetAtPath(path, typeof(GameObject));

                GeneratorTile.GeneratorTileSingle newTileSingle = new GeneratorTile.GeneratorTileSingle()
                {
                    chance = 100,
                    tile = tile
                };
                string tileName = Path.GetFileNameWithoutExtension(path).Substring(1);
                if (tileName.Contains("_"))
                {
                    tileName = tileName.Substring(0, tileName.Length - 2);
                }
                int tileId = int.Parse(tileName);

                GeneratorTile asset = new GeneratorTile()
                {
                    idName = "Tile " + tileId,
                    id = tileId
                };
                asset.tiles.Add(newTileSingle);
                gm.artFloorAlt.Add(asset);
            }

            for (int i = 0; i < tileGuids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(tileGuids[i]);
                GameObject tile = (GameObject)AssetDatabase.LoadAssetAtPath(path, typeof(GameObject));

                GeneratorTile.GeneratorTileSingle newTileSingle = new GeneratorTile.GeneratorTileSingle()
                {
                    chance = 100,
                    tile = tile
                };
                string tileName = Path.GetFileNameWithoutExtension(path).Substring(1);
                if (tileName.Contains("_"))
                {
                    tileName = tileName.Substring(0, tileName.Length - 2);
                }
                int tileId = int.Parse(tileName);
                bool exists = false;
                for (int t = 0; t < gm.art.Count; t++)
                {
                    if (gm.art[t].id == tileId)
                    {
                        exists = true;
                        int newChance = Mathf.RoundToInt(100f / (gm.art[t].tiles.Count + 1) * 1f);
                        gm.art[t].tiles.Add(newTileSingle);

                        for (int c = 0; c < gm.art[t].tiles.Count; c++)
                        {
                            gm.art[t].tiles[c].chance = newChance;
                        }
                    }
                }
                if (!exists)
                {
                    GeneratorTile asset = new GeneratorTile()
                    {
                        idName = "Tile " + tileId,
                        id = tileId
                    };
                    asset.tiles.Add(newTileSingle);
                    gm.art.Add(asset);
                }
            }

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }
    }

    public static void LoadGeneratorPresets()
    {
        GeneratorManager gm = GameObject.FindObjectOfType<GeneratorManager>();
        if (gm)
        {
            //Load sprites
            gm.presets.Clear();

            string[] guids = AssetDatabase.FindAssets("t:GameObject", new string[] { "Assets/Items/Generator Presets" });

            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                GameObject preset = (GameObject)AssetDatabase.LoadAssetAtPath(path, typeof(GameObject));
                
                gm.presets.Add(preset.GetComponent<GeneratorPreset>());
            }

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }
    }

    public static void LoadTileSets()
    {
        GeneratorManager gm = GameObject.FindObjectOfType<GeneratorManager>();
        if (gm)
        {
            //Load sprites
            gm.tilesets.Clear();

            string[] spriteGuids = AssetDatabase.FindAssets("t:Texture", new string[] {"Assets/Art/Sprites/Tiles"});

            for (int i = 0; i < spriteGuids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(spriteGuids[i]);
                Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToArray();

                GeneratorTileset asset = new GeneratorTileset()
                {
                    name = Path.GetFileNameWithoutExtension(path),
                    sprites = sprites
                };
                gm.tilesets.Add(asset);
            }

            if (Application.isPlaying) return;
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }
    }
}

public static class EditorUtilityExtra
{
    public static void PlayClip(AudioClip clip)
    {
        Assembly unityEditorAssembly = typeof(AudioImporter).Assembly;
        Type audioUtilClass = unityEditorAssembly.GetType("UnityEditor.AudioUtil");
        MethodInfo method = audioUtilClass.GetMethod(
            "PlayClip",
            BindingFlags.Static | BindingFlags.Public,
            null,
            new System.Type[] {
                typeof(AudioClip)
            },
            null
        );
        method.Invoke(
            null,
            new object[] {
                clip
            }
        );
    } 

    public static void StopAllClips()
    {
        Assembly unityEditorAssembly = typeof(AudioImporter).Assembly;
        Type audioUtilClass =
              unityEditorAssembly.GetType("UnityEditor.AudioUtil");
        MethodInfo method = audioUtilClass.GetMethod(
            "StopAllClips",
            BindingFlags.Static | BindingFlags.Public,
            null,
            new System.Type[] { },
            null
        );
        method.Invoke(
            null,
            new object[] { }
        );
    }
}