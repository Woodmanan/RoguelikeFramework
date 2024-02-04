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
        if (caller[0].inventory == null)
        {
            Debug.LogError($"{caller[0].GetLocalizedName()} cannot pickup without an inventory! Skipping turn to prevent deadlock.", caller[0].unity);
            caller[0].energy = 0;
            yield break;
        }

        if (indices.Count == 0)
        {
            Debug.Log($"{caller[0].GetLocalizedName()} tried to pick up no items.");
            yield break;
        }

        //Setup names
        Inventory tileInventory = Map.current.GetTile(caller[0].location).inventory;
        List<string> itemNames = indices.Select(x => tileInventory[x].GetName(true)).ToList();

        foreach (int index in indices)
        {
            caller[0].inventory.PickUp(index);
        }

        RogueLog.singleton.LogTemplate("ItemPickup", new { monster = caller[0].GetName(), singular = caller[0].singular, items = itemNames }, null, LogPriority.IMPORTANT);

        caller[0].energy -= 100;

        caller[0].inventory.GetFloor().Collapse();
    }

    public override string GetDebugString()
    {
        return $"Pickup Action on indicies: {string.Join(", ", indices.Select(x => x.ToString()))}";
    }

    //Called after construction, but before execution!
    //This is THE FIRST spot where caller is not null! Heres a great spot to actually set things up.
    public override void OnSetup()
    {

    }
}
