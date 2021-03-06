﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class CreateCustomTemplates
{
    private const string pathToEffectTemplate = "Assets/Scripts/CustomEditor/ScriptTemplates/EffectTemplate.cs.txt";
    private const string pathToPanelTemplate = "Assets/Scripts/CustomEditor/ScriptTemplates/UIPanelTemplate.cs.txt";

    [MenuItem(itemName: "Assets/Create/Script Templates/New Effect Script", isValidateFunction: false, priority: 51)]
    public static void CreateEffectFromTemplate()
    {
        ProjectWindowUtil.CreateScriptAssetFromTemplateFile(pathToEffectTemplate, "NewEffect.cs");
    }

    [MenuItem(itemName: "Assets/Create/Script Templates/New UIPanel Script", isValidateFunction: false, priority: 52)]
    public static void CreateUIPanelFromTemplate()
    {
        ProjectWindowUtil.CreateScriptAssetFromTemplateFile(pathToPanelTemplate, "NewUIPanel.cs");
    }

}
