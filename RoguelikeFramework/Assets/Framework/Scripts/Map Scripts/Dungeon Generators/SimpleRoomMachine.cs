using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Machine", menuName = "Dungeon Generator/Machines/Simple Room Machine", order = 1)]
public class SimpleRoomMachine : Machine
{
    public int numRooms;
    public int attemptsBeforeFail;
    public List<Room> rooms;
    public List<Room> placedRooms;

    override public IEnumerator Activate()
    {
        placedRooms = new List<Room>();
        int failureCount = 0;
        while (failureCount < attemptsBeforeFail)
        {
            //Get a room (you two)
            Room current = Instantiate(rooms[Random.Range(0, rooms.Count)]);
            yield return null;
            current.Setup();

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
                failureCount = 0;
                placedRooms.Add(current);
                if (placedRooms.Count == numRooms)
                {
                    break;
                }
            }
            else
            {
                if (failureCount == attemptsBeforeFail)
                {
                    break;
                }
            }
        }

        foreach (Room r in placedRooms)
        {
            r.Write(this.generator);
            this.generator.rooms.Add(r);
        }
    }
}
