using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;

public class ItemSortWizard
{
    [MenuItem("Tools/Dangerous/Sort Items")]
    static void SortItems()
    {
        string path = GetPathToFolder("Items");
        Debug.Log($"Path to folder is {path}");

        List<Item> items = new List<Item>();

        var info = new DirectoryInfo(path);

        foreach (FileInfo f in info.GetFiles("*.prefab"))
        {
            Debug.Log($"File {f.Name} being searched!");
            string filePath = f.FullName;
            int length = filePath.Length - info.FullName.Length + path.Length;
            filePath = filePath.Substring(f.FullName.Length - length, length);

            Debug.Log($"Loading {filePath} from that!");

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
                        val = a.GetNameClean().CompareTo(b.GetNameClean());
                    }
                }
                return val;
            }
        );

        for (int i = 0; i < items.Count; i++)
        {
            items[i].ID = i;
            AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(items[i]), $"{i.ToString().PadLeft(3, '0')} {items[i].GetNameClean()}");
            //items[i].gameObject.name = $"{i.ToString().PadLeft(0, '0')} {items[i].name}";
            //Debug.Log($"{i}{(i >= 10 ? "" : " ")}: {items[i].name}");
        }

        AssetDatabase.Refresh();
    }

    static string GetPathToFolder(string folder)
    {
        string path = "Assets";
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
