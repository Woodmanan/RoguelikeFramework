using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Machine", menuName = "Dungeon Generator/Machines/Empty", order = 1)]
public class Machine : ScriptableObject
{
    //Runtime Priority
    public int priority;

    //Sizing Details
    public bool global;
    public Vector2Int size;
    public Vector2Int start;
    public Vector2Int end;
    public bool canShareSpace;
    public bool canExpand;

    public DungeonGenerator generator;
    
    
    public virtual void Connect(DungeonGenerator d)
    {
        generator = d;
        if (global)
        {
            size = d.bounds;
        }
        else
        {
            if (size.x > d.bounds.x || size.y > d.bounds.y)
            {
                Debug.LogError("Machine can no longer fit into map!");
                size = d.bounds;
            }
        }
    }

    public virtual void SetPosition(Vector2Int start)
    {
        this.start = start;
        end = start + size;
    }

    public virtual void SetPosition(Vector2Int start, Vector2Int bounds)
    {
        if (global)
        {
            this.start = Vector2Int.zero;
            size = generator.bounds;
            end = generator.bounds;
        }
        else
        {
            this.start = start;
            if (canExpand)
            {
                size = bounds;
            }
            end = start + size;
        }
    }

    public virtual IEnumerator Activate() 
    {
        Debug.LogWarning("Default Machine call was made. Did you mean to do this, or did you forget to Override Activate()?", this);
        for (int i = start.x; i < end.x; i++)
        {
            for (int j = start.y; j < end.y; j++)
            {
                generator.map[i,j] = 1;
            }
        }
        yield break;
    }

    public virtual void PostActivation(Map m)
    {

    }

    public bool Overlaps(Machine other)
    {
        if (canShareSpace || other.canShareSpace)
        {
            return false;
        }

        if (global || other.global)
        {
            Machine globalMachine = global ? this : other;
            Machine nonGlobalMachine = global ? other : this;
            if (globalMachine.canShareSpace)
            {
                return false;
            }
            else
            {
                if (!nonGlobalMachine.canShareSpace)
                {
                    Debug.LogError("Machine has Global Overlap and no sharing enabled.", globalMachine);
                }
                return true;
            }
        }

        //Rectangle Magic Checks
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
}
