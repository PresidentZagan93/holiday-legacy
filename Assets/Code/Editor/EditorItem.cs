using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Data;

[CustomEditor(typeof(Item))]
public class EditorItem : Editor
{
    void ShowDuplicate(int id)
    {
        if (!ItemManager.singleton) ItemManager.singleton = FindObjectOfType<ItemManager>();

        List<string> duplicates = new List<string>();

        for (int i = 0; i < ItemManager.singleton.items.Count; i++)
        {
            if (ItemManager.singleton.items[i].ID == id && ItemManager.singleton.items[i] != (Item)target)
            {
                duplicates.Add(ItemManager.singleton.items[i].name + " already has this ID!");
            }
        }

        if (duplicates.Count > 0)
        {
            int smallestId = 0;
            for (int i = 0; i < ItemManager.singleton.items.Count; i++)
            {
                if (!ItemManager.GetItem(i))
                {
                    smallestId = i;
                    break;
                }
            }
            EditorGUILayout.HelpBox(string.Join("\n", duplicates.ToArray()) + "\nTry " + smallestId, UnityEditor.MessageType.Error);
            if (GUILayout.Button("Fix ID"))
            {
                Item item = (Item)target;
                item.lockId = false;
                item.ID = smallestId;
                item.lockId = true;
            }
        }
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        Item item = (Item)target;

        GUIStyle style = "IN LockButton";
        item.lockId = GUI.Toggle(new Rect(Screen.width - 34, 52, 25, 25), item.lockId, GUIContent.none, style);

        EditorGUI.BeginDisabledGroup(item.lockId);
        int id = EditorGUILayout.IntField("ID", item.ID, GUILayout.Width(Screen.width - 48));
        item.ID = id;
        EditorGUI.EndDisabledGroup();

        ShowDuplicate(item.ID);

        item.price = EditorGUILayout.IntField("Price", item.price);

        item.maxStacks = EditorGUILayout.IntField("Max stacks", item.maxStacks);
        if(item.maxStacks < 1)
        {
            item.maxStacks = 1;
        }

        item.ready = EditorGUILayout.Toggle("Ready for game", item.ready);
        item.icon = (Sprite)EditorGUILayout.ObjectField("Icon", item.icon, typeof(Sprite), false);
        item.description = EditorGUILayout.TextField("Description", item.description);
        item.type = (Consumable.ConsumableType)EditorGUILayout.EnumPopup("Type", item.type);

        SerializedProperty requirements = serializedObject.FindProperty("requirements");
        EditorGUILayout.PropertyField(requirements, new GUIContent("Requirements"), true);

        Quality.ShowInspector(ref item.qualities);

        item.audio = (AudioSet)EditorGUILayout.ObjectField("Audio", item.audio, typeof(AudioSet), false);

        serializedObject.ApplyModifiedProperties();
        EditorUtility.SetDirty(target);
    }
}