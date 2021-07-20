using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using System.Linq;

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(StatusEffect))]
public class EffectPropertyDrawer : PropertyDrawer
{
    private static bool SHOW_SCRIPT_FIELD = false;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        float BTN_WIDTH = EditorGUIUtility.singleLineHeight + 3f;
        float BTN_GAP = 1f;

        //Clear the editor every frame - More expensive, but caching it acts weird in lists.
        Editor editor = null;

        //Sets up the total property, to determine sizing
        EditorGUI.BeginProperty(position, label, property);

        //Generate the list of all effect types
        List<Type> types = GetAllEffectTypes();
        List<String> names = types.Select( x => x.Name ).ToList();

        //Create the bounding boxes for our new piece. Not super important, the start positions are the main part that we need. xRect is unused if 'heldEffect' is null.
        var effectRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        var cRect = new Rect(position.x + position.width - (2 * BTN_WIDTH) - BTN_GAP, position.y, BTN_WIDTH, EditorGUIUtility.singleLineHeight);
        var xRect = new Rect(position.x + position.width - (BTN_WIDTH), position.y, BTN_WIDTH, EditorGUIUtility.singleLineHeight);
        

        //Get our serialized properties
        SerializedProperty typeVal = property.FindPropertyRelative("typeVal");
        SerializedProperty effect = property.FindPropertyRelative("heldEffect");

        //Handle the deletion of a property / New property creation!
        if (effect.objectReferenceValue == null)
        {
            typeVal.intValue = -1;
        }
        else
        {
            //Confirm that we have the right type
            typeVal.intValue = types.IndexOf(effect.objectReferenceValue.GetType());

            if (typeVal.intValue == -1)
            {
                Debug.LogError("Didn't find it!");
            }

            effectRect.width -= 2 * (BTN_WIDTH + BTN_GAP);

            //If we have a type, offer a clear button!
            if (GUI.Button(cRect, "C"))
            {
                //Create a new asset of the chosen type
                var newSO = ScriptableObject.CreateInstance(types[typeVal.intValue]);
                DateTime time = DateTime.Now;
                AssetDatabase.CreateAsset(newSO, $"Assets\\Prefabs and Script Objects\\Status Effects\\{names[typeVal.intValue]}-{time.ToString("MMddyyHHmmttff")}.asset");
                AssetDatabase.SaveAssets();

                //Refresh the editor values, in case the dropdown gets shown this frame
                effect.objectReferenceValue = newSO;
                Editor.CreateCachedEditor(effect.objectReferenceValue, null, ref editor);
            }
            
            //If we have a type, offer a deletion button!
            if (GUI.Button(xRect, "X"))
            {
                string path = AssetDatabase.GetAssetPath(effect.objectReferenceValue);
                effect.objectReferenceValue = null;
                AssetDatabase.DeleteAsset(path);
            }
        }

        //Set up the dropdown
        int selection = EditorGUI.Popup(effectRect, property.displayName, typeVal.intValue, names.ToArray());

        //Did we switch types? Only happens when the user clicks a new option on the dropdown
        if (selection != typeVal.intValue)
        {
            typeVal.intValue = selection;

            //We want something new, so clear the old value if it exists.
            if (effect.objectReferenceValue != null)
            {
                string path = AssetDatabase.GetAssetPath(effect.objectReferenceValue);
                effect.objectReferenceValue = null;
                AssetDatabase.DeleteAsset(path);
            }

            //Create a new asset of the chosen type
            var newSO = ScriptableObject.CreateInstance(types[typeVal.intValue]);
            DateTime time = DateTime.Now;
            //Doesn't account for decade. Surely this won't bite me in the ass. Also, no way someone makes two of these in 100th of a second, right?
            AssetDatabase.CreateAsset(newSO, $"Assets\\Prefabs and Script Objects\\Status Effects\\{names[typeVal.intValue]}-{time.ToString("MMddyyHHmmttff")}.asset"); 
            AssetDatabase.SaveAssets();

            //Refresh the editor values, in case the dropdown gets shown this frame
            effect.objectReferenceValue = newSO;
            Editor.CreateCachedEditor(effect.objectReferenceValue, null, ref editor);
        }

