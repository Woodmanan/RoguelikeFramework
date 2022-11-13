using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[Group("Branch Secrets")]
public class ClockSecretMachine : Machine
{
    public const int upIndex = 3;
    public const int downIndex = 4;
    public const int leftIndex = 5;
    public const int rightIndex = 6;

    public RexRoom secretRoom;
    public int attempts = 100;
    // Activate is called to start the machine
    public override IEnumerator Activate()
    {
        List<Room> placedRooms = generator.rooms;
        int failureCount = 0;
        bool placed = false;

        Room current = ScriptableObject.Instantiate(secretRoom);
        yield return null;
        current.Setup();
        failureCount = 0;
        while (failureCount < attempts)
        {
            //Set it's position randomly, keeping it in bounds.
            Vector2Int placeBounds = this.size - current.size;
            Vector2Int newStart = new Vector2Int(Random.Range(1, placeBounds.x - 1), Random.Range(1, placeBounds.y - 1));
            newStart += this.start;
            current.SetPosition(newStart);

            bool success = true;
            foreach (Room r in placedRooms)
            {
                if (current.OverlapsExtra(r))
                {
                    success = false;
                    failureCount++;
                    break;
                }
            }
            if (success)
            {
                placed = true;
                break;
            }
            yield return null;
        }


        if (placed)
        {
            /*int closest = 0;
            for (int i = 1; i < generator.rooms.Count; i++)
            {
                if (Vector2.Distance(generator.rooms[i].center, current.center) < Vector2.Distance(generator.rooms[closest].center, current.center))
                {
                    closest = i;
                }
                yield return null;
            }*/

            this.generator.rooms.Add(current);

            //ConnectConveyors(generator.rooms.Count - 1, closest);
            current.Write(this.generator);
        }
    }

    public void ConnectConveyors(int firstRoomIndex, int secondRoomIndex)
    {
        //Swap the rooms to maintain some preconditions

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
