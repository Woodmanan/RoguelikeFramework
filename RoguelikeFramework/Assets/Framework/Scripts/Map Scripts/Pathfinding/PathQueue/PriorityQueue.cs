using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/* An inplace priority queue based on an array-backed binary heap
 * 
 * Made to be used with the roguelike framework pathfinder
 * 
 * Custom built to solve the allocation problems being faced by
 * the priority queue implementation that's been used up until now
 */


public class PriorityQueue<T>
{
    (T, float)[] values;
    int capacity;
    int held;
    int maxHeld;

    public int Count
    {
        get { return held; }
    }

    public PriorityQueue(int capacity = 0)
    {
        this.capacity = capacity;
        values = new (T, float)[capacity];
        held = 0;
        maxHeld = 0;
    }

    public int GetCapacity()
    {
        return capacity;
    }

    public int GetValueLength()
    {
        return values.Length;
    }

    public void Clear()
    {
        //All in place - why actually delete anything? New writes are always overwritten anyhow.
        //Should give massive speedups on actual pathfinding queries
        held = 0;
        maxHeld = 0;
    }

    public void Expand()
    {
        int newCapacity = (capacity < 4) ? 4 : capacity * 2;
        Debug.Log($"Queue expanding to capacity {newCapacity}");
        Array.Resize(ref values, newCapacity);
        capacity = newCapacity;
    }

    public void ExpandTo(int desiredCapacity)
    {
        if (capacity == 0) capacity = 4;
        while (capacity < desiredCapacity)
        {
            capacity *= 2;
        }
        Array.Resize(ref values, capacity);
    }

    public void Enqueue(T element, float priority)
    {
        if (held == capacity)
        {
            Expand();
        }
        values[held] = (element, priority);
        FixHeapInsert(held);
        held++;
        maxHeld = Mathf.Max(held, maxHeld);
    }

    public void FixHeapInsert(int index)
    {
        while (true)
        {
            if (index == 0) break;
            int parent = (index - 1) / 2;
            if (values[parent].Item2 > values[index].Item2)
            {
                (T, float) hold = values[parent];
                values[parent] = values[index];
                values[index] = hold;
                index = parent;
            }
            else
            {
                break;
            }
        }
    }

    public bool IsEmpty()
    {
        return held == 0;
    }

    public int GetMax()
    {
        return maxHeld;
    }

    public T Dequeue()
    {
        if (held == 0)
        {
            throw new InvalidOperationException("Can't Dequeue an empty list");
        }
        (T, float) result = values[0];

        values[0] = values[held - 1];
        held--;

        //Fix the heap from the top down, starting at the main node.
        if (held > 0)
        {
            FixHeapTopDown(0);
        }

        return result.Item1;
    }

    public void FixHeapTopDown(int index)
    {
        int child1 = (index * 2) + 1;
        int child2 = (index * 2) + 2;

        bool swapOne = (child1 < held && values[index].Item2 > values[child1].Item2);
        bool swapTwo = (child2 < held && values[index].Item2 > values[child2].Item2);

        //Need to some sort of swapping
        if (swapOne || swapTwo)
        {
            bool oneSmaller = (child2 >= held || values[child1].Item2 < values[child2].Item2);
            if (oneSmaller)
            {
                (T, float) hold = values[child1];
                values[child1] = values[index];
                values[index] = hold;
                FixHeapTopDown(child1);
            }
            else
            {
                (T, float) hold = values[child2];
                values[child2] = values[index];
                values[index] = hold;
                FixHeapTopDown(child2);
            }
        }
    }
}
