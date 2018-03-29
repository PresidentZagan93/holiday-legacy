using UnityEngine;
using UnityEditor;
using System.Collections;
using Data;

[CanEditMultipleObjects]
[CustomEditor(typeof(Tip))]
public class EditorTip : Editor
{
    void OnEnable()
    {
        ConfigManager.Load();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        Tip tip = (Tip)target;

        tip.tip = EditorGUILayout.TextArea(tip.tip);

        SerializedProperty references = serializedObject.FindProperty("references");
        EditorGUILayout.PropertyField(references, new GUIContent("References"));
        if (references.isExpanded)
        {
            for (int i = 0; i < references.arraySize; i++)
            {
                EditorGUI.indentLevel = 1;
                SerializedProperty arrayItem = references.GetArrayElementAtIndex(i);

                EditorGUILayout.PropertyField(arrayItem.FindPropertyRelative("type"), new GUIContent("Reference ["+i+"]"));
                Tip.TipReference.TipReferenceType type = (Tip.TipReference.TipReferenceType)arrayItem.FindPropertyRelative("type").enumValueIndex;

                if (type == Tip.TipReference.TipReferenceType.Armor)
                {
                    EditorGUILayout.PropertyField(arrayItem.FindPropertyRelative("armorValue"), new GUIContent(""));
                }
                if (type == Tip.TipReference.TipReferenceType.Build)
                {
                    EditorGUILayout.PropertyField(arrayItem.FindPropertyRelative("buildValue"), new GUIContent(""));
                }
                if (type == Tip.TipReference.TipReferenceType.Enemy)
                {
                    EditorGUILayout.PropertyField(arrayItem.FindPropertyRelative("enemyValue"), new GUIContent(""));
                }
                if (type == Tip.TipReference.TipReferenceType.Gun)
                {
                    EditorGUILayout.PropertyField(arrayItem.FindPropertyRelative("gunValue"), new GUIContent(""));
                }
                if (type == Tip.TipReference.TipReferenceType.Item)
                {
                    EditorGUILayout.PropertyField(arrayItem.FindPropertyRelative("itemValue"), new GUIContent(""));
                }
                if (type == Tip.TipReference.TipReferenceType.Tileset)
                {
                    EditorGUILayout.PropertyField(arrayItem.FindPropertyRelative("tilesetValue"), new GUIContent(""));
                }
                if (type == Tip.TipReference.TipReferenceType.Consumable)
                {
                    EditorGUILayout.PropertyField(arrayItem.FindPropertyRelative("consumableValue"), new GUIContent(""));
                }
                if (type == Tip.TipReference.TipReferenceType.Setting)
                {
                    EditorGUILayout.PropertyField(arrayItem.FindPropertyRelative("settingValue"), new GUIContent(""));
                }

                if (GUILayout.Button("Remove"))
                {
                    tip.references.RemoveAt(i);
                }
            }

            if(GUILayout.Button("Add"))
            {
                tip.references.Add(new Tip.TipReference());
            }
        }
        EditorGUI.indentLevel = 0;

        EditorGUILayout.PropertyField(serializedObject.FindProperty("specificGuns"), new GUIContent("Gun Specifics"));
        SerializedProperty specificGuns = serializedObject.FindProperty("specificGuns");
        if (specificGuns.isExpanded)
        {
            for (int i = 0; i < specificGuns.arraySize; i++)
            {
                EditorGUI.indentLevel = 1;
                SerializedProperty arrayItem = specificGuns.GetArrayElementAtIndex(i);
                EditorGUILayout.PropertyField(arrayItem);
                if (GUILayout.Button("Remove"))
                {
                    tip.specificGuns.RemoveAt(i);
                }
            }
            if (GUILayout.Button("Add"))
            {
                tip.specificGuns.Add(null);
            }
        }

        EditorGUI.indentLevel = 0;
        
        EditorGUILayout.PropertyField(serializedObject.FindProperty("specificArmor"), new GUIContent("Armor Specifics"));
        SerializedProperty specificArmor = serializedObject.FindProperty("specificArmor");
        if (specificArmor.isExpanded)
        {
            for (int i = 0; i < specificArmor.arraySize; i++)
            {
                EditorGUI.indentLevel = 1;
                SerializedProperty arrayItem = specificArmor.GetArrayElementAtIndex(i);
                EditorGUILayout.PropertyField(arrayItem);
                if (GUILayout.Button("Remove"))
                {
                    tip.specificArmor.RemoveAt(i);
                }
            }
            if (GUILayout.Button("Add"))
            {
                tip.specificArmor.Add(null);
            }
        }

        EditorGUI.indentLevel = 0;

        EditorGUILayout.PropertyField(serializedObject.FindProperty("specificItems"), new GUIContent("Item Specifics"));
        SerializedProperty specificItems = serializedObject.FindProperty("specificItems");
        if (specificItems.isExpanded)
        {
            for (int i = 0; i < specificItems.arraySize; i++)
            {
                EditorGUI.indentLevel = 1;
                SerializedProperty arrayItem = specificItems.GetArrayElementAtIndex(i);
                EditorGUILayout.PropertyField(arrayItem);
                if (GUILayout.Button("Remove"))
                {
                    tip.specificItems.RemoveAt(i);
                }
            }
            if (GUILayout.Button("Add"))
            {
                tip.specificItems.Add(null);
            }
        }

        EditorGUI.indentLevel = 0;

        EditorGUILayout.PropertyField(serializedObject.FindProperty("specificConsumables"), new GUIContent("Consumable Specifics"));
        SerializedProperty specificConsumables = serializedObject.FindProperty("specificConsumables");
        if (specificConsumables.isExpanded)
        {
            for (int i = 0; i < specificConsumables.arraySize; i++)
            {
                EditorGUI.indentLevel = 1;
                SerializedProperty arrayItem = specificConsumables.GetArrayElementAtIndex(i);
                EditorGUILayout.PropertyField(arrayItem);
                if (GUILayout.Button("Remove"))
                {
                    tip.specificConsumables.RemoveAt(i);
                }
            }
            if (GUILayout.Button("Add"))
            {
                tip.specificConsumables.Add(null);
            }
        }

        EditorGUI.indentLevel = 0;

        EditorGUILayout.PropertyField(serializedObject.FindProperty("specificLevels"), new GUIContent("Level Specifics"));
        SerializedProperty specificLevels = serializedObject.FindProperty("specificLevels");
        if (specificLevels.isExpanded)
        {
            for (int i = 0; i < specificLevels.arraySize; i++)
            {
                EditorGUI.indentLevel = 1;
                SerializedProperty arrayItem = specificLevels.GetArrayElementAtIndex(i);
                EditorGUILayout.PropertyField(arrayItem);
                if (GUILayout.Button("Remove"))
                {
                    tip.specificLevels.RemoveAt(i);
                }
            }
            if (GUILayout.Button("Add"))
            {
                tip.specificLevels.Add(null);
            }
        }

        EditorGUI.indentLevel = 0;

        EditorGUILayout.PropertyField(serializedObject.FindProperty("specificTilesets"), new GUIContent("Tileset Specifics"));
        SerializedProperty specificTilesets = serializedObject.FindProperty("specificTilesets");
        if (specificTilesets.isExpanded)
        {
            for (int i = 0; i < specificTilesets.arraySize; i++)
            {
                EditorGUI.indentLevel = 1;
                SerializedProperty arrayItem = specificTilesets.GetArrayElementAtIndex(i);
                EditorGUILayout.PropertyField(arrayItem);
                if (GUILayout.Button("Remove"))
                {
                    tip.specificTilesets.RemoveAt(i);
                }
            }
            if (GUILayout.Button("Add"))
            {
                tip.specificTilesets.Add(null);
            }
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Preview : ");

        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.richText = true;
        style.wordWrap = true;

        EditorGUILayout.LabelField(tip.TipText, style);

        EditorUtility.SetDirty(target);
    }
}