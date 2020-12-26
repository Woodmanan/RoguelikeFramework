using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using Priority_Queue;

public class Path
{
    private Stack<Vector2Int> locations;
    private float cost;

    public Vector2Int Pop()
    {
        return locations.Pop();
    }

    public Vector2Int Peek()
    {
        return locations.Peek();
    }

    public void Push(Vector2Int i)
    {
        locations.Push(i);
    }

    public void Clear()
    {
        locations.Clear();
    }

    public int Count()
    {
        return locations.Count;
    }

    public float Cost()
    {
        return cost;
    }

    

    public Path(Stack<Vector2Int> locations, float cost)
    {
        this.locations = locations;
        this.cost = cost;
    }

    public Path(Stack<Vector2Int> locations)
    {
#if UNITY_EDITOR
        Debug.LogError("Very expensive constructor called for path. This is generally unnecessary, as Pathfinding.cs can provide the same thing.");
#endif
        float newCost = 0.0f;
        Map m = Map.singleton;
        foreach (Vector2Int pos in this.locations)
        {
            newCost += m.MovementCostAt(pos);
        }

        this.locations = locations;
        this.cost = newCost;
    }
}


public static class Pathfinding
{
    private static bool[,] alreadyChecked;
    private static int width = 0;
    private static int height = 0;
    private static float[,] costMap;
    private static readonly float sqrtTwo = Mathf.Sqrt(2.0f);

    //This controls how the algorithm searches! Currently set for Chebyshev space, or 8 way movement
    private static Space space = Space.Chebyshev;

    //Max nodes should be checked; may not be enough
    private static FastPriorityQueue<PathTile> frontier = new FastPriorityQueue<PathTile>(10000);
    
    
    public class PathTile : FastPriorityQueueNode
    {
        public Vector2Int loc;
        public float cost;

        public PathTile(Vector2Int loc, float cost)
        {
            this.loc = loc;
            this.cost = cost;
        }
    }

    public static Path FindPath(Vector2Int start, Vector2Int end)
    {
        //Preliminary checks, resize board and confirm valid movements
        RebuildChecked();
        if (!(InBounds(start) && InBounds(end)))
        {
            return new Path(new Stack<Vector2Int>(), 0.0f);
        }
        
        //Cleared to move forward, do expensive reset op.
        ClearSeenFlags();
        frontier.Clear();
        frontier.Enqueue(new PathTile(start, 0), 0);

        return PerformSearch();
    }

    public static Path PerformSearch()
    {
        Map m = Map.singleton;
        while (true)
        {
            if (frontier.Count == 0)
            {
                return new Path(new Stack<Vector2Int>(), 0.0f);
            }
            else
            {
                PathTile current = frontier.Dequeue();
                
                //Have we already added this tile?
                if (alreadyChecked[current.loc.x, current.loc.y])
                {
                    //By nature of the algorithm, we can't possibly be faster than whoever already saw it
                    continue;
                }

                alreadyChecked[current.loc.x, current.loc.y] = true;
                costMap[current.loc.x, current.loc.y] = current.cost;

                for (int i = -1; i <= 1; i++)
                {
                    for (int j = -1; j <= 1; j++)
                    {
                        //Skip middle!
                        if (i == 0 && j == 0)
                        {
                            continue;
                        }
                        
                        //Create corner calculation, skip if Manhattan
                        bool isCorner = (i * j) == 0;
                        if (space == Space.Manhattan && isCorner)
                        {
                            continue;
                        }

                        Vector2Int newLoc = current.loc + new Vector2Int(i, j);
                        if (alreadyChecked[newLoc.x, newLoc.y])
                        {
                            continue;
                        }
                        
                        float newCost = 0.0f;
                        float newPriority = 0.0f;

                        if (isCorner && space == Space.Euclidean)
                        {
                            newCost = current.cost + sqrtTwo * m.MovementCostAt(newLoc);
                            newPriority = newCost;
                        }
                        else
                        { 
                            newCost = current.cost + m.MovementCostAt(newLoc);
                            newPriority = newCost;
                        }
                        
                        frontier.Enqueue(new PathTile(newLoc, newCost), newPriority);
                    }
                }


            }
        }
    }
    
    

    private static void RebuildChecked()
    {
        Map m = Map.singleton;
        if (width != m.width || height != m.height)
        {
            alreadyChecked = new bool[m.width,m.height];
            width = m.width;
            height = m.height;
        }
    }

    private static void ClearSeenFlags()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                alreadyChecked[i, j] = false;
            }
        }
    }

    private static bool InBounds(Vector2Int position)
    {
        return (position.x >= 0 && position.x < width && position.y >= 0 && position.y < height);
    }
}
