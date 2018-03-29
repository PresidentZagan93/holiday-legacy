using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using Data;

public class EditorItemsWindow : EditorWindow
{
    [MenuItem("Custom/Items")]
    public static void ItemsOpen()
    {
        GetWindow(typeof(EditorItemsWindow), false, "Items");
    }

    string search = "";
    Vector2 scrollPosition = new Vector2(0, 0);

    private void OnEnable()
    {
        Search();
    }

    void ShowSearchField()
    {
        GUILayout.BeginHorizontal(GUI.skin.FindStyle("Toolbar"));
        GUILayout.Space(Screen.width - 150);
        string newSearch = search;
        GUIStyle style = GUI.skin.FindStyle("ToolbarSeachTextField");
        if(style == null)
        {
            style = EditorStyles.objectField;
        }
        search = GUILayout.TextField(search, style);
        if (GUILayout.Button("", GUI.skin.FindStyle("ToolbarSeachCancelButton")))
        {
            // Remove focus if cleared
            search = "";
            GUI.FocusControl(null);
        }

        if(newSearch != search)
        {
            Search();
        }

        GUILayout.EndHorizontal();
    }

    bool Has(object obj, string word)
    {
        word = word.ToLower();

        var fields = obj.GetType().GetFields();
        for(int i = 0; i < fields.Length;i++)
        {
            var type = fields[i].FieldType;

            if(type == typeof(string))
            {
                return fields[i].GetValue(obj).ToString().ToLower().Contains(word);
            }
        }

        return false;
    }

    List<IItem> results = new List<IItem>();

    void Search()
    {
        ItemManager im = GameObject.FindObjectOfType<ItemManager>();

        results.Clear();
        bool lookForArc = search.Contains("arc");

        var items = im.items.OrderBy(x => x.ID).ToList();
        for (int i = 0; i < items.Count;i++)
        {
            if (items[i].name.ToLower().Contains(search.ToLower()) || search == "" || Has(items[i], search))
            {
                results.Add(items[i]);
            }
        }
        var consumables = im.consumables.OrderBy(x => x.ID).ToList();
        for (int i = 0; i < consumables.Count; i++)
        {
            if (consumables[i].name.ToLower().Contains(search.ToLower()) || search == "" || Has(consumables[i], search))
            {
                results.Add(consumables[i]);
            }
        }
        var guns = im.guns.OrderBy(x => x.ID).ToList();
        for (int i = 0; i < guns.Count; i++)
        {
            bool sameType = guns[i].type.ToString().ToLower().Contains(search.ToLower()) ||
                guns[i].ammoType.ToString().ToLower().Contains(search.ToLower());

            if (guns[i].name.ToLower().Contains(search.ToLower()) || search == "" || Has(guns[i], search) || sameType || (guns[i].projectileArcs && lookForArc))
            {
                results.Add(guns[i]);
            }
        }
        var armor = im.armor.OrderBy(x => x.ID).ToList();
        for (int i = 0; i < armor.Count; i++)
        {
            bool sameType = armor[i].type.ToString().ToLower().Contains(search.ToLower());

            if (armor[i].name.ToLower().Contains(search.ToLower()) || search == "" || Has(armor[i], search) || sameType)
            {
                results.Add(armor[i]);
            }
        }
    }

    private void OnGUI()
    {
        ShowSearchField();

        EditorGUI.LabelField(new Rect(5, 0, 100, 25), "Type");
        EditorGUI.LabelField(new Rect(105, 0, 200, 25), "ID");
        EditorGUI.LabelField(new Rect(155, 0, 200, 25), "Name");

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        for (int i = 0; i < results.Count;i++)
        {
            if(GUI.Button(new Rect(0, i * 20, Screen.width-4, 20), "", EditorStyles.helpBox))
            {
                //EditorGUIUtility.PingObject(results[i] as Object);
                Selection.activeObject = results[i] as Object;
            }

            string type = "";
            if (results[i] is Item)
            {
                type = "Item";
            }
            else if (results[i] is Gun)
            {
                type = "Gun";
            }
            else if (results[i] is Armor)
            {
                type = "Armor";
            }
            else if (results[i] is Consumable)
            {
                type = "Consumable";
            }

            EditorGUI.LabelField(new Rect(5, 2 + i * 20, 100, 25), type);
            EditorGUI.LabelField(new Rect(105, 2 + i * 20, 100, 25), results[i].GetID() + "");
            EditorGUI.LabelField(new Rect(155, 2 + i * 20, 300, 25), results[i].GetName());

            GUILayout.Space(20);
        }

        EditorGUILayout.EndScrollView();
    }
}