using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(QueryTerm))]
public class QueryPropertyDrawer : PropertyDrawer
{
    List<QuerySubject> usesSubjectMod = new List<QuerySubject>{
        QuerySubject.ENEMIES,
        QuerySubject.ALLIES_EXCLUSIVE,
        QuerySubject.ALLIES_INCLUSIVE
    };

    List<QuerySubjectModifier> usesSubjectCount = new List<QuerySubjectModifier>
    {
        QuerySubjectModifier.MORE_THAN,
        QuerySubjectModifier.LESS_THAN,
        QuerySubjectModifier.PERCENT_OF
    };

    List<QueryProperty> usesResource = new List<QueryProperty>
    {
        QueryProperty.RESOURCE
    };

    public float labelWidth = EditorGUIUtility.labelWidth / 2;
    
    public float subjectDistance = 110f;
    public float subjectModDistance = 100f;
    public float subjectCountDistance = 25f;

    public float haveDistance = 37f;
    public float propertyValDistance = 30f;
    public float resourceDistance = 80f;
    public float equalityDistance = 100f;

    public float valueDistance = 35f;
    public float valueModDistance = 100f;
    public float weightDistance = 35f;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        //Save the indent level, because we do this manually.
        float savedWidth = position.width;
        position = EditorGUI.IndentedRect(position);
        //position.width = savedWidth;

        int save = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        SerializedProperty subject = property.FindPropertyRelative("subject");
        SerializedProperty subjectMod = property.FindPropertyRelative("subjectMod");
        SerializedProperty subjectCount = property.FindPropertyRelative("subjectCount");

        float subjectBlockDist = subjectDistance + subjectModDistance + subjectCountDistance;

        float valueBlockDist = valueDistance + valueModDistance + weightDistance;

        float resourceBlockDist = position.width - subjectBlockDist - valueBlockDist;



        SerializedProperty propertyVal = property.FindPropertyRelative("property");
        SerializedProperty resource = property.FindPropertyRelative("resource");
        SerializedProperty equality = property.FindPropertyRelative("equality");

        SerializedProperty value = property.FindPropertyRelative("value");
        SerializedProperty valueMod = property.FindPropertyRelative("valueMod");

        SerializedProperty weight = property.FindPropertyRelative("weight");

        //Create the naming block -----------------------------------------------------
        float start = position.x;
        float totalWidth = subjectBlockDist;
        Rect rect = position;

        if (usesSubjectMod.Contains((QuerySubject)subject.enumValueIndex))
        {
            if (usesSubjectCount.Contains((QuerySubjectModifier)subjectMod.enumValueIndex))
            {
                //Uses both!
                totalWidth -= subjectCountDistance;
                rect.x = start + totalWidth;
                rect.width = subjectCountDistance;

                subjectCount.intValue = EditorGUI.IntField(rect, subjectCount.intValue);
            }

            totalWidth -= subjectModDistance;
            rect.x = start + totalWidth;
            rect.width = subjectModDistance;
            subjectMod.enumValueIndex = EditorGUI.Popup(rect, subjectMod.enumValueIndex, subjectMod.enumDisplayNames);

        }

        rect.x = start;
        rect.width = totalWidth;

        subject.enumValueIndex = EditorGUI.Popup(rect, subject.enumValueIndex, subject.enumDisplayNames);

        bool display = !(subject.enumValueIndex == (int)QuerySubject.ALWAYS);

        //Resource block ------------------------------------------------------
        start = position.x + subjectBlockDist;
        totalWidth = resourceBlockDist;

        rect.x = start + totalWidth - equalityDistance;
        rect.width = equalityDistance;
        totalWidth -= equalityDistance;

        if (display) equality.enumValueIndex = EditorGUI.Popup(rect, equality.enumValueIndex, equality.enumDisplayNames);

        if (usesResource.Contains((QueryProperty)propertyVal.enumValueIndex))
        {
            //Render the resource block!
            rect.x = start + totalWidth - resourceDistance;
            rect.width = resourceDistance;
            totalWidth -= resourceDistance;

            if (display) resource.enumValueIndex = EditorGUI.Popup(rect, resource.enumValueIndex, resource.enumDisplayNames);
        }

        rect.x = start + haveDistance;
        rect.width = totalWidth - haveDistance;

        if (display) propertyVal.enumValueIndex = EditorGUI.Popup(rect, propertyVal.enumValueIndex, propertyVal.enumDisplayNames);

        rect.x = start;
        rect.width = haveDistance;
        if (display) EditorGUI.LabelField(rect, new GUIContent(" have"));

        //Value block! --------------------------------------------------------
        start = position.x + subjectBlockDist + resourceBlockDist;
        totalWidth = valueBlockDist;

        rect.x = start;
        rect.width = valueDistance;

        value.floatValue = EditorGUI.FloatField(rect, value.floatValue);

        rect.x = start + rect.width;
        rect.width = valueModDistance;

        valueMod.enumValueIndex = EditorGUI.Popup(rect, valueMod.enumValueIndex, valueMod.enumDisplayNames);

        rect.x = rect.x + rect.width;
        rect.width = weightDistance;

        weight.intValue = EditorGUI.IntField(rect, weight.intValue);

        EditorGUI.indentLevel = save;
    }


    //Fantastic code modeled from Fydar's on the Unity Forums. Thanks for all the help!!
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float height = EditorGUIUtility.singleLineHeight;

        return height;
    }
}
