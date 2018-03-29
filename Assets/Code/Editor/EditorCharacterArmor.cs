using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Data;

[CanEditMultipleObjects]
[CustomEditor(typeof(CharacterArmor))]
public class EditorCharacterArmor : Editor {


    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        CharacterArmor armor = (CharacterArmor)target;

        armor.root = (Transform)EditorGUILayout.ObjectField("Root", armor.root, typeof(Transform), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("startingArmor"), true);

        armor.maxArmor = EditorGUILayout.IntField("Slots", armor.maxArmor);
        armor.headOffset = EditorGUILayout.IntField("Head offset", armor.headOffset);
        armor.spritePlayer = (SpritePlayer)EditorGUILayout.ObjectField("Sprite player", armor.spritePlayer, typeof(SpritePlayer), true);
        armor.offsets = (SpritePlayerOffsets)EditorGUILayout.ObjectField("Offsets", armor.offsets, typeof(SpritePlayerOffsets), true);
        
        var prop = serializedObject.FindProperty("armor");
        if(prop.arraySize > 0)
        {
            EditorGUILayout.PropertyField(prop);
            if (prop.isExpanded)
            {
                for (int i = 0; i < prop.arraySize; i++)
                {
                    EditorGUI.indentLevel = 1;
                    SerializedProperty arrayItem = prop.GetArrayElementAtIndex(i);
                    string offsetLabel = ((Armor)arrayItem.FindPropertyRelative("armor").objectReferenceValue).name;

                    EditorGUILayout.LabelField(offsetLabel + " " + arrayItem.FindPropertyRelative("health").intValue);
                }
            }
        }
        else
        {
            EditorGUILayout.LabelField("No armor equipped");
        }
        

        EditorGUI.indentLevel = 0;

        serializedObject.ApplyModifiedProperties();
        EditorUtility.SetDirty(target);
    }
}
