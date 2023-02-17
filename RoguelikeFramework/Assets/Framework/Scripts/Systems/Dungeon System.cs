using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Dungeon systems - linked to individual floors / dungeon overall
 * 
 * Made to control generalized events that can happen in the dungeon,
 * such as "Randomly teleport all monsters every 50 turns" or "spawn
 * a dangerous, unique enemy if the player stays on this floor too long"
 * 
 * The game controller is responsible for sending the signals to these systems
 * 
 * For more complicated detection effects, consider applying a status effect that 
 * signals the system with what to do.
 */

[System.Serializable]
public class DungeonSystem
{
    protected World world;
    protected Branch branch;
    protected Map map;

    public void Setup(World world, Branch branch = null, Map map = null)
    {
        this.world = world;
        this.branch = branch;
        this.map = map;
        OnSetup(world, branch, map);
    }

    public virtual void OnSetup(World world, Branch branch = null, Map map = null)
    {

    }

    public virtual void OnGlobalTurnStart(int turn)
    {

    }

    public virtual void OnGlobalTurnEnd(int turn)
    {

    }

    public virtual void OnEnterLevel(Map m)
    {

    }

    public virtual void OnExitLevel(Map m)
    {

    }

    public DungeonSystem Instantiate()
    {
        return (DungeonSystem) this.MemberwiseClone();
    }
}
