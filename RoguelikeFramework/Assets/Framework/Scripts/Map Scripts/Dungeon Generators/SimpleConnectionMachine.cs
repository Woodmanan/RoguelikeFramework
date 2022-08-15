using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

[CreateAssetMenu(fileName = "New Connector", menuName = "Dungeon Generator/Machines/Simple Connector", order = 2)]
public class SimpleConnectionMachine : Machine
{


    // Activate is called to start the machine
    public override void Activate()
    {
        //Get all unconnected rooms
        List<Room> toConnect = generator.rooms.FindAll(x=>!x.connected);
        List<Room> connected = generator.rooms.FindAll(x=>x.connected);

        //Set up some preconditions, if they don't already exist.
        if (connected.Count == 0 && toConnect.Count > 0)
        {
            toConnect[0].connected = true;
            connected.Add(toConnect[0]);
            toConnect.RemoveAt(0);
        }

        foreach (Room r in toConnect)
        {
            //Find nearest room
            connected.Sort(
                (x,y)=> Vector2Int.Distance(r.center, x.center).CompareTo(Vector2Int.Distance(r.center, y.center)));
            Room nearest = connected[0];

            //Draw simple connection
            Vector2Int diff = (nearest.center - r.center);
            if (UnityEngine.Random.Range(0, 2) == 0)
            {
                Vector2Int corner = new Vector2Int(nearest.center.x, r.center.y);
                //Do x first
                for (int x = Math.Min(r.center.x, nearest.center.x); x <= Math.Max(r.center.x, nearest.center.x); x++)
                {
                    generator.map[x,corner.y] = 1;
                }

                for (int y = Math.Min(r.center.y, nearest.center.y); y <= Math.Max(r.center.y, nearest.center.y); y++)
                {
                    generator.map[corner.x, y] = 1;
                }
            }
            else
            {
                //Do y first
                Vector2Int corner = new Vector2Int(r.center.x, nearest.center.y);

                for (int y = Math.Min(r.center.y, nearest.center.y); y <= Math.Max(r.center.y, nearest.center.y); y++)
                {
                    generator.map[corner.x, y] = 1;
                }

                //Do x first
                for (int x = Math.Min(r.center.x, nearest.center.x); x <= Math.Max(r.center.x, nearest.center.x); x++)
                {
                    generator.map[x,corner.y] = 1;
                }
            }

            //Flip connected bit
            r.connected = true;
            connected.Add(r);
        }
    }
}
