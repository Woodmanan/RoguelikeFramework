﻿using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
//using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;
using Debug = UnityEngine.Debug;
using System.Linq;

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

    //public List<Monster> visibleMonsters;
    public List<RogueHandle<Monster>> visibleEnemies;
    public List<RogueHandle<Monster>> visibleFriends;
    public List<Item> visibleItems;

    private Vector2Int startsAt;
    private Vector2Int endsAt;

    public LOSData(int radius, Vector2Int centeredAt)
    {
        this.radius = radius;
        origin = centeredAt;
        definedArea = new bool[radius * 2 + 1, radius * 2 + 1];
        precalculatedSight = new bool[radius * 2 + 1, radius * 2 + 1];
        startsAt = origin - Vector2Int.one * radius; //Inclusive
        endsAt = origin + Vector2Int.one * (radius + 1); //Exclusive

        visibleFriends = new List<RogueHandle<Monster>>();
        visibleEnemies = new List<RogueHandle<Monster>>();
        visibleItems = new List<Item>();
    }

    public IEnumerable<RogueHandle<Monster>> GetVisibleMonsters(RogueHandle<Monster> viewer)
    {
        foreach (RogueHandle<Monster> monster in visibleEnemies)
        {
            yield return monster;
        }

        foreach (RogueHandle<Monster> monster in visibleFriends)
        {
            yield return monster;
        }

        if (viewer.IsValid())
        {
            yield return viewer;
        }
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
    public void PrecalculateValues(Map map)
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

    public bool ValueAtWorld(Vector2Int location)
    {
        return ValueAtWorld(location.x, location.y);
    }

    public bool ValueAtWorld(int x, int y)
    {
        if (Contains(x, y))
        {
            return definedArea[x - startsAt.x, y - startsAt.y];
        }
        else
        {
            return false;
        }
    }

    //Returns if an (x, y) pair could be found in our sightmap
    public bool Contains(int x, int y)
    {
        return (x >= startsAt.x && x < endsAt.x && y >= startsAt.y && y < endsAt.y);
    }

    public void CollectEntities(Map map, RogueHandle<Monster> viewer)
    {
        visibleEnemies.Clear();
        visibleFriends.Clear();
        visibleItems.Clear();
        Vector2Int start = origin - Vector2Int.one * radius;
        for (int i = 0; i < (radius * 2 + 1); i++)
        {
            for (int j = 0; j < (radius * 2 + 1); j++)
            {
                if (definedArea[i,j])
                {
                    Vector2Int loc = new Vector2Int(i + start.x, j + start.y);
                    if (loc.x >= 0 && loc.x < map.width && loc.y >= 0 && loc.y < map.height)
                    {
                        RogueTile tile = map.GetTile(new Vector2Int(i + start.x, j + start.y));
                        if (tile.currentlyStanding.IsValid())
                        {
                            if (viewer != null && tile.currentlyStanding != viewer)
                            {
                                if (tile.currentlyStanding[0].IsEnemy(viewer))
                                {
                                    visibleEnemies.Add(tile.currentlyStanding);
                                }
                                else
                                {
                                    visibleFriends.Add(tile.currentlyStanding);
                                }
                            }
                        }
                        visibleItems.AddRange(tile.inventory.AllHeld());
                    }
                }
            }
        }
    }

    public void ImprintGraphics(Map map)
    {
        Vector2Int start = origin - Vector2Int.one * radius;
        for (int i = 0; i < (radius * 2 + 1); i++)
        {
            for (int j = 0; j < (radius * 2 + 1); j++)
            {
                if (definedArea[i, j])
                {
                    map.RevealGraphics(new Vector2Int(i + start.x, j + start.y));
                }
            }
        }
    }

    public void DeprintGraphics(Map map)
    {
        Vector2Int start = origin - Vector2Int.one * radius;
        for (int i = 0; i < (radius * 2 + 1); i++)
        {
            for (int j = 0; j < (radius * 2 + 1); j++)
            {
                map.ClearGraphics(new Vector2Int(i + start.x, j + start.y));
            }
        }
    }

    public void ImprintPlayerVisibility(Map map)
    {
        Vector2Int start = origin - Vector2Int.one * radius;
        for (int i = 0; i < (radius * 2 + 1); i++)
        {
            for (int j = 0; j < (radius * 2 + 1); j++)
            {
                if (definedArea[i, j])
                {
                    map.RevealPlayerVisibility(new Vector2Int(i + start.x, j + start.y));
                }
            }
        }
    }

    public void DeprintPlayerVisibility(Map map)
    {
        Vector2Int start = origin - Vector2Int.one * radius;
        for (int i = 0; i < (radius * 2 + 1); i++)
        {
            for (int j = 0; j < (radius * 2 + 1); j++)
            {
                map.ClearPlayerVisibility(new Vector2Int(i + start.x, j + start.y));
            }
        }
    }

    public IEnumerable<Vector2Int> GetVisibleTiles(Map map)
    {
        Vector2Int start = origin - Vector2Int.one * radius;
        for (int i = 0; i < (radius * 2 + 1); i++)
        {
            for (int j = 0; j < (radius * 2 + 1); j++)
            {
                if (definedArea[i, j])
                {
                    yield return new Vector2Int(i + start.x, j + start.y);
                }
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

    public static float operator *(float a, fraction f)
    {
        return a * f.value();
    }

    public static float operator *(fraction f, float a)
    {
        return a * f.value();
    }
}







public class LOS : MonoBehaviour
{

    public static LOSData lastCall;
    public static LOSData lastGraphicsCall;
    
    public static fraction Slope(Vector2Int tile)
    {
        return new fraction(2 * tile.x - 1, 2 * tile.y);
    }

    public static bool IsSymmetric(Row r, Vector2Int tile)
    {
        return (tile.x >= r.depth * r.startSlope) && (tile.x <= r.depth * r.endSlope);
    }

    public static LOSData LosAt(Map map, Vector2Int position, int distance)
    {
        if (distance <= 0)
        {
            return new LOSData(0, position);
        }
        LOSData toReturn = new LOSData(distance, position);
        toReturn.PrecalculateValues(map);
        toReturn.setAt(0,0, Direction.NORTH, true);
        for (int d = 0; d < 4; d++)
        {
            Quadrant q = new Quadrant((Direction) d, position);

            Row firstRow = new Row(1, new fraction(-1,1), new fraction(1,1), q);
            Scan(firstRow, toReturn);
        }

        return toReturn;
    }

    public static BresenhamResults GetLineFrom(Vector2Int start, Vector2Int end, bool tilesBlock = true, bool monstersBlock = false)
    {
        return Bresenham.CalculateLine(start, end, tilesBlock, monstersBlock);
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

    public static void WritePlayerLOS()
    {
        WritePlayerLOS(Map.current, Player.player[0].location, Player.player[0].visionRadius);
    }

    public static void WritePlayerGraphics()
    {
        WritePlayerGraphics(Map.current, Player.player[0].location, Player.player[0].visionRadius);
    }

    public static void WritePlayerLOS(Map map, Vector2Int location, int radius)
    {
        if (lastCall != null)
        {
            lastCall.DeprintPlayerVisibility(Map.current);
        }
        lastCall = LosAt(map, location, radius);
        lastCall.ImprintPlayerVisibility(Map.current);
    }

    public static void WritePlayerGraphics(Map map, Vector2Int location, int radius)
    {
        LOSData view = LosAt(map, location, radius);
        Vector2Int[] locations = view.GetVisibleMonsters(RogueHandle<Monster>.Default).Select(x => x[0].location).ToArray();
        WritePlayerGraphics(view, locations);
    }

    public static void WritePlayerGraphics(LOSData view, Vector2Int[] monsterLocations)
    {
        if (lastGraphicsCall != null)
        {
            lastGraphicsCall.DeprintGraphics(Map.current);
        }
        lastGraphicsCall = view;
        lastGraphicsCall.ImprintGraphics(Map.current);
    }

    public static void ClearAllLevelLOS(Map old)
    {
        if (lastCall != null)
        {
            lastCall.DeprintPlayerVisibility(old);
            lastCall = null;
        }

        if (lastGraphicsCall != null)
        {
            lastGraphicsCall.DeprintGraphics(old);
            lastGraphicsCall = null;
        }
    }
}
