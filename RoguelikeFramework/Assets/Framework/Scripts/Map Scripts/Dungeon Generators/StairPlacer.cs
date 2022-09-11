using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

[Serializable]
public class LevelConnection
{
    public string from;
    public string to;
    public Stair fromStair;
    public Vector2Int fromLocation;
    public int fromLevel;
    public Stair toStair;
    public Vector2Int toLocation;
    public int toLevel;
    public bool oneWay = false;

    public LevelConnection(string from, string to)
    {
        this.from = from;
        this.to = to;
    }

    public LevelConnection(string from, string to, bool oneWay)
    {
        this.from = from;
        this.to = to;
        this.oneWay = oneWay;
    }
}

public class StairPlacer
{
    [Header("Tile numbers")]
    public const int stairIndex = 2;

    List<Room> roomsToConnect = new List<Room>();

    List<LevelConnection> inConnections = new List<LevelConnection>();
    List<LevelConnection> outConnections = new List<LevelConnection>();

    // Activate is called to start the machine
    public void Activate(World world, DungeonGenerator generator, int index)
    {
        roomsToConnect.Clear();
        foreach (Room r in generator.rooms)
        {
            if (r.acceptsStairs)
            {
                roomsToConnect.Add(r);
            }
        }

        //Shuffle!
        roomsToConnect = roomsToConnect.OrderBy(x => UnityEngine.Random.Range(int.MinValue, int.MaxValue)).ToList();

        { //Generate levels up
            foreach (LevelConnection connection in world.connections)
            {
                if (connection.to.Equals(generator.name))
                {
                    inConnections.Add(connection);
                }
            }
        }

        { //Generate levels down
            foreach (LevelConnection connection in world.connections)
            {
                if (connection.from.Equals(generator.name))
                {
                    outConnections.Add(connection);
                    Debug.Log($"Floor {index} registered an out-connection: {connection.from} to {connection.to}{(connection.oneWay ? ": One Way" : "")}");
                }
            }
        }

        int roomIndex = 0;

        foreach (LevelConnection connection in inConnections)
        {
            Room r = roomsToConnect[roomIndex];
            roomIndex = (roomIndex + 1) % roomsToConnect.Count;
            Vector2Int loc = r.GetOpenSpace(1, generator.map);
            connection.toLocation = loc;
            connection.toLevel = index;
            Debug.Log($"Floor {index} registered an in-connection: {connection.from} to {connection.to}{(connection.oneWay ? ": One Way" : "")}");
            if (!connection.oneWay) //Do we need a landing stair?
            {
                generator.map[loc.x, loc.y] = stairIndex;
                Debug.Log($"Placed a stair for it at {loc}");
            }
            else
            {
                Debug.Log($"Placed no stair, but did find a suitable landing zone at {loc}");
            }

            
        }

        foreach (LevelConnection connection in outConnections)
        {
            Room r = roomsToConnect[roomIndex];
            roomIndex = (roomIndex + 1) % roomsToConnect.Count;
            Vector2Int loc = r.GetOpenSpace(1, generator.map);
            connection.fromLocation = loc;
            connection.fromLevel = index;
            generator.map[loc.x, loc.y] = stairIndex;
        }
    }

    public void SetupStairTiles(Map m)
    {
        foreach (LevelConnection connection in inConnections)
        {
            if (!connection.oneWay)
            {
                Stair stair = m.GetTile(connection.toLocation) as Stair;
                connection.toStair = stair;
                stair.SetConnection(connection, false);
            }
            else
            {
                m.GetTile(connection.toLocation).color = Color.red;
            }
        }

        m.entrances = inConnections;

        foreach (LevelConnection connection in outConnections)
        {
            Stair stair = m.GetTile(connection.fromLocation) as Stair;
            connection.fromStair = stair;
            stair.SetConnection(connection, true);
        }

        m.exits = outConnections;
    }
}
