using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class CreateCustomTemplates
{
    [MenuItem(itemName: "Assets/Create/Script Templates/New Effect Script", isValidateFunction: false, priority: 51)]
    public static void CreateEffectFromTemplate()
    {
        ProjectWindowUtil.CreateScriptAssetFromTemplateFile(GetPathTo("EffectTemplate.cs.txt"), "NewEffect.cs");
    }

    [MenuItem(itemName: "Assets/Create/Script Templates/New UIPanel Script", isValidateFunction: false, priority: 53)]
    public static void CreateUIPanelFromTemplate()
    {
        ProjectWindowUtil.CreateScriptAssetFromTemplateFile(GetPathTo("UIPanelTemplate.cs.txt"), "NewUIPanel.cs");
    }

    [MenuItem(itemName: "Assets/Create/Script Templates/New GameAction Script", isValidateFunction: false, priority: 52)]
    public static void CreateGameActionFromTemplate()
    {
        ProjectWindowUtil.CreateScriptAssetFromTemplateFile(GetPathTo("ActionTemplate.cs.txt"), "NewGameAction.cs");
    }

    [MenuItem(itemName: "Assets/Create/Script Templates/New Ability Script", isValidateFunction: false, priority: 53)]
    public static void CreateAbilityFromTemplate()
    {
        ProjectWindowUtil.CreateScriptAssetFromTemplateFile(GetPathTo("AbilityTemplate.cs.txt"), "NewAbility.cs");
    }

    static string GetPathTo(string filename)
    {
        string path = "Assets/Scripts";
        var info = new DirectoryInfo(path);

        List<string> filesToModify = new List<string>();

        FileInfo[] files = info.GetFiles("*.*", SearchOption.AllDirectories);

        foreach (FileInfo f in files)
        {
            if (filename.Equals(f.Name))
            {
                string filePath = f.FullName;
                int length = filePath.Length - info.FullName.Length + path.Length;
                filePath = filePath.Substring(f.FullName.Length - length, length);
                return filePath;
            }
        }
        return "No File Found!";
    }

}
