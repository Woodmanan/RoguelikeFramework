using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnewayTile : RogueTile
{
    public Vector2 movesTo = Vector2.up;
    public float leniency = .35f;

    //The EXTRA cost to move from this tile to a tile in direction.
    //For most tiles, this is 0. If you're extra special fast, it can be negative.
    public override float CostToMoveIn(Vector2Int direction)
    {
        Vector2 normDirection = direction;
        if (Vector2.Dot(movesTo.normalized, normDirection.normalized) > leniency)
        {
            return -0.5f; //Conveyor belt pays for helf the cost
        }
        else
        {
            return 1000f; //Conver belt fights you forever
        }
    }
}
