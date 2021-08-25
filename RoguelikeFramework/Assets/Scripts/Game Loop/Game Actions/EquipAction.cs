using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        caller.equipment.Equip(itemIndex, equipIndex);
        caller.energy -= 100;
        if (false) { yield return null; } //Here so compiler doesn't yell.
    }

    //Called after construction, but before execution!
    //This is THE FIRST spot where caller is not null! Heres a great spot to actually set things up.
    public override void OnSetup()
    {

    }
}
