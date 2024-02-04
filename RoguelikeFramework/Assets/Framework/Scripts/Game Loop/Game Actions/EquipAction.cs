using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/*********************************************
 * RANT WARNING: I'm leaving this here so I don't forget why I made these choices.
 * If you disagree with these, you're welcome to come try to convince me of something else.
 * 
 * Odd edge case, but hear me out
 * If you've already equipped an item, this WHOLE thing should be completely free. 
 * Essentially, you're just reorganizing your layout
 *
 * CAVEAT - some slots are special (like an arm you just grew, and is maybe super strength).
 * Reorganization should probably be paid in blood (or time), in case other game effects rely on it.
 * We can apply some special super-speed buff out of combat later, if we need to.
 * 
 * Reorganized Edge Case - If you realize that your slots are up for grabs,
 * you should remove yourself (essentially for free?)
 * For now, this will cost. This makes the most sense from a 
 * simulationist point of view (moving a helmet between your heads requires you
 * to take it off first), and while it feels wrong it makes the most loical sense.
 * I think this will all feel better after implementing Equip / Unequip speeds.
***********************************************/

public class EquipAction : GameAction
{
    public int itemIndex;
    public int equipIndex;

    //Constuctor for the action; must include caller!
    public EquipAction(int itemIndex, int equipIndex)
    {
        //Construct me! Assigns caller by default in the base class
        this.itemIndex = itemIndex;
        this.equipIndex = equipIndex;
    }

    //The main function! This EXACT coroutine will be executed, even across frames.
    //See GameAction.cs for more information on how this function should work!
    public override IEnumerator TakeAction()
    {
        if (!caller[0].equipment.CanEquip(itemIndex, equipIndex)) {
            yield break;
        }

        //Quick check: Are we trying to do something redundant?
        if (!caller[0].equipment.RequiresReequip(itemIndex, equipIndex))
        {
            Debug.Log("You can't re-equip an item to its own slot. Why would you want to do this?");
            yield break;
        }


        //We can attempt to move forward here!
        List<int> neededSlots = caller[0].equipment.SlotsNeededToEquip(itemIndex, equipIndex);

        ItemStack item = caller[0].inventory[itemIndex];
        if (item.held[0].equipable.isEquipped)
        {
            yield return SubAction(new RemoveAction(item));
        }

        if (neededSlots.Count > 0)
        {
            //We need to remove those slots!
            yield return SubAction(new RemoveAction(neededSlots));
        }

        //We can now Equip!
        //TODO: Make this energy cost item dependent
        caller[0].equipment.Equip(itemIndex, equipIndex);
        caller[0].energy -= 100;
    }

    public override string GetDebugString()
    {
        Item item = caller[0].inventory[itemIndex].held[0];
        EquipmentSlot slot = caller[0].equipment.equipmentSlots[equipIndex];
        return $"Equip Action: Equiping index {itemIndex}:{item.friendlyName} to slot {equipIndex}: {slot.slotName}";
    }

    //Called after construction, but before execution!
    //This is THE FIRST spot where caller is not null! Heres a great spot to actually set things up.
    public override void OnSetup()
    {

    }
}
