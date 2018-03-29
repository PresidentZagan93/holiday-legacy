using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ItemManager))]
public class EditorItemManager : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        ItemManager manager = (ItemManager)target;

        SerializedProperty armor = serializedObject.FindProperty("armor");
        SerializedProperty items = serializedObject.FindProperty("items");
        SerializedProperty consumables = serializedObject.FindProperty("consumables");
        SerializedProperty guns = serializedObject.FindProperty("guns");
        SerializedProperty ammo = serializedObject.FindProperty("ammo");
        SerializedProperty builds = serializedObject.FindProperty("builds");

        EditorGUILayout.PropertyField(armor, true);
        EditorGUILayout.PropertyField(items, true);
        EditorGUILayout.PropertyField(consumables, true);
        EditorGUILayout.PropertyField(guns, true);
        EditorGUILayout.PropertyField(ammo, true);
        EditorGUILayout.PropertyField(builds, true);

        Quality.ShowInspector(ref manager.qualityOffsets);

        serializedObject.ApplyModifiedProperties();
        EditorUtility.SetDirty(target);
    }
}
