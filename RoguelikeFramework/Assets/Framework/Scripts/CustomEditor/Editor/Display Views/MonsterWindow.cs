using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

public class MonsterWindow : EditorWindow
{
    static Monster[] monsters;
    static Editor[] editors;
    static string[] enumNames;
    static int refreshCount = 0;
    static int refreshMovingVal = 200;
    static GUIStyle style;

    const int maxNumToShow = 20;

    //Background color options
    static float lowVal = 0;
    static float highVal = 100;
    static Gradient grad;

    //Main UI background colors
    Color colorOne = new Color(.3f, .3f, .3f);
    Color colorTwo = new Color(.2f, .2f, .2f);

    // Add menu item named "My Window" to the Window menu
    [MenuItem("Tools/Moster Stats Window", priority = 0)]
    public static void ShowWindow()
    {
        Debug.Log("Showing window!");
        //Show existing window instance. If one doesn't exist, make one.
        MonsterWindow window = (MonsterWindow) GetWindow(typeof(MonsterWindow), false, "Monster stats", true);
        window.CacheMonsterData();
    }

    Vector2 scrollPos = Vector2.zero;

    void OnGUI()
    {
        //Confirm that data is okay
        CheckCachedData();

        Rect rect = position;

        float boxHeight = EditorGUIUtility.singleLineHeight;
        float gap = EditorGUIUtility.standardVerticalSpacing;
        float boxWidth = position.width * 1f;

        //Clever fix - if there are too many objects, just show more
        if ((position.height / boxHeight) > maxNumToShow)
        {
            boxHeight = position.height / maxNumToShow;
        }

        int count = monsters.Length;

        Rect scrollRect = position;
        scrollRect.y += (boxHeight + gap);
        scrollRect.height -= (boxHeight + gap);

        float oldScroll = scrollPos.y;

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, true, true, GUILayout.MinHeight(scrollRect.height), GUILayout.ExpandHeight(true));

        //Generate title rect early
        Rect titleRect = GUILayoutUtility.GetRect(boxWidth, boxHeight);

        { // Draw the main content bars
            int startingPoint = (int)(scrollPos.y / (boxHeight + gap));
            int endingPoint = (int)((scrollPos.y + scrollRect.height) / (boxHeight + gap)) + 1;
            endingPoint = (Mathf.Min(endingPoint, count));

            GUILayoutUtility.GetRect(boxWidth, count * (boxHeight + gap));

            for (int i = startingPoint; i < endingPoint; i++)
            {
                rect = new Rect(0, (i + 1) * (boxHeight + gap), boxWidth, boxHeight);
                if ((rect.y + rect.height) > scrollPos.y && (rect.y < scrollPos.y + scrollRect.height))
                {
                    EditorGUI.DrawRect(rect, (i % 2 == 1) ? colorOne : colorTwo);
                    DrawBox(rect, i);
                }
            }
        }

        { // Draw the title bar
            titleRect.y = oldScroll;
            EditorGUI.DrawRect(titleRect, colorOne);
            int enumCount = enumNames.Length + 1;

            titleRect.width = titleRect.width / enumCount;
            EditorGUI.LabelField(titleRect, new GUIContent("Name"));

            bool other = true;

            foreach (string name in enumNames)
            {
                titleRect.x += titleRect.width;
                EditorGUI.DrawRect(titleRect, other ? colorTwo : colorOne);
                EditorGUI.LabelField(titleRect, new GUIContent(name));
                other = !other;
            }
        }

