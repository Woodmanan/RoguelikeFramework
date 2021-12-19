using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EquipSlotPanel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameBox;
    EquipmentScreen controller;
    EquipmentSlot displaying;
    int index;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Setup(EquipmentScreen controller, int index)
    {
        this.controller = controller;
        this.index = index;
        displaying = controller.examinedEquipment[index];
    }

    public void RebuildGraphics()
    {
        string newName = $"{Conversions.IntToNumbering(index)} - {displaying.slotName}";
        if (displaying.active)
        {
            //TODO: Set up the picture here as well
            newName += $" - {displaying.equipped.held[0].GetNameClean()}";
        }
        nameBox.text = newName;
    }

    public void Click()
    {
        UIController.singleton.OpenInventoryEquip(index);
    }
}
