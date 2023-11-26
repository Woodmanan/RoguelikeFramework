using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Diagnostics;

[CustomEditor(typeof(RexRoom))]
public class RexRoomEditor : Editor
{
    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();

        DrawDefaultInspector();

        SerializedProperty RexRoomProp = serializedObject.FindProperty("RexFile");

        RexRoom rexTarget = target as RexRoom;

        string objName = rexTarget?.name;
        if (objName == null) return;

        if (RexRoomProp.objectReferenceValue == null)
        {
            if (GUILayout.Button("Create matching RexPaint file"))
            {
                string name = "REXPaint.exe";
                string path = GetPathTo(name);
                if (path.Equals("No File Found!"))
                {
                    UnityEngine.Debug.LogError("You must have RexPaint installed! Try running the 'Launch RexPaint' command to auto-install it.");
                }
                else
                {
                    string ImageFolderPath = GetPathToFolder("images", "Assets/RexPaint");

                    string branchName = AssetDatabase.GetAssetPath(target);
                    string[] folders = branchName.Split(new[] { '/' });
                    if (folders[folders.Length - 2].Equals("Rooms"))
                    {
                        branchName = folders[folders.Length - 3];
                    }
                    else
                    {
                        branchName = folders[folders.Length - 2];
                    }

                    string DefaultPath = ImageFolderPath + "\\default.xp";

                    //Create branch folder, if it's not there
                    ImageFolderPath += $"\\{branchName}";
                    System.IO.Directory.CreateDirectory(ImageFolderPath);
                    ImageFolderPath += $"\\{objName}.xp";

                    //Copy default file into new XP file
                    File.Copy(DefaultPath, ImageFolderPath);
                    AssetDatabase.Refresh();

                    //Load new object into reference
                    Undo.RecordObject(target, "Set RexAsset");
                    rexTarget.RexFile = AssetDatabase.LoadAssetAtPath<TextAsset>(ImageFolderPath);

                    //Flush everything under the sun
                    EditorUtility.SetDirty(target);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
            }
        }
        else if (GUILayout.Button("Open RexPaint file"))
        {

        }
    }

    public static Texture2D CachedTexture;
    public static string PathToImages;

    public override Texture2D RenderStaticPreview(string assetPath, Object[] subAssets, int width, int height)
    {
#if UNITY_EDITOR
        if (!EditorPrefs.GetBool("RexPreviewEnabled", false))
        {
            return null;
        }

        RexRoom room = AssetDatabase.LoadAssetAtPath<RexRoom>(assetPath);
        if (!room || !room.RexFile) return null;

        string rexPath = AssetDatabase.GetAssetPath(room.RexFile);
        string assetName = rexPath.Substring(rexPath.LastIndexOf('/') + 1);
        assetName = assetName.Substring(0, assetName.LastIndexOf('.')) + ".png";

        if (true || PathToImages == null)
        {
            string exePath = GetPathTo("REXPaint.exe");
            PathToImages = exePath.Substring(0, exePath.LastIndexOf('\\') + 1) + "images";
            PathToImages = PathToImages.Substring(exePath.LastIndexOf("Assets"));
        }

        assetName = PathToImages + '\\' + assetName;

        Texture2D rexPNG = AssetDatabase.LoadAssetAtPath<Texture2D>(assetName);

        if (rexPNG == null)
        {
            LoadRexpaintImages();
            rexPNG = AssetDatabase.LoadAssetAtPath<Texture2D>(assetName);
            if (rexPNG == null)
            {
                return null;
            }
        }

        if (!rexPNG.isReadable)
        {
            TextureImporter texImport = (TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(rexPNG));
            if (texImport)
            {
                texImport.isReadable = true;
                EditorUtility.SetDirty(texImport);
                texImport.SaveAndReimport();
            }

            rexPNG = AssetDatabase.LoadAssetAtPath<Texture2D>(assetName);
        }

        Texture2D toReturn = new Texture2D(width, height);

        float xStep = 1.0f / width;
        float xOffset = xStep / 2;
        float yStep = 1.0f / height;
        float yOffset = yStep / 2;

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                toReturn.SetPixel(i, j, rexPNG.GetPixelBilinear(xOffset + xStep * i, yOffset + yStep * j));
            }
        }

        toReturn.Apply();

        CachedTexture = toReturn;


        return CachedTexture;
#else
return null;
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

    static void LoadRexpaintImages()
    {
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
}