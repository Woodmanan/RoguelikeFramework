using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;

public class MonsterSortWizard
{
    [MenuItem("Tools/Dangerous/Sort Monsters")]
    static void SortItems()
    {
        string path = GetPathToFolder("Monsters");
        Debug.Log($"Path to folder is {path}");

        List<Monster> monsters = new List<Monster>();

        var info = new DirectoryInfo(path);

        foreach (FileInfo f in info.GetFiles("*.prefab", SearchOption.AllDirectories))
        {
            Debug.Log($"File {f.Name} being searched!");
            string filePath = f.FullName;
            int length = filePath.Length - info.FullName.Length + path.Length;
            filePath = filePath.Substring(f.FullName.Length - length, length);

            Debug.Log($"Loading {filePath} from that!");

            monsters.Add(AssetDatabase.LoadAssetAtPath<Monster>(filePath));
        }

        Debug.Log($"Search discovered {monsters.Count} items to order!");

        monsters.Sort((a, b) =>
        {
            int val = (a.minDepth.CompareTo(b.minDepth));
            if (val == 0)
            {
                val = (a.maxDepth.CompareTo(b.maxDepth));
                if (val == 0)
                {
                    val = a.friendlyName.CompareTo(b.friendlyName);
                }
            }
            return val;
        }
        );

        for (int i = 0; i < monsters.Count; i++)
        {
            EditorUtility.DisplayProgressBar($"Rebuilding data for {monsters.Count} prefabs", $"({i.ToString("00")}/{monsters.Count}) Rebuilding {monsters[i].friendlyName}", ((float)i) / monsters.Count);

            AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(monsters[i]), $"{i.ToString().PadLeft(3, '0')} {monsters[i].friendlyName}");
            Undo.RecordObject(monsters[i], "Set ID");
            monsters[i].ID = i;
            EditorUtility.SetDirty(monsters[i]);
            //items[i].gameObject.name = $"{i.ToString().PadLeft(0, '0')} {items[i].name}";
            //Debug.Log($"{i}{(i >= 10 ? "" : " ")}: {items[i].name}");
        }

        EditorUtility.ClearProgressBar();

        AssetDatabase.SaveAssets();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

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
