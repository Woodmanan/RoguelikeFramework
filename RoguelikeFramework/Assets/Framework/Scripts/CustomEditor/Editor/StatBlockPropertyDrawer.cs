using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Juce.ImplementationSelector.Layout;
using System.Linq;

[CustomPropertyDrawer(typeof(Stats))]
public class StatBlockPropertyDrawer : PropertyDrawer
{
    private readonly PropertyDrawerLayoutHelper layoutHelper = new PropertyDrawerLayoutHelper();
    private static Dictionary<ResourceType, Resources[]> cachedResourceTypes;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        object[] attributes = fieldInfo.GetCustomAttributes(typeof(ResourceGroup), false);
        ResourceType resourceType = (attributes.Count() > 0) ? ((ResourceGroup)attributes[0]).resourceTypeValue : ResourceType.NONE;

        SerializedProperty keys = property.FindPropertyRelative("_keys");
        SerializedProperty vals = property.FindPropertyRelative("_vals");

        layoutHelper.Init(position);

        Rect rect = layoutHelper.NextVerticalRect();

        property.isExpanded = EditorGUI.Foldout(rect, property.isExpanded, GUIContent.none);

        EditorGUI.LabelField(rect, label);

        if (!property.isExpanded)
        {
            return;
        }

        int max = Mathf.Min(keys.arraySize, vals.arraySize);

        layoutHelper.Indent();

        Rect walkRect = layoutHelper.NextVerticalRect();
        
        //Display each value
        for (int i = 0; i < max; i++)
        {
            Rect rectOne = walkRect;
            rectOne.width = Mathf.Min(rectOne.width * .5f, 150);
            Rect rectTwo = walkRect;
            rectTwo.x += rectOne.width;
            rectTwo.width -= rectOne.width;

            SerializedProperty key = keys.GetArrayElementAtIndex(i);
            SerializedProperty val = vals.GetArrayElementAtIndex(i);

            int keyVal = key.enumValueIndex;

            EditorGUI.PropertyField(rectOne, key, new GUIContent());

            val.floatValue = EditorGUI.DelayedFloatField(rectTwo, val.floatValue);

            if (key.enumValueIndex != keyVal)
            {
                int numCopies = 0;
                //Key has been changed! Confirm that that was an okay thing to do.
                for (int j = 0; j < max; j++)
                {
                    if (keys.GetArrayElementAtIndex(j).enumValueIndex == key.enumValueIndex)
                    {
                        numCopies++;
                    }
                }
                if (numCopies > 1)
                {
                    //Reset!
                    key.enumValueIndex = keyVal;
                }
            }

            walkRect = layoutHelper.NextVerticalRect();
        }

        Rect buttonRectOne = walkRect;
        buttonRectOne.width = buttonRectOne.width / 2;
        Rect buttonRectTwo = walkRect;
        buttonRectTwo.x += buttonRectOne.width;
        buttonRectTwo.width -= buttonRectOne.width;

        Resources[] values = GetValuesForType(resourceType);
        //Check if we can cancel early
        if (keys.arraySize != values.Length)
        {
            {//Create the custom "Add" button with dropdown
                List<string> names = values.Select(x => x.ToString()).ToList();
                List<byte> pairs = values.Cast<byte>().ToList();

                for (int i = pairs.Count - 1; i >= 0; i--)
                {
                    bool remove = false;
                    for (int j = 0; j < keys.arraySize; j++)
                    {
                        if (keys.GetArrayElementAtIndex(j).intValue == pairs[i])
                        {
                            remove = true;
                        }
                    }
                    if (remove)
                    {
                        pairs.RemoveAt(i);
                        names.RemoveAt(i);
                    }
                }

                names.Add("Add");

                int index = EditorGUI.Popup(buttonRectOne, names.Count - 1, names.ToArray());

                if (index != names.Count - 1)
                {
                    keys.InsertArrayElementAtIndex(keys.arraySize);
                    keys.GetArrayElementAtIndex(keys.arraySize - 1).enumValueIndex = pairs[index];

                    vals.InsertArrayElementAtIndex(vals.arraySize);
                    vals.GetArrayElementAtIndex(vals.arraySize - 1).floatValue = 100f;
                    return;
                }
            }


            //Display the add all button
            if (GUI.Button(buttonRectTwo, new GUIContent("Add All")))
            {
                foreach (int i in values)
                {
                    bool canAdd = true;
                    for (int j = 0; j < keys.arraySize; j++)
                    {
                        if (keys.GetArrayElementAtIndex(j).intValue == i)
                        {
                            canAdd = false;
                        }
                    }
                    if (canAdd)
                    {
                        keys.InsertArrayElementAtIndex(keys.arraySize);
                        keys.GetArrayElementAtIndex(keys.arraySize - 1).enumValueIndex = i;

                        vals.InsertArrayElementAtIndex(vals.arraySize);
                        vals.GetArrayElementAtIndex(vals.arraySize - 1).floatValue = 100f;
                    }
                }
            }
        }


        //Clear out old values!
        for (int i = max - 1; i >= 0; i--)
        {
            if (vals.GetArrayElementAtIndex(i).floatValue == 0.0f)
            {
                keys.DeleteArrayElementAtIndex(i);
                vals.DeleteArrayElementAtIndex(i);
            }
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (property.isExpanded)
        {
            float height = EditorGUIUtility.singleLineHeight;
            float gap = EditorGUIUtility.standardVerticalSpacing;

            SerializedProperty keys = property.FindPropertyRelative("_keys");
            SerializedProperty vals = property.FindPropertyRelative("_vals");

            int max = Mathf.Min(keys.arraySize, vals.arraySize);

            System.Array values = System.Enum.GetValues(typeof(Resources));
            //Check if we can cancel early
            if (keys.arraySize != values.Length)
            {
                //Account for button height
                max++;
            }

            return height * (max + 1) + (gap * (max));
        }
        else
        {
            return EditorGUIUtility.singleLineHeight;
        }
    }

    public Resources[] GetValuesForType(ResourceType types)
    {
        if (cachedResourceTypes == null)
        {
            cachedResourceTypes = new Dictionary<ResourceType, Resources[]>();
        }

        if (cachedResourceTypes.ContainsKey(types))
        {
            return cachedResourceTypes[types];
        }

        //Find 
        var type = typeof(Resources);

        Resources[] values = System.Enum.GetValues(typeof(Resources)).Cast<Resources>().ToArray();
        bool[] addFlag = new bool[values.Length];
        bool adding = false;
        for (int i = 0; i < values.Length; i++)
        {
            var memInfo = type.GetMember(values[i].ToString());

            var attributes = memInfo[0].GetCustomAttributes(typeof(ResourceGroup), false);
            ResourceGroup groupValue = (attributes.Length > 0) ? (ResourceGroup)attributes[0] : null;

            if (groupValue != null)
            {
                adding = (groupValue.resourceTypeValue & types) > 0;
            }

            addFlag[i] = adding;
        }


        Resources[] toAdd = values.Zip(addFlag, (resource, add) => (resource, add))
                                  .Where(x => x.add)
                                  .Select(x => x.resource)
                                  .ToArray();
        cachedResourceTypes[types] = toAdd;
        return toAdd;
    }
}
