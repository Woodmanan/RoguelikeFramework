using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;


//Should probably be a struct, but it might be large and unwieldy, so I didn't want to pass by value
public class BresenhamResults
{
    public List<CustomTile> path;
    public List<CustomTile> fullPath;
    public bool blocked;
}

public class Bresenham
{
    public static BresenhamResults CalculateLine(Vector2Int start, Vector2Int end, bool tilesBlock = true, bool monstersBlock = false)
    {
        BresenhamResults results = new BresenhamResults();
        results.path = new List<CustomTile>();
        results.fullPath = new List<CustomTile>();
        bool beenBlocked = false;

        Vector2Int[] line = GetPointsOnLine(start.x, start.y, end.x, end.y).ToArray();

        for (int i = 0; i < line.Length; i++)
        { 
            //It's assumed that something that blocks movement blocks this line.
            //TODO: Make sure this assumption actually makes sense
            CustomTile t = Map.current.GetTile(line[i]);
            if (!beenBlocked)
            {
                results.path.Add(t);
                results.fullPath.Add(t);
            }
            //Check for movement block validity
            if (tilesBlock && t.BlocksMovement())
            {
                beenBlocked = true;
                break;
            }
            //Check for monster block validity
            if (monstersBlock && (i != 0 && i != line.Length - 1) && t.currentlyStanding != null)
            {
                beenBlocked = true;
                break;
            }
        }
        results.blocked = beenBlocked;
        return results;
    }

    //Bresenham Line from http://ericw.ca/notes/bresenhams-line-algorithm-in-csharp.html
    //TODO: Either write one of these yourself, or MAKE SURE YOU INCLUDE THE LICENSE
    //TODO: Write one based on this? http://members.chello.at/~easyfilter/bresenham.c
    /*public static IEnumerable<Vector2Int> GetPointsOnLine(int x0, int y0, int x1, int y1)
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
    }*/

    /*
     * Awesome Brensham line algorithm adapted from  http://members.chello.at/~easyfilter/bresenham.c
     * This algorithm uses an error function to move the slope correctly, meaning it doesn't
     * rely on setting things up to a quadrant to work correctly. Much nicer behavior, since all
     * lines now originate from x0, y0
     */
    public static IEnumerable<Vector2Int> GetPointsOnLine(int x0, int y0, int x1, int y1)
    {
        int dx = Mathf.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
        int dy = -Mathf.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
        int err = dx + dy, e2;                                  /* error value e_xy */


        for (; ; )
        {                                                        /* loop */
            yield return new Vector2Int(x0, y0);
            e2 = 2 * err;
            if (e2 >= dy)
            {                                       /* e_xy+e_x > 0 */
                if (x0 == x1) break;
                err += dy; x0 += sx;
            }
            if (e2 <= dx)
            {                                       /* e_xy+e_y < 0 */
                if (y0 == y1) break;
                err += dx; y0 += sy;
            }
        }
    }

    public static IEnumerable<Vector2Int> GetPointsOnLine(Vector2Int start, Vector2Int end)
    {
        return GetPointsOnLine(start.x, start.y, end.x, end.y);
    }

}
