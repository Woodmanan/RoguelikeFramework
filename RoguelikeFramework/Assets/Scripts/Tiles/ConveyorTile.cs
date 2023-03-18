using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConveyorTile : RogueTile
{
    public Vector2Int movesTo = Vector2Int.up;
    public float leniency = .8f;
    public float penalty = -.8f;

    bool willMoveMonsters;
    bool willMoveItems;
    bool hasCheckedMonsters = false;
    bool hasCheckedItems = false;
    public Monster heldMonster;
    public ItemStack[] items;

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
    public void ClearMovementCache()
    {
        willMoveMonsters = false;
        willMoveItems = true;
        hasCheckedMonsters = false;
        hasCheckedItems = false;
    }

    public void CheckMovement()
    {
        if (currentlyStanding && WillMove())
        {
            heldMonster = currentlyStanding;
            this.currentlyStanding = null;
            heldMonster.currentTile = null;
        }
    }

    public void CheckItems()
    {
        if (inventory && WillMoveItems())
        {
            items = inventory.items;
            inventory.ClearItems();
        }
    }

    //Semi-recursive check - runs down the line, looking for open conveyor. If open, returns true up the stack and caches for later checks.
    public bool WillMove()
    {
        if (!hasCheckedMonsters)
        {
            hasCheckedMonsters = true;

            //Early out - we are non-blocking.
            if (!currentlyStanding)
            {
                willMoveMonsters = true;
                return true;
            }

            RogueTile next = Map.current.GetTile(location + movesTo);
            if (next.currentlyStanding || next.BlocksMovement())
            {
                ConveyorTile nextConveyor = next as ConveyorTile;
                if (nextConveyor)
                {
                    willMoveMonsters = nextConveyor.WillMove();
                }
                else
                {
                    willMoveMonsters = false;
                }
            }
            else
            {
                willMoveMonsters = true;
            }
        }

        return willMoveMonsters;
    }

    public bool WillMoveItems()
    {
        if (!hasCheckedItems)
        {
            hasCheckedItems = true;

            //Early out - we are non-blocking.
            if (!inventory)
            {
                willMoveItems = false;
                return false;
            }

            int numItems = inventory.Count;

            RogueTile next = Map.current.GetTile(location + movesTo);
            if (next.BlocksMovement() || next.inventory == null || next.inventory.available < numItems)
            {
                ConveyorTile nextConveyor = next as ConveyorTile;
                if (nextConveyor)
                {
                    willMoveItems = nextConveyor.WillMoveItems();
                }
                else
                {
                    willMoveItems = false;
                }
            }
            else
            {
                willMoveItems = true;
            }
        }

        return willMoveItems;
    }

    public void PassToMovement()
    {
        RogueTile tile = Map.current.GetTile(location + movesTo);

        //Pass back to yourself
        if (tile.BlocksMovement() || tile.currentlyStanding)
        {
            tile = this;
        }

        if (willMoveMonsters)
        {
            heldMonster.SetPosition(tile.location);
            heldMonster.UpdateLOS();
            if (tile != this && (isVisible || tile.isVisible))
            {
                AnimationController.AddAnimationForObject(new SlideAnimation(heldMonster, location, tile.location), heldMonster);
            }
            heldMonster = null;
        }
    }
}
