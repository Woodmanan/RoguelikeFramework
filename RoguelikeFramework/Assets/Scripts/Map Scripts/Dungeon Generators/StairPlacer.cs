using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

[Serializable]
public struct StairConnection
{
    public string connectsTo;
    public bool next;
    public int numConnections;
}

[CreateAssetMenu(fileName = "New Stairway", menuName = "Dungeon Generator/Machines/Stairway Placer", order = 2)]
public class StairPlacer : Machine
{
    [Header("Tile numbers")]
    public int upStairsIndex;
    public int downStairsIndex;

    [Header("Stair settings")]
    public List<StairConnection> stairsUp;
    public List<StairConnection> stairsDown;

    List<Vector2Int> ups = new List<Vector2Int>();
    List<Vector2Int> downs = new List<Vector2Int>();

    public List<int> excludeRoom;

    List<int> roomsToConnect = new List<int>();

    // Activate is called to start the machine
    public override void Activate()
    {
        ups.Clear();
        downs.Clear();
        roomsToConnect.Clear();
        for (int i = 0; i < generator.rooms.Count; i++)
        {
            roomsToConnect.Add(i);
        }

        foreach (int i in excludeRoom)
        {
            roomsToConnect.Remove(i);
        }

        //Shuffle!
        roomsToConnect = roomsToConnect.OrderBy(x => UnityEngine.Random.Range(int.MinValue, int.MaxValue)).ToList();
        Debug.Log($"Number of rooms: {roomsToConnect.Count}");

        int c = 0;
        //Place up stairs
        for (int i = 0; i < stairsUp.Count; i++)
        {
            StairConnection connection = stairsUp[i];
            for (int j = 0; j < connection.numConnections; j++)
            {
                Room r = generator.rooms[roomsToConnect[c]];
                Vector2Int loc = r.GetOpenSpace(1);

                generator.map[loc.x, loc.y] = upStairsIndex;

                ups.Add(loc);
                c++;
            }
        }

        //Shuffle!
        roomsToConnect = roomsToConnect.OrderBy(x => UnityEngine.Random.Range(int.MinValue, int.MaxValue)).ToList();
        Debug.Log($"Number of rooms: {roomsToConnect.Count}");

        c = 0;
        //Place down stairs
        //Place up stairs
        for (int i = 0; i < stairsDown.Count; i++)
        {
            Debug.Log($"Loop {i} of {stairsDown.Count}");
            StairConnection connection = stairsDown[i];
            for (int j = 0; j < connection.numConnections; j++)
            {
                Debug.Log($"Internal Loop {j} of {connection.numConnections}");
                Room r = generator.rooms[roomsToConnect[c]];
                Vector2Int loc = r.GetOpenSpace(1);

                Debug.Log("Right before here!");
                generator.map[loc.x, loc.y] = downStairsIndex;
                Debug.Log("Right after here!");

                downs.Add(loc);
                c++;
            }
        }

        Debug.Log("Finished!");
    }

    public override void PostActivation(Map m)
    {
        int c = 0;
        for (int i = 0; i < stairsUp.Count; i++)
        {
            for (int j = 0; j < stairsUp[i].numConnections; j++)
            {
                m.entrances.Add(ups[c]);

                Stair stairs = m.GetTile(ups[c]) as Stair;
                if (stairs)
                {
                    if (stairsUp[i].next)
                    {
                        stairs.connectsTo = m.depth - 1;
                    }
                    else
                    {
                        stairs.connectsTo = LevelLoader.singleton.GetDepthOf(stairsUp[i].connectsTo);
                    }
                }
                else
                {
                    Debug.LogError("Grabbed tile that was not stair!", this);
                }
                c++;
            }
        }

        c = 0;
        for (int i = 0; i < stairsDown.Count; i++)
        {
            for (int j = 0; j < stairsDown[i].numConnections; j++)
            {
                m.exits.Add(downs[c]);

                Stair stairs = m.GetTile(downs[c]) as Stair;
                if (stairs)
                {
                    if (stairsDown[i].next)
                    {
                        stairs.connectsTo = m.depth + 1;
                    }
                    else
                    {
                        stairs.connectsTo = LevelLoader.singleton.GetDepthOf(stairsDown[i].connectsTo);
                    }
                }
                else
                {
                    Debug.LogError("Grabbed tile that was not stair!", this);
                }
                c++;
            }
        }
    }
}
