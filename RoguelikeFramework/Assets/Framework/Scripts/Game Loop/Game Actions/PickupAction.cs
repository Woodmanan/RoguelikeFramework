using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PickupAction : GameAction
{
    List<int> indices;

    //Constuctor for the action; must include caller!
    public PickupAction(params int[] indices)
    {
        this.indices = indices.ToList();
    }

    public PickupAction(List<int> indices)
    {
        this.indices = indices;
    }

    public void AddIndex(int i)
    {
        #if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Assert(!indices.Contains(i), "Drop action cannot have duplicates!");
        #endif

        indices.Add(i);
    }

    //The main function! This EXACT coroutine will be executed, even across frames.
    //See GameAction.cs for more information on how this function should work!
    public override IEnumerator TakeAction()
    {
        if (caller.inventory == null)
        {
            Debug.LogError($"{caller.GetLocalizedName()} cannot pickup without an inventory! Skipping turn to prevent deadlock.", caller);
            caller.energy = 0;
            yield break;
        }

        if (indices.Count == 0)
        {
            Debug.Log($"{caller.GetLocalizedName()} tried to pick up no items.");
            yield break;
        }

        foreach (int index in indices)
        {
            caller.inventory.PickUp(index);
        }

        caller.energy -= 100;

        caller.inventory.GetFloor().Collapse();
    }

    //Called after construction, but before execution!
    //This is THE FIRST spot where caller is not null! Heres a great spot to actually set things up.
    public override void OnSetup()
    {

    }
}
