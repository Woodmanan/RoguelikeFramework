using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(StatusEffectList))]
public class EffectListPropertyDrawer : PropertyDrawer
{
    float BTN_WIDTH = 2 * EditorGUIUtility.singleLineHeight;// + 3f;
    float BTN_GAP = 1f;

    const float countSize = 40f;
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
        int count = EditorGUI.IntField(countRect, list.arraySize);
        EditorGUI.indentLevel = save;

        if (property.isExpanded)
        {
            Rect sweepRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing,
                                      position.width, EditorGUIUtility.singleLineHeight);

            EditorGUI.indentLevel++;
            Debug.Log($"There are {list.arraySize} elements now!");
            for (int i = 0; i < list.arraySize; i++)
            {
                SerializedProperty prop = list.GetArrayElementAtIndex(i);
                float height = EditorGUI.GetPropertyHeight(prop);
                sweepRect.height = height;
                EditorGUI.PropertyField(EditorGUI.IndentedRect(sweepRect), prop);
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
        if (count > 200) return;
        while (count > list.arraySize)
        {
            //Add elements!
            list.arraySize++;
            SerializedProperty prop = list.GetArrayElementAtIndex(list.arraySize - 1);
            SerializedProperty effect = prop.FindPropertyRelative("heldEffect");
            effect.objectReferenceValue = null;
        }
        while (count < list.arraySize)
        {
            //Delete elements!
            bool foundEmpty = false;

            //First, search for empty elements to get rid of? This is convenient, and useful to boot!
            for (int i = list.arraySize - 1; i >= 0; i--)
            {
                if (list.GetArrayElementAtIndex(i).FindPropertyRelative("heldEffect").objectReferenceValue == null)
                {
                    //Found an empty! Remove it, and continue.
                    if (i == list.arraySize - 1)
                    {
                        list.arraySize--;
                    }
                    else
                    {
                        list.DeleteArrayElementAtIndex(i);
                    }
                    foundEmpty = true;
                    break;
                }
            }
            if (foundEmpty) continue;

            //Got here, so we know that all elements are filled, but we still need to remove one.
            SerializedProperty prop = list.GetArrayElementAtIndex(list.arraySize - 1);
            SerializedProperty effect = prop.FindPropertyRelative("heldEffect");

            string path = AssetDatabase.GetAssetPath(effect.objectReferenceValue);
            effect.objectReferenceValue = null;
            AssetDatabase.DeleteAsset(path);

            list.arraySize--;
        }
    }
}

#endif
