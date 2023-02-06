using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[Group("Connectors")]
public class ComplexConnectMachine : Machine
{
    public int NumberOfExtraEdges = 3;
    // Activate is called to start the machine
    public override IEnumerator Activate()
    {
        //Get all unconnected rooms
        List<Room> toConnect = generator.rooms.Where(r => !r.connected).ToList();
        foreach (Room r in toConnect)
        {
            r.CalculateOutermostPoint(generator.bounds);
        }

        List<Edge> edges = GraphFunctions.GenerateAllEdges(toConnect);
        List<Edge> unused = new List<Edge>();
        List<Edge> MST = GraphFunctions.GetMinimumSpanningTree(toConnect.Count, edges, ref unused);
        Debug.Assert(MST.Count == toConnect.Count - 1);
        yield return null;

        /*List<int> hull = GraphFunctions.GetHull(toConnect);
        yield return null;

        for (int i = 0; i < hull.Count; i++)
        {
            ConnectStandard(hull[i], hull[(i + 1) % hull.Count]);
            yield return null;
        }*/

        
        foreach (Edge edge in MST)
        {
            ConnectBresenham(edge.one, edge.two);
            /*
            int index = -1;// hull.IndexOf(edge.one);
            if (index == -1)
            {
                ConnectStandard(edge.one, edge.two);
            }
            else
            {
                if (edge.two != (hull[(index + hull.Count - 1) % hull.Count]) && edge.two != (hull[(index + 1) % hull.Count]))
                {
                    ConnectStandard(edge.one, edge.two);
                }
            }*/
            yield return null;
        }

        int currentlyAdded = 0;
        while (unused.Count > 0)
        {
            yield return null;
            
            int index = Mathf.Min(unused.Count - 1, toConnect.Count * 2 - 1); ;// Random.Range(0, edges.Count);
            Edge edge = unused[index];
            unused.RemoveAt(index);

            bool success = true;
            foreach (Edge other in MST)
            {
                if (GraphFunctions.Overlaps(edge, other, ref toConnect))
                {
                    success = false;
                    break;
                }
            }

            if (success)
            {
                ConnectBresenham(edge.one, edge.two);
                MST.Add(edge);
                currentlyAdded++;
                if (currentlyAdded >= NumberOfExtraEdges)
                {
                    break;
                }
            }
        }
    }

    public void ConnectStandard(int firstRoomIndex, int secondRoomIndex)
    {
        Room firstRoom = generator.rooms[firstRoomIndex];
        Room secondRoom = generator.rooms[secondRoomIndex];

        //Draw simple connection
        if (UnityEngine.Random.Range(0, 2) == 0)
        {
            Vector2Int corner = new Vector2Int(secondRoom.center.x, firstRoom.center.y);
            //Do x first
            for (int x = Mathf.Min(firstRoom.center.x, secondRoom.center.x); x <= Mathf.Max(firstRoom.center.x, secondRoom.center.x); x++)
            {
                Vector2Int loc = new Vector2Int(x, corner.y);
                if ((!secondRoom.Contains(loc) && !firstRoom.Contains(loc)) || generator.map[loc.x, loc.y] == 0)
                {
                    generator.map[x, corner.y] = 1;
                }
            }

            for (int y = Mathf.Min(firstRoom.center.y, secondRoom.center.y); y <= Mathf.Max(firstRoom.center.y, secondRoom.center.y); y++)
            {
                Vector2Int loc = new Vector2Int(corner.x, y);
                if ((!secondRoom.Contains(loc) && !firstRoom.Contains(loc)) || generator.map[loc.x, loc.y] == 0)
                {
                    generator.map[corner.x, y] = 1;
                }
            }
        }
        else
        {
            //Do y first
            Vector2Int corner = new Vector2Int(firstRoom.center.x, secondRoom.center.y);

            for (int y = Mathf.Min(firstRoom.center.y, secondRoom.center.y); y <= Mathf.Max(firstRoom.center.y, secondRoom.center.y); y++)
            {
                Vector2Int loc = new Vector2Int(corner.x, y);
                if ((!secondRoom.Contains(loc) && !firstRoom.Contains(loc)) || generator.map[loc.x, loc.y] == 0)
                {
                    generator.map[corner.x, y] = 1;
                }
            }

            //Do x first
            for (int x = Mathf.Min(firstRoom.center.x, secondRoom.center.x); x <= Mathf.Max(firstRoom.center.x, secondRoom.center.x); x++)
            {
                Vector2Int loc = new Vector2Int(x, corner.y);
                if ((!secondRoom.Contains(loc) && !firstRoom.Contains(loc)) || generator.map[loc.x, loc.y] == 0)
                {
                    generator.map[x, corner.y] = 1;
                }
            }
        }
    }

    public void ConnectBresenham(int firstRoomIndex, int secondRoomIndex)
    {
        Room firstRoom = generator.rooms[firstRoomIndex];
        Room secondRoom = generator.rooms[secondRoomIndex];

        List<Vector2Int> results = Bresenham.GetPointsOnLine(firstRoom.center, secondRoom.center).ToList();
        foreach (Vector2Int spot in results)
        {
            if (generator.map[spot.x, spot.y] == 0)
            {
                generator.map[spot.x, spot.y] = 1;
            }
        }
    }
}
