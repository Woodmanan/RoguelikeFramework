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
    public bool didMove = false;

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
        RogueTile tile = Map.current.GetTile(intendedLocation);
        if (tile.IsInteractable())
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
        }
        else if (tile.BlocksMovement())
        {
            Debug.Log("Console Message: You don't can't do that.");
            yield break;
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
                Monster other = tile.currentlyStanding;

                if (other.willSwap)
                {

                    RogueTile currentTile = caller.currentTile;
                    RogueTile otherTile = other.currentTile;

                    //Force a swap to happen
                    Map.current.SwapMonsters(currentTile, otherTile);

                    //Drain some energy - prevents weird double-ups happening - faster monsters get to the front.
                    other.energy -= other.energyPerStep * currentTile.movementCost;
                    caller.energy -= caller.energyPerStep * otherTile.movementCost;

                    if (animates && caller.renderer.enabled)
                    {
                        AnimationController.AddAnimation(new MoveAnimation(caller, currentTile.location, otherTile.location, true));
                        AnimationController.AddAnimation(new MoveAnimation(other, otherTile.location, currentTile.location, true));
                    }
                }
                else
                {
                    //Don't swap with someone who's doing stuff.
                    caller.energy -= caller.energyPerStep * tile.movementCost;
                }

                yield break;
            }
        }

        if (costs)
        {
            caller.energy -= caller.energyPerStep * tile.movementCost;
        }

        //Pull out the old location for the animation
        Vector2Int oldLocation = caller.location;

        //Set that we managed to change locations
        didMove = true;
        caller.willSwap = true;

        caller.SetPosition(intendedLocation);

        //Add the movement anim
        if (animates && caller.renderer.enabled)
        {
            AnimationController.AddAnimation(new MoveAnimation(caller, oldLocation, intendedLocation));
        }
        else
        {
            caller.transform.position = new Vector3(intendedLocation.x, intendedLocation.y, Monster.monsterZPosition);
        }

        Stair stair = tile as Stair;
        if (stair && caller == Player.player && useStair)
        {
            ChangeLevelAction act = new ChangeLevelAction(stair.up);
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