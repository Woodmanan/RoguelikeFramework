using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

public class ExamineDescription : MonoBehaviour, IDescribable
{
    public LocalizedString locName;
    public LocalizedString locDescription;
    public Sprite image;

    public string GetName(bool shorten = false)
    {
        return locName.GetLocalizedString(this);
    }

    public string GetDescription()
    {
        return locDescription.GetLocalizedString(this);
    }

    public Sprite GetImage()
    {
        return image;
    }
}
