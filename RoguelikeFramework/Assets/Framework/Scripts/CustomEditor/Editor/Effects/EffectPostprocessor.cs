using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System;
using System.Linq;

public class EffectPostprocessor : AssetPostprocessor
{
	static void OnPostprocessAllAssets(
		 string[] importedAssets,
		 string[] deletedAssets,
		 string[] movedAssets,
		 string[] movedFromAssetPaths)
	{
		foreach (string str in importedAssets)
		{
			string[] splitStr = str.Split('/', '.');

			string folder = splitStr[splitStr.Length - 3];
			string fileName = splitStr[splitStr.Length - 2];
			string extension = splitStr[splitStr.Length - 1];

			if (extension.Equals("cs"))
            {
				Type fileType = AppDomain.CurrentDomain.GetAssemblies()
								.Reverse()
								.Select(assembly => assembly.GetType(fileName))
								.FirstOrDefault(t => t != null);
				if (fileType != null && fileType.BaseType == typeof(Effect))
                {
					Debug.Log($"Reimported an effect named {fileName}");
					Debug.Log($"It has name {fileType.Name}");
					Debug.Log($"It was in assembly {fileType.Assembly.FullName}");
					EditorPrefs.SetBool($"Rebuild_{fileName}", true);
                }
            }
		}
	}
}
