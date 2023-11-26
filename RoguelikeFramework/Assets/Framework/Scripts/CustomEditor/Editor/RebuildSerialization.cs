using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;

public class RebuildSerializationWizard
{

    [MenuItem("Tools/Rebuild Serialization")]

    static void RebuildSerialization()
    {
        AssetDatabase.ForceReserializeAssets();
    }
}
