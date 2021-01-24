using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using Priority_Queue;

//This file handles all of the pathfinding requests of the game. See below for more implementation information.

//Object that is returned by the request - essentially a queue with some extra goodies
//AI's can just Pop() new places to move off of the stack
public class Path
{
    private Stack<Vector2Int> locations;
    private float cost;

    public Vector2Int destination;

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

    public IEnumerator<Vector2Int> GetEnumerator()
    {
        return locations.GetEnumerator();
    }

    

    public Path(Stack<Vector2Int> locations, float cost)
    {
        this.locations = locations;
        this.cost = cost;
        this.destination = locations.LastOrDefault();
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
        this.destination = locations.LastOrDefault();
    }
}

/*
 * A class for doing A* pathfinding
 * This uses the classical A* algorithm, but will accept errors of .001 or less in the correct answer
 * if it results in a 'better looking' path. This shouldn't ever actually change anything, but makes
 * the paths produced really nice and predictable for gameplay purposes.
 *
 * To use this class, just call Pathfinding.FindPath(start, end) and it will return you a path object
 * that traverses those points, or one that has a negative cost otherwise.
 */
public static class Pathfinding
{
    private static bool[,] alreadyChecked;
    private static int width = 0;
    private static int height = 0;
    private static float[,] costMap;
    private static readonly float sqrtTwo = Mathf.Sqrt(2.0f);
    private static readonly float Epsilon = 0.001f; //Acceptable difference for cleaning up the path
    private static Vector2Int source;
    private static Vector2Int goal;

    
    //Max nodes should be checked; may not be enough. Currently supports up to 200x200
    private static FastPriorityQueue<PathTile> frontier = new FastPriorityQueue<PathTile>(40000);
    
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

        goal = end;
        source = start;

        return PerformSearch();
    }

    public static Path PerformSearch()
    {
        Map m = Map.singleton;
        while (true)
        {
            if (frontier.Count == 0)
            {
                return new Path(new Stack<Vector2Int>(), -1.0f);
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

                if (current.loc == goal)
                {
                    //Construct the path back
                    Stack<Vector2Int> path = new Stack<Vector2Int>();
                    Vector2Int searching = goal;
                    while (searching != source)
                    {
                        path.Push(searching);
                        Vector2Int check = Vector2Int.zero;
                        float cost = float.PositiveInfinity;
                        float rank = float.PositiveInfinity;

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
                                bool isCorner = (i * j) != 0;
                                if (Map.space == Space.Manhattan && isCorner)
                                {
                                    continue;
                                }


                                Vector2Int newCheck = searching + new Vector2Int(i,j);

                                if (newCheck.x < 0 || newCheck.x >= width || newCheck.y < 0 || newCheck.y >= height)
                                {
                                    continue;
                                }

                                float newCost = costMap[newCheck.x, newCheck.y];
                                float newRank = Heuristic(newCheck) * ReturnHeuristic(newCheck);
                                
                                if (alreadyChecked[newCheck.x, newCheck.y] && (newCost < cost))
                                {
                                    if ((newCost < cost))
                                    {
                                        cost = newCost;
                                        check = newCheck;
                                        rank = newRank;
                                    }
                                    else if (Mathf.Abs(newCost - cost) < Epsilon)
                                    {
                                        //Rank check, for points that are very close
                                        if (newRank < rank)
                                        {
                                            cost = newCost;
                                            check = newCheck;
                                            rank = newRank;
                                        }
                                    }
                                }
                            }
                        }
                        searching = check;
                    }
                    return new Path(path, current.cost);
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
                        bool isCorner = (i * j) != 0;
                        if (Map.space == Space.Manhattan && isCorner)
                        {
                            continue;
                        }

                        Vector2Int newLoc = current.loc + new Vector2Int(i, j);
                        
                        if (newLoc.x < 0 || newLoc.y < 0 || newLoc.x >= width || newLoc.y >= height || alreadyChecked[newLoc.x, newLoc.y])
                        {
                            continue;
                        }

                        //For now, skip things that are walls.
                        if (m.BlocksMovement(newLoc))
                        {
                            /*
                             * This behaviour is great in 99% of cases, but there's a slight misstep
                             * when the goal path is in a wall. Below is a quick check for that.
                             */
                            if (newLoc == goal)
                            {
                                frontier.Enqueue(new PathTile(newLoc, current.cost + 1.0f), current.cost + 1.0f);
                            }
                            continue;
                        }
                        
                        float newCost = 0.0f;
                        float newPriority = 0.0f;

                        if (isCorner && Map.space == Space.Euclidean)
                        {
                            newCost = current.cost + sqrtTwo * m.MovementCostAt(newLoc);
                            newPriority = newCost + Heuristic(newLoc);
                        }
                        else
                        {
                            newCost = current.cost + m.MovementCostAt(newLoc);
                            newPriority = newCost + Heuristic(newLoc);
                        }
                        frontier.Enqueue(new PathTile(newLoc, newCost), newPriority);
                    }
                }


            }
        }
    }
    
    private static float Heuristic(Vector2Int loc)
    {
        switch (Map.space)
        {
            case Space.Manhattan:
                return Mathf.Abs(loc.x - goal.x) + Mathf.Abs(loc.y - goal.y);
            case Space.Chebyshev:
                //return Mathf.Max(Mathf.Abs(loc.x - goal.x), Mathf.Abs(loc.y - goal.y));
            case Space.Euclidean:
                return (loc - goal).magnitude;
        }
        return 0;
    }

    //Seperate Heuristic, designed to create nice, straight lines back - Essentially manhattan
    private static float ReturnHeuristic(Vector2Int loc)
    {
        return Mathf.Abs(loc.x - source.x) + Mathf.Abs(loc.y - source.y);
    }

    private static void RebuildChecked()
    {
        Map m = Map.singleton;
        if (width != m.width || height != m.height)
        {
            alreadyChecked = new bool[m.width,m.height];
            costMap = new float[m.width, m.height];
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
                costMap[i,j] = float.PositiveInfinity;
            }
        }
    }

    private static bool InBounds(Vector2Int position)
    {
        return (position.x >= 0 && position.x < width && position.y >= 0 && position.y < height);
    }
}
