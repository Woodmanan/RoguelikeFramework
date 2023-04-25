using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Diagnostics;

[InitializeOnLoad]
public class RexpaintWizard
{
    const int RefreshEveryX = 10;
    static RexpaintWizard()
    {
        if (!SessionState.GetBool("FirstInitDone", false))
        {
            SessionState.SetBool("FirstInitDone", true);

            int count = EditorPrefs.GetInt("RexpaintRefresh", 0);
            if (count == 0 || EditorPrefs.GetBool("HasRexpaintChanges", false))
            {
                if (EditorPrefs.GetBool("RexPreviewEnabled", false))
                {
                    RebuildRexRoomPreview();
                }
            }

            EditorPrefs.SetInt("RexpaintRefresh", (count + 1) % RefreshEveryX);
        }
    }

    [MenuItem("Tools/Rexpaint/Enable and Refresh Preview")]
    static void EnableAndRebuildPreview()
    {
        EditorPrefs.SetBool("RexPreviewEnabled", true);
        RebuildRexRoomPreview();
    }

    [MenuItem("Tools/Rexpaint/Disable Preview")]
    static void DisablePreview()
    {
        EditorPrefs.SetBool("RexPreviewEnabled", false);
    }


    static void RebuildRexRoomPreview()
    {
        UnityEngine.Debug.Log("Refreshing RexPaint cached images!");
        string name = "REXPaint.exe";
        string path = GetPathTo(name);
        if (path.Equals("No File Found!"))
        {
            UnityEngine.Debug.LogError("No RexPaint install was found! Try running the 'Launch Rexpaint' tool command.");
            return;
        }
        path = GetPathTo(name);
        ProcessStartInfo info = new ProcessStartInfo(path);
        //info.WorkingDirectory = GetPathToFolder("Rex Files");
        info.WorkingDirectory = path.Remove(path.Length - name.Length);
        info.Arguments = "-exportAll";
        Process.Start(info);

        AssetDatabase.Refresh();
    }

    static string GetPathTo(string filename, string path = "Assets")
    {
        var info = new DirectoryInfo(path);

        List<string> filesToModify = new List<string>();

        FileInfo[] files = info.GetFiles("*.exe", SearchOption.AllDirectories);

        foreach (FileInfo f in files)
        {
            if (filename.Equals(f.Name))
            {
                return f.FullName;
            }
        }
        return "No File Found!";
    }
}
