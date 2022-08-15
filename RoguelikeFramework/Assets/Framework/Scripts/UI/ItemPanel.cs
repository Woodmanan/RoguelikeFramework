using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemPanel : MonoBehaviour
{
    InventoryScreen controller;
    [SerializeField] private TextMeshProUGUI textbox;
    [SerializeField] private Image image;
    private bool selected = false;
    [HideInInspector] public int index;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Setup(InventoryScreen control, int i)
    {
        controller = control;
        index = i;
    }

    public void Select()
    {
        selected = !selected;
        GenerateItemDescription();
    }

    public void Click()
    {
        controller.Click(index);
    }

    public void GenerateItemDescription()
    {
        ItemStack representing = controller.examinedInventory[index];
        textbox.text = $"{Conversions.IntToNumbering(representing.position)} {(selected ? "+" : "-")} {representing.GetName()}";
        SpriteRenderer render = representing.held[0].GetComponent<SpriteRenderer>();
        image.sprite = render.sprite;
        image.color = render.color;
    }
}
