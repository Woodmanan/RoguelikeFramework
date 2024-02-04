using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveAction : GameAction
{
    public Vector2Int intendedLocation;
    public Direction direction;
    public bool costs;
    public bool useStair;
    public bool didMove = false;
    public bool picksUpItems;

    //Constuctor for the action
    public MoveAction(Vector2Int location, bool costs = true, bool useStair = true, bool picksUpItems = false)
    {
        //Construct me! Assigns caller by default in the base class
        intendedLocation = location;
        this.costs = costs;
        this.useStair = useStair;
        this.picksUpItems = picksUpItems;
    }

    //The main function! This EXACT coroutine will be executed, even across frames.
    //See GameAction.cs for more information on how this function should work!
    public override IEnumerator TakeAction()
    {
        yield return GameAction.StateCheck;
        bool canMove = true;
        caller[0].connections.OnMoveInitiated.Invoke(ref intendedLocation, ref canMove);

        if (!canMove)
        {
            yield break;
        }

        
        if (!Map.current.ValidLocation(intendedLocation))
        {
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log("Monster tried to move to null tile!");
            caller[0].energy--;
            #endif
            yield break;
        }

        RogueTile tile = Map.current.GetTile(intendedLocation);

        if (tile.IsInteractable())
        {
            InteractableTile interact = tile as InteractableTile;
            if (interact)
            {
                yield return SubAction(new InteractAction(interact));
                yield break;
            }
        }
        else if (tile.BlocksMovement())
        {
            if (caller == Player.player)
            {
                RogueLog.singleton.Log("You don't can't do that.");
            }
            else
            {
                Debug.Log("Monster tried to move through a tile! Fix this!!!");
            }
            yield break;
        }
        

        if (tile.currentlyStanding)
        {
            if (caller[0].IsEnemy(tile.currentlyStanding))
            {
                yield return SubAction(new AttackAction(tile.currentlyStanding));
                yield break;
            }
            else
            {
                // Don't hurt your friends stupid
                Monster other = tile.currentlyStanding;

                if (other.willSwap)
                {

                    RogueTile currentTile = caller[0].currentTile;
                    RogueTile otherTile = other.currentTile;

                    //Force a swap to happen
                    Map.current.SwapMonsters(currentTile, otherTile);

                    //Drain some energy - prevents weird double-ups happening - faster monsters get to the front.
                    other.energy -= other.energyPerStep * currentTile.movementCost;
                    caller[0].energy -= caller[0].energyPerStep * otherTile.movementCost;

                    AnimationController.AddAnimationForMonster(new MoveAnimation(caller, currentTile.location, otherTile.location), caller);
                    AnimationController.AddAnimationForMonster(new MoveAnimation(other, otherTile.location, currentTile.location), other);
                }
                else
                {
                    //Don't swap with someone who's doing stuff.
                    caller[0].energy -= caller[0].energyPerStep * tile.movementCost;
                    caller[0].willSwap = true;
                }

                yield break;
            }
        }

        

        if (costs)
        {
            caller[0].energy -= caller[0].energyPerStep * tile.movementCost;
        }

        //Pull out the old location for the animation
        Vector2Int oldLocation = caller[0].location;

        //Set that we managed to change locations
        didMove = true;
        caller[0].willSwap = true;

        caller[0].SetPosition(intendedLocation);
        //caller[0].UpdateLOS();

        //Add the movement anim
        AnimationController.AddAnimationForMonster(new MoveAnimation(caller, oldLocation, intendedLocation), caller);

        caller[0].connections.OnMove.Invoke();

        Stair stair = tile as Stair;
        if (stair && !stair.locked && caller == Player.player && useStair)
        {
            yield return SubAction(new ChangeLevelAction(stair.up));
            yield return GameAction.AbortAll;
        }
        
        //Logging items found (player only)
        if (picksUpItems)
        {
            yield return SubAction(new AutoPickupAction());
        }
    }

    public override string GetDebugString()
    {
        return $"Move Action to {intendedLocation}";
    }

    //Called after construction, but before execution!
    public override void OnSetup()
    {

    }
}