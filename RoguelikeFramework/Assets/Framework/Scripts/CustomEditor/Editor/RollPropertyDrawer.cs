using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(DamagePairing))]
public class DamagePairingPropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        //Save the indent level, because we do this manually.
        int save = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        
        float typeSize = 90f;
        float gap = 2f;

        SerializedProperty roll = property.FindPropertyRelative("damage");

        SerializedProperty damageType = property.FindPropertyRelative("type");

        var rect = new Rect(position.x + position.width - typeSize, position.y, typeSize, EditorGUIUtility.singleLineHeight);
        var saveRect = rect;
        damageType.enumValueIndex = EditorGUI.Popup(rect, damageType.enumValueIndex, damageType.enumDisplayNames);

        float rollSize = position.width + gap - typeSize - EditorGUIUtility.labelWidth;

        rect.x -= (4 * gap + rollSize - 1);
        rect.width = rollSize;

        SerializedProperty dice = roll.FindPropertyRelative("dice");
        SerializedProperty rolls = roll.FindPropertyRelative("rolls");

        GUIContent[] list = { new GUIContent(" "), new GUIContent("d") };
        int[] nums = { rolls.intValue, dice.intValue };


        damageType.enumValueIndex = EditorGUI.Popup(saveRect, damageType.enumValueIndex, damageType.enumDisplayNames);
        EditorGUI.MultiIntField(rect, list, nums);


        rolls.intValue = nums[0];
        dice.intValue = nums[1];

        EditorGUI.indentLevel = save;

        rect.width = EditorGUIUtility.labelWidth;
        rect.x = position.x;

        EditorGUI.LabelField(rect, property.displayName);

    }

    //Fantastic code modeled from Fydar's on the Unity Forums. Thanks for all the help!!
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float height = EditorGUIUtility.singleLineHeight;

        return height;
    }
}
