using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Juce.ImplementationSelector.Layout;
using Juce.ImplementationSelector.Extensions;

[CustomPropertyDrawer(typeof(RogueHandle<Monster>))]
public class MonsterHandlePropertyDrawer : RogueHandlePropertyDrawer<Monster> { }

public class RogueHandlePropertyDrawer<T> : PropertyDrawer
{
    private readonly PropertyDrawerLayoutHelper layoutHelper = new PropertyDrawerLayoutHelper();

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (Application.isPlaying)
        {
            float height = layoutHelper.GetElementsHeight(1);

            bool isCollapsed = !property.isExpanded;

            if (isCollapsed)
            {
                return height;
            }

            SerializedProperty value = property.FindPropertyRelative("serialValue");

            if (value != null)
            {
                return height + value.GetVisibleChildHeight();
            }

            return layoutHelper.GetElementsHeight(2);
        }
        else
        {
            float height = layoutHelper.GetElementsHeight(1);

            bool isCollapsed = !property.isExpanded;

            if (isCollapsed)
            {
                return height;
            }

            return layoutHelper.GetElementsHeight(2);
        }
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        layoutHelper.Init(position);
        bool shouldDrawChildren = (property.isExpanded);

        Rect popupRect = layoutHelper.NextVerticalRect();

        property.isExpanded = EditorGUI.Foldout(popupRect, property.isExpanded, GUIContent.none);
        EditorGUI.LabelField(popupRect, label);

        if (!property.isExpanded)
        {
            return;
        }

        EditorGUI.indentLevel++;
        if (Application.isPlaying)
        {
            SerializedProperty value = property.FindPropertyRelative("serialValue");
            if (value != null)
            {
                value.ForeachVisibleChildren(DrawChildPropertyField);
            }    
            else
            {
                EditorGUI.LabelField(layoutHelper.NextVerticalRect(), new GUIContent("Invalid!"));
            }    
            
        }
        else
        {
            EditorGUI.LabelField(layoutHelper.NextVerticalRect(), new GUIContent("Runtime Only!"));
        }
        EditorGUI.indentLevel--;
    }

    private void DrawChildPropertyField(SerializedProperty childProperty)
    {
        EditorGUI.PropertyField(
            layoutHelper.NextVerticalRect(childProperty),
            childProperty,
            includeChildren: true
            );
    }
}
