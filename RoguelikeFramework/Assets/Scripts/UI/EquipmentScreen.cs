using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EquipmentScreen : RogueUIPanel
{
    //Don't uncomment these! These are already declared in the base class,
    //and are listed here so you know they exist.

    //bool inFocus; - Tells you if this is the window that is currently focused. Not too much otherwise.

    //Other Variables
    Transform contentHolder;
    [SerializeField] public Transform holdingPanel;
    [SerializeField] public GameObject EquipmentSlotPrefab;
    //[SerializeField] public GameObject itemHeaderPrefab;
    [SerializeField] private TextMeshProUGUI title;


    public Equipment examinedEquipment;
    public ItemAction queuedAction;
    public ItemStack queuedItem;

    //public bool[] selected;
    public List<EquipmentSlot> displayed = new List<EquipmentSlot>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
           
    }

    /*
     * One of the more important functions here. When in focus, this will be called
     * every frame with the stored input from InputTracking
     */
    public override void HandleInput(PlayerAction action, string inputString)
    {
        switch (queuedAction)
        {
            case ItemAction.INSPECT:
                //Break down input into item types
                foreach (char c in inputString.Where(c => char.IsLetter(c)))
                {
                    int index = Conversions.NumberingToInt(c);
                    if (index < examinedEquipment.equipmentSlots.Count && index >= 0)
                    {
                        if (examinedEquipment.equipmentSlots[index].active)
                        {
                            //Inspect a held item
                            UIController.singleton.OpenItemInspect(examinedEquipment.monster.inventory, examinedEquipment.equipmentSlots[index].equipped.position);
                        }
                        else
                        {
                            //Pick up a new item!
                            UIController.singleton.OpenInventoryEquip(index);
                        }
                        break;
                    }
                }
                break;
            case ItemAction.EQUIP:
                //Break down input into item types
                foreach (char c in inputString.Where(c => char.IsLetter(c)))
                {
                    int index = Conversions.NumberingToInt(c);
                    if (index < examinedEquipment.equipmentSlots.Count && index >= 0)
                    {
                        EquipmentSlot currentSlot = examinedEquipment.equipmentSlots[index];
                        EquippableItem toEquip = queuedItem.held[0].GetComponent<EquippableItem>();

                        if (toEquip == null)
                        {
                            //Flip out
                            Debug.LogError("UNEQUIPABLE ITEM HAS BYPASSED CHECKS, THIS IS REALLY BAD", queuedItem.held[0]);

                            //Desperately try to abort
                            ExitAllWindows();
                            return;
                        }

                        if (currentSlot.type.Contains(toEquip.primarySlot))
                        {
                            //Good to go for equip!
                            if (currentSlot.active)
                            {
                                //Need to unequip something first
                                examinedEquipment.UnequipSlot(index);
                                if (currentSlot.active)
                                {
                                    //TODO: Console message, couldn't get rid of item
                                    Debug.Log($"Could not remove item at slot {index}");
                                    return;
                                }
                            }

                            examinedEquipment.Equip(queuedItem.position, index);
                            ExitAllWindows();
                            break;
                        }
                        else
                        {
                            //TODO: Console message that says you can't equip that here
                            Debug.LogError($"Can't equip {queuedItem.held[0].GetName()} to slot {currentSlot.slotName}");
                        }
                    }
                }
                break;
        }
    }

    public void Setup(Equipment equip, ItemAction queuedAction, ItemStack queuedItem)
    { 
        examinedEquipment = equip;
        this.queuedAction = queuedAction;
        this.queuedItem = queuedItem;
    }

    /* Called every time this panel is activated by the controller */
    public override void OnActivation()
    {
        //Clear old items
        for (int i = holdingPanel.childCount - 1; i >= 0; i--)
        {
            Destroy(holdingPanel.GetChild(i).gameObject);
        }

        //Set up new item displays
        displayed = examinedEquipment.equipmentSlots;
        switch (queuedAction)
        {
            case ItemAction.EQUIP:
                EquippableItem equip = queuedItem.held[0].GetComponent<EquippableItem>();
                displayed = displayed.FindAll(x => x.type.Contains(equip.primarySlot));
                if (displayed.Count == 0)
                {
                    //TODO: Console log that you apparently can't wear this
                    Debug.LogError($"Player can't equip, as they have no slots of type {equip.primarySlot}");
                    return;
                }
                else if (displayed.Count == 1)
                {
                    //You probably just want to equip this? We'll just go ahead and do it
                    examinedEquipment.Equip(queuedItem.position, displayed[0].position);
                    ExitAllWindows();
                    break;
                }
                break;
        }


        print($"How many items to display: {displayed.Count}");

        //Sort the list into item types
        //ItemType currentType = ItemType.NONE;

        for (int i = 0; i < displayed.Count; i++)
        {
            GameObject instance = Instantiate(EquipmentSlotPrefab);
            EquipSlotPanel current = instance.GetComponent<EquipSlotPanel>();
            EquipmentSlot slot = displayed[i];

            //Set up item readout
            current.Setup(this, slot.position);
            current.RebuildGraphics();
            instance.transform.parent = holdingPanel;
        }
    }
    
    /* Called every time this panel is deactived by the controller */
    public override void OnDeactivation()
    {

    }

    /* Called every time this panel is focused on. Use this to refresh values that might have changed */
    public override void OnFocus()
    {

    }

    /*
     * Called when this panel is no longer focused on (added something to the UI stack). I don't know 
     * what on earth this would ever get used for, but I'm leaving it just in case (Nethack design!)
     */
    public override void OnDefocus()
    {
        
    }
}
