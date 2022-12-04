using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;


[Serializable]
public struct HeaderPair
{
    public ItemType type;
    public string name;
    public Sprite image;
    public Color color;
}

public class ItemHeader : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI textBox;
    [SerializeField] Image image;
    [SerializeField] private List<HeaderPair> pairs;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Setup(ItemType t)
    {
        HeaderPair pair = pairs.Find(x => (x.type & t) > 0);
        textBox.text = pair.name;
        image.sprite = pair.image;
        image.color = pair.color;
    }

}
