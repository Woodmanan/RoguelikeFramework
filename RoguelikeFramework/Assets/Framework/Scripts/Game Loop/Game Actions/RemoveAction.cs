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
        if (slotsToRemove.Count == 0)
        {
            RogueLog.singleton.Log("You can't remove nothing!", priority: LogPriority.IMPORTANT);
            yield break;
        }

        int actualRemoves = 0;
        foreach (int slot in slotsToRemove)
        {
            if (caller[0].equipment.UnequipSlot(slot))
            {
                actualRemoves++;
            }
        }

        //TODO: Maybe make this pause in between turns
        //This is fine for now, though.
        caller[0].energy -= 100 * actualRemoves;
    }

    public override string GetDebugString()
    {
        return $"Remove Action on indicies: {string.Join(", ", slotsToRemove.Select(x => x.ToString()))}";
    }

    //Called after construction, but before execution!
    //This is THE FIRST spot where caller is not null! Heres a great spot to actually set things up.
    public override void OnSetup()
    {
        if (item)
        {
            int indexOf = caller[0].equipment.EquippedIndexOf(item);
            if (indexOf == -1)
            {
                Debug.LogError("(1/2) Can't remove an item that is not equipped to this caller (error linked to caller)", caller[0].unity);
                Debug.LogError("(2/2) Can't remove an item that is not equipped to this caller (error linked to item)", item);
                return;
            }
            slotsToRemove.Add(indexOf);
        }
    }
}
