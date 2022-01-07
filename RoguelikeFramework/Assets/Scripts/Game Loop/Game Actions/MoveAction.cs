using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveAction : GameAction
{
    public Vector2Int intendedLocation;
    public Direction direction;
    public bool costs;

    //Constuctor for the action
    public MoveAction(Vector2Int location, bool costs = true)
    {
        //Construct me! Assigns caller by default in the base class
        intendedLocation = location;
        this.costs = costs;
    }

    //The main function! This EXACT coroutine will be executed, even across frames.
    //See GameAction.cs for more information on how this function should work!
    public override IEnumerator TakeAction()
    {
        yield return GameAction.StateCheck;
        CustomTile tile = Map.current.GetTile(intendedLocation);
        if (tile.BlocksMovement())
        {
            Debug.Log($"{caller.name} tried to move into a {tile.name}.");
            yield break;
        }

        caller.connections.OnMove.Invoke();

        if (tile.currentlyStanding != null)
        {
            AttackAction attack = new AttackAction(tile.currentlyStanding);
            attack.Setup(caller);
            while (attack.action.MoveNext())
            {
                yield return attack.action.Current;
            }
            yield break;
        }

        caller.SetPosition(intendedLocation);

        if (costs)
        {
            caller.energy -= caller.energyPerStep * tile.movementCost;
        }

        caller.UpdateLOS();
    }

    //Called after construction, but before execution!
    public override void OnSetup()
    {

    }
}
