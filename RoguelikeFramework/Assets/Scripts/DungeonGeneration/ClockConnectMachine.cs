using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[Group("Connectors")]
public class ClockConnectMachine : Machine
{
    public const int upIndex = 3;
    public const int downIndex = 4;
    public const int leftIndex = 5;
    public const int rightIndex = 6;
    bool clockwise = true;

    public bool layEmptyHallways = false;

    // Activate is called to start the machine
    public override IEnumerator Activate()
    {
        clockwise = (RogueRNG.Linear(0, 2) == 0);
        //Get all unconnected rooms
        List<Room> toConnect = generator.rooms.Where(r => !r.connected).ToList();
        foreach (Room r in toConnect)
        {
            r.CalculateOutermostPoint(generator.bounds);
        }

        List<Edge> MST = GraphFunctions.GetMinimumSpanningTree(toConnect.Count, GraphFunctions.GenerateAllEdges(toConnect));
        Debug.Assert(MST.Count == toConnect.Count - 1);
        yield return null;

        List<int> hull = GraphFunctions.GetHull(toConnect);
        yield return null;

        for (int i = 0; i < hull.Count; i++)
        {
            if (layEmptyHallways)
            {
                ConnectStandard(hull[i], hull[(i + 1) % hull.Count]);
            }
            else
            {
                ConnectConveyors(hull[i], hull[(i + 1) % hull.Count]);
            }
            yield return null;
        }

        
        foreach (Edge edge in MST)
        {
            int index = hull.IndexOf(edge.one);
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
            }
            yield return null;
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

    public void ConnectConveyors(int firstRoomIndex, int secondRoomIndex)
    {
        //Swap the rooms to maintain some preconditions
        if (clockwise)
        {
            int hold = firstRoomIndex;
            firstRoomIndex = secondRoomIndex;
            secondRoomIndex = hold;
        }

        Room firstRoom = generator.rooms[firstRoomIndex];
        Room secondRoom = generator.rooms[secondRoomIndex];

        int horizontal = rightIndex;
        int vertical = upIndex;

        if (firstRoom.center.x < secondRoom.center.x)
        {
            horizontal = leftIndex;
        }

        if (firstRoom.center.y < secondRoom.center.y)
        {
            vertical = downIndex;
        }

        //Draw simple connection
        if (UnityEngine.Random.Range(0, 2) == 0)
        {
            Vector2Int corner = new Vector2Int(secondRoom.center.x, firstRoom.center.y);
            //Do x first
            for (int x = Mathf.Min(firstRoom.center.x, secondRoom.center.x); x <= Mathf.Max(firstRoom.center.x, secondRoom.center.x); x++)
            {
                Vector2Int loc = new Vector2Int(x, corner.y);
                if (generator.map[loc.x, loc.y] == 0)
                {
                    generator.map[x, corner.y] = horizontal;
                }
            }

            for (int y = Mathf.Min(firstRoom.center.y, secondRoom.center.y); y <= Mathf.Max(firstRoom.center.y, secondRoom.center.y); y++)
            {
                Vector2Int loc = new Vector2Int(corner.x, y);
                if (generator.map[loc.x, loc.y] == 0)
                {
                    generator.map[corner.x, y] = vertical;
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
                if (generator.map[loc.x, loc.y] == 0)
                {
                    generator.map[corner.x, y] = vertical;
                }
            }

            //Do x first
            for (int x = Mathf.Min(firstRoom.center.x, secondRoom.center.x); x <= Mathf.Max(firstRoom.center.x, secondRoom.center.x); x++)
            {
                Vector2Int loc = new Vector2Int(x, corner.y);
                if (generator.map[loc.x, loc.y] == 0)
                {
                    generator.map[x, corner.y] = horizontal;
                }
            }
        }
    }
}
