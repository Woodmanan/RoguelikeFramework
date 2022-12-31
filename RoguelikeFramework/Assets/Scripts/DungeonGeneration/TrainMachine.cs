using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[Group("Branch/Western")]
public class TrainMachine : Machine
{
    public List<Room> mustAdd;
    public List<Room> roomOptions;
    public RandomNumber gap;
    public int buffer;

    // Activate is called to start the machine
    public override IEnumerator Activate()
    {
        foreach (Room r in roomOptions)
        {
            r.Setup();
            r.forciblyWritesWalls = true;
        }
        List<Room> addedRooms = new List<Room>();
        for (int i = 0; i < generator.bounds.x; i++)
        {
            for (int j = 0; j < generator.bounds.y; j++)
            {
                generator.map[i, j] = 4; //Set to "Open air" tile
            }
            yield return null;
        }

        int currentY = buffer;
        while (true)
        {
            yield return null;
            if (currentY != buffer)
            {
                currentY += gap.Evaluate();
            }

            Room roomToAdd = GetRoomWithinHeight(currentY);
            yield return null;
            if (roomToAdd == null) break;
            Vector2Int startPos = new Vector2Int((generator.bounds.x - roomToAdd.size.x) / 2, currentY);
            roomToAdd.SetPosition(startPos);
            roomToAdd.Write(generator);
            yield return null;
            currentY += roomToAdd.size.y;
            addedRooms.Add(roomToAdd);
        }

        int x = generator.bounds.x / 2;
        int maxY = addedRooms[addedRooms.Count - 1].center.y;

        //Add in thew missing tiles
        for (int y = buffer + 1; y < maxY; y++)
        {
            int below = generator.map[x, y - 1];
            int above = generator.map[x, y + 1];
            int current = generator.map[x, y];

            bool openBelow = (below == 4 && above == 1);
            bool openAbove = (above == 4 && below == 1);

            bool connects = (current == 0) && (openBelow || openAbove);
            if (connects)
            {
                generator.map[x, y] = 3;
            }
        }

        //Connect with rail lines
        for (int y = buffer + 1; y < maxY; y++)
        {
            if (generator.map[x,y] == 4)
            {
                generator.map[x, y] = 1;
            }
        }

        //Create in and out stairs
        generator.desiredInStairs.Add(addedRooms[0].center);
        generator.desiredOutStairs.Add(addedRooms[addedRooms.Count - 1].center);

        generator.rooms.AddRange(addedRooms);
    }

    public Room GetRoomWithinHeight(float currentY)
    {
        Room roomToAdd = null;
        float available = generator.bounds.y - currentY - buffer;
        while (mustAdd.Count > 0)
        {
            roomToAdd = mustAdd[0];
            mustAdd.RemoveAt(0);
            if (roomToAdd.size.y < available)
            {
                roomToAdd = Room.Instantiate(roomToAdd);
                break;
            }
            roomToAdd = null;
        }

        if (!roomToAdd)
        {
            List<Room> options = roomOptions.Where(x => x.size.y < available).ToList();
            if (options.Count == 0) return null;
            roomToAdd = Room.Instantiate(options[RogueRNG.Linear(0, options.Count)]);
        }

        roomToAdd.Setup();
        return roomToAdd;
    }
}
