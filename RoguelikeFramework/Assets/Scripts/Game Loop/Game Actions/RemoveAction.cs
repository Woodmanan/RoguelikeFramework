using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RemoveAction : GameAction
{
    List<int> slotsToRemove;
    Item item;

    //Constuctor for the action; must include caller!
    public RemoveAction(params int[] equipsToRemove)
    {
        //Construct me! Assigns caller by default in the base class
        slotsToRemove = equipsToRemove.ToList();
    }

    public RemoveAction(List<int> equipsToremove)
    {
        slotsToRemove = equipsToremove;
    }

    public RemoveAction(ItemStack stack)
    {
        slotsToRemove = new List<int>();
        this.item = stack.held[0];
    }

    public RemoveAction(Item item)
    {
        slotsToRemove = new List<int>();
        this.item = item;
    }

    //The main function! This EXACT coroutine will be executed, even across frames.
    //See GameAction.cs for more information on how this function should work!
    public override IEnumerator TakeAction()
    {
        Debug.Log("I used the action to unequip!");
        if (slotsToRemove.Count == 0)
        {
            Debug.Log("You can't remove nothing!");
            yield break;
        }

        int actualRemoves = 0;
        foreach (int slot in slotsToRemove)
        {
            if (caller.equipment.UnequipSlot(slot))
            {
                actualRemoves++;
            }
        }

        //TODO: Maybe make this pause in between turns
        //This is fine for now, though.
        caller.energy -= 100 * actualRemoves;
    }

    //Called after construction, but before execution!
    //This is THE FIRST spot where caller is not null! Heres a great spot to actually set things up.
    public override void OnSetup()
    {
        if (item)
        {
            int indexOf = caller.equipment.EquippedIndexOf(item);
            if (indexOf == -1)
            {
                Debug.LogError("(1/2) Can't remove an item that is not equipped to this caller (error linked to caller)", caller);
                Debug.LogError("(2/2) Can't remove an item that is not equipped to this caller (error linked to item)", item);
                return;
            }
            slotsToRemove.Add(indexOf);
        }
    }
}
