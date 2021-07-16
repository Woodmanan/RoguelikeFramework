using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HighlightBlock : MonoBehaviour
{
    private Image img;
    public Color color;
    Color alphaMask = new Color(1, 1, 1, 0);
    Color alphaAdd = new Color(0, 0, 0, .5f);

    // Start is called before the first frame update
    void Start()
    {

    }

    public void Setup()
    {
        img = GetComponent<Image>();
    }

    public void Show(Color c)
    {
        img.color = c * alphaMask + alphaAdd;
    }

    public void Show()
    {
        img.color = color * alphaMask + alphaAdd;
    }

    public void Hide()
    {
        img.color = color * alphaMask;
    }

    
}
