using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//AMAZING tooltip control class created by Joey Perrino
//All credit goes to him for this boilerplate class

//Usage - attach to a ui panel. That panel will now smoothly follow the mouse
public class TooltipControl : MonoBehaviour
{
    private RectTransform rect;

    void Start()
    {
        rect = GetComponent<RectTransform>();
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

        Vector2 finalPosition = mousePosition;
        finalPosition.x += rectSize.x / 2;
        finalPosition.y -= rectSize.y / 2;

        if (mousePosition.x >= Screen.width - rectSize.x) finalPosition.x -= rectSize.x;
        if (mousePosition.y <= rectSize.y) finalPosition.y += rectSize.y;

        rect.position = finalPosition;
    }
}