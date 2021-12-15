using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(ChanceEffect))]
public class ChanceEffectPropertyDrawer : PropertyDrawer
{
    const float countSize = 60f;
    const float dSize = 30f;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        //position.width = 300;//position.width - EditorGUI.indentLevel * 10;
        Rect nameRect = new Rect(position.x, position.y, position.width - countSize, EditorGUIUtility.singleLineHeight);
        Rect chanceRect = new Rect(position.x + position.width - countSize, position.y, countSize, EditorGUIUtility.singleLineHeight);
        Rect dRect = new Rect(position.x + position.width - dSize, position.y, dSize, EditorGUIUtility.singleLineHeight);

        SerializedProperty name = property.FindPropertyRelative("name");
        if (name.stringValue.Length == 0)
        {
            name.stringValue = label.text;
        }

        property.isExpanded = EditorGUI.Foldout(nameRect, property.isExpanded, new GUIContent(name.stringValue));

        SerializedProperty chance = property.FindPropertyRelative("percentChance");

        chance.floatValue = EditorGUI.FloatField(chanceRect, chance.floatValue);
        chance.floatValue = Mathf.Clamp(chance.floatValue, 0, 100);

        EditorGUI.LabelField(dRect, "%");

        if (property.isExpanded)
        {
            Debug.Log($"Parent sees width as {position.width}");
            nameRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            nameRect.width = position.width;
            EditorGUI.PropertyField(nameRect, property.FindPropertyRelative("name"));
            nameRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            nameRect.x += 10;
            nameRect.width -= 10;
            EditorGUI.PropertyField(nameRect, property.FindPropertyRelative("appliedEffects"));
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float height = EditorGUIUtility.singleLineHeight;
        if (property.isExpanded)
        {
            //Name template
            height += EditorGUIUtility.standardVerticalSpacing;
            height += EditorGUIUtility.singleLineHeight;

            //Effects
            height += EditorGUIUtility.standardVerticalSpacing;
            height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("appliedEffects"));
        }
        return height;
    }

}
#endif