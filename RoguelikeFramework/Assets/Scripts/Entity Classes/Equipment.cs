using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class EquipmentSlot
{
    public string slotName;
    public List<EquipSlotType> type; //Slots can be more than one type of slot
    public ItemStack equipped; //But can only ever hold one item
    public bool active;
    //Similar to inventory slots, but *theoretically* shouldn't change. Might change when mutation system is online.
    [HideInInspector] public int position;
}

public class Equipment : MonoBehaviour
{
    public bool CanUnequip = true;
    [HideInInspector] public Monster monster;
    private Inventory inventory;

    public List<EquipmentSlot> equipmentSlots;

    public EquipmentSlot this[int index]
    {
        get { return equipmentSlots[index]; }
    }

    // Start is called before the first frame update
    void Start()
    {
        monster = GetComponent<Monster>();
        inventory = GetComponent<Inventory>();
        this.enabled = false; //Shuts off expensive events

        //Set up positional stuff. This should never change, so there's a lot less logic for it.
        for (int i = 0; i < equipmentSlots.Count; i++)
        {
            equipmentSlots[i].position = i;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    //Monster of a function that equips a given item to this equipment holder
    public void Equip(int itemIndex, int EquipIndex)
    {
        //Setup vars
        List<int> neededSlots = new List<int>();

        //Get item
        ItemStack item = inventory[itemIndex];
        if (item == null)
        {
            Debug.LogError("Can't attach null item at {itemIndex}");
        }

        //Confirm that slot is open and primary
        EquippableItem equip = item.held[0].GetComponent<EquippableItem>();
        EquipSlotType primary = equip.primarySlot;

        EquipmentSlot main = equipmentSlots[EquipIndex];
        if (!main.type.Contains(primary))
        {
            //TODO: Console error!
            Debug.LogError("Item equipped to wrong type of primary slot!", equip);
            return;
        }

        if (main.active)
        {
            //TODO: Console error!
            //TODO: Consider if this makes sense, might be better to just unequip the other item.
            Debug.LogError("This slot is already filled!");
            return;
        }

        neededSlots.Add(EquipIndex);
        main.active = true;
        print($"Main is == to equipslots[{EquipIndex}]: {main == equipmentSlots[EquipIndex]}");


        //Confirm that secondary slots are available
        foreach (EquipSlotType t in equip.secondarySlots)
        {
            bool succeeded = false;
            //See if there is a free spot that is not already in the list
            for (int i = 0; i < equipmentSlots.Count; i++)
            {
                EquipmentSlot slot = equipmentSlots[i];
                if (slot.active) continue;
                if (slot.type.Contains(t))
                {
                    //Found a match!
                    slot.active = true;
                    succeeded = true;
                    neededSlots.Add(i);
                    break;
                }
            }
            if (!succeeded)
            {
                //TODO: Log console message, then fail gracefully
                Debug.LogError($"Can't fill slot of type {t}, aborting");

                //Cancel search, return unused slots
                foreach (int i in neededSlots)
                {
                    EquipmentSlot slot = equipmentSlots[i];
                    slot.active = false;
                }

                return;
            }
        }

        //Quick sanity check that we have all the slots
        Debug.Assert(neededSlots.Count == equip.secondarySlots.Count + 1, "Counts did not align correctly!", this);

        //Attach to all slots
        foreach (int i in neededSlots)
        {
            EquipmentSlot slot = equipmentSlots[i];
            slot.equipped = item;
            if (!slot.active) Debug.LogError($"Slot {i} should be active, but is not. Slot was either incorrectly added or incorrectly set to inactive");
            slot.active = true;
        }

        //Fire off equip function
        equip.OnEquip(monster);

        //Done!
    }

    public void UnequipItem(int ItemIndex)
    {
        Unequip(inventory[ItemIndex]);
    }

    public void UnequipSlot(int SlotIndex)
    {
        ItemStack i = equipmentSlots[SlotIndex].equipped;
        Unequip(i);
    }

    public void Unequip(ItemStack toRemove)
    {
        for (int i = 0; i < equipmentSlots.Count; i++)
        {
            EquipmentSlot slot = equipmentSlots[i];
            if (slot.equipped == toRemove)
            {
                slot.active = false;
                slot.equipped = null;
            }
        }
        
        toRemove.held[0].GetComponent<EquippableItem>().OnUnequip();
    }
    
    //Has to search inventory for item stack number, try to avoid
    public void Unequip(Item i)
    {
        int index = monster.inventory.GetIndexOf(i);
        if (index == -1)
        {
            Debug.LogError("Something has gone very wrong. An item thinks it was equipped, but it's monster did not hold it.", this);
            return;
        }
        UnequipItem(index);
    }
}
