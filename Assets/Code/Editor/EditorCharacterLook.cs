using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CanEditMultipleObjects]
[CustomEditor(typeof(CharacterLook))]
public class EditorCharacterLook : Editor {

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        CharacterLook look = (CharacterLook)target;

        EditorGUILayout.LabelField("Direction : "+look.lookDirection.ToString());

        look.mode = (CharacterLook.CharacterLookMode)EditorGUILayout.EnumPopup("Mode", look.mode);

        EditorGUI.indentLevel = 1;
        if(look.mode == CharacterLook.CharacterLookMode.Mouse)
        {
            EditorGUILayout.LabelField("User driven mouse look");
        }
        if(look.mode == CharacterLook.CharacterLookMode.Direction)
        {
            look.direction = EditorGUILayout.FloatField("Angle direction", look.direction);
        }
        if(look.mode == CharacterLook.CharacterLookMode.Target)
        {
            look.maxTargetDistance = EditorGUILayout.FloatField("Max distance", look.maxTargetDistance);
            look.lookAtTeam = (GameTeam)EditorGUILayout.EnumPopup("Target team", look.lookAtTeam);
            EditorGUI.indentLevel = 2;
            if (look.target)
            {
                EditorGUILayout.LabelField("Looking at "+look.target.name);
            }
            else
            {
                EditorGUILayout.LabelField("No target to look at");
            }
            EditorGUI.indentLevel = 1;
        }

        EditorGUI.indentLevel = 0;

        serializedObject.ApplyModifiedProperties();
        EditorUtility.SetDirty(target);
    }
}
