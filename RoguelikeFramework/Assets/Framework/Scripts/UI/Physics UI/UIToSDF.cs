using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIToSDF : MonoBehaviour
{
    RectTransform rectTransform;
    RectTransform backgroundRect;
    Rect lastRect;

    // Start is called before the first frame update
    void Start()
    {
        rectTransform = transform as RectTransform;
        backgroundRect = BackgroundController.singleton.transform as RectTransform;
    }

    // Update is called once per frame
    void Update()
    {
        if (rectTransform.hasChanged)
        {
            BackgroundController.singleton.values.Remove(lastRect);
            lastRect = GetScreenRect();
            BackgroundController.singleton.values.Add(lastRect);
            BackgroundController.singleton.hasChanges = true;

        }
    }

    public Rect GetScreenRect()
    {
        Rect rect = rectTransform.rect;

        //Offset position for SDF
        rect.position = Camera.main.WorldToScreenPoint(rectTransform.position);

        //Scale everthing to screen space
        rect.position /= backgroundRect.rect.size.x * 2;
        rect.size /= backgroundRect.rect.size * 2;

        //Move to actual positions
        //rect.position += (Vector2.one / 2);

        return rect;
    }
}
