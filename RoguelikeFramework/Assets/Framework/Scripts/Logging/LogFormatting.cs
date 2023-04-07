using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class LogFormatting : MonoBehaviour
{ 
    public static string FormatNameForMonster(Monster monster)
    {
        if (monster)
        {
            if (monster.named)
            {
                return monster.localName.GetLocalizedString();
            }
            else
            {
                return LocalizationSettings.StringDatabase.GetLocalizedString("GenericNameFormat", arguments: monster.localName.GetLocalizedString());
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