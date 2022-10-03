using Juce.ImplementationSelector.Data;
using Juce.ImplementationSelector.Extensions;
using Juce.ImplementationSelector.Layout;
using Juce.ImplementationSelector.Logic;
using Juce.ImplementationSelector;
using UnityEditor;
using UnityEngine;
using System;
using System.Linq;


[CustomPropertyDrawer(typeof(Machine))]
public class MachinePropertyDrawer : PropertyDrawer
{
    private readonly PropertyDrawerLayoutHelper layoutHelper = new PropertyDrawerLayoutHelper();

    private readonly EditorData editorData = new EditorData();

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        //SelectImplementationAttribute typeAttribute = (SelectImplementationAttribute)attribute;

        float height = layoutHelper.GetElementsHeight(1);

        bool isCollapsed = !property.isExpanded;

        if (isCollapsed)
        {
            return height;
        }

        return height + property.GetVisibleChildHeight();
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SelectImplementationAttribute typeAttribute = new SelectImplementationAttribute(typeof(Machine));
        

        TryCacheTypesLogic.Execute(editorData, typeAttribute);
        TryCacheMachineNamesLogic.Execute(editorData, typeAttribute);

        bool typeIndexFound = TryGetTypeIndexLogic.Execute(
            editorData,
            property,
            out int typeIndex
            );

        bool isUninitalized = !typeIndexFound && editorData.Types.Length > 0;

        if (isUninitalized)
        {
            typeIndex = GetDefaultTypeIndexLogic.Execute(editorData);

            InitializePropertyAtIndexLogic.Execute(
                editorData,
                property,
                typeIndex
                );
        }

        if (Event.current.type == EventType.Layout)
        {
            return;
        }

        layoutHelper.Init(position);

        bool shouldDrawChildren = (property.hasVisibleChildren && property.isExpanded) || typeAttribute.ForceExpanded;

        Rect popupRect = layoutHelper.NextVerticalRect();

        GUIContent finalLabel = GUIContent.none;

        if (typeAttribute.DisplayLabel)
        {
            finalLabel = label;
        }

        if (typeAttribute.ForceExpanded)
        {
            property.isExpanded = true;
        }
        else
        {
            property.isExpanded = EditorGUI.Foldout(popupRect, property.isExpanded, GUIContent.none);
        }

        int newTypeIndex = EditorGUI.Popup(
            popupRect,
            editorData.Types[typeIndex].Name,
            typeIndex,
            editorData.EffectNames
            );

        if (newTypeIndex != typeIndex)
        {
            InitializePropertyAtIndexLogic.Execute(
                editorData,
                property,
                newTypeIndex
                );
        }

        if (!shouldDrawChildren && !property.isExpanded)
        {
            return;
        }

        EditorGUI.indentLevel++;
        {
            property.ForeachVisibleChildren(DrawChildPropertyField);
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

public static class TryCacheMachineNamesLogic
{
    //Effect-only version
    public static void Execute(
        EditorData editorData,
        SelectImplementationAttribute typeAttribute
        )
    {
        if (editorData.EffectNames != null)
        {
            return;
        }

        Type baseType = typeAttribute.FieldType;

        string removeTailString = GetRemoveTailString(baseType);

        editorData.EffectNames = new string[editorData.Types.Length];

        for (int i = 0; i < editorData.Types.Length; ++i)
        {
            Type type = editorData.Types[i];

            //Find GroupAttribute
            string groupName = "Default";

            GroupAttribute group = (GroupAttribute)Attribute.GetCustomAttribute(type, typeof(GroupAttribute));
            if (group != null)
            {
                //Find the name, remove trailing slashes that shouldn't be there!
                groupName = group.groupName.Trim('/');
            }

            editorData.EffectNames[i] = $"{groupName}/{type.Name}";
        }
    }

    private static string GetTypeTooltip(Type type)
    {
        SelectImplementationTooltipAttribute tooltipAttribute = Attribute.GetCustomAttribute(
            type,
            typeof(SelectImplementationTooltipAttribute)
            ) as SelectImplementationTooltipAttribute;

        if (tooltipAttribute == null)
        {
            return string.Empty;
        }

        return tooltipAttribute.Tooltip;
    }

    private static string GetRemoveTailString(Type type)
    {
        SelectImplementationTrimDisplayNameAttribute trimDisplayNameAttribute
            = Attribute.GetCustomAttribute(
                type,
                typeof(SelectImplementationTrimDisplayNameAttribute)
                ) as SelectImplementationTrimDisplayNameAttribute;

        if (trimDisplayNameAttribute == null)
        {
            return string.Empty;
        }

        return trimDisplayNameAttribute.TrimDisplayNameValue;
    }

    private static bool TryGetCustomDisplayName(
        Type type,
        out string customName
        )
    {
        SelectImplementationCustomDisplayNameAttribute customDisplayNameAttribute
            = Attribute.GetCustomAttribute(
                type,
                typeof(SelectImplementationCustomDisplayNameAttribute)
                ) as SelectImplementationCustomDisplayNameAttribute;

        if (customDisplayNameAttribute == null)
        {
            customName = default;
            return false;
        }

        customName = customDisplayNameAttribute.CustomDisplayName;
        return true;
    }
}


