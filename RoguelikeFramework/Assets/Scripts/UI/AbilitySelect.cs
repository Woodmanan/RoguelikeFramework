using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AbilitySelect : MonoBehaviour, IDescribable
{
    public int index;

    [HideInInspector]
    public ClassPanel panel;

    public Image mainImage;
    public Image highlight;

    public Color selected;
    public Color unselected;

    bool isSelected;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Setup(ClassPanel panel, int index)
    {
        this.index = index;
        this.panel = panel;

        mainImage.sprite = panel.current.abilities[index].image;
        highlight.color = selected;
        isSelected = true;
    }

    public void Click()
    {
        isSelected = !isSelected;
        highlight.color = (isSelected) ? selected : unselected;
        panel.Click(index, isSelected);
    }

    public string GetName(bool shorten = false)
    {
        return panel.current.abilities[index].GetName(shorten);
    }

    public string GetDescription()
    {
        return panel.current.abilities[index].GetDescription();
    }

    public Sprite GetImage()
    {
        return panel.current.abilities[index].GetImage();
    }
}
