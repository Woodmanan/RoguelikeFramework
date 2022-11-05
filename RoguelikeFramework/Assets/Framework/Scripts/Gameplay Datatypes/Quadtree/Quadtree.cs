using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/* Homemade quadtree class because why not, I need one anyhow
 * 
 * Use cases:
 *  - Item spawning. Items have 'area' in that they they have two dimensions
 *    of spawn cases - rarity and available depth. This way, branches can just
 *    bunch up all of their items into one big ball and get the correct ones super fast.
 *    
 *  - Monster collection. Inverse idea, monsters are the points and we want to be able to 
 *    query how many of them are near X point or in Y box.
 */

public class Quadtree<T>
{
    public Quadtree<T> SE;
    public Quadtree<T> SW;
    public Quadtree<T> NE;
    public Quadtree<T> NW;

    public static readonly Vector2 epsilon = new Vector2(0.001f, 0.001f);

    public Rect rect;
    public (T, Rect)[] contained = new (T, Rect)[2];
    public int held = 0;

    public Quadtree(Rect rect)
    {
        this.rect = rect;
    }

    public void Insert(T item, Rect newRect)
    {
        if (newRect.size.x <= 0 || newRect.size.y <= 0)
        {
            Debug.Log("Can't insert 0 area rect into the quad tree.");
            return;
        }
        if (!FullyContains(newRect))
        {
            Debug.LogError($"Tried to insert a rect that is outside the bounds of this tree. {newRect} into {rect}");
            return;
        }

        if (SE == null)
        {
            BuildChildren();
        }

        if (SE.FullyContains(newRect))
        {
            SE.Insert(item, newRect);
            return;
        }

        if (SW.FullyContains(newRect))
        {
            SW.Insert(item, newRect);
            return;
        }

        if (NE.FullyContains(newRect))
        {
            NE.Insert(item, newRect);
            return;
        }

        if (NW.FullyContains(newRect))
        {
            NW.Insert(item, newRect);
            return;
        }
        
        //None of my children can fully fit it - it belongs to me!
        if (contained.Length == held)
        {
            Array.Resize(ref contained, Mathf.Max(4, contained.Length * 2));
        }

        contained[held] = (item, newRect);
        held++;
    }

    public bool Overlaps(Rect newRect)
    {
        return rect.Overlaps(newRect);
    }

    //Extremely clever idea - clamp center of circle to nearest spot in rectangle. Compute distance. Done.
    public bool Overlaps(Vector2 circleCenter, float circleRadiusSquared)
    {
        return rect.Overlaps(circleCenter, circleRadiusSquared);
    }

    public void BuildChildren()
    {
        Rect working = rect;
        working.size = working.size / 2;
        SE = new Quadtree<T>(working);

        working.x += working.size.x;
        SW = new Quadtree<T>(working);

        working.y += working.size.y;
        NW = new Quadtree<T>(working);

        working.x -= working.size.x;
        NE = new Quadtree<T>(working);
    }

    public bool FullyContains(Rect item)
    {
        return (rect.Contains(item.min) && rect.Contains(item.max - epsilon));
    }

    public List<T> GetItemsAt(Vector2 point)
    {
        Rect searchArea = new Rect(point, Vector2.one / 100);
        return GetItemsIn(searchArea);
    }

    public List<T> GetItemsIn(Rect searchArea)
    {
        List<T> itemsOut = new List<T>();
        GetItemsIn(ref itemsOut, searchArea);
        return itemsOut;
    }

    public void GetItemsIn(ref List<T> current, Rect searchArea)
    {
        for (int i = 0; i < held; i++)
        {
            (T item, Rect rect) = contained[i];
            if (rect.Overlaps(searchArea))
            {
                current.Add(item);
            }
        }

        if (SE == null)
        {
            return;
        }

        if (SE.Overlaps(searchArea))
        {
            SE.GetItemsIn(ref current, searchArea);
        }

        if (SW.Overlaps(searchArea))
        {
            SW.GetItemsIn(ref current, searchArea);
        }

        if (NE.Overlaps(searchArea))
        {
            NE.GetItemsIn(ref current, searchArea);
        }

        if (NW.Overlaps(searchArea))
        {
            NW.GetItemsIn(ref current, searchArea);
        }
    }

    public List<T> GetItemsIn(Vector2 center, float radius)
    {
        List<T> itemsOut = new List<T>();
        GetItemsIn(ref itemsOut, center, radius * radius);
        return itemsOut;
    }

    public void GetItemsIn(ref List<T> current, Vector2 center, float radiusSquared)
    {
        for (int i = 0; i < held; i++)
        {
            (T item, Rect rect) = contained[i];
            if (rect.Overlaps(center, radiusSquared))
            {
                current.Add(item);
            }
        }

        if (SE == null)
        {
            return;
        }

        if (SE.Overlaps(center, radiusSquared))
        {
            SE.GetItemsIn(ref current, center, radiusSquared);
        }

        if (SW.Overlaps(center, radiusSquared))
        {
            SW.GetItemsIn(ref current, center, radiusSquared);
        }

        if (NE.Overlaps(center, radiusSquared))
        {
            NE.GetItemsIn(ref current, center, radiusSquared);
        }

        if (NW.Overlaps(center, radiusSquared))
        {
            NW.GetItemsIn(ref current, center, radiusSquared);
        }
    }


    public int GetDepth()
    {
        if (SW == null)
        {
            return 1;
        }

        return Mathf.Max(SW.GetDepth(), SE.GetDepth(), NE.GetDepth(), NW.GetDepth()) + 1;
    }
}

public static class RectExtensions
{
    public static bool Overlaps(this Rect rect, Vector2 circleCenter, float circleRadiusSquared)
    {
        Vector2 nearest = new Vector2(
                     Mathf.Clamp(circleCenter.x, rect.xMin, rect.xMax),  //Clamped X
                     Mathf.Clamp(circleCenter.y, rect.yMin, rect.yMax)); //Clamped Y

        nearest = nearest - circleCenter;

        return (nearest.sqrMagnitude <= circleRadiusSquared);
    }
}