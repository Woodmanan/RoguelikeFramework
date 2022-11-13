using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Room", menuName = "Dungeon Generator/New Room", order = 2)]
public class Room : ScriptableObject
{
    public Vector2Int size;
    public bool acceptsStairs = true;
    
    public string layout;

    //Hidden Variables, used at runtime
    [HideInInspector] public Vector2Int start;
    [HideInInspector] public Vector2Int end;
    [HideInInspector] public bool connected = false;
    [HideInInspector] public Vector2Int center;
    [HideInInspector] public Vector2Int outermostPoint;
    
    public void Write(DungeonGenerator generator)
    {
        layout = layout.Replace("\n", "");
        for (int i = 0; i < size.x; i++)
        {
            for (int j = 0; j < size.y; j++)
            {
                generator.map[start.x + i, start.y + j] = GetValueAt(i, j);
            }
        }
    }

    public virtual void Setup()
    {

    }

    public virtual int GetValueAt(int x, int y)
    {
        char value = layout[(y * size.x) + x];
        int num;
        if (int.TryParse(value.ToString(), out num))
        {
            return num;
        }
        else
        {
            //TODO: Deal with non-numerical characters later
            Debug.LogError($"Tried to parse {value} as an int. Code not written to do that yet.", this);
            return 0;
        }
    }

    //Called after the map has been finished! Use this for any room-specific
    //fanciness you want.
    public virtual IEnumerator PostActivation(Map m)
    {
        yield break;
    }

    private int GetValueAtWorld(Vector2Int spot)
    {
        spot = spot - start;
        return GetValueAt(spot.x, spot.y);
    }

    public void SetPosition(Vector2Int start)
    {
        this.start = start;
        this.end = start + size;
        this.center = start + (size/2);
        this.outermostPoint = center;
    }

    public void CalculateOutermostPoint(Vector2Int mapBounds)
    {
        Vector2Int mapCenter = mapBounds / 2;
        Vector2Int clampedCenter = new Vector2Int(
            Mathf.Clamp(mapCenter.x, start.x, end.x - 1),
            Mathf.Clamp(mapCenter.y, start.y, end.y - 1));

        //Mathimagic - reflects clamped center over center point.
        outermostPoint = 2 * center - clampedCenter;
    }

    public bool Contains(Vector2Int spot)
    {
        if (spot.x >= start.x && spot.x < end.x && spot.y >= start.y && spot.y < end.y)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool Overlaps(Room other)
    {
        //Rectangle Magic
        if (start.y > other.end.y || end.y < other.start.y)
        {
            return false;
        }

        if (end.x < other.start.x || start.x > other.end.x)
        {
            return false;
        }

        return true;
    }

    public Vector2Int GetOpenSpace(int type, int[,] map)
    {
        for (int attempt = 0; attempt < 100; attempt++)
        {
            Vector2Int spot = new Vector2Int(Random.Range(start.x, end.x), Random.Range(start.y, end.y));
            if (map[spot.x, spot.y] == type)
            {
                return spot;
            }
        }

        //TODO: Iterative search for a space

        //Return not found
        return new Vector2Int(-1, -1);
    }

    //Overlaps, but with 1 extra layer of space. Helps prevent some weirdness
    //when rooms get packed together really tight.
    public bool OverlapsExtra(Room other)
    {
        if (start.y > (other.end.y + 1) || (end.y + 1) < other.start.y)
        {
            return false;
        }

        if ((end.x + 1) < other.start.x || start.x > (other.end.x + 1))
        {
            return false;
        }

        return true;
    }
}
