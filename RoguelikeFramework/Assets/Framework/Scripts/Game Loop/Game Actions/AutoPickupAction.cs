﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class AutoPickupAction : GameAction
{
    public static HashSet<Item> SeenItems = new HashSet<Item>();

    Item toPickup;
    bool instantPickup = false;

    //Constuctor for the action
    public AutoPickupAction(Item toPickup)
    {
        this.toPickup = toPickup;
    }

    public AutoPickupAction()
    {
        this.instantPickup = true;
    }

    //The main function! This EXACT coroutine will be executed, even across frames.
    //See GameAction.cs for more information on how this function should work!
    public override IEnumerator TakeAction()
    {
        if (!instantPickup && (!toPickup || toPickup.held))
        {
            yield break;
        }

        //Instant pickups don't move
        Vector2Int goalLocation = instantPickup ? caller.location : toPickup.location;

        { //Move to goal - action does not execute for instant pickups
            PathfindAction pathAction = new PathfindAction(goalLocation, false);
            pathAction.Setup(caller);
            while (pathAction.action.MoveNext())
            {
                if (caller.view.visibleEnemies.Count > 0)
                {
                    if (caller == Player.player)
                    {
                        RogueLog.singleton.Log($"You see a " + caller.view.visibleEnemies[0].GetLocalizedName() + " and stop.", priority: LogPriority.HIGH);
                    }
                    yield break;
                }
                yield return pathAction.action.Current;
            }
        }

        if (caller.location == goalLocation)
        {
            List<int> pickupIndices = new List<int>();
            List<ItemStack> stopIndices = new List<ItemStack>();
            RogueTile tile = Map.current.GetTile(goalLocation);
            //We made it!
            foreach (ItemStack stack in tile.inventory.items)
            {
                if (stack == null) continue;
                if (IsPriorityPickup(stack.held[0]))
                {
                    pickupIndices.Add(stack.position);
                }
                else
                {
                    stopIndices.Add(stack);
                    
                }
            }

            //If enemies are visible, move all items into stop indices
            //This stops the pickup, but still gets them read out.
            if (caller.view.visibleEnemies.Count > 0)
            {

                foreach (int index in pickupIndices)
                {
                    stopIndices.Add(tile.inventory[index]);
                }
                pickupIndices.Clear();
            }

            //Pickup all priority items
            if (pickupIndices.Count > 0)
            {
                PickupAction pickupAction = new PickupAction(pickupIndices);
                pickupAction.Setup(caller);
                while (pickupAction.action.MoveNext())
                {
                    yield return pickupAction.action.Current;
                }
            }

            //Stop with pickup message
            if (stopIndices.Count > 0)
            {
                if (caller == Player.player)
                {
                    RogueLog.singleton.LogTemplate("ItemsSeen", new { items = stopIndices.Select(x => x.GetName(true)) }, null, LogPriority.HIGH);
                }

                //If we could safely pick these up, mark them
                if (caller.view.visibleEnemies.Count == 0)
                {
                    foreach (ItemStack stack in stopIndices)
                    {
                        SeenItems.Add(stack.held[0]);
                    }
                }   
            }

            yield return AbortAll;
        }
    }

    //Called after construction, but before execution!
    //This is THE FIRST spot where caller is not null! Heres a great spot to actually set things up.
    public override void OnSetup()
    {

    }

    public static bool IsPriorityPickup(Item item)
    {
        return item.tags.MatchAnyTags("Item.Consumable");
    }
}
