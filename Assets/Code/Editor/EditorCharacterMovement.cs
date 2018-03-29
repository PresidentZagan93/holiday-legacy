using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CanEditMultipleObjects]
[CustomEditor(typeof(CharacterMovement))]
public class EditorCharacterMovement : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        CharacterMovement movement = (CharacterMovement)target;
        CharacterLook look = movement.Look ?? movement.GetComponent<CharacterLook>() ?? null;

        movement.acceleration = EditorGUILayout.IntField("Acceleration", movement.acceleration);
        movement.speed = EditorGUILayout.IntField("Speed", movement.speed);
        movement.runOnTarget = EditorGUILayout.Toggle("Runs", movement.runOnTarget);
        if(movement.runOnTarget)
        {
            movement.runSpeed = EditorGUILayout.IntField("Run Speed", movement.runSpeed);
        }

        movement.footsteps = EditorGUILayout.Toggle("Footsteps", movement.footsteps);

        movement.mode = (CharacterMovement.CharacterMovementMode)EditorGUILayout.EnumPopup("Mode", movement.mode);
        movement.root = (Transform)EditorGUILayout.ObjectField("Root", movement.root, typeof(Transform), true);
        if (movement.mode == CharacterMovement.CharacterMovementMode.Player)
        {
            EditorGUI.indentLevel = 1;
            EditorGUILayout.LabelField("User driven movement");
            EditorGUI.indentLevel = 0;
        }
        else if(movement.mode == CharacterMovement.CharacterMovementMode.Charge)
        {
            EditorGUI.indentLevel = 1;
            movement.chargeRate = EditorGUILayout.FloatField("Charge rate", movement.chargeRate);
            movement.chargeDuration = EditorGUILayout.FloatField("Charge duration", movement.chargeDuration);
            movement.chargeSpeed = EditorGUILayout.FloatField("Charge speed", movement.chargeSpeed);
            EditorGUI.indentLevel = 0;
        }
        else if (movement.mode == CharacterMovement.CharacterMovementMode.Wander)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("moveDuration"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("moveRate"));
        }
        else if(movement.mode == CharacterMovement.CharacterMovementMode.FollowTeam || movement.mode == CharacterMovement.CharacterMovementMode.FollowOwner)
        {
            movement.wanderIfNoTarget = EditorGUILayout.Toggle("Wander", movement.wanderIfNoTarget);
            if(movement.wanderIfNoTarget)
            {
                EditorGUI.indentLevel = 1;
                EditorGUILayout.PropertyField(serializedObject.FindProperty("moveDuration"));
                EditorGUI.indentLevel = 1;
                EditorGUILayout.PropertyField(serializedObject.FindProperty("moveRate"));
                EditorGUI.indentLevel = 0;
            }

            movement.overrideFollowTeam = EditorGUILayout.Toggle("Customize", movement.overrideFollowTeam);
            
            if (movement.overrideFollowTeam)
            {
                EditorGUI.indentLevel++;
                if (movement.mode == CharacterMovement.CharacterMovementMode.FollowTeam)
                {
                    movement.overrideTeam = (GameTeam)EditorGUILayout.EnumPopup("Team", movement.overrideTeam);
                    movement.followDistance = EditorGUILayout.FloatField("Distance", movement.followDistance);
                    movement.ignoreRaycast = EditorGUILayout.Toggle("Ignore LOS", movement.ignoreRaycast);
                }
                else
                {
                    movement.followDistance = EditorGUILayout.FloatField("Follow distance", movement.followDistance);
                    EditorGUILayout.LabelField("Follows owner");
                }
                if (movement.followTarget)
                {
                    EditorGUILayout.LabelField("Following "+movement.followTarget.name);
                }
                else
                {
                    EditorGUILayout.LabelField("No follow target");
                }
                EditorGUI.indentLevel--;
            }
            else
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.LabelField("Will follow the " + look.lookAtTeam+" team");
                EditorGUI.indentLevel--;
            }
        }

        serializedObject.ApplyModifiedProperties();
        EditorUtility.SetDirty(target);
    }
}
