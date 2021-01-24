using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class CreateCustomTemplates
{
    private const string pathToYourScriptTemplate = "Assets/Scripts/CustomEditor/ScriptTemplates/EffectTemplate.cs.txt";

    [MenuItem(itemName: "Assets/Create/Create New Effect Script", isValidateFunction: false, priority: 51)]
    public static void CreateScriptFromTemplate()
    {
        ProjectWindowUtil.CreateScriptAssetFromTemplateFile(pathToYourScriptTemplate, "NewEffect.cs");
    }

}
