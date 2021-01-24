using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/*
 * Why a custom editor? Because I'm a sucker for timesinks like this, and it looks nice.
 * (It's sterile and I like the taste, dammit)
 *
 * This class should be pretty easily extensible. To make a new inspector for a machine class,
 * just add the relevant tags, extend MachineEditor, and fill in the DrawInfo function with the 
 * desired properties. Then, it should render out into a nice window that animates and hides any
 * unneccessary info!
 *
 * See SimpleRoomMachineEditor below for a good example of how to do this!
 *
 * NOTE: If you're looking for a class that is in any capacity an editor and not just pretty, I would look for RoomEditor instead.
 */

[CustomEditor(typeof(Machine))]
[CanEditMultipleObjects]
public class MachineEditor : Editor
{
    bool showSize = true;
    bool showSpecifics = true;
    bool showNormal = false;

    public override void OnInspectorGUI () {
        serializedObject.Update();
        

        EditorGUILayout.PropertyField(serializedObject.FindProperty("priority"));
        DrawSizing();
        DrawMachineSpecifics();
        DrawNormal();

        serializedObject.ApplyModifiedProperties();
	}

    public void DrawSizing()
    {
        SerializedProperty global = serializedObject.FindProperty("global");
        showSize = EditorGUILayout.BeginFoldoutHeaderGroup(showSize, "Size Requirements");
        if (showSize)
        {
            EditorGUILayout.PropertyField(global);
            if (!global.boolValue)
            {           
                EditorGUILayout.PropertyField(serializedObject.FindProperty("size"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("canShareSpace"), new GUIContent("Overlap Allowed"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("canExpand"));
            }
            else
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("canShareSpace"), new GUIContent("Overlap Allowed"));
            }
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    public void DrawMachineSpecifics()
    {
        showSpecifics = EditorGUILayout.BeginFoldoutHeaderGroup(showSpecifics, "Machine Specific Information");
        if (showSpecifics)
        {
            DrawInfo();
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
    }
    
    public virtual void DrawInfo()
    {

    }


    public void DrawNormal()
    {
        showNormal = EditorGUILayout.BeginFoldoutHeaderGroup(showNormal, "Normal Inspector");
        if (showNormal)
        {
            DrawDefaultInspector();
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
    }
}

[CustomEditor(typeof(SimpleRoomMachine))]
[CanEditMultipleObjects]
public class SimpleRoomMachineEditor : MachineEditor
{
    public override void DrawInfo()
    {
        EditorGUILayout.PropertyField(serializedObject.FindProperty("numRooms"), new GUIContent("Max rooms"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("attemptsBeforeFail"), new GUIContent("Max failures"));
        ++EditorGUI.indentLevel;
        EditorGUILayout.PropertyField(serializedObject.FindProperty("rooms"));
        --EditorGUI.indentLevel;
    }
}

[CustomEditor(typeof(SimpleConnectionMachine))]
[CanEditMultipleObjects]
public class SimpleConnectionMachineEditor : MachineEditor
{
    public override void DrawInfo()
    {
        EditorGUILayout.HelpBox("This machine has no additional features", MessageType.Info);
    }
}