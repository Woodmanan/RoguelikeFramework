using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Quadrant
{
    public Direction dir;
    private Vector2Int origin;

    public Quadrant(Direction dir, Vector2Int origin)
    {
        this.dir = dir;
        this.origin = origin;
    }

    public Vector2Int transformWorld(int row, int col)
    {
        switch (dir)
        {
            case Direction.SOUTH:
                return new Vector2Int(origin.x + col, origin.y - row);
            case Direction.NORTH:
                return new Vector2Int(origin.x + col, origin.y + row);
            case Direction.EAST:
                return new Vector2Int(origin.x + row, origin.y + col);
            case Direction.WEST:
                return new Vector2Int(origin.x - row, origin.y + col);
        }
        
        Debug.LogError("Dir not correctly set.");
        return Vector2Int.zero;
    }
}
