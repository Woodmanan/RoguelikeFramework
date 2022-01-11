using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class DropAction : GameAction
{
    List<int> indices;

    //Constuctor for the action; must include caller!
    public DropAction(params int[] indices)
    {
        this.indices = indices.ToList();
    }

    public DropAction(List<int> indices)
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
            Debug.LogError($"{caller.name} cannot drop without an inventory! Skipping turn to prevent deadlock.", caller);
            caller.energy = 0;
            yield break;
        }

        if (indices.Count == 0)
        {
            Debug.Log($"{caller.name} tried to drop no items.");
            yield break;
        }

        //Collect all removables, dump them in one go
        List<int> needsToBeRemoved = new List<int>();
        foreach (int index in indices)
        {
            //For each item, check if it's equipped. If so, remove it.
            ItemStack item = caller.inventory[index];
            if (item == null) continue;
            EquipableItem equip = item.held[0].equipable;
            if (equip && equip.isEquipped)
            {
                needsToBeRemoved.Add(index);
            }
        }

        if (needsToBeRemoved.Count > 0)
        {
            RemoveAction secondaryAction = new RemoveAction(needsToBeRemoved);
            secondaryAction.Setup(caller);
            while (secondaryAction.action.MoveNext())
            {
                yield return secondaryAction.action.Current;
            }
        }

        foreach (int index in indices)
        {
            caller.inventory.Drop(index);
        }

        caller.energy -= 100;

        caller.inventory.GetFloor().Collapse();
    }

    //Called after construction, but before execution!
    public override void OnSetup()
    {

    }
}
