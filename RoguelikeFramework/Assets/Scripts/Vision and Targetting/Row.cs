using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Row
{
    public int depth;
    public fraction startSlope;
    public fraction endSlope;
    private Map map; //For speed purposes, we want to grab this just once
    public Quadrant quad;

    public Row(int depth, fraction startSlope, fraction endSlope, Quadrant q)
    {
        this.depth = depth;
        this.startSlope = startSlope;
        this.endSlope = endSlope;
        this.quad = q;
        map = Map.current;
        quad = q;
    }

    public System.Collections.Generic.IEnumerable<Vector2Int> tiles()
    {
        int min = Mathf.FloorToInt(depth * startSlope + 0.5f);
        int max = Mathf.CeilToInt(depth * endSlope - 0.5f);
        for (int i = min; i <= max; i++)
        {
            yield return new Vector2Int(i, depth);
        }
    }

    public Vector2Int transform(Vector2Int input)
    {
        return quad.transformWorld(input.y, input.x);
    }
    
    public Row next()
    {
        return new Row(depth + 1, startSlope, endSlope, quad);
    }
}
