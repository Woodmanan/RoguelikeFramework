using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(ChanceEffectList))]
public class ChanceEffectListPropertyDrawer : PropertyDrawer
{
    float BTN_WIDTH = 2 * EditorGUIUtility.singleLineHeight;
    float BTN_GAP = 1f;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        Rect nameRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        Rect countRect = new Rect(position.x + position.width - (BTN_WIDTH), position.y, BTN_WIDTH, EditorGUIUtility.singleLineHeight);
        nameRect.width -= 1 * (BTN_WIDTH + BTN_GAP);


        SerializedProperty list = property.FindPropertyRelative("list");
        if (!list.isArray)
        {
            Debug.LogError("The 'list' property either did not exist, or was not an array. Did you change the name or type?");
            return;
        }

        if (list.arraySize == 0)
        {
            EditorGUI.LabelField(nameRect, label);
        }
        else
        {
            property.isExpanded = EditorGUI.Foldout(nameRect, property.isExpanded, label);
        }

        int save = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;
        int count = EditorGUI.DelayedIntField(countRect, list.arraySize);
        EditorGUI.indentLevel = save;

        if (property.isExpanded)
        {
            Rect sweepRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing,
                                      position.width, EditorGUIUtility.singleLineHeight);

            EditorGUI.indentLevel++;
            for (int i = 0; i < list.arraySize; i++)
            {
                SerializedProperty prop = list.GetArrayElementAtIndex(i);
                float height = EditorGUI.GetPropertyHeight(prop);
                sweepRect.height = height;
                EditorGUI.PropertyField(sweepRect, prop);
                sweepRect.y += height + EditorGUIUtility.standardVerticalSpacing;
            }
            EditorGUI.indentLevel--;
        }

        if (count != list.arraySize)
        {
            if (list.arraySize == 0 && count > 0)
            {
                property.isExpanded = true;
            }
            ResizeArray(list, count);
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        SerializedProperty list = property.FindPropertyRelative("list");

        float totalHeight = EditorGUIUtility.singleLineHeight;

        if (property.isExpanded && list.isArray) //Second check is paranoia!
        {
            for (int i = 0; i < list.arraySize; i++)
            {
                SerializedProperty prop = list.GetArrayElementAtIndex(i);
                totalHeight += EditorGUIUtility.standardVerticalSpacing;
                totalHeight += EditorGUI.GetPropertyHeight(prop);
            }
        }

        return totalHeight;
    }

    public void ResizeArray(SerializedProperty list, int count)
    {
        if (count < 0) return;
        if (count > 30) return;

        while (count > list.arraySize)
        {
            //Add elements!
            list.arraySize++;
            SerializedProperty prop = list.GetArrayElementAtIndex(list.arraySize - 1);
            SerializedProperty sublist = prop.FindPropertyRelative("appliedEffects").FindPropertyRelative("list");

            sublist.ClearArray();

            //Set the name nicely
            SerializedProperty name = prop.FindPropertyRelative("name");
            name.stringValue = $"New Effect {list.arraySize - 1}";
        }
        while (count < list.arraySize)
        {
            //Work backwards, clearing and deleting all elements of all lists
            for (int i = list.arraySize - 1; i >= count; i--)
            {
                SerializedProperty subList = list.GetArrayElementAtIndex(i).FindPropertyRelative("appliedEffects").FindPropertyRelative("list");
                Debug.Log($"Sublist exists with size {subList.arraySize}");
                for (int j = subList.arraySize - 1; j >= 0; j--)
                {
                    SerializedProperty effect = subList.GetArrayElementAtIndex(j).FindPropertyRelative("heldEffect");
                    if (effect.objectReferenceValue != null)
                    {
                        string path = AssetDatabase.GetAssetPath(effect.objectReferenceValue);
                        effect.objectReferenceValue = null;
                        AssetDatabase.DeleteAsset(path);
                    }
                    subList.arraySize--;
                }
                list.arraySize--;
            }
        }
    }
}
#endif
