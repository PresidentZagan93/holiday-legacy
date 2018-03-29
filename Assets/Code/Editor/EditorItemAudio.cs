using UnityEngine;
using UnityEditor;
using System.Collections;
using Data;

[CanEditMultipleObjects]
[CustomEditor(typeof(AudioSet))]
public class EditorItemAudio : Editor
{
    void OnEnable()
    {

    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        AudioSet item = (AudioSet)target;

        EditorGUI.indentLevel = 0;
        SerializedProperty chance = serializedObject.FindProperty("audioClips");

        for (int i = 0; i < chance.arraySize; i++)
        {
            SerializedProperty arrayItem = chance.GetArrayElementAtIndex(i);
            string offsetLabel = arrayItem.FindPropertyRelative("name").stringValue;
            arrayItem.FindPropertyRelative("name").stringValue = EditorGUILayout.TextField(offsetLabel);
            EditorGUILayout.ObjectField(arrayItem.FindPropertyRelative("clip"));
            if (GUILayout.Button("Play"))
            {
                EditorUtilityExtra.PlayClip(arrayItem.FindPropertyRelative("clip").objectReferenceValue as AudioClip);
            }
            if (GUILayout.Button("Remove"))
            {
                item.audioClips.RemoveAt(i);
            }
        }

        if(GUILayout.Button("Add"))
        {
            string chanceName = "AudioClip"+item.audioClips.Count;

            item.audioClips.Add(new AudioSet.AudioClipPair(chanceName));
        }

        EditorGUI.indentLevel = 0;

        serializedObject.ApplyModifiedProperties();
        EditorUtility.SetDirty(target);
    }
}