using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System;
using System.Linq;

public class EffectPostprocessor : AssetPostprocessor
{
	static List<string> names;

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
				if (names == null)
                {
					Debug.Log("Reimported all effect names!");
					Type effectType = typeof(Effect);

					names = AppDomain.CurrentDomain.GetAssemblies()
						.Select(x => x.GetTypes())
						.SelectMany(x => x)
						.Where(x => x.BaseType == effectType)
						.Select(x => x.Name)
						.ToList();
                }

				if (names.Contains(fileName))
                {
					Debug.Log($"Reimported an effect named {fileName}");
					EditorPrefs.SetBool($"Rebuild_{fileName}", true);
                }
            }
		}
	}
}
