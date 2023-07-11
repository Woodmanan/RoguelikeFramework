using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct StatueImage
{
    public TotemType totem;
    public Sprite sprite;
}

public class JungleStatueTile : RogueTile
{
    public List<StatueImage> images;

    public TotemType totem;

    public void SetSpriteForTotem(TotemType newTotem)
    {
        totem = newTotem;
        foreach (StatueImage image in images)
        {
            if (image.totem == totem)
            {
                GetComponent<SpriteRenderer>().sprite = image.sprite;
                return;
            }
        }

        Debug.LogError($"No totem image found for {totem}");
    }
}
