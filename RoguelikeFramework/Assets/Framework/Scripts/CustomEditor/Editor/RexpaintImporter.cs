using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Diagnostics;
using UnityEditor.Experimental.AssetImporters;

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