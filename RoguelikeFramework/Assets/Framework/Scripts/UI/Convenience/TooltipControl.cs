using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//AMAZING tooltip control class created by Joey Perrino
//All credit goes to him for this boilerplate class

//Usage - attach to a ui panel. That panel will now smoothly follow the mouse
public class TooltipControl : MonoBehaviour
{
    private RectTransform rect;

    RectTransform background;
    [SerializeField]
    Vector2 offset;

    void Start()
    {
        rect = GetComponent<RectTransform>();
        background = transform.parent.GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {
        Resize();
    }

    public void Resize()
    {
        Vector2 mousePosition = Input.mousePosition;
        Vector2 rectSize = rect.sizeDelta;

        Vector2 finalPosition = mousePosition + offset;
        //finalPosition.x += rectSize.x / 2;
        //finalPosition.y -= rectSize.y / 2;

        if (mousePosition.x >= Screen.width - rectSize.x) finalPosition.x -= rectSize.x;
        if (mousePosition.y <= rectSize.y) finalPosition.y += rectSize.y;

        if (finalPosition.x + rect.rect.width > background.rect.width)
        {
            finalPosition.x = background.rect.width - rect.rect.width;
        }

        if (finalPosition.y > background.rect.height)
        {
            finalPosition.y = background.rect.height - 2*offset.y;
        }

        rect.position = finalPosition;
    }
}