using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

[Serializable]
public class EquipmentSlot
{
    public string slotName;
    public List<EquipSlotType> type; //Slots can be more than one type of slot
    public ItemStack equipped; //But can only ever hold one item
    public bool active;
    public bool removable = true;
    //Similar to inventory slots, but *theoretically* shouldn't change. Might change when mutation system is online.
    [HideInInspector] public int position;
    public bool CanAttackUnarmed;
    public WeaponBlock unarmedAttack;

    public EquipmentSlot Instantiate()
    {
        return (EquipmentSlot) this.MemberwiseClone();
    }
}

public class Equipment : MonoBehaviour
{
    public bool CanUnequip = true;
    [HideInInspector] public Monster monster;
    private Inventory inventory;

    public List<EquipmentSlot> equipmentSlots;

    [Tooltip("Will this monster attempt to equip anything loaded into it's inventory?")]
    public bool WillEquipInventory = true;

    public EquipmentSlot this[int index]
    {
        get { return equipmentSlots[index]; }
    }

    public Action OnEquipmentAdded;

    // Start is called before the first frame update
    public void Setup()
    {
        monster = GetComponent<Monster>();
        inventory = GetComponent<Inventory>();
        this.enabled = false; //Shuts off expensive events

        //Set up positional stuff. This should never change, so there's a lot less logic for it.
        for (int i = 0; i < equipmentSlots.Count; i++)
        {
            equipmentSlots[i].position = i;
            equipmentSlots[i].equipped = null;
        }

        if (WillEquipInventory && inventory != null)
        {
            //Setup inventory to confirm we've loaded our items.
            inventory.Setup();

            foreach (int itemIndex in inventory.AllIndices())
            {
                EquipableItem equip = inventory[itemIndex].held[0].GetComponent<EquipableItem>();
                if (equip)
                {
                    int equipSlot = CanSafelyEquip(equip);
                    if (equipSlot >= 0)
                    {
                        Equip(itemIndex, equipSlot);
                    }
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public int CanSafelyEquip(EquipableItem equip)
    {
        //Determine which slots are available
        EquipSlotType neededType = equip.primarySlot;
        foreach (EquipmentSlot slot in equipmentSlots.OrderBy(x => x.type.Count))
        {
            if (slot.active) continue;
            if (slot.type.Contains(neededType))
            {
                if (CanEquip(equip, slot.position))
                {
                    return slot.position;
                }
                return -1;
            }
        }

        return -1;

    }

    //Checks if we are about to equip an object to it's own slot, when it's already equipped there.
    //Weird edge case that deserved it's own function call.
    public bool RequiresReequip(int itemIndex, int EquipIndex)
    {
        //Get item
        ItemStack item = inventory[itemIndex];
        EquipableItem equip = item.held[0].equipable;
        if (equip.isEquipped && equipmentSlots[EquipIndex].active)
        {
            if (object.ReferenceEquals(equipmentSlots[EquipIndex].equipped.held[0], item.held[0]))
            {
                return false;
            }
        }
        return true;
    }

    public bool CanEquip(EquipableItem equip, int EquipIndex)
    {
        List<int> neededSlots = new List<int>();

        EquipSlotType primary = equip.primarySlot;

        //Confirm that, if item is equipped already, it could be moved.
        if (equip.isEquipped && !equip.removable)
        {
            return false;
        }

        { //Main slot checking. Done seperately, caused we can't reroute this one.

            EquipmentSlot main = equipmentSlots[EquipIndex];
            if (!main.type.Contains(primary))
            {
                //We can't equp this to that.
                return false;
            }

            //Check for main slot cursed
            //This check is probably unecessary, but I don't think it hurts, so I'm leaving it.
            if (main.active)
            {
                if (!main.removable)
                {
                    //TODO: Console message about why that's not allowed, and you should feel bad
                    return false;
                }
            }

            neededSlots.Add(EquipIndex);
        }

        { //Second slot checking. If one of these fails, we keep moving down the list until we get one that we like. Having none kills the check.

            foreach (EquipSlotType t in equip.secondarySlots)
            {
                bool succeeded = false;

                //See if there is a free spot that is not already in the list
                for (int i = 0; i < equipmentSlots.Count; i++)
                {
                    EquipmentSlot slot = equipmentSlots[i];
                    if (slot.removable && slot.type.Contains(t) && !neededSlots.Contains(i))
                    {
                        //Found a match!
                        neededSlots.Add(i);
                        succeeded = true;
                        break;
                    }
                }

                if (!succeeded)
                {
                    RogueLog.singleton.Log($"You must have your {t} slot avaible to equip a {equip.GetComponent<Item>().GetName()}");
                    return false;
                }
            }
        }

        return true;
    }
    
    public int GetFirstSlot(EquipableItem item)
    {
        foreach (EquipmentSlot s in equipmentSlots)
        {
            if (s.type.Contains(item.primarySlot))
            {
                return s.position;
            }
        }
        return -1;
    }

    //Confirms the existence of enough slots to attach this item
    public bool CanEquip(int itemIndex, int EquipIndex)
    {
        //Get item
        ItemStack item = inventory[itemIndex];
        if (item == null)
        {
            Debug.LogError($"Can't attach null item at {itemIndex}");
            return false;
        }

        
        EquipableItem equip = item.held[0].equipable;

        return CanEquip(equip, EquipIndex);
    }

    /*
     * Returns the slots that must be unequipped. The behaviour of this
     * function is only sensible while CanEquip() of the same info is true.
     * This function heavily relies on assumptions given by CanEquip,
     * so MAKE SURE that that one works first
     */
    public List<int> SlotsNeededToEquip(EquipableItem equip, int EquipIndex)
    {
        //Setup vars
        List<int> neededSlots = new List<int>();

        EquipSlotType primary = equip.primarySlot;

        EquipmentSlot main = equipmentSlots[EquipIndex];
        if (main.active)
        {
            neededSlots.Add(EquipIndex);
        }

        foreach (EquipSlotType t in equip.secondarySlots)
        {
            bool succeeded = false;
            //See if there is a free spot that is not already in the list
            for (int i = 0; i < equipmentSlots.Count; i++)
            {
                EquipmentSlot slot = equipmentSlots[i];
                if (!slot.active && slot.type.Contains(t) && !neededSlots.Contains(i))
                {
                    //Found a match!
                    neededSlots.Add(i);
                    succeeded = true;
                    break;
                }
            }

            if (succeeded) break;

            //We're here, so we didn't succeed. Now try the same thing, but with slots that we could remove
            for (int i = 0; i < equipmentSlots.Count; i++)
            {
                EquipmentSlot slot = equipmentSlots[i];
                if (slot.active && slot.removable && slot.type.Contains(t) && !neededSlots.Contains(i))
                {
                    //Found a match!
                    neededSlots.Add(i);
                    succeeded = true;
                    break;
                }
            }

            if (!succeeded)
            {
                Debug.LogError("This should not be possible! Needed Slots has failed the assertion that CanEquip should have provided");
                return neededSlots;
            }
        }

        for (int i = neededSlots.Count - 1; i >= 0; i--)
        {
            if (!equipmentSlots[neededSlots[i]].active)
            {
                neededSlots.RemoveAt(i);
            }
        }

        return neededSlots;
    }

    public List<int> SlotsNeededToEquip(int itemIndex, int EquipIndex)
    {
        //Setup vars
        List<int> neededSlots = new List<int>();

        //Get item
        ItemStack item = inventory[itemIndex];

        if (item == null)
        {
            Debug.LogError($"Can't attach null item at {itemIndex}");

            return new List<int> { -1 };
        }

        //Confirm that slot is open and primary
        EquipableItem equip = item.held[0].equipable;

        return SlotsNeededToEquip(equip, EquipIndex);
    }

    //Monster of a function that equips a given item to this equipment holder
    public void Equip(int itemIndex, int EquipIndex)
    {
        //Reassert everything we already know!
        #if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Assert(CanEquip(itemIndex, EquipIndex), "Equip Error! Equipping something should require you to be able to equip it!");
        Debug.Assert(RequiresReequip(itemIndex, EquipIndex), "Equip Error! You shouldn't be trying to re-equip something to the same slot!");
        Debug.Assert(SlotsNeededToEquip(itemIndex, EquipIndex).Count == 0, "Equip Error! You must have freed all the needed slots before equipping something!");
        Debug.Assert(!inventory[itemIndex].held[0].equipable.isEquipped, "Equip Error! Someone should have unequipped the main item already.");
        #endif

        //Setup vars
        List<int> neededSlots = new List<int>();

        //Get item
        ItemStack item = inventory[itemIndex];
        EquipableItem equip = item.held[0].equipable;

        //Confirm that slot is open and primary
        EquipmentSlot main = equipmentSlots[EquipIndex];
        if (!main.type.Contains(equip.primarySlot))
        {
            //TODO: Console error!
            RogueLog.singleton.Log("<color=red>Item equipped to wrong type of primary slot!");
            //Debug.LogError("Item equipped to wrong type of primary slot!", equip);
            return;
        }

        neededSlots.Add(EquipIndex);

        //Confirm that secondary slots are available
        foreach (EquipSlotType t in equip.secondarySlots)
        {
            bool succeeded = false;
            //See if there is a free spot that is not already in the list
            for (int i = 0; i < equipmentSlots.Count; i++)
            {
                EquipmentSlot slot = equipmentSlots[i];
                if (slot.active) continue; //This skip is completely okay now! Allowed because we know we've already freed all the slots we needed
                if (slot.type.Contains(t) && !neededSlots.Contains(i))
                {
                    //Found a match!
                    succeeded = true;
                    neededSlots.Add(i);
                    break;
                }
            }


            //PARANOIA - Check that we didn't somehow fail to do the thing we've asserted twice now
            if (!succeeded)
            {
                //TODO: Log console message, then fail gracefully
                Debug.LogError("Equipment has failed to equip an item - This means that CanEquip and NeededSlots did not make good on their assertions");
                return;
            }
        }

        //Quick sanity check that we have all the slots (Which is main slot + number of secondary slots)
        #if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Assert(neededSlots.Count == equip.secondarySlots.Count + 1, "Counts did not align correctly!", this);
        #endif

        //Attach to all slots
        foreach (int i in neededSlots)
        {
            EquipmentSlot slot = equipmentSlots[i];

            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Assert(!slot.active, $"Slot {i} should be inactive, but is is still set active. Slot was either incorrectly added to list, or was was not unequipped correctly.");
            //Debug.Assert(slot.equipped == null, $"Slot still has an item in it! We're going to just override now, but that item was something?", slot.equipped.held[0]);
            #endif

            slot.equipped = item;
            slot.active = true;
            slot.removable = equip.removable;
        }

        //Fire off equip function
        equip.OnEquip(monster);

        //Fire off our own events to let people know that this has succeeded
        //succ
        OnEquipmentAdded?.Invoke();

        
        //Done!
    }

    //TODO: Change all remove functions from bools to ints/floats, and have them returns the cost of the thing they just removed.
    //Then edit RemoveAction.CS and have it more properly reflect how that works.
    public bool UnequipItem(int ItemIndex)
    {
        return Unequip(inventory[ItemIndex]);
    }

    public bool UnequipSlot(int SlotIndex)
    {
        #if UNITY_EDITOR || GFXDEVICE_WAITFOREVENT_MESSAGEPUMP
        if (equipmentSlots.Count <= SlotIndex)
        {
            Debug.LogError($"Tried to unequip slot {SlotIndex}, but monster only had {equipmentSlots.Count} slots.");
            return false;
        }
        #endif

        if (equipmentSlots[SlotIndex].active)
        {
            ItemStack i = equipmentSlots[SlotIndex].equipped;
            return Unequip(i);
        }
        else
        {
            return false;
        }
    }

    //TODO: Finish refactor for removable code
    public bool Unequip(ItemStack toRemove)
    {
        if (!toRemove.held[0].equipable.removable)
        {
            Debug.Log($"Your {toRemove.held[0].GetName()} is cursed and cannot be removed.");
            return false;
        }
        bool removedSomething = false;
        for (int i = 0; i < equipmentSlots.Count; i++)
        {
            EquipmentSlot slot = equipmentSlots[i];
            if (slot.equipped == toRemove)
            {
                slot.active = false;
                slot.removable = true;
                slot.equipped = null;
                removedSomething = true;
            }
        }
        if (removedSomething)
        {
            toRemove.held[0].equipable.OnUnequip();
        }
        return removedSomething;
    }

    public void Equip(Item i)
    {
        //Get item index
        int index = monster.inventory.GetIndexOf(i);
        if (index == -1)
        {
            Debug.LogError("Something has gone very wrong. An item thinks it was equipped, but it's monster did not hold it.", this);
            return;
        }

        //Get equipment index
        EquipableItem e = i.equipable;
        EquipSlotType slot = e.primarySlot;

        for (int c = 0; c < equipmentSlots.Count; c++)
        {
            if (equipmentSlots[c].type.Contains(slot) && !equipmentSlots[c].active)
            {
                Equip(index, c);
                return;
            }
        }
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

    public int EquippedIndexOf(Item item)
    {
        for (int i = 0; i < equipmentSlots.Count; i++)
        {
            EquipmentSlot slot = equipmentSlots[i];
            if (slot.active && slot.equipped.held[0] == item)
            {
                return i;
            }
        }
        return -1;
    }
}
