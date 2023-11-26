using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class LogFormatting : MonoBehaviour
{ 
    public static string FormatNameForMonster(Monster monster, bool definite = true)
    {
        if (monster)
        {
            if (monster.named)
            {
                return monster.GetLocalizedName();
            }
            else
            {
                return LocalizationSettings.StringDatabase.GetLocalizedString((definite ? "GenericNameDefinite" : "GenericNameIndefinite"), arguments: monster.GetLocalizedName());
            }
        }
        return "";
    }

    public static string GetActionStringWithName(string key, bool singular)
    {
        return LocalizationSettings.StringDatabase.GetLocalizedString(key, arguments: new { singular = singular });
    }

    public static string GetFormattedString(string key, object args)
    {
        return LocalizationSettings.StringDatabase.GetLocalizedString(key, arguments: args);
    }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}