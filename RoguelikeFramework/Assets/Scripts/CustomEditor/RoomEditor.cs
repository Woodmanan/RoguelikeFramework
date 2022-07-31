using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text;

[CustomEditor(typeof(Room))] //Intentionally left without multi-edit. Yell at Woody if this is bad.
public class RoomEditor : Editor
{
    MessageType info = MessageType.None;
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        SerializedProperty layout = serializedObject.FindProperty("layout");
        SerializedProperty vector = serializedObject.FindProperty("size");
        Vector2Int size = vector.vector2IntValue;
        EditorGUILayout.PropertyField(vector);

        GUIStyle style = new GUIStyle();
        style.font = (Font)UnityEngine.Resources.Load(@"Fonts\Ubuntu_Mono\UbuntuMono-R");

        layout.stringValue = EditorGUILayout.TextArea(layout.stringValue, style, GUILayout.Height(14 * size.y + 2*(size.y - 1)), GUILayout.Width(8*size.x));

        if (GUILayout.Button("Validate"))
        {
            info = MessageType.Info;
            string[] cuts = layout.stringValue.Split('\n');
            if (cuts.Length != size.y)
            {
                info = MessageType.Error;
            }
            else
            {
                for (int i = 0; i < size.y; i++)
                {
                    string x = cuts[i];
                    if (x.Length != size.x)
                    {
                        info = MessageType.Error;
                        break;
                    }
                }
            }
        }
        switch (info)
        {
            case MessageType.Info:
                EditorGUILayout.HelpBox("Valid Room!", info);
                break;
            case MessageType.Error:
                EditorGUILayout.HelpBox("Invalid Room! Please fix dimensions, or use the buttons below.", info);
                bool resize = false;
                if (GUILayout.Button("Set Size to Room"))
                {
                    string[] cuts = layout.stringValue.Split('\n');
                    int y = cuts.Length;
                    int x = 0;
                    for (int i = 0; i < cuts.Length; i++)
                    {
                        if (cuts[i].Length > x)
                        {
                            x = cuts[i].Length;
                        }
                    }
                    vector.vector2IntValue = new Vector2Int(x, y);
                    size = new Vector2Int(x,y);
                    resize = true;
                }
                if (GUILayout.Button("Fit Room to Size") || resize)
                {
                    string[] cuts = layout.stringValue.Split('\n');
                    StringBuilder sb = new StringBuilder("", ((size.x + 1) * size.y) - 1);
                    for (int j = 0; j < size.y; j++)
                    {
                        if (j >= cuts.Length)
                        {
                            //Add a row of 0's
                            for (int i = 0; i < size.x; i++)
                            {
                                sb.Append('0');
                            }
                            if (j != size.y - 1)
                            {                            
                                sb.Append('\n');
                            }
                        }
                        else
                        {
                            //Append to the end!
                            string curr = cuts[j];
                            if (curr.Length >= size.x)
                            {
                                sb.Append(curr.Substring(0, size.x));
                            }
                            else
                            {
                                int toAdd = (size.x - curr.Length);
                                sb.Append(curr);
                                for (int i = 0; i < toAdd; i++)
                                {
                                    sb.Append('0');
                                }
                            }
                            if (j != size.y - 1)
                            {                            
                                sb.Append('\n');
                            }
                        }
                    }

                    layout.stringValue = sb.ToString();
                    info = MessageType.Info;
                }
                break;
        }

        
    

        serializedObject.ApplyModifiedProperties();
    }
}
