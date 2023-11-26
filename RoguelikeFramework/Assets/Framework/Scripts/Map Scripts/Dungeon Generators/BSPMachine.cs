using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[Group("Branch/Nightmare")]
public class BSPMachine : Machine
{
    public int maxWidth; //Written this way to prevent even numbers!
    public int MaxDepth = 1;

    [Range(0, 1)]
    public float chanceForHallway;

    public int minHallwayConnects = 2;

    public List<Room> roomsToAdd;

    List<Rect> roomOptions;

    // Activate is called to start the machine
    public override IEnumerator Activate()
    {
        roomsToAdd = roomsToAdd.Select(x => Room.Instantiate(x)).ToList();
        bool horizontal = Random.value > .5f;
        roomOptions = new List<Rect>();

        Rect bounds = new Rect(start + Vector2Int.one, size - 2 * Vector2Int.one);

        BSP(bounds, horizontal, 0);

        yield return null;

        for (int i = 0; i < roomOptions.Count; i++)
        {
            Rect r = roomOptions[i];
            r.size -= Vector2.one * 2;
            r.position += Vector2.one;

            FillRect(r);
            yield return null;
        }
    }

    public void BSP(Rect bounds, bool horizontal, int depth)
    {
        //int radius = Mathf.Clamp((MaxDepth - depth), 0, maxWidth);
        int radius = maxWidth;
        if (bounds.width <= radius || bounds.height <= radius)
        {
            return;
        }
        
        if (depth >= MaxDepth)
        {
            roomOptions.Add(bounds);
            return;
        }

        int width = 0;
        int pos = 0;

        if (horizontal)
        {
            width = Mathf.RoundToInt(bounds.width);
        }
        else
        {
            width = Mathf.RoundToInt(bounds.height);
        }

        width -= radius * 2; //Rounds to nearest double
        //pos = RogueRNG.Linear(0, width) + radius;
        pos = RogueRNG.Binomial(width, .5f);

        Rect left;
        Rect right;
        Rect hallway;
        Rect temp;
        if (horizontal)
        {
            (left, temp) = bounds.SplitOnX(pos - radius);
            (hallway, right) = temp.SplitOnX(radius * 2 + 1);
        }
        else
        {
            (left, temp) = bounds.SplitOnY(pos - radius);
            (hallway, right) = temp.SplitOnY(radius * 2 + 1);
        }

        for (int x = Mathf.RoundToInt(hallway.x); x < Mathf.RoundToInt(hallway.x + hallway.width); x++)
        {
            for (int y = Mathf.RoundToInt(hallway.y); y < Mathf.RoundToInt(hallway.y + hallway.height); y++)
            {
                generator.map[x, y] = 1;
            }
        }

        BSP(left, !horizontal, depth + 1);
        BSP(right, !horizontal, depth + 1);
    }

