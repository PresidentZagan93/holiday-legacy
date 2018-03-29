using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Data;

[CanEditMultipleObjects]
[CustomEditor(typeof(Armor))]
public class EditorArmor : Editor
{
    void ShowDuplicate(int id)
    {
        if (!ItemManager.singleton) ItemManager.singleton = FindObjectOfType<ItemManager>();

        List<string> duplicates = new List<string>();

        for (int i = 0; i < ItemManager.singleton.armor.Count; i++)
        {
            if (ItemManager.singleton.armor[i].ID == id && ItemManager.singleton.armor[i] != (Armor)target)
            {
                duplicates.Add(ItemManager.singleton.armor[i].name + " already has this ID!");
            }
        }

        if(duplicates.Count > 0)
        {
            int smallestId = 0;
            for (int i = 0; i < ItemManager.singleton.armor.Count; i++)
            {
                if (!ItemManager.GetArmor(i))
                {
                    smallestId = i;
                    break;
                }
            }
            EditorGUILayout.HelpBox(string.Join("\n", duplicates.ToArray()) + "\nTry " + smallestId, UnityEditor.MessageType.Error);
            if(GUILayout.Button("Fix ID"))
            {
                Armor item = (Armor)target;
                item.lockId = false;
                item.ID = smallestId;
                item.lockId = true;
            }
        }
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        Armor item = (Armor)target;

        GUIStyle style = "IN LockButton";
        item.lockId = GUI.Toggle(new Rect(Screen.width - 34, 52, 25, 25), item.lockId, GUIContent.none, style);

        EditorGUI.BeginDisabledGroup(item.lockId);
        int id = EditorGUILayout.IntField("ID", item.ID, GUILayout.Width(Screen.width - 48));
        item.ID = id;
        EditorGUI.EndDisabledGroup();

        ShowDuplicate(item.ID);

        item.price = EditorGUILayout.IntField("Price", item.price);

        item.ready = EditorGUILayout.Toggle("Ready for game", item.ready);
        item.type = (Armor.ArmorType)EditorGUILayout.EnumPopup("Type", item.type);
        item.order = (Armor.ArmorOrder)EditorGUILayout.EnumPopup("Order", item.order);
        if(item.type == Armor.ArmorType.Shield)
        {
            item.icon = (Sprite)EditorGUILayout.ObjectField("Active Shield", item.icon, typeof(Sprite), false);
            item.shieldDisabledIcon = (Sprite)EditorGUILayout.ObjectField("Idle Shield", item.shieldDisabledIcon, typeof(Sprite), false);
        }
        else
        {
            item.icon = (Sprite)EditorGUILayout.ObjectField("Icon", item.icon, typeof(Sprite), false);
        }

        item.description = EditorGUILayout.TextField("Description", item.description);
        item.visible = EditorGUILayout.Toggle("Visible", item.visible);
        if (item.visible)
        {
            EditorGUI.indentLevel++;
            if (item.type == Armor.ArmorType.Shield)
            {
                item.position = EditorGUILayout.Vector2Field("Active Position", item.position);
                item.shieldIdlePosition = EditorGUILayout.Vector2Field("Idle Position", item.shieldIdlePosition);
            }
            else
            {
                item.position = EditorGUILayout.Vector2Field("Position", item.position);
            }
            item.rotation = EditorGUILayout.IntField("Rotation", item.rotation);
            EditorGUI.indentLevel--;
        }

        Quality.ShowInspector(ref item.qualities);
        item.audio = (AudioSet)EditorGUILayout.ObjectField("Audio", item.audio, typeof(AudioSet), false);

        serializedObject.ApplyModifiedProperties();
        EditorUtility.SetDirty(target);
    }
}