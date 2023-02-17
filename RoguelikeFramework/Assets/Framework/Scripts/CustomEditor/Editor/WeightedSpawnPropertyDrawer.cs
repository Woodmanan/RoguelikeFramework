using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/*
 * Custom property drawer for the weighted spawn struct that monster tables use
 */

[CustomPropertyDrawer(typeof(WeightedSpawn))]
public class WeightedSpawnPropertyDrawer : PropertyDrawer
{
    const float floatWidth = 80;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        float floatOffset = Mathf.Min(position.width / 2, floatWidth);
        position.width -= floatOffset;
        SerializedProperty monsterProp = property.FindPropertyRelative("toSpawn");
        SerializedProperty weightProp = property.FindPropertyRelative("weight");

        EditorGUI.ObjectField(position, monsterProp, new GUIContent(""));

        position.x += position.width;
        position.width = floatOffset;
        weightProp.intValue = EditorGUI.IntField(position, weightProp.intValue);
    }
}