    public void FillRect(Rect rect)
    {
        List<Room> addedRooms = new List<Room>();
        List<Room> addedHallways = new List<Room>();

        //Skip inappropriately small rects
        if (rect.width < 5 || rect.height < 5)
        {
            return;
        }

        //Cough not that there's anything wrong with that'

        //Search for room addition that could fit
        Room toAdd = null;

        for (int i = 0; i < roomsToAdd.Count; i++)
        {
            Room r = roomsToAdd[i];
            if (r.GetSize().x < rect.size.x && r.GetSize().y < rect.size.y)
            {
                toAdd = r;
                roomsToAdd.RemoveAt(i);
                break;
            }
        }

        if (toAdd != null)
        {
            Rect firstRemainder, firstTemp, secondRemainder, roomRect;

            if (Random.value > .5f)
            {
                //Horizontal case
                if (Random.value > .5f)
                {
                    //We're the left
                    (firstTemp, firstRemainder) = BreakRectOnX(rect, toAdd.GetSize().x);
                }
                else
                {
                    //We're the right
                    (firstRemainder, firstTemp) = BreakRectOnX(rect, Mathf.RoundToInt(rect.size.x - toAdd.GetSize().x - 1));
                }

                //Split remainder vertically
                if (Random.value > .5f)
                {
                    //We're lower
                    (roomRect, secondRemainder) = BreakRectOnY(firstTemp, toAdd.GetSize().y);
                }
                else
                {
                    //We're upper
                    (secondRemainder, roomRect) = BreakRectOnY(firstTemp, Mathf.RoundToInt(firstTemp.size.y - toAdd.GetSize().y - 1));
                }

                toAdd.SetPosition(new Vector2Int((int)roomRect.position.x, (int)roomRect.position.y));

                SubdivideRect(firstRemainder, false, ref addedRooms, ref addedHallways);
                SubdivideRect(secondRemainder, true, ref addedRooms, ref addedHallways);
            }
            else
            {
                //Vertical first
                if (Random.value > .5f)
                {
                    //We're the bottom
                    (firstTemp, firstRemainder) = BreakRectOnY(rect, toAdd.GetSize().y);
                }
                else
                {
                    //We're the top
                    (firstRemainder, firstTemp) = BreakRectOnY(rect, Mathf.RoundToInt(rect.size.y - toAdd.GetSize().y - 1));
                }

                //Now split horizontally
                if (Random.value > .5f)
                {
                    //We're lower
                    (roomRect, secondRemainder) = BreakRectOnX(firstTemp, toAdd.GetSize().x);
                }
                else
                {
                    //We're upper
                    (secondRemainder, roomRect) = BreakRectOnX(firstTemp, Mathf.RoundToInt(firstTemp.size.x - toAdd.GetSize().x - 1));
                }

                toAdd.SetPosition(new Vector2Int((int)roomRect.position.x, (int)roomRect.position.y));

                SubdivideRect(firstRemainder, false, ref addedRooms, ref addedHallways);
                SubdivideRect(secondRemainder, true, ref addedRooms, ref addedHallways);
            }
        }
        else
        {
            SubdivideRect(rect, (Random.value > .5f), ref addedRooms, ref addedHallways);
        }

        if (toAdd)
        {
            toAdd.Write(generator);
        }
        foreach (Room r in addedRooms)
        {
            for (int x = r.start.x; x < r.end.x; x++)
            {
                for (int y = r.start.y; y < r.end.y; y++)
                {
                    generator.map[x, y] = 1;
                }
            }
        }

        foreach (Room r in addedHallways)
        {
            for (int x = r.start.x; x < r.end.x; x++)
            {
                for (int y = r.start.y; y < r.end.y; y++)
                {
                    generator.map[x, y] = 1;
                }
            }
        }

        { //Connect everything up!
            if (toAdd)
            {
                addedRooms.Add(toAdd);
            }
            if (!ConnectRoomsInRect(rect, addedHallways, addedRooms))
            {
                for (int x = Mathf.RoundToInt(rect.x); x < Mathf.RoundToInt(rect.x + rect.width); x++)
                {
                    for (int y = Mathf.RoundToInt(rect.y); y < Mathf.RoundToInt(rect.y + rect.height); y++)
                    {
                        generator.map[x, y] = 0;
                    }
                }
                //FUBAR - Redo this case, but skip retrying the same room.
                FillRect(rect);
                if (toAdd)
                {
                    roomsToAdd.Add(toAdd);
                }
                return;
            }
        }

        { //Final cleanup - fill hallways
            foreach (Room hall in addedHallways)
            {
                ShrinkHallway(hall);
            }
        }

        //Lastly, if all succeeds, add in the rooms we've spawned in.
        generator.rooms.AddRange(addedRooms);
    }

    //Given a whole bunch of adjacent rooms in a block, connect them internally and externally
    public bool ConnectRoomsInRect(Rect bounds, List<Room> hallways, List<Room> roomsToConnect)
    {
        List<Room> totalRooms = new List<Room>();
        totalRooms.AddRange(hallways);
        totalRooms.AddRange(roomsToConnect);

        //Shuffle connection list!
        roomsToConnect = roomsToConnect.OrderBy(x => Random.value).ToList();

        //-1 means connected to the outside. We want EVERYONE to end up there, because that means walkable.
        List<int> parents = Enumerable.Range(0, totalRooms.Count).ToList();

        { //Connect someone to the OUTSIDE WORLD!
            Room total = new Room(bounds);
            //total.size = total.size - 2 * Vector2Int.one;
            //total.SetPosition(total.size + Vector2Int.one);
            List<Vector2Int> positions = GetValidCuts(total);
            while (positions.Count > 0)
            {
                int index = RogueRNG.Linear(0, positions.Count);
                Vector2Int chosen = positions[index];
                positions.RemoveAt(index);
                if (TryMakeCut(chosen, ref totalRooms, ref parents))
                {
                    break;
                }
            }

            if (!parents.Contains(-1))
            {
                Debug.LogError("Somehow made a hull cut without a parent!");
                return false;
            }
        }

        
        for (int i = 0; i < hallways.Count; i++)
        {
            for (int c = 0; c < minHallwayConnects; c++)
            {
                List<Vector2Int> positions = GetValidCuts(hallways[i]);

                while (positions.Count > 0)
                {
                    int index = RogueRNG.Linear(0, positions.Count);
                    Vector2Int chosen = positions[index];
                    positions.RemoveAt(index);
                    if (TryMakeCut(chosen, ref totalRooms, ref parents))
                    {
                        break;
                    }
                }
            }
        }
        bool success = false;
        //While unconnected rooms exist
        for (int count = 0; count < totalRooms.Count; count++)
        {
            Room roomToConnect = null;
            //We know there is at least 1 unconnected room - find it (and it's parent)
            for (int i = parents.Count - 1; i >= 0; i--)
            {
                if (GetParent(i, ref parents) == i)
                {
                    //Traverse up the chain - encourages deeper room structure
                    roomToConnect = totalRooms[i];
                    break;
                }
            }

            if (roomToConnect == null)
            {
                success = true;
                break;
            }

            List<Vector2Int> positions = GetValidCuts(roomToConnect);

            while (positions.Count > 0)
            {
                int index = RogueRNG.Linear(0, positions.Count);
                Vector2Int chosen = positions[index];
                positions.RemoveAt(index);

                if (TryMakeCut(chosen, ref totalRooms, ref parents))
                {
                    break;
                }
            }
        }

        return success;
    }

