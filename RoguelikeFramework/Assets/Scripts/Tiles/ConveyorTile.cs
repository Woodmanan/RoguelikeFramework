using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConveyorTile : RogueTile
{
    public Vector2Int movesTo = Vector2Int.up;
    public float leniency = .8f;
    public float penalty = -.8f;

    bool willMove;
    bool hasChecked = false;
    public Monster heldMonster;
    ItemStack[] heldItems;

    //The EXTRA cost to move from this tile to a tile in direction.
    //For most tiles, this is 0. If you're extra special fast, it can be negative.
    public override float CostToMoveIn(Vector2Int direction)
    {
        Vector2 normDirection = direction;
        float dir = Vector2.Dot(((Vector2)movesTo).normalized, normDirection.normalized);
        if (dir > leniency)
        {
            return -1.0f; //Conveyor belt pays for half the cost
        }
        else if (dir > penalty)
        {
            return 1.0f;
        }
        else
        {
            return 1000f; //Conver belt fights you forever
        }
    }

    public override float GetMovementCost()
    {
        return 2 * movementCost;
    }

    //Undo caching - things have likely moved.
    public void Clear()
    {
        willMove = false;
        hasChecked = false;
    }

    public void CheckMovement()
    {
        if (currentlyStanding && WillMove())
        {
            heldMonster = currentlyStanding;
            this.currentlyStanding = null;
            heldMonster.currentTile = null;
            if (heldMonster == Player.player)
            {
                Debug.Log("Player is on a tile!");
            }
        }
    }

    //Semi-recursive check - runs down the line, looking for open conveyor. If open, returns true up the stack and caches for later checks.
    public bool WillMove()
    {
        if (!hasChecked)
        {
            hasChecked = true;

            //Early out - we are non-blocking.
            if (!currentlyStanding)
            {
                willMove = true;
                return true;
            }

            RogueTile next = Map.current.GetTile(location + movesTo);
            if (next.currentlyStanding || next.BlocksMovement())
            {
                ConveyorTile nextConveyor = next as ConveyorTile;
                if (nextConveyor)
                {
                    willMove = nextConveyor.WillMove();
                }
                else
                {
                    willMove = false;
                }
            }
            else
            {
                willMove = true;
            }
        }

        return willMove;
    }

    public void PassToMovement()
    {
        RogueTile tile = Map.current.GetTile(location + movesTo);

        //Pass back to yourself
        if (tile.BlocksMovement() || tile.currentlyStanding)
        {
            tile = this;
        }

        if (heldMonster)
        {
            heldMonster.SetPosition(tile.location);
            heldMonster.UpdateLOS();
            if (tile != this && (isVisible || tile.isVisible))
            {
                AnimationController.AddAnimation(new SlideAnimation(heldMonster, location, tile.location, false));
            }
            heldMonster = null;
        }

    }
}