        EditorGUILayout.EndScrollView();
    }



    void CheckCachedData()
    {
        if (monsters == null || monsters.Length == 0 || refreshCount >= refreshMovingVal)
        {
            CacheMonsterData();
            refreshCount = 0;
            enumNames = System.Enum.GetNames(typeof(Resources));
            BuildStyle();
        }
        refreshCount++;
    }

    void BuildStyle()
    {
        {//Construct style
            style = new GUIStyle();
            style.alignment = TextAnchor.MiddleCenter;
            style.normal.textColor = Color.black;
            style.clipping = TextClipping.Clip;
        }

        {//Construct Gradient
            grad = new Gradient();
            GradientColorKey[] keys = new GradientColorKey[4];
            Color col;
            ColorUtility.TryParseHtmlString("#E06C75", out col);
            keys[0].color = col;
            keys[0].time = .33f;

            ColorUtility.TryParseHtmlString("#E5C07B", out col);
            keys[1].color = col;
            keys[1].time = .66f;

            ColorUtility.TryParseHtmlString("#98C379", out col);
            keys[2].color = col;
            keys[2].time = 1.0f;

            GradientAlphaKey[] alpha = new GradientAlphaKey[2];
            alpha[0].alpha = 1.0f;
            alpha[0].time = 0.0f;
            alpha[1].alpha = 1.0f;
            alpha[1].time = 1.0f;

            grad.SetKeys(keys, alpha);
            grad.mode = GradientMode.Fixed;
        }
    }

    void CacheMonsterData()
    {
        DirectoryInfo files = new DirectoryInfo("Assets");

        List<Monster> tempMonsters = new List<Monster>();

        foreach (FileInfo f in files.GetFiles("*.prefab", SearchOption.AllDirectories))
        {
            string filePath = f.FullName;
            int length = filePath.Length - files.FullName.Length;
            filePath = filePath.Substring(f.FullName.Length - length, length);
            Monster monster = AssetDatabase.LoadAssetAtPath<Monster>("Assets" + filePath);
            if (monster != null)
            {
                tempMonsters.Add(monster);
            }
        }
        //Floating refresh
        {
            if (monsters != null && monsters.Length == tempMonsters.Count)
            {
                //We cached but it was wrong, do that less
                refreshMovingVal += 100;
            }
            else
            {
                refreshMovingVal = 200;
            }
        }

        monsters = tempMonsters.OrderBy(x => x.name).ToArray();
        editors = new Editor[monsters.Length];
    }

    void DrawBox(Rect rect, int index)
    {
        rect.width = rect.width / (enumNames.Length + 1);

        Editor.CreateCachedEditor(monsters[index], null, ref editors[index]);
        if (editors[index] == null)
        {
            Debug.LogError($"Editor {index} was null after instantiation!");
            return;
        }
        Editor edit = editors[index];

        //VERY IMPORTANT - GET PREVIOUS UPDATES
        edit.serializedObject.Update();

        {//Name
            SerializedProperty name = edit.serializedObject.FindProperty("friendlyName");
            EditorGUI.PropertyField(rect, name, new GUIContent());
        }

        {//Stats
            SerializedProperty prop = edit.serializedObject.FindProperty("baseStats");

            SerializedProperty keys = prop.FindPropertyRelative("_keys");
            SerializedProperty vals = prop.FindPropertyRelative("_vals");


            int showInd = 0;
            int showMax = enumNames.Length;
            int listInd = 0;
            int Listmax = keys.arraySize;


            for (; showInd < showMax; showInd++)
            {
                rect.x += rect.width;
                if (listInd < Listmax && keys.GetArrayElementAtIndex(listInd).enumValueIndex == showInd)
                {
                    SerializedProperty val = vals.GetArrayElementAtIndex(listInd); // - pull it out to delay, so that it can't delete early
                    style.normal.textColor = grad.Evaluate(Mathf.Clamp(val.floatValue, lowVal, highVal) / highVal);
                    val.floatValue = EditorGUI.DelayedFloatField(rect, val.floatValue, style);
                    listInd++;
                    continue;
                }
                else
                {
                    style.normal.textColor = Color.black;
                    float newVal = EditorGUI.DelayedFloatField(rect, 0, style);
                    if (newVal != 0)
                    {
                        keys.InsertArrayElementAtIndex(keys.arraySize);
                        vals.InsertArrayElementAtIndex(vals.arraySize);
                        keys.GetArrayElementAtIndex(keys.arraySize - 1).enumValueIndex = showInd;
                        vals.GetArrayElementAtIndex(vals.arraySize - 1).floatValue = newVal;
                    }
                }
            }

        }

        { //Clean up afterwards
            edit.serializedObject.ApplyModifiedProperties();
        }
    }
}