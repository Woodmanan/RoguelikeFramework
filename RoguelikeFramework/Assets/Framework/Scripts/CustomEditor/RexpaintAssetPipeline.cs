using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Diagnostics;


public class RexpaintAssetPipeline
{
    #if UNITY_EDITOR
    //This ended up being unecessary! The regular files seem to work fine. If the clutter
    //becomes to much, come back and package them isntead!
    /*
    [MenuItem("Tools/Build AssetBundles")]
    static void BuildAllAssetBundles()
    {
        string assetBundleDirectory = "Assets/AssetBundles";
        if (!Directory.Exists(assetBundleDirectory))
        {
            Directory.CreateDirectory(assetBundleDirectory);
        }
        BuildPipeline.BuildAssetBundles(assetBundleDirectory,
                                        BuildAssetBundleOptions.None,
                                        BuildTarget.StandaloneWindows);
    }*/
    //TODO: Discover the magic of MacOS, and figure out how to launch Wine

    [MenuItem("Tools/Launch Rexpaint", priority = 5)]
    public static void LaunchRexpaint()
    {
        string name = "REXPaint.exe";
        string path = GetPathTo(name);
        if (path.Equals("No File Found!"))
        {
            UnityEngine.Debug.Log("No RexPaint install was found! Installing it for you now - this should be quick.");
            string folderPath = GetPathToFolder("RexPaint");
            if (folderPath.Equals("No Folder Found!"))
            {
                UnityEngine.Debug.LogError("No folder found! Aborting install, please create a 'RexPaint' folder. In fact, yell at Woody. That should be there.");
                return;
            }
            using (var client = new WebClient())
            {
                UnityEngine.Debug.Log("Starting download...");
                client.DownloadFile("https://www.gridsagegames.com/blogs/fileDownload.php?fileName=REXPaint-v1.60.zip", folderPath + "/REXPaint-v1.60.zip");

                UnityEngine.Debug.Log("Download complete! Installing...");

                ZipFile.ExtractToDirectory(folderPath + "/REXPaint-v1.60.zip", folderPath);

                UnityEngine.Debug.Log("Installation complete! Cleaning up unused files...");

                //Clean old file
                File.Delete(folderPath + "/REXPaint-v1.60.zip");
                UnityEngine.Debug.Log("You now have Rexpaint! Opening it up for you...");
                AssetDatabase.Refresh();
            }
        }
        path = GetPathTo(name);
        ProcessStartInfo info = new ProcessStartInfo(path);
        //info.WorkingDirectory = GetPathToFolder("Rex Files");
        info.WorkingDirectory = path.Remove(path.Length - name.Length);
        Process.Start(info);
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

    static string GetPathToFolder(string folder, string path = "Assets")
    {
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
        return "No Folder Found!";
    }
    #endif

    public static SadRex.Image Load(TextAsset asset)
    {
        MemoryStream stream = new MemoryStream(System.Convert.FromBase64String(asset.text));
        return SadRex.Image.Load(stream);
    }
}
