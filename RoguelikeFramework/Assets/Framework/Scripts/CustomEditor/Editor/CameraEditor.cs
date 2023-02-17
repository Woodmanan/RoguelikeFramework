using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CameraTracking))]
public class CameraEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        SerializedProperty mode = serializedObject.FindProperty("mode");

        EditorGUILayout.PropertyField(mode);
        
        if (mode.enumValueIndex == (int) CameraTrackingMode.ConstantSpeed)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("speed"));
        }

        if (mode.enumValueIndex == (int)CameraTrackingMode.Lerp)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("lerpAmount"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("stopDist"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("stopSpeedMultiplier"));
        }

        serializedObject.ApplyModifiedProperties();
    }
}
