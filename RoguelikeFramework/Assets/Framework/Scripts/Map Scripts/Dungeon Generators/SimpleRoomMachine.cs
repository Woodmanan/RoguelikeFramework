using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Group("Room Placers")]
public class SimpleRoomMachine : Machine
{
    public int numRooms;
    public int attemptsPerRoom;
    public List<Room> rooms;
    public List<Room> placedRooms;

    override public IEnumerator Activate()
    {
        placedRooms = new List<Room>();
        int failureCount = 0;
        for (int i = 0; i < numRooms; i++)
        {
            //Get a room (you two)
            Room current = ScriptableObject.Instantiate(rooms[Random.Range(0, rooms.Count)]);
            yield return null;
            current.Setup();
            failureCount = 0;
            while (failureCount < attemptsPerRoom)
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
                    placedRooms.Add(current);
                    break;
                }
                yield return null;
            }
        }

        foreach (Room r in placedRooms)
        {
            r.Write(this.generator);
            this.generator.rooms.Add(r);
            yield return null;
        }
    }
}
