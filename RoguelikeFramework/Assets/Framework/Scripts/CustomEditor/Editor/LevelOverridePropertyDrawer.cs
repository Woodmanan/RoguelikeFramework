using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Juce.ImplementationSelector.Layout;

[CustomPropertyDrawer(typeof(LevelOverride))]
public class LevelOverridePropertyDrawer : PropertyDrawer
{
    private readonly PropertyDrawerLayoutHelper layoutHelper = new PropertyDrawerLayoutHelper();

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        layoutHelper.Init(position);

        Rect rect = layoutHelper.NextVerticalRect();
        property.isExpanded = EditorGUI.Foldout(rect, property.isExpanded, GUIContent.none);

        SerializedProperty name = property.FindPropertyRelative("name");
        SerializedProperty level = property.FindPropertyRelative("level");

        if (property.isExpanded)
        {
            rect.width = rect.width / 2;
            EditorGUI.PropertyField(rect, name, new GUIContent());
            rect.x += rect.width;
            EditorGUI.PropertyField(rect, level, new GUIContent());
        }
        else
        {
            EditorGUI.LabelField(rect, new GUIContent($"Level {level.intValue}: {name.stringValue}"));
        }

        if (property.isExpanded)
        {
            
            SerializedProperty type = property.FindPropertyRelative("type");
            rect = layoutHelper.NextVerticalRect();
            EditorGUI.PropertyField(rect, type, new GUIContent());
            layoutHelper.Indent();
            switch (type.enumValueIndex)
            {
                case (int)MachineOverrideType.Add:
                    rect = layoutHelper.NextVerticalRect();
                    EditorGUI.PropertyField(rect, property.FindPropertyRelative("machines"), true);
                    break;
                case (int)MachineOverrideType.Delete:
                    rect = layoutHelper.NextVerticalRect();
                    SerializedProperty delete = property.FindPropertyRelative("deleteIndex");
                    SerializedProperty branchMachines = property.serializedObject.FindProperty("machines");
                    if (branchMachines != null)
                    {
                        string[] names = new string[branchMachines.arraySize];
                        for (int i = 0; i < branchMachines.arraySize; i++)
                        {
                            SerializedProperty machine = branchMachines.GetArrayElementAtIndex(i);
                            names[i] = $"{i}: {machine.managedReferenceFullTypename.Remove(0, 16)}";
                        }
                        delete.intValue = Mathf.Clamp(delete.intValue, 0, branchMachines.arraySize);

                        delete.intValue = EditorGUI.Popup(rect, delete.intValue, names);
                    }
                    else
                    {
                        delete.intValue = EditorGUI.IntField(rect, delete.intValue);
                    }
                    break;
                case (int)MachineOverrideType.Resize:
                    rect = layoutHelper.NextVerticalRect();
                    EditorGUI.PropertyField(rect, property.FindPropertyRelative("resize"), new GUIContent());
                    break;
                case (int)MachineOverrideType.Replace:
                    rect = layoutHelper.NextVerticalRect();
                    EditorGUI.PropertyField(rect, property.FindPropertyRelative("resize"), new GUIContent());
                    rect = layoutHelper.NextVerticalRect();
                    EditorGUI.PropertyField(rect, property.FindPropertyRelative("machines"), true);
                    break;
            }

            layoutHelper.Unindent();
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (property.isExpanded)
        {
            float height = EditorGUIUtility.singleLineHeight;
            float gap = EditorGUIUtility.standardVerticalSpacing;

            SerializedProperty machines = property.FindPropertyRelative("machines");

            switch (property.FindPropertyRelative("type").enumValueIndex)
            {
                case (int)MachineOverrideType.Add:
                    return 2 * height + 2 * gap + EditorGUI.GetPropertyHeight(machines);
                case (int)MachineOverrideType.Delete:
                    return 3 * height + 2 * gap;
                case (int)MachineOverrideType.Resize:
                    return 3 * height + 2 * gap;
                case (int)MachineOverrideType.Replace:
                    return 3 * height + 3 * gap + EditorGUI.GetPropertyHeight(machines);
                default:
                    return height;
            }

        }
        else
        {
            return EditorGUIUtility.singleLineHeight;
        }
    }

    /*public string[] CacheNames(Branch branch)
    {
        return [];
    }*/
}
