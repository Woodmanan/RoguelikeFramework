using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Diagnostics;

#if UNITY_EDITOR
using UnityEditor.Experimental.AssetImporters;
#endif


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
        #if UNITY_EDITOR_WIN
        path = GetPathTo(name);
        ProcessStartInfo info = new ProcessStartInfo(path);
        //info.WorkingDirectory = GetPathToFolder("Rex Files");
        info.WorkingDirectory = path.Remove(path.Length - name.Length);
        Process.Start(info);
        #endif

        #if UNITY_EDITOR_OSX
        UnityEngine.Debug.LogError("This currently does not work on mac. Want it to? Yell at Woody and maybe let me borrow your mac for an hour");
        return;
        path = GetPathTo(name);
        var command = "wine";
        var processInfo = new ProcessStartInfo()
        {
            FileName = command,
            //Arguments = "Rexpaint.exe",
            WorkingDirectory = path.Remove(path.Length - name.Length),
            UseShellExecute = false,
            RedirectStandardOutput = true,
            CreateNoWindow = false
        };
 
        Process process = Process.Start(processInfo);
        while (!process.StandardOutput.EndOfStream)
        {
            UnityEngine.Debug.Log(process.StandardOutput.ReadLine());
        }
        #endif

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

#if UNITY_EDITOR
/*
 * What the hell is this doing? Good question, I'm not entirely sure myself.
 * 
 * So, I want to use rexpaint files for prefab objects, because they seem to make a lot of sense
 * with the design requirements. They're basically text files with layers, which is exactly
 * what we need for our project. Now the queston is, how to we get an .xp file into Unity?
 * 
 * You would think you could just include it. Turns out you can't. Unity has an internal asset
 * pipeline that is uses, and it just happens to be extremely good for most objects that you
 * would ever want in your project. If your object isn't one of these, it just doesn't get included
 * in any form, and won't show up in the build. We fix this by writing a custom ScriptedImporter!
 * 
 * This is the magic bit that's going to turn our asset into a usable asset, preferably in the 
 * form of a TextAsset. To do this, we need to get it into a string, because for some reason that's
 * the only thing TextAssets take in. Gross. To get it into a string, we can convert from bytes. However,
 * only memStreams nicely convert to bytes, so we need one of those. The original asset, though, is a 
 * regular file, so we need a filestream first. 
 * 
 * So, the final order of importing becomes:
 * Asset context -> path -> filestream -> memstream
 * memstream -> bytes -> binary encoding as string
 * string -> TextAsset
 */

[ScriptedImporter(1, "xp")]
public class RexpaintImporter : ScriptedImporter
{
    public override void OnImportAsset(AssetImportContext ctx)
    {
        MemoryStream memStream = new MemoryStream();
        FileStream stream = new FileStream(ctx.assetPath, FileMode.Open);
        stream.CopyTo(memStream);

        var xp = new TextAsset(System.Convert.ToBase64String(memStream.ToArray()));

        ctx.AddObjectToAsset("main", xp);
        ctx.SetMainObject(xp);
    }
}
#endif