        //Offer that handy dropdown arrow, but only if we have something to show.
        if (effect.objectReferenceValue != null)
        {
            property.isExpanded = EditorGUI.Foldout(effectRect, property.isExpanded, GUIContent.none);
        }

        // Draw the SO properties, if we have an effect to draw. This code stolen from the internet. Thanks StackOverflow!
        if (effect.objectReferenceValue != null && property.isExpanded)
        {
            // Make child fields be indented
            EditorGUI.indentLevel++;

            // Draw object properties
            Editor.CreateCachedEditor(effect.objectReferenceValue, null, ref editor);

            if (editor == null)
                return; //This is the something went really wrong case.

            //March out rectangles for the sub-editor positions
            SerializedProperty field = editor.serializedObject.GetIterator();
            field.NextVisible(true);
            List<Rect> propertyRects = new List<Rect>();

            var marchingRect = new Rect(position.x, position.y + 20f, position.width, EditorGUIUtility.singleLineHeight);

            if (SHOW_SCRIPT_FIELD)
            {
                propertyRects.Add(marchingRect);
                marchingRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            }

            while (field.NextVisible(false))
            {
                marchingRect.height = EditorGUI.GetPropertyHeight(field, true);
                propertyRects.Add(marchingRect);
                marchingRect.y += marchingRect.height + EditorGUIUtility.standardVerticalSpacing;
            }

            //Draw the actual editor fields!
            int index = 0;
            field = editor.serializedObject.GetIterator();
            field.NextVisible(true);

            if (SHOW_SCRIPT_FIELD)
            {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUI.PropertyField(propertyRects[index], field, true);
                EditorGUI.EndDisabledGroup();
                index++;
            }

            //The magicc
            while (field.NextVisible(false))
            {
                try
                {
                    EditorGUI.PropertyField(propertyRects[index], field, true);
                }
                catch (StackOverflowException)
                {
                    field.objectReferenceValue = null;
                    Debug.LogError("Self-nesting detected. The offending object has been set to null, please don't do that anymore.");
                }

                index++;
            }

            EditorGUI.indentLevel--;

            //VERY IMPORTANT - THE WHOLE THING BREAKS WITHOUT THIS
            editor.serializedObject.ApplyModifiedProperties();
        }

        EditorGUI.EndProperty();
    }

    //Fantastic code modeled from Fydar's on the Unity Forums. Thanks for all the help!!
    //https://forum.unity.com/threads/editor-tool-better-scriptableobject-inspector-editing.484393/
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        Editor editor = null;

        float height = 0.0f;

        //Always have a single line's worth of height, since we always have our selector object
        height += EditorGUIUtility.singleLineHeight;

        SerializedProperty effect = property.FindPropertyRelative("heldEffect");

        if (effect.objectReferenceValue == null)
        {
            return height;
        }

        if (property.isExpanded == false)
        {
            return height;
        }

        Editor.CreateCachedEditor(effect.objectReferenceValue, null, ref editor);

        if (editor == null)
        {
            return height;
        }

        SerializedProperty field = editor.serializedObject.GetIterator();

        field.NextVisible(true);

        if (SHOW_SCRIPT_FIELD)
        {
            height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        }

        while (field.NextVisible(false))
        {
            height += EditorGUI.GetPropertyHeight(field, true) + EditorGUIUtility.standardVerticalSpacing;
        }


        return height;
    }

    public List<Type> GetAllEffectTypes()
    {
        Type baseType = typeof(Effect);
        Assembly assembly = baseType.Assembly;

        return assembly.GetTypes().Where(t => t.IsSubclassOf(baseType)).ToList();
    }
}
#endif