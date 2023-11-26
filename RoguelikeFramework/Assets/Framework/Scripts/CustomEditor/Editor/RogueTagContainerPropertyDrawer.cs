using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Juce.ImplementationSelector.Layout;
using System.Linq;

[CustomPropertyDrawer(typeof(RogueTagContainer))]
public class RogueTagContainerPropertyDrawer : PropertyDrawer
{
    private readonly PropertyDrawerLayoutHelper layoutHelper = new PropertyDrawerLayoutHelper();

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty keys = property.FindPropertyRelative("_keys");
        SerializedProperty vals = property.FindPropertyRelative("_vals");

        layoutHelper.Init(position);

        Rect rect = layoutHelper.NextVerticalRect();

        property.isExpanded = EditorGUI.Foldout(rect, property.isExpanded, GUIContent.none);

        EditorGUI.LabelField(rect, label);

        if (!property.isExpanded)
        {
            return;
        }

        SerializedProperty[] keyNames = new SerializedProperty[keys.arraySize];
        for (int i = 0; i < keys.arraySize; i++)
        {
            keyNames[i] = keys.GetArrayElementAtIndex(i).FindPropertyRelative("name");
        }

        int max = keys.arraySize;

        layoutHelper.Indent();

        Rect walkRect = layoutHelper.NextVerticalRect();
        
        //Display each value
        for (int i = 0; i < max; i++)
        {
            SerializedProperty key = keyNames[i];
            //SerializedProperty val = vals.GetArrayElementAtIndex(i);

            key.stringValue = EditorGUI.DelayedTextField(walkRect, key.stringValue);
            //EditorGUI.PropertyField(rectOne, key, new GUIContent());

            //val.floatValue = EditorGUI.DelayedFloatField(rectTwo, val.floatValue);

            walkRect = layoutHelper.NextVerticalRect();
        }

        Rect rectOne = walkRect;
        rectOne.width = Mathf.Min(rect.width / 2, 120);
        Rect rectTwo = walkRect;
        rectTwo.width = rectTwo.width - rectOne.width;
        rectTwo.x += rectOne.width;

        string stringBox = EditorGUI.DelayedTextField(rectTwo, "");

        if (stringBox != "")
        {
            keys.InsertArrayElementAtIndex(keys.arraySize);
            keys.GetArrayElementAtIndex(keys.arraySize - 1).FindPropertyRelative("name").stringValue = stringBox;


            vals.InsertArrayElementAtIndex(vals.arraySize);
            vals.GetArrayElementAtIndex(vals.arraySize - 1).intValue = 1;
        }

        GUI.Button(rectOne, new GUIContent("Add new string ->"));

        //Display the add all button
        /*if (GUI.Button(rectOne, new GUIContent("Add Tag")))
        {
            keys.InsertArrayElementAtIndex(keys.arraySize);
            keys.GetArrayElementAtIndex(keys.arraySize - 1).FindPropertyRelative("name").stringValue = "Tag.New";


            vals.InsertArrayElementAtIndex(vals.arraySize);
            vals.GetArrayElementAtIndex(vals.arraySize - 1).intValue = 1;
        }*/


        //Clear out old values!
        for (int i = max - 1; i >= 0; i--)
        {
            if (keyNames[i].stringValue.Equals(""))
            {
                keys.DeleteArrayElementAtIndex(i);
                vals.DeleteArrayElementAtIndex(i);
            }
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (property.isExpanded)
        {
            float height = EditorGUIUtility.singleLineHeight;
            float gap = EditorGUIUtility.standardVerticalSpacing;

            SerializedProperty keys = property.FindPropertyRelative("_keys");
            SerializedProperty vals = property.FindPropertyRelative("_vals");

            int max = Mathf.Min(keys.arraySize, vals.arraySize);

            System.Array values = System.Enum.GetValues(typeof(Resources));
            //Check if we can cancel early
            if (keys.arraySize != values.Length)
            {
                //Account for button height
                max++;
            }

            return height * (max + 1) + (gap * (max));
        }
        else
        {
            return EditorGUIUtility.singleLineHeight;
        }
    }
}
