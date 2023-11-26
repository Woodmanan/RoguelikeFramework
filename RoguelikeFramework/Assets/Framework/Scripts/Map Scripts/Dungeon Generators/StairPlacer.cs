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
    public Branch fromBranch;
    public int fromLevel = -1;
    public Stair toStair;
    public Vector2Int toLocation;
    public int toLevel = -1;
    public Branch toBranch;
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
        
        if (roomsToConnect.Count == 0)
        {
            Debug.LogError("You must have some rooms for stairs! Adding in a backup.");
            Room room = new Room();
            room.size = generator.bounds;
            room.SetPosition(Vector2Int.zero);
            roomsToConnect.Add(room);
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
                }
            }
        }

        int roomIndex = 0;

        int maxUp = Mathf.Min(generator.desiredInStairs.Count, inConnections.Count);

        for (int i = 0; i < maxUp; i++)
        {
            LevelConnection connection = inConnections[i];
            Vector2Int loc = generator.desiredInStairs[i];
            connection.toLocation = loc;
            connection.toLevel = index;

            if (!connection.oneWay)
            {
                generator.map[loc.x, loc.y] = stairIndex;
            }
        }

        foreach (LevelConnection connection in inConnections.Skip(maxUp))
        {
            Room r = roomsToConnect[roomIndex];
            roomIndex = (roomIndex + 1) % roomsToConnect.Count;
            Vector2Int loc = r.GetOpenSpace(1, generator.map);
            connection.toLocation = loc;
            connection.toLevel = index;

            if (!connection.oneWay) //Do we need a landing stair?
            {
                generator.map[loc.x, loc.y] = stairIndex;
            }
        }

        int maxDown = Mathf.Min(generator.desiredOutStairs.Count, outConnections.Count);

        for (int i = 0; i < maxDown; i++)
        {
            LevelConnection connection = outConnections[i];
            Vector2Int loc = generator.desiredOutStairs[i];

            connection.fromLocation = loc;
            connection.fromLevel = index;
            generator.map[loc.x, loc.y] = stairIndex;
        }

        foreach (LevelConnection connection in outConnections.Skip(maxDown))
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