    public bool TryMakeCut(Vector2Int position, ref List<Room> rooms, ref List<int> parents)
    {
        (int left, int right) = GetBorderingRooms(position, rooms);

        int leftParent = GetParent(left, ref parents);
        int rightParent = GetParent(right, ref parents);

        //Connect the two to lowest parent
        if (leftParent != rightParent)
        {
            //Cut the door into the map
            generator.map[position.x, position.y] = 3;

            int lower = Mathf.Min(leftParent, rightParent);
            if (leftParent >= 0)
            {
                parents[leftParent] = lower;
            }
            if (rightParent >= 0)
            {
                parents[rightParent] = lower;
            }
            if (left > 0)
            {
                parents[left] = lower;
            }
            if (right > 0)
            {
                parents[right] = lower;
            }

            return true;
        }

        return false;
    }

    public (int, int) GetBorderingRooms(Vector2Int position, List<Room> roomsToConnect)
    {
        bool horizontal = (generator.map[position.x + 1, position.y] == 1);

        int roomOneIndex = -1;
        int roomTwoIndex = -1;

        Vector2Int left;
        Vector2Int right;

        if (horizontal)
        {
            left = position + Vector2Int.left;
            right = position + Vector2Int.right;
        }
        else
        {
            left = position + Vector2Int.down;
            right = position + Vector2Int.up;
        }

        for (int i = 0; i < roomsToConnect.Count; i++)
        {
            if (roomsToConnect[i].Contains(left))
            {
                roomOneIndex = i;
            }

            if (roomsToConnect[i].Contains(right))
            {
                roomTwoIndex = i;
            }
        }

        return (roomOneIndex, roomTwoIndex);
    }

    //Cleans up hallways by removing useless dead ends.
    public void ShrinkHallway(Room hallway)
    {
        Fill(hallway.start);
        Fill(hallway.end - Vector2Int.one);
    }

    public void Fill(Vector2Int position)
    {
        //Skip if wall or door.
        if (generator.map[position.x, position.y] != 1 &&
            generator.map[position.x, position.y] != 3) return;

        int[] positions = new int[4];
        positions[0] = generator.map[position.x + 1, position.y];
        positions[1] = generator.map[position.x - 1, position.y];
        positions[2] = generator.map[position.x, position.y + 1];
        positions[3] = generator.map[position.x, position.y - 1];

        int wallCount = positions.Where(x => x == 0).Count();
        if (wallCount >= 3)
        {
            generator.map[position.x, position.y] = 0;
            Fill(position + Vector2Int.left);
            Fill(position + Vector2Int.right);
            Fill(position + Vector2Int.up);
            Fill(position + Vector2Int.down);
        }
    }

    public List<Vector2Int> GetValidCuts(Room room)
    {
        List<Vector2Int> returnedPositions = new List<Vector2Int>();
        
        for (int x = room.start.x; x < room.end.x; x++)
        {
            Vector2Int roof = new Vector2Int(x, room.end.y);
            Vector2Int floor = new Vector2Int(x, room.start.y - 1);

            if (IsValidCutPosition(roof)) returnedPositions.Add(roof);
            if (IsValidCutPosition(floor)) returnedPositions.Add(floor);
        }

        for (int y = room.start.y; y < room.end.y; y++)
        {
            Vector2Int left = new Vector2Int(room.start.x - 1, y);
            Vector2Int right = new Vector2Int(room.end.x, y);

            if (IsValidCutPosition(left)) returnedPositions.Add(left);
            if (IsValidCutPosition(right)) returnedPositions.Add(right);
        }

        return returnedPositions;
    }

