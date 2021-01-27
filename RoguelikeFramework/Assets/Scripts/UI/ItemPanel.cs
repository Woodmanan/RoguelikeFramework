using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ItemPanel : MonoBehaviour
{
    InventoryScreen controller;
    [SerializeField] private TextMeshProUGUI textbox;
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

    public void GenerateItemDescription()
    {
        ItemStack representing = controller.examinedInventory[index];
        textbox.text = $"{Conversions.IntToNumbering(representing.position)} {(selected ? "+" : "-")} {representing.GetName()}";
    }
}
