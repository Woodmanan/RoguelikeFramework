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
            case Direction.South:
                return new Vector2Int(origin.x + col, origin.y - row);
            case Direction.North:
                return new Vector2Int(origin.x + col, origin.y + row);
            case Direction.East:
                return new Vector2Int(origin.x + row, origin.y + col);
            case Direction.West:
                return new Vector2Int(origin.x - row, origin.y + col);
        }
        
        Debug.LogError("Dir not correctly set.");
        return Vector2Int.zero;
    }
}