    public bool IsValidCutPosition(Vector2Int position)
    {
        if (generator.map[position.x, position.y] != 0) return false;

        int[] positions = new int[4];
        positions[0] = generator.map[position.x + 1, position.y    ];
        positions[1] = generator.map[position.x - 1, position.y    ];
        positions[2] = generator.map[position.x    , position.y + 1];
        positions[3] = generator.map[position.x    , position.y - 1];

        int numOpen = positions.Where(x => x == 1).Count();
        int numWalls = positions.Where(x => x == 0).Count();

        if (numOpen != 2 || numWalls != 2) return false;

        //Confirm that we're catty-corner
        if (positions[0] + positions[2] != 1) return false;
        if (positions[0] + positions[3] != 1) return false;

        return true;
    }

    //Helper function for a SUPER weird data structure
    public static int GetParent(int index, ref List<int> parents)
    {
        //Outside flag is always outside
        if (index == -1) return index;

        int workingIndex = index;
        while (workingIndex != -1 && parents[workingIndex] != workingIndex)
        {
            workingIndex = parents[workingIndex];
        }

        //Optimization - skip us up the ladder, so we're O(1) next time without an update.
        parents[index] = workingIndex;
        return workingIndex;
    }

    public void SubdivideRect(Rect inRect, bool horizontal, ref List<Room> rooms, ref List<Room> hallways)
    {
        //If too small in 1 dimension, make it a hallway
        if (inRect.height == 1 || inRect.width == 1)
        {
            Room r = new Room();
            r.size = new Vector2Int((int)inRect.size.x, (int)inRect.size.y);
            r.SetPosition(new Vector2Int((int)inRect.x, (int)inRect.y));
            hallways.Add(r);
            return;
        }

        //Else if fairly small in both dimensions, make it a room
        if (inRect.height <= 7 && inRect.width <= 7 || (inRect.height == 2 || inRect.width == 2))
        {
            Room r = new Room();
            r.size = new Vector2Int((int)inRect.size.x, (int)inRect.size.y);
            r.SetPosition(new Vector2Int((int)inRect.x, (int)inRect.y));
            rooms.Add(r);
            return;
        }

        //Try to catch really abnormally sized rooms, and fix them here.
        if ((horizontal && inRect.width <= 3) ||(!horizontal && inRect.height <=3))
        {
            horizontal = !horizontal;
        }
        

        Rect left, right;

        //Otherwise, keep subdividing!
        if (horizontal)
        {
            (left, right) = BreakRectOnX(inRect);
        }
        else
        {
            (left, right) = BreakRectOnY(inRect);
        }

        SubdivideRect(left, !horizontal, ref rooms, ref hallways);
        SubdivideRect(right, !horizontal, ref rooms, ref hallways);
    }

    public (Rect, Rect) BreakRectOnX(Rect inRect, int x = -1)
    {
        if (x < 0)
        {
            int lowerBound = 2;
            int upperBound = Mathf.RoundToInt(inRect.width - 3);

            if (Random.value < chanceForHallway || (lowerBound >= upperBound - 1))
            { 
                if (Random.value < .5f)
                {
                    x = 1;
                }
                else
                {
                    x = Mathf.RoundToInt(inRect.width - 2);
                }
            }
            else
            {
                x = RogueRNG.Linear(lowerBound, upperBound);
            }
        }

        Rect left;
        Rect temp;
        Rect middle;
        Rect right;

        (left, temp) = inRect.SplitOnX(x);
        (middle, right) = temp.SplitOnX(1);

        return (left, right);
    }

    public (Rect, Rect) BreakRectOnY(Rect inRect, int y = -1)
    {
        if (y < 0)
        {
            int lowerBound = 2;
            int upperBound = Mathf.RoundToInt(inRect.height - 3);
            if (Random.value < chanceForHallway || (lowerBound >= upperBound - 1))
            {
                if (Random.value < .5f)
                {
                    y = 1;
                }
                else
                {
                    y = Mathf.RoundToInt(inRect.height - 2);
                }
            }
            else
            {
                y = RogueRNG.Linear(lowerBound, upperBound);
            }
        }

        Rect left;
        Rect temp;
        Rect middle;
        Rect right;

        (left, temp) = inRect.SplitOnY(y);
        (middle, right) = temp.SplitOnY(1);

        return (left, right);
    }
}
