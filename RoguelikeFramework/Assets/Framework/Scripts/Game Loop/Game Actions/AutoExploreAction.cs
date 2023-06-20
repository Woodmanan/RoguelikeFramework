﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoExploreAction : GameAction
{
    //Constuctor for the action; must include caller!
    public AutoExploreAction()
    {
        //Construct me! Assigns caller by default in the base class
    }

    //The main function! This EXACT coroutine will be executed, even across frames.
    //See GameAction.cs for more information on how this function should work!
    public override IEnumerator TakeAction()
    {
        Player player = caller as Player;
        
        //Push empty action to trick animation system into speeding up
        InputTracking.PushAction(PlayerAction.NONE);

        while (true)
        {
            if (caller.view.visibleEnemies.Count > 0)
            {
                RogueLog.singleton.Log("You cannot auto-explore while enemies are in sight.");
                yield break;
            }

            { //Check for auto actions before true exploration starts

                //Rest action first!
                GameAction restAct = new RestAction();
                restAct.Setup(caller);
                while (restAct.action.MoveNext())
                {
                    yield return restAct.action.Current;
                }

                //Check for any items about
                Item priorityItem = GetNextPriorityItem();
                if (priorityItem != null)
                {
                    GameAction autoPickupAction = new AutoPickupAction(priorityItem);
                    autoPickupAction.Setup(caller);
                    while (autoPickupAction.action.MoveNext())
                    {
                        yield return autoPickupAction.action.Current;
                    }
                }

            }
            

            //Build up the points we need!
            List<Vector2Int> goals = new List<Vector2Int>();
            for (int i = 1; i < Map.current.width - 1; i++)
            {
                for (int j = 1; j < Map.current.height - 1; j++)
                {
                    Vector2Int pos = new Vector2Int(i, j);
                    if (Map.current.NeedsExploring(pos))
                    {
                        goals.Add(pos);
                    }
                }
            }

            if (goals.Count == 0)
            {
                RogueLog.singleton.Log("There's nothing else to explore!", null, LogPriority.HIGH, LogDisplay.STANDARD);

                yield break;
            }

            Path path = Pathfinding.CreateDjikstraWithAstar(caller.location, goals);

            if (path.Count() == 0)
            {
                RogueLog.singleton.Log("Can't reach anymore unexplored spaces from here!");
                yield break;
            }

            while (path.Count() > 0)
            {
                //Did we get moved on our path? Great, skip one.
                if(caller.location == path.Peek())
                {
                    path.Pop();
                    if (path.Count() == 0)
                    {
                        break;
                    }
                }

                if (Vector2Int.Distance(caller.location, path.Peek()) > 1.8f)
                {
                    Debug.Log("Not making any progress, aborting!");
                    this.successful = false;
                    yield break;
                }
                Vector2Int next = path.Pop();
                MoveAction act = new MoveAction(next, true, false);

                caller.UpdateLOS();

                act.Setup(caller);
                while (act.action.MoveNext())
                {
                    yield return act.action.Current;
                }
                
                //Check for player escape
                if (InputTracking.NumOfUnmatchedActions(PlayerAction.NONE, PlayerAction.AUTO_EXPLORE) > 0)
                {
                    InputTracking.Clear();
                    yield break;
                }

                //Copied to try and get ahead of the wait check.
                if (caller.view.visibleEnemies.Count > 0)
                {
                    RogueLog.singleton.Log($"You see a " + caller.view.visibleEnemies[0].GetLocalizedName() + " and stop.", priority: LogPriority.HIGH);
                    yield break;
                }

                //Check for any items about
                Item priorityItem = GetNextPriorityItem();
                if (priorityItem != null)
                {
                    GameAction autoPickupAction = new AutoPickupAction(priorityItem);
                    autoPickupAction.Setup(caller);
                    while (autoPickupAction.action.MoveNext())
                    {
                        yield return autoPickupAction.action.Current;
                    }
                    break;
                }

                //Uncomment for timed steps
                //yield return new WaitForSeconds(.05f);

                yield return GameAction.StateCheck;
            }
        }
    }

    private Item GetNextPriorityItem()
    {
        foreach (Item item in caller.view.visibleItems)
        {
            if (AutoPickupAction.IsPriorityPickup(item) || !AutoPickupAction.SeenItems.Contains(item))
            {
                return item;
            }
        }

        return null;
    }

    

    //Called after construction, but before execution!
    //This is THE FIRST spot where caller is not null! Heres a great spot to actually set things up.
    public override void OnSetup()
    {

    }
}