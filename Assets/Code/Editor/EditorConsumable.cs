using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Data;

[CustomEditor(typeof(Consumable))]
public class EditorConsumable : Editor
{
    void ShowDuplicate(int id)
    {
        if (!ItemManager.singleton) ItemManager.singleton = FindObjectOfType<ItemManager>();

        List<string> duplicates = new List<string>();

        for (int i = 0; i < ItemManager.singleton.consumables.Count; i++)
        {
            if (ItemManager.singleton.consumables[i].ID == id && ItemManager.singleton.consumables[i] != (Consumable)target)
            {
                duplicates.Add(ItemManager.singleton.consumables[i].name + " already has this ID!");
            }
        }

        if (duplicates.Count > 0)
        {
            int smallestId = 0;
            for (int i = 0; i < ItemManager.singleton.consumables.Count; i++)
            {
                if (!ItemManager.GetConsumable(i))
                {
                    smallestId = i;
                    break;
                }
            }
            EditorGUILayout.HelpBox(string.Join("\n", duplicates.ToArray()) + "\nTry " + smallestId, UnityEditor.MessageType.Error);
            if (GUILayout.Button("Fix ID"))
            {
                Consumable item = (Consumable)target;
                item.lockId = false;
                item.ID = smallestId;
                item.lockId = true;
            }
        }
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        Consumable item = (Consumable)target;

        GUIStyle style = "IN LockButton";
        item.lockId = GUI.Toggle(new Rect(Screen.width - 34, 52, 25, 25), item.lockId, GUIContent.none, style);

        EditorGUI.BeginDisabledGroup(item.lockId);
        int id = EditorGUILayout.IntField("ID", item.ID, GUILayout.Width(Screen.width - 48));
        item.ID = id;
        EditorGUI.EndDisabledGroup();

        ShowDuplicate(item.ID);

        item.price = EditorGUILayout.IntField("Price", item.price);

        item.ready = EditorGUILayout.Toggle("Ready for game", item.ready);
        item.clearOnFinish = EditorGUILayout.Toggle("Clears on finish", item.clearOnFinish);
        item.forever = EditorGUILayout.Toggle("Lasts forever", item.forever);
        if (!item.forever)
        {
            item.duration = EditorGUILayout.FloatField("Duration", item.duration);
            item.extendOnKill = EditorGUILayout.Toggle("Extend on kill", item.extendOnKill);
            if (item.extendOnKill)
            {
                EditorGUI.indentLevel++;
                item.extendOnKillAmount = EditorGUILayout.FloatField("Extend amount", item.extendOnKillAmount);
                EditorGUI.indentLevel--;
            }
        }

        item.icon = (Sprite)EditorGUILayout.ObjectField("Icon", item.icon, typeof(Sprite), false);
        item.description = EditorGUILayout.TextArea(item.description);
        item.type = (Consumable.ConsumableType)EditorGUILayout.EnumPopup("Type", item.type);

        Quality.ShowInspector(ref item.qualities);

        item.audio = (AudioSet)EditorGUILayout.ObjectField("Audio", item.audio, typeof(AudioSet), false);

        if(GUI.changed)
        {
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(target);
        }

        serializedObject.ApplyModifiedProperties();
        EditorUtility.SetDirty(target);
    }
}