using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RemoveAction : GameAction
{
    List<int> slotsToRemove;

    //Constuctor for the action; must include caller!
    public RemoveAction(params int[] equipsToRemove)
    {
        //Construct me! Assigns caller by default in the base class
        slotsToRemove = equipsToRemove.ToList();
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
        
        foreach (int slot in slotsToRemove)
        {
            caller.equipment.UnequipSlot(slot);
        }

        caller.energy -= 100;
    }

    //Called after construction, but before execution!
    //This is THE FIRST spot where caller is not null! Heres a great spot to actually set things up.
    public override void OnSetup()
    {

    }
}
