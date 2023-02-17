using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public struct Edge
{
    public int one;
    public int two;
    public float weight;

    public Edge(int one, int two, float weight)
    {
        this.one = one;
        this.two = two;
        this.weight = weight;
    }
}

public class GraphFunctions
{
    public static List<Edge> GenerateAllEdges(List<Room> nodes)
    {
        List<Edge> edges = new List<Edge>();

        for (int i = 0; i < nodes.Count - 1; i++)
        {
            for (int j = i + 1; j < nodes.Count; j++)
            {
                float dist = Vector2Int.Distance(nodes[i].center, nodes[j].center);
                edges.Add(new Edge(i, j, dist));
            }
        }

        return edges;
    }

    public static List<int> GetHull(List<Room> nodes)
    {
        //If we can't run, just return the points
        if (nodes.Count < 3)
        {
            return Enumerable.Range(0, nodes.Count).ToList();
        }

        List<int> hull = new List<int>();

        //Find leftmost point (by y in case of ties)
        int bottom = 0;
        for (int i = 1; i < nodes.Count; i++)
        {
            if (nodes[i].outermostPoint.x < nodes[bottom].outermostPoint.x)
            {
                bottom = i;
            }
        }

        int current = bottom;
        int next = -1;
        do
        {
            hull.Add(current);
            next = (current + 1) % nodes.Count;
            for (int i = 0; i < nodes.Count; i++)
            {
                //Magic part! If we find a node clockwise of our next node, update next to that instead.
                //At the end, next should be our most clockwise node.
                if (Orientation(nodes[current].outermostPoint, nodes[i].outermostPoint, nodes[next].outermostPoint) == 2)
                {
                    next = i;
                }
            }
            current = next;
        } while (current != bottom); //Loop until we hit our start point again.

        return hull;
    }

    //Credit for this function to https://iq.opengenus.org/gift-wrap-jarvis-march-algorithm-convex-hull/
    //I did not come up with this clever math
    public static int Orientation(Vector2Int p, Vector2Int q, Vector2Int r)
    {
        int slopes = (q.y - p.y) * (r.x - q.x) - 
                     (q.x - p.x) * (r.y - q.y);
        if (slopes == 0) return 0;
        if (slopes > 0)
        {
            //Clockwise!
            return 1;
        }
        else
        {
            //Counter clockwise!
            return 2;
        }
    }

    public static List<Edge> GetMinimumSpanningTree(int nodes, List<Edge> edges)
    {
        List<Edge> unused = null;
        return GetMinimumSpanningTree(nodes, edges, ref unused);
    }

    public static List<Edge> GetMinimumSpanningTree(int nodes, List<Edge> edges, ref List<Edge> unused)
    {
        List<Edge> MST = new List<Edge>();

        //Construct working set, sorted by weight
        List<Edge> working = new List<Edge>(edges);
        working.Sort((x, y) => x.weight.CompareTo(y.weight));

        List<int> parents = Enumerable.Range(0, nodes).ToList();

        foreach (Edge edge in working)
        {
            if (GetParent(edge.one, ref parents) != GetParent(edge.two, ref parents))
            {
                //Edge we can use!
                MST.Add(edge);

                //Connect parents to one node, so we know they're in the same tree
                parents[GetParent(edge.one, ref parents)] = GetParent(edge.two, ref parents);
            }
            else
            {
                if (unused != null)
                {
                    unused.Add(edge);
                }
            }
        }

        return MST;
    }

    //Helper function for a SUPER weird data structure
    public static int GetParent(int index, ref List<int> parents)
    {
        int workingIndex = index;
        while (parents[workingIndex] != workingIndex)
        {
            workingIndex = parents[workingIndex];
        }

        //Optimization - skip us up the ladder, so we're O(1) next time without an update.
        parents[index] = workingIndex;
        return workingIndex;
    }

    public static bool Overlaps(Edge one, Edge two, ref List<Room> nodes)
    {
        Vector2Int p = nodes[one.one].center;
        Vector2Int r = nodes[one.two].center - p;

        Vector2Int q = nodes[two.one].center;
        Vector2Int s = nodes[two.two].center - q;

        Vector2Int qmp = q - p;

        float rxs = r.Cross(s);
        float qmpxr = qmp.Cross(r);
        float qmpxs = qmp.Cross(s);

        if (rxs == 0)
        {
            if (qmpxr == 0)
            {
                //Colinear! Any overlap means an intersection now.
                Rect bboxOne = new Rect(p, r);
                Rect bboxTwo = new Rect(q, s);
                return bboxOne.Overlaps(bboxTwo);
            }
            else
            {
                //Parallel and non-intersecting
                return false;
            }
        }
        else
        {
            float t = qmpxs / rxs;
            float u = qmpxr / rxs;

            return (t > 0 && t < 1 && u > 0 && u < 1);
        }
    }
}
