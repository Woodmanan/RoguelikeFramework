using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor;

[CustomPropertyDrawer(typeof(ResourceEffectKey))]
public class ResourceEffectKeyPropertyDrawer : PropertyDrawer
{
    public static string[] names;
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (names == null || names.Length == 0)
        {
            EffectResourceTable table = UnityEngine.Resources.Load<EffectResourceTable>("Standard Effects");
            if (table != null)
            {
                names = table.GetNames().Append("Clear").ToArray();
            }
            else
            {
                return;
            }
        }

        int currentIndex = -1;
        SerializedProperty currentName = property.FindPropertyRelative("name");
        if (currentName.stringValue.Length != 0)
        {
            currentIndex = System.Array.BinarySearch(names, 0, names.Length, currentName.stringValue);
        }

        currentIndex = EditorGUI.Popup(position, currentIndex, names);
        if (currentIndex == names.Length - 1)
        {
            currentIndex = -1;
            currentName.stringValue = "";
            property.serializedObject.ApplyModifiedProperties();
        }

        if (currentIndex != -1)
        {
            currentName.stringValue = names[currentIndex];
            property.serializedObject.ApplyModifiedProperties();
        }
    }

}