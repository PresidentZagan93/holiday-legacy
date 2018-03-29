using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class EditorGenerator : EditorWindow
{
    [MenuItem("Custom/Generator")]
	public static void GeneratorOpen()
    {
        EditorWindow.GetWindow(typeof(EditorGenerator));
    }

    string GetAdjective()
    {
        #region NameGeneration
        string[] adjectives = new string[] {
            "putrid",
            "horrid",
            "amazing",
            "beatiful",
            "sexy",
            "big",
            "small",
            "tiny",
            "gruesome",
            "funny",
            "stinky",
            "smelly"
            };

        #endregion

        string adj = adjectives[Random.Range(0, adjectives.Length)];
        if (adj.EndsWith("l") && !adj.EndsWith("ll"))
        {
            adj = adj.Substring(0, adj.Length - 1) + "ly";
        }

        adj = (adj[0] + "").ToUpper() + adj.Substring(1);

        return adj;
    }

    private void OnGUI()
    {
        GUILayout.Label("Weapon Generator");
        if(GUILayout.Button("Generate"))
        {
            string gunName = "";
            
            gunName += GetAdjective() + " ";
            gunName += GetAdjective() + " ";

            string[] types = new string[]
            {
                "Cannon",
                "Pulversiser",
                "Bullpup",
                "Sniper",
                "Pistol",
                "Lasergun",
                "Rocket Launcher",
                "Grenade Launcher",
                "Machine Gun",
                "Gun",
                "Flamethrower",
                "Sword",
                "Revolver"
            };

            string[] mods = new string[]
             {
                "Dual",
                "Triple",
                "Quad",
                "Recursive",
                "Compressed",
                "Backwards"
             };

            gunName += mods[Random.Range(0, mods.Length)] + " ";
            gunName += types[Random.Range(0, types.Length)];

            EditorPrefs.SetString("randomGun", gunName);
            EditorGUIUtility.systemCopyBuffer = gunName;
        }

        GUILayout.Label(EditorPrefs.GetString("randomGun", ""), EditorStyles.largeLabel);
    }
}
