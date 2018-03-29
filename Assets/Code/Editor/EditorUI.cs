using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class EditorUI : EditorWindow
{
    HUD hudObject;

    [MenuItem("Custom/Helper")]
    public static void Init()
    {
        EditorUI window = EditorWindow.GetWindow<EditorUI>("Helper");
        window.Show();
        window.Focus();
    }
    
    string command;

    void OnGUI()
    {
        if (EditorSceneManager.GetActiveScene().name != "Game")
        {
            return;
        }

        EditorPrefs.SetBool("ui", EditorGUILayout.Foldout(EditorPrefs.GetBool("ui"), "UI"));
        if (EditorPrefs.GetBool("ui"))
        {
            EditorGUI.indentLevel = 1;
            if (hudObject)
            {
                for (int i = 0; i < hudObject.layers.Length; i++)
                {
                    if (GUILayout.Button(hudObject.layers[i].name, GUILayout.Height(25)))
                    {
                        EditorGUIUtility.PingObject(hudObject.layers[i]);
                        HUD.ChangeLayer(hudObject.layers[i].name);
                    }
                }
            }
            EditorGUI.indentLevel = 0;
        }

        EditorPrefs.SetBool("playtesting", EditorGUILayout.Foldout(EditorPrefs.GetBool("playtesting"), "Playtesting"));
        if (EditorPrefs.GetBool("playtesting"))
        {
            EditorGUI.indentLevel = 1;

            if(!LevelManager.singleton)
            {
                LevelManager.singleton = FindObjectOfType<LevelManager>();
            }
            if(!GeneratorManager.singleton)
            {
                GeneratorManager.singleton = FindObjectOfType<GeneratorManager>();
            }

            List<string> levels = new List<string>();
            for(int i = 0; i < LevelManager.Order.Count;i++)
            {
                levels.Add(i+" "+LevelManager.Order[i].tileset+" ("+LevelManager.Order[i].levelName+")");
            }

            EditorGUILayout.LabelField("Start level : ");

            GeneratorManager.singleton.startStage = EditorGUILayout.Popup(GeneratorManager.singleton.startStage, levels.ToArray());
            GeneratorManager.singleton.autoGenerate = EditorGUILayout.Toggle("Auto generate", GeneratorManager.singleton.autoGenerate);
            
            EditorGUILayout.LabelField("Console command : ");
            command = EditorGUILayout.TextField(command);
            if (GUILayout.Button("Execute"))
            {
                Console.Run(command);
            }

            EditorPrefs.SetBool("god", EditorGUILayout.Toggle("God mode", EditorPrefs.GetBool("god")));
            EditorPrefs.SetBool("infAmmo", EditorGUILayout.Toggle("Inf ammo", EditorPrefs.GetBool("infAmmo")));

            if (Character.Player)
            {
                if(!ItemManager.singleton)
                {
                    ItemManager.singleton = FindObjectOfType<ItemManager>();
                }

                Character.Player.Health.invincible = EditorPrefs.GetBool("god");
                Character.Player.GunShooter.infiniteAmmo = EditorPrefs.GetBool("infAmmo");
                
                //guns
                int primaryIndex = -1;
                int secondaryIndex = -1;

                if(Character.Player.GunShooter.guns.Count > 0)
                {
                    List<string> guns = new List<string>();
                    for (int i = 0; i < ItemManager.singleton.guns.Count; i++)
                    {
                        if (ItemManager.singleton.guns[i].name == Character.Player.GunShooter.guns[0].gun.name)
                        {
                            primaryIndex = guns.Count;
                        }
                        if (Character.Player.GunShooter.guns.Count > 1 && ItemManager.singleton.guns[i].name == Character.Player.GunShooter.guns[1].gun.name)
                        {
                            secondaryIndex = guns.Count;
                        }
                        guns.Add(ItemManager.singleton.guns[i].name);
                    }

                    if (secondaryIndex == -1)
                    {
                        secondaryIndex = guns.Count;
                    }
                    if (primaryIndex == -1)
                    {
                        primaryIndex = guns.Count;
                    }
                    guns.Add("Empty");

                    EditorGUILayout.LabelField("Guns : ");

                    int newPrimary = EditorGUILayout.Popup(primaryIndex, guns.ToArray());
                    int newSecondary = EditorGUILayout.Popup(secondaryIndex, guns.ToArray());

                    if (newPrimary != primaryIndex)
                    {
                        Character.Player.GunShooter.guns[0].gun = ItemManager.GetGun(guns[newPrimary]);
                        Character.Player.GunShooter.guns[0].nextShot = 0f;
                    }
                    if (newSecondary != secondaryIndex)
                    {
                        if (Character.Player.GunShooter.guns.Count == 1 && Character.Player.GunShooter.CanEquip)
                        {
                            Character.Player.GunShooter.Equip(guns[newPrimary]);
                        }
                        else if (Character.Player.GunShooter.guns.Count > 1)
                        {
                            Character.Player.GunShooter.guns[1].gun = ItemManager.GetGun(guns[newSecondary]);
                            Character.Player.GunShooter.guns[1].nextShot = 0f;
                        }
                    }
                }
            }

            EditorGUI.indentLevel = 0;
        }
    }

    void Update()
    {
        if (EditorSceneManager.GetActiveScene().name != "Game")
        {
            EditorSceneManager.OpenScene("Assets/Scenes/Game.unity");
            return;
        }

        if (!Application.isPlaying)
        {
            hudObject = FindObjectOfType<HUD>();
            if (hudObject)
            {
                hudObject.layers = hudObject.GetComponentsInChildren<InterfaceLayer>(true);
            }
        }
    }
}
