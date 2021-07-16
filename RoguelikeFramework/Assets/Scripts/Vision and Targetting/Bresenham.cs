using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


//Should probably be a class, but it might be large and unwieldy, so I didn't want to pass by value
public class BresenhamResults
{
    public List<CustomTile> path;
    public List<CustomTile> fullPath;
    public bool blocked;
}

public class Bresenham
{
    public static BresenhamResults CalculateLine(Vector2Int start, Vector2Int end)
    {
        BresenhamResults results = new BresenhamResults();
        results.path = new List<CustomTile>();
        results.fullPath = new List<CustomTile>();
        bool beenBlocked = false;
        foreach (Vector2Int spot in GetPointsOnLine(start.x, start.y, end.x, end.y))
        {
            //It's assumed that something that blocks movement blocks this line.
            //TODO: Make sure this assumption actually makes sense
            CustomTile t = Map.singleton.GetTile(spot);
            if (!beenBlocked)
            {
                results.path.Add(t);
                results.fullPath.Add(t);
            }
            if (t.BlocksMovement())
            {
                beenBlocked = true;
            }
        }
        results.blocked = beenBlocked;
        return results;
    }

    //Bresenham Line from http://ericw.ca/notes/bresenhams-line-algorithm-in-csharp.html
    //TODO: Either write one of these yourself, or MAKE SURE YOU INCLUDE THE LICENSE
    //TODO: Write one based on this? http://members.chello.at/~easyfilter/bresenham.c
    public static IEnumerable<Vector2Int> GetPointsOnLine(int x0, int y0, int x1, int y1)
    {
        bool steep = Math.Abs(y1 - y0) > Math.Abs(x1 - x0);
        if (steep)
        {
            int t;
            t = x0; // swap x0 and y0
            x0 = y0;
            y0 = t;
            t = x1; // swap x1 and y1
            x1 = y1;
            y1 = t;
        }
        if (x0 > x1)
        {
            int t;
            t = x0; // swap x0 and x1
            x0 = x1;
            x1 = t;
            t = y0; // swap y0 and y1
            y0 = y1;
            y1 = t;
        }
        int dx = x1 - x0;
        int dy = Math.Abs(y1 - y0);
        int error = dx / 2;
        int ystep = (y0 < y1) ? 1 : -1;
        int y = y0;
        for (int x = x0; x <= x1; x++)
        {
            yield return new Vector2Int((steep ? y : x), (steep ? x : y));
            error = error - dy;
            if (error < 0)
            {
                y += ystep;
                error += dx;
            }
        }
        yield break;
    }

}
