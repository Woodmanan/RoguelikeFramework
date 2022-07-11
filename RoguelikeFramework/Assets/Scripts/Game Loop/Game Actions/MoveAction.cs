using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveAction : GameAction
{
    public Vector2Int intendedLocation;
    public Direction direction;
    public bool costs;
    public bool useStair;
    public bool animates;

    //Constuctor for the action
    public MoveAction(Vector2Int location, bool costs = true, bool useStair = true, bool animates = true)
    {
        //Construct me! Assigns caller by default in the base class
        intendedLocation = location;
        this.costs = costs;
        this.useStair = useStair;
        this.animates = true;
    }

    //The main function! This EXACT coroutine will be executed, even across frames.
    //See GameAction.cs for more information on how this function should work!
    public override IEnumerator TakeAction()
    {
        yield return GameAction.StateCheck;
        CustomTile tile = Map.current.GetTile(intendedLocation);
        if (tile.BlocksMovement())
        {
            InteractableTile interact = tile as InteractableTile;
            if (interact)
            {
                GameAction interactAction = new InteractAction(interact);
                interactAction.Setup(caller);
                while (interactAction.action.MoveNext())
                {
                    yield return interactAction.action.Current;
                }
                yield break;
            }
            else
            {
                Debug.Log("Console Message: You don't can't do that.");
                yield break;
            }
        }

        caller.connections.OnMove.Invoke();

        if (tile.currentlyStanding != null)
        {
            if (caller.GetComponent<Monster>().IsEnemy(tile.currentlyStanding))
            {
                AttackAction attack = new AttackAction(tile.currentlyStanding);
                attack.Setup(caller);
                while (attack.action.MoveNext())
                {
                    yield return attack.action.Current;
                }
                yield break;
            }
            else
            {
                // Don't hurt your friends stupid
                caller.energy -= caller.energyPerStep * tile.movementCost;
                yield break;
            }
        }

        if (costs)
        {
            caller.energy -= caller.energyPerStep * tile.movementCost;
        }

        //Pull out the old location for the animation
        Vector2Int oldLocation = caller.location;

        caller.SetPosition(intendedLocation);

        //Add the movement anim
        if (animates && caller.renderer.enabled)
        {
            AnimationController.AddAnimation(new MoveAnimation(caller, oldLocation, intendedLocation));
        }

        Stair stair = tile as Stair;
        if (stair && caller == Player.player && useStair)
        {
            ChangeLevelAction act = new ChangeLevelAction(stair.upStair);
            act.Setup(caller);
            while (act.action.MoveNext())
            {
                yield return act.action.Current;
            }
            yield return GameAction.AbortAll;
        }

        caller.UpdateLOS();
    }

    //Called after construction, but before execution!
    public override void OnSetup()
    {

    }
}