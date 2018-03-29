using System.Collections;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class EditorDependencyChecker : EditorWindow {

    List<MonoScript> types = new List<MonoScript>();
    List<GameObject> dependencies = new List<GameObject>();

    [MenuItem("Custom/Dependency Checker")]
    public static void ItemsOpen()
    {
        GetWindow(typeof(EditorDependencyChecker), false, "DP Check");
    }

    private void OnEnable()
    {
        Refresh();
    }

    void Refresh()
    {
        this.types.Clear();
        var types = EditorHelper.Types.ToList();
        for(int i = 0; i < types.Count;i++)
        {
            var guids = AssetDatabase.FindAssets("t:Script " + types[i].name + "", null);
            for (int a = 0; a < guids.Length; a++)
            {
                string fileName = Path.GetFileNameWithoutExtension(AssetDatabase.GUIDToAssetPath(guids[a]));
                if(fileName == types[i].name)
                {
                    this.types.Add(types[i]);
                    break;
                }
            }
        }
    }

    void FindDependencies(MonoScript script)
    {
        dependencies.Clear();
        var guids = AssetDatabase.FindAssets("t:GameObject", null);
        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            GameObject asset = (GameObject)AssetDatabase.LoadAssetAtPath(path, typeof(GameObject));

            MonoBehaviour[] behs = asset.GetComponentsInChildren<MonoBehaviour>();
            bool contains = false;
            for (int b = 0; b < behs.Length; b++)
            {
                if (behs[b].GetType() == script.GetType()) contains = true;
            }
            if(contains) dependencies.Add(asset);
        }
    }

    private void OnGUI()
    {
        if(GUILayout.Button("Refresh Assembly"))
        {
            Refresh();
        }

        int selectedIndex = EditorPrefs.GetInt("dpCheck_index", 0);
        List<string> options = new List<string>();
        for(int i = 0; i < types.Count;i++)
        {
            options.Add(types[i].name);
        }

        selectedIndex = EditorGUILayout.Popup("Script", selectedIndex, options.ToArray());
        EditorPrefs.SetInt("dpCheck_index", selectedIndex);

        if (GUILayout.Button("Find Dependencies"))
        {
            FindDependencies(types[selectedIndex]);
        }

        for (int i = 0; i < dependencies.Count; i++)
        {
            string parent = dependencies[i].transform.parent ? dependencies[i].transform.parent.name + "/" : "";
            string superParent = parent != "" ? dependencies[i].transform.parent.parent ? dependencies[i].transform.parent.parent.name + "/" : "" : "";
            string extremeParent = superParent != "" ? dependencies[i].transform.parent.parent.parent ? dependencies[i].transform.parent.parent.parent.name + "/" : "" : "";

            if (GUILayout.Button(extremeParent + superParent + dependencies[i].name))
            {
                EditorGUIUtility.PingObject(dependencies[i]);
            }
        }
    }
}
