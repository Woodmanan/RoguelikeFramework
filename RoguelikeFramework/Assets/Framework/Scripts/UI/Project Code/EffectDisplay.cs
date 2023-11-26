using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EffectDisplay : MonoBehaviour
{
    [SerializeField] Image image;
    [SerializeField] TextMeshProUGUI title;
    [SerializeField] TextMeshProUGUI description;

    public void SetDisplay(Effect toDisplay)
    {
        if (toDisplay.ShouldDisplay())
        {
            title.text = toDisplay.GetName();
            description.text = toDisplay.GetDescription();
            image.sprite = toDisplay.GetImage();
        }
        else
        {
            title.text = "No name!";
            description.text = "An effect that should not display tried to display here.";
        }
    }
}
