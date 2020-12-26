using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;
using Debug = UnityEngine.Debug;

/*
 * A class (and helper classes) for doing recursive, symmetric shadowcasting on our map structure.
 *
 * Based on the lovely writeup by Albert Ford: https://www.albertford.com/shadowcasting/
 *
 * All credit goes to Albert Ford for the original algorithm, this Unity port by Woody McCoy.
 *
 * Licensed under CC0: https://github.com/370417/symmetric-shadowcasting/blob/master/LICENSE.txt
 *
 * This is currently only partially optimized. However, it runs very large queries at 60 FPS, so
 * as of now it's 'good nuff'
 */

public class LOSData
{
    public int radius;
    public Vector2Int origin;

    public bool[,] definedArea;
    public bool[,] precalculatedSight;
    public Map map;

    public LOSData(int radius, Vector2Int centeredAt)
    {
        this.radius = radius;
        origin = centeredAt;
        definedArea = new bool[radius * 2 + 1, radius * 2 + 1];
        precalculatedSight = new bool[radius * 2 + 1, radius * 2 + 1];
        map = Map.singleton;
    }

    public void setAt(int row, int col, Direction dir, bool val)
    {
        switch (dir)
        {
            case Direction.SOUTH:
                definedArea[radius + col, radius - row] = val;
                break;
            case Direction.NORTH:
                definedArea[radius + col, radius + row] = val;
                break;
            case Direction.EAST:
                definedArea[radius + row, radius + col] = val;
                break;
            case Direction.WEST:
                definedArea[radius - row, radius + col] = val;
                break;
        }
    }

    public bool getAt(int row, int col, Direction dir)
    {
        switch (dir)
        {
            case Direction.SOUTH:
                return precalculatedSight[radius + col, radius - row];
            case Direction.NORTH:
                return precalculatedSight[radius + col, radius + row];
            case Direction.EAST:
                return precalculatedSight[radius + row, radius + col];
            case Direction.WEST:
                return precalculatedSight[radius - row, radius + col];
        }

        return false;
    }

    //Attempt to take advantage of some locality to get vision measurements faster
    public void PrecalculateValues()
    {
        Vector2Int start = origin - Vector2Int.one * radius;
        for (int i = 0; i < radius * 2 + 1; i++)
        {
            for (int j = 0; j < radius * 2 + 1; j++)
            {
                precalculatedSight[i, j] = map.BlocksSight(i + start.x, j + start.y);
            }
        }
    }

    public void Imprint()
    {
        Vector2Int start = origin - Vector2Int.one * radius;
        for (int i = 0; i < (radius * 2 + 1); i++)
        {
            for (int j = 0; j < (radius * 2 + 1); j++)
            {
                if (definedArea[i, j])
                {
                    Map.singleton.Reveal(new Vector2Int(i + start.x, j + start.y));
                }
            }
        }
    }

    public void Deprint()
    {
        Vector2Int start = origin - Vector2Int.one * radius;
        for (int i = 0; i < (radius * 2 + 1); i++)
        {
            for (int j = 0; j < (radius * 2 + 1); j++)
            {
                Map.singleton.ClearLOS(new Vector2Int(i + start.x, j + start.y));
            }
        }
    }
}

public struct fraction
{
    private int rise;
    private int run;

    public fraction(int x, int y)
    {
        rise = x;
        run = y;
    }

    public float value()
    {
        return ((float) rise) / ((float) run);
    }

    public static float operator *(int a, fraction f)
    {
        return a * f.value();
    }

    public static float operator *(fraction f, int a)
    {
        return a * f.value();
    }
}







public class LOS : MonoBehaviour
{

    public static LOSData lastCall;
    
    public static fraction Slope(Vector2Int tile)
   {
       return new fraction((2 * tile.x) - 1, 2 * tile.y);
   }

   public static bool IsSymmetric(Row r, Vector2Int tile)
   {
       return (tile.x >= r.depth * r.startSlope) && (tile.x <= r.depth * r.endSlope);
   }

   public static LOSData LosAt(Vector2Int position, int distance)
   {
       if (distance <= 0)
       {
           return new LOSData(0, position);
       }
       LOSData toReturn = new LOSData(distance, position);
       toReturn.PrecalculateValues();
       toReturn.setAt(0,0, Direction.NORTH, true);
       for (int d = 0; d < 4; d++)
       {
           Quadrant q = new Quadrant((Direction) d, position);

           Row firstRow = new Row(1, new fraction(-1,1), new fraction(1,1), q);
           Scan(firstRow, toReturn);
       }

       return toReturn;
   }

   public static void Reveal(Vector2Int tile, LOSData l, Direction d)
   {
       l.setAt(tile.y, tile.x, d, true);
   }

   public static bool IsWall(Vector2Int tile, Row r, LOSData l)
   {
       if (tile.x == -1 && tile.y == -1)
       {
           //Special case for starting tile
           return false;
       }

       return l.getAt(tile.y, tile.x, r.quad.dir);
   }

   public static bool IsFloor(Vector2Int tile, Row r, LOSData l)
   {
       if (tile.x == -1 && tile.y == -1)
       {
           return false;
       }

       return !l.getAt(tile.y, tile.x, r.quad.dir);
   }

   public static void Scan(Row r, LOSData l)
   {
       Vector2Int previous = -1 * Vector2Int.one;
       foreach (Vector2Int tile in r.tiles())
       {

           if (IsWall(tile, r, l) || IsSymmetric(r, tile))
           {
               Reveal(tile, l, r.quad.dir);
           }

           if (IsWall(previous, r, l) && IsFloor(tile, r, l))
           {
               r.startSlope = Slope(tile);
           }

           if (IsFloor(previous, r, l) && IsWall(tile, r, l))
           {
               if (r.depth != l.radius)
               {
                   Row next_row = r.next();
                   next_row.endSlope = Slope(tile);
                   Scan(next_row, l);
               }
           }

           previous = tile;
       }

       if (IsFloor(previous, r, l))
       {
           if (r.depth != l.radius)
           {
               Scan(r.next(), l);
           }
       }
   }

   public static void GeneratePlayerLOS(Vector2Int location, int radius)
   {
       if (lastCall != null)
       {
           lastCall.Deprint();
       }

       lastCall = LosAt(location, radius);
       lastCall.Imprint();
   }





}
