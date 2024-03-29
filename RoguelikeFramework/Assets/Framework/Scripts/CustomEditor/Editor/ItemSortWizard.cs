﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;

public class ItemSortWizard
{
    [MenuItem("Tools/Dangerous/Sort Items")]
    static void SortItems()
    {
        Selection.activeGameObject = null;
        string path = GetPathToFolder("Items");
        Debug.Log($"Path to folder is {path}");

        List<Item> items = new List<Item>();

        var info = new DirectoryInfo(path);

        foreach (FileInfo f in info.GetFiles("*.prefab"))
        {
            //Debug.Log($"File {f.Name} being searched!");
            string filePath = f.FullName;
            int length = filePath.Length - info.FullName.Length + path.Length;
            filePath = filePath.Substring(f.FullName.Length - length, length);

            //Debug.Log($"Loading {filePath} from that!");

            items.Add(AssetDatabase.LoadAssetAtPath<Item>(filePath));
        }

        Debug.Log($"Search discovered {items.Count} items to order!");

        items.Sort((a, b) =>
            {
                int val = (a.type.CompareTo(b.type));
                if (val == 0)
                {
                    val = (a.rarity.CompareTo(b.rarity));
                    if (val == 0)
                    {
                        val = a.friendlyName.CompareTo(b.friendlyName);
                    }
                }
                return val;
            }
        );

        for (int i = 0; i < items.Count; i++)
        {
            EditorUtility.DisplayProgressBar($"Rebuilding data for {items.Count} items", $"({i.ToString("00")}/{items.Count}) Rebuilding {items[i].friendlyName}", ((float)i) / items.Count);

            int ID = i + 1; //Make ID's start at 1, easier for modding

            AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(items[i]), $"{ID.ToString().PadLeft(3, '0')} {items[i].friendlyName}");

            Undo.RecordObject(items[i], "Set ID");
            items[i].ID = ID;

            EditorUtility.SetDirty(items[i]);
        }

        EditorUtility.ClearProgressBar();

        AssetDatabase.SaveAssets();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

        AssetDatabase.Refresh();

        if (items[items.Count - 1].ID != items.Count - 1)
        {
            Debug.LogError("You can't be looking at the folder when this happens! I don't know why!");
        }

        Debug.Log("Done!");
    }

    static string GetPathToFolder(string folder)
    {
        string path = "Assets/Prefabs and Script Objects";
        var info = new DirectoryInfo(path);

        DirectoryInfo[] directories = info.GetDirectories("*", SearchOption.AllDirectories);

        foreach (DirectoryInfo d in directories)
        {
            if (d.Name.Equals(folder))
            {
                string filePath = d.FullName;
                int length = filePath.Length - info.FullName.Length + path.Length;
                filePath = filePath.Substring(d.FullName.Length - length, length);
                return filePath;
            }
        }
        return "No File Found!";
    }
}
